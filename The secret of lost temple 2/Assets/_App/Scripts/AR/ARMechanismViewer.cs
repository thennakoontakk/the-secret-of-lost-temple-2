using UnityEngine;
using Input = StudyInput;
using UnityEngine.EventSystems;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Handles 360° orbital inspection in View-Only mode and
    /// two-finger pinch-to-scale + effort manipulation in Interactive mode.
    /// </summary>
    public class ARMechanismViewer : MonoBehaviour
    {
        [Header("Target & Pivot")]
        [SerializeField] private Transform modelRoot;
        [SerializeField] private Transform beamPivot;

        [Header("View-Only: 360° Orbit Inspection")]
        [SerializeField] private bool autoOrbitInViewOnly = true;
        [SerializeField] private float autoOrbitSpeed = 18f; // Degrees per second
        [SerializeField] private float dragRotateSpeed = 0.35f;
        [SerializeField] private float orbitDamping = 5f;

        [Header("Interactive: 2-Finger Pinch Scale")]
        [SerializeField] private float minScale = 0.4f;
        [SerializeField] private float maxScale = 2.5f;
        [SerializeField] private float pinchScaleSpeed = 0.005f;
        [SerializeField] private float scrollScaleSpeed = 0.15f;
        [SerializeField] private float scaleSmoothTime = 0.12f;

        [Header("Interactive: 1-Finger Force Tilt")]
        [SerializeField] private float maxTiltAngle = 14f;
        [SerializeField] private float tiltSensitivity = 0.08f;

        private InteractionMode currentMode = InteractionMode.ViewOnly;
        private float currentYaw = 0f;
        private float targetYaw = 0f;
        private float targetScaleFactor = 1f;
        private float currentScaleFactor = 1f;
        private Vector3 initialScale;
        private Vector2 previousTouchPos0;
        private Vector2 previousTouchPos1;
        private float lastInteractionTime;
        private float currentBeamTilt = 6f;
        private bool gestureActive;

        private void Awake()
        {
            if (modelRoot == null) modelRoot = transform;
            initialScale = modelRoot.localScale;
            currentScaleFactor = 1f;
            targetScaleFactor = 1f;
            currentYaw = modelRoot.localEulerAngles.y;
            targetYaw = currentYaw;

            var sm = GetComponent<SimpleMachine>();
            if (sm != null)
            {
                sm.OnModeChanged += HandleModeChanged;
                currentMode = sm.IsInteractive ? InteractionMode.Interactive : InteractionMode.ViewOnly;
            }
        }

        private void OnDestroy()
        {
            var sm = GetComponent<SimpleMachine>();
            if (sm != null) sm.OnModeChanged -= HandleModeChanged;
        }

        public void ResetViewer()
        {
            targetYaw = 0f;
            currentYaw = 0f;
            targetScaleFactor = 1f;
            currentScaleFactor = 1f;
            currentBeamTilt = 0f;
            if (modelRoot != null)
            {
                modelRoot.localRotation = Quaternion.identity;
                modelRoot.localScale = initialScale;
            }
        }

        public void Rebase()
        {
            initialScale = modelRoot.localScale;
            currentScaleFactor = targetScaleFactor = 1f;
            targetYaw = currentYaw = modelRoot.localEulerAngles.y;
        }
        public void RotateBy(float degrees)
        {
            if (!StudyPolicy.InputAllowed) return;
            targetYaw += degrees;
            SecretOfLostTemple.Research.Research.Record("rotate_step", degrees.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public void SetInteractionMode(InteractionMode mode)
        {
            mode = StudyPolicy.Resolve(mode);
            currentMode = mode;
            if (currentMode == InteractionMode.Interactive)
            {
                targetYaw = 0f; // Reset to front-facing, stop 360 spin
            }
        }

        private void HandleModeChanged(SimpleMachine sm, InteractionMode mode)
        {
            SetInteractionMode(mode);
        }

        private void Update()
        {

            if (StudyPolicy.Measured && !StudyPolicy.Tracking) return;
            if (StudyPolicy.Passive) { targetYaw += autoOrbitSpeed * Time.deltaTime; ApplySmoothTransforms(); return; }
            float previousYaw = targetYaw;
            float previousScale = targetScaleFactor;
            HandlePinchAndScrollScale();
            HandleViewOnly360();
            ApplySmoothTransforms();
            if (currentMode == InteractionMode.Interactive &&
                (!Mathf.Approximately(previousYaw, targetYaw) || !Mathf.Approximately(previousScale, targetScaleFactor)))
                gestureActive = true;
            bool touching = Input.touchCount > 0 || Input.GetMouseButton(0);
            if (!touching && gestureActive) {
                SecretOfLostTemple.Research.Research.Record("model_gesture", "yaw=" + targetYaw.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";scale=" + targetScaleFactor.ToString(System.Globalization.CultureInfo.InvariantCulture));
                gestureActive = false;
            }
        }

        private void HandlePinchAndScrollScale()
        {
            // --- A. Two-Finger Pinch-to-Scale (Mobile) ---
            if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                if (!IsPointerOverUI(t0.position) && !IsPointerOverUI(t1.position))
                {
                    if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                    {
                        previousTouchPos0 = t0.position;
                        previousTouchPos1 = t1.position;
                    }
                    else if (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
                    {
                        float prevDistance = Vector2.Distance(previousTouchPos0, previousTouchPos1);
                        float currentDistance = Vector2.Distance(t0.position, t1.position);
                        float delta = currentDistance - prevDistance;

                        targetScaleFactor += delta * pinchScaleSpeed;
                        targetScaleFactor = Mathf.Clamp(targetScaleFactor, minScale, maxScale);

                        previousTouchPos0 = t0.position;
                        previousTouchPos1 = t1.position;
                    }
                }
            }
            // --- B. Mouse Scroll Wheel Scale (Desktop / Editor) ---
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f && !IsPointerOverUI(Input.mousePosition))
            {
                targetScaleFactor += scroll * scrollScaleSpeed;
                targetScaleFactor = Mathf.Clamp(targetScaleFactor, minScale, maxScale);
            }
        }

        private void HandleViewOnly360()
        {
            bool userInteracting = false;

            // 1-Finger Touch Drag on Mobile
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (!IsPointerOverUI(touch.position))
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        targetYaw -= touch.deltaPosition.x * dragRotateSpeed;
                        userInteracting = true;
                        lastInteractionTime = Time.time;
                    }
                }
            }
            // Mouse Drag in Editor / Desktop
            else if (Input.GetMouseButton(0) && !IsPointerOverUI(Input.mousePosition))
            {
                float deltaX = Input.GetAxis("Mouse X");
                if (Mathf.Abs(deltaX) > 0.01f)
                {
                    targetYaw -= deltaX * dragRotateSpeed * 15f;
                    userInteracting = true;
                    lastInteractionTime = Time.time;
                }
            }

            // Auto orbit when idle (after 3s of no input)
            if (currentMode == InteractionMode.ViewOnly && !StudyPolicy.Measured && autoOrbitInViewOnly && !userInteracting && (Time.time - lastInteractionTime > 3f))
            {
                targetYaw += autoOrbitSpeed * Time.deltaTime;
            }
        }

        private void HandleInteractiveLeverDrag()
        {
            // In Interactive mode, user can drag down the Effort point to preview lifting
            if (beamPivot == null) return;

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (!IsPointerOverUI(touch.position) && touch.position.x > Screen.width * 0.5f)
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        currentBeamTilt += touch.deltaPosition.y * tiltSensitivity;
                        currentBeamTilt = Mathf.Clamp(currentBeamTilt, -maxTiltAngle, maxTiltAngle);
                    }
                }
            }
        }

        private void ApplySmoothTransforms()
        {
            if (modelRoot == null) return;

            // Smooth Yaw Rotation
            currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * orbitDamping);
            modelRoot.localRotation = Quaternion.Euler(0f, currentYaw, 0f);

            // Smooth Scale
            currentScaleFactor = Mathf.Lerp(currentScaleFactor, targetScaleFactor, Time.deltaTime * 8f);
            modelRoot.localScale = initialScale * currentScaleFactor;
        }

        private bool IsPointerOverUI(Vector2 screenPos)
        {

            if (EventSystem.current == null) return false;
            var pointer = new PointerEventData(EventSystem.current) { position = screenPos };
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            return hits.Count > 0;
        }
    }
}
