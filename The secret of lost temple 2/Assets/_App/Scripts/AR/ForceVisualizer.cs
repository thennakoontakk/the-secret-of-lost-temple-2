using UnityEngine;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Draws animated force arrows and motion trails on simple machines.
    /// Designed with an ancient golden energy stream theme.
    /// </summary>
    public class ForceVisualizer : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private LineRenderer effortForceArrow;
        [SerializeField] private LineRenderer loadForceArrow;
        [SerializeField] private LineRenderer fulcrumMarker;
        [SerializeField] private TextMesh mechanicalAdvantageText;
        
        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem forceParticles;
        [SerializeField] private ParticleSystem motionTrail;

        [Header("Styling")]
        [SerializeField] private Color effortColor = new Color(1f, 0.84f, 0f); // #FFD700
        [SerializeField] private Color loadColor = new Color(1f, 0.26f, 0.26f); // #FF4444
        [SerializeField] private Color fulcrumColor = new Color(0.26f, 1f, 0.26f); // #44FF44
        [SerializeField] private float arrowLength = 1f;
        [SerializeField] private float arrowHeadSize = 0.2f;
        [SerializeField] private float animationSpeed = 2f;

        private void Awake()
        {
            InitializeLineRenderer(effortForceArrow, effortColor);
            InitializeLineRenderer(loadForceArrow, loadColor);
            InitializeLineRenderer(fulcrumMarker, fulcrumColor);
            HideForces();
        }

        private void Update()
        {
            if (effortForceArrow != null && effortForceArrow.gameObject.activeSelf)
            {
                // Animate arrow width pulsing
                float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * animationSpeed);
                effortForceArrow.widthMultiplier = pulse;
                if (loadForceArrow != null) loadForceArrow.widthMultiplier = pulse;
            }
        }

        private void InitializeLineRenderer(LineRenderer lr, Color color)
        {
            if (lr != null)
            {
                lr.startColor = color;
                lr.endColor = color;
                lr.startWidth = arrowHeadSize * 0.5f;
                lr.endWidth = arrowHeadSize;
                lr.positionCount = 2;
                lr.useWorldSpace = true;
            }
        }

        /// <summary>
        /// Shows the forces applied on the machine.
        /// </summary>
        public void ShowForces(SimpleMachine machine, float effortForce, float loadForce)
        {
            if (!machine.IsInteractive) return;

            if (effortForceArrow != null) effortForceArrow.gameObject.SetActive(true);
            if (loadForceArrow != null) loadForceArrow.gameObject.SetActive(true);
            
            Vector3 pos = machine.transform.position;
            
            if (effortForceArrow != null)
            {
                effortForceArrow.SetPosition(0, pos + Vector3.up);
                effortForceArrow.SetPosition(1, pos + Vector3.up + Vector3.down * (effortForce * arrowLength));
            }
            
            UpdateMechanicalAdvantage(effortForce, loadForce);
        }

        /// <summary>
        /// Hides all force visualizations.
        /// </summary>
        public void HideForces()
        {
            if (effortForceArrow != null) effortForceArrow.gameObject.SetActive(false);
            if (loadForceArrow != null) loadForceArrow.gameObject.SetActive(false);
            if (fulcrumMarker != null) fulcrumMarker.gameObject.SetActive(false);
            if (mechanicalAdvantageText != null) mechanicalAdvantageText.gameObject.SetActive(false);
            if (forceParticles != null) forceParticles.Stop();
        }

        /// <summary>
        /// Animates applying a force in a given direction.
        /// </summary>
        public void AnimateForceApplication(Vector3 direction, float magnitude)
        {
            if (forceParticles != null)
            {
                forceParticles.transform.forward = direction;
                var main = forceParticles.main;
                main.startSpeed = magnitude * 5f;
                forceParticles.Play();
            }
        }

        /// <summary>
        /// Updates the 3D text showing the Mechanical Advantage (MA).
        /// </summary>
        public void UpdateMechanicalAdvantage(float effort, float load)
        {
            if (mechanicalAdvantageText != null && effort > 0)
            {
                mechanicalAdvantageText.gameObject.SetActive(true);
                float ma = load / effort;
                mechanicalAdvantageText.text = $"MA: {ma:F1}";
            }
        }

        /// <summary>
        /// Shows a trail of motion along a given path.
        /// </summary>
        public void ShowMotionTrail(Vector3[] path)
        {
            if (motionTrail != null && path.Length > 0)
            {
                motionTrail.transform.position = path[0];
                motionTrail.Play();
            }
        }
    }
}
