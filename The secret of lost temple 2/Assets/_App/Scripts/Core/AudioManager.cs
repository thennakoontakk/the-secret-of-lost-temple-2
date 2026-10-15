using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SecretOfLostTemple.Core
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Default Music Clip")]
        [SerializeField] private AudioClip templeBGMClip;
        [SerializeField] private float defaultBGMVolume = 0.65f;
        [SerializeField] private bool autoPlayAfterSplash = true;

        private Coroutine fadeRoutine;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSources();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void InitializeSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.volume = defaultBGMVolume;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            if (templeBGMClip == null)
            {
                templeBGMClip = Resources.Load<AudioClip>("ancient_temple_bgm");
            }
        }

        private void Start()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene != "SplashScreen" && autoPlayAfterSplash)
            {
                PlayTempleBGM();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SplashScreen")
            {
                // In splash screen, video plays its own audio
                StopBGM(0.3f);
            }
            else
            {
                // In all game scenes (AuthScene, HomeMapScene, MainARScene), play ambient temple BGM
                if (!IsBGMPlaying() && autoPlayAfterSplash)
                {
                    PlayTempleBGM();
                }
            }
        }

        public void PlayTempleBGM(float fadeDuration = 1.2f)
        {
            if (templeBGMClip != null)
            {
                PlayBGM(templeBGMClip, true, fadeDuration);
            }
        }

        public void PlayBGM(AudioClip clip, bool loop = true, float fadeDuration = 1.0f)
        {
            if (clip == null) return;
            if (bgmSource == null) InitializeSources();

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeInMusicRoutine(clip, loop, fadeDuration));
        }

        public void StopBGM(float fadeDuration = 0.8f)
        {
            if (bgmSource == null || !bgmSource.isPlaying) return;
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOutMusicRoutine(fadeDuration));
        }

        public void SetBGMVolume(float volume)
        {
            defaultBGMVolume = Mathf.Clamp01(volume);
            if (bgmSource != null)
            {
                bgmSource.volume = defaultBGMVolume;
            }
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null) return;
            if (sfxSource == null) InitializeSources();
            sfxSource.PlayOneShot(clip, volumeScale);
        }

        public bool IsBGMPlaying()
        {
            return bgmSource != null && bgmSource.isPlaying;
        }

        private IEnumerator FadeInMusicRoutine(AudioClip newClip, bool loop, float duration)
        {
            float targetVol = defaultBGMVolume;

            // Fade out current if playing
            if (bgmSource.isPlaying)
            {
                float startVol = bgmSource.volume;
                float elapsed = 0f;
                while (elapsed < duration * 0.5f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (duration * 0.5f));
                    yield return null;
                }
            }

            bgmSource.Stop();
            bgmSource.clip = newClip;
            bgmSource.loop = loop;
            bgmSource.volume = 0f;
            bgmSource.Play();

            // Fade in
            float fadeElapsed = 0f;
            while (fadeElapsed < duration)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, targetVol, fadeElapsed / duration);
                yield return null;
            }
            bgmSource.volume = targetVol;
        }

        private IEnumerator FadeOutMusicRoutine(float duration)
        {
            float startVol = bgmSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
                yield return null;
            }
            bgmSource.Stop();
            bgmSource.volume = defaultBGMVolume;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
