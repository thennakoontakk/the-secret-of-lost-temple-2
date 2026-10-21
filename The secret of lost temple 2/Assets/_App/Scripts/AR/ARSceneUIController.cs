using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SecretOfLostTemple.AR;
using SecretOfLostTemple.Adaptive;
using SecretOfLostTemple.Assessment;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Master UI and testbed controller for the MainARScene.
    /// Orchestrates all 3 Research Components (AR Interaction, Adaptive Story/Hints, In-Situ Assessment)
    /// and provides seamless editor fallback & mobile AR controls.
    /// </summary>
    public class ARSceneUIController : MonoBehaviour
    {
        [Header("Top Navigation HUD")]
        [SerializeField] private Button backToMapButton;
        [SerializeField] private Text sceneTitleText;
        [SerializeField] private Text characterInfoText;
        [SerializeField] private Image characterAvatarImage;
        [SerializeField] private Sprite mayaAvatarSprite;
        [SerializeField] private Sprite senukAvatarSprite;

        [Header("Component 1: AR Mode Switcher")]
        [SerializeField] private ARModeSwitcher modeSwitcher;
        [SerializeField] private Button viewOnlyModeBtn;
        [SerializeField] private Button interactiveModeBtn;
        [SerializeField] private Text currentModeText;
        [SerializeField] private Text modeInstructionText;

        [Header("Simple Machine Selector Bar")]
        [SerializeField] private Button selectLeverBtn;
        [SerializeField] private Button selectPulleyBtn;
        [SerializeField] private Button selectRampBtn;
        [SerializeField] private Button selectWheelBtn;
        [SerializeField] private Text activeMachineNameText;

        [Header("Machine 3D Testbed Objects")]
        [SerializeField] private GameObject leverRoot;
        [SerializeField] private GameObject pulleyRoot;
        [SerializeField] private GameObject inclinedPlaneRoot;
        [SerializeField] private GameObject wheelAxleRoot;
        [SerializeField] private Text mechanicalAdvantageText;

        [Header("Component 2: Adaptive & Story HUD")]
        [SerializeField] private Button hintButton;
        [SerializeField] private Text difficultyTierBadge;
        [SerializeField] private GameObject storyDialoguePanel;
        [SerializeField] private Text dialogueSpeakerText;
        [SerializeField] private Text dialogueContentText;
        [SerializeField] private Image dialogueAvatarImage;
        [SerializeField] private Button dismissDialogueBtn;

        [Header("Component 3: Assessment HUD")]
        [SerializeField] private Button startQuizButton;
        [SerializeField] private GameObject quizContainerPanel;
        [SerializeField] private Text quizQuestionText;
        [SerializeField] private Button[] quizOptionButtons;
        [SerializeField] private Text[] quizOptionTexts;
        [SerializeField] private Text quizFeedbackText;
        [SerializeField] private Text quizScoreBadgeText;

        [Header("Research Telemetry Live HUD")]
        [SerializeField] private Text liveTelemetryText;
        [SerializeField] private Button toggleTelemetryBtn;
        [SerializeField] private GameObject telemetryPanel;

        [Header("Audio & Feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip successClip;

        private MachineType currentMachine = MachineType.Lever;
        private string selectedCharacter = "Maya";
        private float sessionStartTime;
        private bool isTelemetryOpen = true;

        private void Start()
        {
            sessionStartTime = Time.time;
            selectedCharacter = ResearchProgress.GetString("SelectedCharacter", "Maya");

            // Initialize Player Character UI
            if (characterInfoText != null)
            {
                string username = PlayerPrefs.GetString("LostTemple_Last_User", "Explorer");
                characterInfoText.text = $"<b>{username}</b> ({selectedCharacter})";
            }
            if (characterAvatarImage != null)
            {
                characterAvatarImage.sprite = (selectedCharacter == "Senuk" && senukAvatarSprite != null) ? senukAvatarSprite : mayaAvatarSprite;
            }

            SetupListeners();

            MachineType initialMachine = MachineType.Lever;
            if (PlayerPrefs.HasKey("Sandbox_Selected_Machine"))
            {
                string sel = PlayerPrefs.GetString("Sandbox_Selected_Machine");
                PlayerPrefs.DeleteKey("Sandbox_Selected_Machine");
                if (sel == "Pulley") initialMachine = MachineType.Pulley;
                else if (sel == "InclinedPlane") initialMachine = MachineType.InclinedPlane;
                else if (sel == "WheelAndAxle") initialMachine = MachineType.WheelAndAxle;
            }
            SwitchSimpleMachine(initialMachine);
            StartCoroutine(UpdateTelemetryRoutine());

            // Check if entered in Sandbox Mode vs Story Quest Mode
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            bool isSandbox = appMode == "Sandbox";

            if (isSandbox)
            {
                if (sceneTitleText != null) sceneTitleText.text = "🏛️ <b>AR SANDBOX</b> • SPATIAL LAB";
                if (hintButton != null) hintButton.gameObject.SetActive(false);
                if (startQuizButton != null) startQuizButton.gameObject.SetActive(true);
                if (storyDialoguePanel != null) storyDialoguePanel.SetActive(false);
                // In Sandbox, allow quiz container
                if (quizContainerPanel != null && !quizContainerPanel.activeSelf) quizContainerPanel.SetActive(false);
                if (telemetryPanel != null) telemetryPanel.SetActive(false);

                var bottomBar = GameObject.Find("BottomMachineBar");
                if (bottomBar != null) bottomBar.SetActive(false);
            }
            else
            {
                if (sceneTitleText != null) sceneTitleText.text = "🏛️ <b>TEMPLE GATES</b> • EXPEDITION";
                // Show initial story intro in Quest mode
                ShowCharacterIntro();
            }
        }

        private void SetupListeners()
        {
            if (backToMapButton != null)
            {
                backToMapButton.onClick.RemoveAllListeners();
                backToMapButton.onClick.AddListener(OnBackToMapClicked);
            }

            foreach (var button in new[] { selectPulleyBtn, selectRampBtn, selectWheelBtn }) if (button != null) button.gameObject.SetActive(false);
            // Machine Selectors
            if (selectLeverBtn != null) selectLeverBtn.onClick.AddListener(() => SwitchSimpleMachine(MachineType.Lever));
            if (selectPulleyBtn != null) selectPulleyBtn.onClick.AddListener(() => SwitchSimpleMachine(MachineType.Pulley));
            if (selectRampBtn != null) selectRampBtn.onClick.AddListener(() => SwitchSimpleMachine(MachineType.InclinedPlane));
            if (selectWheelBtn != null) selectWheelBtn.onClick.AddListener(() => SwitchSimpleMachine(MachineType.WheelAndAxle));

            // Mode Switcher buttons
            if (viewOnlyModeBtn != null) viewOnlyModeBtn.onClick.AddListener(() => SetInteractionMode(InteractionMode.ViewOnly));
            if (interactiveModeBtn != null) interactiveModeBtn.onClick.AddListener(() => SetInteractionMode(InteractionMode.Interactive));

            // Hint Button
            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(OnHintClicked);
            }

            // Quiz Button
            if (startQuizButton != null)
            {
                startQuizButton.onClick.RemoveAllListeners();
                startQuizButton.onClick.AddListener(OnStartQuizClicked);
            }

            // Dismiss Dialogue
            if (dismissDialogueBtn != null)
            {
                dismissDialogueBtn.onClick.RemoveAllListeners();
                dismissDialogueBtn.onClick.AddListener(() => {
                    if (storyDialoguePanel != null) storyDialoguePanel.SetActive(false);
                });
            }

            // Telemetry toggle
            if (toggleTelemetryBtn != null)
            {
                toggleTelemetryBtn.onClick.RemoveAllListeners();
                toggleTelemetryBtn.onClick.AddListener(ToggleTelemetryHUD);
            }
        }

        public void SwitchSimpleMachine(MachineType machine)
        {
            if (machine != MachineType.Lever) return;
            currentMachine = machine;
            PlaySound(buttonClickClip);

            if (leverRoot != null) leverRoot.SetActive(machine == MachineType.Lever);
            if (pulleyRoot != null) pulleyRoot.SetActive(machine == MachineType.Pulley);
            if (inclinedPlaneRoot != null) inclinedPlaneRoot.SetActive(machine == MachineType.InclinedPlane);
            if (wheelAxleRoot != null) wheelAxleRoot.SetActive(machine == MachineType.WheelAndAxle);

            string nameStr = "";
            string maStr = "";

            switch (machine)
            {
                case MachineType.Lever:
                    nameStr = "🏛️ Class 1 Ancient Lever (Door Lifter)";
                    maStr = "<b>Mechanical Advantage:</b> MA = Effort Arm / Load Arm = <b>2.50x</b>\n<color=#f5d77f>Effort required: 400N to lift 1000N stone gate</color>";
                    break;
                case MachineType.Pulley:
                    nameStr = "⚙️ Compound Block & Tackle Pulley System";
                    maStr = "<b>Mechanical Advantage:</b> MA = Number of Supporting Ropes = <b>2.00x</b>\n<color=#f5d77f>Effort required: 500N to hoist heavy relic</color>";
                    break;
                case MachineType.InclinedPlane:
                    nameStr = "📐 Ancient Temple Ramp (Inclined Plane)";
                    maStr = "<b>Mechanical Advantage:</b> MA = Slope Length / Height = <b>3.00x</b>\n<color=#f5d77f>Gentle slope reduces lifting force by 67%</color>";
                    break;
                case MachineType.WheelAndAxle:
                    nameStr = "🎡 Temple Portcullis Capstan (Wheel & Axle)";
                    maStr = "<b>Mechanical Advantage:</b> MA = Wheel Radius / Axle Radius = <b>4.00x</b>\n<color=#f5d77f>Turning outer wheel multiplies rotational torque</color>";
                    break;
            }

            if (activeMachineNameText != null) activeMachineNameText.text = nameStr;
            if (mechanicalAdvantageText != null) mechanicalAdvantageText.text = maStr;

            CognitiveTelemetryLogger.Instance?.LogInteraction("switch_machine", null, $"{{\"machine\":\"{machine}\"}}");
            EngagementTracker.Instance?.RecordInteraction();
        }

        public void SetInteractionMode(InteractionMode mode)
        {
            mode = StudyPolicy.Resolve(mode);
            PlaySound(buttonClickClip);
            if (modeSwitcher != null)
            {
                modeSwitcher.SwitchMode(mode);
            }

            if (currentModeText != null)
            {
                currentModeText.text = mode == InteractionMode.ViewOnly ? "MODE: <b>VIEW-ONLY</b> (Observation)" : "MODE: <b>INTERACTIVE</b> (Physics Drag)";
                currentModeText.color = mode == InteractionMode.ViewOnly ? new Color(0.4f, 0.85f, 1f) : new Color(1f, 0.85f, 0.35f);
            }

            if (modeInstructionText != null)
            {
                modeInstructionText.text = mode == InteractionMode.ViewOnly
                    ? "🔍 <b>View-Only:</b> Rotate and inspect the ancient mechanical parts from 360°."
                    : "⚡ <b>Interactive:</b> Drag effort points, adjust fulcrums, and simulate load physics in real-time!";
            }

            // Update SimpleMachine components in scene
            var machines = FindObjectsByType<SimpleMachine>(FindObjectsSortMode.None);
            foreach (var m in machines)
            {
                m.SetMode(mode);
            }
        }

        public void OnHintClicked()
        {
            PlaySound(buttonClickClip);
            EngagementTracker.Instance?.RecordHintRequest();

            var hintSystem = HintSystem.Instance;
            if (hintSystem != null)
            {
                hintSystem.RequestHint();
            }

            // Display adaptive character hint
            ShowDialogue(
                selectedCharacter == "Senuk" ? "SENUK (Explorer Clue)" : "MAYA (Scholar Insight)",
                GetAdaptiveHintMessage(),
                (selectedCharacter == "Senuk" && senukAvatarSprite != null) ? senukAvatarSprite : mayaAvatarSprite
            );
        }

        private string GetAdaptiveHintMessage()
        {
            switch (currentMachine)
            {
                case MachineType.Lever:
                    return selectedCharacter == "Senuk"
                        ? "“Look at the fulcrum pivot! If you slide it closer to the heavy stone door, you'll need way less strength to lift it!”"
                        : "“The ancient glyphs show: <i>Torque = Force × Distance</i>. A longer effort arm multiplies your mechanical advantage!”";
                case MachineType.Pulley:
                    return selectedCharacter == "Senuk"
                        ? "“Notice how the movable pulley shares the weight with the ceiling beam! Each strand cuts the effort in half!”"
                        : "“Mechanical advantage in pulleys equals the number of rope segments directly supporting the load.”";
                case MachineType.InclinedPlane:
                    return selectedCharacter == "Senuk"
                        ? "“A longer ramp means you push for a greater distance, but the force needed is much lighter!”"
                        : "“Spreading the vertical work over a longer incline reduces the required input force proportionally.”";
                case MachineType.WheelAndAxle:
                    return selectedCharacter == "Senuk"
                        ? "“Grab the large outer wheel! Turning the wider ring spins the smaller axle with massive torque!”"
                        : "“The ratio of wheel radius to axle radius defines your force multiplication factor.”";
                default:
                    return "“Inspect the golden force vectors and observe how mechanical advantage changes with distance.”";
            }
        }

        public void OnStartQuizClicked()
        {
            FindFirstObjectByType<LeverARScreenController>()?.OpenARQuiz();
        }

        private void ShowFallbackQuiz(QuizQuestion q)
        {
            FindFirstObjectByType<LeverARScreenController>()?.OpenARQuiz();
        }

        private void ShowCharacterIntro()
        {
            ShowDialogue(
                selectedCharacter == "Senuk" ? "SENUK" : "MAYA",
                selectedCharacter == "Senuk"
                    ? "“We reached the inner Temple Gates! Ancient simple machines are locking the passage. Let's test their mechanical advantage to open them!”"
                    : "“Welcome to the Temple Gates sanctuary. Here we can analyze how simple machines amplify force and study their mechanical physics in AR!”",
                (selectedCharacter == "Senuk" && senukAvatarSprite != null) ? senukAvatarSprite : mayaAvatarSprite
            );
        }

        private void ShowDialogue(string speaker, string text, Sprite portrait)
        {
            if (storyDialoguePanel != null)
            {
                storyDialoguePanel.SetActive(true);
                if (dialogueSpeakerText != null) dialogueSpeakerText.text = $"<b>{speaker}</b>";
                if (dialogueContentText != null) dialogueContentText.text = text;
                if (dialogueAvatarImage != null && portrait != null) dialogueAvatarImage.sprite = portrait;
            }
        }

        private void ToggleTelemetryHUD()
        {
            isTelemetryOpen = !isTelemetryOpen;
            if (telemetryPanel != null) telemetryPanel.SetActive(isTelemetryOpen);
        }

        private IEnumerator UpdateTelemetryRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                if (liveTelemetryText != null)
                {
                    liveTelemetryText.text = ResearchData.Summary();
                }

                if (difficultyTierBadge != null)
                {
                    var diff = AdaptiveDifficultyEngine.Instance;
                    string tierName = diff != null ? diff.GetCurrentProfile()?.tier.ToString() : "Apprentice";
                    difficultyTierBadge.text = $"<b>Tier:</b> {tierName}";
                }
            }
        }

        public void OnBackToMapClicked()
        {
            PlaySound(buttonClickClip);
            CognitiveTelemetryLogger.Instance?.EndSession();
            EngagementTracker.Instance?.StopTracking();

            // Export session summary to Firestore
            string userId = PlayerPrefs.GetString("LostTemple_Auth_LocalId", "local_explorer");
            var record = ResearchDataExporter.Instance?.CollectAllData(userId);
            if (record != null)
            {
                ResearchDataExporter.Instance.ExportToFirestore(record);
            }

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
