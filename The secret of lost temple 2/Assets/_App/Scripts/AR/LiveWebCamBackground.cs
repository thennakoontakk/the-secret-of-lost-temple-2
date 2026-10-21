using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Manages AR Camera background rendering and live WebCam feed fallback
    /// so the camera opens reliably on both Android ARCore devices and PC/WebCam setups.
    /// Dedicated to MainARScene (Expedition Marker Scanner).
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
        private bool isCameraScene;
        private bool arHardwareRenderingFrames;

        public bool IsCameraOpen =>
            (webCamTexture != null && webCamTexture.isPlaying && webCamTexture.width > 16) ||
            arHardwareRenderingFrames ||
            (ARSession.state == ARSessionState.SessionTracking);

        private void Awake()
        {
            arCameraBackground = GetComponent<ARCameraBackground>();
            arCameraManager = GetComponent<ARCameraManager>();
            arCamera = GetComponent<Camera>();

            if (webCamDisplayImage == null)
            {
                var canvasGo = GameObject.Find("AR Main Canvas");
                if (canvasGo != null)
                {
                    var rawTr = canvasGo.transform.Find("WebCamRawImage");
                    if (rawTr != null)
                    {
                        webCamDisplayImage = rawTr.GetComponent<RawImage>();
                        aspectRatioFitter = rawTr.GetComponent<AspectRatioFitter>();
                    }
                }
            }

            if (webCamDisplayImage != null && webCamDisplayImage.texture == null)
            {
                webCamDisplayImage.color = Color.clear;
                webCamDisplayImage.enabled = false;
                webCamDisplayImage.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (arCameraManager != null)
            {
                arCameraManager.frameReceived += OnARCameraFrameReceived;
            }
        }

        private void OnDisable()
        {
            if (arCameraManager != null)
            {
                arCameraManager.frameReceived -= OnARCameraFrameReceived;
            }
        }

        private void OnARCameraFrameReceived(ARCameraFrameEventArgs args)
        {
            arHardwareRenderingFrames = true;
        }

        private IEnumerator Start()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Quest");

            // MainARScene is exclusively for Expedition Marker Scan (Quest Mode).
            // FreePracticeScene and Physics3DLabScene are pure 3D virtual scenes and do not use the camera.
            isCameraScene = (sceneName == "MainARScene" && appMode != "PhysicsLab");

            if (!isCameraScene)
            {
                if (arCameraBackground != null) arCameraBackground.enabled = false;
                if (arCameraManager != null) arCameraManager.enabled = false;
                if (webCamDisplayImage != null) webCamDisplayImage.gameObject.SetActive(false);

                var arSession = FindFirstObjectByType<ARSession>();
                if (arSession != null) arSession.enabled = false;

                if (arCamera != null)
                {
                    arCamera.clearFlags = CameraClearFlags.SolidColor;
                    arCamera.backgroundColor = new Color(0.08f, 0.12f, 0.10f, 1f);
                }

                yield break;
            }

            // EXPEDITION MARKER SCAN (MainARScene): Request Camera Permission
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

#if UNITY_EDITOR
            // In Unity Editor, ARCameraBackground cannot render physical webcam frames.
            // Disable ARSession/ARCameraBackground and immediately open the live physical WebCamTexture!
            var editorSession = FindFirstObjectByType<ARSession>();
            if (editorSession != null) editorSession.enabled = false;
            if (arCameraBackground != null) arCameraBackground.enabled = false;
            if (arCameraManager != null) arCameraManager.enabled = false;

            StartLiveWebCamFeed();
            yield break;
#else
            // On mobile device: Initialize XR Loader & ARSession
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
            if (session != null)
            {
                session.gameObject.SetActive(true);
                session.enabled = true;
            }
            if (arCameraBackground != null) arCameraBackground.enabled = true;
            if (arCameraManager != null) arCameraManager.enabled = true;

            if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
            {
                yield return ARSession.CheckAvailability();
            }

            float waitAr = 0f;
            while (waitAr < 1.8f && !arHardwareRenderingFrames && ARSession.state != ARSessionState.SessionTracking && ARSession.state != ARSessionState.Unsupported)
            {
                waitAr += Time.unscaledDeltaTime;
                yield return null;
            }

            // If ARCore is not actively delivering camera frames on this device, open WebCamTexture directly so the camera ALWAYS opens!
            if (!arHardwareRenderingFrames && ARSession.state != ARSessionState.SessionTracking)
            {
                if (arCameraBackground != null) arCameraBackground.enabled = false;
                if (arCameraManager != null) arCameraManager.enabled = false;
                if (session != null) session.enabled = false;
                StartLiveWebCamFeed();
            }
#endif
        }

        public void StartLiveWebCamFeed()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                return;
            }

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
            if (webCamDisplayImage == null) return;

            if (arCamera == null) arCamera = Camera.main;

            if (backgroundCanvas == null && arCamera != null)
            {
                var bgCanvasGo = new GameObject("AR_CameraFeedBackgroundCanvas");
                backgroundCanvas = bgCanvasGo.AddComponent<Canvas>();
                backgroundCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                backgroundCanvas.worldCamera = arCamera;
                float safePlaneDist = Mathf.Clamp(arCamera.farClipPlane * 0.5f, 5f, 50f);
                backgroundCanvas.planeDistance = safePlaneDist; // Behind 3D Lever (0.60m), inside camera farClipPlane
                backgroundCanvas.sortingOrder = -100;

                var scaler = bgCanvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (backgroundCanvas != null)
            {
                webCamDisplayImage.transform.SetParent(backgroundCanvas.transform, false);
            }
            else
            {
                webCamDisplayImage.transform.SetAsFirstSibling();
            }

            var rt = webCamDisplayImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        private void Update()
        {
            if (!isCameraScene || webCamDisplayImage == null) return;

            // If ARCore is actively delivering frames on device, let ARCameraBackground render hardware passthrough
            if (!Application.isEditor && (arHardwareRenderingFrames || ARSession.state == ARSessionState.SessionTracking) && (webCamTexture == null || !webCamTexture.isPlaying))
            {
                if (webCamDisplayImage.enabled) webCamDisplayImage.enabled = false;
                if (webCamDisplayImage.gameObject.activeSelf) webCamDisplayImage.gameObject.SetActive(false);
                return;
            }

            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                if (!webCamDisplayImage.gameObject.activeSelf) webCamDisplayImage.gameObject.SetActive(true);
                if (!webCamDisplayImage.enabled) webCamDisplayImage.enabled = true;
                webCamDisplayImage.texture = webCamTexture;
                webCamDisplayImage.color = Color.white;

                if (webCamTexture.width > 16)
                {
                    if (aspectRatioFitter != null)
                    {
                        float ratio = (float)webCamTexture.width / Mathf.Max(1f, (float)webCamTexture.height);
                        aspectRatioFitter.aspectRatio = ratio;
                    }

                    float scaleY = webCamTexture.videoVerticallyMirrored ? -1f : 1f;
                    webCamDisplayImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
                    webCamDisplayImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, -webCamTexture.videoRotationAngle);
                }
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
