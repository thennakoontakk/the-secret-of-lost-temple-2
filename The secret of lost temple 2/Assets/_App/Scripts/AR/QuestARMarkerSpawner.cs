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
    /// Dedicated Quest Mode AR Spawner & Anchor.
    /// Completely isolated from Sandbox mode.
    /// Instantiates the 3D Simple Machine model from Assets and anchors it
    /// directly onto the physical Storybook Image Marker plane as a direct child.
    /// </summary>
    public class QuestARMarkerSpawner : MonoBehaviour
    {
        [Header("AR Foundation")]
        [SerializeField] private ARTrackedImageManager trackedImageManager;
        [SerializeField] private XRReferenceImageLibrary referenceLibrary;

        [Header("Quest 3D Machine Prefabs")]
        [SerializeField] private GameObject questLeverPrefab;

        [Header("UI & HUD")]
        [SerializeField] private GameObject scanningMarkerOverlay;
        [SerializeField] private Text scanStatusText;
        [SerializeField] private GameObject leverARUIRoot;

        [Header("Audio & Effects")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip markerFoundClip;
        [SerializeField] private ParticleSystem markerSpawnParticles;

        [Header("Physical Tracking Geometry")]
        [SerializeField] private float physicalScale = 0.045f; // ~10.8cm span, fits A4 printed illustration
        [SerializeField] private float surfaceElevationOffset = 0.013f; // Sits flush on paper plane

        // Active instances
        private GameObject spawnedLeverInstance;
        private Transform leverAnchor;
        private bool wasTracking;
        private bool simulated;
        private bool physicalMarkerSeen;
        private Coroutine scanTimeout;
        private ARTrackedImage activeTrackedImage;
        private bool isQuestMode = false;
        private bool isMarkerDetected = false;

        private void Awake()
        {
            if (trackedImageManager == null)
            {
                trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
            }

            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            isQuestMode = (appMode != "Sandbox" && appMode != "PhysicsLab");

            if (!isQuestMode)
            {
                // In Sandbox mode: this component is completely inactive
                enabled = false;
                return;
            }

            FindSceneReferences();
            PrepareQuestModeScene();
            if (trackedImageManager != null)
            {
                if (trackedImageManager.referenceLibrary == null && referenceLibrary != null)
                    trackedImageManager.referenceLibrary = referenceLibrary;
                trackedImageManager.enabled = true;
                Debug.Log("[QuestARMarkerSpawner] Image tracking enabled for printed lever target.");
            }
        }

        private void FindSceneReferences()
        {
            var canvas = GameObject.Find("AR Main Canvas");
            if (canvas != null)
            {
                for (int i = 0; i < canvas.transform.childCount; i++)
                {
                    var ch = canvas.transform.GetChild(i);
                    if (ch.name == "ScanningMarkerOverlay")
                    {
                        scanningMarkerOverlay = ch.gameObject;
                        scanStatusText = ch.Find("GuidanceCard/StatusText")?.GetComponent<Text>();
                    }
                    if (ch.name == "LeverAR_UIRoot")
                    {
                        leverARUIRoot = ch.gameObject;
                    }
                }
            }

            if (questLeverPrefab == null)
            {
                questLeverPrefab = Resources.Load<GameObject>("Quest_Lever_MarkerModel");
            }
        }

        private void PrepareQuestModeScene()
        {
            // Hide the virtual temple environment
            var temple = GameObject.Find("TempleEnvironmentRoot");
            if (temple != null) temple.SetActive(false);

            // Hide the Sandbox lever model
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "SimpleMachine_Lever") root.SetActive(false);
            }

            // Show scanning overlay HUD
            if (scanningMarkerOverlay != null) scanningMarkerOverlay.SetActive(true);
            if (leverARUIRoot != null) leverARUIRoot.SetActive(false);

            if (scanStatusText != null)
            {
                scanStatusText.text = "<b>POINT CAMERA AT THE PRINTED LEVER PAGE</b>\n<size=18>Keep the whole SCAN ME illustration visible. Move slowly sideways to start tracking.</size>";
            }

            // Top-down desk lighting for AR
            var dirLight = GameObject.Find("Directional Light");
            if (dirLight != null)
            {
                var light = dirLight.GetComponent<Light>();
                if (light != null)
                {
                    light.intensity = 1.1f;
                    light.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
                }
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
            if (scanTimeout != null) { StopCoroutine(scanTimeout); scanTimeout = null; }
            StudyPolicy.Tracking = false;
            StudyPolicy.SimulatedTracking = false;
            if (trackedImageManager != null)
            {
                trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (isQuestMode && !isMarkerDetected && GUI.Button(new Rect(Screen.width - 170, 8, 160, 44), "Simulate Marker (PC)"))
                SimulateMarkerScan();
        }
#endif

        public void SimulateMarkerScan()
        {
            if (!isQuestMode || isMarkerDetected || questLeverPrefab == null || Camera.main == null) return;
            var camera = Camera.main.transform;
            leverAnchor = new GameObject("SimulatedLeverAnchor").transform;
            leverAnchor.position = camera.position + camera.forward * .85f - Vector3.up * .12f;
            leverAnchor.rotation = Quaternion.Euler(0, camera.eulerAngles.y, 0);
            leverAnchor.localScale = Vector3.one * .20f;
            spawnedLeverInstance = Instantiate(questLeverPrefab, leverAnchor, false);
            spawnedLeverInstance.transform.localPosition = Vector3.zero;
            spawnedLeverInstance.transform.localRotation = Quaternion.identity;
            spawnedLeverInstance.transform.localScale = Vector3.one;
            var viewer = spawnedLeverInstance.GetComponent<ARMechanismViewer>() ?? spawnedLeverInstance.AddComponent<ARMechanismViewer>();
            viewer.Rebase();
            viewer.SetInteractionMode(StudyPolicy.Resolve(InteractionMode.ViewOnly));
            ConnectToScreenController(spawnedLeverInstance);
            simulated = true;
            Debug.Log("[QuestARMarkerSpawner] SIMULATED marker: developer bypass, no physical scan.");
            SecretOfLostTemple.Research.Research.Record("marker_simulated", "lever;developer_bypass");
            OnMarkerRecognized("Lever");
            SetTracking(true);
        }

        private void OnDestroy()
        {
            if (leverAnchor != null) Destroy(leverAnchor.gameObject);
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

            foreach (var kvp in eventArgs.removed)
            {
                if (activeTrackedImage != null && kvp.Key == activeTrackedImage.trackableId) { activeTrackedImage = null; SetTracking(false); }
            }
        }

        private void HandleTrackedImage(ARTrackedImage trackedImage)
        {

            if (trackedImage == null || !trackedImage.referenceImage.name.StartsWith("lever_target", StringComparison.OrdinalIgnoreCase)) return;
            if (activeTrackedImage != null && activeTrackedImage != trackedImage && activeTrackedImage.trackingState == TrackingState.Tracking) return;
            activeTrackedImage = trackedImage;
            bool tracking = trackedImage.trackingState == TrackingState.Tracking;
            if (tracking) {
                if (!physicalMarkerSeen || simulated)
                    SecretOfLostTemple.Research.Research.Record("marker_physical_acquired", trackedImage.referenceImage.name);
                physicalMarkerSeen = true;
                simulated = false;
                SpawnAndAnchorLever(trackedImage, true);
                OnMarkerRecognized("Lever");
            }
            SetTracking(tracking);
        }

        /// <summary>
        /// Instantiates the 3D Lever directly from the Prefab into the ARTrackedImage transform.
        /// </summary>
        private void SpawnAndAnchorLever(ARTrackedImage trackedImage, bool shouldBeActive)
        {

            if (questLeverPrefab == null) return;
            if (leverAnchor == null) {
                leverAnchor = new GameObject("LeverMarkerAnchor").transform;
                leverAnchor.SetParent(trackedImage.transform, false);
                leverAnchor.localPosition = new Vector3(0, surfaceElevationOffset, 0);
                leverAnchor.localScale = Vector3.one * physicalScale;
                spawnedLeverInstance = Instantiate(questLeverPrefab, leverAnchor);
                spawnedLeverInstance.name = "Quest_Lever_3D_Model";
                spawnedLeverInstance.transform.localPosition = Vector3.zero;
                spawnedLeverInstance.transform.localRotation = Quaternion.identity;
                spawnedLeverInstance.transform.localScale = Vector3.one;
                var viewer = spawnedLeverInstance.GetComponent<ARMechanismViewer>();
                if (viewer == null) viewer = spawnedLeverInstance.AddComponent<ARMechanismViewer>();
                viewer.Rebase();
                viewer.SetInteractionMode(StudyPolicy.Resolve(InteractionMode.ViewOnly));
                ConnectToScreenController(spawnedLeverInstance);
            } else if (leverAnchor.parent != trackedImage.transform) {
                leverAnchor.SetParent(trackedImage.transform, false);
                leverAnchor.localPosition = new Vector3(0, surfaceElevationOffset, 0);
                leverAnchor.localRotation = Quaternion.identity;
                leverAnchor.localScale = Vector3.one * physicalScale;
            }
            spawnedLeverInstance.SetActive(shouldBeActive);
        }

        private void Update()
        {

            if (!isQuestMode) return;
            SetTracking(simulated || (activeTrackedImage != null && activeTrackedImage.trackingState == TrackingState.Tracking));
        }


        private void SetTracking(bool tracking)
        {
            StudyPolicy.SimulatedTracking = simulated && tracking;
            StudyPolicy.Tracking = tracking && spawnedLeverInstance != null;
            if (spawnedLeverInstance != null) spawnedLeverInstance.SetActive(tracking);
            if (scanningMarkerOverlay != null) scanningMarkerOverlay.SetActive(!tracking);
            if (leverARUIRoot != null) leverARUIRoot.SetActive(tracking);
            if (wasTracking != tracking) SecretOfLostTemple.Research.Research.Record(tracking ? "tracking_acquired" : "tracking_lost", simulated ? "simulated_lever" : "lever_target");
            wasTracking = tracking;
        }

        private void OnMarkerRecognized(string machineName)
        {
            if (isMarkerDetected) return;
            isMarkerDetected = true;

            Debug.Log($"[QuestARMarkerSpawner] '{machineName}' Recognized! Unlocking Quest HUD.");

            if (scanningMarkerOverlay != null) scanningMarkerOverlay.SetActive(false);
            if (leverARUIRoot != null) leverARUIRoot.SetActive(true);

            if (audioSource != null && markerFoundClip != null)
            {
                audioSource.PlayOneShot(markerFoundClip);
            }

            if (markerSpawnParticles != null)
            {
                markerSpawnParticles.Play();
            }

            var screenController = FindFirstObjectByType<LeverARScreenController>();
            if (screenController != null)
            {
                screenController.SetInteractionMode(InteractionMode.Interactive);
            }
        }

        private void ConnectToScreenController(GameObject leverInstance)
        {
            var screenCtrl = FindFirstObjectByType<LeverARScreenController>();
            if (screenCtrl == null || leverInstance == null) return;

            screenCtrl.BindTrackedLever(leverInstance);
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
