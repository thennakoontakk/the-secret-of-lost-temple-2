using System;
using UnityEngine;
using UnityEngine.UI;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Physics and mathematical simulation controller for the 3D Ancient Lever.
    /// Uses 3D models (Fulcrum.glb, Beam.glb, Boulder.glb) to calculate real-time
    /// torque equilibrium, mechanical advantage, and animated force vectors.
    /// </summary>
    public class LeverPhysicsController : MonoBehaviour
    {
        [Header("3D Model References")]
        [SerializeField] private Transform fulcrumRoot;
        [SerializeField] private Transform beamRoot;
        [SerializeField] private Transform boulderRoot;
        [SerializeField] private Transform effortHandle;

        [Header("Visual Force Vectors")]
        [SerializeField] private LineRenderer loadForceLine;
        [SerializeField] private LineRenderer effortForceLine;
        [SerializeField] private ParticleSystem liftSuccessParticles;

        [Header("Physics Constants")]
        [SerializeField] private float beamLength = 2.4f; // Total beam length in meters
        [SerializeField] private float boulderMassKg = 100f / 9.81f; // 100 N teaching load
        [SerializeField] private float gravity = 9.81f;

        [Header("Interactive State")]
        [Range(0.2f, 2.2f)]
        [SerializeField] private float fulcrumPositionAlongBeam = 0.8f; // Distance from boulder end to fulcrum
        [Range(0f, 1500f)]
        [SerializeField] private float appliedEffortForce = 0f; // Force applied in Newtons

        [Header("Animation & Limits")]
        [SerializeField] private float maxTiltAngle = 14f; // Max angle beam can tilt
        [SerializeField] private float tiltSmoothSpeed = 6f;

        // Calculated physics properties
        public float LoadForceNewtons => boulderMassKg * gravity;
        public float LoadArmLength => Mathf.Clamp(fulcrumPositionAlongBeam, 0.15f, beamLength - 0.15f);
        public float EffortArmLength => Mathf.Max(0.15f, beamLength - LoadArmLength);
        public float MechanicalAdvantage => EffortArmLength / LoadArmLength;
        public float RequiredEffortForce => LoadForceNewtons / MechanicalAdvantage;
        public float AppliedEffortForce => appliedEffortForce;
        public float LoadTorque => LoadForceNewtons * LoadArmLength;
        public float EffortTorque => appliedEffortForce * EffortArmLength;
        public float NetTorque => EffortTorque - LoadTorque;
        public bool IsBoulderLifted => NetTorque > 15f;
        public bool IsBalanced => Mathf.Abs(NetTorque) <= 15f && appliedEffortForce > 10f;

        public event Action<LeverPhysicsController> OnPhysicsUpdated;

        private float currentBeamAngle = 0f;
        private Vector3 initialBoulderLocalPos;
        private bool hasTriggeredSuccess = false;

        private void Start()
        {
            if (boulderRoot != null)
            {
                initialBoulderLocalPos = boulderRoot.localPosition;
            }

            // Ensure Beam_3D_Model (which is authored along Z in Beam.glb) is rotated 90 degrees around Y to span along X
            if (beamRoot != null)
            {
                var beamModel = beamRoot.Find("Beam_3D_Model");
                if (beamModel != null)
                {
                    beamModel.localRotation = Quaternion.Euler(0f, 90f, 0f);
                }
            }

            SetupForceLines();
            UpdatePhysicsLayout();
        }

        private void SetupForceLines()
        {
            if (loadForceLine != null)
            {
                loadForceLine.useWorldSpace = true;
                loadForceLine.positionCount = 2;
                loadForceLine.startWidth = 0.035f;
                loadForceLine.endWidth = 0.035f;
                loadForceLine.material = new Material(Shader.Find("Sprites/Default"));
                loadForceLine.startColor = new Color(1f, 0.3f, 0.3f, 0.95f);
                loadForceLine.endColor = new Color(1f, 0.15f, 0.15f, 0.95f);
            }

            if (effortForceLine != null)
            {
                effortForceLine.useWorldSpace = true;
                effortForceLine.positionCount = 2;
                effortForceLine.startWidth = 0.035f;
                effortForceLine.endWidth = 0.035f;
                effortForceLine.material = new Material(Shader.Find("Sprites/Default"));
                effortForceLine.startColor = new Color(1f, 0.85f, 0.2f, 0.95f);
                effortForceLine.endColor = new Color(1f, 0.7f, 0.1f, 0.95f);
            }
        }

        private void Update()
        {
            AnimateBeamPhysics();
            UpdateForceVectors();
        }

        /// <summary>
        /// Sets the position of the fulcrum along the beam (in meters from the boulder end).
        /// </summary>
        public void SetFulcrumPosition(float distanceMeters)
        {
            fulcrumPositionAlongBeam = Mathf.Clamp(distanceMeters, 0.2f, beamLength - 0.2f);
            UpdatePhysicsLayout();
            OnPhysicsUpdated?.Invoke(this);
        }

        /// <summary>
        /// Sets the applied downward effort force in Newtons.
        /// </summary>
        public void SetAppliedEffortForce(float forceNewtons)
        {
            appliedEffortForce = Mathf.Clamp(forceNewtons, 0f, 1500f);
            OnPhysicsUpdated?.Invoke(this);
        }

        /// <summary>
        /// Updates the 3D positioning of fulcrum, boulder, beam and effort point based on lengths.
        /// </summary>
        public void UpdatePhysicsLayout()
        {
            float halfBeam = beamLength * 0.5f;
            float fulcrumOffsetFromCenter = LoadArmLength - halfBeam;

            // Move Fulcrum to the pivot position
            if (fulcrumRoot != null)
            {
                fulcrumRoot.localPosition = new Vector3(fulcrumOffsetFromCenter, 0f, 0f);
            }

            // Beam is mounted on top of fulcrum
            if (beamRoot != null)
            {
                beamRoot.localPosition = new Vector3(fulcrumOffsetFromCenter, 0.35f, 0f) - beamRoot.localRotation * new Vector3(fulcrumOffsetFromCenter, 0f, 0f);
            }

            // Boulder is on the left end (Load side)
            if (boulderRoot != null)
            {
                boulderRoot.localPosition = new Vector3(-halfBeam, 0.25f, 0f);
            }

            // Effort handle is on the right end (Effort side)
            if (effortHandle != null)
            {
                effortHandle.localPosition = new Vector3(halfBeam, 0.1f, 0f);
            }
        }

        private void AnimateBeamPhysics()
        {
            if (beamRoot == null) return;

            float targetAngle = 0f;

            if (appliedEffortForce < 1f)
            {
                // No effort applied: boulder pulls its side down to ground
                targetAngle = maxTiltAngle;
            }
            else if (IsBoulderLifted)
            {
                // Enough effort applied: effort side goes down, boulder lifts UP!
                float excessRatio = Mathf.Clamp01((appliedEffortForce - RequiredEffortForce) / (RequiredEffortForce * 0.5f + 1f));
                targetAngle = Mathf.Lerp(0f, -maxTiltAngle, excessRatio);

                if (!hasTriggeredSuccess && excessRatio > 0.4f)
                {
                    hasTriggeredSuccess = true;
                    if (liftSuccessParticles != null) liftSuccessParticles.Play();
                }
            }
            else if (IsBalanced)
            {
                // Perfectly balanced horizontal state
                targetAngle = 0f;
            }
            else
            {
                // Inadequate effort: beam tilts towards boulder
                float effortRatio = appliedEffortForce / (RequiredEffortForce + 0.001f);
                targetAngle = Mathf.Lerp(maxTiltAngle, 0f, effortRatio);
                hasTriggeredSuccess = false;
            }

            currentBeamAngle = Mathf.Lerp(currentBeamAngle, targetAngle, Time.deltaTime * tiltSmoothSpeed);
            beamRoot.localRotation = Quaternion.Euler(0f, 0f, currentBeamAngle);
            UpdatePhysicsLayout();
        }

        private void UpdateForceVectors()
        {
            // Load force arrow (downward from boulder)
            if (loadForceLine != null && boulderRoot != null)
            {
                Vector3 start = boulderRoot.position;
                Vector3 end = start + Vector3.down * 0.45f;
                loadForceLine.SetPosition(0, start);
                loadForceLine.SetPosition(1, end);
            }

            // Effort force arrow (downward from effort handle, scaled by applied force)
            if (effortForceLine != null && effortHandle != null)
            {
                if (appliedEffortForce > 5f)
                {
                    effortForceLine.enabled = true;
                    Vector3 start = effortHandle.position;
                    float arrowLen = Mathf.Clamp(appliedEffortForce / 1000f * 0.5f, 0.1f, 0.6f);
                    Vector3 end = start + Vector3.down * arrowLen;
                    effortForceLine.SetPosition(0, start);
                    effortForceLine.SetPosition(1, end);
                }
                else
                {
                    effortForceLine.enabled = false;
                }
            }
        }
    }
}
