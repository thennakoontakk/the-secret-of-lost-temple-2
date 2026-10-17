using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Manages AR Camera background rendering and live WebCam feed fallback
    /// so the camera opens reliably on both Android ARCore devices and PC/WebCam setups.
    /// </summary>
    public class LiveWebCamBackground : MonoBehaviour
    {
        [Header("WebCam Raw Image (For Editor / PC / Non-ARCore Fallback)")]
        [SerializeField] private RawImage webCamDisplayImage;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;

        private WebCamTexture webCamTexture;
        private ARCameraBackground arCameraBackground;
        private ARCameraManager arCameraManager;
        private Camera arCamera;
        private Canvas backgroundCanvas;
        private bool isQuestMode;

        public bool IsCameraOpen =>
            (webCamTexture != null && webCamTexture.isPlaying && webCamTexture.width > 16) ||
            (ARSession.state == ARSessionState.SessionTracking || ARSession.state == ARSessionState.SessionInitializing);

        private void Awake()
        {
            arCameraBackground = GetComponent<ARCameraBackground>();
            arCameraManager = GetComponent<ARCameraManager>();
            arCamera = GetComponent<Camera>();

            if (webCamDisplayImage != null && webCamDisplayImage.texture == null)
            {
                webCamDisplayImage.color = Color.clear;
                webCamDisplayImage.enabled = false;
                webCamDisplayImage.gameObject.SetActive(false);
            }
        }

        private IEnumerator Start()
        {
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");
            isQuestMode = (appMode != "PhysicsLab");

            if (!isQuestMode)
            {
                // PHYSICS LAB MODE: Pure 3D virtual lab environment (no camera feed needed)
                if (arCameraBackground != null) arCameraBackground.enabled = false;
                if (arCameraManager != null) arCameraManager.enabled = false;
                if (webCamDisplayImage != null) webCamDisplayImage.gameObject.SetActive(false);

                var arSession = FindFirstObjectByType<ARSession>();
                if (arSession != null) arSession.enabled = false;

                if (arCamera != null)
                {
                    arCamera.clearFlags = CameraClearFlags.SolidColor;
                    arCamera.backgroundColor = new Color(0.05f, 0.07f, 0.10f, 1f);
                }

                yield break;
            }

            // QUEST MODE: Ensure Camera Permission on Android
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
                float permWait = 0f;
                while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera) && permWait < 6f)
                {
                    permWait += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
#else
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }
#endif

            if (arCamera != null)
            {
                arCamera.clearFlags = CameraClearFlags.SolidColor;
                arCamera.backgroundColor = new Color(0.06f, 0.08f, 0.10f, 1f);
            }

            bool arFoundationActive = false;

#if !UNITY_EDITOR
            // Initialize XR Loader if not already running on mobile device
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                var mgr = XRGeneralSettings.Instance.Manager;
                if (mgr.activeLoader == null)
                {
                    yield return mgr.InitializeLoader();
                }
                if (mgr.activeLoader != null)
                {
                    mgr.StartSubsystems();
                }
            }

            var session = FindFirstObjectByType<ARSession>();
            if (session != null) session.enabled = true;
            if (arCameraBackground != null) arCameraBackground.enabled = true;
            if (arCameraManager != null) arCameraManager.enabled = true;

            if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
            {
                yield return ARSession.CheckAvailability();
            }

            float waitAr = 0f;
            while (waitAr < 1.5f && ARSession.state != ARSessionState.SessionTracking && ARSession.state != ARSessionState.Unsupported)
            {
                waitAr += Time.unscaledDeltaTime;
                yield return null;
            }

            arFoundationActive = (ARSession.state == ARSessionState.SessionTracking || ARSession.state == ARSessionState.SessionInitializing);
#endif

            // If in Unity Editor (PC) or if ARCore is not actively tracking on this device, start live WebCamTexture
            if (!arFoundationActive)
            {
                StartLiveWebCamFeed();
            }
        }

        private void StartLiveWebCamFeed()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices == null || devices.Length == 0)
            {
                Debug.LogWarning("[LiveWebCamBackground] No physical webcam devices found.");
                return;
            }

            // Prefer back-facing camera on mobile/tablets, otherwise first camera
            string selectedCameraName = devices[0].name;
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    selectedCameraName = devices[i].name;
                    break;
                }
            }

            EnsureBackgroundCameraCanvas();

            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }

            webCamTexture = new WebCamTexture(selectedCameraName, 1280, 720, 30);
            webCamTexture.Play();

            if (webCamDisplayImage != null)
            {
                webCamDisplayImage.gameObject.SetActive(true);
                webCamDisplayImage.enabled = true;
                webCamDisplayImage.texture = webCamTexture;
                webCamDisplayImage.color = Color.white;
                webCamDisplayImage.raycastTarget = false;
            }

            Debug.Log($"[LiveWebCamBackground] Started live camera feed: {selectedCameraName}");
        }

        private void EnsureBackgroundCameraCanvas()
        {
            if (webCamDisplayImage == null || arCamera == null) return;

            if (backgroundCanvas == null)
            {
                var bgCanvasGo = new GameObject("AR_CameraFeedBackgroundCanvas");
                backgroundCanvas = bgCanvasGo.AddComponent<Canvas>();
                backgroundCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                backgroundCanvas.worldCamera = arCamera;
                backgroundCanvas.planeDistance = 50f; // Behind 3D Lever (0.85m), inside far clip plane (1000m)
                backgroundCanvas.sortingOrder = -100;

                var scaler = bgCanvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            webCamDisplayImage.transform.SetParent(backgroundCanvas.transform, false);
            var rt = webCamDisplayImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        private void Update()
        {
            if (webCamDisplayImage == null) return;

            // If ARCore is actively tracking on device, let ARCameraBackground render hardware passthrough
            if (!Application.isEditor && ARSession.state == ARSessionState.SessionTracking && (webCamTexture == null || !webCamTexture.isPlaying))
            {
                if (webCamDisplayImage.enabled) webCamDisplayImage.enabled = false;
                if (webCamDisplayImage.gameObject.activeSelf) webCamDisplayImage.gameObject.SetActive(false);
                return;
            }

            if (webCamTexture != null && webCamTexture.isPlaying && webCamTexture.width > 16)
            {
                if (!webCamDisplayImage.gameObject.activeSelf) webCamDisplayImage.gameObject.SetActive(true);
                if (!webCamDisplayImage.enabled) webCamDisplayImage.enabled = true;
                webCamDisplayImage.color = Color.white;

                if (aspectRatioFitter != null)
                {
                    float ratio = (float)webCamTexture.width / Mathf.Max(1f, (float)webCamTexture.height);
                    aspectRatioFitter.aspectRatio = ratio;
                }

                float scaleY = webCamTexture.videoVerticallyMirrored ? -1f : 1f;
                webCamDisplayImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
                webCamDisplayImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, -webCamTexture.videoRotationAngle);
            }
            else if (webCamDisplayImage.texture == null)
            {
                if (webCamDisplayImage.enabled) webCamDisplayImage.enabled = false;
                if (webCamDisplayImage.gameObject.activeSelf) webCamDisplayImage.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
            if (backgroundCanvas != null)
            {
                Destroy(backgroundCanvas.gameObject);
            }
        }

        public WebCamTexture GetWebCamTexture() => webCamTexture;
    }
}
