using System;
using UnityEngine;

namespace SecretOfLostTemple.AR
{
    public enum MachineType
    {
        Lever,
        Pulley,
        InclinedPlane,
        WheelAndAxle
    }

    public enum InteractionMode
    {
        ViewOnly,
        Interactive
    }

    /// <summary>
    /// Represents a single simple machine in AR.
    /// Manages its state, interaction mode, and provides visual feedback.
    /// </summary>
    public class SimpleMachine : MonoBehaviour
    {
        [SerializeField] private MachineType machineType;
        [SerializeField] private InteractionMode currentMode = InteractionMode.ViewOnly;
        
        [Header("Visual Indicators")]
        [SerializeField] private GameObject viewOnlyIndicator;
        [SerializeField] private GameObject interactiveIndicator;
        
        [Header("Feedback")]
        [SerializeField] private Renderer mainRenderer;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private float emissionPulseSpeed = 2f;
        [SerializeField] private Color emissionColor = new Color(1f, 0.84f, 0f); // Gold

        // Telemetry tracking
        private int manipulationCount;
        private int errorCount;
        private float firstInteractionTime = -1f;
        private float totalDwellTime;
        private bool hasBeenInteracted;
        private float activeSessionTime;

        public event Action<SimpleMachine, InteractionMode> OnModeChanged;
        public event Action<SimpleMachine, bool> OnManipulationResult;

        public bool IsInteractive => currentMode == InteractionMode.Interactive;
        public MachineType Type => machineType;
        public bool HasBeenInteracted => hasBeenInteracted;
        public int ManipulationCount => manipulationCount;
        public int ErrorCount => errorCount;
        public float FirstInteractionTime => firstInteractionTime;
        public float TotalDwellTime => totalDwellTime;

        private Material materialInstance;

        private void Awake()
        {
            if (mainRenderer != null)
            {
                materialInstance = mainRenderer.material;
                materialInstance.EnableKeyword("_EMISSION");
            }
            UpdateIndicators();
        }

        private void Update()
        {
            activeSessionTime += Time.deltaTime;
            
            if (hasBeenInteracted)
            {
                totalDwellTime += Time.deltaTime;
            }

            // Glowing outline pulse
            if (materialInstance != null)
            {
                float emission = Mathf.PingPong(Time.time * emissionPulseSpeed, 1.0f);
                materialInstance.SetColor("_EmissionColor", emissionColor * emission);
            }
        }

        /// <summary>
        /// Sets the interaction mode of the simple machine.
        /// </summary>
        public void SetMode(InteractionMode mode)
        {
            mode = StudyPolicy.Resolve(mode);
            if (currentMode != mode)
            {
                currentMode = mode;
                UpdateIndicators();
                OnModeChanged?.Invoke(this, currentMode);
            }
        }

        private void UpdateIndicators()
        {
            if (viewOnlyIndicator != null) viewOnlyIndicator.SetActive(currentMode == InteractionMode.ViewOnly);
            if (interactiveIndicator != null) interactiveIndicator.SetActive(currentMode == InteractionMode.Interactive);
        }

        /// <summary>
        /// Called when the user correctly manipulates the machine.
        /// </summary>
        public void OnCorrectManipulation()
        {
            if (!hasBeenInteracted)
            {
                firstInteractionTime = activeSessionTime;
                hasBeenInteracted = true;
            }
            
            manipulationCount++;
            if (successParticles != null) successParticles.Play();
            
            OnManipulationResult?.Invoke(this, true);
        }

        /// <summary>
        /// Called when the user incorrectly manipulates the machine.
        /// </summary>
        public void OnIncorrectManipulation()
        {
            if (!hasBeenInteracted)
            {
                firstInteractionTime = activeSessionTime;
                hasBeenInteracted = true;
            }
            
            manipulationCount++;
            errorCount++;
            
            OnManipulationResult?.Invoke(this, false);
        }

        /// <summary>
        /// Resets the machine to its initial state.
        /// </summary>
        public void ResetMachine()
        {
            manipulationCount = 0;
            errorCount = 0;
            hasBeenInteracted = false;
            totalDwellTime = 0f;
            firstInteractionTime = -1f;
            activeSessionTime = 0f;
        }

        private void OnDestroy()
        {
            if (materialInstance != null)
            {
                Destroy(materialInstance);
            }
        }
    }
}
