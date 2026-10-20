using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Dedicated Quest Mode AR Spawner & StoryBook Page 9 Scanner.
    /// Provides a wide landscape Book-Page AR Viewfinder (replacing the old square QR reticle),
    /// supports both ARCore hardware image tracking and live camera visual marker scanning,
    /// and automatically opens the Interactive Lever Challenge once scanned.
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
        [SerializeField] private float physicalScale = 0.12f;
        [SerializeField] private float surfaceElevationOffset = 0.005f;

        // Active instances
        private GameObject spawnedLeverInstance;
        private Transform leverAnchor;
        private bool wasTracking;
        private bool simulated;
        private bool physicalMarkerSeen;
        private ARTrackedImage activeTrackedImage;
        private bool isQuestMode = false;
        private bool isMarkerDetected = false;

        // Custom Book-Page Scanner UI elements
        private LiveWebCamBackground liveWebCam;
        private RectTransform scanBeamRect;
        private Image scanProgressFill;
        private Text scanProgressLabel;
        private Text cameraStateBadgeText;
        private float scanProgress = 0f;
        private float nextFrameSampleTime = 0f;
        private bool frameHasTargetFeatures = false;
        private Vector3 prevAvgColor = Vector3.zero;

        private void Awake()
        {
            // Enforce compact StoryBook page marker scale (~18cm-22cm total visual width -> 0.12f) and 5mm surface elevation offset
            physicalScale = 0.12f;
            surfaceElevationOffset = 0.005f;

            if (trackedImageManager == null)
            {
                trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
            }
            if (liveWebCam == null)
            {
                liveWebCam = FindFirstObjectByType<LiveWebCamBackground>();
            }

            string activeScene = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            isQuestMode = (activeScene == "MainARScene" && appMode != "Sandbox" && appMode != "PhysicsLab");

            if (!isQuestMode)
            {
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
                Debug.Log("[QuestARMarkerSpawner] Image tracking enabled for printed StoryBook Page 9 target.");
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
            // Hide the virtual temple environment so live camera feed is visible
            var temple = GameObject.Find("TempleEnvironmentRoot");
            if (temple != null) temple.SetActive(false);

            // Hide the Sandbox lever model
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "SimpleMachine_Lever") root.SetActive(false);
            }

            // Show scanning overlay HUD and hide Lever UI until scanned
            if (scanningMarkerOverlay != null)
            {
                scanningMarkerOverlay.SetActive(true);
                BuildBookPageScannerUI(scanningMarkerOverlay.transform);
            }

            if (leverARUIRoot != null) leverARUIRoot.SetActive(false);

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

        /// <summary>
        /// Replaces the old square QR-code reticle with a wide Landscape StoryBook Page 9 AR Scanner HUD.
        /// </summary>
        private void BuildBookPageScannerUI(Transform overlayRoot)
        {
            // 1. Remove dark tint on overlay so camera feed is bright and clear
            var overlayImg = overlayRoot.GetComponent<Image>();
            if (overlayImg != null)
            {
                overlayImg.color = Color.clear;
                overlayImg.raycastTarget = false;
            }

            // 2. Hide legacy square QR-style ReticleFrame and GuidanceCard
            var oldReticle = overlayRoot.Find("ReticleFrame");
            if (oldReticle != null) oldReticle.gameObject.SetActive(false);

            var oldGuidance = overlayRoot.Find("GuidanceCard");
            if (oldGuidance != null) oldGuidance.gameObject.SetActive(false);

            var existingHud = overlayRoot.Find("BookPageScannerHUD");
            if (existingHud != null)
            {
                if (Application.isPlaying) Destroy(existingHud.gameObject);
                else DestroyImmediate(existingHud.gameObject);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var hudRoot = new GameObject("BookPageScannerHUD", typeof(RectTransform));
            hudRoot.transform.SetParent(overlayRoot, false);
            var hudRt = (RectTransform)hudRoot.transform;
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = Vector2.zero;
            hudRt.offsetMax = Vector2.zero;

            // Load Temple Aesthetic Sprites (Top Banner, Bottom Console, Target Card)
            Sprite sTopBanner = Resources.Load<Sprite>("scanner_top_banner");
            Sprite sTargetCard = Resources.Load<Sprite>("scanner_target_card");
            Sprite sBottomConsole = Resources.Load<Sprite>("scanner_bottom_console");

            // --- A. TOP STONE & JUNGLE VINE BANNER (Header Bar) ---
            var topBar = CreatePanel("TopScannerBar", hudRoot.transform, new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(1500f, 250f), Color.clear);
            var topBarImg = topBar.GetComponent<Image>();
            if (sTopBanner != null)
            {
                topBarImg.sprite = sTopBanner;
                topBarImg.color = Color.white;
                topBarImg.preserveAspect = true;
            }

            // Invisible clickable button layered over the left "BACK TO STORY" section of banner
            var backBtnGo = new GameObject("Btn_BackToStory", typeof(RectTransform), typeof(Image), typeof(Button));
            backBtnGo.transform.SetParent(topBar.transform, false);
            var rtBack = (RectTransform)backBtnGo.transform;
            rtBack.anchorMin = new Vector2(0.01f, 0.15f);
            rtBack.anchorMax = new Vector2(0.24f, 0.85f);
            rtBack.offsetMin = Vector2.zero;
            rtBack.offsetMax = Vector2.zero;
            var backImg = backBtnGo.GetComponent<Image>();
            backImg.color = Color.clear; // Invisible raycast hit zone over carved wooden back button
            var backBtn = backBtnGo.GetComponent<Button>();
            backBtn.onClick.AddListener(() =>
            {
                if (SecretOfLostTemple.Story.StoryBookViewerController.StoryActivityActive)
                    SecretOfLostTemple.Story.StoryBookViewerController.ReturnToStory(false);
                else
                    SceneManager.LoadScene("HomeMapScene");
            });

            // Camera status text reference (empty by default so baked "CAMERA ACTIVE" artwork shines)
            cameraStateBadgeText = CreateText("CamStatusText", topBar.transform, font, new Vector2(1f, 0.5f), new Vector2(-155f, 0f), new Vector2(240f, 40f),
                "", 18, new Color(0.4f, 1f, 0.65f), TextAnchor.MiddleCenter);

            // --- B. CENTRAL CODE-BASED AR MARKER PLACEHOLDER (VIEWFINDER & CORNERS) ---
            // Built purely with code - no image sprite required, completely eliminating cropping or stretching
            var bookFrame = new GameObject("BookPageViewfinder", typeof(RectTransform), typeof(Image));
            bookFrame.transform.SetParent(hudRoot.transform, false);
            var bfRt = (RectTransform)bookFrame.transform;
            bfRt.anchorMin = bfRt.anchorMax = bfRt.pivot = new Vector2(0.5f, 0.5f);
            bfRt.anchoredPosition = new Vector2(-150f, -20f);
            bfRt.sizeDelta = new Vector2(760f, 490f);

            var bfImg = bookFrame.GetComponent<Image>();
            // Subtle translucent aperture fill so camera feed is bright and clear
            bfImg.color = new Color(0.02f, 0.05f, 0.03f, 0.10f);
            bfImg.raycastTarget = false;

            // 1. Four perimeter border guidelines (subtle golden framing)
            Color guideBorderCol = new Color(1f, 0.85f, 0.35f, 0.35f);
            CreateLine(bookFrame.transform, "Border_T", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 2f), guideBorderCol);
            CreateLine(bookFrame.transform, "Border_B", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 2f), guideBorderCol);
            CreateLine(bookFrame.transform, "Border_L", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(2f, 0f), guideBorderCol);
            CreateLine(bookFrame.transform, "Border_R", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(2f, 0f), guideBorderCol);

            // 2. Four Ornate Golden AR Corner Brackets (Code-Generated)
            Color cornerCol = new Color(1f, 0.82f, 0.22f, 0.95f);
            Color cornerInnerCol = new Color(1f, 0.95f, 0.60f, 0.80f);
            float cLen = 65f;
            float cThick = 5f;

            // Outer brackets
            CreateCornerBracket(bookFrame.transform, "Corner_TL", new Vector2(0f, 1f), Vector2.zero, cLen, cThick, 1f, -1f, cornerCol);
            CreateCornerBracket(bookFrame.transform, "Corner_TR", new Vector2(1f, 1f), Vector2.zero, cLen, cThick, -1f, -1f, cornerCol);
            CreateCornerBracket(bookFrame.transform, "Corner_BL", new Vector2(0f, 0f), Vector2.zero, cLen, cThick, 1f, 1f, cornerCol);
            CreateCornerBracket(bookFrame.transform, "Corner_BR", new Vector2(1f, 0f), Vector2.zero, cLen, cThick, -1f, 1f, cornerCol);

            // Inner stepped accents
            CreateCornerBracket(bookFrame.transform, "CornerInner_TL", new Vector2(0f, 1f), new Vector2(10f, -10f), 26f, 3f, 1f, -1f, cornerInnerCol);
            CreateCornerBracket(bookFrame.transform, "CornerInner_TR", new Vector2(1f, 1f), new Vector2(-10f, -10f), 26f, 3f, -1f, -1f, cornerInnerCol);
            CreateCornerBracket(bookFrame.transform, "CornerInner_BL", new Vector2(0f, 0f), new Vector2(10f, 10f), 26f, 3f, 1f, 1f, cornerInnerCol);
            CreateCornerBracket(bookFrame.transform, "CornerInner_BR", new Vector2(1f, 0f), new Vector2(-10f, 10f), 26f, 3f, -1f, 1f, cornerInnerCol);

            // 3. Center Target Reticle Crosshairs
            var centerReticle = new GameObject("CenterReticle", typeof(RectTransform));
            centerReticle.transform.SetParent(bookFrame.transform, false);
            var crRt = (RectTransform)centerReticle.transform;
            crRt.anchorMin = crRt.anchorMax = crRt.pivot = new Vector2(0.5f, 0.5f);
            crRt.sizeDelta = new Vector2(50f, 50f);

            Color reticleCol = new Color(1f, 0.85f, 0.35f, 0.60f);
            CreateLine(centerReticle.transform, "Reticle_H", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44f, 2f), reticleCol);
            CreateLine(centerReticle.transform, "Reticle_V", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2f, 44f), reticleCol);
            CreatePanel("Reticle_Dot", centerReticle.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8f, 8f), new Color(1f, 0.95f, 0.6f, 0.85f));

            // 4. Top Badge on Viewfinder: "ALIGN STORYBOOK PAGE 9 HERE"
            var topBadge = CreatePanel("PlaceholderTopBadge", bookFrame.transform, new Vector2(0.5f, 1f), new Vector2(0f, 22f), new Vector2(360f, 34f), new Color(0.12f, 0.08f, 0.04f, 0.90f));
            CreateLine(topBadge.transform, "BadgeBorder_B", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 2f), new Color(1f, 0.82f, 0.25f, 0.8f));
            CreateText("BadgeText", topBadge.transform, font, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(350f, 30f),
                "<b><color=#FFE066>⛶ ALIGN STORYBOOK PAGE 9 HERE</color></b>", 15, Color.white, TextAnchor.MiddleCenter);

            // 5. Code-generated Animated Scan Laser Beam (pure code, zero image asset)
            var beamGo = new GameObject("GoldenScanBeam", typeof(RectTransform));
            beamGo.transform.SetParent(bookFrame.transform, false);
            var beamRt = (RectTransform)beamGo.transform;
            beamRt.anchorMin = beamRt.anchorMax = beamRt.pivot = new Vector2(0.5f, 0.5f);
            beamRt.sizeDelta = new Vector2(740f, 20f);

            CreatePanel("LaserGlow", beamGo.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(740f, 16f), new Color(1f, 0.80f, 0.20f, 0.22f));
            CreatePanel("LaserCore", beamGo.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(730f, 3f), new Color(1f, 0.98f, 0.70f, 0.95f));
            CreatePanel("PipL", beamGo.transform, new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(6f, 6f), new Color(1f, 0.9f, 0.4f, 0.95f));
            CreatePanel("PipR", beamGo.transform, new Vector2(1f, 0.5f), new Vector2(-4f, 0f), new Vector2(6f, 6f), new Color(1f, 0.9f, 0.4f, 0.95f));

            scanBeamRect = beamRt;

            // 6. Dynamic Scanning Progress Bar below Viewfinder
            var progressTrack = CreatePanel("ScanProgressTrack", bookFrame.transform, new Vector2(0.5f, 0f), new Vector2(0f, -14f), new Vector2(560f, 8f), new Color(0.12f, 0.12f, 0.12f, 0.75f));
            var fillGo = CreatePanel("ProgressFill", progressTrack.transform, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(0f, 8f), new Color(0.35f, 0.95f, 0.55f, 0.95f));
            var fillRt = (RectTransform)fillGo.transform;
            fillRt.pivot = new Vector2(0f, 0.5f);
            scanProgressFill = fillGo.GetComponent<Image>();

            scanProgressLabel = CreateText("ProgressLabel", bookFrame.transform, font, new Vector2(0.5f, 0f), new Vector2(0f, -32f), new Vector2(680f, 28f),
                "<b><color=#FFF0AA>Point camera at Page 9 illustration or tap 'Scan Your Storybook'</color></b>", 16, Color.white, TextAnchor.MiddleCenter);
            scanStatusText = scanProgressLabel;

            // --- C. RIGHT TARGET MARKER PAGE CARD ---
            var refCard = CreatePanel("TargetPageReferenceCard", hudRoot.transform, new Vector2(1f, 0.5f), new Vector2(-45f, -20f), new Vector2(420f, 450f), Color.clear);
            var rcImg = refCard.GetComponent<Image>();
            if (sTargetCard != null)
            {
                rcImg.sprite = sTargetCard;
                rcImg.color = Color.white;
                rcImg.preserveAspect = true;
            }

            // --- D. BOTTOM SCAN CONSOLE BANNER (Wood/Stone with Camera Icon) ---
            var bottomBar = CreatePanel("BottomScannerConsole", hudRoot.transform, new Vector2(0.5f, 0f), new Vector2(-150f, 16f), new Vector2(720f, 195f), Color.clear);
            var botImg = bottomBar.GetComponent<Image>();
            if (sBottomConsole != null)
            {
                botImg.sprite = sBottomConsole;
                botImg.color = Color.white;
                botImg.preserveAspect = true;
            }

            // Bottom bar click zone to trigger scan / unlock mechanism
            var bottomBtnGo = new GameObject("Btn_BottomScanAction", typeof(RectTransform), typeof(Image), typeof(Button));
            bottomBtnGo.transform.SetParent(bottomBar.transform, false);
            var rtBotBtn = (RectTransform)bottomBtnGo.transform;
            rtBotBtn.anchorMin = Vector2.zero;
            rtBotBtn.anchorMax = Vector2.one;
            rtBotBtn.sizeDelta = Vector2.zero;
            var botBtnImg = bottomBtnGo.GetComponent<Image>();
            botBtnImg.color = Color.clear;
            var botBtn = bottomBtnGo.GetComponent<Button>();
            botBtn.onClick.AddListener(() =>
            {
                SimulateMarkerScan();
            });
        }

        private static GameObject CreateLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 anchoredPos, Vector2 size, Color bgColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = false;
            return go;
        }

        private static void CreateCornerBracket(Transform parent, string name, Vector2 anchor, Vector2 pos, float length, float thickness, float dirX, float dirY, Color color)
        {
            var horiz = new GameObject(name + "_H", typeof(RectTransform), typeof(Image));
            horiz.transform.SetParent(parent, false);
            var hRt = (RectTransform)horiz.transform;
            hRt.anchorMin = hRt.anchorMax = anchor;
            hRt.pivot = new Vector2(dirX > 0 ? 0f : 1f, dirY > 0 ? 0f : 1f);
            hRt.anchoredPosition = pos;
            hRt.sizeDelta = new Vector2(length, thickness);
            horiz.GetComponent<Image>().color = color;
            horiz.GetComponent<Image>().raycastTarget = false;

            var vert = new GameObject(name + "_V", typeof(RectTransform), typeof(Image));
            vert.transform.SetParent(parent, false);
            var vRt = (RectTransform)vert.transform;
            vRt.anchorMin = vRt.anchorMax = anchor;
            vRt.pivot = new Vector2(dirX > 0 ? 0f : 1f, dirY > 0 ? 0f : 1f);
            vRt.anchoredPosition = pos;
            vRt.sizeDelta = new Vector2(thickness, length);
            vert.GetComponent<Image>().color = color;
            vert.GetComponent<Image>().raycastTarget = false;
        }

        private static Text CreateText(string name, Transform parent, Font font, Vector2 anchor, Vector2 pos, Vector2 size, string content, int fontSize, Color color, TextAnchor align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var txt = go.GetComponent<Text>();
            txt.font = font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.supportRichText = true;
            txt.raycastTarget = false;
            return txt;
        }

        private static Button CreateStyledButton(string name, Transform parent, Font font, Vector2 anchor, Vector2 pos, Vector2 size, Color bgColor, string label, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = bgColor;
            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(onClick);

            CreateText("Label", go.transform, font, new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(12f, 8f), "<b>" + label + "</b>", fontSize, new Color(1f, 0.96f, 0.82f), TextAnchor.MiddleCenter);
            return btn;
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
            StudyPolicy.Tracking = false;
            StudyPolicy.SimulatedTracking = false;
            if (trackedImageManager != null)
            {
                trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
            }
        }

        public void SimulateMarkerScan()
        {
            if (!isQuestMode || isMarkerDetected || questLeverPrefab == null || Camera.main == null) return;
            var camera = Camera.main.transform;
            leverAnchor = new GameObject("SimulatedLeverAnchor").transform;
            leverAnchor.position = camera.position + camera.forward * 0.60f - camera.up * 0.08f;
            leverAnchor.rotation = Quaternion.Euler(0f, camera.eulerAngles.y, 0f);
            leverAnchor.localScale = Vector3.one * 0.12f;
            spawnedLeverInstance = Instantiate(questLeverPrefab, leverAnchor, false);
            spawnedLeverInstance.transform.localPosition = Vector3.zero;
            spawnedLeverInstance.transform.localRotation = Quaternion.identity;
            spawnedLeverInstance.transform.localScale = Vector3.one;
            var viewer = spawnedLeverInstance.GetComponent<ARMechanismViewer>() ?? spawnedLeverInstance.AddComponent<ARMechanismViewer>();
            viewer.Rebase();
            viewer.SetInteractionMode(StudyPolicy.Resolve(InteractionMode.Interactive));
            ConnectToScreenController(spawnedLeverInstance);
            simulated = true;
            SecretOfLostTemple.Research.Research.Record("marker_scanned", "lever;camera_scan_lock");
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
                if (activeTrackedImage != null && kvp.Key == activeTrackedImage.trackableId)
                {
                    activeTrackedImage = null;
                    if (!physicalMarkerSeen && !isMarkerDetected)
                    {
                        SetTracking(false);
                    }
                }
            }
        }

        private void HandleTrackedImage(ARTrackedImage trackedImage)
        {
            if (trackedImage == null) return;
            string refName = trackedImage.referenceImage.name ?? "";
            if (!refName.StartsWith("lever", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(refName)) return;
            if (activeTrackedImage != null && activeTrackedImage != trackedImage && activeTrackedImage.trackingState == TrackingState.Tracking) return;

            activeTrackedImage = trackedImage;
            bool tracking = trackedImage.trackingState == TrackingState.Tracking;
            if (tracking)
            {
                if (!physicalMarkerSeen || simulated)
                    SecretOfLostTemple.Research.Research.Record("marker_physical_acquired", refName);
                physicalMarkerSeen = true;
                simulated = false;
                SpawnAndAnchorLever(trackedImage, true);
                OnMarkerRecognized("Lever");
                SetTracking(true);
            }
            else if (physicalMarkerSeen || isMarkerDetected)
            {
                // Keep 3D lever persistent at its last known anchor pose during momentary TrackingState.Limited dropouts
                SetTracking(true);
            }
            else
            {
                SetTracking(false);
            }
        }

        private void SpawnAndAnchorLever(ARTrackedImage trackedImage, bool shouldBeActive)
        {
            if (questLeverPrefab == null) return;
            float scaleToUse = 0.12f;
            float elevation = 0.025f; // Elevate slightly above the marker center so the 3D lever sits higher in the viewport

            if (leverAnchor == null)
            {
                leverAnchor = new GameObject("LeverMarkerAnchor").transform;
                leverAnchor.SetParent(trackedImage.transform, false);
                leverAnchor.localPosition = new Vector3(0f, elevation, 0.03f);
                leverAnchor.localRotation = Quaternion.identity;
                leverAnchor.localScale = Vector3.one * scaleToUse;
                spawnedLeverInstance = Instantiate(questLeverPrefab, leverAnchor, false);
                spawnedLeverInstance.name = "Quest_Lever_3D_Model";
                spawnedLeverInstance.transform.localPosition = Vector3.zero;
                spawnedLeverInstance.transform.localRotation = Quaternion.identity;
                spawnedLeverInstance.transform.localScale = Vector3.one;
                var viewer = spawnedLeverInstance.GetComponent<ARMechanismViewer>();
                if (viewer == null) viewer = spawnedLeverInstance.AddComponent<ARMechanismViewer>();
                viewer.Rebase();
                viewer.SetInteractionMode(StudyPolicy.Resolve(InteractionMode.Interactive));
                ConnectToScreenController(spawnedLeverInstance);
            }
            else if (leverAnchor.parent != trackedImage.transform)
            {
                leverAnchor.SetParent(trackedImage.transform, false);
                leverAnchor.localPosition = new Vector3(0f, elevation, 0.03f);
                leverAnchor.localRotation = Quaternion.identity;
                leverAnchor.localScale = Vector3.one * scaleToUse;
            }
            else
            {
                leverAnchor.localPosition = new Vector3(0f, elevation, 0.03f);
                leverAnchor.localRotation = Quaternion.identity;
                leverAnchor.localScale = Vector3.one * scaleToUse;
            }
            spawnedLeverInstance.SetActive(true);
        }

        private void Update()
        {
            if (!isQuestMode) return;

            if (!isMarkerDetected)
            {
                UpdateScannerVisualsAndCameraDetection();
            }

            bool isCurrentlyTracking = simulated || physicalMarkerSeen || isMarkerDetected ||
                                       (activeTrackedImage != null && activeTrackedImage.trackingState == TrackingState.Tracking);
            SetTracking(isCurrentlyTracking);
        }

        private void UpdateScannerVisualsAndCameraDetection()
        {
            // 1. Animate golden scan laser line smoothly across the code-based marker placeholder
            if (scanBeamRect != null)
            {
                float yOffset = Mathf.Sin(Time.unscaledTime * 2.4f) * 200f;
                scanBeamRect.anchoredPosition = new Vector2(0f, yOffset);
            }

            if (liveWebCam == null) liveWebCam = FindFirstObjectByType<LiveWebCamBackground>();
            var webCamTex = liveWebCam != null ? liveWebCam.GetWebCamTexture() : null;
            bool camPlaying = (webCamTex != null && webCamTex.isPlaying && webCamTex.width > 16);
            bool arTrackingReady = (ARSession.state == ARSessionState.SessionTracking || ARSession.state == ARSessionState.SessionInitializing);

            if (cameraStateBadgeText != null)
            {
                if (!camPlaying && !arTrackingReady)
                    cameraStateBadgeText.text = "<color=#FF7777>⚠️ CAMERA WAITING</color>";
                else
                    cameraStateBadgeText.text = ""; // Keep clear so baked "CAMERA ACTIVE" artwork shines
            }

            // 2. Sample live camera frames to detect when pointed at the marker illustration
            if (camPlaying && Time.unscaledTime >= nextFrameSampleTime)
            {
                nextFrameSampleTime = Time.unscaledTime + 0.12f;
                frameHasTargetFeatures = EvaluateCameraFrameForMarker(webCamTex);
            }

            if (camPlaying && frameHasTargetFeatures)
            {
                // Advance scan progress while marker illustration features are held in camera view
                scanProgress = Mathf.Clamp01(scanProgress + Time.unscaledDeltaTime * 0.48f);
            }
            else
            {
                scanProgress = Mathf.Max(0f, scanProgress - Time.unscaledDeltaTime * 0.25f);
            }

            if (scanProgressFill != null)
            {
                float fullWidth = 560f;
                scanProgressFill.rectTransform.sizeDelta = new Vector2(fullWidth * scanProgress, 8f);
            }

            if (scanProgressLabel != null)
            {
                int pct = Mathf.RoundToInt(scanProgress * 100f);
                scanProgressLabel.text = pct > 0
                    ? $"<b><color=#55FF88>🔍 Detecting Page 9 Illustration... {pct}% (Hold Steady)</color></b>"
                    : "<b><color=#FFF0AA>Point camera at Page 9 illustration or tap 'Scan Your Storybook'</color></b>";
            }

            if (scanProgress >= 1f)
            {
                SimulateMarkerScan();
            }
        }

        /// <summary>
        /// Evaluates the center region of the live camera feed for Page 9 illustration visual features
        /// (sufficient illumination, color richness/contrast, and frame stability).
        /// </summary>
        private bool EvaluateCameraFrameForMarker(WebCamTexture tex)
        {
            try
            {
                int w = tex.width;
                int h = tex.height;
                if (w < 32 || h < 32) return false;

                int blockW = Mathf.Clamp(w / 3, 16, 160);
                int blockH = Mathf.Clamp(h / 3, 16, 120);
                int startX = (w - blockW) / 2;
                int startY = (h - blockH) / 2;

                Color[] pixels = tex.GetPixels(startX, startY, blockW, blockH);
                if (pixels == null || pixels.Length == 0) return false;

                float sumR = 0f, sumG = 0f, sumB = 0f;
                float minLum = 1f, maxLum = 0f;
                int step = Mathf.Max(1, pixels.Length / 64);
                int count = 0;

                for (int i = 0; i < pixels.Length; i += step)
                {
                    Color c = pixels[i];
                    sumR += c.r;
                    sumG += c.g;
                    sumB += c.b;
                    float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                    if (lum < minLum) minLum = lum;
                    if (lum > maxLum) maxLum = lum;
                    count++;
                }

                if (count == 0) return false;
                Vector3 avgColor = new Vector3(sumR / count, sumG / count, sumB / count);
                float avgLum = (avgColor.x + avgColor.y + avgColor.z) / 3f;
                float contrast = maxLum - minLum;
                float motionDelta = Vector3.Distance(avgColor, prevAvgColor);
                prevAvgColor = avgColor;

                // Requires a bright, high-contrast printed/screen image held steady in front of the camera
                return (avgLum > 0.15f && avgLum < 0.92f && contrast > 0.22f && motionDelta < 0.08f);
            }
            catch
            {
                return false;
            }
        }

        private void SetTracking(bool tracking)
        {
            bool effectiveVisible = tracking || physicalMarkerSeen || isMarkerDetected;
            StudyPolicy.SimulatedTracking = simulated && effectiveVisible;
            StudyPolicy.Tracking = effectiveVisible && spawnedLeverInstance != null;
            if (spawnedLeverInstance != null && effectiveVisible)
            {
                if (!spawnedLeverInstance.activeSelf)
                    spawnedLeverInstance.SetActive(true);
            }
            if (scanningMarkerOverlay != null) scanningMarkerOverlay.SetActive(!effectiveVisible);
            if (leverARUIRoot != null) leverARUIRoot.SetActive(effectiveVisible);
            if (wasTracking != effectiveVisible)
            {
                SecretOfLostTemple.Research.Research.Record(effectiveVisible ? "tracking_acquired" : "tracking_lost", simulated ? "camera_locked_lever" : "lever_target");
            }
            wasTracking = effectiveVisible;
        }

        private void OnMarkerRecognized(string machineName)
        {
            if (isMarkerDetected) return;
            isMarkerDetected = true;

            Debug.Log($"[QuestARMarkerSpawner] '{machineName}' Recognized! Unlocking Quest HUD & Opening Challenge.");

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
                screenController.OnQuestMarkerScanned();
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
