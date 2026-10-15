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
    /// Dedicated Quest Mode AR Controller.
    /// Anchors the 3D Simple Machine model directly onto the physical Storybook Image Marker plane
    /// as a direct child of ARTrackedImage, ensuring 100% rock-solid physical placement on the paper.
    /// </summary>
    public class QuestARImageAnchorController : MonoBehaviour
    {
        [Header("AR Foundation References")]
        [SerializeField] private ARTrackedImageManager trackedImageManager;
        [SerializeField] private XRReferenceImageLibrary referenceLibrary;

        [Header("3D Simple Machine Models")]
        [SerializeField] private GameObject leverMachineRoot;
        [SerializeField] private GameObject pulleyMachineRoot;
        [SerializeField] private GameObject inclineMachineRoot;

        [Header("UI & Guidance")]
        [SerializeField] private GameObject scanningMarkerOverlay;
        [SerializeField] private Text scanStatusText;
        [SerializeField] private GameObject leverARUIRoot;
        [SerializeField] private GameObject templeEnvironmentRoot;

        [Header("Audio & Effects")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip markerFoundClip;
        [SerializeField] private ParticleSystem markerSpawnParticles;

        [Header("Physical Tracking Settings")]
        [SerializeField] private float physicalScale = 0.045f; // ~10.8cm span, fits exactly on the printed storybook drawing
        [SerializeField] private float surfaceElevationOffset = 0.012f; // Aligns fulcrum base with paper plane at 0.045 scale

        // Active State
        private bool isQuestMode = true;
        private string activeMachine = "Lever";
        private bool isMarkerDetected = false;
        private ARTrackedImage currentTrackedImage;
        private bool hasValidAnchor = false;

        private void Awake()
        {
            if (trackedImageManager == null)
            {
                trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
            }

            FindSceneReferences();

            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            isQuestMode = (appMode != "Sandbox" && appMode != "PhysicsLab");
            activeMachine = PlayerPrefs.GetString("Sandbox_Selected_Machine", "Lever");

            // Recalculate mesh bounds early
            if (leverMachineRoot != null) RecalculateAllMeshBounds(leverMachineRoot);
            if (pulleyMachineRoot != null) RecalculateAllMeshBounds(pulleyMachineRoot);
            if (inclineMachineRoot != null) RecalculateAllMeshBounds(inclineMachineRoot);

            ConfigureInitialSceneState();
        }

        private void FindSceneReferences()
        {
            if (leverMachineRoot == null)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name == "SimpleMachine_Lever") leverMachineRoot = root;
                    if (root.name == "TempleEnvironmentRoot") templeEnvironmentRoot = root;
                }
            }

            if (leverARUIRoot == null)
            {
                var ui = GameObject.Find("AR Main Canvas/LeverAR_UIRoot");
                if (ui != null) leverARUIRoot = ui;
            }

            if (scanningMarkerOverlay == null)
            {
                var overlay = GameObject.Find("AR Main Canvas/ScanningMarkerOverlay");
                if (overlay != null) scanningMarkerOverlay = overlay;
            }
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
            ConfigureInitialSceneState();
        }

        /// <summary>
        /// Sets up scene visibility according to whether we are in Quest Mode (AR Marker) or Sandbox (Virtual Lab).
        /// </summary>
        public void ConfigureInitialSceneState()
        {
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            isQuestMode = (appMode != "Sandbox" && appMode != "PhysicsLab");
            activeMachine = PlayerPrefs.GetString("Sandbox_Selected_Machine", "Lever");

            if (templeEnvironmentRoot == null)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name == "TempleEnvironmentRoot") templeEnvironmentRoot = root;
                }
            }

            if (isQuestMode)
            {
                // QUEST MODE: Hide virtual 3D temple to reveal real camera background
                if (templeEnvironmentRoot != null) templeEnvironmentRoot.SetActive(false);

                // Dim virtual light for AR passthrough
                var dirLight = GameObject.Find("Directional Light");
                if (dirLight != null)
                {
                    var light = dirLight.GetComponent<Light>();
                    if (light != null)
                    {
                        light.intensity = 1.2f;
                        light.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
                    }
                }

                if (!isMarkerDetected)
                {
                    // Show scanning overlay HUD
                    if (scanningMarkerOverlay != null) scanningMarkerOverlay.SetActive(true);

                    // Hide interactive UI until marker is identified
                    if (leverARUIRoot != null) leverARUIRoot.SetActive(false);

                    if (scanStatusText != null)
                    {
                        scanStatusText.text = "<b>🔍 POINT CAMERA AT STORY BOOK MARKER</b>\n<size=18>Scan Page 9 from 20cm - 30cm to awaken the Ancient Lever!</size>";
                    }

                    // Hide all 3D machines offscreen until the marker is scanned
                    if (leverMachineRoot != null)
                    {
                        leverMachineRoot.transform.position = new Vector3(0f, -100f, 0f);
                        leverMachineRoot.SetActive(false);
                    }
                    if (pulleyMachineRoot != null) pulleyMachineRoot.SetActive(false);
                    if (inclineMachineRoot != null) inclineMachineRoot.SetActive(false);
                }
            }
            else
            {
                // SANDBOX MODE: Show 3D virtual temple altar
                if (templeEnvironmentRoot != null) templeEnvironmentRoot.SetActive(true);
                if (scanningMarkerOverlay != null) scanningMarkerOverlay.SetActive(false);
                if (leverARUIRoot != null) leverARUIRoot.SetActive(true);

                var dirLight = GameObject.Find("Directional Light");
                if (dirLight != null)
                {
                    var light = dirLight.GetComponent<Light>();
                    if (light != null)
                    {
                        light.intensity = 1.0f;
                        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                    }
                }

                if (leverMachineRoot != null)
                {
                    if (leverMachineRoot.transform.parent != null) leverMachineRoot.transform.SetParent(null, true);
                    leverMachineRoot.SetActive(activeMachine == "Lever");
                    leverMachineRoot.transform.position = new Vector3(0f, 0.50f, 1.40f);
                    leverMachineRoot.transform.rotation = Quaternion.identity;
                    leverMachineRoot.transform.localScale = Vector3.one * 2.0f;
                    RecalculateAllMeshBounds(leverMachineRoot);
                }
            }
        }

        private void Update()
        {
            if (!isQuestMode || !hasValidAnchor || currentTrackedImage == null) return;

            GameObject machine = GetActiveMachine();
            if (machine == null) return;

            if (!machine.activeSelf) machine.SetActive(true);

            // Direct lock on the physical image plane (parented to trackedImage)
            if (machine.transform.parent != currentTrackedImage.transform)
            {
                machine.transform.SetParent(currentTrackedImage.transform, false);
            }

            // Local coordinates relative to physical image marker
            machine.transform.localPosition = new Vector3(0f, surfaceElevationOffset, 0f);
            machine.transform.localRotation = Quaternion.identity;
            machine.transform.localScale = Vector3.one * physicalScale;
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
                HandleTrackedImage(trackedImage);
            }
        }

        private void HandleTrackedImage(ARTrackedImage trackedImage)
        {
            if (trackedImage == null) return;
            string imgName = trackedImage.referenceImage.name.ToLower();

            Debug.Log($"[QuestAR] Detected image marker: '{imgName}' (State: {trackedImage.trackingState}) @ {trackedImage.transform.position}");

            if (imgName.Contains("lever") || activeMachine == "Lever")
            {
                AnchorModelDirectlyToTrackedImage(leverMachineRoot, trackedImage);
                OnMarkerRecognized("Lever");
            }
            else if (imgName.Contains("pulley") || activeMachine == "Pulley")
            {
                AnchorModelDirectlyToTrackedImage(pulleyMachineRoot, trackedImage);
                OnMarkerRecognized("Pulley");
            }
            else if (imgName.Contains("incline") || activeMachine == "InclinedPlane")
            {
                AnchorModelDirectlyToTrackedImage(inclineMachineRoot, trackedImage);
                OnMarkerRecognized("InclinedPlane");
            }
        }

        /// <summary>
        /// Anchors the 3D model directly on top of the physical image marker plane as a direct child.
        /// </summary>
        private void AnchorModelDirectlyToTrackedImage(GameObject machineObj, ARTrackedImage trackedImage)
        {
            if (machineObj == null || trackedImage == null) return;

            currentTrackedImage = trackedImage;
            hasValidAnchor = true;

            // Direct parent attachment to the AR Tracked Image
            if (machineObj.transform.parent != trackedImage.transform)
            {
                machineObj.transform.SetParent(trackedImage.transform, false);
            }

            // Local coordinates relative to physical image plane
            machineObj.transform.localPosition = new Vector3(0f, surfaceElevationOffset, 0f);
            machineObj.transform.localRotation = Quaternion.identity;
            machineObj.transform.localScale = Vector3.one * physicalScale;

            machineObj.SetActive(true);

            // Recompute mesh bounds to guarantee visibility from all viewing angles
            RecalculateAllMeshBounds(machineObj);
        }

        private void OnMarkerRecognized(string machineName)
        {
            if (isMarkerDetected) return;
            isMarkerDetected = true;

            Debug.Log($"[QuestAR] Marker '{machineName}' Recognized! Activating AR UI & Interactive Controls.");

            // Hide scan guidance HUD
            if (scanningMarkerOverlay != null)
            {
                scanningMarkerOverlay.SetActive(false);
            }

            // Show interactive control HUD
            if (leverARUIRoot != null)
            {
                leverARUIRoot.SetActive(true);
            }

            // Play celebratory recognition sound
            if (audioSource != null && markerFoundClip != null)
            {
                audioSource.PlayOneShot(markerFoundClip);
            }

            // Play spawn particles
            if (markerSpawnParticles != null)
            {
                markerSpawnParticles.Play();
            }

            // Switch to interactive controls
            var screenController = FindFirstObjectByType<LeverARScreenController>();
            if (screenController != null)
            {
                screenController.SetInteractionMode(InteractionMode.Interactive);
            }
        }

        private GameObject GetActiveMachine()
        {
            if (activeMachine == "Pulley") return pulleyMachineRoot;
            if (activeMachine == "InclinedPlane") return inclineMachineRoot;
            return leverMachineRoot;
        }

        private void RecalculateAllMeshBounds(GameObject root)
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
    }
}
