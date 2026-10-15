using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Real-time On-Screen AR Diagnostic HUD and Logging Tool.
    /// Displays live tracking metrics, model world coordinates, distance,
    /// viewport visibility, and ARCore status directly on the mobile screen.
    /// </summary>
    public class ARDiagnosticsHUD : MonoBehaviour
    {
        [Header("Target References")]
        [SerializeField] private GameObject leverModel;
        [SerializeField] private ARTrackedImageManager imageManager;
        [SerializeField] private Text debugText;
        [SerializeField] private GameObject debugPanel;

        private ARSession arSession;
        private Camera arCamera;
        private ARTrackedImage lastTrackedImage;
        private float updateTimer = 0f;
        private bool isVisible = true;
        private StringBuilder sb = new StringBuilder(512);

        private void Awake()
        {
            arSession = FindFirstObjectByType<ARSession>();
            arCamera = Camera.main;
            if (imageManager == null) imageManager = FindFirstObjectByType<ARTrackedImageManager>();

            FindSceneReferences();
            CreateDebugUIIfNeeded();
        }

        private void FindSceneReferences()
        {
            if (leverModel == null)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name == "SimpleMachine_Lever")
                    {
                        leverModel = root;
                        break;
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (imageManager != null)
            {
                imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
            }
        }

        private void OnDisable()
        {
            if (imageManager != null)
            {
                imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
            }
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            foreach (var img in eventArgs.added)
            {
                lastTrackedImage = img;
                Debug.Log($"[ARDiagnostics] Image ADDED: {img.referenceImage.name} pos={img.transform.position}");
            }
            foreach (var img in eventArgs.updated)
            {
                lastTrackedImage = img;
            }
        }

        private void CreateDebugUIIfNeeded()
        {
            if (debugText != null) return;

            var canvas = GameObject.Find("AR Main Canvas");
            if (canvas == null) return;

            // Create floating diagnostic card at top center
            debugPanel = new GameObject("AR_Diagnostic_HUD", typeof(RectTransform), typeof(Image));
            debugPanel.transform.SetParent(canvas.transform, false);

            var rt = debugPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.72f);
            rt.anchorMax = new Vector2(0.95f, 0.88f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = debugPanel.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.82f); // Semi-transparent dark

            var textGO = new GameObject("DebugText", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(debugPanel.transform, false);

            var textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12f, 6f);
            textRt.offsetMax = new Vector2(-12f, -6f);

            debugText = textGO.GetComponent<Text>();
            debugText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (debugText.font == null) debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            debugText.fontSize = 20;
            debugText.color = Color.white;
            debugText.alignment = TextAnchor.MiddleLeft;
            debugText.supportRichText = true;

            // Add tap to toggle HUD size / visibility
            var btn = debugPanel.AddComponent<Button>();
            btn.onClick.AddListener(() => {
                isVisible = !isVisible;
                textGO.SetActive(isVisible);
                rt.anchorMin = isVisible ? new Vector2(0.05f, 0.72f) : new Vector2(0.85f, 0.84f);
                rt.anchorMax = isVisible ? new Vector2(0.95f, 0.88f) : new Vector2(0.95f, 0.88f);
            });
        }

        private void Update()
        {
            updateTimer += Time.unscaledDeltaTime;
            if (updateTimer < 0.15f) return; // Update 6 times per second
            updateTimer = 0f;

            if (debugText == null) return;
            if (arCamera == null) arCamera = Camera.main;
            if (leverModel == null) FindSceneReferences();

            sb.Clear();
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");

            // 1. AR Session & Mode
            sb.Append($"<b>[AR DEBUG]</b> Mode: <color=#ffe066>{appMode}</color> | ARCore: ");
            if (ARSession.state == ARSessionState.SessionTracking)
                sb.Append("<color=#80ffaa>TRACKING (OK)</color>");
            else if (ARSession.state == ARSessionState.Ready)
                sb.Append("<color=#ffff80>READY</color>");
            else
                sb.Append($"<color=#ff8080>{ARSession.state}</color>");

            // 2. Tracked Image Status
            sb.Append("\n🎯 <b>Marker:</b> ");
            if (lastTrackedImage != null)
            {
                string stateColor = (lastTrackedImage.trackingState == TrackingState.Tracking) ? "#80ffaa" : "#ffaa80";
                sb.Append($"'{lastTrackedImage.referenceImage.name}' (<color={stateColor}>{lastTrackedImage.trackingState}</color>)");
                Vector3 imgPos = lastTrackedImage.transform.position;
                sb.Append($" @ ({imgPos.x:F2}, {imgPos.y:F2}, {imgPos.z:F2})");
            }
            else
            {
                sb.Append("<color=#ff8080>NOT DETECTED YET (Scan Page 9)</color>");
            }

            // 3. 3D Model Coordinates & Viewport Visibility
            sb.Append("\n🏛️ <b>3D Lever:</b> ");
            if (leverModel != null)
            {
                bool active = leverModel.activeSelf && leverModel.activeInHierarchy;
                sb.Append(active ? "<color=#80ffaa>ACTIVE</color>" : "<color=#ff8080>INACTIVE</color>");

                Vector3 pos = leverModel.transform.position;
                sb.Append($" @ ({pos.x:F2}, {pos.y:F2}, {pos.z:F2}) S={leverModel.transform.localScale.x:F2}");

                if (arCamera != null && active)
                {
                    float dist = Vector3.Distance(arCamera.transform.position, pos);
                    Vector3 vp = arCamera.WorldToViewportPoint(pos);

                    bool onScreen = (vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f);
                    string vpStatus = onScreen ? "<color=#80ffaa>ON SCREEN</color>" :
                                      (vp.z < 0f) ? "<color=#ff8080>BEHIND CAMERA</color>" :
                                      "<color=#ffff80>OFF SCREEN SIDES</color>";

                    sb.Append($" | <b>View:</b> {vpStatus} [Vx:{vp.x:F2}, Vy:{vp.y:F2}, Z:{vp.z:F2}] (Dist: {dist:F2}m)");
                }
            }
            else
            {
                sb.Append("<color=#ff8080>NULL (Not found)</color>");
            }

            debugText.text = sb.ToString();
        }
    }
}
