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
        [SerializeField] private float physicalScale = 0.24f;
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
            // Enforce realistic StoryBook page marker scale (0.22f - 0.26f) and 5mm surface elevation offset
            if (physicalScale < 0.22f || physicalScale > 0.26f)
            {
                physicalScale = 0.24f;
            }
            surfaceElevationOffset = 0.005f;

            if (trackedImageManager == null)
            {
                trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
            }
            if (liveWebCam == null)
            {
                liveWebCam = FindFirstObjectByType<LiveWebCamBackground>();
            }

            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            isQuestMode = (appMode != "Sandbox" && appMode != "PhysicsLab");

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
            if (existingHud != null) return;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var hudRoot = new GameObject("BookPageScannerHUD", typeof(RectTransform));
            hudRoot.transform.SetParent(overlayRoot, false);
            var hudRt = (RectTransform)hudRoot.transform;
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = Vector2.zero;
            hudRt.offsetMax = Vector2.zero;

            // --- A. TOP NAVIGATION & CAMERA STATUS BAR ---
            var topBar = CreatePanel("TopScannerBar", hudRoot.transform, new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(1840f, 76f), new Color(0.10f, 0.07f, 0.04f, 0.88f));

            // Back to StoryBook Button
            var backBtn = CreateStyledButton("Btn_BackToStory", topBar.transform, font, new Vector2(0f, 0.5f), new Vector2(150f, 0f), new Vector2(260f, 52f),
                new Color(0.35f, 0.22f, 0.10f, 0.95f), "⬅ BACK TO STORY", 18, () =>
                {
                    if (SecretOfLostTemple.Story.StoryBookViewerController.StoryActivityActive)
                        SecretOfLostTemple.Story.StoryBookViewerController.ReturnToStory(false);
                    else
                        SceneManager.LoadScene("HomeMapScene");
                });

            // Header Title
            var titleTxt = CreateText("HeaderTitle", topBar.transform, font, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(960f, 60f),
                "📖 <b>CHAPTER 1: THE SACRED LEVER — STORYBOOK PAGE 9 SCANNER</b>", 22, new Color(1f, 0.90f, 0.55f), TextAnchor.MiddleCenter);

            // Live Camera Badge
            var camBadge = CreatePanel("CameraBadge", topBar.transform, new Vector2(1f, 0.5f), new Vector2(-170f, 0f), new Vector2(300f, 46f), new Color(0.08f, 0.28f, 0.16f, 0.92f));
            cameraStateBadgeText = CreateText("CamStatusText", camBadge.transform, font, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280f, 40f),
                "📹 OPENING CAMERA...", 16, new Color(0.4f, 1f, 0.65f), TextAnchor.MiddleCenter);

            // --- B. WIDE LANDSCAPE STORYBOOK PAGE VIEWFINDER (4:3 Book Page Ratio, NOT a square QR box!) ---
            var bookFrame = new GameObject("BookPageViewfinder", typeof(RectTransform));
            bookFrame.transform.SetParent(hudRoot.transform, false);
            var bfRt = (RectTransform)bookFrame.transform;
            bfRt.anchorMin = bfRt.anchorMax = bfRt.pivot = new Vector2(0.5f, 0.5f);
            bfRt.anchoredPosition = new Vector2(-110f, 18f);
            bfRt.sizeDelta = new Vector2(820f, 540f); // Wide landscape storybook page frame

            Color goldCorner = new Color(1f, 0.82f, 0.28f, 0.92f);
            float armLen = 64f;
            float armThick = 6f;

            // Top-Left L-Corner
            CreateCornerBracket(bookFrame.transform, "Corner_TL", new Vector2(0f, 1f), new Vector2(0f, 0f), armLen, armThick, 1f, -1f, goldCorner);
            // Top-Right L-Corner
            CreateCornerBracket(bookFrame.transform, "Corner_TR", new Vector2(1f, 1f), new Vector2(0f, 0f), armLen, armThick, -1f, -1f, goldCorner);
            // Bottom-Left L-Corner
            CreateCornerBracket(bookFrame.transform, "Corner_BL", new Vector2(0f, 0f), new Vector2(0f, 0f), armLen, armThick, 1f, 1f, goldCorner);
            // Bottom-Right L-Corner
            CreateCornerBracket(bookFrame.transform, "Corner_BR", new Vector2(1f, 0f), new Vector2(0f, 0f), armLen, armThick, -1f, 1f, goldCorner);

            // Subtle top caption inside book viewfinder
            CreateText("ViewfinderCaption", bookFrame.transform, font, new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(760f, 34f),
                "✨ <b>ALIGN PRINTED STORYBOOK PAGE 9 ILLUSTRATION HERE</b> ✨", 17, new Color(1f, 0.95f, 0.75f, 0.9f), TextAnchor.MiddleCenter);

            // Animated Horizontal Golden Scan Beam
            var beamGo = CreatePanel("GoldenScanBeam", bookFrame.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(790f, 4f), new Color(1f, 0.85f, 0.25f, 0.85f));
            scanBeamRect = (RectTransform)beamGo.transform;

            // --- C. TARGET PAGE 9 ILLUSTRATION REFERENCE CARD (Right Side) ---
            var refCard = CreatePanel("TargetPageReferenceCard", hudRoot.transform, new Vector2(1f, 0.5f), new Vector2(-190f, 18f), new Vector2(320f, 360f), new Color(0.12f, 0.09f, 0.05f, 0.92f));

            CreateText("RefHeader", refCard.transform, font, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(290f, 36f),
                "🎯 <b>TARGET MARKER PAGE</b>", 17, new Color(1f, 0.86f, 0.40f), TextAnchor.MiddleCenter);

            var previewHolder = CreatePanel("Page9PreviewFrame", refCard.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(280f, 200f), new Color(0.22f, 0.18f, 0.12f, 1f));
            var previewImgGo = new GameObject("Page9Image", typeof(RectTransform), typeof(Image));
            previewImgGo.transform.SetParent(previewHolder.transform, false);
            var pimRt = (RectTransform)previewImgGo.transform;
            pimRt.anchorMin = Vector2.zero;
            pimRt.anchorMax = Vector2.one;
            pimRt.offsetMin = new Vector2(6f, 6f);
            pimRt.offsetMax = new Vector2(-6f, -6f);
            var pim = previewImgGo.GetComponent<Image>();
            pim.preserveAspect = true;

            Sprite targetSprite = SecretOfLostTemple.Story.StoryBookViewerController.ActiveTargetPageSprite;
            if (targetSprite == null && referenceLibrary != null && referenceLibrary.count > 0)
            {
                var refTex = referenceLibrary[0].texture;
                if (refTex != null)
                {
                    targetSprite = Sprite.Create(refTex, new Rect(0, 0, refTex.width, refTex.height), new Vector2(0.5f, 0.5f));
                }
            }
            if (targetSprite != null)
            {
                pim.sprite = targetSprite;
                pim.color = Color.white;
            }
            else
            {
                pim.color = new Color(0.25f, 0.38f, 0.22f, 1f);
            }

            CreateText("RefFooter", refCard.transform, font, new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(290f, 68f),
                "<b>StoryBook — Page 9</b>\n<size=15><color=#d8cfa8>Point camera at the 'SCAN ME' Lever illustration to unlock the challenge!</color></size>",
                16, Color.white, TextAnchor.MiddleCenter);

            // --- D. BOTTOM SCAN PROGRESS & ACTION CONSOLE ---
            var bottomBar = CreatePanel("BottomScannerConsole", hudRoot.transform, new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(1280f, 130f), new Color(0.10f, 0.07f, 0.04f, 0.93f));

            scanStatusText = CreateText("LiveScanInstruction", bottomBar.transform, font, new Vector2(0f, 1f), new Vector2(440f, -32f), new Vector2(820f, 44f),
                "📸 <b>Point Camera at StoryBook Page 9 ('SCAN ME' Lever Illustration)</b>", 20, new Color(1f, 0.94f, 0.72f), TextAnchor.MiddleLeft);

            // Progress Bar Track
            var progBg = CreatePanel("ScanProgressTrack", bottomBar.transform, new Vector2(0f, 0f), new Vector2(440f, 34f), new Vector2(820f, 32f), new Color(0.05f, 0.04f, 0.03f, 0.95f));
            var fillGo = new GameObject("ScanProgressFill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(progBg.transform, false);
            var fillRt = (RectTransform)fillGo.transform;
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.anchoredPosition = Vector2.zero;
            fillRt.sizeDelta = new Vector2(0f, 0f);
            scanProgressFill = fillGo.GetComponent<Image>();
            scanProgressFill.color = new Color(0.20f, 0.88f, 0.48f, 0.95f);

            scanProgressLabel = CreateText("ScanProgressPercent", progBg.transform, font, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 28f),
                "Waiting for Page 9 Marker in Camera View... 0%", 15, Color.white, TextAnchor.MiddleCenter);

            // Direct "SCAN NOW / LOCK MARKER" Button on the right of Bottom Console
            CreateStyledButton("Btn_LockMarkerNow", bottomBar.transform, font, new Vector2(1f, 0.5f), new Vector2(-200f, 0f), new Vector2(340f, 82f),
                new Color(0.14f, 0.58f, 0.28f, 0.98f), "📸 SCAN & LOCK MARKER\n<size=14>Open 3D Lever Challenge</size>", 19, () =>
                {
                    SimulateMarkerScan();
                });
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
            leverAnchor.position = camera.position + camera.forward * 0.60f - camera.up * 0.15f;
            leverAnchor.rotation = Quaternion.Euler(0f, camera.eulerAngles.y, 0f);
            leverAnchor.localScale = Vector3.one * 0.22f;
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
                    SetTracking(false);
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
            }
            SetTracking(tracking);
        }

        private void SpawnAndAnchorLever(ARTrackedImage trackedImage, bool shouldBeActive)
        {
            if (questLeverPrefab == null) return;
            float scaleToUse = Mathf.Clamp(physicalScale, 0.22f, 0.26f);
            float elevation = 0.005f; // 5mm above marker base plane to prevent mesh z-fighting

            if (leverAnchor == null)
            {
                leverAnchor = new GameObject("LeverMarkerAnchor").transform;
                leverAnchor.SetParent(trackedImage.transform, false);
                leverAnchor.localPosition = new Vector3(0f, elevation, 0f);
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
                leverAnchor.localPosition = new Vector3(0f, elevation, 0f);
                leverAnchor.localRotation = Quaternion.identity;
                leverAnchor.localScale = Vector3.one * scaleToUse;
            }
            else
            {
                leverAnchor.localPosition = new Vector3(0f, elevation, 0f);
                leverAnchor.localRotation = Quaternion.identity;
                leverAnchor.localScale = Vector3.one * scaleToUse;
            }
            spawnedLeverInstance.SetActive(shouldBeActive);
        }

        private void Update()
        {
            if (!isQuestMode) return;

            if (!isMarkerDetected)
            {
                UpdateScannerVisualsAndCameraDetection();
            }

            SetTracking(simulated || (activeTrackedImage != null && activeTrackedImage.trackingState == TrackingState.Tracking));
        }

        private void UpdateScannerVisualsAndCameraDetection()
        {
            // 1. Animate golden scan laser line smoothly across the book viewfinder
            if (scanBeamRect != null)
            {
                float yOffset = Mathf.Sin(Time.unscaledTime * 2.4f) * 235f;
                scanBeamRect.anchoredPosition = new Vector2(0f, yOffset);
            }

            if (liveWebCam == null) liveWebCam = FindFirstObjectByType<LiveWebCamBackground>();
            var webCamTex = liveWebCam != null ? liveWebCam.GetWebCamTexture() : null;
            bool camPlaying = (webCamTex != null && webCamTex.isPlaying && webCamTex.width > 16);
            bool arTrackingReady = (ARSession.state == ARSessionState.SessionTracking || ARSession.state == ARSessionState.SessionInitializing);

            if (cameraStateBadgeText != null)
            {
                if (camPlaying || arTrackingReady)
                    cameraStateBadgeText.text = "🟢 <b>LIVE CAMERA ACTIVE</b>";
                else
                    cameraStateBadgeText.text = "📷 <b>CAMERA READY</b>";
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
                if (scanStatusText != null)
                {
                    scanStatusText.text = "🔍 <b>Detecting StoryBook Page 9 ('SCAN ME') Marker... Hold Steady!</b>";
                }
            }
            else
            {
                scanProgress = Mathf.Max(0f, scanProgress - Time.unscaledDeltaTime * 0.25f);
                if (scanStatusText != null)
                {
                    scanStatusText.text = "📸 <b>Point Camera at StoryBook Page 9 ('SCAN ME' Lever Illustration)</b>";
                }
            }

            if (scanProgressFill != null)
            {
                float fullWidth = 820f;
                scanProgressFill.rectTransform.sizeDelta = new Vector2(fullWidth * scanProgress, 0f);
            }

            if (scanProgressLabel != null)
            {
                int pct = Mathf.RoundToInt(scanProgress * 100f);
                scanProgressLabel.text = pct > 0
                    ? $"<b>Scanning Page 9 Illustration... {pct}%</b>"
                    : "Align Page 9 in Viewfinder or Tap 'Scan & Lock Marker'";
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
            StudyPolicy.SimulatedTracking = simulated && tracking;
            StudyPolicy.Tracking = tracking && spawnedLeverInstance != null;
            if (spawnedLeverInstance != null) spawnedLeverInstance.SetActive(tracking);
            if (scanningMarkerOverlay != null) scanningMarkerOverlay.SetActive(!tracking);
            if (leverARUIRoot != null) leverARUIRoot.SetActive(tracking);
            if (wasTracking != tracking)
            {
                SecretOfLostTemple.Research.Research.Record(tracking ? "tracking_acquired" : "tracking_lost", simulated ? "camera_locked_lever" : "lever_target");
            }
            wasTracking = tracking;
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
