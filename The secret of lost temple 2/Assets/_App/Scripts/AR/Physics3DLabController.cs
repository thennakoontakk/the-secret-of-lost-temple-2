using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SecretOfLostTemple.Core;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Master controller for the 3D Physics Lab Mode (dedicated Image Tracking & Physics Simulation Scene).
    /// Manages the Vuforia/AR image scanning workflow, materializes the 3D Lever (Beam.glb, Boulder.glb, Fulcrum.glb),
    /// and provides interactive sliders, formula visualizations, and mechanical advantage analytics.
    /// </summary>
    public class Physics3DLabController : MonoBehaviour
    {
        [Header("3D Physics Lever Reference")]
        [SerializeField] private LeverPhysicsController leverPhysics;
        [SerializeField] private GameObject lever3DRoot;

        [Header("Image Scanner Overlay")]
        [SerializeField] private GameObject scannerOverlayPanel;
        [SerializeField] private Image targetPreviewImage;
        [SerializeField] private Text scannerInstructionText;
        [SerializeField] private Button simulateTargetFoundBtn; // For instant testing in Editor / without printout
        [SerializeField] private Sprite leverTargetSprite;
        [SerializeField] private Sprite pulleyTargetSprite;
        [SerializeField] private Sprite inclinedPlaneTargetSprite;

        [Header("Top Navigation HUD")]
        [SerializeField] private Button backToMapButton;
        [SerializeField] private Text labTitleText;
        [SerializeField] private Text characterInfoText;
        [SerializeField] private Image characterAvatarImage;
        [SerializeField] private Sprite mayaAvatarSprite;
        [SerializeField] private Sprite senukAvatarSprite;

        [Header("Interactive Physics Sliders")]
        [SerializeField] private Slider appliedForceSlider;
        [SerializeField] private Text appliedForceValueText;
        [SerializeField] private Slider fulcrumPositionSlider;
        [SerializeField] private Text fulcrumPositionValueText;

        [Header("Preset Buttons")]
        [SerializeField] private Button presetHighMAButton;
        [SerializeField] private Button presetEqualArmsButton;
        [SerializeField] private Button presetLowMAButton;
        [SerializeField] private Button resetSimulationButton;

        [Header("Live Physics Dashboard")]
        [SerializeField] private Text mechanicalAdvantageValueText;
        [SerializeField] private Text loadForceValueText;
        [SerializeField] private Text requiredForceValueText;
        [SerializeField] private Text torqueEquationText;
        [SerializeField] private Text simulationStatusBadge;

        [Header("Target Switcher Tabs")]
        [SerializeField] private Button leverTargetTab;
        [SerializeField] private Button pulleyTargetTab;
        [SerializeField] private Button inclinedPlaneTargetTab;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip targetDetectedClip;

        private bool isTargetTracked = false;
        public bool IsTargetTracked => isTargetTracked;
        private string currentActiveTarget = "lever_target";
        public string CurrentActiveTarget => currentActiveTarget;

        private void Start()
        {
            InitializePlayerProfile();
            SetupButtonListeners();
            SetupSliders();

            if (leverPhysics != null)
            {
                leverPhysics.OnPhysicsUpdated += UpdateDashboard;
            }

            OnForceSliderChanged(appliedForceSlider != null ? appliedForceSlider.value : 0f);
            OnFulcrumSliderChanged(fulcrumPositionSlider != null ? fulcrumPositionSlider.value : .8f);
            // Initially show scanner prompt
            ShowScannerPrompt("lever_target");
        }

        private void OnDestroy()
        {
            if (leverPhysics != null) leverPhysics.OnPhysicsUpdated -= UpdateDashboard;
        }

        private void InitializePlayerProfile()
        {
            string username = PlayerPrefs.GetString("LostTemple_Last_User", "Explorer");
            string selectedChar = ResearchProgress.GetString("SelectedCharacter", "Maya");

            if (characterInfoText != null)
            {
                characterInfoText.text = $"<b>{username}</b> ({selectedChar})";
            }

            if (characterAvatarImage != null)
            {
                characterAvatarImage.sprite = (selectedChar == "Senuk" && senukAvatarSprite != null) ? senukAvatarSprite : mayaAvatarSprite;
            }
        }

        private void SetupButtonListeners()
        {
            if (backToMapButton != null)
            {
                backToMapButton.onClick.RemoveAllListeners();
                backToMapButton.onClick.AddListener(OnBackToMapClicked);
            }

            if (simulateTargetFoundBtn != null)
            {
                simulateTargetFoundBtn.onClick.RemoveAllListeners();
                simulateTargetFoundBtn.gameObject.SetActive(true);
                simulateTargetFoundBtn.interactable = true;
                simulateTargetFoundBtn.onClick.AddListener(() => { ResearchData.Record("marker_simulated", "physics_lab;manual"); OnTargetDetected(); });
            }

            if (presetHighMAButton != null)
            {
                presetHighMAButton.onClick.RemoveAllListeners();
                presetHighMAButton.onClick.AddListener(() => SetPreset(0.5f)); // Load arm 0.5m -> MA = 3.8x
            }

            if (presetEqualArmsButton != null)
            {
                presetEqualArmsButton.onClick.RemoveAllListeners();
                presetEqualArmsButton.onClick.AddListener(() => SetPreset(1.2f)); // Load arm 1.2m -> MA = 1.0x
            }

            if (presetLowMAButton != null)
            {
                presetLowMAButton.onClick.RemoveAllListeners();
                presetLowMAButton.onClick.AddListener(() => SetPreset(1.6f)); // Load arm 1.6m -> MA = 0.5x
            }

            if (resetSimulationButton != null)
            {
                resetSimulationButton.onClick.RemoveAllListeners();
                resetSimulationButton.onClick.AddListener(ResetSimulation);
            }

            // Target tabs
            if (leverTargetTab != null) leverTargetTab.onClick.AddListener(() => SelectTarget("lever_target"));
            if (pulleyTargetTab != null) pulleyTargetTab.interactable = false;
            if (inclinedPlaneTargetTab != null) inclinedPlaneTargetTab.interactable = false;
        }

        private void SetupSliders()
        {
            if (appliedForceSlider != null)
            {
                appliedForceSlider.minValue = 0f;
                appliedForceSlider.maxValue = 1200f;
                appliedForceSlider.value = 0f;
                appliedForceSlider.onValueChanged.RemoveAllListeners();
                appliedForceSlider.onValueChanged.AddListener(OnForceSliderChanged);
            }

            if (fulcrumPositionSlider != null)
            {
                fulcrumPositionSlider.minValue = 0.3f;
                fulcrumPositionSlider.maxValue = 2.1f;
                fulcrumPositionSlider.value = 0.8f;
                fulcrumPositionSlider.onValueChanged.RemoveAllListeners();
                fulcrumPositionSlider.onValueChanged.AddListener(OnFulcrumSliderChanged);
            }
        }

        public void SelectTarget(string targetName)
        {
            if (targetName != "lever_target") return;
            currentActiveTarget = targetName;
            PlaySound(buttonClickClip);
            ShowScannerPrompt(targetName);
        }

        private void ShowScannerPrompt(string targetName)
        {
            if (scannerOverlayPanel != null) scannerOverlayPanel.SetActive(true);
            if (lever3DRoot != null) lever3DRoot.SetActive(false);
            isTargetTracked = false;

            if (targetPreviewImage != null)
            {
                if (targetName == "lever_target" && leverTargetSprite != null) targetPreviewImage.sprite = leverTargetSprite;
                else if (targetName == "pulley_target" && pulleyTargetSprite != null) targetPreviewImage.sprite = pulleyTargetSprite;
                else if (targetName == "inclined_plane_target" && inclinedPlaneTargetSprite != null) targetPreviewImage.sprite = inclinedPlaneTargetSprite;
            }

            if (scannerInstructionText != null)
            {
                scannerInstructionText.text = $"<b>POINT CAMERA AT '{targetName.ToUpper().Replace('_', ' ')}'</b>\n<size=17><color=#d5c59f>Scan image to materialize ancient 3D physics mechanism or tap 'Simulate Scan'</color></size>";
            }
        }

        /// <summary>
        /// Called when the Vuforia image target is tracked or simulated.
        /// </summary>
        public void OnTargetDetected()
        {
            if (isTargetTracked) return;
            isTargetTracked = true;
            PlaySound(targetDetectedClip != null ? targetDetectedClip : successClip);

            if (scannerOverlayPanel != null)
            {
                StartCoroutine(FadeOutScannerPrompt());
            }

            if (lever3DRoot != null)
            {
                lever3DRoot.SetActive(true);
                StartCoroutine(AnimateMaterializeObject(lever3DRoot.transform));
            }

            // Reset simulation values
            if (appliedForceSlider != null) appliedForceSlider.value = 0f;
            if (fulcrumPositionSlider != null) fulcrumPositionSlider.value = 0.8f;

            if (leverPhysics != null)
            {
                leverPhysics.SetFulcrumPosition(0.8f);
                leverPhysics.SetAppliedEffortForce(0f);
            }

            UpdateDashboard(leverPhysics);
        }

        private IEnumerator FadeOutScannerPrompt()
        {
            CanvasGroup cg = scannerOverlayPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = scannerOverlayPanel.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            float duration = 0.35f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            scannerOverlayPanel.SetActive(false);
            cg.alpha = 1f;
        }

        private IEnumerator AnimateMaterializeObject(Transform tr)
        {
            Vector3 targetScale = Vector3.one;
            tr.localScale = Vector3.zero;
            float elapsed = 0f;
            float duration = 0.45f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smooth = Mathf.SmoothStep(0f, 1f, t);
                tr.localScale = Vector3.Lerp(Vector3.zero, targetScale, smooth);
                yield return null;
            }
            tr.localScale = targetScale;
        }

        private void OnForceSliderChanged(float val)
        {
            if (appliedForceValueText != null)
            {
                appliedForceValueText.text = $"{val:F0} N";
            }

            if (leverPhysics != null)
            {
                leverPhysics.SetAppliedEffortForce(val);
            }
        }

        private void OnFulcrumSliderChanged(float val)
        {
            if (fulcrumPositionValueText != null)
            {
                fulcrumPositionValueText.text = $"{val:F2} m";
            }

            if (leverPhysics != null)
            {
                leverPhysics.SetFulcrumPosition(val);
            }
        }

        private void SetPreset(float fulcrumDist)
        {
            PlaySound(buttonClickClip);
            if (fulcrumPositionSlider != null)
            {
                fulcrumPositionSlider.value = fulcrumDist;
            }
            if (appliedForceSlider != null)
            {
                appliedForceSlider.value = 0f;
            }
        }

        public void ResetSimulation()
        {
            PlaySound(buttonClickClip);
            if (appliedForceSlider != null) appliedForceSlider.value = 0f;
            if (fulcrumPositionSlider != null) fulcrumPositionSlider.value = 0.8f;
            if (leverPhysics != null)
            {
                leverPhysics.SetFulcrumPosition(0.8f);
                leverPhysics.SetAppliedEffortForce(0f);
            }
        }

        private void UpdateDashboard(LeverPhysicsController lp)
        {
            if (lp == null) return;

            float ma = lp.MechanicalAdvantage;
            float reqForce = lp.RequiredEffortForce;
            float appForce = lp.AppliedEffortForce;
            float loadForce = lp.LoadForceNewtons;

            if (mechanicalAdvantageValueText != null)
            {
                mechanicalAdvantageValueText.text = $"<b>MA = d_e / d_l = {ma:F2}x | Load {loadForce:F0} N</b>\n<size=15><color=#d5c59f>Effort Arm ({lp.EffortArmLength:F2}m) / Load Arm ({lp.LoadArmLength:F2}m)</color></size>";
            }

            if (loadForceValueText != null)
            {
                loadForceValueText.text = $"<b>{loadForce:F0} N</b> (Boulder weight)";
            }

            if (requiredForceValueText != null)
            {
                requiredForceValueText.text = $"<b>{reqForce:F0} N</b>\n<size=14><color=#f5d77f>Load / MA ({loadForce:F0}N / {ma:F2})</color></size>";
            }

            if (torqueEquationText != null)
            {
                torqueEquationText.text =
                    $"<b>TORQUE BALANCE</b>\n" +
                    $"• Load Torque (τ₁): {lp.LoadTorque:F0} N·m\n" +
                    $"• Effort Torque (τ₂): {lp.EffortTorque:F0} N·m  (Net: {lp.NetTorque:+0;-0;0} N·m)";
            }

            if (simulationStatusBadge != null)
            {
                if (lp.IsBoulderLifted)
                {
                    simulationStatusBadge.text = "<color=#55ff88><b>🎉 SUCCESS: BOULDER HOISTED!</b>\nApplied force overcomes load torque.</color>";
                }
                else if (lp.IsBalanced)
                {
                    simulationStatusBadge.text = "<color=#ffff55><b>⚖️ EQUILIBRIUM: BALANCED!</b>\nEffort torque matches load torque perfectly.</color>";
                }
                else if (appForce > 10f)
                {
                    float deficit = reqForce - appForce;
                    simulationStatusBadge.text = $"<color=#ff6666><b>⚠️ INADEQUATE FORCE: NEED +{deficit:F0} N MORE</b>\nIncrease applied force or move fulcrum closer to boulder.</color>";
                }
                else
                {
                    simulationStatusBadge.text = "<color=#ffffff><b>DRAG EFFORT SLIDER TO APPLY FORCE</b>\nObserve how mechanical advantage changes the force needed.</color>";
                }
            }
        }

        public void OnBackToMapClicked()
        {
            PlaySound(buttonClickClip);
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
