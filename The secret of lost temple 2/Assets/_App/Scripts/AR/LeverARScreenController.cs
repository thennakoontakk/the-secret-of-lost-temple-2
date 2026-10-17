using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        private bool isChallengeActive = false;
        public bool IsChallengeActive => isChallengeActive;
        private string activeCharacter = "Maya";
        private int lastVoiceState = -1;

        // Exploded View smooth transform targets
        private bool isExplodedView = false;
        private Vector3 fulcrumAssembledLocalPos = Vector3.zero;
        private Vector3 beamAssembledLocalPos = new Vector3(0f, 0.38f, 0f);
        private Vector3 boulderAssembledLocalPos = new Vector3(-0.90f, 0.15f, 0f);
        private Vector3 effortAssembledLocalPos = new Vector3(0.90f, 0.15f, 0f);

        private Vector3 fulcrumExplodedLocalPos = new Vector3(0f, -0.05f, 0f);
        private Vector3 beamExplodedLocalPos = new Vector3(0f, 0.44f, 0f);
        private Vector3 boulderExplodedLocalPos = new Vector3(-0.95f, 0.56f, 0f);
        private Vector3 effortExplodedLocalPos = new Vector3(0.95f, 0.56f, 0f);

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
            CaptureInitialObjectPositions();

            // Requirement 1 & 2: Hide "How It Works" tablet and Companion Avatar by default for a clean AR screen
            if (howItWorksPanel != null) howItWorksPanel.SetActive(false);
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
            if (howItWorksPanel != null) howItWorksPanel.SetActive(false);
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
            if (howItWorksPanel != null) howItWorksPanel.SetActive(false);
            if (characterCompanionImage != null) characterCompanionImage.gameObject.SetActive(false);
            if (liftStatusBannerText != null && liftStatusBannerText.transform.parent != null)
            {
                liftStatusBannerText.transform.parent.gameObject.SetActive(false);
            }
            if (challengeModalPanel != null) challengeModalPanel.SetActive(false);
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
                    templeEnv.transform.position = new Vector3(0f, 0.28f, 0f);
                    templeEnv.transform.rotation = Quaternion.identity;
                    templeEnv.transform.localScale = Vector3.one;
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
                if (scanOverlay != null) scanOverlay.SetActive(false);

                if (tryChallengeButton != null) tryChallengeButton.gameObject.SetActive(false);
                if (continueStoryVictoryButton != null) continueStoryVictoryButton.gameObject.SetActive(false);
                if (challengeModalPanel != null) challengeModalPanel.SetActive(false);
                if (btnSmartHint != null) btnSmartHint.gameObject.SetActive(false);
                if (smartHintModalPanel != null) smartHintModalPanel.SetActive(false);
                if (btnARQuiz != null) btnARQuiz.gameObject.SetActive(false);
                if (quizModalPanel != null) quizModalPanel.SetActive(false);
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
            var uiRoot = canvasTr.Find("LeverAR_UIRoot");
            if (uiRoot != null)
            {
                EnsureChallengeModalBuilt(uiRoot);
                if (howItWorksPanel == null) howItWorksPanel = uiRoot.Find("Panel_HowItWorks")?.gameObject;
                if (toggleHowItWorksBtn == null) toggleHowItWorksBtn = uiRoot.Find("TopBar/Btn_HowItWorks")?.GetComponent<Button>();
                EnsureHowItWorksToggleButtonBuilt(uiRoot);

                if (backOrSettingsButton == null) backOrSettingsButton = uiRoot.Find("TopBar/Btn_Back")?.GetComponent<Button>();
                if (modeToggleButton == null) modeToggleButton = uiRoot.Find("TopBar/Btn_ModeToggle")?.GetComponent<Button>();
                if (modeToggleText == null) modeToggleText = uiRoot.Find("TopBar/Btn_ModeToggle/Text")?.GetComponent<Text>();
                if (modeToggleBg == null) modeToggleBg = uiRoot.Find("TopBar/Btn_ModeToggle")?.GetComponent<Image>();

                if (viewOnlyBottomBar == null) viewOnlyBottomBar = uiRoot.Find("ViewOnlyBottomBar")?.gameObject;
                if (btnForcesAndMotion == null) btnForcesAndMotion = uiRoot.Find("ViewOnlyBottomBar/Btn_ForcesAndMotion")?.GetComponent<Button>();
                if (btnExplodedParts == null) btnExplodedParts = uiRoot.Find("ViewOnlyBottomBar/Btn_ExplodedParts")?.GetComponent<Button>();
                if (imgForcesHighlight == null) imgForcesHighlight = uiRoot.Find("ViewOnlyBottomBar/Btn_ForcesAndMotion")?.GetComponent<Image>();
                if (imgExplodedHighlight == null) imgExplodedHighlight = uiRoot.Find("ViewOnlyBottomBar/Btn_ExplodedParts")?.GetComponent<Image>();

                if (explodedPartsPillsContainer == null) explodedPartsPillsContainer = uiRoot.Find("Pills_ExplodedParts")?.gameObject;
                if (pillFulcrumBtn == null) pillFulcrumBtn = uiRoot.Find("Pills_ExplodedParts/Pill_Fulcrum")?.GetComponent<Button>();
                if (pillBeamBtn == null) pillBeamBtn = uiRoot.Find("Pills_ExplodedParts/Pill_Beam")?.GetComponent<Button>();
                if (pillLoadStoneBtn == null) pillLoadStoneBtn = uiRoot.Find("Pills_ExplodedParts/Pill_LoadStone")?.GetComponent<Button>();
                if (pillEffortBtn == null) pillEffortBtn = uiRoot.Find("Pills_ExplodedParts/Pill_Effort")?.GetComponent<Button>();

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
                // Ensure Lever_Beam rests cleanly on the apex of Fulcrum_Pivot (y = 0.38f) without piercing
                beamPivot.localPosition = new Vector3(beamPivot.localPosition.x, 0.38f, beamPivot.localPosition.z);
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
            // Requirement 1: If user taps anywhere outside howItWorksPanel while it is open, close it
            if (howItWorksPanel != null && howItWorksPanel.activeSelf && Input.GetMouseButtonDown(0))
            {
                var panelRt = howItWorksPanel.GetComponent<RectTransform>();
                var btnRt = toggleHowItWorksBtn != null ? toggleHowItWorksBtn.GetComponent<RectTransform>() : null;
                Vector2 mousePos = Input.mousePosition;
                bool insidePanel = panelRt != null && RectTransformUtility.RectangleContainsScreenPoint(panelRt, mousePos, null);
                bool insideToggle = btnRt != null && RectTransformUtility.RectangleContainsScreenPoint(btnRt, mousePos, null);
                if (!insidePanel && !insideToggle)
                {
                    howItWorksPanel.SetActive(false);
                }
            }

            // Track user activity for Abeywickrama's Smart Hint & Engagement Tracker
            if (StudyPolicy.Measured && !StudyPolicy.Tracking) return;
            if (researchForeground && quizModalPanel != null && quizModalPanel.activeSelf && !researchAnswerSubmitted) researchQuestionSeconds += Time.unscaledDeltaTime;
            if (pendingFulcrumLog >= 0 && Time.unscaledTime - pendingFulcrumLog > .4f) {
                ResearchData.Record("fulcrum_committed", fulcrumOffset.ToString(System.Globalization.CultureInfo.InvariantCulture)); pendingFulcrumLog = -1;
            }
            if (StudyPolicy.Passive) {
                demonstrationTime += Time.deltaTime;
                fulcrumOffset = 0f;
                currentEffortForceN = 80f + 50f * Mathf.Sin(demonstrationTime * .7f);
                targetBeamAngle = currentEffortForceN > 105f ? -12f : currentEffortForceN < 95f ? 6f : 0f;
                UpdateForceUI();
                if (liftStatusBannerText != null) liftStatusBannerText.text = "Standard demonstration: equal arms, load 100 N. Effort " + currentEffortForceN.ToString("F0") + " N. Above 100 N the load rises (ideal lever).";
            }
            if (Input.anyKeyDown || Input.touchCount > 0)
            {
                lastInteractionTimestamp = Time.time;
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
                    new Vector3(fulcrumOffset, fulcrumExplodedLocalPos.y, fulcrumExplodedLocalPos.z) :
                    new Vector3(fulcrumOffset, fulcrumAssembledLocalPos.y, fulcrumAssembledLocalPos.z);
                fulcrumRoot.localPosition = Vector3.Lerp(fulcrumRoot.localPosition, targetPos, speed);
            }

            if (beamPivot != null)
            {
                Vector3 targetPos = isExplodedView ? beamExplodedLocalPos : beamAssembledLocalPos;
                if (!isExplodedView) targetPos += new Vector3(fulcrumOffset, 0, 0) - beamPivot.localRotation * new Vector3(fulcrumOffset, 0, 0);
                beamPivot.localPosition = Vector3.Lerp(beamPivot.localPosition, targetPos, speed);
            }

            if (boulderRoot != null)
            {
                Vector3 targetPos = isExplodedView ? boulderExplodedLocalPos : boulderAssembledLocalPos;
                boulderRoot.localPosition = Vector3.Lerp(boulderRoot.localPosition, targetPos, speed);
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
            UpdateForceArrow(ref effortVectorObject, ref effortVectorText, "Vector_EffortForce", effortPoint,
                -up, new Color(1f, .85f, .2f), "EFFORT " + currentEffortForceN.ToString("F0") + " N", scale, cam);
            UpdateForceArrow(ref loadVectorObject, ref loadVectorText, "Vector_LoadWeight", boulderRoot,
                -up, new Color(1f, .35f, .35f), "LOAD 100 N", scale, cam);
            UpdateForceArrow(ref fulcrumVectorObject, ref fulcrumVectorText, "Vector_FulcrumReaction", fulcrumRoot,
                up, new Color(.4f, 1f, .6f), "SUPPORT " + (100f + currentEffortForceN).ToString("F0") + " N", scale, cam);
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
            Vector3 direction, Color color, string caption, float scale, Camera cam)
        {
            if (anchor == null) return;
            if (root == null) {
                root = new GameObject(name).transform;
                root.SetParent(forceVectorsRoot.transform, false);
            }
            root.gameObject.SetActive(true);
            root.position = anchor.position;
            var line = root.GetComponent<LineRenderer>();
            if (line == null) {
                line = root.gameObject.AddComponent<LineRenderer>();
                line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            }
            line.useWorldSpace = true; line.positionCount = 5;
            float unit = cam != null ? WorldUnitsPerPixel(cam, anchor.position) : .001f;
            float length = Mathf.Max(.20f * scale, unit * 24f);
            Vector3 tip = anchor.position;
            Vector3 tail = tip - direction * length;
            Vector3 side = cam != null ? cam.transform.right : leverRoot.right;
            line.startWidth = line.endWidth = Mathf.Max(.006f * scale, unit * 1.6f);
            line.startColor = line.endColor = color;
            line.SetPosition(0, tail); line.SetPosition(1, tip);
            line.SetPosition(2, tip - direction * length * .22f + side * length * .13f);
            line.SetPosition(3, tip); line.SetPosition(4, tip - direction * length * .22f - side * length * .13f);
            if (label == null) {
                label = new GameObject("ForceLabel").AddComponent<TextMesh>();
                label.transform.SetParent(root, false);
                label.fontSize = 48; label.characterSize = 1f;
                label.fontStyle = FontStyle.Bold;
                label.anchor = TextAnchor.MiddleCenter;
            }
            label.text = caption; label.color = color;
            label.transform.position = tail - direction * unit * 12f;
            if (cam != null) label.transform.rotation = cam.transform.rotation;
            SetWorldScale(label.transform, Vector3.one * unit * 8.0f);
        }

        private void HandleDirect3DPartClicks()
        {
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed) return;
            if (currentMode != InteractionMode.ViewOnly || currentViewOnlySubMode != ViewOnlySubMode.ExplodedParts) return;

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main != null ? Camera.main.ScreenPointToRay(Input.mousePosition) : default;
                if (ray.direction != Vector3.zero && Physics.Raycast(ray, out RaycastHit hit))
                {
                    Transform hitTr = hit.transform;
                    if (hitTr == fulcrumRoot || hitTr.IsChildOf(fulcrumRoot))
                    {
                        ShowPartDetails("fulcrum");
                    }
                    else if (hitTr == boulderRoot || hitTr.IsChildOf(boulderRoot))
                    {
                        ShowPartDetails("load");
                    }
                    else if (hitTr == effortPoint || hitTr.IsChildOf(effortPoint))
                    {
                        ShowPartDetails("effort");
                    }
                    else if (hitTr == beamPivot || hitTr.IsChildOf(beamPivot))
                    {
                        ShowPartDetails("beam");
                    }
                }
            }
        }

        public void SetInteractionMode(InteractionMode mode)
        {
            mode = StudyPolicy.Resolve(mode);
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
                if (explodedPartsPillsContainer != null) explodedPartsPillsContainer.SetActive(true);

                if (imgForcesHighlight != null) imgForcesHighlight.color = new Color(0.4f, 0.35f, 0.3f, 0.7f);
                if (imgExplodedHighlight != null) imgExplodedHighlight.color = new Color(0.2f, 0.9f, 1f, 1f);

                if (liftStatusBannerText != null)
                {
                    liftStatusBannerText.text = "<color=#00e5ff><b>🔍 EXPLODED VIEW: ANATOMY OF A LEVER</b></color>\n<size=17>Tap any separated component below or in 3D to learn its physics role & temple lore!</size>";
                }
            }
        }

        public void ShowPartDetails(string partKey)
        {
            PlaySound(partInspectClip != null ? partInspectClip : clickClip);

            if (partDetailModalPanel == null) return;
            partDetailModalPanel.SetActive(true);

            switch (partKey.ToLower())
            {
                case "fulcrum":
                    if (partDetailTitleText != null) partDetailTitleText.text = "🏔️ FULCRUM (ආධාරක ලක්ෂ්‍යය)";
                    if (partDetailCategoryText != null) partDetailCategoryText.text = "Class 1 Lever Central Pivot Point";
                    if (partDetailBodyText != null) partDetailBodyText.text = "The fixed point on which the lever beam balances and turns. It bears the entire weight of both the load and the applied effort forces.";
                    if (partDetailFormulaText != null) partDetailFormulaText.text = "<b>Mechanical Advantage:</b> MA = Effort Arm ÷ Load Arm\n<i>Moving fulcrum closer to load multiplies lifting strength!</i>";
                    if (partDetailLoreText != null) partDetailLoreText.text = "<b>Temple Architecture Lore:</b> Ancient stone masons carved triangular granite fulcrum wedges to pry massive megalithic gate lintels into place.";
                    break;

                case "beam":
                    if (partDetailTitleText != null) partDetailTitleText.text = "🪵 LEVER BEAM (ලිවර දණ්ඩ)";
                    if (partDetailCategoryText != null) partDetailCategoryText.text = "Rigid Rotational Force Transmitter";
                    if (partDetailBodyText != null) partDetailBodyText.text = "A rigid bar that transmits rotational torque (moment of force) from the effort side to the load side across the fulcrum pivot.";
                    if (partDetailFormulaText != null) partDetailFormulaText.text = "<b>Torque Principle:</b> τ = Force × Distance (F × d)\n<i>Equal torque balances the lever: (F₁ × d₁) = (F₂ × d₂)</i>";
                    if (partDetailLoreText != null) partDetailLoreText.text = "<b>Temple Architecture Lore:</b> Beams were shaped from mountain cedar and banded with bronze to withstand hundreds of thousands of Newtons of bending stress.";
                    break;

                case "load":
                    if (partDetailTitleText != null) partDetailTitleText.text = "🪨 LOAD STONE (භාරය - 100 N)";
                    if (partDetailCategoryText != null) partDetailCategoryText.text = "Gravitational Resistance / Output Mass";
                    if (partDetailBodyText != null) partDetailBodyText.text = "The output resistance force to be lifted or moved. In this temple chamber, it is a 100 N carved stone seal blocking the secret sanctuary doorway.";
                    if (partDetailFormulaText != null) partDetailFormulaText.text = "<b>Load Torque:</b> τ_load = 100 N × Load Arm Distance\n<i>Required Effort = Load Force ÷ Mechanical Advantage</i>";
                    if (partDetailLoreText != null) partDetailLoreText.text = "<b>Temple Architecture Lore:</b> These multi-ton stone doors sealed ancient reliquaries. No human could lift them by bare hands without the leverage of simple machines.";
                    break;

                case "effort":
                    if (partDetailTitleText != null) partDetailTitleText.text = "⚡ EFFORT POINT (ආයාස ලක්ෂ්‍යය)";
                    if (partDetailCategoryText != null) partDetailCategoryText.text = "Input Force Application Handle";
                    if (partDetailBodyText != null) partDetailBodyText.text = "The location where the explorer applies downward muscle or weight force to operate the mechanism.";
                    if (partDetailFormulaText != null) partDetailFormulaText.text = "<b>Effort Torque:</b> τ_effort = Applied Force × Effort Arm Distance\n<i>Longer effort arm = less muscle force needed!</i>";
                    if (partDetailLoreText != null) partDetailLoreText.text = "<b>Temple Architecture Lore:</b> By extending the effort arm, a single explorer like Maya or Senuk can easily lift stone blocks three times their own body weight!";
                    break;
            }

            CognitiveTelemetryLogger.Instance?.LogInteraction("inspect_part", null, $"{{\"part\":\"{partKey}\"}}");
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
                bottomConsole.SetActive(isInteractive); // HIDE IN VIEW-ONLY, SHOW IN INTERACTIVE!
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
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed) return;
            hasUserInteractedWithPhysics = true;
            PlaySound(forceChangeClip != null ? forceChangeClip : clickClip);
            currentEffortForceN = Mathf.Clamp(currentEffortForceN + deltaN, 0f, 160f);
            forceChangeCount++;

            CalculatePhysicsAndAnimate();
            UpdateForceUI();

            CognitiveTelemetryLogger.Instance?.LogInteraction("adjust_force", null, $"{{\"forceN\":{currentEffortForceN}}}");
        }

        public void MoveFulcrumStep(float delta)
        {
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed) return;
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
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed) return;
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

            if (currentMode == InteractionMode.Interactive)
            {
                if (netTorque > 5f)
                {
                    targetBeamAngle = -12f;
                    string msg = $"<color=#00ff88><b>🎉 {charName}: \"100 N BOULDER LIFTED!\"</b></color>\n<size=18>Required: {Mathf.CeilToInt(requiredForceN)} N | Applied: {Mathf.RoundToInt(currentEffortForceN)} N | MA: {mechanicalAdvantage:F1}x</size>";
                    if (liftStatusBannerText != null) liftStatusBannerText.text = msg;

                    if (lastVoiceState != 0)
                    {
                        lastVoiceState = 0;
                        if (hasUserInteractedWithPhysics)
                        {
                            PlayCompanionReaction(0, msg);
                        }

                        if (PlayerPrefs.GetString("LostTemple_App_Mode", "Sandbox") == "Quest")
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
                    targetBeamAngle = 6f;
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
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed) return;
            if (leverRoot != null) leverRoot.GetComponent<ARMechanismViewer>()?.RotateBy(30f);
        }

        public void ResetMechanism()
        {
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed) return;
            PlaySound(clickClip);
            currentEffortForceN = 60f;
            fulcrumOffset = 0f;
            currentBeamAngle = 0f;
            targetBeamAngle = 0f;
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
            if (uiRoot == null || toggleHowItWorksBtn != null) return;
            var topBar = uiRoot.Find("TopBar");
            if (topBar == null) return;

            var existing = topBar.Find("Btn_HowItWorks");
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

        private void GetChallengeParametersForTier(out int tier, out float targetLoadN, out float maxAllowedEffortN, out string tierLabel)
        {
            bool useAdaptive = !StudyPolicy.Measured || (ResearchData.Store.Current != null && ResearchData.Store.Current.adaptive);
            var engine = SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance;
            tier = useAdaptive ? (engine != null ? (int)engine.CurrentTier : ResearchProgress.GetInt("AdaptiveDifficultyTier", 1)) : 2;
            if (tier <= 1)
            {
                targetLoadN = 100f;
                maxAllowedEffortN = 60f;
                tierLabel = "Tier 1 (Apprentice)";
            }
            else if (tier == 2)
            {
                targetLoadN = 100f;
                maxAllowedEffortN = 40f;
                tierLabel = "Tier 2 (Scholar)";
            }
            else
            {
                targetLoadN = 120f;
                maxAllowedEffortN = 40f;
                tierLabel = "Tier 3 (Master)";
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

                // Stream completion data to research dashboard
                ResearchData.Record("lever_challenge_solved", $"tier={tier};ma={ma:F2};load={targetLoadN:F0};effort={currentEffortForceN:F0}");
                ResearchCloudSync.Instance?.TriggerImmediateSync("Lever Challenge Completed!");

                if (challengeFeedbackText != null)
                {
                    challengeFeedbackText.text = $"<color=#00ff88><b>🎉 CHALLENGE COMPLETE ({tierLabel})!</b> MA: <b>{ma:F1}x</b> | Lifted {targetLoadN:F0}N with {Mathf.RoundToInt(currentEffortForceN)}N! Tap <b>CONTINUE STORY ➡</b> to advance!</color>";
                }
                if (celebrationParticles != null)
                {
                    celebrationParticles.Play();
                }

                // Requirement 4: Hide tryChallengeButton and activate golden Btn_ContinueStory in its exact position on the bottom console!
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
                    StartCoroutine(AnimateBoulderRoll());
                }

                if (PlayerPrefs.GetString("LostTemple_App_Mode", "Sandbox") == "Quest")
                {
                    string victoryMsg = $"<color=#00ff88><b>🎉 {targetLoadN:F0} N BOULDER LIFTED!</b></color>\n<size=18><b>LEVER TASK COMPLETE! Tap 'CONTINUE STORY ➡' to finish your StoryBook!</b></size>";
                    PlayCompanionReaction(0, victoryMsg);
                    ResearchData.Record("lever_lifted", "lever");
                }

                CognitiveTelemetryLogger.Instance?.LogInteraction("challenge_completed", null, $"{{\"attempts\":{challengeAttempts},\"ma\":{ma:F2},\"tier\":{tier}}}");
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
                float t = elapsed / duration;
                boulderRoot.localPosition = startPos + new Vector3(-0.4f * t, 0.35f * Mathf.Sin(t * Mathf.PI), 0f);
                boulderRoot.localRotation = startRot * Quaternion.Euler(0f, 0f, 180f * t);
                yield return null;
            }
        }

        // ==========================================
        // PATHIRANA: IN-SITU AR ASSESSMENT & REVIEW
        // ==========================================

        [Serializable]
        private struct InSituQuizQuestion
        {
            public string category;
            public string question;
            public string[] options;
            public int correctIndex;
            public string explanation;
            public string targetPart;
        }

        private InSituQuizQuestion[] quizQuestions = new InSituQuizQuestion[]
        {
            new InSituQuizQuestion {
                category = "Mechanical Advantage & Force Calculation",
                question = "The stone door weighs 120 N. If the Effort Arm is 3x longer than the Load Arm (MA = 3), what force is needed to lift it?",
                options = new string[] { "A) 40 N  (120 N ÷ 3)", "B) 120 N  (Equal force)", "C) 360 N  (120 N × 3)", "D) 10 N" },
                correctIndex = 0,
                explanation = "Mechanical Advantage = Effort Arm ÷ Load Arm = 3. Required Effort = Load ÷ MA = 120 N ÷ 3 = 40 N.",
                targetPart = "effort"
            },
            new InSituQuizQuestion {
                category = "Fulcrum Position & Leverage",
                question = "Where should the explorer slide the triangular fulcrum to hoist the megalithic door with LEAST effort?",
                options = new string[] { "A) Closer to the heavy Stone (Load)", "B) Exactly in the middle", "C) Closer to the handle (Effort)", "D) At the very end of the beam" },
                correctIndex = 0,
                explanation = "Placing the fulcrum closer to the Load lengthens the Effort Arm, multiplying lifting strength!",
                targetPart = "fulcrum"
            },
            new InSituQuizQuestion {
                category = "Torque Equilibrium Principle",
                question = "What happens to the required muscle force as you push further away from the fulcrum pivot point?",
                options = new string[] { "A) Required Force Decreases (Easier)", "B) Required Force Increases (Harder)", "C) Force stays exactly the same", "D) Lever loses balance" },
                correctIndex = 0,
                explanation = "Torque = Force × Distance (τ = F × d). A longer distance produces greater rotational torque with less effort.",
                targetPart = "beam"
            },
            new InSituQuizQuestion {
                category = "Class 1 Lever Anatomy",
                question = "In this temple Class 1 Lever, which crucial component represents the Load being lifted by the beam?",
                options = new string[] { "A) Heavy Stone Boulder (Load)", "B) Fulcrum Pivot Point", "C) Effort Handle", "D) Rope Pulley" },
                correctIndex = 0,
                explanation = "In a Class 1 Lever, the heavy stone boulder is the Load resisting the lever's movement and being lifted.",
                targetPart = "load"
            }
        };

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

        public void OpenARQuiz()
        {
            if (StudyPolicy.Measured && (!ResearchData.Store.Current.embeddedAssessment || ResearchData.Store.PendingTest != null)) return;
            researchQuizRun = System.Guid.NewGuid().ToString("N");
            ResearchData.Record("formative_start", researchQuizRun);
            PlaySound(clickClip);
            if (quizModalPanel == null) AutoBindAllReferences();
            if (quizModalPanel == null) return;

            quizModalPanel.SetActive(true);
            currentQuizIndex = 0;
            currentQuizScore = 0;
            quizStartTime = Time.time;

            if (quizNextButton != null)
            {
                quizNextButton.onClick.RemoveAllListeners();
                quizNextButton.onClick.AddListener(NextQuizQuestion);
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
            if (quizModalPanel != null) quizModalPanel.SetActive(false);
        }

        private void DisplayCurrentQuizQuestion()
        {
            if (currentQuizIndex < quizQuestions.Length && (!StudyPolicy.Measured || ResearchData.Store.Current.adaptive)) {
                int tier = ResearchProgress.GetInt("AdaptiveDifficultyTier", 1);
                if (tier == 1) quizQuestions[currentQuizIndex] = new InSituQuizQuestion {
                    category = "Guided lever parts", question = currentQuizIndex % 2 == 0 ? "Which part is the lever's turning point?" : "Which part is the object being lifted?",
                    options = new[] { "The fulcrum", "The load", "The effort", "The beam" }, correctIndex = currentQuizIndex % 2,
                    explanation = "The fulcrum is the pivot. The load is what we lift. Effort is the applied force.", targetPart = currentQuizIndex % 2 == 0 ? "fulcrum" : "load" };
                else quizQuestions[currentQuizIndex] = new InSituQuizQuestion {
                    category = tier == 2 ? "Lever relationships" : "Lever calculation",
                    question = tier == 2 ? "Where should the fulcrum move to reduce effort?" : "An ideal lever has a 3 m effort arm and 1 m load arm. What effort balances a 90 N load?",
                    options = tier == 2 ? new[] { "Toward the effort", "Toward the load", "Remove the pivot", "Off the beam" } : new[] { "90 N", "270 N", "30 N", "10 N" },
                    correctIndex = tier == 2 ? 1 : 2, explanation = tier == 2 ? "A longer effort arm and shorter load arm reduce the required force." : "Balance torques: effort x 3 = 90 x 1, so effort = 30 N.", targetPart = "effort" };
            }
            quizStartTime = Time.time;
            researchAnswerSubmitted = false;
            researchAttempt = 0;
            researchQuestionSeconds = 0;
            if (quizFeedbackPanel != null) quizFeedbackPanel.SetActive(false);
            if (quizNextButton != null) quizNextButton.gameObject.SetActive(false);
            if (btnReviewMode != null) btnReviewMode.gameObject.SetActive(false);

            if (currentQuizIndex < quizQuestions.Length)
            {
                var q = quizQuestions[currentQuizIndex];
                if (quizTitleText != null) quizTitleText.text = $"📝 IN-SITU AR ASSESSMENT ({currentQuizIndex + 1}/{quizQuestions.Length})";
                if (quizCategoryText != null) quizCategoryText.text = $"<b>Topic:</b> {q.category}";
                if (quizQuestionText != null) quizQuestionText.text = q.question;

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
                            if (btnImg != null) btnImg.color = new Color(0.18f, 0.14f, 0.10f, 0.95f);

                            if (quizOptionTexts[i] != null)
                            {
                                quizOptionTexts[i].text = q.options[i];
                                quizOptionTexts[i].color = Color.white;
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
            else
            {
                // Quiz completed - Show summary with Star Badges
                if (quizTitleText != null) quizTitleText.text = "🎉 ASSESSMENT COMPLETED!";
                if (quizCategoryText != null) quizCategoryText.text = "<b>Overall Performance Evaluation</b>";
                float percentage = (float)currentQuizScore / quizQuestions.Length * 100f;
                string starBadge = percentage >= 100f ? "⭐⭐⭐ <b>MASTER EXPLORER!</b>" : (percentage >= 75f ? "⭐⭐☆ <b>SKILLED PHYSICIST!</b>" : "⭐☆☆ <b>APPRENTICE EXPLORER</b>");
                
                if (quizQuestionText != null)
                {
                    quizQuestionText.text = $"<size=22>{starBadge}</size>\n\n" +
                        $"<b>Final Score: {currentQuizScore} / {quizQuestions.Length} ({percentage:F0}%)</b>\n\n" +
                        ResearchData.FormalStatus + "\n\n" +
                        ResearchData.Summary();
                }

                if (quizOptionButtons != null)
                {
                    for (int i = 0; i < quizOptionButtons.Length; i++)
                    {
                        if (quizOptionButtons[i] != null) quizOptionButtons[i].gameObject.SetActive(false);
                    }
                }

                if (btnReviewMode != null) btnReviewMode.gameObject.SetActive(true);
                if (btnRetentionReport != null) btnRetentionReport.gameObject.SetActive(true);
            }
        }

        public void SelectQuizAnswer(int optionIndex)
        {

            if (currentQuizIndex >= quizQuestions.Length || researchAnswerSubmitted) return;
            var q = quizQuestions[currentQuizIndex];
            if (optionIndex < 0 || optionIndex >= q.options.Length) return;
            researchAttempt++;
            bool correct = optionIndex == q.correctIndex;
            float elapsed = researchQuestionSeconds;
            string attemptId = researchQuizRun + ":" + currentQuizIndex + ":" + researchAttempt;
            int tier = ResearchProgress.GetInt("AdaptiveDifficultyTier", 1);
            try { ResearchData.Store.RecordAttempt("lever_formative_" + currentQuizIndex, attemptId, researchAttempt, optionIndex, correct, elapsed, tier, JsonUtility.ToJson(q) + ";review=" + StudyPolicy.Review); }
            catch (System.Exception e) { researchAttempt--; if (quizFeedbackPanel != null) quizFeedbackPanel.SetActive(true); if (quizFeedbackText != null) quizFeedbackText.text = "Answer NOT saved. Please stop and ask the researcher: " + e.Message; return; }
            bool adaptStory = !StudyPolicy.Measured || ResearchData.Store.Current.adaptive;
            if (quizFeedbackPanel != null) quizFeedbackPanel.SetActive(true);
            if (!correct && researchAttempt < 2) {
                lastFailedConceptPart = q.targetPart;
                if (quizFeedbackText != null) quizFeedbackText.text = "<color=#ffdd55>Try once more.</color> " + ConceptHint(q.targetPart);
                ResearchData.Record("hint", q.targetPart + ":level1:wrong_answer:" + attemptId);
                if (adaptStory) ResearchData.Record("story_transition", "gate->support;trigger=" + attemptId + ";rule=lever-pilot-1");
                HighlightConcept(q.targetPart);
                quizStartTime = Time.time;
                researchQuestionSeconds = 0;
                return;
            }
            researchAnswerSubmitted = true;
            if (correct) currentQuizScore++;
            if (quizFeedbackText != null) quizFeedbackText.text = (correct ? "Well done! " : "Let's learn this together. ") + q.explanation +
                (!adaptStory ? "\nMaya: Continue along the temple path." : correct && researchAttempt == 1 ? "\nMaya: You opened the next gate independently." : "\nMaya: Use this explanation, then rejoin the temple path.");
            if (quizOptionButtons != null) foreach (var button in quizOptionButtons) if (button != null) button.interactable = false;
            if (quizNextButton != null) quizNextButton.gameObject.SetActive(true);
            if (adaptStory) {
                ResearchData.Record("story_transition", (correct && researchAttempt == 1 ? "gate->independent" : "support->rejoin") + ";trigger=" + attemptId + ";rule=lever-pilot-1");
                ResearchProgress.SetString("StoryNode", correct && researchAttempt == 1 ? "node_3" : "node_2");
            }
            if (!StudyPolicy.Measured || ResearchData.Store.Current.adaptive)
                SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance?.RecordTaskResult(correct && researchAttempt == 1, elapsed);
            UpdateAdaptiveTierHUD();
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
            if (!researchAnswerSubmitted) return;
            PlaySound(clickClip);
            currentQuizIndex++;
            DisplayCurrentQuizQuestion();
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

        public void UpdateAdaptiveTierHUD()
        {
            var engine = SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance;
            int tier = engine != null ? (int)engine.CurrentTier : ResearchProgress.GetInt("AdaptiveDifficultyTier", 1);

            if (adaptiveTierBadgeText != null)
            {
                switch (tier)
                {
                    case 1:
                        adaptiveTierBadgeText.text = "🎓 <b>APPRENTICE (Tier 1)</b>";
                        break;
                    case 2:
                        adaptiveTierBadgeText.text = "⚔️ <b>SCHOLAR (Tier 2)</b>";
                        break;
                    case 3:
                        adaptiveTierBadgeText.text = "👑 <b>MASTER (Tier 3)</b>";
                        break;
                }
            }
        }

        private void InitializeInteractiveStoryBranchUI()
        {
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
            if (storyChoiceWidget == null) return;
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

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
