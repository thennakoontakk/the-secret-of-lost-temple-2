using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Anchors Simple Machine 3D models onto scanned AR target image markers from the Story Book.
    /// Handles:
    /// - Lever marker tracking (lever_target)
    /// - Pulley marker tracking (pulley_target)
    /// - Inclined plane marker tracking (inclined_plane_target)
    /// - Quest Mode Scan Overlay & Editor Simulation
    /// - Automatic Interactive Mode activation upon marker recognition
    /// </summary>
    public class ARTrackedImageMachineAnchor : MonoBehaviour
    {
        [Header("AR Foundation References")]
        [SerializeField] private ARTrackedImageManager trackedImageManager;
        [SerializeField] private XRReferenceImageLibrary referenceLibrary;

        [Header("3D Machine Roots")]
        [SerializeField] private GameObject leverMachineRoot;
        [SerializeField] private GameObject pulleyMachineRoot;
        [SerializeField] private GameObject inclineMachineRoot;

        [Header("UI Scan Guidance")]
        [SerializeField] private GameObject scanningMarkerOverlay;
        [SerializeField] private Text scanStatusText;
        [SerializeField] private GameObject leverARUIRoot;

        [Header("Controllers")]
        [SerializeField] private LeverARScreenController leverARScreenController;

        [Header("Audio & Effects")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip markerFoundClip;
        [SerializeField] private ParticleSystem markerSpawnParticles;

        private bool isQuestMode = false;
        private string activeMachine = "Lever";
        private bool isMarkerDetected = false;

        // Persistent AR World Anchor Target
        private Vector3 targetWorldPos;
        private Quaternion targetWorldRot;
        private bool hasValidAnchor = false;
        private ARTrackedImage currentTrackedImage;

        private void Awake()
        {
            if (trackedImageManager == null)
            {
                trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
            }

            if (leverARScreenController == null)
            {
                leverARScreenController = FindFirstObjectByType<LeverARScreenController>();
            }

            if (leverARUIRoot == null)
            {
                var ui = GameObject.Find("AR Main Canvas/LeverAR_UIRoot");
                if (ui != null) leverARUIRoot = ui;
            }

            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            isQuestMode = (appMode != "Sandbox" && appMode != "PhysicsLab");
            activeMachine = PlayerPrefs.GetString("Sandbox_Selected_Machine", "Lever");

            if (leverMachineRoot != null)
            {
                RecalculateBoundsOnAllMeshes(leverMachineRoot);
            }

            // Immediately configure the scene for the correct mode in Awake
            // so the very first rendered frame shows the right state
            SetupForMode();
        }

        private void OnEnable()
        {
            if (trackedImageManager != null)
            {
                trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
            }
        }

        private void OnDisable()
        {
            if (trackedImageManager != null)
            {
                trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
            }
        }

        private void Start()
        {
            // SetupForMode already called in Awake, but refresh in case anything changed
            SetupForMode();
        }

        private void Update()
        {
            if (!isQuestMode || !hasValidAnchor) return;

            // In Quest Mode: Keep model visible and smoothly interpolate to physical image marker
            GameObject activeObj = GetActiveMachineObject();
            if (activeObj != null)
            {
                if (!activeObj.activeSelf) activeObj.SetActive(true);

                if (currentTrackedImage != null && currentTrackedImage.trackingState == TrackingState.Tracking)
                {
                    // The tracked image plane: transform.up is the surface normal (perpendicular to image)
                    // Place model cleanly on the image surface (elevate fulcrum base by 5.7cm along normal so base touches paper)
                    targetWorldPos = currentTrackedImage.transform.position + currentTrackedImage.transform.up * 0.057f;
                    targetWorldRot = currentTrackedImage.transform.rotation;

                    // Direct rigid lock to physical paper plane while tracking (zero float / zero lag)
                    activeObj.transform.position = targetWorldPos;
                    activeObj.transform.rotation = targetWorldRot;
                }
                else
                {
                    // When tracking transitions to Limited, stay pinned in AR space at last known marker anchor
                    activeObj.transform.position = targetWorldPos;
                    activeObj.transform.rotation = targetWorldRot;
                }

                activeObj.transform.localScale = Vector3.one * 0.22f;
            }
        }

        private GameObject GetActiveMachineObject()
        {
            if (activeMachine == "Pulley") return pulleyMachineRoot;
            if (activeMachine == "InclinedPlane") return inclineMachineRoot;
            return leverMachineRoot;
        }

        public void SetupForMode()
        {
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            isQuestMode = (appMode != "Sandbox" && appMode != "PhysicsLab");
            activeMachine = PlayerPrefs.GetString("Sandbox_Selected_Machine", "Lever");

            var templeEnv = GameObject.Find("TempleEnvironmentRoot");

            if (isQuestMode)
            {
                // QUEST MODE: AR Image Tracking will be enabled by LiveWebCamBackground
                // AFTER camera permission is granted. Do NOT enable it here.

                // Critical: Hide the virtual 3D temple so real camera feed shows through
                if (templeEnv != null) templeEnv.SetActive(false);

                // Also hide the directional light's harsh virtual lighting
                var dirLight = GameObject.Find("Directional Light");
                if (dirLight != null)
                {
                    var light = dirLight.GetComponent<Light>();
                    if (light != null) light.intensity = 0.6f; // Dim for AR
                }

                if (!isMarkerDetected)
                {
                    // Show scanning guidance overlay
                    if (scanningMarkerOverlay != null)
                    {
                        scanningMarkerOverlay.SetActive(true);
                    }

                    // Hide interactive lever UI until marker is found
                    if (leverARUIRoot != null)
                    {
                        leverARUIRoot.SetActive(false);
                    }

                    if (scanStatusText != null)
                    {
                        scanStatusText.text = "<b>\ud83d\udd0d POINT CAMERA AT STORY BOOK MARKER</b>\n<size=18>Scan Page 9 to awaken the Ancient Lever!</size>";
                    }

                    // Hide all machines until scanning detects the marker
                    if (leverMachineRoot != null) leverMachineRoot.SetActive(false);
                    if (pulleyMachineRoot != null) pulleyMachineRoot.SetActive(false);
                    if (inclineMachineRoot != null) inclineMachineRoot.SetActive(false);
                }
            }
            else
            {
                // SANDBOX MODE: Disable AR Image tracking, show 3D virtual temple lab,
                // and display selected machine on the altar pedestal
                if (trackedImageManager != null) trackedImageManager.enabled = false;
                if (templeEnv != null) templeEnv.SetActive(true);

                if (scanningMarkerOverlay != null) scanningMarkerOverlay.SetActive(false);
                if (leverARUIRoot != null) leverARUIRoot.SetActive(true);
                ActivateSelectedMachine();
            }
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            if (!isQuestMode) return;

            foreach (var trackedImage in eventArgs.added)
            {
                HandleTrackedImage(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
            {
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    HandleTrackedImage(trackedImage);
                }
            }
        }

        private void HandleTrackedImage(ARTrackedImage trackedImage)
        {
            if (trackedImage == null) return;
            string imgName = trackedImage.referenceImage.name.ToLower();

            Debug.Log("[ARTrackedImage] Detected image: '" + imgName + "' state=" + trackedImage.trackingState);

            // Match marker to machine
            if (imgName.Contains("lever") || activeMachine == "Lever")
            {
                PositionMachineOnMarker(leverMachineRoot, trackedImage);
                OnMarkerRecognized("Lever");
            }
            else if (imgName.Contains("pulley") || activeMachine == "Pulley")
            {
                PositionMachineOnMarker(pulleyMachineRoot, trackedImage);
                OnMarkerRecognized("Pulley");
            }
            else if (imgName.Contains("incline") || activeMachine == "InclinedPlane")
            {
                PositionMachineOnMarker(inclineMachineRoot, trackedImage);
                OnMarkerRecognized("InclinedPlane");
            }
        }

        private void PositionMachineOnMarker(GameObject machineObj, ARTrackedImage trackedImage)
        {
            if (machineObj == null || trackedImage == null) return;

            currentTrackedImage = trackedImage;
            hasValidAnchor = true;

            // Ensure machine remains a world-root object (do NOT parent into ephemeral ARTrackables)
            if (machineObj.transform.parent != null)
            {
                machineObj.transform.SetParent(null, true);
            }

            // Elevate fulcrum base by 5cm along image normal so it rests cleanly on top of the paper surface
            targetWorldPos = trackedImage.transform.position + trackedImage.transform.up * 0.05f;
            targetWorldRot = trackedImage.transform.rotation;

            // Set natural physical scale matching the storybook page (approx 26-28cm beam span)
            machineObj.transform.localScale = Vector3.one * 0.22f;

            // Immediate snap on first recognition
            if (!isMarkerDetected)
            {
                machineObj.transform.position = targetWorldPos;
                machineObj.transform.rotation = targetWorldRot;
            }

            machineObj.SetActive(true);

            // Critical: Recalculate bounds on all meshes to prevent frustum culling
            RecalculateBoundsOnAllMeshes(machineObj);
        }

        private void RecalculateBoundsOnAllMeshes(GameObject root)
        {
            if (root == null) return;
            var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in meshFilters)
            {
                if (mf != null && mf.sharedMesh != null)
                {
                    mf.sharedMesh.RecalculateBounds();
                }
            }
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = true;
            }
        }

        private void OnMarkerRecognized(string machineName)
        {
            if (isMarkerDetected) return;
            isMarkerDetected = true;

            Debug.Log("[ARTrackedImage] Marker RECOGNIZED: " + machineName + " — activating AR interface!");

            if (scanningMarkerOverlay != null)
            {
                scanningMarkerOverlay.SetActive(false);
            }

            if (leverARUIRoot != null)
            {
                leverARUIRoot.SetActive(true);
            }

            if (audioSource != null && markerFoundClip != null)
            {
                audioSource.PlayOneShot(markerFoundClip);
            }

            if (markerSpawnParticles != null)
            {
                markerSpawnParticles.Play();
            }

            // In Quest Mode: Automatically enter Interactive Mode
            if (leverARScreenController != null)
            {
                leverARScreenController.SetInteractionMode(InteractionMode.Interactive);
            }
        }

        public void SimulateMarkerScan()
        {
            // For Editor testing or testing without camera marker
            Transform camTr = Camera.main != null ? Camera.main.transform : transform;
            Vector3 spawnPos = camTr.position + camTr.forward * 0.85f + Vector3.down * 0.12f;
            Quaternion spawnRot = Quaternion.LookRotation(camTr.forward, Vector3.up);

            GameObject targetMachine = GetActiveMachineObject();

            if (targetMachine != null)
            {
                if (targetMachine.transform.parent != null) targetMachine.transform.SetParent(null, true);
                targetMachine.SetActive(true);
                targetMachine.transform.position = spawnPos;
                targetMachine.transform.rotation = spawnRot;
                targetMachine.transform.localScale = Vector3.one * 0.20f;

                targetWorldPos = spawnPos;
                targetWorldRot = spawnRot;
                hasValidAnchor = true;
            }

            OnMarkerRecognized(activeMachine);
        }

        private void ActivateSelectedMachine()
        {
            if (leverMachineRoot != null)
            {
                if (leverMachineRoot.transform.parent != null) leverMachineRoot.transform.SetParent(null, true);
                leverMachineRoot.SetActive(activeMachine == "Lever");
                leverMachineRoot.transform.position = new Vector3(0f, 0.50f, 1.40f);
                leverMachineRoot.transform.rotation = Quaternion.identity;
                leverMachineRoot.transform.localScale = Vector3.one * 2.0f; // 2x scale in Sandbox Altar
            }
            if (pulleyMachineRoot != null)
            {
                if (pulleyMachineRoot.transform.parent != null) pulleyMachineRoot.transform.SetParent(null, true);
                pulleyMachineRoot.SetActive(activeMachine == "Pulley");
                pulleyMachineRoot.transform.position = new Vector3(0f, 0.75f, 1.40f);
                pulleyMachineRoot.transform.rotation = Quaternion.identity;
                pulleyMachineRoot.transform.localScale = Vector3.one;
            }
            if (inclineMachineRoot != null)
            {
                if (inclineMachineRoot.transform.parent != null) inclineMachineRoot.transform.SetParent(null, true);
                inclineMachineRoot.SetActive(activeMachine == "InclinedPlane");
                inclineMachineRoot.transform.position = new Vector3(0f, 0.55f, 1.40f);
                inclineMachineRoot.transform.rotation = Quaternion.identity;
                inclineMachineRoot.transform.localScale = Vector3.one;
            }
        }
    }
}
