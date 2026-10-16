using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Manages AR Camera background rendering and WebCam fallbacks.
    /// </summary>
    public class LiveWebCamBackground : MonoBehaviour
    {
        [Header("WebCam Raw Image (For Editor / PC)")]
        [SerializeField] private RawImage webCamDisplayImage;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;

        private WebCamTexture webCamTexture;
        private ARCameraBackground arCameraBackground;
        private ARCameraManager arCameraManager;
        private Camera arCamera;

        private void Awake()
        {
            arCameraBackground = GetComponent<ARCameraBackground>();
            arCameraManager = GetComponent<ARCameraManager>();
            arCamera = GetComponent<Camera>();

            // An enabled RawImage without a texture renders opaque white over ARCameraBackground.
            // Ensure the unused WebCamRawImage overlay is disabled/hidden when no texture is present so the live AR camera feed is visible.
            if (webCamDisplayImage != null)
            {
                if (webCamDisplayImage.texture == null)
                {
                    webCamDisplayImage.color = Color.clear;
                    webCamDisplayImage.enabled = false;
                    webCamDisplayImage.gameObject.SetActive(false);
                }
            }
        }

        private void Update()
        {
            if (webCamDisplayImage != null)
            {
                if (webCamDisplayImage.texture == null || (webCamTexture != null && !webCamTexture.isPlaying))
                {
                    if (webCamDisplayImage.enabled) webCamDisplayImage.enabled = false;
                    if (webCamDisplayImage.gameObject.activeSelf) webCamDisplayImage.gameObject.SetActive(false);
                }
                else if (webCamTexture != null && webCamTexture.isPlaying)
                {
                    if (!webCamDisplayImage.gameObject.activeSelf) webCamDisplayImage.gameObject.SetActive(true);
                    if (!webCamDisplayImage.enabled) webCamDisplayImage.enabled = true;
                    webCamDisplayImage.color = Color.white;
                }
            }
        }

        private IEnumerator Start()
        {
            string appMode = PlayerPrefs.GetString("LostTemple_App_Mode", "Sandbox");
            bool isQuestMode = (appMode == "Quest");

            if (!isQuestMode)
            {
                // SANDBOX MODE: Pure 3D virtual temple environment
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

            // QUEST MODE
            if (arCamera != null)
            {
                arCamera.clearFlags = CameraClearFlags.SolidColor;
                arCamera.backgroundColor = Color.black;
            }

            var session = FindFirstObjectByType<ARSession>();
            if (session != null) session.enabled = true;
            if (arCameraBackground != null) arCameraBackground.enabled = true;
            if (arCameraManager != null) arCameraManager.enabled = true;

            yield break;
        }

        private void OnDestroy()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
        }

        public WebCamTexture GetWebCamTexture() => webCamTexture;
    }
}
