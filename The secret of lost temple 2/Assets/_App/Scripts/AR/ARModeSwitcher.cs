using System;
using UnityEngine;
using UnityEngine.UI;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// UI component for switching between View-Only and Interactive modes.
    /// Logs interactions and controls UI presentation.
    /// </summary>
    public class ARModeSwitcher : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button viewOnlyButton;
        [SerializeField] private Button interactiveButton;
        [SerializeField] private Text modeLabel;
        [SerializeField] private Text modeDescriptionText;
        [SerializeField] private Image viewOnlyHighlight;
        [SerializeField] private Image interactiveHighlight;
        
        [Header("Panels")]
        [SerializeField] private GameObject viewOnlyPanel;
        [SerializeField] private GameObject interactivePanel;

        [Header("Styling")]
        [SerializeField] private Color activeColor = new Color(1f, 0.84f, 0f); // Gold
        [SerializeField] private Color inactiveColor = Color.grey;

        private InteractionMode currentMode = InteractionMode.ViewOnly;

        public InteractionMode CurrentMode => currentMode;

        public event Action<InteractionMode> OnModeSwitch;

        private const string ViewOnlyDescription = "Observe the standardized lever demonstration.";
        private const string InteractiveDescription = "Touch, drag, and apply forces to operate the mechanism!";

        private void Awake()
        {
            if (viewOnlyButton != null)
                viewOnlyButton.onClick.AddListener(() => SwitchMode(InteractionMode.ViewOnly));
            
            if (interactiveButton != null)
                interactiveButton.onClick.AddListener(() => SwitchMode(InteractionMode.Interactive));
                
            currentMode = StudyPolicy.Resolve(currentMode);
            UpdateUI();
        }

        /// <summary>
        /// Switches the current AR interaction mode.
        /// </summary>
        public void SwitchMode(InteractionMode mode)
        {
            mode = StudyPolicy.Resolve(mode);
            if (currentMode == mode) return;

            InteractionMode oldMode = currentMode;
            currentMode = mode;
            
            UpdateUI();
            
            // Log mode switch
            CognitiveTelemetryLogger.Instance.LogModeSwitch(oldMode, currentMode);
            
            OnModeSwitch?.Invoke(currentMode);
        }

        /// <summary>
        /// Toggles between view-only and interactive modes.
        /// </summary>
        public void ToggleMode()
        {
            SwitchMode(currentMode == InteractionMode.ViewOnly ? InteractionMode.Interactive : InteractionMode.ViewOnly);
        }

        private void UpdateUI()
        {
            bool isViewOnly = currentMode == InteractionMode.ViewOnly;

            if (viewOnlyHighlight != null) viewOnlyHighlight.color = isViewOnly ? activeColor : inactiveColor;
            if (interactiveHighlight != null) interactiveHighlight.color = !isViewOnly ? activeColor : inactiveColor;

            if (modeLabel != null) modeLabel.text = isViewOnly ? "View Only" : "Interactive";
            if (modeDescriptionText != null) modeDescriptionText.text = isViewOnly ? ViewOnlyDescription : InteractiveDescription;

            if (viewOnlyPanel != null) viewOnlyPanel.SetActive(isViewOnly);
            if (interactivePanel != null) interactivePanel.SetActive(!isViewOnly);
        }
        
        private void OnDestroy()
        {
            if (viewOnlyButton != null) viewOnlyButton.onClick.RemoveAllListeners();
            if (interactiveButton != null) interactiveButton.onClick.RemoveAllListeners();
        }
    }
}
