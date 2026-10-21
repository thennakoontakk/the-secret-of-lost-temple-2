using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input = StudyInput;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace SecretOfLostTemple.AR
{
    public enum ViewOnlySubMode
    {
        ForcesAndMotion,
        ExplodedParts
    }

    /// <summary>
    /// Master UI and Interactive Controller for the Lever AR Sandbox.
    /// Perfectly matches the primary school-friendly temple UI with:
    /// - Top "LEVER AR" carved banner & "View Only / Interactive" toggle
    /// - Default: VIEW ONLY MODE (Bottom stone console hidden)
    /// - View-Only Sub-Bar with 2 Buttons:
    ///     1) 360° Forces & Motion (Continuous orbit + Dynamic Force Direction Arrows)
    ///     2) Exploded Parts & Anatomy (Separated parts + Interactive detail knowledge popup)
    /// - Interactive Mode: Shows Bottom Stone Console (Force -/+, Move Fulcrum, Reset, Try Challenge)
    /// - 3D in-world floating labels with animated glowing force direction arrows
    /// - Real-time physics equilibrium, mechanical advantage, and research cognitive telemetry logging
    /// </summary>
    public class LeverARScreenController : MonoBehaviour
    {
        [Header("Top Navigation & Header")]
        [SerializeField] private Button backOrSettingsButton;
        [SerializeField] private Button modeToggleButton;
        [SerializeField] private Text modeToggleText;
        [SerializeField] private Image modeToggleBg;
        [SerializeField] private Sprite viewOnlyModeSprite;
        [SerializeField] private Sprite interactiveModeSprite;

        [Header("Right 'How it Works' Tablet")]
        [SerializeField] private GameObject howItWorksPanel;
        [SerializeField] private Button toggleHowItWorksBtn;

        [Header("View-Only Sub Navigation Bar (Replaces console in View-Only)")]
        [SerializeField] private GameObject viewOnlyBottomBar;
        [SerializeField] private Button btnForcesAndMotion;
        [SerializeField] private Button btnExplodedParts;
        [SerializeField] private Image imgForcesHighlight;
        [SerializeField] private Image imgExplodedHighlight;
        [SerializeField] private GameObject explodedPartsPillsContainer;
        [SerializeField] private Button pillFulcrumBtn;
        [SerializeField] private Button pillBeamBtn;
        [SerializeField] private Button pillLoadStoneBtn;
        [SerializeField] private Button pillEffortBtn;

        [Header("Bottom Stone Console (Active ONLY in Interactive Mode)")]
        [SerializeField] private GameObject bottomConsole;
        [SerializeField] private Button rotateButton;
        [SerializeField] private Button forceDecreaseBtn;
        [SerializeField] private Button forceIncreaseBtn;
        [SerializeField] private Text forceValueText;
        [SerializeField] private Button fulcrumLeftBtn;
        [SerializeField] private Button fulcrumRightBtn;
        [SerializeField] private Text fulcrumStatusText;
        [SerializeField] private Slider fulcrumSlider;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button tryChallengeButton;
        [SerializeField] private Text liftStatusBannerText;

        [Header("3D In-World Annotations & Force Indicators")]
        [SerializeField] private GameObject loadAnnotationBadge;
        [SerializeField] private GameObject fulcrumAnnotationBadge;
        [SerializeField] private GameObject effortAnnotationBadge;
        [SerializeField] private LineRenderer effortArrowLine;
        [SerializeField] private LineRenderer loadArrowLine;
        [SerializeField] private LineRenderer fulcrumReactionArrowLine;
        [SerializeField] private GameObject forceVectorsRoot;
        [SerializeField] private Transform effortVectorObject;
        [SerializeField] private Transform loadVectorObject;
        [SerializeField] private Transform fulcrumVectorObject;
        [SerializeField] private TextMesh effortVectorText;
        [SerializeField] private TextMesh loadVectorText;
        [SerializeField] private TextMesh fulcrumVectorText;

        [Header("Exploded Character Dialogue UI")]
        private GameObject explodedDialoguePanel;
        private Image explodedCharAvatarImage;
        private Text explodedSpeakerNameText;
        private Text explodedDialogueBodyText;
        private Button explodedCloseButton;

        [Header("Part Detail Knowledge Modal")]
        [SerializeField] private GameObject partDetailModalPanel;
        [SerializeField] private Text partDetailTitleText;
        [SerializeField] private Text partDetailCategoryText;
        [SerializeField] private Text partDetailBodyText;
        [SerializeField] private Text partDetailFormulaText;
        [SerializeField] private Text partDetailLoreText;
        [SerializeField] private Button partDetailCloseBtn;
        [SerializeField] private Button partDetailBgDismissBtn;

        [Header("Challenge & Feedback Modal")]
        [SerializeField] private GameObject challengeModalPanel;
        [SerializeField] private Text challengeTitleText;
        [SerializeField] private Text challengeDescText;
        [SerializeField] private Text challengeFeedbackText;
        [SerializeField] private Button challengeSubmitBtn;
        [SerializeField] private Button challengeCloseBtn;
        [SerializeField] private ParticleSystem celebrationParticles;

        [Header("3D Model References")]
        [SerializeField] private Transform leverRoot;
        [SerializeField] private Transform beamPivot;
        [SerializeField] private Transform fulcrumRoot;
        [SerializeField] private Transform boulderRoot;
        [SerializeField] private Transform effortPoint;

        [Header("Character Companion & Live Voice")]
        [SerializeField] private Image characterCompanionImage;
        [SerializeField] private Sprite mayaFullSprite;
        [SerializeField] private Sprite senukFullSprite;
        [SerializeField] private AudioClip mayaBoulderLiftedClip;
        [SerializeField] private AudioClip mayaTooHeavyClip;
        [SerializeField] private AudioClip mayaBalancedClip;
        [SerializeField] private AudioClip senukBoulderLiftedClip;
        [SerializeField] private AudioClip senukTooHeavyClip;
        [SerializeField] private AudioClip senukBalancedClip;

        [Header("Pathirana: In-Situ AR Quiz & Assessment")]
        [SerializeField] private Button btnARQuiz;
        [SerializeField] private GameObject quizModalPanel;
        [SerializeField] private Text quizTitleText;
        [SerializeField] private Text quizCategoryText;
        [SerializeField] private Text quizQuestionText;
        [SerializeField] private Button[] quizOptionButtons;
        [SerializeField] private Text[] quizOptionTexts;
        [SerializeField] private GameObject quizFeedbackPanel;
        [SerializeField] private Text quizFeedbackText;
        [SerializeField] private Button quizNextButton;
        [SerializeField] private Button quizCloseButton;
        private Text quizTimerText;
        private Image quizTimerBarImage;
        private Coroutine quizTimerCoroutine;
        private int tierQuizCursor = 0;
        [SerializeField] private Button btnReviewMode;

        [Header("Pathirana: Spaced Retention & Research Exporter")]
        [SerializeField] private Button btnRetentionReport;
        [SerializeField] private GameObject retentionModalPanel;
        [SerializeField] private Text retentionSummaryText;
        [SerializeField] private Button retentionExportBtn;
        [SerializeField] private Button retentionCloseBtn;

        [Header("Abeywickrama: Adaptive Difficulty & Smart Hints")]
        [SerializeField] private Text adaptiveTierBadgeText;
        [SerializeField] private Button btnSmartHint;
        [SerializeField] private GameObject smartHintModalPanel;
        [SerializeField] private Text smartHintBodyText;
        [SerializeField] private Button smartHintCloseBtn;
        [SerializeField] private Image smartHintGlowImage;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip forceChangeClip;
        [SerializeField] private AudioClip partInspectClip;

        // State variables
        private InteractionMode currentMode = InteractionMode.ViewOnly; // DEFAULT IS VIEW ONLY
        private ViewOnlySubMode currentViewOnlySubMode = ViewOnlySubMode.ForcesAndMotion;
        private float currentEffortForceN = 60f; // Default 60N
        private float fulcrumOffset = 0f; // -0.4 to +0.4
        private bool isAutoRotating = true;
        private float currentBeamAngle = 0f;
        private float targetBeamAngle = 0f;
        private Coroutine boulderRollCoroutine;
        private bool isChallengeActive = false;
        public bool IsChallengeActive => isChallengeActive;
        private string activeCharacter = "Maya";
        private int lastVoiceState = -1;
        // --- Story Dialogue & Guided Mission Sequence State ---
        private GameObject storyDialoguePanel;
        private Image storyCharAvatarImage;
        private Text storySpeakerNameText;
        private Text storyDialogueBodyText;
        private Button storyNextBtn;
        private Text storyNextBtnText;
        private int currentStoryDialogueIndex = 0;
        private bool isGuidedStoryFlowActive = false;

        // Exploded View smooth transform targets
        private bool isExplodedView = false;
        private Vector3 fulcrumAssembledLocalPos = Vector3.zero;
        private Vector3 beamAssembledLocalPos = new Vector3(0f, 0.58f, 0f);
        private Vector3 boulderAssembledLocalPos = new Vector3(-0.90f, 0.15f, 0f);
        private Vector3 effortAssembledLocalPos = new Vector3(0.90f, 0.15f, 0f);

        private Vector3 fulcrumExplodedLocalPos = new Vector3(0f, -0.05f, 0f);
        private Vector3 beamExplodedLocalPos = new Vector3(0f, 0.68f, 0f);
        private Vector3 boulderExplodedLocalPos = new Vector3(-0.95f, 0.60f, 0f);
        private Vector3 effortExplodedLocalPos = new Vector3(0.95f, 0.60f, 0f);

        // Telemetry
        private int forceChangeCount = 0;
        private int fulcrumChangeCount = 0;
        private int challengeAttempts = 0;
        private float sessionStartTime;
        private Vector3 baseHintScale = Vector3.one;
        private bool questMarkerScanned = false;
        private bool hasUserInteractedWithPhysics = false;
        private Coroutine companionBubbleCoroutine;
        private Button continueStoryVictoryButton;

        private void Awake()
        {
            Debug.developerConsoleVisible = false;
            sessionStartTime = Time.time;
            AutoBindAllReferences();
            if (btnSmartHint != null && btnSmartHint.transform.localScale.sqrMagnitude > 0.01f)
            {
                baseHintScale = btnSmartHint.transform.localScale;
            }
            InitializeCharacterCompanion();
            SetupButtonListeners();
            if (leverRoot != null && leverRoot.GetComponent<ARMechanismViewer>() == null)
            {
                var viewer = leverRoot.gameObject.AddComponent<ARMechanismViewer>();
            }
            CaptureInitialObjectPositions();

            // Disable legacy line renderers that previously rendered as a flat white quad on the altar pedestal
            if (effortArrowLine != null) { effortArrowLine.enabled = false; effortArrowLine.gameObject.SetActive(false); }
            if (loadArrowLine != null) { loadArrowLine.enabled = false; loadArrowLine.gameObject.SetActive(false); }
            if (fulcrumReactionArrowLine != null) { fulcrumReactionArrowLine.enabled = false; fulcrumReactionArrowLine.gameObject.SetActive(false); }

            // Requirement 1 & 2: Hide "How It Works" tablet and Companion Avatar by default for a clean AR screen
            if (howItWorksPanel != null) howItWorksPanel.SetActive(true);
            if (characterCompanionImage != null) characterCompanionImage.gameObject.SetActive(false);
            if (liftStatusBannerText != null && liftStatusBannerText.transform.parent != null)
            {
                liftStatusBannerText.transform.parent.gameObject.SetActive(false);
            }
            if (challengeModalPanel != null) challengeModalPanel.SetActive(false);
        }

        private void Start()
        {
            Debug.developerConsoleVisible = false;
            AutoBindAllReferences();
            SetupButtonListeners();
            hasUserInteractedWithPhysics = false;
            ResetMechanism();

            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            if (questMarkerScanned || appMode == "Quest")
            {
                // In Quest mode (after scanning Page 9 marker), activate Interactive Mode console so student can freely experiment
                SetInteractionMode(InteractionMode.Interactive);
            }
            else
            {
                // In Sandbox ("Free Practice") mode, start in Interactive Mode so students can freely move fulcrum & apply force
                SetInteractionMode(InteractionMode.Interactive);
            }

            ApplyAppModeVisibility();
            UpdateAdaptiveTierHUD();
            SecretOfLostTemple.Adaptive.EngagementTracker.Instance?.StartTracking(ResearchData.Store.ParticipantCode);
            InitializeInteractiveStoryBranchUI();

            // Requirement 1, 2 & 3: Ensure How It Works, Companion Avatar, and Challenge Modal are hidden until explicitly triggered
            if (howItWorksPanel != null) howItWorksPanel.SetActive(true);
            if (characterCompanionImage != null) characterCompanionImage.gameObject.SetActive(false);
            if (liftStatusBannerText != null && liftStatusBannerText.transform.parent != null)
            {
                liftStatusBannerText.transform.parent.gameObject.SetActive(false);
            }
            if (challengeModalPanel != null) challengeModalPanel.SetActive(false);
        }

        /// <summary>
        /// Called by QuestARMarkerSpawner when the Page 9 Lever Marker is scanned and locked.
        /// Transitions into Interactive Mode with the bottom console active, WITHOUT auto-popping the Challenge Modal.
        /// </summary>
        public void OnQuestMarkerScanned()
        {
            questMarkerScanned = true;
            hasUserInteractedWithPhysics = false;
            AutoBindAllReferences();
            SetupButtonListeners();
            SetInteractionMode(InteractionMode.Interactive);

            // Keep screen clean: hide How It Works, Companion, and Challenge Modal (Challenge opens ONLY when tapping TRY CHALLENGE)
            if (howItWorksPanel != null) howItWorksPanel.SetActive(true);
            if (characterCompanionImage != null) characterCompanionImage.gameObject.SetActive(false);
            if (liftStatusBannerText != null && liftStatusBannerText.transform.parent != null)
            {
                liftStatusBannerText.transform.parent.gameObject.SetActive(false);
            }
            if (challengeModalPanel != null) challengeModalPanel.SetActive(false);

            string activeScene = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            if (activeScene != "FreePracticeScene" && appMode != "Sandbox")
            {
                StartGuidedStoryDialogueSequence();
            }

            // After scanning target image, after 10 seconds quiz should popup!
            microQuizTimer = 0f;
            microQuizEnabled = true;
            if (postScanQuizCoroutine != null) StopCoroutine(postScanQuizCoroutine);
            postScanQuizCoroutine = StartCoroutine(TriggerQuizAfterDelay(10.0f));
        }

        private IEnumerator TriggerQuizAfterDelay(float delaySeconds)
        {
            Debug.Log($"[MicroQuiz] Target scanned. Timed quiz will popup in {delaySeconds} seconds.");
            yield return new WaitForSeconds(delaySeconds);
            if (quizModalPanel == null || !quizModalPanel.activeSelf)
            {
                TriggerTimedMicroQuiz();
            }
        }

        private void ApplyAppModeVisibility()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            bool isSandbox = (activeScene == "FreePracticeScene" || appMode == "Sandbox");

            if (isSandbox)
            {
                // Dedicated Free Practice (FreePracticeScene):
                // Elevate TempleEnvironmentRoot & frame camera so the 3D Lever & Altar sit in the upper-middle viewport, completely above bottom buttons!
                var templeEnv = GameObject.Find("TempleEnvironmentRoot");
                if (templeEnv != null)
                {
                    templeEnv.SetActive(true);
                    templeEnv.transform.position = new Vector3(0f, 0.42f, 0f);
                    templeEnv.transform.rotation = Quaternion.identity;
                    templeEnv.transform.localScale = Vector3.one;

                    var pedestal = templeEnv.transform.Find("Temple_Altar_Pedestal");
                    if (pedestal != null)
                    {
                        pedestal.localPosition = new Vector3(0f, -0.26f, 1.20f);
                    }
                }

                if (leverRoot != null)
                {
                    if (templeEnv != null && leverRoot.parent != templeEnv.transform)
                    {
                        leverRoot.SetParent(templeEnv.transform, false);
                    }
                    leverRoot.gameObject.SetActive(true);
                    leverRoot.localPosition = new Vector3(0f, -0.20f, 1.20f);
                }

                if (Camera.main != null && activeScene == "FreePracticeScene")
                {
                    Camera.main.transform.localPosition = new Vector3(0f, 0.55f, -2.25f);
                    Camera.main.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
                }

                ApplyFreePracticeCompactUILayout();

                var scanOverlay = GameObject.Find("ScanningMarkerOverlay");
                if (scanOverlay != null) scanOverlay.SetActive(!questMarkerScanned);

                if (tryChallengeButton != null) tryChallengeButton.gameObject.SetActive(false);
                if (continueStoryVictoryButton != null) continueStoryVictoryButton.gameObject.SetActive(false);
                if (challengeModalPanel != null) challengeModalPanel.SetActive(false);
                if (btnSmartHint != null) btnSmartHint.gameObject.SetActive(false);
                if (smartHintModalPanel != null) smartHintModalPanel.SetActive(false);
                if (btnARQuiz != null) btnARQuiz.gameObject.SetActive(true);
                if (quizModalPanel != null && !quizModalPanel.activeSelf) quizModalPanel.SetActive(false);
                if (btnRetentionReport != null) btnRetentionReport.gameObject.SetActive(false);
                if (retentionModalPanel != null) retentionModalPanel.SetActive(false);
                if (adaptiveTierBadgeText != null && adaptiveTierBadgeText.transform.parent != null)
                {
                    adaptiveTierBadgeText.transform.parent.gameObject.SetActive(false);
                }
                if (storyChoiceWidget != null) storyChoiceWidget.SetActive(false);
                if (modeToggleButton != null) modeToggleButton.interactable = true;
                if (btnExplodedParts != null) btnExplodedParts.interactable = true;
            }
            else
            {
                // Dedicated Expedition / Story Quest Mode (MainARScene):
                if (tryChallengeButton != null) tryChallengeButton.gameObject.SetActive(!challengeResolved);
                if (continueStoryVictoryButton != null) continueStoryVictoryButton.gameObject.SetActive(challengeResolved);
                if (modeToggleButton != null) modeToggleButton.interactable = !StudyPolicy.Measured;
                if (btnExplodedParts != null) btnExplodedParts.interactable = !StudyPolicy.Passive;
                if (btnARQuiz != null) btnARQuiz.gameObject.SetActive(!StudyPolicy.Measured || ResearchData.Store.Current.embeddedAssessment);
                if (btnSmartHint != null) btnSmartHint.gameObject.SetActive(ResearchData.Store.PendingTest == null);
                if (adaptiveTierBadgeText != null && adaptiveTierBadgeText.transform.parent != null)
                {
                    adaptiveTierBadgeText.transform.parent.gameObject.SetActive(true);
                }
            }
        }

        private void ApplyFreePracticeCompactUILayout()
        {
            // 1. Center and compact the Interactive Bottom Console (since Try Challenge is hidden in Free Practice)
            if (bottomConsole != null)
            {
                var bcRt = bottomConsole.GetComponent<RectTransform>();
                if (bcRt != null)
                {
                    bcRt.anchorMin = new Vector2(0.5f, 0f);
                    bcRt.anchorMax = new Vector2(0.5f, 0f);
                    bcRt.pivot = new Vector2(0.5f, 0f);
                    bcRt.anchoredPosition = new Vector2(0f, 14f);
                    bcRt.sizeDelta = new Vector2(1180f, 145f);
                }

                var boxForce = bottomConsole.transform.Find("Box_Force")?.GetComponent<RectTransform>();
                if (boxForce != null)
                {
                    boxForce.anchorMin = boxForce.anchorMax = boxForce.pivot = new Vector2(0.5f, 0.5f);
                    boxForce.anchoredPosition = new Vector2(-360f, 0f);
                    boxForce.sizeDelta = new Vector2(330f, 145f);
                }

                var boxFulcrum = bottomConsole.transform.Find("Box_Fulcrum")?.GetComponent<RectTransform>();
                if (boxFulcrum != null)
                {
                    boxFulcrum.anchorMin = boxFulcrum.anchorMax = boxFulcrum.pivot = new Vector2(0.5f, 0.5f);
                    boxFulcrum.anchoredPosition = new Vector2(10f, 0f);
                    boxFulcrum.sizeDelta = new Vector2(310f, 145f);
                }

                if (resetButton != null)
                {
                    var resetRt = resetButton.GetComponent<RectTransform>();
                    if (resetRt != null)
                    {
                        resetRt.anchorMin = resetRt.anchorMax = resetRt.pivot = new Vector2(0.5f, 0.5f);
                        resetRt.anchoredPosition = new Vector2(320f, 0f);
                        resetRt.sizeDelta = new Vector2(138f, 138f);
                    }
                }
            }

            // 2. Compact the View-Only Bottom Bar so 360° Forces & Exploded Parts buttons don't block the 3D model
            if (viewOnlyBottomBar != null)
            {
                var voRt = viewOnlyBottomBar.GetComponent<RectTransform>();
                if (voRt != null)
                {
                    voRt.anchorMin = new Vector2(0.5f, 0f);
                    voRt.anchorMax = new Vector2(0.5f, 0f);
                    voRt.pivot = new Vector2(0.5f, 0f);
                    voRt.anchoredPosition = new Vector2(0f, 14f);
                    voRt.sizeDelta = new Vector2(1080f, 110f);
                }

                if (btnForcesAndMotion != null)
                {
                    var fmRt = btnForcesAndMotion.GetComponent<RectTransform>();
                    if (fmRt != null)
                    {
                        fmRt.anchoredPosition = new Vector2(-240f, 0f);
                        fmRt.sizeDelta = new Vector2(440f, 104f);
                    }
                }

                if (btnExplodedParts != null)
                {
                    var epRt = btnExplodedParts.GetComponent<RectTransform>();
                    if (epRt != null)
                    {
                        epRt.anchoredPosition = new Vector2(240f, 0f);
                        epRt.sizeDelta = new Vector2(440f, 104f);
                    }
                }
            }

            // 3. Position Exploded Parts Pills neatly right above the compact ViewOnlyBottomBar
            if (explodedPartsPillsContainer != null)
            {
                var pillsRt = explodedPartsPillsContainer.GetComponent<RectTransform>();
                if (pillsRt != null)
                {
                    pillsRt.anchorMin = new Vector2(0.5f, 0f);
                    pillsRt.anchorMax = new Vector2(0.5f, 0f);
                    pillsRt.pivot = new Vector2(0.5f, 0f);
                    pillsRt.anchoredPosition = new Vector2(0f, 132f);
                    pillsRt.sizeDelta = new Vector2(1180f, 48f);
                }
            }

            // 4. Keep Lift Status Banner cleanly above the compact console without covering the 3D lever
            if (liftStatusBannerText != null && liftStatusBannerText.transform.parent != null)
            {
                var bannerRt = liftStatusBannerText.transform.parent.GetComponent<RectTransform>();
                if (bannerRt != null)
                {
                    bannerRt.anchoredPosition = new Vector2(0f, 172f);
                    bannerRt.sizeDelta = new Vector2(780f, 68f);
                }
            }
        }

        private void AutoBindAllReferences()
        {
            var canvasTr = transform;
            var uiRoot = canvasTr.Find("LeverAR_UIRoot") ?? canvasTr;
            if (uiRoot != null)
            {
                EnsureChallengeModalBuilt(uiRoot);
                EnsureQuizModalBuilt(uiRoot);
                if (howItWorksPanel == null) howItWorksPanel = uiRoot.Find("Panel_HowItWorks")?.gameObject;
                if (toggleHowItWorksBtn == null) toggleHowItWorksBtn = uiRoot.Find("TopBar/Btn_HowItWorks")?.GetComponent<Button>();
                EnsureHowItWorksToggleButtonBuilt(uiRoot);

                if (backOrSettingsButton == null) backOrSettingsButton = uiRoot.Find("TopBar/Btn_Back")?.GetComponent<Button>();
                if (modeToggleButton == null) modeToggleButton = uiRoot.Find("TopBar/Btn_ModeToggle")?.GetComponent<Button>();
                if (modeToggleText == null) modeToggleText = uiRoot.Find("TopBar/Btn_ModeToggle/Text")?.GetComponent<Text>();
                if (modeToggleBg == null) modeToggleBg = uiRoot.Find("TopBar/Btn_ModeToggle")?.GetComponent<Image>();

                // Top Carved Banner: Apply custom lever_ar_title sprite asset and hide redundant plain text
                var bannerTr = uiRoot.Find("TopBar/Banner_LeverAR");
                if (bannerTr != null)
                {
                    var bannerImg = bannerTr.GetComponent<Image>();
                    if (bannerImg != null)
                    {
                        var sprite = Resources.Load<Sprite>("lever_ar_title");
                        if (sprite != null)
                        {
                            bannerImg.sprite = sprite;
                            bannerImg.color = Color.white;
                            bannerImg.preserveAspect = true;
                        }
                    }
                    var rt = bannerTr.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = new Vector2(450f, 150f);
                    }
                    var txt = bannerTr.Find("Text");
                    if (txt != null)
                    {
                        txt.gameObject.SetActive(false);
                    }
                }

                if (viewOnlyBottomBar == null) viewOnlyBottomBar = uiRoot.Find("ViewOnlyBottomBar")?.gameObject;
                if (btnForcesAndMotion == null) btnForcesAndMotion = uiRoot.Find("ViewOnlyBottomBar/Btn_ForcesAndMotion")?.GetComponent<Button>();
                if (btnExplodedParts == null) btnExplodedParts = uiRoot.Find("ViewOnlyBottomBar/Btn_ExplodedParts")?.GetComponent<Button>();
                if (imgForcesHighlight == null) imgForcesHighlight = uiRoot.Find("ViewOnlyBottomBar/Btn_ForcesAndMotion")?.GetComponent<Image>();
                if (imgExplodedHighlight == null) imgExplodedHighlight = uiRoot.Find("ViewOnlyBottomBar/Btn_ExplodedParts")?.GetComponent<Image>();

                if (explodedPartsPillsContainer == null) explodedPartsPillsContainer = uiRoot.Find("Pills_ExplodedParts")?.gameObject;
                if (explodedPartsPillsContainer != null) explodedPartsPillsContainer.SetActive(false);
                EnsureExploded3DColliders();
                if (pillFulcrumBtn == null) pillFulcrumBtn = uiRoot.Find("Pills_ExplodedParts/Pill_Fulcrum")?.GetComponent<Button>();
                if (pillBeamBtn == null) pillBeamBtn = uiRoot.Find("Pills_ExplodedParts/Pill_Beam")?.GetComponent<Button>();
                if (pillLoadStoneBtn == null) pillLoadStoneBtn = uiRoot.Find("Pills_ExplodedParts/Pill_LoadStone")?.GetComponent<Button>();
                if (pillEffortBtn == null) pillEffortBtn = uiRoot.Find("Pills_ExplodedParts/Pill_Effort")?.GetComponent<Button>();

                // Ensure pure English text in View-Only UI components
                if (btnForcesAndMotion != null) { var t = btnForcesAndMotion.GetComponentInChildren<Text>(); if (t != null) t.text = "<b>🔄 360° FORCES & MOTION</b>\n<size=17><color=#ffe066>Vector Arrows & Rotation</color></size>"; }
                if (btnExplodedParts != null) { var t = btnExplodedParts.GetComponentInChildren<Text>(); if (t != null) t.text = "<b>🔍 PARTS & ANATOMY</b>\n<size=17><color=#80e5ff>Exploded View & Lore</color></size>"; }
                if (pillFulcrumBtn != null) { var t = pillFulcrumBtn.GetComponentInChildren<Text>(); if (t != null) t.text = "🏔️ 1. FULCRUM"; }
                if (pillBeamBtn != null) { var t = pillBeamBtn.GetComponentInChildren<Text>(); if (t != null) t.text = "🪵 2. LEVER BEAM"; }
                if (pillLoadStoneBtn != null) { var t = pillLoadStoneBtn.GetComponentInChildren<Text>(); if (t != null) t.text = "🪨 3. LOAD STONE"; }
                if (pillEffortBtn != null) { var t = pillEffortBtn.GetComponentInChildren<Text>(); if (t != null) t.text = "⚡ 4. EFFORT POINT"; }

                if (bottomConsole == null) bottomConsole = uiRoot.Find("BottomConsole")?.gameObject;
                if (forceDecreaseBtn == null) forceDecreaseBtn = uiRoot.Find("BottomConsole/Box_Force/Btn_Dec")?.GetComponent<Button>();
                if (forceIncreaseBtn == null) forceIncreaseBtn = uiRoot.Find("BottomConsole/Box_Force/Btn_Inc")?.GetComponent<Button>();
                if (forceValueText == null) forceValueText = uiRoot.Find("BottomConsole/Box_Force/ValText")?.GetComponent<Text>();
                if (fulcrumLeftBtn == null) fulcrumLeftBtn = uiRoot.Find("BottomConsole/Box_Fulcrum/Btn_FulcLeft")?.GetComponent<Button>();
                if (fulcrumRightBtn == null) fulcrumRightBtn = uiRoot.Find("BottomConsole/Box_Fulcrum/Btn_FulcRight")?.GetComponent<Button>();
                if (fulcrumSlider == null) fulcrumSlider = uiRoot.Find("BottomConsole/Box_Fulcrum/Slider")?.GetComponent<Slider>();
                if (resetButton == null) resetButton = uiRoot.Find("BottomConsole/Btn_Reset")?.GetComponent<Button>();
                if (tryChallengeButton == null) tryChallengeButton = uiRoot.Find("BottomConsole/Btn_TryChallenge")?.GetComponent<Button>();
                EnsureContinueStoryVictoryButtonBuilt();
                if (liftStatusBannerText == null) liftStatusBannerText = uiRoot.Find("Banner_LiftStatus/Text")?.GetComponent<Text>();

                if (partDetailModalPanel == null) partDetailModalPanel = uiRoot.Find("Panel_PartDetailModal")?.gameObject;
                if (partDetailTitleText == null) partDetailTitleText = uiRoot.Find("Panel_PartDetailModal/ModalCard/TitleText")?.GetComponent<Text>();
                if (partDetailCategoryText == null) partDetailCategoryText = uiRoot.Find("Panel_PartDetailModal/ModalCard/CategoryText")?.GetComponent<Text>();
                if (partDetailBodyText == null) partDetailBodyText = uiRoot.Find("Panel_PartDetailModal/ModalCard/BodyText")?.GetComponent<Text>();
                if (partDetailFormulaText == null) partDetailFormulaText = uiRoot.Find("Panel_PartDetailModal/ModalCard/FormulaText")?.GetComponent<Text>();
                if (partDetailLoreText == null) partDetailLoreText = uiRoot.Find("Panel_PartDetailModal/ModalCard/LoreText")?.GetComponent<Text>();
                if (partDetailCloseBtn == null) partDetailCloseBtn = uiRoot.Find("Panel_PartDetailModal/ModalCard/Btn_Close")?.GetComponent<Button>();
                if (partDetailBgDismissBtn == null) partDetailBgDismissBtn = uiRoot.Find("Panel_PartDetailModal/BackdropDim")?.GetComponent<Button>();

                if (characterCompanionImage == null) characterCompanionImage = uiRoot.Find("Companion_Character")?.GetComponent<Image>();

                // Pathirana: AR Quiz & Assessment References
                if (btnARQuiz == null) btnARQuiz = uiRoot.Find("TopBar/Btn_ARQuiz")?.GetComponent<Button>();
                if (quizModalPanel == null) quizModalPanel = uiRoot.Find("Panel_ARQuizModal")?.gameObject;
                if (quizModalPanel != null)
                {
                    if (quizTitleText == null) quizTitleText = quizModalPanel.transform.Find("ModalCard/TitleText")?.GetComponent<Text>();
                    if (quizCategoryText == null) quizCategoryText = quizModalPanel.transform.Find("ModalCard/CategoryText")?.GetComponent<Text>();
                    if (quizQuestionText == null) quizQuestionText = quizModalPanel.transform.Find("ModalCard/QuestionText")?.GetComponent<Text>();
                    if (quizFeedbackPanel == null) quizFeedbackPanel = quizModalPanel.transform.Find("ModalCard/Panel_Feedback")?.gameObject;
                    if (quizFeedbackPanel != null && quizFeedbackText == null) quizFeedbackText = quizFeedbackPanel.transform.Find("FeedbackText")?.GetComponent<Text>();
                    if (quizNextButton == null)
                    {
                        quizNextButton = quizModalPanel.transform.Find("ModalCard/Btn_NextQuestion")?.GetComponent<Button>()
                                      ?? quizModalPanel.transform.Find("ModalCard/Btn_Next")?.GetComponent<Button>();
                    }
                    if (quizCloseButton == null) quizCloseButton = quizModalPanel.transform.Find("ModalCard/Btn_Close")?.GetComponent<Button>();
                    if (btnReviewMode == null) btnReviewMode = quizModalPanel.transform.Find("ModalCard/Btn_ReviewMode")?.GetComponent<Button>();
                    if (btnRetentionReport == null) btnRetentionReport = quizModalPanel.transform.Find("ModalCard/Btn_RetentionReport")?.GetComponent<Button>();

                    if (quizOptionButtons == null || quizOptionButtons.Length < 4)
                    {
                        quizOptionButtons = new Button[4];
                        quizOptionTexts = new Text[4];
                        for (int i = 0; i < 4; i++)
                        {
                            var optTr = quizModalPanel.transform.Find($"ModalCard/Btn_Option_{i + 1}");
                            if (optTr != null)
                            {
                                quizOptionButtons[i] = optTr.GetComponent<Button>();
                                quizOptionTexts[i] = optTr.Find("Text")?.GetComponent<Text>();
                            }
                        }
                    }
                }

                // Pathirana: Spaced Retention & Data Exporter References
                if (retentionModalPanel == null) retentionModalPanel = uiRoot.Find("Panel_RetentionModal")?.gameObject;
                if (retentionModalPanel != null)
                {
                    if (retentionSummaryText == null) retentionSummaryText = retentionModalPanel.transform.Find("ModalCard/SummaryText")?.GetComponent<Text>();
                    if (retentionExportBtn == null) retentionExportBtn = retentionModalPanel.transform.Find("ModalCard/Btn_ExportCSV")?.GetComponent<Button>();
                    if (retentionCloseBtn == null) retentionCloseBtn = retentionModalPanel.transform.Find("ModalCard/Btn_Close")?.GetComponent<Button>();
                }

                // Abeywickrama: Adaptive Difficulty & Smart Hints
                if (adaptiveTierBadgeText == null) adaptiveTierBadgeText = uiRoot.Find("TopBar/Badge_AdaptiveTier/Text")?.GetComponent<Text>();
                if (btnSmartHint == null) btnSmartHint = uiRoot.Find("TopBar/Btn_SmartHint")?.GetComponent<Button>();
                if (btnSmartHint != null && smartHintGlowImage == null) smartHintGlowImage = btnSmartHint.GetComponent<Image>();
                if (smartHintModalPanel == null) smartHintModalPanel = uiRoot.Find("Panel_SmartHintModal")?.gameObject;
                if (smartHintModalPanel != null)
                {
                    if (smartHintBodyText == null) smartHintBodyText = smartHintModalPanel.transform.Find("ModalCard/BodyText")?.GetComponent<Text>();
                    if (smartHintCloseBtn == null) smartHintCloseBtn = smartHintModalPanel.transform.Find("ModalCard/Btn_Close")?.GetComponent<Button>();
                }

                EnsureStarsBadgeBuilt(uiRoot);
            }

            if (leverRoot == null)
            {
                var leverGo = GameObject.Find("SimpleMachine_Lever");
                if (leverGo != null) leverRoot = leverGo.transform;
            }

            if (leverRoot != null)
            {
                if (beamPivot == null) beamPivot = leverRoot.Find("Lever_Beam");
                if (fulcrumRoot == null) fulcrumRoot = leverRoot.Find("Fulcrum_Pivot");
                if (beamPivot != null)
                {
                    if (boulderRoot == null) boulderRoot = beamPivot.Find("Load_Stone_DoorWeight");
                    if (effortPoint == null) effortPoint = beamPivot.Find("Effort_Point_Handle");
                }

                if (forceVectorsRoot == null) forceVectorsRoot = leverRoot.Find("ForceVectorsRoot")?.gameObject;
                if (forceVectorsRoot != null)
                {
                    if (effortVectorObject == null) effortVectorObject = forceVectorsRoot.transform.Find("Vector_EffortForce");
                    if (loadVectorObject == null) loadVectorObject = forceVectorsRoot.transform.Find("Vector_LoadWeight");
                    if (fulcrumVectorObject == null) fulcrumVectorObject = forceVectorsRoot.transform.Find("Vector_FulcrumReaction");

                    if (effortVectorObject != null && effortVectorText == null) effortVectorText = effortVectorObject.GetComponentInChildren<TextMesh>();
                    if (loadVectorObject != null && loadVectorText == null) loadVectorText = loadVectorObject.GetComponentInChildren<TextMesh>();
                    if (fulcrumVectorObject != null && fulcrumVectorText == null) fulcrumVectorText = fulcrumVectorObject.GetComponentInChildren<TextMesh>();
                }
            }
        }

        public void BindTrackedLever(GameObject instance)
        {
            if (instance == null || leverRoot == instance.transform) return;
            var existingVectors = instance.transform.Find("ForceVectorsRoot");
            var template = forceVectorsRoot;
            leverRoot = instance.transform;
            beamPivot = leverRoot.Find("Lever_Beam");
            fulcrumRoot = leverRoot.Find("Fulcrum_Pivot");
            boulderRoot = beamPivot != null ? beamPivot.Find("Load_Stone_DoorWeight") : null;
            effortPoint = beamPivot != null ? beamPivot.Find("Effort_Point_Handle") : null;
            forceVectorsRoot = existingVectors != null ? existingVectors.gameObject :
                (template != null ? Instantiate(template, leverRoot, false) : null);
            if (forceVectorsRoot != null) forceVectorsRoot.name = "ForceVectorsRoot";
            effortVectorObject = loadVectorObject = fulcrumVectorObject = null;
            effortVectorText = loadVectorText = fulcrumVectorText = null;
            AutoBindAllReferences();
            CaptureInitialObjectPositions();
            UpdateInWorldBadgesAndArrows();
        }

        private void CaptureInitialObjectPositions()
        {
            if (fulcrumRoot != null)
            {
                fulcrumRoot.localPosition = new Vector3(0f, 0f, 0f);
                fulcrumAssembledLocalPos = fulcrumRoot.localPosition;
            }
            if (beamPivot != null)
            {
                // Ensure Lever_Beam rests cleanly on the apex of Fulcrum_Pivot (y = 0.58f, since Fulcrum apex is at y = 0.553f) without piercing!
                beamPivot.localPosition = new Vector3(beamPivot.localPosition.x, 0.58f, beamPivot.localPosition.z);
                beamAssembledLocalPos = beamPivot.localPosition;
            }
            if (boulderRoot != null)
            {
                boulderRoot.localPosition = new Vector3(-0.90f, 0.15f, 0f);
                boulderAssembledLocalPos = boulderRoot.localPosition;

                // Ensure child Model_Instance_Boulder sits on TOP of the wooden beam (y = 0.18f, never negative/sunken)
                var boulderMesh = boulderRoot.Find("Model_Instance_Boulder");
                if (boulderMesh != null)
                {
                    boulderMesh.localPosition = new Vector3(0f, 0.18f, 0f);
                }
            }
            if (effortPoint != null)
            {
                effortPoint.localPosition = new Vector3(0.90f, 0.15f, 0f);
                effortAssembledLocalPos = effortPoint.localPosition;
            }
        }

        private void InitializeCharacterCompanion()
        {
            activeCharacter = ResearchProgress.GetString("SelectedCharacter", "Maya");
            if (characterCompanionImage != null)
            {
                if (activeCharacter == "Senuk" && senukFullSprite != null)
                {
                    characterCompanionImage.sprite = senukFullSprite;
                }
                else if (mayaFullSprite != null)
                {
                    characterCompanionImage.sprite = mayaFullSprite;
                }
                characterCompanionImage.preserveAspect = true;
            }
        }

        private void SetupButtonListeners()
        {
            if (backOrSettingsButton != null)
            {
                backOrSettingsButton.onClick.RemoveAllListeners();
                backOrSettingsButton.onClick.AddListener(OnBackClicked);
            }

            if (modeToggleButton != null)
            {
                modeToggleButton.onClick.RemoveAllListeners();
                modeToggleButton.onClick.AddListener(ToggleInteractionMode);
            }

            // View-Only Sub-Bar Buttons
            if (btnForcesAndMotion != null)
            {
                btnForcesAndMotion.onClick.RemoveAllListeners();
                btnForcesAndMotion.onClick.AddListener(() => SetViewOnlySubMode(ViewOnlySubMode.ForcesAndMotion));
            }

            if (btnExplodedParts != null)
            {
                btnExplodedParts.onClick.RemoveAllListeners();
                btnExplodedParts.onClick.AddListener(() => SetViewOnlySubMode(ViewOnlySubMode.ExplodedParts));
            }

            // Exploded View Part Selection Pills
            if (pillFulcrumBtn != null) { pillFulcrumBtn.onClick.RemoveAllListeners(); pillFulcrumBtn.onClick.AddListener(() => ShowPartDetails("fulcrum")); }
            if (pillBeamBtn != null) { pillBeamBtn.onClick.RemoveAllListeners(); pillBeamBtn.onClick.AddListener(() => ShowPartDetails("beam")); }
            if (pillLoadStoneBtn != null) { pillLoadStoneBtn.onClick.RemoveAllListeners(); pillLoadStoneBtn.onClick.AddListener(() => ShowPartDetails("load")); }
            if (pillEffortBtn != null) { pillEffortBtn.onClick.RemoveAllListeners(); pillEffortBtn.onClick.AddListener(() => ShowPartDetails("effort")); }

            // Part Detail Modal Close Buttons
            if (partDetailCloseBtn != null) { partDetailCloseBtn.onClick.RemoveAllListeners(); partDetailCloseBtn.onClick.AddListener(ClosePartDetailsModal); }
            if (partDetailBgDismissBtn != null) { partDetailBgDismissBtn.onClick.RemoveAllListeners(); partDetailBgDismissBtn.onClick.AddListener(ClosePartDetailsModal); }

            // Pathirana: AR Quiz & Assessment Listeners
            if (btnARQuiz != null) { btnARQuiz.onClick.RemoveAllListeners(); btnARQuiz.onClick.AddListener(OpenARQuiz); }
            if (quizCloseButton != null) { quizCloseButton.onClick.RemoveAllListeners(); quizCloseButton.onClick.AddListener(CloseARQuiz); }
            if (quizNextButton != null) { quizNextButton.onClick.RemoveAllListeners(); quizNextButton.onClick.AddListener(NextQuizQuestion); }
            if (btnReviewMode != null) { btnReviewMode.onClick.RemoveAllListeners(); btnReviewMode.onClick.AddListener(ToggleReviewMode); }
            if (btnRetentionReport != null) { btnRetentionReport.onClick.RemoveAllListeners(); btnRetentionReport.onClick.AddListener(OpenRetentionSummary); }
            if (retentionCloseBtn != null) { retentionCloseBtn.onClick.RemoveAllListeners(); retentionCloseBtn.onClick.AddListener(CloseRetentionSummary); }
            if (retentionExportBtn != null) { retentionExportBtn.onClick.RemoveAllListeners(); retentionExportBtn.onClick.AddListener(ExportResearchCSV); }

            if (quizOptionButtons != null)
            {
                for (int i = 0; i < quizOptionButtons.Length; i++)
                {
                    int optIdx = i;
                    if (quizOptionButtons[i] != null)
                    {
                        quizOptionButtons[i].onClick.RemoveAllListeners();
                        quizOptionButtons[i].onClick.AddListener(() => SelectQuizAnswer(optIdx));
                    }
                }
            }

            // Abeywickrama: Adaptive Smart Hint Listeners
            if (btnSmartHint != null) { btnSmartHint.onClick.RemoveAllListeners(); btnSmartHint.onClick.AddListener(TriggerSmartHint); }
            if (smartHintCloseBtn != null) { smartHintCloseBtn.onClick.RemoveAllListeners(); smartHintCloseBtn.onClick.AddListener(CloseSmartHint); }

            // Interactive Bottom Console Buttons
            if (rotateButton != null)
            {
                rotateButton.onClick.RemoveAllListeners();
                rotateButton.onClick.AddListener(OnRotateClicked);
            }

            if (forceDecreaseBtn != null)
            {
                forceDecreaseBtn.onClick.RemoveAllListeners();
                forceDecreaseBtn.onClick.AddListener(() => AdjustForce(-20f));
            }

            if (forceIncreaseBtn != null)
            {
                forceIncreaseBtn.onClick.RemoveAllListeners();
                forceIncreaseBtn.onClick.AddListener(() => AdjustForce(+20f));
            }

            if (fulcrumLeftBtn != null)
            {
                fulcrumLeftBtn.onClick.RemoveAllListeners();
                fulcrumLeftBtn.onClick.AddListener(() => MoveFulcrumStep(-0.10f));
            }

            if (fulcrumRightBtn != null)
            {
                fulcrumRightBtn.onClick.RemoveAllListeners();
                fulcrumRightBtn.onClick.AddListener(() => MoveFulcrumStep(+0.10f));
            }

            if (fulcrumSlider != null)
            {
                fulcrumSlider.onValueChanged.RemoveAllListeners();
                fulcrumSlider.onValueChanged.AddListener(OnFulcrumSliderChanged);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(ResetMechanism);
            }

            if (tryChallengeButton != null)
            {
                tryChallengeButton.onClick.RemoveAllListeners();
                tryChallengeButton.onClick.AddListener(OpenChallengeModal);
            }

            if (challengeCloseBtn != null)
            {
                challengeCloseBtn.onClick.RemoveAllListeners();
                challengeCloseBtn.onClick.AddListener(CloseChallengeModal);
            }

            if (challengeSubmitBtn != null)
            {
                challengeSubmitBtn.onClick.RemoveAllListeners();
                challengeSubmitBtn.onClick.AddListener(EvaluateChallenge);
            }

            if (toggleHowItWorksBtn != null)
            {
                toggleHowItWorksBtn.onClick.RemoveAllListeners();
                toggleHowItWorksBtn.onClick.AddListener(ToggleHowItWorks);
            }
        }

        private void Update()
        {
            // Timed Micro-Quiz Trigger (Every ~10s of active AR exploration after scanning marker) - RUNS FIRST!
            if (microQuizEnabled && !isGuidedStoryFlowActive && questMarkerScanned)
            {
                bool modalOpen = (quizModalPanel != null && quizModalPanel.activeSelf) ||
                                 (challengeModalPanel != null && challengeModalPanel.activeSelf) ||
                                 (partDetailModalPanel != null && partDetailModalPanel.activeSelf) ||
                                 (smartHintModalPanel != null && smartHintModalPanel.activeSelf) ||
                                 (retentionModalPanel != null && retentionModalPanel.activeSelf) ||
                                 (storyDialoguePanel != null && storyDialoguePanel.activeSelf);

                if (!modalOpen)
                {
                    microQuizTimer += Time.deltaTime;
                    if (microQuizTimer >= MICRO_QUIZ_INTERVAL)
                    {
                        microQuizTimer = 0f;
                        TriggerTimedMicroQuiz();
                    }
                }
            }

            // Requirement 1: If user taps anywhere outside howItWorksPanel while it is open, close it
            // howItWorksPanel stays visible as requested by user

            if (Input.anyKeyDown || Input.touchCount > 0)
            {
                lastInteractionTimestamp = Time.time;
            }

            // Track user activity for Abeywickrama's Smart Hint & Engagement Tracker
            if (!StudyPolicy.Measured || StudyPolicy.Tracking)
            {
                if (researchForeground && quizModalPanel != null && quizModalPanel.activeSelf && !researchAnswerSubmitted) researchQuestionSeconds += Time.unscaledDeltaTime;
                if (pendingFulcrumLog >= 0 && Time.unscaledTime - pendingFulcrumLog > .4f) {
                    ResearchData.Record("fulcrum_committed", fulcrumOffset.ToString(System.Globalization.CultureInfo.InvariantCulture)); pendingFulcrumLog = -1;
                    ResearchCloudSync.Instance?.TriggerImmediateSync($"Fulcrum moved to {fulcrumOffset:+0.00;-0.00;0.00}m");
                }
                if (StudyPolicy.Passive) {
                    demonstrationTime += Time.deltaTime;
                    fulcrumOffset = 0f;
                    currentEffortForceN = 80f + 50f * Mathf.Sin(demonstrationTime * .7f);
                    targetBeamAngle = currentEffortForceN > 105f ? -16f : currentEffortForceN < 95f ? 20f : 0f;
                    UpdateForceUI();
                    if (liftStatusBannerText != null) liftStatusBannerText.text = "Standard demonstration: equal arms, load 100 N. Effort " + currentEffortForceN.ToString("F0") + " N. Above 100 N the load rises (ideal lever).";
                }
            }

            // Pulse Smart Hint button scale when learner hesitates (>15s idle)
            if (btnSmartHint != null)
            {
                if (baseHintScale.sqrMagnitude < 0.01f)
                {
                    baseHintScale = btnSmartHint.transform.localScale.sqrMagnitude > 0.01f ? btnSmartHint.transform.localScale : Vector3.one;
                }

                bool isIdle = (Time.time - lastInteractionTimestamp) > 15f;
                if (isIdle)
                {
                    float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);
                    btnSmartHint.transform.localScale = baseHintScale * (1.0f + 0.08f * pulse);
                }
                else
                {
                    btnSmartHint.transform.localScale = baseHintScale;
                }
            }

            // Smooth Beam Tilt Animation (Only in Interactive mode or when not exploded)
            if (beamPivot != null && !isExplodedView)
            {
                currentBeamAngle = Mathf.Lerp(currentBeamAngle, targetBeamAngle, Time.deltaTime * 6f);
                beamPivot.localRotation = Quaternion.Euler(0f, 0f, currentBeamAngle);
            }
            else if (beamPivot != null && isExplodedView)
            {
                beamPivot.localRotation = Quaternion.Slerp(beamPivot.localRotation, Quaternion.identity, Time.deltaTime * 6f);
            }

            // Handle Exploded View smooth positioning
            UpdateExplodedTransforms();

            // Update floating badges and 3D force direction vectors
            UpdateInWorldBadgesAndArrows();

            // Check for direct 3D part clicks on touchscreen / mouse
            HandleDirect3DPartClicks();
        }

        private void UpdateExplodedTransforms()
        {
            float speed = Time.deltaTime * 5f;

            if (fulcrumRoot != null)
            {
                Vector3 targetPos = isExplodedView ?
                    new Vector3(fulcrumOffset, -0.28f, fulcrumExplodedLocalPos.z) :
                    new Vector3(fulcrumOffset, fulcrumAssembledLocalPos.y, fulcrumAssembledLocalPos.z);
                fulcrumRoot.localPosition = Vector3.Lerp(fulcrumRoot.localPosition, targetPos, speed);
            }

            if (beamPivot != null)
            {
                Vector3 targetPos = isExplodedView ? new Vector3(0f, 0.46f, 0f) : beamAssembledLocalPos;
                if (!isExplodedView) targetPos += new Vector3(fulcrumOffset, 0, 0) - beamPivot.localRotation * new Vector3(fulcrumOffset, 0, 0);
                beamPivot.localPosition = Vector3.Lerp(beamPivot.localPosition, targetPos, speed);
                if (isExplodedView)
                {
                    beamPivot.localRotation = Quaternion.Slerp(beamPivot.localRotation, Quaternion.identity, speed);
                }
            }

            if (boulderRoot != null && boulderRollCoroutine == null)
            {
                Vector3 targetPos = isExplodedView ? new Vector3(-1.10f, 0.70f, 0f) : boulderAssembledLocalPos;
                boulderRoot.localPosition = Vector3.Lerp(boulderRoot.localPosition, targetPos, speed);
                if (!isExplodedView)
                {
                    boulderRoot.localRotation = Quaternion.Slerp(boulderRoot.localRotation, Quaternion.identity, speed);
                }
            }

            if (effortPoint != null)
            {
                Vector3 targetPos = isExplodedView ? effortExplodedLocalPos : effortAssembledLocalPos;
                effortPoint.localPosition = Vector3.Lerp(effortPoint.localPosition, targetPos, speed);
            }
        }

        private void UpdateInWorldBadgesAndArrows()
        {
            if (leverRoot == null) return;
            var cam = Camera.main;
            Vector3 up = leverRoot.up;
            float scale = Mathf.Abs(leverRoot.lossyScale.y);
            bool show = (currentMode == InteractionMode.ViewOnly && currentViewOnlySubMode == ViewOnlySubMode.ForcesAndMotion)
                        || currentMode == InteractionMode.Interactive;
            if (forceVectorsRoot == null) {
                forceVectorsRoot = new GameObject("ForceVectorsRoot");
                forceVectorsRoot.transform.SetParent(leverRoot, false);
            }
            forceVectorsRoot.SetActive(show);
            UpdateBadge(loadAnnotationBadge, boulderRoot, up * .45f * scale, cam);
            UpdateBadge(effortAnnotationBadge, effortPoint, up * .4f * scale, cam);
            UpdateBadge(fulcrumAnnotationBadge, fulcrumRoot, -up * .3f * scale, cam);
            if (!show) return;

            // Effort Force Arrow (Vivid Gold) pointing DOWN to effort handle
            float effortLen = Mathf.Clamp(0.18f + (currentEffortForceN / 160f) * 0.22f, 0.18f, 0.40f) * scale;
            UpdateForceArrow(ref effortVectorObject, ref effortVectorText, "Vector_EffortForce", effortPoint,
                -up, new Color(1f, 0.85f, 0.0f), $"EFFORT {currentEffortForceN:F0} N", effortLen, scale, cam, up * (0.05f * scale));

            // Load Weight Arrow (Vivid Red) pointing DOWN to boulder top
            float loadLen = 0.30f * scale;
            UpdateForceArrow(ref loadVectorObject, ref loadVectorText, "Vector_LoadWeight", boulderRoot,
                -up, new Color(1f, 0.15f, 0.15f), "LOAD 100 N", loadLen, scale, cam, up * (0.28f * scale));

            // Fulcrum Support Reaction Arrow (Vivid Green) pointing UP under the pivot
            float fulcrumForce = 100f + currentEffortForceN;
            float fulcrumLen = Mathf.Clamp(0.20f + (fulcrumForce / 260f) * 0.20f, 0.20f, 0.40f) * scale;
            UpdateForceArrow(ref fulcrumVectorObject, ref fulcrumVectorText, "Vector_FulcrumReaction", fulcrumRoot,
                up, new Color(0.0f, 0.95f, 0.45f), $"SUPPORT {fulcrumForce:F0} N", fulcrumLen, scale, cam, up * (0.55f * scale));

            // Legacy lines are replaced by arrows with visible heads.
            if (effortArrowLine != null) effortArrowLine.enabled = false;
            if (loadArrowLine != null) loadArrowLine.enabled = false;
            if (fulcrumReactionArrowLine != null) fulcrumReactionArrowLine.enabled = false;
        }

        private static void UpdateBadge(GameObject badge, Transform anchor, Vector3 offset, Camera cam)
        {
            if (badge == null || anchor == null) return;
            badge.transform.position = anchor.position + offset;
            var canvas = badge.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && cam != null) {
                badge.transform.rotation = cam.transform.rotation;
                SetWorldScale(badge.transform, Vector3.one * WorldUnitsPerPixel(cam, badge.transform.position));
            }
        }

        private static float WorldUnitsPerPixel(Camera cam, Vector3 position)
        {
            float height = cam.orthographic ? 2f * cam.orthographicSize :
                2f * Mathf.Max(.1f, Vector3.Dot(position - cam.transform.position, cam.transform.forward)) * Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * .5f);
            return height / Mathf.Max(1, cam.pixelHeight);
        }

        private static void SetWorldScale(Transform target, Vector3 size)
        {
            var parent = target.parent != null ? target.parent.lossyScale : Vector3.one;
            target.localScale = new Vector3(size.x / Mathf.Max(.0001f, Mathf.Abs(parent.x)),
                size.y / Mathf.Max(.0001f, Mathf.Abs(parent.y)), size.z / Mathf.Max(.0001f, Mathf.Abs(parent.z)));
        }

        private void UpdateForceArrow(ref Transform root, ref TextMesh label, string name, Transform anchor,
            Vector3 direction, Color color, string caption, float length, float scale, Camera cam, Vector3 anchorOffset)
        {
            if (anchor == null) return;
            if (root == null) {
                root = new GameObject(name).transform;
                root.SetParent(forceVectorsRoot.transform, false);
            }
            root.gameObject.SetActive(true);
            Vector3 tip = anchor.position + anchorOffset;
            root.position = tip;

            var line = root.GetComponent<LineRenderer>();
            if (line == null) {
                line = root.gameObject.AddComponent<LineRenderer>();
                Shader s = Shader.Find("Universal Render Pipeline/Unlit") 
                           ?? Shader.Find("Sprites/Default") 
                           ?? Shader.Find("Unlit/Color")
                           ?? Shader.Find("UI/Default");
                if (s != null) line.sharedMaterial = new Material(s);
            }
            line.useWorldSpace = true;
            line.positionCount = 5;
            float unit = cam != null ? WorldUnitsPerPixel(cam, tip) : 0.001f;
            Vector3 tail = tip - direction * length;
            Vector3 side = cam != null ? cam.transform.right : leverRoot.right;

            line.startWidth = line.endWidth = Mathf.Max(0.014f * scale, unit * 8f);
            line.startColor = line.endColor = color;
            line.sortingOrder = 40;

            float headLen = length * 0.25f;
            float headSpread = length * 0.14f;

            line.SetPosition(0, tail);
            line.SetPosition(1, tip);
            line.SetPosition(2, tip - direction * headLen + side * headSpread);
            line.SetPosition(3, tip);
            line.SetPosition(4, tip - direction * headLen - side * headSpread);

            if (label == null) {
                var labelGo = new GameObject("ForceLabel");
                labelGo.transform.SetParent(root, false);
                label = labelGo.AddComponent<TextMesh>();
                label.fontSize = 72;
                label.characterSize = 1f;
                label.fontStyle = FontStyle.Bold;
                label.anchor = TextAnchor.MiddleCenter;
                label.alignment = TextAlignment.Center;
                var mr = labelGo.GetComponent<MeshRenderer>();
                if (mr != null) mr.sortingOrder = 52;
            }

            Transform shadowT = root.Find("ForceLabelShadow");
            TextMesh shadow = shadowT != null ? shadowT.GetComponent<TextMesh>() : null;
            if (shadow == null) {
                var shadowGo = new GameObject("ForceLabelShadow");
                shadowGo.transform.SetParent(root, false);
                shadow = shadowGo.AddComponent<TextMesh>();
                shadow.fontSize = 72;
                shadow.characterSize = 1f;
                shadow.fontStyle = FontStyle.Bold;
                shadow.anchor = TextAnchor.MiddleCenter;
                shadow.alignment = TextAlignment.Center;
                shadow.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
                var mrShadow = shadowGo.GetComponent<MeshRenderer>();
                if (mrShadow != null) mrShadow.sortingOrder = 51;
            }

            Vector3 textPos = tail - direction * (0.05f * scale);
            label.text = caption;
            label.color = color;
            label.transform.position = textPos;
            if (cam != null) label.transform.rotation = cam.transform.rotation;
            SetWorldScale(label.transform, Vector3.one * (unit * 7.5f));

            if (shadow != null) {
                shadow.text = caption;
                Vector3 shadowOffset = (cam != null ? (cam.transform.right - cam.transform.up) : (leverRoot.right - leverRoot.up)) * (unit * 1.6f);
                shadow.transform.position = textPos + shadowOffset;
                if (cam != null) shadow.transform.rotation = cam.transform.rotation;
                SetWorldScale(shadow.transform, Vector3.one * (unit * 7.5f));
            }
        }

        private void HandleDirect3DPartClicks()
        {
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed) return;
            if (currentMode != InteractionMode.ViewOnly || currentViewOnlySubMode != ViewOnlySubMode.ExplodedParts) return;

            // If the character dialogue panel is open, do not raycast 3D objects behind it
            if (explodedDialoguePanel != null && explodedDialoguePanel.activeSelf) return;

            bool pointerDown = Input.GetMouseButtonDown(0);
            Vector2 screenPos = Input.mousePosition;

            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    pointerDown = true;
                    screenPos = t.position;
                }
            }

            if (pointerDown)
            {
                if (EventSystem.current != null)
                {
                    if (EventSystem.current.IsPointerOverGameObject()) return;
                    if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
                }

                Camera cam = Camera.main;
                if (cam == null) cam = FindFirstObjectByType<Camera>();
                if (cam == null) return;

                Ray ray = cam.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    Transform hitTr = hit.transform;
                    if (boulderRoot != null && (hitTr == boulderRoot || hitTr.IsChildOf(boulderRoot)))
                    {
                        ShowPartDetails("load");
                    }
                    else if (fulcrumRoot != null && (hitTr == fulcrumRoot || hitTr.IsChildOf(fulcrumRoot)))
                    {
                        ShowPartDetails("fulcrum");
                    }
                    else if (effortPoint != null && (hitTr == effortPoint || hitTr.IsChildOf(effortPoint)))
                    {
                        ShowPartDetails("effort");
                    }
                    else if (beamPivot != null && (hitTr == beamPivot || hitTr.IsChildOf(beamPivot)))
                    {
                        ShowPartDetails("beam");
                    }
                }
            }
        }

        public void SetInteractionMode(InteractionMode mode)
        {
            string activeScene = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            bool isSandbox = (activeScene == "FreePracticeScene" || appMode == "Sandbox");

            if (!isSandbox)
            {
                mode = StudyPolicy.Resolve(mode);
            }
            var previous = currentMode;
            currentMode = mode;
            UpdateUI();
            if (leverRoot != null) leverRoot.GetComponent<ARMechanismViewer>()?.SetInteractionMode(mode);
            if (previous != mode) CognitiveTelemetryLogger.Instance?.LogModeSwitch(previous, mode);
        }

        public void ToggleInteractionMode()
        {
            PlaySound(clickClip);
            SetInteractionMode(currentMode == InteractionMode.Interactive ? InteractionMode.ViewOnly : InteractionMode.Interactive);
        }

        public void SetViewOnlySubMode(ViewOnlySubMode subMode)
        {
            if (StudyPolicy.Passive) subMode = ViewOnlySubMode.ForcesAndMotion;
            PlaySound(clickClip);
            currentViewOnlySubMode = subMode;

            if (subMode == ViewOnlySubMode.ForcesAndMotion)
            {
                isExplodedView = false;
                isAutoRotating = true;
                if (explodedPartsPillsContainer != null) explodedPartsPillsContainer.SetActive(false);

                if (imgForcesHighlight != null) imgForcesHighlight.color = new Color(1f, 0.88f, 0.2f, 1f);
                if (imgExplodedHighlight != null) imgExplodedHighlight.color = new Color(0.4f, 0.35f, 0.3f, 0.7f);

                if (liftStatusBannerText != null)
                {
                    liftStatusBannerText.text = "<color=#ffdd44><b>🔄 360° FORCES & ROTATION ACTIVE</b></color>\n<size=17>Glowing arrows indicate input effort, gravity load, and fulcrum reaction force!</size>";
                }
            }
            else if (subMode == ViewOnlySubMode.ExplodedParts)
            {
                isExplodedView = true;
                isAutoRotating = false;
                if (explodedPartsPillsContainer != null) explodedPartsPillsContainer.SetActive(false);

                if (imgForcesHighlight != null) imgForcesHighlight.color = new Color(0.4f, 0.35f, 0.3f, 0.7f);
                if (imgExplodedHighlight != null) imgExplodedHighlight.color = new Color(0.2f, 0.9f, 1f, 1f);

                if (liftStatusBannerText != null)
                {
                    liftStatusBannerText.text = "<color=#00e5ff><b>🔍 EXPLODED VIEW: ANATOMY OF A LEVER</b></color>\n<size=17>Tap any separated component below or in 3D to learn its physics role & temple lore!</size>";
                }
            }
        }

        private void EnsureExploded3DColliders()
        {
            if (boulderRoot != null)
            {
                var col = boulderRoot.GetComponent<Collider>();
                if (col == null)
                {
                    var sc = boulderRoot.gameObject.AddComponent<SphereCollider>();
                    sc.radius = 0.45f;
                }
            }

            if (fulcrumRoot != null)
            {
                var col = fulcrumRoot.GetComponent<Collider>();
                if (col == null)
                {
                    var bc = fulcrumRoot.gameObject.AddComponent<BoxCollider>();
                    bc.center = new Vector3(0f, 0.25f, 0f);
                    bc.size = new Vector3(0.65f, 0.65f, 0.65f);
                }
            }

            if (beamPivot != null)
            {
                var col = beamPivot.GetComponent<Collider>();
                if (col == null)
                {
                    var bc = beamPivot.gameObject.AddComponent<BoxCollider>();
                    bc.center = new Vector3(0f, 0f, 0f);
                    bc.size = new Vector3(3.2f, 0.25f, 0.45f);
                }
            }
        }

        private void EnsureExplodedCharacterBubbleBuilt(Transform uiRoot)
        {
            if (uiRoot == null) return;
            if (explodedDialoguePanel != null) return;

            var existing = uiRoot.Find("Panel_ExplodedCharacterDialogue");
            if (existing != null)
            {
                explodedDialoguePanel = existing.gameObject;
                explodedCharAvatarImage = explodedDialoguePanel.transform.Find("Avatar_Character")?.GetComponent<Image>();
                explodedSpeakerNameText = explodedDialoguePanel.transform.Find("DialogueBubble/HeaderPlate/SpeakerText")?.GetComponent<Text>();
                explodedDialogueBodyText = explodedDialoguePanel.transform.Find("DialogueBubble/BodyText")?.GetComponent<Text>();
                explodedCloseButton = explodedDialoguePanel.transform.Find("DialogueBubble/Btn_Close")?.GetComponent<Button>();
                return;
            }

            Font font = Resources.Load<Font>("Montserrat-Bold");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Root Panel (Transparent dim backdrop that intercepts clicks)
            explodedDialoguePanel = new GameObject("Panel_ExplodedCharacterDialogue", typeof(RectTransform), typeof(Image), typeof(Button));
            explodedDialoguePanel.transform.SetParent(uiRoot, false);
            var panelRt = (RectTransform)explodedDialoguePanel.transform;
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var bgImg = explodedDialoguePanel.GetComponent<Image>();
            bgImg.color = new Color(0.02f, 0.03f, 0.05f, 0.45f);
            var bgBtn = explodedDialoguePanel.GetComponent<Button>();
            bgBtn.onClick.AddListener(CloseExplodedCharacterDialogue);

            // 2. Character Avatar on the left side
            var avatarGo = new GameObject("Avatar_Character", typeof(RectTransform), typeof(Image));
            avatarGo.transform.SetParent(explodedDialoguePanel.transform, false);
            var avatarRt = (RectTransform)avatarGo.transform;
            avatarRt.anchorMin = new Vector2(0f, 0f);
            avatarRt.anchorMax = new Vector2(0f, 0f);
            avatarRt.pivot = new Vector2(0f, 0f);
            avatarRt.anchoredPosition = new Vector2(30f, 0f);
            avatarRt.sizeDelta = new Vector2(460f, 680f);

            explodedCharAvatarImage = avatarGo.GetComponent<Image>();
            explodedCharAvatarImage.preserveAspect = true;
            explodedCharAvatarImage.raycastTarget = false;

            // 3. Dialogue Bubble
            var bubbleGo = new GameObject("DialogueBubble", typeof(RectTransform), typeof(Image));
            bubbleGo.transform.SetParent(explodedDialoguePanel.transform, false);
            var bubbleRt = (RectTransform)bubbleGo.transform;
            bubbleRt.anchorMin = new Vector2(0f, 0f);
            bubbleRt.anchorMax = new Vector2(1f, 0f);
            bubbleRt.pivot = new Vector2(0.5f, 0f);
            bubbleRt.offsetMin = new Vector2(460f, 35f);
            bubbleRt.offsetMax = new Vector2(-40f, 380f);

            var bubbleImg = bubbleGo.GetComponent<Image>();
            var bubbleSprite = Resources.Load<Sprite>("dialog_bubble_frame");
            if (bubbleSprite != null)
            {
                bubbleImg.sprite = bubbleSprite;
                bubbleImg.type = Image.Type.Sliced;
                bubbleImg.color = Color.white;
            }
            else
            {
                bubbleImg.color = new Color(0.12f, 0.09f, 0.05f, 0.95f);
            }

            var bubbleOutline = bubbleGo.AddComponent<Outline>();
            bubbleOutline.effectColor = new Color(0.95f, 0.78f, 0.28f, 0.9f);
            bubbleOutline.effectDistance = new Vector2(3f, -3f);

            // 4. Header / Speaker Tag Plate
            var headerGo = new GameObject("HeaderPlate", typeof(RectTransform), typeof(Image));
            headerGo.transform.SetParent(bubbleGo.transform, false);
            var headerRt = (RectTransform)headerGo.transform;
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(0f, 1f);
            headerRt.pivot = new Vector2(0f, 1f);
            headerRt.anchoredPosition = new Vector2(25f, -14f);
            headerRt.sizeDelta = new Vector2(420f, 48f);

            var headerImg = headerGo.GetComponent<Image>();
            headerImg.color = new Color(0.85f, 0.62f, 0.16f, 0.95f);
            var headerOutline = headerGo.AddComponent<Outline>();
            headerOutline.effectColor = new Color(0.3f, 0.15f, 0.02f, 0.8f);
            headerOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var nameTxtGo = new GameObject("SpeakerText", typeof(RectTransform), typeof(Text));
            nameTxtGo.transform.SetParent(headerGo.transform, false);
            var nameTxtRt = (RectTransform)nameTxtGo.transform;
            nameTxtRt.anchorMin = Vector2.zero;
            nameTxtRt.anchorMax = Vector2.one;
            nameTxtRt.offsetMin = new Vector2(15f, 2f);
            nameTxtRt.offsetMax = new Vector2(-15f, -2f);

            explodedSpeakerNameText = nameTxtGo.GetComponent<Text>();
            explodedSpeakerNameText.font = font;
            explodedSpeakerNameText.fontSize = 20;
            explodedSpeakerNameText.fontStyle = FontStyle.Bold;
            explodedSpeakerNameText.alignment = TextAnchor.MiddleLeft;
            explodedSpeakerNameText.color = new Color(0.12f, 0.07f, 0.01f, 1f);

            // 5. Dialogue Body Text
            var bodyGo = new GameObject("BodyText", typeof(RectTransform), typeof(Text));
            bodyGo.transform.SetParent(bubbleGo.transform, false);
            var bodyRt = (RectTransform)bodyGo.transform;
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(35f, 75f);
            bodyRt.offsetMax = new Vector2(-35f, -65f);

            explodedDialogueBodyText = bodyGo.GetComponent<Text>();
            explodedDialogueBodyText.font = font;
            explodedDialogueBodyText.fontSize = 23;
            explodedDialogueBodyText.fontStyle = FontStyle.Normal;
            explodedDialogueBodyText.alignment = TextAnchor.UpperLeft;
            explodedDialogueBodyText.lineSpacing = 1.25f;
            explodedDialogueBodyText.supportRichText = true;
            explodedDialogueBodyText.color = new Color(1f, 0.97f, 0.90f, 1f);

            // 6. "GOT IT! 👍" Button
            var closeBtnGo = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGo.transform.SetParent(bubbleGo.transform, false);
            var closeRt = (RectTransform)closeBtnGo.transform;
            closeRt.anchorMin = new Vector2(1f, 0f);
            closeRt.anchorMax = new Vector2(1f, 0f);
            closeRt.pivot = new Vector2(1f, 0f);
            closeRt.anchoredPosition = new Vector2(-25f, 16f);
            closeRt.sizeDelta = new Vector2(210f, 52f);

            var closeBtnImg = closeBtnGo.GetComponent<Image>();
            closeBtnImg.color = new Color(0.88f, 0.65f, 0.18f, 1f);
            var closeBtnOutline = closeBtnGo.AddComponent<Outline>();
            closeBtnOutline.effectColor = new Color(0.35f, 0.18f, 0.02f, 0.9f);
            closeBtnOutline.effectDistance = new Vector2(2f, -2f);

            explodedCloseButton = closeBtnGo.GetComponent<Button>();
            explodedCloseButton.onClick.AddListener(CloseExplodedCharacterDialogue);

            var closeTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            closeTxtGo.transform.SetParent(closeBtnGo.transform, false);
            var closeTxtRt = (RectTransform)closeTxtGo.transform;
            closeTxtRt.anchorMin = Vector2.zero;
            closeTxtRt.anchorMax = Vector2.one;
            closeTxtRt.offsetMin = closeTxtRt.offsetMax = Vector2.zero;

            var closeTxt = closeTxtGo.GetComponent<Text>();
            closeTxt.font = font;
            closeTxt.fontSize = 20;
            closeTxt.fontStyle = FontStyle.Bold;
            closeTxt.alignment = TextAnchor.MiddleCenter;
            closeTxt.color = new Color(0.12f, 0.07f, 0.01f, 1f);
            closeTxt.text = "GOT IT! 👍";

            explodedDialoguePanel.SetActive(false);
        }

        public void CloseExplodedCharacterDialogue()
        {
            PlaySound(clickClip);
            if (explodedDialoguePanel != null)
            {
                explodedDialoguePanel.SetActive(false);
            }
        }

        public void ShowPartDetails(string partKey)
        {
            PlaySound(partInspectClip != null ? partInspectClip : clickClip);

            if (partDetailModalPanel != null) partDetailModalPanel.SetActive(false);

            Transform canvasTr = null;
            var arMainCanvas = GameObject.Find("AR Main Canvas");
            if (arMainCanvas != null) canvasTr = arMainCanvas.transform;
            else
            {
                var anyCanvas = FindFirstObjectByType<Canvas>();
                if (anyCanvas != null) canvasTr = anyCanvas.transform;
            }

            EnsureExplodedCharacterBubbleBuilt(canvasTr);
            if (explodedDialoguePanel == null) return;

            string charKey = ResearchProgress.GetString("SelectedCharacter", "Maya");
            if (string.IsNullOrEmpty(charKey)) charKey = PlayerPrefs.GetString("LostTemple_Selected_Character", "Maya");
            if (string.IsNullOrEmpty(charKey)) charKey = "Maya";

            if (mayaFullSprite == null) mayaFullSprite = Resources.Load<Sprite>("char_maya_full");
            if (senukFullSprite == null) senukFullSprite = Resources.Load<Sprite>("char_senuk_full");

            if (explodedCharAvatarImage != null)
            {
                explodedCharAvatarImage.sprite = (charKey == "Senuk" && senukFullSprite != null) ? senukFullSprite : mayaFullSprite;
            }

            string speakerTitle = (charKey == "Senuk") ? "👤 Senuk (Temple Explorer)" : "👤 Maya (Explorer Guide)";
            if (explodedSpeakerNameText != null)
            {
                explodedSpeakerNameText.text = speakerTitle;
            }

            string explanation = "";
            switch (partKey.ToLower())
            {
                case "load":
                case "boulder":
                    explanation = "<size=28><color=#ffd700><b>🪨 THE LOAD STONE (100 N)</b></color></size>\n\n" +
                                  "This heavy rock is the <b>Load</b>! In a lever, the Load is the heavy weight you want to lift. Gravity pulls it down with 100 Newtons of force, so we use leverage to raise it up!";
                    HighlightConcept("load");
                    break;

                case "fulcrum":
                    explanation = "<size=28><color=#ffd700><b>🔺 THE FULCRUM (PIVOT)</b></color></size>\n\n" +
                                  "This triangular stone is the <b>Fulcrum</b>! It is the stationary support point that balances the entire beam. Moving the fulcrum closer to the heavy rock multiplies your lifting strength!";
                    HighlightConcept("fulcrum");
                    break;

                case "beam":
                    explanation = "<size=28><color=#ffd700><b>🪵 THE LEVER BEAM</b></color></size>\n\n" +
                                  "This long wooden bar is the <b>Lever Beam</b>! It balances across the fulcrum. When you push down on one side, the beam transmits that turning force to lift the stone on the other side!";
                    HighlightConcept("beam");
                    break;

                case "effort":
                    explanation = "<size=28><color=#ffd700><b>⚡ THE EFFORT POINT</b></color></size>\n\n" +
                                  "This is the <b>Effort Point</b> where you push down! The farther this handle is from the fulcrum, the less muscle force you need to lift the heavy stone!";
                    HighlightConcept("effort");
                    break;
            }

            if (explodedDialogueBodyText != null)
            {
                explodedDialogueBodyText.text = explanation;
            }

            explodedDialoguePanel.SetActive(true);
            explodedDialoguePanel.transform.SetAsLastSibling();

            CognitiveTelemetryLogger.Instance?.LogInteraction("inspect_exploded_3d_part", null, $"{{\"part\":\"{partKey}\",\"character\":\"{charKey}\"}}");
            ResearchCloudSync.Instance?.TriggerImmediateSync($"Exploded 3D Inspect: {partKey}");
        }

        public void ClosePartDetailsModal()
        {
            PlaySound(clickClip);
            if (partDetailModalPanel != null)
            {
                partDetailModalPanel.SetActive(false);
            }
        }

        private void UpdateUI()
        {
            AutoBindAllReferences();
            bool isInteractive = currentMode == InteractionMode.Interactive;

            // 1. Toggle Mode Button Art & Text
            if (modeToggleBg != null)
            {
                if (isInteractive)
                {
                    if (interactiveModeSprite != null)
                    {
                        modeToggleBg.sprite = interactiveModeSprite;
                        modeToggleBg.color = Color.white;
                        modeToggleBg.preserveAspect = true;
                    }
                    else
                    {
                        modeToggleBg.color = new Color(0.2f, 0.65f, 0.3f, 0.95f);
                    }
                }
                else
                {
                    if (viewOnlyModeSprite != null)
                    {
                        modeToggleBg.sprite = viewOnlyModeSprite;
                        modeToggleBg.color = Color.white;
                        modeToggleBg.preserveAspect = true;
                    }
                    else
                    {
                        modeToggleBg.color = new Color(0.2f, 0.45f, 0.7f, 0.95f);
                    }
                }
            }

            if (modeToggleText != null)
            {
                bool hasSprite = isInteractive ? (interactiveModeSprite != null) : (viewOnlyModeSprite != null);
                modeToggleText.text = hasSprite ? "" : (isInteractive ? "INTERACTIVE MODE" : "VIEW ONLY MODE");
            }

            // 2. Control Bottom Console vs View-Only Sub-Bar Visibility
            if (bottomConsole != null)
            {
                bottomConsole.SetActive(isInteractive && !isGuidedStoryFlowActive); // HIDE IN VIEW-ONLY OR GUIDED STORY!
            }

            if (viewOnlyBottomBar != null)
            {
                viewOnlyBottomBar.SetActive(!isInteractive); // SHOW IN VIEW-ONLY!
            }

            // 3. Update rotation and submode
            if (isInteractive)
            {
                isAutoRotating = false;
                isExplodedView = false;
                if (explodedPartsPillsContainer != null) explodedPartsPillsContainer.SetActive(false);
            }
            else
            {
                SetViewOnlySubMode(currentViewOnlySubMode);
            }

            UpdateForceUI();
            CalculatePhysicsAndAnimate();
            ApplyAppModeVisibility();
        }

        public void AdjustForce(float deltaN)
        {
            string activeScene = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            bool isSandbox = (activeScene == "FreePracticeScene" || appMode == "Sandbox" || activeScene == "Physics3DLabScene");
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed && !isSandbox) return;
            hasUserInteractedWithPhysics = true;
            PlaySound(forceChangeClip != null ? forceChangeClip : clickClip);
            currentEffortForceN = Mathf.Clamp(currentEffortForceN + deltaN, 0f, 160f);
            forceChangeCount++;

            CalculatePhysicsAndAnimate();
            UpdateForceUI();

            CognitiveTelemetryLogger.Instance?.LogInteraction("adjust_force", null, $"{{\"forceN\":{currentEffortForceN}}}");
            ResearchCloudSync.Instance?.TriggerImmediateSync($"Effort Force set to {currentEffortForceN:F0}N");
        }

        public void MoveFulcrumStep(float delta)
        {
            string activeScene = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            bool isSandbox = (activeScene == "FreePracticeScene" || appMode == "Sandbox" || activeScene == "Physics3DLabScene");
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed && !isSandbox) return;
            hasUserInteractedWithPhysics = true;
            PlaySound(clickClip);
            fulcrumOffset = Mathf.Clamp(fulcrumOffset + delta, -0.45f, 0.45f);
            fulcrumChangeCount++;

            if (fulcrumSlider != null) fulcrumSlider.value = fulcrumOffset;
            if (fulcrumRoot != null) fulcrumRoot.localPosition = new Vector3(fulcrumOffset, 0f, 0f);

            CalculatePhysicsAndAnimate();
            pendingFulcrumLog = Time.unscaledTime;
        }

        private void OnFulcrumSliderChanged(float val)
        {
            string activeScene = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            bool isSandbox = (activeScene == "FreePracticeScene" || appMode == "Sandbox" || activeScene == "Physics3DLabScene");
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed && !isSandbox) return;
            hasUserInteractedWithPhysics = true;
            fulcrumOffset = Mathf.Clamp(val, -0.45f, 0.45f);
            fulcrumChangeCount++;

            if (fulcrumRoot != null)
            {
                fulcrumRoot.localPosition = new Vector3(fulcrumOffset, 0f, 0f);
            }

            CalculatePhysicsAndAnimate();
            pendingFulcrumLog = Time.unscaledTime;
        }

        private void CalculatePhysicsAndAnimate()
        {
            float fulcrumPos = fulcrumOffset;
            float loadArm = Mathf.Abs(-0.90f - fulcrumPos);
            float effortArm = Mathf.Abs(0.90f - fulcrumPos);

            loadArm = Mathf.Max(0.1f, loadArm);
            effortArm = Mathf.Max(0.1f, effortArm);

            float mechanicalAdvantage = effortArm / loadArm;
            float loadWeightN = 100f;
            float requiredForceN = loadWeightN / mechanicalAdvantage;

            float loadTorque = loadWeightN * loadArm;
            float effortTorque = currentEffortForceN * effortArm;
            float netTorque = effortTorque - loadTorque;

            string charName = string.IsNullOrEmpty(activeCharacter) ? "EXPLORER" : activeCharacter.ToUpper();

            // Calculate natural resting down angle (boulder touching ground/pedestal) and lifted up angle
            float restingAngle = Mathf.Clamp(Mathf.Asin(Mathf.Clamp(0.33f / loadArm, 0.1f, 0.55f)) * Mathf.Rad2Deg, 14f, 25f);
            float liftedAngle = -Mathf.Clamp(Mathf.Asin(Mathf.Clamp(0.28f / effortArm, 0.1f, 0.45f)) * Mathf.Rad2Deg, 10f, 18f);

            if (currentMode == InteractionMode.Interactive)
            {
                if (netTorque > 5f)
                {
                    targetBeamAngle = liftedAngle;
                    string msg = $"<color=#00ff88><b>🎉 {charName}: \"100 N BOULDER LIFTED!\"</b></color>\n<size=18>Required: {Mathf.CeilToInt(requiredForceN)} N | Applied: {Mathf.RoundToInt(currentEffortForceN)} N | MA: {mechanicalAdvantage:F1}x</size>";
                    if (liftStatusBannerText != null) liftStatusBannerText.text = msg;

                    string activeScene = SceneManager.GetActiveScene().name;
                    string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
                    bool isQuest = (activeScene == "MainARScene" && appMode != "Sandbox" && appMode != "PhysicsLab");

                    // In Quest/Expedition mode, as soon as the student lifts the boulder (especially after adjusting Fulcrum/Force),
                    // unlock Story progression and swap "TRY CHALLENGE" to "CONTINUE STORY ➡" on the bottom console!
                    if (isQuest && hasUserInteractedWithPhysics && !challengeResolved)
                    {
                        GetChallengeParametersForTier(out int cTier, out float cLoadN, out float cMaxEffortN, out string cTierLabel);
                        // Solve if within challenge effort limit OR if they haven't opened the modal and successfully lifted the boulder by moving fulcrum/force
                        if (!isChallengeActive || currentEffortForceN <= cMaxEffortN)
                        {
                            UnlockQuestVictoryAndShowContinueStory(cTier, mechanicalAdvantage, cLoadN);
                        }
                    }

                    if (lastVoiceState != 0)
                    {
                        lastVoiceState = 0;
                        if (hasUserInteractedWithPhysics)
                        {
                            PlayCompanionReaction(0, msg);
                        }

                        if (isQuest)
                        {
                            int currentUnlocked = ResearchProgress.GetInt("LostTemple_Quest_Unlocked_Level", 1);
                            if (currentUnlocked < 2)
                            {
                                ResearchProgress.SetInt("LostTemple_Quest_Unlocked_Level", 2);
                                PlayerPrefs.Save();
                                if (celebrationParticles != null) celebrationParticles.Play();
                                ResearchData.Record("lever_lifted", "lever");
                            }
                        }
                    }
                    else if (hasUserInteractedWithPhysics)
                    {
                        ShowCompanionAndBubbleTemporary(4.5f);
                    }
                }
                else if (netTorque < -5f)
                {
                    targetBeamAngle = restingAngle;
                    string msg = $"<color=#ffaa44><b>⚠️ {charName}: \"TOO HEAVY TO LIFT!\"</b></color>\n<size=18>Need at least <b>{Mathf.CeilToInt(requiredForceN)} N</b> with this Fulcrum position!</size>";
                    if (liftStatusBannerText != null) liftStatusBannerText.text = msg;

                    if (lastVoiceState != 1)
                    {
                        lastVoiceState = 1;
                        if (hasUserInteractedWithPhysics)
                        {
                            PlayCompanionReaction(1, msg);
                        }
                    }
                    else if (hasUserInteractedWithPhysics)
                    {
                        ShowCompanionAndBubbleTemporary(4.5f);
                    }
                }
                else
                {
                    targetBeamAngle = 0f;
                    string msg = $"<color=#ffdd44><b>⚖️ {charName}: \"BALANCED EQUILIBRIUM!\"</b></color>\n<size=18>Effort matches Load Torque exactly ({Mathf.RoundToInt(requiredForceN)} N)</size>";
                    if (liftStatusBannerText != null) liftStatusBannerText.text = msg;

                    if (lastVoiceState != 2)
                    {
                        lastVoiceState = 2;
                        if (hasUserInteractedWithPhysics)
                        {
                            PlayCompanionReaction(2, msg);
                        }
                    }
                    else if (hasUserInteractedWithPhysics)
                    {
                        ShowCompanionAndBubbleTemporary(4.5f);
                    }
                }
            }

            if (fulcrumStatusText != null)
            {
                fulcrumStatusText.text = $"<b>{fulcrumOffset:+0.00;-0.00;0.00} m</b>";
            }

            // Live challenge progress helper so the student knows immediately when their Fulcrum & Force settings solve the mission
            if (isChallengeActive && !challengeResolved && challengeFeedbackText != null)
            {
                GetChallengeParametersForTier(out int cTier, out float cLoadN, out float cMaxEffortN, out string cTierLabel);
                float cLoadTorque = cLoadN * loadArm;
                float cEffortTorque = currentEffortForceN * effortArm;
                if (cEffortTorque > cLoadTorque + 5f && currentEffortForceN <= cMaxEffortN)
                {
                    challengeFeedbackText.text =
                        $"<color=#00ff88>✨ <b>Target Reached!</b> Boulder lifted with <b>{currentEffortForceN:F0} N</b> (MA: <b>{mechanicalAdvantage:F1}x</b>) — Tap <b>✅ CHECK SOLUTION</b> to complete!</color>";
                }
            }
        }

        /// <summary>
        /// Requirement 2: Shows Maya/Senuk companion avatar and speech banner simultaneously,
        /// plays voice reaction, and hides them after ~4.5 seconds when idle.
        /// </summary>
        public void PlayCompanionReaction(int voiceState, string feedbackMessage = null)
        {
            if (!string.IsNullOrEmpty(feedbackMessage) && liftStatusBannerText != null)
            {
                liftStatusBannerText.text = feedbackMessage;
            }
            PlayCharacterVoice(voiceState);
            ShowCompanionAndBubbleTemporary(4.5f);
        }

        public void ShowCompanionHint()
        {
            ShowCompanionAndBubbleTemporary(4.5f);
        }

        private void ShowCompanionAndBubbleTemporary(float durationSeconds = 4.5f)
        {
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            if (appMode == "Sandbox") return;

            if (companionBubbleCoroutine != null)
            {
                StopCoroutine(companionBubbleCoroutine);
            }
            companionBubbleCoroutine = StartCoroutine(CompanionBubbleRoutine(durationSeconds));
        }

        private IEnumerator CompanionBubbleRoutine(float durationSeconds = 4.5f)
        {
            GameObject bannerGo = liftStatusBannerText != null && liftStatusBannerText.transform.parent != null
                ? liftStatusBannerText.transform.parent.gameObject
                : null;

            CanvasGroup compCg = null;
            if (characterCompanionImage != null)
            {
                characterCompanionImage.gameObject.SetActive(true);
                compCg = characterCompanionImage.GetComponent<CanvasGroup>();
                if (compCg == null) compCg = characterCompanionImage.gameObject.AddComponent<CanvasGroup>();
                compCg.alpha = 1f;
            }

            CanvasGroup bannerCg = null;
            if (bannerGo != null)
            {
                bannerGo.SetActive(true);
                bannerCg = bannerGo.GetComponent<CanvasGroup>();
                if (bannerCg == null) bannerCg = bannerGo.AddComponent<CanvasGroup>();
                bannerCg.alpha = 1f;
            }

            yield return new WaitForSeconds(durationSeconds);

            float fadeDuration = 0.35f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                if (compCg != null) compCg.alpha = alpha;
                if (bannerCg != null) bannerCg.alpha = alpha;
                yield return null;
            }

            if (characterCompanionImage != null)
            {
                if (compCg != null) compCg.alpha = 1f;
                characterCompanionImage.gameObject.SetActive(false);
            }
            if (bannerGo != null)
            {
                if (bannerCg != null) bannerCg.alpha = 1f;
                bannerGo.SetActive(false);
            }
            companionBubbleCoroutine = null;
        }

        private void PlayCharacterVoice(int state)
        {
            if (audioSource == null) return;
            AudioClip clipToPlay = null;

            if (activeCharacter == "Senuk")
            {
                if (state == 0) clipToPlay = senukBoulderLiftedClip;
                else if (state == 1) clipToPlay = senukTooHeavyClip;
                else if (state == 2) clipToPlay = senukBalancedClip;
            }
            else
            {
                if (state == 0) clipToPlay = mayaBoulderLiftedClip;
                else if (state == 1) clipToPlay = mayaTooHeavyClip;
                else if (state == 2) clipToPlay = mayaBalancedClip;
            }

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(clipToPlay);
            }
        }

        private void UpdateForceUI()
        {
            if (forceValueText != null)
            {
                forceValueText.text = $"{Mathf.RoundToInt(currentEffortForceN)} N";
            }
        }

        public void OnRotateClicked()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            bool isSandbox = (activeScene == "FreePracticeScene" || appMode == "Sandbox" || activeScene == "Physics3DLabScene");
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed && !isSandbox) return;
            if (leverRoot != null) leverRoot.GetComponent<ARMechanismViewer>()?.RotateBy(30f);
        }

        public void ResetMechanism()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            bool isSandbox = (activeScene == "FreePracticeScene" || appMode == "Sandbox" || activeScene == "Physics3DLabScene");
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed && !isSandbox) return;
            PlaySound(clickClip);
            if (boulderRollCoroutine != null)
            {
                StopCoroutine(boulderRollCoroutine);
                boulderRollCoroutine = null;
            }

            currentEffortForceN = 60f;
            fulcrumOffset = 0f;
            isAutoRotating = (currentMode == InteractionMode.ViewOnly && currentViewOnlySubMode == ViewOnlySubMode.ForcesAndMotion);
            isExplodedView = (currentMode == InteractionMode.ViewOnly && currentViewOnlySubMode == ViewOnlySubMode.ExplodedParts);

            if (fulcrumSlider != null) fulcrumSlider.value = 0f;
            if (fulcrumRoot != null) fulcrumRoot.localPosition = fulcrumAssembledLocalPos;
            if (beamPivot != null)
            {
                beamPivot.localRotation = Quaternion.identity;
                beamPivot.localPosition = beamAssembledLocalPos;
            }
            if (boulderRoot != null)
            {
                boulderRoot.localPosition = boulderAssembledLocalPos;
                boulderRoot.localRotation = Quaternion.identity;
                var boulderMesh = boulderRoot.Find("Model_Instance_Boulder");
                if (boulderMesh != null) boulderMesh.localPosition = new Vector3(0f, 0.18f, 0f);
            }
            if (effortPoint != null) effortPoint.localPosition = effortAssembledLocalPos;

            if (leverRoot != null)
            {
                string mode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
                if (mode == "Sandbox")
                {
                    GameObject envRoot = GameObject.Find("TempleEnvironmentRoot");
                    if (envRoot != null && leverRoot.parent != envRoot.transform)
                    {
                        leverRoot.SetParent(envRoot.transform, false);
                    }
                    leverRoot.localPosition = new Vector3(0f, -0.20f, 1.20f);
                    leverRoot.localRotation = Quaternion.identity;
                    leverRoot.localScale = Vector3.one;
                }
            }

            UpdateForceUI();
            CalculatePhysicsAndAnimate();

            // Set natural resting angle immediately
            currentBeamAngle = targetBeamAngle;
            if (beamPivot != null && !isExplodedView)
            {
                beamPivot.localRotation = Quaternion.Euler(0f, 0f, currentBeamAngle);
            }

            CognitiveTelemetryLogger.Instance?.LogInteraction("reset_mechanism", null, "{\"reset\":true}");
        }

        public void ToggleHowItWorks()
        {
            PlaySound(clickClip);
            if (howItWorksPanel != null)
            {
                howItWorksPanel.SetActive(!howItWorksPanel.activeSelf);
            }
        }

        private void EnsureHowItWorksToggleButtonBuilt(Transform uiRoot)
        {
            if (uiRoot == null) return;
            var topBar = uiRoot.Find("TopBar");
            if (topBar != null)
            {
                var existingBtn = topBar.Find("Btn_HowItWorks");
                if (existingBtn != null) existingBtn.gameObject.SetActive(false);
            }
            if (toggleHowItWorksBtn != null) toggleHowItWorksBtn.gameObject.SetActive(false);
            if (howItWorksPanel != null) howItWorksPanel.SetActive(true);
            return;
            var existing = topBar != null ? topBar.Find("Btn_HowItWorks") : null;
            if (existing != null)
            {
                toggleHowItWorksBtn = existing.GetComponent<Button>();
                var existingRt = existing.GetComponent<RectTransform>();
                if (existingRt != null)
                {
                    existingRt.anchoredPosition = new Vector2(-390f, 0f);
                }
                return;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var btnGo = new GameObject("Btn_HowItWorks", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(topBar, false);
            var rt = (RectTransform)btnGo.transform;
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-390f, 0f);
            rt.sizeDelta = new Vector2(190f, 48f);

            var img = btnGo.GetComponent<Image>();
            img.color = new Color(0.24f, 0.17f, 0.08f, 0.94f);
            var outline = btnGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.78f, 0.25f, 0.9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            toggleHowItWorksBtn = btnGo.GetComponent<Button>();
            toggleHowItWorksBtn.onClick.AddListener(ToggleHowItWorks);

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRt = (RectTransform)txtGo.transform;
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;
            var txt = txtGo.GetComponent<Text>();
            txt.font = font;
            txt.fontSize = 15;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(1f, 0.92f, 0.65f);
            txt.text = "📘 HOW IT WORKS";
        }

        private void EnsureContinueStoryVictoryButtonBuilt()
        {
            if (continueStoryVictoryButton != null || bottomConsole == null) return;

            var existing = bottomConsole.transform.Find("Btn_ContinueStory");
            if (existing != null)
            {
                continueStoryVictoryButton = existing.GetComponent<Button>();
                return;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var btnGo = new GameObject("Btn_ContinueStory", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(bottomConsole.transform, false);
            var rt = (RectTransform)btnGo.transform;

            if (tryChallengeButton != null)
            {
                var tryRt = (RectTransform)tryChallengeButton.transform;
                rt.anchorMin = tryRt.anchorMin;
                rt.anchorMax = tryRt.anchorMax;
                rt.pivot = tryRt.pivot;
                rt.anchoredPosition = tryRt.anchoredPosition;
                rt.sizeDelta = new Vector2(Mathf.Min(tryRt.sizeDelta.x, 340f), Mathf.Min(tryRt.sizeDelta.y, 110f));
            }
            else
            {
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-190f, 0f);
                rt.sizeDelta = new Vector2(340f, 100f);
            }

            var img = btnGo.GetComponent<Image>();
            img.color = new Color(0.88f, 0.62f, 0.10f, 0.98f);
            var outline = btnGo.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.94f, 0.55f, 1f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            continueStoryVictoryButton = btnGo.GetComponent<Button>();
            continueStoryVictoryButton.onClick.AddListener(OnContinueStoryVictoryClicked);

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRt = (RectTransform)txtGo.transform;
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(8f, 6f);
            txtRt.offsetMax = new Vector2(-8f, -6f);
            var txt = txtGo.GetComponent<Text>();
            txt.font = font;
            txt.fontSize = 22;
            txt.fontStyle = FontStyle.Bold;
            txt.supportRichText = true;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.14f, 0.08f, 0.02f, 1f);
            bool isStory = SecretOfLostTemple.Story.StoryBookViewerController.StoryActivityActive;
            txt.text = isStory ? "<b>CONTINUE STORY ➡</b>" : "<b>NEXT CHAPTER ➡</b>";

            btnGo.SetActive(false);
        }

        private void OnContinueStoryVictoryClicked()
        {
            PlaySound(clickClip);
            CloseChallengeModal();
            if (SecretOfLostTemple.Story.StoryBookViewerController.StoryActivityActive)
            {
                SecretOfLostTemple.Story.StoryBookViewerController.ReturnToStory(true);
            }
            else
            {
                SceneManager.LoadScene("HomeMapScene");
            }
        }

        private void UnlockQuestVictoryAndShowContinueStory(int tier, float ma, float targetLoadN, string tierLabel = "Tier 1 (Apprentice)")
        {
            challengeResolved = true;
            PlaySound(successClip);
            currentHintTierStep = 0;

            int currentUnlocked = ResearchProgress.GetInt("LostTemple_Quest_Unlocked_Level", 1);
            if (currentUnlocked < 2)
            {
                ResearchProgress.SetInt("LostTemple_Quest_Unlocked_Level", 2);
                PlayerPrefs.Save();
            }

            // Mark challenge complete to unlock StoryBook page progression past Page 9
            PlayerPrefs.SetInt("LostTemple_Lever_Challenge_Completed", 1);
            PlayerPrefs.Save();

            // Award +5 Stars for completing the temple gate challenge!
            int currentStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
            currentStars += 5;
            ResearchProgress.SetInt("LostTemple_Total_Stars", currentStars);
            PlayerPrefs.Save();

            float solveSeconds = Mathf.Max(2f, Time.time - challengeStarted);
            if (challengeAttempts == 0)
            {
                challengeAttempts = 1;
                ResearchData.Record("challenge_attempt", $"lever;tier={tier};load={targetLoadN:F0};effort={currentEffortForceN:F0};success=True", false, true, solveSeconds);
            }
            if (SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance != null)
            {
                SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance.RecordTaskResult(true, solveSeconds);
                UpdateAdaptiveTierHUD();
            }

            // Stream completion data to research dashboard
            ResearchData.Record("lever_challenge_solved", $"tier={tier};ma={ma:F2};load={targetLoadN:F0};effort={currentEffortForceN:F0}", false, true, solveSeconds);
            ResearchCloudSync.Instance?.TriggerImmediateSync("Lever Challenge Completed!");

            if (challengeFeedbackText != null)
            {
                challengeFeedbackText.text = $"<color=#00ff88><b>🎉 CHALLENGE COMPLETE ({tierLabel})! ⭐ +5 STARS EARNED! (Total: {currentStars} ⭐)</b>\nMA: <b>{ma:F1}x</b> | Lifted {targetLoadN:F0}N with {Mathf.RoundToInt(currentEffortForceN)}N! Tap <b>CONTINUE STORY ➡</b> to advance!</color>";
            }
            if (celebrationParticles != null)
            {
                celebrationParticles.Play();
            }

            // Hide tryChallengeButton and activate golden Btn_ContinueStory in its exact position on the bottom console!
            EnsureContinueStoryVictoryButtonBuilt();
            if (tryChallengeButton != null)
            {
                tryChallengeButton.gameObject.SetActive(false);
            }
            if (continueStoryVictoryButton != null)
            {
                var txt = continueStoryVictoryButton.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    bool isStory = SecretOfLostTemple.Story.StoryBookViewerController.StoryActivityActive;
                    txt.text = isStory ? "<b>CONTINUE STORY ➡</b>" : "<b>NEXT CHAPTER ➡</b>";
                }
                continueStoryVictoryButton.gameObject.SetActive(true);
            }

            // Transform submit button inside modal into "CONTINUE STORY ➡" as well
            if (challengeSubmitBtn != null)
            {
                var btnImg = challengeSubmitBtn.GetComponent<Image>();
                if (btnImg != null) btnImg.color = new Color(0.85f, 0.55f, 0.08f, 1f);
                var btnText = challengeSubmitBtn.GetComponentInChildren<Text>();
                if (btnText != null) btnText.text = "<b>CONTINUE STORY ➡</b>";
                challengeSubmitBtn.onClick.RemoveAllListeners();
                challengeSubmitBtn.onClick.AddListener(OnContinueStoryVictoryClicked);
            }

            // Boulder roll & lift animation
            if (boulderRoot != null)
            {
                if (boulderRollCoroutine != null) StopCoroutine(boulderRollCoroutine);
                boulderRollCoroutine = StartCoroutine(AnimateBoulderRoll());
            }

            string victoryMsg = $"<color=#00ff88><b>🎉 {targetLoadN:F0} N BOULDER LIFTED!</b></color>\n<size=18><b>LEVER TASK COMPLETE! Tap 'CONTINUE STORY ➡' to finish your StoryBook!</b></size>";
            PlayCompanionReaction(0, victoryMsg);
            ResearchData.Record("lever_lifted", "lever");

            CognitiveTelemetryLogger.Instance?.LogInteraction("challenge_completed", null, $"{{\"attempts\":{Mathf.Max(1, challengeAttempts)},\"ma\":{ma:F2},\"tier\":{tier}}}");
        }

        private void EnsureChallengeModalBuilt(Transform uiRoot)
        {
            if (uiRoot == null) return;

            if (challengeModalPanel == null)
            {
                var existing = uiRoot.Find("Panel_ChallengeModal");
                if (existing != null) challengeModalPanel = existing.gameObject;
            }

            if (challengeModalPanel == null)
            {
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                // Non-blocking upper Mission HUD Card so the user can simultaneously adjust Fulcrum & Force in BottomConsole
                challengeModalPanel = new GameObject("Panel_ChallengeModal", typeof(RectTransform));
                challengeModalPanel.transform.SetParent(uiRoot, false);
                var panelRt = (RectTransform)challengeModalPanel.transform;
                panelRt.anchorMin = new Vector2(0.5f, 1f);
                panelRt.anchorMax = new Vector2(0.5f, 1f);
                panelRt.pivot = new Vector2(0.5f, 1f);
                panelRt.anchoredPosition = new Vector2(0f, -92f);
                panelRt.sizeDelta = new Vector2(980f, 200f);

                var cardImg = challengeModalPanel.AddComponent<Image>();
                cardImg.color = new Color(0.08f, 0.06f, 0.03f, 0.94f);
                var cardOutline = challengeModalPanel.AddComponent<Outline>();
                cardOutline.effectColor = new Color(0.95f, 0.76f, 0.22f, 0.92f);
                cardOutline.effectDistance = new Vector2(2f, -2f);

                // Title Header
                var titleGo = new GameObject("TitleText", typeof(RectTransform));
                titleGo.transform.SetParent(challengeModalPanel.transform, false);
                var titleRt = (RectTransform)titleGo.transform;
                titleRt.anchorMin = new Vector2(0f, 1f);
                titleRt.anchorMax = new Vector2(1f, 1f);
                titleRt.pivot = new Vector2(0.5f, 1f);
                titleRt.anchoredPosition = new Vector2(-20f, -10f);
                titleRt.sizeDelta = new Vector2(-70f, 32f);
                challengeTitleText = titleGo.AddComponent<Text>();
                challengeTitleText.font = font;
                challengeTitleText.fontSize = 20;
                challengeTitleText.fontStyle = FontStyle.Bold;
                challengeTitleText.alignment = TextAnchor.MiddleLeft;
                challengeTitleText.color = new Color(1f, 0.85f, 0.28f);

                // Close Button (top-right corner)
                var closeGo = new GameObject("Btn_Close", typeof(RectTransform));
                closeGo.transform.SetParent(challengeModalPanel.transform, false);
                var closeRt = (RectTransform)closeGo.transform;
                closeRt.anchorMin = new Vector2(1f, 1f);
                closeRt.anchorMax = new Vector2(1f, 1f);
                closeRt.pivot = new Vector2(1f, 1f);
                closeRt.anchoredPosition = new Vector2(-10f, -10f);
                closeRt.sizeDelta = new Vector2(36f, 36f);
                var closeImg = closeGo.AddComponent<Image>();
                closeImg.color = new Color(0.35f, 0.14f, 0.12f, 0.95f);
                challengeCloseBtn = closeGo.AddComponent<Button>();
                challengeCloseBtn.onClick.AddListener(CloseChallengeModal);

                var closeTxtGo = new GameObject("Text", typeof(RectTransform));
                closeTxtGo.transform.SetParent(closeGo.transform, false);
                var closeTxtRt = (RectTransform)closeTxtGo.transform;
                closeTxtRt.anchorMin = Vector2.zero;
                closeTxtRt.anchorMax = Vector2.one;
                closeTxtRt.offsetMin = closeTxtRt.offsetMax = Vector2.zero;
                var closeTxt = closeTxtGo.AddComponent<Text>();
                closeTxt.font = font;
                closeTxt.fontSize = 18;
                closeTxt.fontStyle = FontStyle.Bold;
                closeTxt.alignment = TextAnchor.MiddleCenter;
                closeTxt.color = Color.white;
                closeTxt.text = "✕";

                // Mission Description Text (left column)
                var descGo = new GameObject("DescText", typeof(RectTransform));
                descGo.transform.SetParent(challengeModalPanel.transform, false);
                var descRt = (RectTransform)descGo.transform;
                descRt.anchorMin = new Vector2(0f, 1f);
                descRt.anchorMax = new Vector2(0f, 1f);
                descRt.pivot = new Vector2(0f, 1f);
                descRt.anchoredPosition = new Vector2(18f, -44f);
                descRt.sizeDelta = new Vector2(650f, 82f);
                challengeDescText = descGo.AddComponent<Text>();
                challengeDescText.font = font;
                challengeDescText.fontSize = 16;
                challengeDescText.supportRichText = true;
                challengeDescText.alignment = TextAnchor.UpperLeft;
                challengeDescText.color = new Color(0.95f, 0.92f, 0.84f);

                // Feedback Text (bottom-left column)
                var fbGo = new GameObject("FeedbackText", typeof(RectTransform));
                fbGo.transform.SetParent(challengeModalPanel.transform, false);
                var fbRt = (RectTransform)fbGo.transform;
                fbRt.anchorMin = new Vector2(0f, 0f);
                fbRt.anchorMax = new Vector2(0f, 0f);
                fbRt.pivot = new Vector2(0f, 0f);
                fbRt.anchoredPosition = new Vector2(18f, 12f);
                fbRt.sizeDelta = new Vector2(650f, 62f);
                challengeFeedbackText = fbGo.AddComponent<Text>();
                challengeFeedbackText.font = font;
                challengeFeedbackText.fontSize = 16;
                challengeFeedbackText.supportRichText = true;
                challengeFeedbackText.alignment = TextAnchor.MiddleLeft;
                challengeFeedbackText.color = new Color(0.45f, 0.95f, 1f);

                // Submit / Check Solution / Continue Story Button (right column)
                var submitGo = new GameObject("Btn_Submit", typeof(RectTransform));
                submitGo.transform.SetParent(challengeModalPanel.transform, false);
                var submitRt = (RectTransform)submitGo.transform;
                submitRt.anchorMin = new Vector2(1f, 0.5f);
                submitRt.anchorMax = new Vector2(1f, 0.5f);
                submitRt.pivot = new Vector2(1f, 0.5f);
                submitRt.anchoredPosition = new Vector2(-18f, -14f);
                submitRt.sizeDelta = new Vector2(276f, 68f);
                var submitImg = submitGo.AddComponent<Image>();
                submitImg.color = new Color(0.14f, 0.62f, 0.28f, 0.98f);
                var submitOutline = submitGo.AddComponent<Outline>();
                submitOutline.effectColor = new Color(1f, 0.86f, 0.32f, 0.95f);
                submitOutline.effectDistance = new Vector2(2f, -2f);
                challengeSubmitBtn = submitGo.AddComponent<Button>();
                challengeSubmitBtn.onClick.AddListener(EvaluateChallenge);

                var submitTxtGo = new GameObject("Text", typeof(RectTransform));
                submitTxtGo.transform.SetParent(submitGo.transform, false);
                var submitTxtRt = (RectTransform)submitTxtGo.transform;
                submitTxtRt.anchorMin = Vector2.zero;
                submitTxtRt.anchorMax = Vector2.one;
                submitTxtRt.offsetMin = submitTxtRt.offsetMax = Vector2.zero;
                var submitTxt = submitTxtGo.AddComponent<Text>();
                submitTxt.font = font;
                submitTxt.fontSize = 19;
                submitTxt.fontStyle = FontStyle.Bold;
                submitTxt.supportRichText = true;
                submitTxt.alignment = TextAnchor.MiddleCenter;
                submitTxt.color = Color.white;
                submitTxt.text = "✅ CHECK SOLUTION";

                challengeModalPanel.SetActive(false);
            }
            else
            {
                if (challengeTitleText == null) challengeTitleText = challengeModalPanel.transform.Find("TitleText")?.GetComponent<Text>();
                if (challengeDescText == null) challengeDescText = challengeModalPanel.transform.Find("DescText")?.GetComponent<Text>();
                if (challengeFeedbackText == null) challengeFeedbackText = challengeModalPanel.transform.Find("FeedbackText")?.GetComponent<Text>();
                if (challengeSubmitBtn == null) challengeSubmitBtn = challengeModalPanel.transform.Find("Btn_Submit")?.GetComponent<Button>();
                if (challengeCloseBtn == null) challengeCloseBtn = challengeModalPanel.transform.Find("Btn_Close")?.GetComponent<Button>();
            }
        }

        private void EnsureQuizModalBuilt(Transform uiRoot)
        {
            if (uiRoot == null) return;

            if (quizModalPanel == null)
            {
                var existing = uiRoot.Find("Panel_ARQuizModal");
                if (existing != null) quizModalPanel = existing.gameObject;
            }

            // If an old undersized modal card exists, recreate it with large readable proportions
            if (quizModalPanel != null)
            {
                var card = quizModalPanel.transform.Find("ModalCard");
                if (card != null && ((RectTransform)card).sizeDelta.x < 1100f)
                {
                    Destroy(quizModalPanel);
                    quizModalPanel = null;
                }
            }

            if (quizModalPanel == null)
            {
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                quizModalPanel = new GameObject("Panel_ARQuizModal", typeof(RectTransform));
                quizModalPanel.transform.SetParent(uiRoot, false);
                var panelRt = (RectTransform)quizModalPanel.transform;
                panelRt.anchorMin = Vector2.zero;
                panelRt.anchorMax = Vector2.one;
                panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;

                // Dim Background
                var dimGo = new GameObject("BackdropDim", typeof(RectTransform), typeof(Image));
                dimGo.transform.SetParent(quizModalPanel.transform, false);
                var dimRt = (RectTransform)dimGo.transform;
                dimRt.anchorMin = Vector2.zero;
                dimRt.anchorMax = Vector2.one;
                dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;
                var dimImg = dimGo.GetComponent<Image>();
                dimImg.color = new Color(0f, 0f, 0f, 0.78f);

                // Modal Card - Sized for mobile landscape readability
                var cardGo = new GameObject("ModalCard", typeof(RectTransform), typeof(Image), typeof(Outline));
                cardGo.transform.SetParent(quizModalPanel.transform, false);
                var cardRt = (RectTransform)cardGo.transform;
                cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                cardRt.pivot = new Vector2(0.5f, 0.5f);
                cardRt.anchoredPosition = Vector2.zero;
                cardRt.sizeDelta = new Vector2(1380f, 750f);

                var cardImg = cardGo.GetComponent<Image>();
                cardImg.color = new Color(0.08f, 0.12f, 0.18f, 0.98f);
                var cardOutline = cardGo.GetComponent<Outline>();
                cardOutline.effectColor = new Color(0.95f, 0.75f, 0.20f, 0.95f);
                cardOutline.effectDistance = new Vector2(3f, -3f);

                // Title (Big, bold, gold)
                var titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
                titleGo.transform.SetParent(cardGo.transform, false);
                var titleRt = (RectTransform)titleGo.transform;
                titleRt.anchorMin = new Vector2(0f, 1f);
                titleRt.anchorMax = new Vector2(1f, 1f);
                titleRt.pivot = new Vector2(0.5f, 1f);
                titleRt.anchoredPosition = new Vector2(0f, -24f);
                titleRt.sizeDelta = new Vector2(-120f, 48f);
                quizTitleText = titleGo.GetComponent<Text>();
                quizTitleText.font = font;
                quizTitleText.fontSize = 32;
                quizTitleText.fontStyle = FontStyle.Bold;
                quizTitleText.alignment = TextAnchor.MiddleCenter;
                quizTitleText.color = new Color(1f, 0.88f, 0.3f);

                // Category (Clear cyan)
                var catGo = new GameObject("CategoryText", typeof(RectTransform), typeof(Text));
                catGo.transform.SetParent(cardGo.transform, false);
                var catRt = (RectTransform)catGo.transform;
                catRt.anchorMin = new Vector2(0f, 1f);
                catRt.anchorMax = new Vector2(1f, 1f);
                catRt.pivot = new Vector2(0.5f, 1f);
                catRt.anchoredPosition = new Vector2(-60f, -72f);
                catRt.sizeDelta = new Vector2(-240f, 34f);
                quizCategoryText = catGo.GetComponent<Text>();
                quizCategoryText.font = font;
                quizCategoryText.fontSize = 22;
                quizCategoryText.alignment = TextAnchor.MiddleLeft;
                quizCategoryText.color = new Color(0.6f, 0.85f, 1f);

                // Countdown Timer Text
                var timerGo = new GameObject("TimerText", typeof(RectTransform), typeof(Text));
                timerGo.transform.SetParent(cardGo.transform, false);
                var timerRt = (RectTransform)timerGo.transform;
                timerRt.anchorMin = new Vector2(1f, 1f);
                timerRt.anchorMax = new Vector2(1f, 1f);
                timerRt.pivot = new Vector2(1f, 1f);
                timerRt.anchoredPosition = new Vector2(-60f, -70f);
                timerRt.sizeDelta = new Vector2(220f, 34f);
                quizTimerText = timerGo.GetComponent<Text>();
                quizTimerText.font = font;
                quizTimerText.fontSize = 24;
                quizTimerText.fontStyle = FontStyle.Bold;
                quizTimerText.alignment = TextAnchor.MiddleRight;
                quizTimerText.color = new Color(1f, 0.85f, 0.2f);
                quizTimerText.text = "⏱️ 30s";

                // Countdown Timer Progress Bar
                var barBgGo = new GameObject("TimerBarBg", typeof(RectTransform), typeof(Image));
                barBgGo.transform.SetParent(cardGo.transform, false);
                var barBgRt = (RectTransform)barBgGo.transform;
                barBgRt.anchorMin = new Vector2(0.5f, 1f);
                barBgRt.anchorMax = new Vector2(0.5f, 1f);
                barBgRt.pivot = new Vector2(0.5f, 1f);
                barBgRt.anchoredPosition = new Vector2(0f, -106f);
                barBgRt.sizeDelta = new Vector2(1260f, 8f);
                var barBgImg = barBgGo.GetComponent<Image>();
                barBgImg.color = new Color(0.12f, 0.16f, 0.22f, 0.9f);

                var barFillGo = new GameObject("TimerBarFill", typeof(RectTransform), typeof(Image));
                barFillGo.transform.SetParent(barBgGo.transform, false);
                var barFillRt = (RectTransform)barFillGo.transform;
                barFillRt.anchorMin = Vector2.zero;
                barFillRt.anchorMax = Vector2.one;
                barFillRt.offsetMin = barFillRt.offsetMax = Vector2.zero;
                quizTimerBarImage = barFillGo.GetComponent<Image>();
                quizTimerBarImage.type = Image.Type.Filled;
                quizTimerBarImage.fillMethod = Image.FillMethod.Horizontal;
                quizTimerBarImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                quizTimerBarImage.fillAmount = 1f;
                quizTimerBarImage.color = new Color(0.95f, 0.75f, 0.20f, 1f);

                // Question - Cleanly inset with 60px padding on left/right so it never spills out!
                var qGo = new GameObject("QuestionText", typeof(RectTransform), typeof(Text));
                qGo.transform.SetParent(cardGo.transform, false);
                var qRt = (RectTransform)qGo.transform;
                qRt.anchorMin = new Vector2(0f, 1f);
                qRt.anchorMax = new Vector2(1f, 1f);
                qRt.pivot = new Vector2(0.5f, 1f);
                qRt.anchoredPosition = new Vector2(0f, -120f);
                qRt.sizeDelta = new Vector2(-120f, 105f);
                quizQuestionText = qGo.GetComponent<Text>();
                quizQuestionText.font = font;
                quizQuestionText.fontSize = 28;
                quizQuestionText.fontStyle = FontStyle.Bold;
                quizQuestionText.alignment = TextAnchor.UpperLeft;
                quizQuestionText.horizontalOverflow = HorizontalWrapMode.Wrap;
                quizQuestionText.verticalOverflow = VerticalWrapMode.Truncate;
                quizQuestionText.lineSpacing = 1.25f;
                quizQuestionText.color = Color.white;

                // 4 Options (Big buttons with 24pt bold text)
                quizOptionButtons = new Button[4];
                quizOptionTexts = new Text[4];
                for (int i = 0; i < 4; i++)
                {
                    int optIdx = i;
                    var optGo = new GameObject($"Btn_Option_{i + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
                    optGo.transform.SetParent(cardGo.transform, false);
                    var optRt = (RectTransform)optGo.transform;
                    optRt.anchorMin = new Vector2(0.5f, 1f);
                    optRt.anchorMax = new Vector2(0.5f, 1f);
                    optRt.pivot = new Vector2(0.5f, 1f);
                    optRt.anchoredPosition = new Vector2(0f, -240f - (i * 74f));
                    optRt.sizeDelta = new Vector2(1260f, 62f);

                    var optImg = optGo.GetComponent<Image>();
                    optImg.color = new Color(0.16f, 0.22f, 0.32f, 0.95f);
                    quizOptionButtons[i] = optGo.GetComponent<Button>();

                    var optTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                    optTxtGo.transform.SetParent(optGo.transform, false);
                    var optTxtRt = (RectTransform)optTxtGo.transform;
                    optTxtRt.anchorMin = Vector2.zero;
                    optTxtRt.anchorMax = Vector2.one;
                    optTxtRt.offsetMin = new Vector2(24f, 0f);
                    optTxtRt.offsetMax = new Vector2(-24f, 0f);
                    quizOptionTexts[i] = optTxtGo.GetComponent<Text>();
                    quizOptionTexts[i].font = font;
                    quizOptionTexts[i].fontSize = 24;
                    quizOptionTexts[i].fontStyle = FontStyle.Bold;
                    quizOptionTexts[i].alignment = TextAnchor.MiddleLeft;
                    quizOptionTexts[i].horizontalOverflow = HorizontalWrapMode.Wrap;
                    quizOptionTexts[i].color = Color.white;

                    quizOptionButtons[i].onClick.AddListener(() => SelectQuizAnswer(optIdx));
                }

                // Panel Feedback
                var fbGo = new GameObject("Panel_Feedback", typeof(RectTransform), typeof(Image));
                fbGo.transform.SetParent(cardGo.transform, false);
                var fbRt = (RectTransform)fbGo.transform;
                fbRt.anchorMin = new Vector2(0.5f, 0f);
                fbRt.anchorMax = new Vector2(0.5f, 0f);
                fbRt.pivot = new Vector2(0.5f, 0f);
                fbRt.anchoredPosition = new Vector2(0f, 85f);
                fbRt.sizeDelta = new Vector2(1260f, 120f);
                var fbImg = fbGo.GetComponent<Image>();
                fbImg.color = new Color(0.08f, 0.25f, 0.14f, 0.98f);
                quizFeedbackPanel = fbGo;

                var fbTxtGo = new GameObject("FeedbackText", typeof(RectTransform), typeof(Text));
                fbTxtGo.transform.SetParent(fbGo.transform, false);
                var fbTxtRt = (RectTransform)fbTxtGo.transform;
                fbTxtRt.anchorMin = Vector2.zero;
                fbTxtRt.anchorMax = Vector2.one;
                fbTxtRt.offsetMin = new Vector2(24f, 10f);
                fbTxtRt.offsetMax = new Vector2(-24f, -10f);
                quizFeedbackText = fbTxtGo.GetComponent<Text>();
                quizFeedbackText.font = font;
                quizFeedbackText.fontSize = 22;
                quizFeedbackText.alignment = TextAnchor.MiddleCenter;
                quizFeedbackText.horizontalOverflow = HorizontalWrapMode.Wrap;
                quizFeedbackText.lineSpacing = 1.25f;
                quizFeedbackText.color = Color.white;
                fbGo.SetActive(false);

                // Btn Next / Continue
                var nextGo = new GameObject("Btn_NextQuestion", typeof(RectTransform), typeof(Image), typeof(Button));
                nextGo.transform.SetParent(cardGo.transform, false);
                var nextRt = (RectTransform)nextGo.transform;
                nextRt.anchorMin = new Vector2(1f, 0f);
                nextRt.anchorMax = new Vector2(1f, 0f);
                nextRt.pivot = new Vector2(1f, 0f);
                nextRt.anchoredPosition = new Vector2(-40f, 18f);
                nextRt.sizeDelta = new Vector2(380f, 55f);
                var nextImg = nextGo.GetComponent<Image>();
                nextImg.color = new Color(0.18f, 0.65f, 0.35f, 1f);
                quizNextButton = nextGo.GetComponent<Button>();
                quizNextButton.onClick.AddListener(OnQuizContinueClicked);

                var nextTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                nextTxtGo.transform.SetParent(nextGo.transform, false);
                var nextTxtRt = (RectTransform)nextTxtGo.transform;
                nextTxtRt.anchorMin = Vector2.zero;
                nextTxtRt.anchorMax = Vector2.one;
                nextTxtRt.offsetMin = nextTxtRt.offsetMax = Vector2.zero;
                var nextTxt = nextTxtGo.GetComponent<Text>();
                nextTxt.font = font;
                nextTxt.fontSize = 22;
                nextTxt.fontStyle = FontStyle.Bold;
                nextTxt.alignment = TextAnchor.MiddleCenter;
                nextTxt.color = Color.white;
                nextTxt.text = "CONTINUE EXPLORING ▶";

                // Btn Close ("✕")
                var closeGo = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
                closeGo.transform.SetParent(cardGo.transform, false);
                var closeRt = (RectTransform)closeGo.transform;
                closeRt.anchorMin = new Vector2(1f, 1f);
                closeRt.anchorMax = new Vector2(1f, 1f);
                closeRt.pivot = new Vector2(1f, 1f);
                closeRt.anchoredPosition = new Vector2(-16f, -16f);
                closeRt.sizeDelta = new Vector2(56f, 56f);
                var closeImg = closeGo.GetComponent<Image>();
                closeImg.color = new Color(0.40f, 0.15f, 0.15f, 0.95f);
                quizCloseButton = closeGo.GetComponent<Button>();
                quizCloseButton.onClick.AddListener(CloseARQuiz);

                var closeTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                closeTxtGo.transform.SetParent(closeGo.transform, false);
                var closeTxtRt = (RectTransform)closeTxtGo.transform;
                closeTxtRt.anchorMin = Vector2.zero;
                closeTxtRt.anchorMax = Vector2.one;
                closeTxtRt.offsetMin = closeTxtRt.offsetMax = Vector2.zero;
                var closeTxt = closeTxtGo.GetComponent<Text>();
                closeTxt.font = font;
                closeTxt.fontSize = 28;
                closeTxt.fontStyle = FontStyle.Bold;
                closeTxt.alignment = TextAnchor.MiddleCenter;
                closeTxt.color = Color.white;
                closeTxt.text = "✕";

                quizModalPanel.SetActive(false);
            }
            else
            {
                if (quizTitleText == null) quizTitleText = quizModalPanel.transform.Find("ModalCard/TitleText")?.GetComponent<Text>();
                if (quizCategoryText == null) quizCategoryText = quizModalPanel.transform.Find("ModalCard/CategoryText")?.GetComponent<Text>();
                if (quizQuestionText == null) quizQuestionText = quizModalPanel.transform.Find("ModalCard/QuestionText")?.GetComponent<Text>();
                if (quizFeedbackPanel == null) quizFeedbackPanel = quizModalPanel.transform.Find("ModalCard/Panel_Feedback")?.gameObject;
                if (quizFeedbackPanel != null && quizFeedbackText == null) quizFeedbackText = quizFeedbackPanel.transform.Find("FeedbackText")?.GetComponent<Text>();
                if (quizNextButton == null)
                {
                    quizNextButton = quizModalPanel.transform.Find("ModalCard/Btn_NextQuestion")?.GetComponent<Button>()
                                  ?? quizModalPanel.transform.Find("ModalCard/Btn_Next")?.GetComponent<Button>();
                }
                if (quizCloseButton == null) quizCloseButton = quizModalPanel.transform.Find("ModalCard/Btn_Close")?.GetComponent<Button>();

                if (quizOptionButtons == null || quizOptionButtons.Length < 4)
                {
                    quizOptionButtons = new Button[4];
                    quizOptionTexts = new Text[4];
                    for (int i = 0; i < 4; i++)
                    {
                        var optTr = quizModalPanel.transform.Find($"ModalCard/Btn_Option_{i + 1}");
                        if (optTr != null)
                        {
                            quizOptionButtons[i] = optTr.GetComponent<Button>();
                            quizOptionTexts[i] = optTr.Find("Text")?.GetComponent<Text>();
                        }
                    }
                }
            }
        }

        private void GetChallengeParametersForTier(out int tier, out float targetLoadN, out float maxAllowedEffortN, out string tierLabel)
        {
            bool useAdaptive = !StudyPolicy.Measured || (ResearchData.Store.Current != null && ResearchData.Store.Current.adaptive);
            var engine = SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance;
            tier = useAdaptive ? (engine != null ? (int)engine.CurrentTier : ResearchProgress.GetInt("AdaptiveDifficultyTier", 1)) : 2;
            if (tier <= 1)
            {
                targetLoadN = 100f;
                maxAllowedEffortN = 60f;
                tierLabel = "Tier 1 (Explorer)";
            }
            else if (tier == 2)
            {
                targetLoadN = 100f;
                maxAllowedEffortN = 40f;
                tierLabel = "Tier 2 (Expert)";
            }
            else
            {
                targetLoadN = 120f;
                maxAllowedEffortN = 40f;
                tierLabel = "Tier 3 (Champion)";
            }
        }

        public void OpenChallengeModal()
        {
            if (StudyPolicy.Passive) return;
            AutoBindAllReferences();

            // Ensure the user is in Interactive Mode so the BottomConsole (Fulcrum slider & Force -/+ buttons) is visible!
            if (currentMode != InteractionMode.Interactive)
            {
                SetInteractionMode(InteractionMode.Interactive);
            }

            challengeResolved = false; challengeStarted = Time.time;
            currentHintTierStep = 0;
            PlaySound(clickClip);
            isChallengeActive = true;
            challengeAttempts++;

            GetChallengeParametersForTier(out int tier, out float targetLoadN, out float maxEffortN, out string tierLabel);

            if (storyChoiceWidget != null)
            {
                storyChoiceWidget.SetActive(false);
            }

            if (challengeModalPanel != null)
            {
                challengeModalPanel.SetActive(true);
                challengeModalPanel.transform.SetAsLastSibling();
            }

            if (challengeTitleText != null)
                challengeTitleText.text = $"⭐ TEMPLE LEVER CHALLENGE — {tierLabel.ToUpper()}";

            if (challengeDescText != null)
                challengeDescText.text = $"<b>Mission ({tierLabel}):</b> Lift the heavy <b>{targetLoadN:F0} N</b> temple gate stone using <b>only {maxEffortN:F0} N or less</b> of applied effort force!\n<i>Hint: Slide the <b>Fulcrum</b> left toward the Boulder (to reach MA ≥ {(targetLoadN / maxEffortN):F1}x), then tap <b>Check Solution</b>!</i>";

            if (challengeFeedbackText != null)
                challengeFeedbackText.text = "👇 Use the bottom console to slide the <b>Fulcrum</b> & adjust <b>Force</b>, then tap <b>✅ CHECK SOLUTION</b>!";

            if (challengeSubmitBtn != null)
            {
                var btnImg = challengeSubmitBtn.GetComponent<Image>();
                if (btnImg != null) btnImg.color = new Color(0.14f, 0.62f, 0.28f, 0.98f);
                var btnText = challengeSubmitBtn.GetComponentInChildren<Text>();
                if (btnText != null) btnText.text = "✅ CHECK SOLUTION";
                challengeSubmitBtn.onClick.RemoveAllListeners();
                challengeSubmitBtn.onClick.AddListener(EvaluateChallenge);
            }
        }

        public void CloseChallengeModal()
        {
            PlaySound(clickClip);
            if (challengeModalPanel != null)
                challengeModalPanel.SetActive(false);
        }

        public void CheckChallengeSolution()
        {
            EvaluateChallenge();
        }

        public void EvaluateChallenge()
        {
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed) return;
            GetChallengeParametersForTier(out int tier, out float targetLoadN, out float maxEffortN, out string tierLabel);

            float loadArm = Mathf.Abs(-0.90f - fulcrumOffset);
            float effortArm = Mathf.Abs(0.90f - fulcrumOffset);
            float ma = effortArm / loadArm;
            float requiredForce = targetLoadN / ma;

            float loadTorque = targetLoadN * loadArm;
            float effortTorque = currentEffortForceN * effortArm;
            bool isSuccessful = (effortTorque > loadTorque + 5f && currentEffortForceN <= maxEffortN);
            if (challengeResolved) return;
            challengeResolved = isSuccessful;
            float challengeDuration = Time.time - challengeStarted;
            ResearchData.Record("challenge_attempt", $"lever;tier={tier};load={targetLoadN:F0};max_effort={maxEffortN:F0};success={isSuccessful}", false, isSuccessful, challengeDuration);
            challengeStarted = Time.time;

            if (!StudyPolicy.Measured || (ResearchData.Store.Current != null && ResearchData.Store.Current.adaptive))
            {
                SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance?.RecordTaskResult(isSuccessful, challengeDuration);
                UpdateAdaptiveTierHUD();
            }

            if (isSuccessful)
            {
                UnlockQuestVictoryAndShowContinueStory(tier, ma, targetLoadN, tierLabel);
            }
            else
            {
                PlaySound(clickClip);
                SecretOfLostTemple.Adaptive.HintSystem.Instance?.RecordError();
                if (challengeFeedbackText != null)
                {
                    float neededMa = targetLoadN / maxEffortN;
                    if (ma < neededMa)
                    {
                        lastFailedConceptPart = "fulcrum";
                        challengeFeedbackText.text = $"<color=#ffcc00>⚠️ For {tierLabel}, you need MA ≥ {neededMa:F1}x (currently {ma:F1}x). Move the <b>FULCRUM</b> closer to the Boulder (slide left)!</color>";
                    }
                    else if (currentEffortForceN > maxEffortN)
                    {
                        lastFailedConceptPart = "effort";
                        challengeFeedbackText.text = $"<color=#ffcc00>⚠️ Your MA is {ma:F1}x, which is enough! Now reduce Effort Force to <b>{maxEffortN:F0} N or less</b>!</color>";
                    }
                    else
                    {
                        lastFailedConceptPart = "effort";
                        challengeFeedbackText.text = $"<color=#ffcc00>⚠️ Mechanical Advantage is {ma:F1}x! Increase Force to at least {Mathf.CeilToInt(requiredForce)} N (while keeping ≤ {maxEffortN:F0} N)!</color>";
                    }
                }
                ShowCompanionAndBubbleTemporary(4.5f);
                CognitiveTelemetryLogger.Instance?.LogInteraction("challenge_failed_attempt", null, $"{{\"attempts\":{challengeAttempts},\"ma\":{ma:F2},\"tier\":{tier}}}");
            }
        }

        private IEnumerator QuestSuccessReturnRoutine()
        {
            yield return new WaitForSeconds(3.0f);
            SceneManager.LoadScene("HomeMapScene");
        }

        private IEnumerator AnimateBoulderRoll()
        {
            if (boulderRoot == null) yield break;

            Vector3 startPos = boulderRoot.localPosition;
            Quaternion startRot = boulderRoot.localRotation;
            float duration = 1.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                boulderRoot.localPosition = startPos + new Vector3(-0.4f * t, 0.35f * Mathf.Sin(t * Mathf.PI), 0f);
                boulderRoot.localRotation = startRot * Quaternion.Euler(0f, 0f, 180f * t);
                yield return null;
            }
            boulderRollCoroutine = null;
        }

        // ==========================================
        // PATHIRANA: IN-SITU AR ASSESSMENT & REVIEW
        // ==========================================

        [Serializable]
        private struct InSituQuizQuestion
        {
            public int tier; // 1 = Explorer, 2 = Expert, 3 = Champion
            public string category;
            public string question;
            public string[] options;
            public int correctIndex;
            public string explanation;
            public string targetPart;
        }

        private InSituQuizQuestion[] quizQuestions = new InSituQuizQuestion[]
        {
            // === TIER 1: EXPLORER (Anatomy & Direct Identification - 30s Timer) ===
            new InSituQuizQuestion {
                tier = 1,
                category = "Class 1 Lever Anatomy",
                question = "In this temple Class 1 Lever, which crucial component represents the Load being lifted by the beam?",
                options = new string[] { "A) Heavy Stone Boulder (Load)", "B) Fulcrum Pivot Point", "C) Effort Handle", "D) Rope Pulley" },
                correctIndex = 0,
                explanation = "In a Class 1 Lever, the heavy stone boulder is the Load resisting the lever's movement and being lifted.",
                targetPart = "load"
            },
            new InSituQuizQuestion {
                tier = 1,
                category = "Class 1 Lever Definition",
                question = "Why is this temple machine classified as a First-Class (Class 1) Lever?",
                options = new string[] { "A) The Fulcrum is located BETWEEN the Load and the Effort", "B) The Load is located between the Fulcrum and Effort", "C) The Effort is located between the Fulcrum and Load", "D) It has no pivot point" },
                correctIndex = 0,
                explanation = "In a Class 1 Lever, the fulcrum sits in the middle between the load and the applied effort force.",
                targetPart = "fulcrum"
            },
            new InSituQuizQuestion {
                tier = 1,
                category = "Lever Effort Point",
                question = "Where does the explorer apply muscle strength (Effort) to tilt the lever beam?",
                options = new string[] { "A) At the long Effort Handle on the right", "B) Directly on top of the boulder", "C) Underneath the triangular fulcrum", "D) In the exact center of the stone" },
                correctIndex = 0,
                explanation = "Effort is the input force applied by the explorer on the handle to rotate the beam around the fulcrum.",
                targetPart = "effort"
            },

            // === TIER 2: EXPERT (Leverage Mechanics & Principles - 20s Timer) ===
            new InSituQuizQuestion {
                tier = 2,
                category = "Fulcrum Position & Leverage",
                question = "Where should the explorer slide the triangular fulcrum to lift the megalithic door with LEAST effort?",
                options = new string[] { "A) Closer to the heavy Stone (Load)", "B) Exactly in the middle", "C) Closer to the handle (Effort)", "D) At the very end of the beam" },
                correctIndex = 0,
                explanation = "Placing the fulcrum closer to the Load lengthens the Effort Arm, multiplying your lifting strength!",
                targetPart = "fulcrum"
            },
            new InSituQuizQuestion {
                tier = 2,
                category = "Fulcrum Distance & Effort",
                question = "If the fulcrum is moved closer to your hand (Effort), what happens to the effort required to lift the stone?",
                options = new string[] { "A) You need MORE effort force (Harder)", "B) You need LESS effort force (Easier)", "C) The force stays identical", "D) The stone lifts by itself" },
                correctIndex = 0,
                explanation = "Moving the fulcrum closer to effort shortens the effort arm and lengthens the load arm, requiring much higher effort!",
                targetPart = "fulcrum"
            },
            new InSituQuizQuestion {
                tier = 2,
                category = "Mechanical Advantage Concept",
                question = "What does a Mechanical Advantage (MA) greater than 1 mean for the explorer?",
                options = new string[] { "A) Less input effort is needed than the load's weight", "B) More input effort is needed than the load's weight", "C) The machine cannot move", "D) The load becomes heavier" },
                correctIndex = 0,
                explanation = "A Mechanical Advantage greater than 1 multiplies input force, allowing you to lift heavy loads with less effort!",
                targetPart = "effort"
            },

            // === TIER 3: CHAMPION (Calculations & Torque Equilibrium - 12s Timer) ===
            new InSituQuizQuestion {
                tier = 3,
                category = "Mechanical Advantage Calculation",
                question = "The stone door weighs 120 N. If the Effort Arm is 3x longer than the Load Arm (MA = 3), what force is needed to lift it?",
                options = new string[] { "A) 40 N  (120 N ÷ 3)", "B) 120 N  (Equal force)", "C) 360 N  (120 N × 3)", "D) 10 N" },
                correctIndex = 0,
                explanation = "Mechanical Advantage = Effort Arm ÷ Load Arm = 3. Required Effort = Load ÷ MA = 120 N ÷ 3 = 40 N.",
                targetPart = "effort"
            },
            new InSituQuizQuestion {
                tier = 3,
                category = "Torque & Balance Condition",
                question = "When does the lever beam achieve perfect rotational equilibrium (balance)?",
                options = new string[] { "A) When Clockwise Torque equals Counter-Clockwise Torque", "B) When only the Effort force is applied", "C) When the Fulcrum is removed", "D) When the stone falls off" },
                correctIndex = 0,
                explanation = "Rotational equilibrium occurs when Clockwise Torque = Counter-Clockwise Torque (Effort × Effort Arm = Load × Load Arm).",
                targetPart = "beam"
            },
            new InSituQuizQuestion {
                tier = 3,
                category = "Torque Equilibrium Principle",
                question = "What happens to the required muscle force as you push further away from the fulcrum pivot point?",
                options = new string[] { "A) Required Force Decreases (Easier)", "B) Required Force Increases (Harder)", "C) Force stays exactly the same", "D) Lever loses balance" },
                correctIndex = 0,
                explanation = "Torque = Force × Distance (τ = F × d). A longer distance produces greater rotational torque with less effort.",
                targetPart = "beam"
            }
        };

        private int GetNextQuizIndexForTier(int tier)
        {
            var matching = new System.Collections.Generic.List<int>();
            for (int i = 0; i < quizQuestions.Length; i++)
            {
                if (quizQuestions[i].tier == tier) matching.Add(i);
            }
            if (matching.Count == 0) return 0;
            tierQuizCursor = (tierQuizCursor + 1) % matching.Count;
            return matching[tierQuizCursor];
        }

        private int currentQuizIndex = 0;
        private int currentQuizScore = 0;
        private float quizStartTime = 0f;
        private bool researchAnswerSubmitted;
        private float researchQuestionSeconds;
        private bool researchForeground = true;
        private int researchAttempt;
        private string researchQuizRun;
        private float demonstrationTime;
        private bool challengeResolved;
        private float challengeStarted;
        private float pendingFulcrumLog = -1;
        private float lastInteractionTimestamp = 0f;
        private string lastFailedConceptPart = "fulcrum";

        // In-situ Timed Micro-Quiz System (Triggered every ~10s of active exploration)
        private float microQuizTimer = 0f;
        private const float MICRO_QUIZ_INTERVAL = 10f;
        private bool microQuizEnabled = true;
        private Coroutine autoCloseQuizCoroutine = null;
        private Coroutine postScanQuizCoroutine = null;
        private Text totalStarsBadgeText = null;

        public void TriggerTimedMicroQuiz()
        {
            var canvasTr = transform;
            var uiRoot = canvasTr.Find("LeverAR_UIRoot") ?? canvasTr;
            EnsureQuizModalBuilt(uiRoot);

            if (quizModalPanel == null) return;
            Debug.Log("[MicroQuiz] Triggering quiz popup now!");

            researchQuizRun = System.Guid.NewGuid().ToString("N");
            ResearchData.Record("micro_quiz_popup", "Q" + (currentQuizIndex + 1));

            quizModalPanel.SetActive(true);
            quizModalPanel.transform.SetAsLastSibling();
            quizStartTime = Time.time;

            if (quizNextButton != null)
            {
                quizNextButton.onClick.RemoveAllListeners();
                quizNextButton.onClick.AddListener(OnQuizContinueClicked);
            }
            if (quizCloseButton != null)
            {
                quizCloseButton.onClick.RemoveAllListeners();
                quizCloseButton.onClick.AddListener(CloseARQuiz);
            }

            DisplayCurrentQuizQuestion();
        }

        public void OpenARQuiz()
        {
            if (StudyPolicy.Measured && (!ResearchData.Store.Current.embeddedAssessment || ResearchData.Store.PendingTest != null)) return;
            var canvasTr = transform;
            var uiRoot = canvasTr.Find("LeverAR_UIRoot") ?? canvasTr;
            EnsureQuizModalBuilt(uiRoot);
            if (quizModalPanel == null) return;

            researchQuizRun = System.Guid.NewGuid().ToString("N");
            ResearchData.Record("formative_start", researchQuizRun);
            PlaySound(clickClip);

            quizModalPanel.SetActive(true);
            quizModalPanel.transform.SetAsLastSibling();
            quizStartTime = Time.time;

            if (quizNextButton != null)
            {
                quizNextButton.onClick.RemoveAllListeners();
                quizNextButton.onClick.AddListener(OnQuizContinueClicked);
            }
            if (quizCloseButton != null)
            {
                quizCloseButton.onClick.RemoveAllListeners();
                quizCloseButton.onClick.AddListener(CloseARQuiz);
            }

            DisplayCurrentQuizQuestion();
        }

        public void CloseARQuiz()
        {
            PlaySound(clickClip);
            if (autoCloseQuizCoroutine != null)
            {
                StopCoroutine(autoCloseQuizCoroutine);
                autoCloseQuizCoroutine = null;
            }
            if (quizModalPanel != null) quizModalPanel.SetActive(false);
            currentQuizIndex = (currentQuizIndex + 1) % quizQuestions.Length;
            microQuizTimer = 0f;
        }

        public void OnQuizContinueClicked()
        {
            PlaySound(clickClip);
            if (autoCloseQuizCoroutine != null)
            {
                StopCoroutine(autoCloseQuizCoroutine);
                autoCloseQuizCoroutine = null;
            }
            currentQuizIndex = (currentQuizIndex + 1) % quizQuestions.Length;
            if (quizModalPanel != null) quizModalPanel.SetActive(false);
            microQuizTimer = 0f;
        }

        private IEnumerator AutoResumeExplorationAfterDelay(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            OnQuizContinueClicked();
        }

        private void DisplayCurrentQuizQuestion()
        {
            if (autoCloseQuizCoroutine != null)
            {
                StopCoroutine(autoCloseQuizCoroutine);
                autoCloseQuizCoroutine = null;
            }

            quizStartTime = Time.time;
            researchAnswerSubmitted = false;
            researchAttempt = 0;
            researchQuestionSeconds = 0;
            if (quizFeedbackPanel != null) quizFeedbackPanel.SetActive(false);
            if (quizNextButton != null) quizNextButton.gameObject.SetActive(false);
            if (btnReviewMode != null) btnReviewMode.gameObject.SetActive(false);
            if (btnRetentionReport != null) btnRetentionReport.gameObject.SetActive(false);

            if (quizTimerCoroutine != null)
            {
                StopCoroutine(quizTimerCoroutine);
                quizTimerCoroutine = null;
            }

            var engine = SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance;
            int activeTier = engine != null ? (int)engine.CurrentTier : ResearchProgress.GetInt("AdaptiveDifficultyTier", 1);
            activeTier = Mathf.Clamp(activeTier, 1, 3);
            string tierBadge = activeTier == 1 ? "EXPLORER" : (activeTier == 2 ? "EXPERT" : "CHAMPION");

            currentQuizIndex = GetNextQuizIndexForTier(activeTier);
            var q = quizQuestions[currentQuizIndex];

            float timerDuration = activeTier == 1 ? 30f : (activeTier == 2 ? 20f : 12f);
            quizTimerCoroutine = StartCoroutine(QuizCountdownTimerRoutine(timerDuration));

            if (quizTitleText != null)
            {
                quizTitleText.text = $"⚡ {tierBadge} AR QUIZ ({tierBadge} TIER) ⚡";
                quizTitleText.fontSize = 32;
            }
            if (quizCategoryText != null)
            {
                int totalStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
                quizCategoryText.text = $"<b>Topic:</b> {q.category}  |  ⭐ <b>Stars:</b> {totalStars}";
                quizCategoryText.fontSize = 22;
            }
            if (quizQuestionText != null)
            {
                quizQuestionText.text = q.question;
                quizQuestionText.fontSize = 28;
                quizQuestionText.lineSpacing = 1.25f;
            }

            if (quizOptionButtons != null && quizOptionTexts != null)
            {
                for (int i = 0; i < quizOptionButtons.Length; i++)
                {
                    if (i < q.options.Length && quizOptionButtons[i] != null)
                    {
                        quizOptionButtons[i].gameObject.SetActive(true);
                        quizOptionButtons[i].interactable = true;

                        // Reset to crisp default theme color
                        var btnImg = quizOptionButtons[i].GetComponent<Image>();
                        if (btnImg != null) btnImg.color = new Color(0.16f, 0.22f, 0.32f, 0.95f);

                        if (quizOptionTexts[i] != null)
                        {
                            quizOptionTexts[i].text = q.options[i];
                            quizOptionTexts[i].color = Color.white;
                            quizOptionTexts[i].fontSize = 24;
                            quizOptionTexts[i].fontStyle = FontStyle.Bold;
                        }

                        quizOptionButtons[i].transform.localScale = Vector3.one;

                        // Bind dynamic click listener
                        int optIndex = i;
                        quizOptionButtons[i].onClick.RemoveAllListeners();
                        quizOptionButtons[i].onClick.AddListener(() => SelectQuizAnswer(optIndex));
                    }
                    else if (quizOptionButtons[i] != null)
                    {
                        quizOptionButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public void SelectQuizAnswer(int optionIndex)
        {
            if (currentQuizIndex >= quizQuestions.Length || researchAnswerSubmitted) return;
            var q = quizQuestions[currentQuizIndex];
            if (optionIndex < 0 || optionIndex >= q.options.Length) return;

            researchAttempt++;
            bool correct = (optionIndex == q.correctIndex);
            float elapsed = researchQuestionSeconds;
            string attemptId = researchQuizRun + ":" + currentQuizIndex + ":" + researchAttempt;
            int tier = ResearchProgress.GetInt("AdaptiveDifficultyTier", 1);

            try
            {
                ResearchData.Store.RecordAttempt("lever_formative_" + currentQuizIndex, attemptId, researchAttempt, optionIndex, correct, elapsed, tier, JsonUtility.ToJson(q) + ";review=" + StudyPolicy.Review);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ARQuiz] Attempt log note: " + e.Message);
            }

            if (quizTimerCoroutine != null)
            {
                StopCoroutine(quizTimerCoroutine);
                quizTimerCoroutine = null;
            }

            researchAnswerSubmitted = true;
            int currentStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
            if (correct)
            {
                currentQuizScore++;
                currentStars += 1;
            }
            else
            {
                currentStars = Mathf.Max(0, currentStars - 1);
            }
            ResearchProgress.SetInt("LostTemple_Total_Stars", currentStars);
            PlayerPrefs.Save();
            UpdateTotalStarsHUD();

            // 1. Immediate Visual Feedback on Option Buttons (Right / Wrong on the spot!)
            if (quizOptionButtons != null)
            {
                for (int i = 0; i < quizOptionButtons.Length; i++)
                {
                    if (quizOptionButtons[i] == null) continue;
                    quizOptionButtons[i].interactable = false; // Lock answers immediately

                    var btnImg = quizOptionButtons[i].GetComponent<Image>();
                    if (btnImg == null) continue;

                    if (i == optionIndex)
                    {
                        // Selected option: Vibrant Green if correct, Vibrant Red if wrong
                        btnImg.color = correct ? new Color(0.18f, 0.75f, 0.35f, 1f) : new Color(0.85f, 0.22f, 0.20f, 1f);
                        StartCoroutine(PulseButton(quizOptionButtons[i].transform));
                    }
                    else if (i == q.correctIndex)
                    {
                        // Always highlight the correct answer in Green so the child learns immediately!
                        btnImg.color = new Color(0.18f, 0.75f, 0.35f, 1f);
                    }
                    else
                    {
                        // Dim non-selected options
                        btnImg.color = new Color(0.10f, 0.14f, 0.20f, 0.50f);
                    }
                }
            }

            // 2. Immediate Audio Feedback
            if (correct)
            {
                AudioClip winClip = successClip != null ? successClip : ((activeCharacter == "Senuk" && senukBalancedClip != null) ? senukBalancedClip : mayaBalancedClip);
                PlaySound(winClip != null ? winClip : clickClip);
            }
            else
            {
                AudioClip loseClip = (activeCharacter == "Senuk" && senukTooHeavyClip != null) ? senukTooHeavyClip : mayaTooHeavyClip;
                PlaySound(loseClip != null ? loseClip : clickClip);
            }

            // 3. Highlight 3D Physical Concept in the AR World
            if (!string.IsNullOrEmpty(q.targetPart))
            {
                HighlightConcept(q.targetPart);
            }

            // 4. Feedback Banner with Instant Right/Wrong Verdict & Scientific Explanation
            if (quizFeedbackPanel != null)
            {
                quizFeedbackPanel.SetActive(true);
                var fbImg = quizFeedbackPanel.GetComponent<Image>();
                if (fbImg != null)
                {
                    fbImg.color = correct ? new Color(0.08f, 0.30f, 0.15f, 0.98f) : new Color(0.35f, 0.10f, 0.10f, 0.98f);
                }
                var fbRt = quizFeedbackPanel.GetComponent<RectTransform>();
                if (fbRt != null)
                {
                    fbRt.sizeDelta = new Vector2(1260f, 120f);
                    fbRt.anchoredPosition = new Vector2(0f, 85f);
                }
            }

            if (quizFeedbackText != null)
            {
                if (correct)
                {
                    int totalStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
                    quizFeedbackText.text = $"<size=24><color=#2ecc71><b>✅ CORRECT! GREAT JOB! ⭐ +1 STAR EARNED! (Total: {totalStars} ⭐)</b></color></size>\n{q.explanation}";
                }
                else
                {
                    int totalStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
                    string correctOptText = (q.correctIndex >= 0 && q.correctIndex < q.options.Length) ? q.options[q.correctIndex] : "";
                    quizFeedbackText.text = $"<size=24><color=#ff6b6b><b>❌ INCORRECT! ⭐ -1 STAR REDUCED! (Total: {totalStars} ⭐)</b></color></size>\n<b>Correct:</b> <color=#55ff88>{correctOptText}</color>\n{q.explanation}";
                }
            }

            // 5. Next/Continue Button
            if (quizNextButton != null)
            {
                quizNextButton.gameObject.SetActive(true);
                var nextTxt = quizNextButton.GetComponentInChildren<Text>();
                if (nextTxt != null)
                {
                    nextTxt.text = "CONTINUE EXPLORING ▶";
                    nextTxt.fontSize = 22;
                    nextTxt.fontStyle = FontStyle.Bold;
                }
                var nextRt = quizNextButton.GetComponent<RectTransform>();
                if (nextRt != null)
                {
                    nextRt.sizeDelta = new Vector2(380f, 55f);
                }
                var nextImg = quizNextButton.GetComponent<Image>();
                if (nextImg != null)
                {
                    nextImg.color = new Color(0.18f, 0.65f, 0.35f, 1f);
                }

                quizNextButton.onClick.RemoveAllListeners();
                quizNextButton.onClick.AddListener(OnQuizContinueClicked);
            }

            // 6. Auto-resume exploration after 4.5 seconds so child seamlessly returns to 3D lever hands-free
            if (autoCloseQuizCoroutine != null) StopCoroutine(autoCloseQuizCoroutine);
            autoCloseQuizCoroutine = StartCoroutine(AutoResumeExplorationAfterDelay(4.5f));

            if (!StudyPolicy.Measured || ResearchData.Store.Current.adaptive)
                SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance?.RecordTaskResult(correct, elapsed);

            UpdateAdaptiveTierHUD();
            ResearchCloudSync.Instance?.TriggerImmediateSync($"MicroQuiz Q{currentQuizIndex + 1} answered ({(correct ? "Correct" : "Incorrect")})");
        }


        private void OnApplicationPause(bool paused) { researchForeground = !paused; }
        private void OnApplicationFocus(bool focus) { researchForeground = focus; }

        private string ConceptHint(string part)
        {
            if (part == "load") return "Find the stone being lifted. That object is the load; the pivot underneath the beam is the fulcrum.";
            if (part == "fulcrum") return "Find the pivot. Compare the distance from the pivot to the load and to your hand.";
            if (part == "effort") return "Compare the effort arm with the load arm. A longer effort arm multiplies the turning effect.";
            return "Turning effect depends on both force and distance from the pivot.";
        }
        private void HighlightConcept(string part)
        {
            Transform target = (part == "load" || part == "boulder") ? boulderRoot : part == "fulcrum" ? fulcrumRoot : part == "effort" ? effortPoint : beamPivot;
            if (target != null) StartCoroutine(HighlightRoutine(target));

            if (part == "load" || part == "boulder")
            {
                if (loadVectorObject != null)
                {
                    loadVectorObject.gameObject.SetActive(true);
                    StartCoroutine(HighlightVectorPulse(loadVectorObject));
                }
                if (loadAnnotationBadge != null) loadAnnotationBadge.SetActive(true);
            }
            else if (part == "fulcrum")
            {
                if (fulcrumVectorObject != null)
                {
                    fulcrumVectorObject.gameObject.SetActive(true);
                    StartCoroutine(HighlightVectorPulse(fulcrumVectorObject));
                }
                if (fulcrumAnnotationBadge != null) fulcrumAnnotationBadge.SetActive(true);
            }
            else if (part == "effort")
            {
                if (effortVectorObject != null)
                {
                    effortVectorObject.gameObject.SetActive(true);
                    StartCoroutine(HighlightVectorPulse(effortVectorObject));
                }
                if (effortAnnotationBadge != null) effortAnnotationBadge.SetActive(true);
            }
        }

        private IEnumerator HighlightVectorPulse(Transform vectorTr)
        {
            if (vectorTr == null) yield break;
            Vector3 originalScale = vectorTr.localScale;
            float elapsed = 0f;
            while (elapsed < 2f)
            {
                elapsed += Time.deltaTime;
                float pulse = 1f + 0.25f * Mathf.Sin(elapsed * 12f);
                vectorTr.localScale = originalScale * pulse;
                yield return null;
            }
            vectorTr.localScale = originalScale;
        }
        private IEnumerator HighlightRoutine(Transform target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            var previous = new System.Collections.Generic.List<MaterialPropertyBlock>();
            foreach (var renderer in renderers) {
                var old = new MaterialPropertyBlock(); renderer.GetPropertyBlock(old); previous.Add(old);
                var highlight = new MaterialPropertyBlock(); renderer.GetPropertyBlock(highlight);
                highlight.SetColor("_BaseColor", Color.yellow); highlight.SetColor("_Color", Color.yellow); renderer.SetPropertyBlock(highlight);
            }
            yield return new WaitForSeconds(2);
            for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].SetPropertyBlock(previous[i]);
        }
        private IEnumerator PulseButton(Transform tr)
        {
            if (tr == null) yield break;
            Vector3 orig = Vector3.one;
            float elapsed = 0f;
            while (elapsed < 0.35f)
            {
                elapsed += Time.deltaTime;
                float scale = 1f + 0.08f * Mathf.Sin((elapsed / 0.35f) * Mathf.PI);
                tr.localScale = orig * scale;
                yield return null;
            }
            tr.localScale = orig;
        }

        private IEnumerator ShakeButton(Transform tr)
        {
            if (tr == null) yield break;
            Vector3 origPos = tr.localPosition;
            float elapsed = 0f;
            while (elapsed < 0.35f)
            {
                elapsed += Time.deltaTime;
                float offset = Mathf.Sin(elapsed * 45f) * (6f * (1f - (elapsed / 0.35f)));
                tr.localPosition = origPos + new Vector3(offset, 0f, 0f);
                yield return null;
            }
            tr.localPosition = origPos;
        }

        public void NextQuizQuestion()
        {
            OnQuizContinueClicked();
        }

        public void ToggleReviewMode()
        {
            ResearchData.Record("review_open", lastFailedConceptPart.ToString());
            PlaySound(clickClip);
            CloseARQuiz();
            CloseRetentionSummary();

            // Set to View-Only Exploded Parts to review the failed concept
            SetInteractionMode(InteractionMode.ViewOnly);
            SetViewOnlySubMode(ViewOnlySubMode.ExplodedParts);
            ShowPartDetails(lastFailedConceptPart);
        }

        public void OpenRetentionSummary()
        {
            PlaySound(clickClip);
            if (retentionModalPanel == null) return;
            retentionModalPanel.SetActive(true);
            if (retentionSummaryText != null) retentionSummaryText.text = ResearchData.FormalStatus + "\n\n" + ResearchData.Summary();
        }

        public void CloseRetentionSummary()
        {
            PlaySound(clickClip);
            if (retentionModalPanel != null) retentionModalPanel.SetActive(false);
        }

        public void ExportResearchCSV()
        {
            try { string path = ResearchData.Store.Export(); if (retentionSummaryText != null) retentionSummaryText.text = "Exported locally: " + path; }
            catch (System.Exception e) { if (retentionSummaryText != null) retentionSummaryText.text = "Export failed: " + e.Message; }
        }

        // ==========================================
        // ABEYWICKRAMA: ADAPTIVE TIER, 3-TIER HINTS & STORY BRANCHING
        // ==========================================

        private int currentHintTierStep = 0;
        private GameObject storyChoiceWidget;
        private Text storyNarrativeLabel;
        private Button storyChoiceBtnA;
        private Button storyChoiceBtnB;

                private void EnsureStarsBadgeBuilt(Transform uiRoot)
        {
            if (uiRoot == null) return;
            var topBar = uiRoot.Find("TopBar");
            if (topBar == null) return;

            var existingBadge = topBar.Find("Badge_Stars");
            if (existingBadge != null)
            {
                totalStarsBadgeText = existingBadge.Find("Text")?.GetComponent<Text>();
                UpdateTotalStarsHUD();
                return;
            }

            var badgeGo = new GameObject("Badge_Stars", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(topBar, false);
            var rt = (RectTransform)badgeGo.transform;
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-460f, 0f);
            rt.sizeDelta = new Vector2(140f, 42f);

            var img = badgeGo.GetComponent<Image>();
            img.color = new Color(0.12f, 0.10f, 0.05f, 0.90f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(badgeGo.transform, false);
            var trt = (RectTransform)textGo.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            totalStarsBadgeText = textGo.GetComponent<Text>();
            totalStarsBadgeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            totalStarsBadgeText.fontSize = 17;
            totalStarsBadgeText.fontStyle = FontStyle.Bold;
            totalStarsBadgeText.alignment = TextAnchor.MiddleCenter;
            totalStarsBadgeText.color = new Color(1f, 0.85f, 0.20f, 1f);

            UpdateTotalStarsHUD();
        }

        public void UpdateTotalStarsHUD()
        {
            int currentStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
            if (totalStarsBadgeText != null)
            {
                totalStarsBadgeText.text = $"⭐ <b>{currentStars} STARS</b>";
            }
        }

        public void UpdateAdaptiveTierHUD()
        {
            var engine = SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance;
            int tier = engine != null ? (int)engine.CurrentTier : ResearchProgress.GetInt("AdaptiveDifficultyTier", 1);

            if (adaptiveTierBadgeText != null)
            {
                switch (tier)
                {
                    case 1:
                        adaptiveTierBadgeText.text = "🎒 <b>EXPLORER</b>";
                        break;
                    case 2:
                        adaptiveTierBadgeText.text = "⚡ <b>EXPERT</b>";
                        break;
                    case 3:
                        adaptiveTierBadgeText.text = "🏆 <b>CHAMPION</b>";
                        break;
                }
            }
        }

        private void InitializeInteractiveStoryBranchUI()
        {
            if (storyChoiceWidget != null) storyChoiceWidget.SetActive(false);
            return;
            // When launched from the StoryBook Page 9 activity, story narrative is read in the StoryBook Viewer — keep the AR viewport clean!
            if (SecretOfLostTemple.Story.StoryBookViewerController.StoryActivityActive)
            {
                if (storyChoiceWidget != null) storyChoiceWidget.SetActive(false);
                return;
            }

            bool showAdaptiveStory = StudyPolicy.Measured && ResearchData.Store.Current != null && ResearchData.Store.Current.adaptive;
            if (!showAdaptiveStory) return;

            var sb = SecretOfLostTemple.Adaptive.StoryBranchManager.Instance;
            if (sb == null) return;
            if (sb.GetCurrentNode() == null)
            {
                string savedNode = ResearchProgress.GetString("StoryNode", "node_2");
                if (string.IsNullOrEmpty(savedNode)) savedNode = "node_2";
                sb.LoadNode(savedNode);
            }

            if (storyChoiceWidget != null) return;
            var uiRoot = transform.Find("LeverAR_UIRoot") ?? transform;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            storyChoiceWidget = new GameObject("StoryBranchChoiceBar", typeof(RectTransform));
            storyChoiceWidget.transform.SetParent(uiRoot, false);
            var rt = (RectTransform)storyChoiceWidget.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -108f);
            rt.sizeDelta = new Vector2(920f, 84f);

            var bg = storyChoiceWidget.AddComponent<Image>();
            bg.color = new Color(0.11f, 0.08f, 0.04f, 0.90f);

            var textGo = new GameObject("StoryText", typeof(RectTransform));
            textGo.transform.SetParent(storyChoiceWidget.transform, false);
            var trt = (RectTransform)textGo.transform;
            trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 1f);
            trt.anchoredPosition = new Vector2(0f, -6f);
            trt.sizeDelta = new Vector2(890f, 32f);
            storyNarrativeLabel = textGo.AddComponent<Text>();
            storyNarrativeLabel.font = font;
            storyNarrativeLabel.fontSize = 16;
            storyNarrativeLabel.alignment = TextAnchor.MiddleCenter;
            storyNarrativeLabel.color = new Color(1f, 0.92f, 0.72f);

            storyChoiceBtnA = CreateStoryChoiceButton("ChoiceA", storyChoiceWidget.transform, font, -220f, -56f, 410f, 34f, () => SelectStoryBranchChoice(0));
            storyChoiceBtnB = CreateStoryChoiceButton("ChoiceB", storyChoiceWidget.transform, font, 220f, -56f, 410f, 34f, () => SelectStoryBranchChoice(1));

            RefreshStoryBranchBar();
        }

        private Button CreateStoryChoiceButton(string name, Transform parent, Font font, float x, float y, float w, float h, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.32f, 0.22f, 0.09f, 0.95f);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var txtGo = new GameObject("Text", typeof(RectTransform));
            txtGo.transform.SetParent(go.transform, false);
            var trt = (RectTransform)txtGo.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            var txt = txtGo.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 15;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(1f, 0.95f, 0.80f);
            return btn;
        }

        private void SelectStoryBranchChoice(int choiceIndex)
        {
            PlaySound(clickClip);
            var sb = SecretOfLostTemple.Adaptive.StoryBranchManager.Instance;
            if (sb == null) return;
            var node = sb.GetCurrentNode();
            if (node == null || node.choices == null || choiceIndex >= node.choices.Count) return;

            string prevId = node.nodeId;
            string nextId = node.choices[choiceIndex].nextNodeId;
            sb.MakeChoice(choiceIndex);
            ResearchData.Record("story_transition", $"{prevId}->{nextId};choice={choiceIndex};rule=lever-pilot-1");
            SecretOfLostTemple.Adaptive.EngagementTracker.Instance?.RecordStoryDecision($"{prevId}:{choiceIndex}");
            RefreshStoryBranchBar();
        }

        private void RefreshStoryBranchBar()
        {
            if (storyChoiceWidget != null) storyChoiceWidget.SetActive(false);
            return;
            var sb = SecretOfLostTemple.Adaptive.StoryBranchManager.Instance;
            var node = sb != null ? sb.GetCurrentNode() : null;
            if (node == null)
            {
                storyChoiceWidget.SetActive(false);
                return;
            }
            storyChoiceWidget.SetActive(true);
            if (storyNarrativeLabel != null)
            {
                storyNarrativeLabel.text = $"📖 <b>{node.characterSpeaker} ({node.title}):</b> {node.narrativeText}";
            }
            if (node.choices != null && node.choices.Count > 0)
            {
                if (storyChoiceBtnA != null)
                {
                    storyChoiceBtnA.gameObject.SetActive(true);
                    storyChoiceBtnA.GetComponentInChildren<Text>().text = "▶ " + node.choices[0].choiceText;
                }
                if (storyChoiceBtnB != null)
                {
                    if (node.choices.Count > 1)
                    {
                        storyChoiceBtnB.gameObject.SetActive(true);
                        storyChoiceBtnB.GetComponentInChildren<Text>().text = "▶ " + node.choices[1].choiceText;
                    }
                    else
                    {
                        storyChoiceBtnB.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                if (storyChoiceBtnA != null)
                {
                    storyChoiceBtnA.gameObject.SetActive(true);
                    storyChoiceBtnA.GetComponentInChildren<Text>().text = "🔄 Explore Another Temple Story Branch";
                    storyChoiceBtnA.onClick.RemoveAllListeners();
                    storyChoiceBtnA.onClick.AddListener(() =>
                    {
                        sb.LoadNode("node_2");
                        storyChoiceBtnA.onClick.RemoveAllListeners();
                        storyChoiceBtnA.onClick.AddListener(() => SelectStoryBranchChoice(0));
                        RefreshStoryBranchBar();
                    });
                }
                if (storyChoiceBtnB != null) storyChoiceBtnB.gameObject.SetActive(false);
            }
        }

        public void TriggerSmartHint()
        {
            if (ResearchData.Store.PendingTest != null) return;
            currentHintTierStep = Mathf.Clamp(currentHintTierStep + 1, 1, 3);
            ResearchData.Record("hint_requested", $"lever:{lastFailedConceptPart}:tier{currentHintTierStep}");
            ResearchData.Record("hint", $"lever:{lastFailedConceptPart}:level{currentHintTierStep}");
            SecretOfLostTemple.Adaptive.EngagementTracker.Instance?.RecordHintRequest();
            HighlightConcept(lastFailedConceptPart);
            PlaySound(clickClip);
            lastInteractionTimestamp = Time.time;

            // Requirement 2: Show Companion Avatar & Speech Banner temporarily during hint
            ShowCompanionHint();

            if (smartHintModalPanel == null) AutoBindAllReferences();
            if (smartHintModalPanel == null) return;

            smartHintModalPanel.SetActive(true);

            if (smartHintCloseBtn != null)
            {
                smartHintCloseBtn.onClick.RemoveAllListeners();
                smartHintCloseBtn.onClick.AddListener(CloseSmartHint);
            }

            var cardTr = smartHintModalPanel.transform.Find("ModalCard");
            if (cardTr != null)
            {
                StopCoroutine("AnimateSpeechBubble");
                StartCoroutine(AnimateSpeechBubble(cardTr));
            }

            string charName = activeCharacter == "Senuk" ? "SENUK" : "MAYA";
            string tierBadge = currentHintTierStep == 1 ? "TIER 1/3 (Visual Nudge)" :
                               currentHintTierStep == 2 ? "TIER 2/3 (Character Clue)" :
                                                          "TIER 3/3 (Direct Formula Walkthrough)";
            var titleTxt = smartHintModalPanel.transform.Find("ModalCard/TitleText")?.GetComponent<Text>();
            if (titleTxt != null)
            {
                titleTxt.text = $"💬 <b>{charName}'S SMART HINT — {tierBadge}</b>";
            }

            if (smartHintBodyText != null)
            {
                if (currentHintTierStep == 1)
                {
                    smartHintBodyText.text =
                        $"<b>🔍 Tier 1 — Visual Nudge ({lastFailedConceptPart.ToUpper()}):</b>\n" +
                        "Watch the glowing yellow highlight on the 3D lever mechanism! Notice how the distances from the triangular <b>Fulcrum</b> to the <b>Boulder</b> and the <b>Handle</b> compare.\n\n" +
                        "<i>(Need a deeper clue? Tap the Hint button again for Tier 2!)</i>";
                }
                else if (currentHintTierStep == 2)
                {
                    if (lastFailedConceptPart == "load")
                    {
                        smartHintBodyText.text =
                            "<b>💡 Tier 2 — Character Clue:</b>\n" +
                            "\"Find the heavy stone boulder on the left side of the beam! That object is the <b>Load</b> resisting movement. Keep the fulcrum closer to the load to shorten the Load Arm!\"\n\n" +
                            "<i>(Tap Hint once more for the exact Tier 3 Formula Walkthrough!)</i>";
                    }
                    else if (lastFailedConceptPart == "effort")
                    {
                        smartHintBodyText.text =
                            "<b>💡 Tier 2 — Character Clue:</b>\n" +
                            "\"The <b>Effort</b> is your downward push at the right handle. A longer Effort Arm multiplies your torque so you need much less muscle force!\"\n\n" +
                            "<i>(Tap Hint once more for the exact Tier 3 Formula Walkthrough!)</i>";
                    }
                    else
                    {
                        smartHintBodyText.text =
                            "<b>💡 Tier 2 — Character Clue:</b>\n" +
                            "\"Slide the triangular <b>Fulcrum</b> toward the left stone boulder! Lengthening the Effort Arm increases Mechanical Advantage!\"\n\n" +
                            "<i>(Tap Hint once more for the exact Tier 3 Formula Walkthrough!)</i>";
                    }
                }
                else
                {
                    GetChallengeParametersForTier(out int tier, out float targetLoadN, out float maxEffortN, out string tierLabel);
                    float neededMa = targetLoadN / maxEffortN;
                    smartHintBodyText.text =
                        $"<b>📐 Tier 3 — Direct Formula Walkthrough ({tierLabel}):</b>\n" +
                        $"• <b>Formula:</b> Mechanical Advantage (MA) = Effort Arm ÷ Load Arm\n" +
                        $"• <b>Required Effort:</b> Load ({targetLoadN:F0} N) ÷ MA ≤ {maxEffortN:F0} N  ⇒  Need <b>MA ≥ {neededMa:F1}x</b>\n" +
                        $"• <b>Exact Step-by-Step Solution:</b> Slide the Fulcrum left to <b>-0.40 m</b> (giving MA = 2.6x–3.0x), then set Force to <b>{maxEffortN:F0} N</b>!";
                }
            }

            CognitiveTelemetryLogger.Instance?.LogInteraction("smart_hint_opened", null, $"{{\"character\":\"{activeCharacter}\",\"hintTier\":{currentHintTierStep}}}");
            ResearchCloudSync.Instance?.TriggerImmediateSync($"Opened Smart Hint (Tier {currentHintTierStep})");
        }

        private IEnumerator AnimateSpeechBubble(Transform bubbleTr)
        {
            bubbleTr.localScale = new Vector3(0.2f, 0.2f, 1f);
            float elapsed = 0f;
            float duration = 0.22f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smooth = Mathf.SmoothStep(0f, 1f, t);
                float scale = Mathf.Lerp(0.2f, 1.05f, smooth);
                bubbleTr.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            bubbleTr.localScale = Vector3.one;
        }

        public void CloseSmartHint()
        {
            PlaySound(clickClip);
            if (smartHintModalPanel != null) smartHintModalPanel.SetActive(false);
            if (characterCompanionImage != null) characterCompanionImage.gameObject.SetActive(false);
        }

        public void ProceedToNextChapter()
        {
            SceneManager.LoadScene("HomeMapScene");
        }

        private IEnumerator QuizCountdownTimerRoutine(float totalSeconds)
        {
            float elapsed = 0f;
            while (elapsed < totalSeconds && !researchAnswerSubmitted)
            {
                elapsed += Time.unscaledDeltaTime;
                float remaining = Mathf.Max(0f, totalSeconds - elapsed);
                float frac = remaining / totalSeconds;
                if (quizTimerBarImage != null)
                {
                    quizTimerBarImage.fillAmount = frac;
                    quizTimerBarImage.color = frac > 0.35f ? new Color(0.95f, 0.75f, 0.20f, 1f) : new Color(0.95f, 0.25f, 0.20f, 1f);
                }
                if (quizTimerText != null)
                {
                    quizTimerText.text = $"⏱️ {Mathf.CeilToInt(remaining)}s";
                }
                yield return null;
            }

            if (!researchAnswerSubmitted)
            {
                OnQuizTimeExpired();
            }
        }

        private void OnQuizTimeExpired()
        {
            if (researchAnswerSubmitted) return;
            researchAnswerSubmitted = true;

            int currentStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
            currentStars = Mathf.Max(0, currentStars - 1);
            ResearchProgress.SetInt("LostTemple_Total_Stars", currentStars);
            PlayerPrefs.Save();
            UpdateTotalStarsHUD();
            if (quizOptionButtons != null)
            {
                for (int i = 0; i < quizOptionButtons.Length; i++)
                {
                    if (quizOptionButtons[i] == null) continue;
                    quizOptionButtons[i].interactable = false;
                    var q = quizQuestions[currentQuizIndex];
                    if (i == q.correctIndex)
                    {
                        var btnImg = quizOptionButtons[i].GetComponent<Image>();
                        if (btnImg != null) btnImg.color = new Color(0.18f, 0.75f, 0.35f, 1f);
                    }
                }
            }

            AudioClip loseClip = (activeCharacter == "Senuk" && senukTooHeavyClip != null) ? senukTooHeavyClip : mayaTooHeavyClip;
            PlaySound(loseClip != null ? loseClip : clickClip);

            var currQ = quizQuestions[currentQuizIndex];
            if (quizFeedbackPanel != null)
            {
                quizFeedbackPanel.SetActive(true);
                var fbImg = quizFeedbackPanel.GetComponent<Image>();
                if (fbImg != null) fbImg.color = new Color(0.38f, 0.12f, 0.12f, 0.98f);
            }
            if (quizFeedbackText != null)
            {
                int totalStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
                string correctOptText = (currQ.correctIndex >= 0 && currQ.correctIndex < currQ.options.Length) ? currQ.options[currQ.correctIndex] : "";
                quizFeedbackText.text = $"<size=24><color=#ff6b6b><b>⏱️ TIME'S UP! ⭐ -1 STAR REDUCED! (Total: {totalStars} ⭐)</b></color></size>\n<b>Correct:</b> <color=#55ff88>{correctOptText}</color>\n{currQ.explanation}";
            }

            if (quizNextButton != null)
            {
                quizNextButton.gameObject.SetActive(true);
                var nextTxt = quizNextButton.GetComponentInChildren<Text>();
                if (nextTxt != null) nextTxt.text = "<b>CONTINUE ➡</b>";
            }

            autoCloseQuizCoroutine = StartCoroutine(AutoResumeExplorationAfterDelay(4.0f));
        }

        public void OnBackClicked()
        {
            PlaySound(clickClip);
            string activeScene = SceneManager.GetActiveScene().name;
            if (activeScene == "FreePracticeScene")
            {
                SceneManager.LoadScene("HomeMapScene");
            }
            else if (StudyPolicy.Measured) ResearchSetupUI.ReturnToPanel();
            else if (SecretOfLostTemple.Story.StoryBookViewerController.StoryActivityActive)
            {
                bool challengeDone = PlayerPrefs.GetInt("LostTemple_Lever_Challenge_Completed", 0) == 1;
                SecretOfLostTemple.Story.StoryBookViewerController.ReturnToStory(challengeDone);
            }
            else SceneManager.LoadScene("HomeMapScene");
        }

        // ==========================================
        // GUIDED STORY DIALOGUE FLOW (PAGE 9 AR QUEST)
        // ==========================================

        private struct GuidedStoryStep
        {
            public string title;
            public string sinhalaText;
            public string highlightPart;
            public string buttonText;
        }

        private readonly GuidedStoryStep[] storyDialogueSteps = new GuidedStoryStep[]
        {
            new GuidedStoryStep
            {
                title = "Step 1/4: Ancient Temple Gate",
                sinhalaText = "We have reached the ancient temple gate! Look, a massive boulder is blocking our path. To open the gate, we need to use this ancient Lever mechanism!",
                highlightPart = "load",
                buttonText = "NEXT ▶"
            },
            new GuidedStoryStep
            {
                title = "Step 2/4: Three Parts of a Lever",
                sinhalaText = "A lever consists of 3 key parts:\n1. 🪨 <b>Load</b> - The heavy stone boulder to be lifted.\n2. 🔺 <b>Fulcrum</b> - The central pivot point balancing the beam.\n3. ⚡ <b>Effort</b> - The downward force applied.",
                highlightPart = "fulcrum",
                buttonText = "NEXT ▶"
            },
            new GuidedStoryStep
            {
                title = "Step 3/4: The Physics Secret",
                sinhalaText = "Here is the key secret:\n🔺 The closer the <b>Fulcrum</b> is to the <b>Load</b>, the less <b>Effort Force</b> you need to lift the stone!\n(A shorter Load Arm gives greater Mechanical Advantage!)",
                highlightPart = "effort",
                buttonText = "NEXT ▶"
            },
            new GuidedStoryStep
            {
                title = "Step 4/4: Temple Challenge",
                sinhalaText = "Now it's your turn!\nSlide the fulcrum closer to the boulder, apply the required force, and lift the stone to unseal the entrance!",
                highlightPart = "load",
                buttonText = "START CHALLENGE! 🚀"
            }
        };

        public void StartGuidedStoryDialogueSequence()
        {
            isGuidedStoryFlowActive = true;
            if (bottomConsole != null) bottomConsole.SetActive(false);

            activeCharacter = ResearchProgress.GetString("SelectedCharacter", "Maya");

            Transform canvasTr = null;
            var arMainCanvas = GameObject.Find("AR Main Canvas");
            if (arMainCanvas != null) canvasTr = arMainCanvas.transform;
            if (canvasTr == null && bottomConsole != null) canvasTr = bottomConsole.transform.parent;
            if (canvasTr == null) canvasTr = GameObject.Find("Canvas")?.transform;
            if (canvasTr == null) return;

            EnsureStoryDialoguePanelBuilt(canvasTr);
            if (storyDialoguePanel == null) return;

            storyDialoguePanel.SetActive(true);
            storyDialoguePanel.transform.SetAsLastSibling();
            currentStoryDialogueIndex = 0;
            DisplayCurrentStoryDialogue();
        }

        private void EnsureStoryDialoguePanelBuilt(Transform uiRoot)
        {
            if (uiRoot == null) return;

            if (mayaFullSprite == null) mayaFullSprite = Resources.Load<Sprite>("char_maya_full");
            if (senukFullSprite == null) senukFullSprite = Resources.Load<Sprite>("char_senuk_full");

            if (storyDialoguePanel == null)
            {
                var existing = uiRoot.Find("Panel_StoryDialogue");
                if (existing != null) storyDialoguePanel = existing.gameObject;
            }

            if (storyDialoguePanel != null)
            {
                if (storyCharAvatarImage == null) storyCharAvatarImage = storyDialoguePanel.transform.Find("Avatar_Character")?.GetComponent<Image>();
                if (storySpeakerNameText == null) storySpeakerNameText = storyDialoguePanel.transform.Find("DialogueBubble/HeaderPlate/SpeakerText")?.GetComponent<Text>();
                if (storyDialogueBodyText == null) storyDialogueBodyText = storyDialoguePanel.transform.Find("DialogueBubble/BodyText")?.GetComponent<Text>();
                if (storyNextBtn == null) storyNextBtn = storyDialoguePanel.transform.Find("DialogueBubble/Btn_StoryNext")?.GetComponent<Button>();
                if (storyNextBtnText == null && storyNextBtn != null) storyNextBtnText = storyNextBtn.GetComponentInChildren<Text>();
                return;
            }

            Font sinhalaFont = Resources.Load<Font>("SinhalaFont");
            if (sinhalaFont == null) sinhalaFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Root Story Dialogue Panel (Semi-transparent backdrop)
            storyDialoguePanel = new GameObject("Panel_StoryDialogue", typeof(RectTransform), typeof(Image));
            storyDialoguePanel.transform.SetParent(uiRoot, false);
            var panelRt = (RectTransform)storyDialoguePanel.transform;
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var bgImg = storyDialoguePanel.GetComponent<Image>();
            bgImg.color = new Color(0.04f, 0.05f, 0.06f, 0.40f);

            // 2. Character Avatar on left side
            var avatarGo = new GameObject("Avatar_Character", typeof(RectTransform), typeof(Image));
            avatarGo.transform.SetParent(storyDialoguePanel.transform, false);
            var avatarRt = (RectTransform)avatarGo.transform;
            avatarRt.anchorMin = new Vector2(0f, 0f);
            avatarRt.anchorMax = new Vector2(0f, 0f);
            avatarRt.pivot = new Vector2(0.5f, 0f);
            avatarRt.anchoredPosition = new Vector2(250f, 10f);
            avatarRt.sizeDelta = new Vector2(440f, 660f);

            storyCharAvatarImage = avatarGo.GetComponent<Image>();
            storyCharAvatarImage.preserveAspect = true;
            storyCharAvatarImage.sprite = (activeCharacter == "Senuk" && senukFullSprite != null) ? senukFullSprite : mayaFullSprite;

            // 3. Dialogue Bubble Container
            var bubbleGo = new GameObject("DialogueBubble", typeof(RectTransform), typeof(Image));
            bubbleGo.transform.SetParent(storyDialoguePanel.transform, false);
            var bubbleRt = (RectTransform)bubbleGo.transform;
            bubbleRt.anchorMin = new Vector2(0f, 0f);
            bubbleRt.anchorMax = new Vector2(1f, 0f);
            bubbleRt.pivot = new Vector2(0.5f, 0f);
            bubbleRt.offsetMin = new Vector2(450f, 30f);
            bubbleRt.offsetMax = new Vector2(-50f, 380f);

            var bubbleImg = bubbleGo.GetComponent<Image>();
            var bubbleSprite = Resources.Load<Sprite>("dialog_bubble_frame");
            if (bubbleSprite != null)
            {
                bubbleImg.sprite = bubbleSprite;
                bubbleImg.type = Image.Type.Sliced;
                bubbleImg.color = Color.white;
            }
            else
            {
                bubbleImg.color = new Color(0.12f, 0.09f, 0.05f, 0.94f);
            }

            var bubbleOutline = bubbleGo.AddComponent<Outline>();
            bubbleOutline.effectColor = new Color(0.95f, 0.78f, 0.28f, 0.9f);
            bubbleOutline.effectDistance = new Vector2(3f, -3f);

            // 4. Header / Speaker Tag Plate
            var headerGo = new GameObject("HeaderPlate", typeof(RectTransform), typeof(Image));
            headerGo.transform.SetParent(bubbleGo.transform, false);
            var headerRt = (RectTransform)headerGo.transform;
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(0f, 1f);
            headerRt.pivot = new Vector2(0f, 1f);
            headerRt.anchoredPosition = new Vector2(28f, -14f);
            headerRt.sizeDelta = new Vector2(400f, 46f);

            var headerImg = headerGo.GetComponent<Image>();
            headerImg.color = new Color(0.85f, 0.62f, 0.16f, 0.95f);
            var headerOutline = headerGo.AddComponent<Outline>();
            headerOutline.effectColor = new Color(0.3f, 0.15f, 0.02f, 0.8f);
            headerOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var nameTxtGo = new GameObject("SpeakerText", typeof(RectTransform), typeof(Text));
            nameTxtGo.transform.SetParent(headerGo.transform, false);
            var nameTxtRt = (RectTransform)nameTxtGo.transform;
            nameTxtRt.anchorMin = Vector2.zero;
            nameTxtRt.anchorMax = Vector2.one;
            nameTxtRt.offsetMin = new Vector2(14f, 2f);
            nameTxtRt.offsetMax = new Vector2(-14f, -2f);

            storySpeakerNameText = nameTxtGo.GetComponent<Text>();
            storySpeakerNameText.font = sinhalaFont;
            storySpeakerNameText.fontSize = 20;
            storySpeakerNameText.fontStyle = FontStyle.Bold;
            storySpeakerNameText.alignment = TextAnchor.MiddleLeft;
            storySpeakerNameText.color = new Color(0.12f, 0.07f, 0.01f, 1f);

            // 5. Dialogue Body Text
            var bodyGo = new GameObject("BodyText", typeof(RectTransform), typeof(Text));
            bodyGo.transform.SetParent(bubbleGo.transform, false);
            var bodyRt = (RectTransform)bodyGo.transform;
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(36f, 85f);
            bodyRt.offsetMax = new Vector2(-36f, -68f);

            storyDialogueBodyText = bodyGo.GetComponent<Text>();
            storyDialogueBodyText.font = sinhalaFont;
            storyDialogueBodyText.fontSize = 26;
            storyDialogueBodyText.fontStyle = FontStyle.Normal;
            storyDialogueBodyText.alignment = TextAnchor.UpperLeft;
            storyDialogueBodyText.lineSpacing = 1.25f;
            storyDialogueBodyText.supportRichText = true;
            storyDialogueBodyText.color = new Color(1f, 0.96f, 0.88f, 1f);

            // 6. Next Button
            var nextBtnGo = new GameObject("Btn_StoryNext", typeof(RectTransform), typeof(Image), typeof(Button));
            nextBtnGo.transform.SetParent(bubbleGo.transform, false);
            var nextRt = (RectTransform)nextBtnGo.transform;
            nextRt.anchorMin = new Vector2(1f, 0f);
            nextRt.anchorMax = new Vector2(1f, 0f);
            nextRt.pivot = new Vector2(1f, 0f);
            nextRt.anchoredPosition = new Vector2(-30f, 18f);
            nextRt.sizeDelta = new Vector2(250f, 58f);

            var nextImg = nextBtnGo.GetComponent<Image>();
            var nextSprite = Resources.Load<Sprite>("btn_dialog_next");
            if (nextSprite != null)
            {
                nextImg.sprite = nextSprite;
                nextImg.type = Image.Type.Sliced;
                nextImg.color = Color.white;
            }
            else
            {
                nextImg.color = new Color(0.92f, 0.70f, 0.15f, 1f);
            }
            var nextOutline = nextBtnGo.AddComponent<Outline>();
            nextOutline.effectColor = new Color(0.25f, 0.12f, 0.02f, 0.9f);
            nextOutline.effectDistance = new Vector2(2f, -2f);

            storyNextBtn = nextBtnGo.GetComponent<Button>();
            storyNextBtn.onClick.AddListener(OnStoryNextDialogueClicked);

            var nextTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            nextTxtGo.transform.SetParent(nextBtnGo.transform, false);
            var nextTxtRt = (RectTransform)nextTxtGo.transform;
            nextTxtRt.anchorMin = Vector2.zero;
            nextTxtRt.anchorMax = Vector2.one;
            nextTxtRt.offsetMin = nextTxtRt.offsetMax = Vector2.zero;

            storyNextBtnText = nextTxtGo.GetComponent<Text>();
            storyNextBtnText.font = sinhalaFont;
            storyNextBtnText.fontSize = 22;
            storyNextBtnText.fontStyle = FontStyle.Bold;
            storyNextBtnText.alignment = TextAnchor.MiddleCenter;
            storyNextBtnText.color = new Color(0.12f, 0.07f, 0.02f, 1f);
            storyNextBtnText.text = "NEXT ▶";

            // 7. Skip Button
            var skipBtnGo = new GameObject("Btn_StorySkip", typeof(RectTransform), typeof(Button));
            skipBtnGo.transform.SetParent(bubbleGo.transform, false);
            var skipRt = (RectTransform)skipBtnGo.transform;
            skipRt.anchorMin = new Vector2(1f, 1f);
            skipRt.anchorMax = new Vector2(1f, 1f);
            skipRt.pivot = new Vector2(1f, 1f);
            skipRt.anchoredPosition = new Vector2(-25f, -14f);
            skipRt.sizeDelta = new Vector2(150f, 40f);

            var skipBtn = skipBtnGo.GetComponent<Button>();
            skipBtn.onClick.AddListener(FinishGuidedStoryDialogueAndUnlockControls);

            var skipTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            skipTxtGo.transform.SetParent(skipBtnGo.transform, false);
            var skipTxtRt = (RectTransform)skipTxtGo.transform;
            skipTxtRt.anchorMin = Vector2.zero;
            skipTxtRt.anchorMax = Vector2.one;
            skipTxtRt.offsetMin = skipTxtRt.offsetMax = Vector2.zero;

            var skipTxt = skipTxtGo.GetComponent<Text>();
            skipTxt.font = sinhalaFont;
            skipTxt.fontSize = 17;
            skipTxt.fontStyle = FontStyle.Normal;
            skipTxt.alignment = TextAnchor.MiddleRight;
            skipTxt.color = new Color(0.85f, 0.82f, 0.72f, 0.85f);
            skipTxt.text = "SKIP ⏭";
        }

        private void DisplayCurrentStoryDialogue()
        {
            if (currentStoryDialogueIndex < 0 || currentStoryDialogueIndex >= storyDialogueSteps.Length)
            {
                FinishGuidedStoryDialogueAndUnlockControls();
                return;
            }

            var step = storyDialogueSteps[currentStoryDialogueIndex];

            if (mayaFullSprite == null) mayaFullSprite = Resources.Load<Sprite>("char_maya_full");
            if (senukFullSprite == null) senukFullSprite = Resources.Load<Sprite>("char_senuk_full");

            if (storyCharAvatarImage != null)
            {
                storyCharAvatarImage.sprite = (activeCharacter == "Senuk" && senukFullSprite != null) ? senukFullSprite : mayaFullSprite;
            }

            string charName = (activeCharacter == "Senuk") ? "Senuk" : "Maya";
            if (storySpeakerNameText != null)
            {
                storySpeakerNameText.text = $"👤 {charName} — {step.title}";
            }

            if (storyDialogueBodyText != null)
            {
                storyDialogueBodyText.text = step.sinhalaText;
            }

            if (storyNextBtnText != null)
            {
                storyNextBtnText.text = step.buttonText;
            }

            if (!string.IsNullOrEmpty(step.highlightPart))
            {
                HighlightConcept(step.highlightPart);
            }

            var bubble = storyDialoguePanel != null ? storyDialoguePanel.transform.Find("DialogueBubble") : null;
            if (bubble != null)
            {
                StopCoroutine("AnimateSpeechBubble");
                StartCoroutine(AnimateSpeechBubble(bubble));
            }
        }

        public void OnStoryNextDialogueClicked()
        {
            PlaySound(clickClip);
            currentStoryDialogueIndex++;
            if (currentStoryDialogueIndex < storyDialogueSteps.Length)
            {
                DisplayCurrentStoryDialogue();
            }
            else
            {
                FinishGuidedStoryDialogueAndUnlockControls();
            }
        }

        public void FinishGuidedStoryDialogueAndUnlockControls()
        {
            PlaySound(clickClip);
            isGuidedStoryFlowActive = false;
            microQuizTimer = 0f;
            microQuizEnabled = true;

            if (storyDialoguePanel != null)
            {
                storyDialoguePanel.SetActive(false);
            }

            if (bottomConsole != null)
            {
                bottomConsole.SetActive(true);
            }

            AudioClip greetingClip = (activeCharacter == "Senuk") ? senukBalancedClip : mayaBalancedClip;
            if (greetingClip != null)
            {
                PlaySound(greetingClip);
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
