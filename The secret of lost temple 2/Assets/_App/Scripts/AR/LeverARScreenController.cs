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
        private Vector3 beamAssembledLocalPos = Vector3.zero;
        private Vector3 boulderAssembledLocalPos = new Vector3(-0.90f, 0.15f, 0f);
        private Vector3 effortAssembledLocalPos = new Vector3(0.90f, 0.15f, 0f);

        private Vector3 fulcrumExplodedLocalPos = new Vector3(0f, -0.45f, -0.25f);
        private Vector3 beamExplodedLocalPos = new Vector3(0f, 0.25f, 0f);
        private Vector3 boulderExplodedLocalPos = new Vector3(-1.45f, 0.65f, 0.25f);
        private Vector3 effortExplodedLocalPos = new Vector3(1.45f, 0.65f, -0.25f);

        // Telemetry
        private int forceChangeCount = 0;
        private int fulcrumChangeCount = 0;
        private int challengeAttempts = 0;
        private float sessionStartTime;
        private Vector3 baseHintScale = Vector3.one;

        private void Awake()
        {
            sessionStartTime = Time.time;
            AutoBindAllReferences();
            if (btnSmartHint != null && btnSmartHint.transform.localScale.sqrMagnitude > 0.01f)
            {
                baseHintScale = btnSmartHint.transform.localScale;
            }
            InitializeCharacterCompanion();
            SetupButtonListeners();
            CaptureInitialObjectPositions();
        }

        private void Start()
        {
            AutoBindAllReferences();
            SetupButtonListeners();
            ResetMechanism();
            // Start in View-Only Mode as requested
            SetInteractionMode(InteractionMode.ViewOnly);
            SetViewOnlySubMode(ViewOnlySubMode.ForcesAndMotion);
            ApplyAppModeVisibility();
        }

        private void ApplyAppModeVisibility()
        {
            // Visibility must never overwrite the assigned interaction condition.
            if (modeToggleButton != null) modeToggleButton.interactable = !StudyPolicy.Measured;
            if (btnExplodedParts != null) btnExplodedParts.interactable = !StudyPolicy.Passive;
            if (btnARQuiz != null) btnARQuiz.gameObject.SetActive(!StudyPolicy.Measured || ResearchData.Store.Current.embeddedAssessment);
            if (btnSmartHint != null) btnSmartHint.gameObject.SetActive(ResearchData.Store.PendingTest == null);
            if (adaptiveTierBadgeText != null && adaptiveTierBadgeText.transform.parent != null) adaptiveTierBadgeText.transform.parent.gameObject.SetActive(true);
        }

        private void AutoBindAllReferences()
        {
            var canvasTr = transform;
            var uiRoot = canvasTr.Find("LeverAR_UIRoot");
            if (uiRoot != null)
            {
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
            if (fulcrumRoot != null) fulcrumAssembledLocalPos = fulcrumRoot.localPosition;
            if (beamPivot != null) beamAssembledLocalPos = beamPivot.localPosition;
            if (boulderRoot != null) boulderAssembledLocalPos = boulderRoot.localPosition;
            if (effortPoint != null) effortAssembledLocalPos = effortPoint.localPosition;
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
                rotateButton.onClick.AddListener(OnRotateClicked);

            if (forceDecreaseBtn != null)
                forceDecreaseBtn.onClick.AddListener(() => AdjustForce(-20f));

            if (forceIncreaseBtn != null)
                forceIncreaseBtn.onClick.AddListener(() => AdjustForce(+20f));

            if (fulcrumLeftBtn != null)
                fulcrumLeftBtn.onClick.AddListener(() => MoveFulcrumStep(-0.10f));

            if (fulcrumRightBtn != null)
                fulcrumRightBtn.onClick.AddListener(() => MoveFulcrumStep(+0.10f));

            if (fulcrumSlider != null)
                fulcrumSlider.onValueChanged.AddListener(OnFulcrumSliderChanged);

            if (resetButton != null)
                resetButton.onClick.AddListener(ResetMechanism);

            if (tryChallengeButton != null)
                tryChallengeButton.onClick.AddListener(OpenChallengeModal);

            if (challengeCloseBtn != null)
                challengeCloseBtn.onClick.AddListener(CloseChallengeModal);

            if (challengeSubmitBtn != null)
                challengeSubmitBtn.onClick.AddListener(EvaluateChallenge);

            if (toggleHowItWorksBtn != null)
                toggleHowItWorksBtn.onClick.AddListener(ToggleHowItWorks);
        }

        private void Update()
        {
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
            Vector3 leverUp = leverRoot != null ? leverRoot.up : Vector3.up;
            float scaleRatio = leverRoot != null ? (leverRoot.lossyScale.y / 2.0f) : 1f;

            // Keep in-world badges floating above their respective 3D objects
            if (loadAnnotationBadge != null && boulderRoot != null)
                loadAnnotationBadge.transform.position = boulderRoot.position + leverUp * (0.45f * scaleRatio);

            if (fulcrumAnnotationBadge != null && fulcrumRoot != null)
                fulcrumAnnotationBadge.transform.position = fulcrumRoot.position - leverUp * (0.25f * scaleRatio);

            if (effortAnnotationBadge != null && effortPoint != null)
                effortAnnotationBadge.transform.position = effortPoint.position + leverUp * (0.35f * scaleRatio);

            // Animate dynamic 3D Force Direction Arrows when in Forces & Motion view
            bool showForces = (currentMode == InteractionMode.ViewOnly && currentViewOnlySubMode == ViewOnlySubMode.ForcesAndMotion) ||
                              (currentMode == InteractionMode.Interactive);

            if (forceVectorsRoot != null)
            {
                forceVectorsRoot.SetActive(showForces);
            }

            if (showForces)
            {
                float pulse = 0.94f + 0.06f * Mathf.Sin(Time.time * 4f);
                var cam = Camera.main;

                // 1. Effort Vector: Positioned above effort point, pointing DOWNWARDS (Push force)
                if (effortVectorObject != null && effortPoint != null)
                {
                    effortVectorObject.gameObject.SetActive(true);
                    effortVectorObject.position = effortPoint.position + leverUp * (0.32f * pulse * scaleRatio);
                    if (effortVectorText != null)
                    {
                        effortVectorText.text = $"<b>⚡ EFFORT FORCE</b>\n<color=#ffe066>{currentEffortForceN:F0} N ↓</color>";
                        if (cam != null) effortVectorText.transform.rotation = cam.transform.rotation;
                    }
                }

                // 2. Load Vector: Positioned above load boulder, pointing DOWNWARDS (Gravity weight)
                if (loadVectorObject != null && boulderRoot != null)
                {
                    loadVectorObject.gameObject.SetActive(true);
                    loadVectorObject.position = boulderRoot.position + leverUp * (0.34f * pulse * scaleRatio);
                    if (loadVectorText != null)
                    {
                        loadVectorText.text = "<b>🪨 LOAD WEIGHT</b>\n<color=#ff8080>100 N ↓</color>";
                        if (cam != null) loadVectorText.transform.rotation = cam.transform.rotation;
                    }
                }

                // 3. Fulcrum Support Vector: Positioned below fulcrum, pointing UPWARDS (Support normal force)
                if (fulcrumVectorObject != null && fulcrumRoot != null)
                {
                    fulcrumVectorObject.gameObject.SetActive(true);
                    fulcrumVectorObject.position = fulcrumRoot.position - leverUp * (0.30f * pulse * scaleRatio);
                    if (fulcrumVectorText != null)
                    {
                        float reactionN = currentEffortForceN + 100f;
                        fulcrumVectorText.text = $"<b>🏔️ FULCRUM SUPPORT</b>\n<color=#80ffaa>{reactionN:F0} N ↑</color>";
                        if (cam != null) fulcrumVectorText.transform.rotation = cam.transform.rotation;
                    }
                }
            }
            else
            {
                if (effortVectorObject != null) effortVectorObject.gameObject.SetActive(false);
                if (loadVectorObject != null) loadVectorObject.gameObject.SetActive(false);
                if (fulcrumVectorObject != null) fulcrumVectorObject.gameObject.SetActive(false);
            }
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
                    if (liftStatusBannerText != null)
                    {
                        liftStatusBannerText.text = $"<color=#00ff88><b>🎉 {charName}: \"100 N BOULDER LIFTED!\"</b></color>\n<size=18>Required: {Mathf.CeilToInt(requiredForceN)} N | Applied: {Mathf.RoundToInt(currentEffortForceN)} N | MA: {mechanicalAdvantage:F1}x</size>";
                    }
                    if (lastVoiceState != 0)
                    {
                        PlayCharacterVoice(0);
                        lastVoiceState = 0;

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
                }
                else if (netTorque < -5f)
                {
                    targetBeamAngle = 6f;
                    if (liftStatusBannerText != null)
                    {
                        liftStatusBannerText.text = $"<color=#ffaa44><b>⚠️ {charName}: \"TOO HEAVY TO LIFT!\"</b></color>\n<size=18>Need at least <b>{Mathf.CeilToInt(requiredForceN)} N</b> with this Fulcrum position!</size>";
                    }
                    if (lastVoiceState != 1)
                    {
                        PlayCharacterVoice(1);
                        lastVoiceState = 1;
                    }
                }
                else
                {
                    targetBeamAngle = 0f;
                    if (liftStatusBannerText != null)
                    {
                        liftStatusBannerText.text = $"<color=#ffdd44><b>⚖️ {charName}: \"BALANCED EQUILIBRIUM!\"</b></color>\n<size=18>Effort matches Load Torque exactly ({Mathf.RoundToInt(requiredForceN)} N)</size>";
                    }
                    if (lastVoiceState != 2)
                    {
                        PlayCharacterVoice(2);
                        lastVoiceState = 2;
                    }
                }
            }

            if (fulcrumStatusText != null)
            {
                fulcrumStatusText.text = $"<b>{fulcrumOffset:+0.00;-0.00;0.00} m</b>";
            }
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
            if (beamPivot != null) beamPivot.localRotation = Quaternion.identity;

            if (leverRoot != null)
            {
                string mode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
                if (mode == "Sandbox")
                {
                    leverRoot.localRotation = Quaternion.identity;
                    leverRoot.localScale = Vector3.one * 2.0f;
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

        public void OpenChallengeModal()
        {
            if (StudyPolicy.Passive) return;
            challengeResolved = false; challengeStarted = Time.time;
            PlaySound(clickClip);
            isChallengeActive = true;
            challengeAttempts++;

            if (challengeModalPanel != null)
            {
                challengeModalPanel.SetActive(true);
            }

            if (challengeTitleText != null)
                challengeTitleText.text = "⭐ TEMPLE LEVER CHALLENGE";

            if (challengeDescText != null)
                challengeDescText.text = "<b>Mission:</b> Lift the heavy 100N temple boulder using <b>only 40 N or less</b> of applied effort force!\n\n<i>Hint: Adjust the Fulcrum slider closer to the Boulder to increase your Mechanical Advantage!</i>";

            if (challengeFeedbackText != null)
                challengeFeedbackText.text = "Set your configuration and tap <b>Check Solution</b>!";
        }

        public void CloseChallengeModal()
        {
            PlaySound(clickClip);
            if (challengeModalPanel != null)
                challengeModalPanel.SetActive(false);
        }

        public void EvaluateChallenge()
        {
            if (StudyPolicy.Measured && !StudyPolicy.InputAllowed) return;
            float loadArm = Mathf.Abs(-0.90f - fulcrumOffset);
            float effortArm = Mathf.Abs(0.90f - fulcrumOffset);
            float ma = effortArm / loadArm;
            float requiredForce = 100f / ma;

            float loadTorque = 100f * loadArm;
            float effortTorque = currentEffortForceN * effortArm;
            bool isSuccessful = (effortTorque > loadTorque + 5f && currentEffortForceN <= 40f);
            if (challengeResolved) return;
            challengeResolved = isSuccessful;
            ResearchData.Record("challenge_attempt", "lever;success=" + isSuccessful, false, isSuccessful, Time.time - challengeStarted);
            challengeStarted = Time.time;

            if (isSuccessful)
            {
                PlaySound(successClip);

                int currentUnlocked = ResearchProgress.GetInt("LostTemple_Quest_Unlocked_Level", 1);
                if (currentUnlocked < 2)
                {
                    ResearchProgress.SetInt("LostTemple_Quest_Unlocked_Level", 2);
                    PlayerPrefs.Save();
                }

                if (challengeFeedbackText != null)
                {
                    challengeFeedbackText.text = $"<color=#00ff88><b>🎉 BRILLIANT DISCOVERY!</b>\nMechanical Advantage: <b>{ma:F1}x</b>!\nYou successfully hoisted the load with only {Mathf.RoundToInt(currentEffortForceN)}N effort!\n\n<b>LEVER TASK COMPLETE!</b></color>";
                }
                if (celebrationParticles != null)
                {
                    celebrationParticles.Play();
                }

                // Boulder roll & lift animation
                if (boulderRoot != null)
                {
                    StartCoroutine(AnimateBoulderRoll());
                }

                if (PlayerPrefs.GetString("LostTemple_App_Mode", "Sandbox") == "Quest")
                {
                    if (liftStatusBannerText != null)
                    {
                        liftStatusBannerText.text = "<color=#00ff88><b>🎉 100 N BOULDER LIFTED!</b></color>\n<size=18><b>LEVER TASK COMPLETE! Continue learning or return to research.</b></size>";
                    }
                    ResearchData.Record("lever_lifted", "lever");
                }

                CognitiveTelemetryLogger.Instance?.LogInteraction("challenge_completed", null, $"{{\"attempts\":{challengeAttempts},\"ma\":{ma:F2}}}");
            }
            else
            {
                PlaySound(clickClip);
                if (challengeFeedbackText != null)
                {
                    if (ma < 2.0f)
                    {
                        challengeFeedbackText.text = "<color=#ffcc00>⚠️ Try moving the <b>FULCRUM</b> closer to the Boulder (slide left) to make it easier!</color>";
                    }
                    else
                    {
                        challengeFeedbackText.text = $"<color=#ffcc00>⚠️ Mechanical Advantage is {ma:F1}x! Increase Force to at least {Mathf.CeilToInt(requiredForce)} N to lift it!</color>";
                    }
                }
                CognitiveTelemetryLogger.Instance?.LogInteraction("challenge_failed_attempt", null, $"{{\"attempts\":{challengeAttempts},\"ma\":{ma:F2}}}");
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
                question = "In this temple Class 1 Lever, which crucial component is positioned in the middle between Load and Effort?",
                options = new string[] { "A) Fulcrum Pivot Point", "B) Heavy Stone Boulder", "C) Effort Handle", "D) Rope Pulley" },
                correctIndex = 0,
                explanation = "In a Class 1 Lever, the Fulcrum is always positioned between the Load and the Effort (Load — Fulcrum — Effort).",
                targetPart = "fulcrum"
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
            Transform target = part == "load" ? boulderRoot : part == "fulcrum" ? fulcrumRoot : part == "effort" ? effortPoint : beamPivot;
            if (target != null) StartCoroutine(HighlightRoutine(target));
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
        // ABEYWICKRAMA: ADAPTIVE TIER & SMART HINTS
        // ==========================================

        public void UpdateAdaptiveTierHUD()
        {
            var engine = SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance;
            int tier = engine != null ? (int)engine.CurrentTier : 1;

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

        public void TriggerSmartHint()
        {
            if (ResearchData.Store.PendingTest != null) return;
            ResearchData.Record("hint_requested", "lever:" + lastFailedConceptPart);
            HighlightConcept(lastFailedConceptPart);
            PlaySound(clickClip);
            lastInteractionTimestamp = Time.time;
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
            var titleTxt = smartHintModalPanel.transform.Find("ModalCard/TitleText")?.GetComponent<Text>();
            if (titleTxt != null)
            {
                titleTxt.text = $"💬 <b>{charName}'S HINT:</b>";
            }

            if (smartHintBodyText != null)
            {
                if (currentMode == InteractionMode.Interactive)
                {
                    smartHintBodyText.text =
                        "\"To lift the heavy 100 N stone seal, slide the triangular <b>Fulcrum</b> closer to the stone (left)! " +
                        "This lengthens your Effort Arm, multiplying Mechanical Advantage so your force easily lifts the weight!\"";
                }
                else
                {
                    smartHintBodyText.text =
                        "\"Tap <b>'360° Forces & Motion'</b> to see glowing vector arrows showing effort, load, and reaction forces, or tap <b>'Exploded View'</b> to inspect each separate temple mechanism and formula!\"";
                }
            }

            CognitiveTelemetryLogger.Instance?.LogInteraction("smart_hint_opened", null, $"{{\"character\":\"{activeCharacter}\"}}");
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
        }

        public void ProceedToNextChapter()
        {
            SceneManager.LoadScene("HomeMapScene");
        }

        public void OnBackClicked()
        {
            PlaySound(clickClip);
            SceneManager.LoadScene("HomeMapScene");
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
