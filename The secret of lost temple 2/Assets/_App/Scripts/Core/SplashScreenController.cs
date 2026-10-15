using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace SecretOfLostTemple.Core
{
    public class SplashScreenController : MonoBehaviour
    {
        [Header("Video Settings")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private bool skipOnTap = true;
        [SerializeField] private float minimumPlayTime = 1.5f;
        [SerializeField] private float maxDurationFallback = 12.0f;

        [Header("Scene Transition")]
        [SerializeField] private string nextSceneName = "AuthScene";
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private bool hasTransitioned = false;
        private float timer = 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ForceLandscapeOrientation()
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

        private void Awake()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }

            if (videoPlayer != null)
            {
                videoPlayer.aspectRatio = VideoAspectRatio.FitOutside;
            }

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
            }
        }

        private void Start()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached += OnVideoFinished;
                videoPlayer.errorReceived += OnVideoError;
                videoPlayer.Play();
            }
            else
            {
                Debug.LogWarning("[SplashScreen] No VideoPlayer found, transitioning directly.");
                StartCoroutine(TransitionToNextScene());
            }

            StartCoroutine(FallbackTimerRoutine());
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (skipOnTap && timer >= minimumPlayTime && !hasTransitioned)
            {
                if (IsUserTapped())
                {
                    OnSkipPressed();
                }
            }
        }

        private bool IsUserTapped()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Pointer.current != null && UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    return true;
                }
            }
            catch {}
#endif
            return false;
        }

        public void OnSkipPressed()
        {
            if (!hasTransitioned)
            {
                StartCoroutine(TransitionToNextScene());
            }
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            if (!hasTransitioned)
            {
                StartCoroutine(TransitionToNextScene());
            }
        }

        private void OnVideoError(VideoPlayer source, string message)
        {
            Debug.LogError($"[SplashScreen] Video error: {message}");
            if (!hasTransitioned)
            {
                StartCoroutine(TransitionToNextScene());
            }
        }

        private IEnumerator FallbackTimerRoutine()
        {
            yield return new WaitForSeconds(maxDurationFallback);
            if (!hasTransitioned)
            {
                StartCoroutine(TransitionToNextScene());
            }
        }

        private IEnumerator TransitionToNextScene()
        {
            if (hasTransitioned) yield break;
            hasTransitioned = true;

            if (fadeCanvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                    yield return null;
                }
                fadeCanvasGroup.alpha = 1f;
            }

            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning($"[SplashScreen] Scene '{nextSceneName}' not found in Build Settings. Loading next index or fallback.");
                if (SceneManager.sceneCountInBuildSettings > 1)
                {
                    SceneManager.LoadScene(1);
                }
            }
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
                videoPlayer.errorReceived -= OnVideoError;
            }
        }
    }
}
