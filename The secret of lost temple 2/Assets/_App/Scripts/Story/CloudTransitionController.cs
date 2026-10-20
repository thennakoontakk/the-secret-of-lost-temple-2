using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SecretOfLostTemple.Story
{
    /// <summary>
    /// Clash of Clans style cloud wipe transition.
    /// Fluffy cartoon clouds roll in from left & right, completely blanketing the screen,
    /// triggering a page/scene swap callback, and then sweeping outward to reveal the new content.
    /// </summary>
    public class CloudTransitionController : MonoBehaviour
    {
        private static CloudTransitionController _instance;
        public static CloudTransitionController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CloudTransitionController>();
                    if (_instance == null)
                    {
                        var go = new GameObject("CloudTransitionController");
                        _instance = go.AddComponent<CloudTransitionController>();
                    }
                }
                return _instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private float closeDuration = 0.70f;
        [SerializeField] private float holdDuration = 0.12f;
        [SerializeField] private float openDuration = 0.76f;

        private Canvas transitionCanvas;
        private RectTransform containerRect;
        private CanvasGroup containerCanvasGroup;
        private Image centerCover;
        private List<CloudPuffData> leftPuffs = new List<CloudPuffData>();
        private List<CloudPuffData> rightPuffs = new List<CloudPuffData>();
        private Sprite cloudSprite;
        private AudioSource audioSource;
        private AudioClip whooshClip;
        private bool isTransitioning = false;

        public bool IsTransitioning => isTransitioning;

        private struct CloudPuffData
        {
            public RectTransform rect;
            public Image image;
            public Vector2 targetPos;
            public Vector2 offscreenPos;
            public float baseScale;
            public float baseRotation;
            public float rotSpeed;
        }

        private void Awake()
        {
            if (_instance == null) _instance = this;
            BuildCloudUIIfNeeded();
        }

        public void PlayCloudTransition(Action onScreenCovered, Action onTransitionComplete = null)
        {
            if (isTransitioning) return;
            BuildCloudUIIfNeeded();
            StartCoroutine(CloudTransitionRoutine(onScreenCovered, onTransitionComplete));
        }

        private void BuildCloudUIIfNeeded()
        {
            if (containerRect != null) return;

            cloudSprite = Resources.Load<Sprite>("cloud_puff") ?? Resources.Load<Sprite>("UI/cloud_puff");

            // Look for existing transition canvas or create a top-priority ScreenSpaceOverlay canvas
            GameObject canvasGo = new GameObject("CloudTransitionCanvas");
            canvasGo.transform.SetParent(transform, false);

            transitionCanvas = canvasGo.AddComponent<Canvas>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 9999; // Top of everything

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Root container
            var containerGo = new GameObject("CloudContainer", typeof(RectTransform), typeof(CanvasGroup));
            containerGo.transform.SetParent(canvasGo.transform, false);
            containerRect = containerGo.GetComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            containerCanvasGroup = containerGo.GetComponent<CanvasGroup>();

            // Center full veil for 100% opacity at peak
            var coverGo = new GameObject("CenterVeil", typeof(RectTransform), typeof(Image));
            coverGo.transform.SetParent(containerRect, false);
            var coverRt = coverGo.GetComponent<RectTransform>();
            coverRt.anchorMin = Vector2.zero;
            coverRt.anchorMax = Vector2.one;
            coverRt.offsetMin = Vector2.zero;
            coverRt.offsetMax = Vector2.zero;
            centerCover = coverGo.GetComponent<Image>();
            centerCover.color = new Color(0.96f, 0.98f, 1f, 0f);
            centerCover.raycastTarget = false;

            // Spawn stylized puffy clouds for left and right clusters
            leftPuffs.Clear();
            rightPuffs.Clear();

            // Left side cluster specs (anchored around left-center)
            Vector2[] leftOffsets = new Vector2[]
            {
                new Vector2(-380f, 0f),
                new Vector2(-220f, 260f),
                new Vector2(-200f, -240f),
                new Vector2(-80f, 80f),
                new Vector2(-100f, -80f),
                new Vector2(80f, 220f),
                new Vector2(60f, -200f),
                new Vector2(160f, 0f)
            };
            float[] leftScales = new float[] { 2.4f, 2.0f, 2.1f, 2.2f, 2.2f, 1.8f, 1.9f, 2.3f };

            // Right side cluster specs (anchored around right-center)
            Vector2[] rightOffsets = new Vector2[]
            {
                new Vector2(380f, 0f),
                new Vector2(220f, 260f),
                new Vector2(200f, -240f),
                new Vector2(80f, 80f),
                new Vector2(100f, -80f),
                new Vector2(-80f, 220f),
                new Vector2(-60f, -200f),
                new Vector2(-160f, 0f)
            };
            float[] rightScales = new float[] { 2.4f, 2.0f, 2.1f, 2.2f, 2.2f, 1.8f, 1.9f, 2.3f };

            for (int i = 0; i < leftOffsets.Length; i++)
            {
                leftPuffs.Add(CreateCloudPuff("LeftPuff_" + i, leftOffsets[i], new Vector2(-1400f, leftOffsets[i].y), leftScales[i], (i % 2 == 0 ? 15f : -15f)));
            }

            for (int i = 0; i < rightOffsets.Length; i++)
            {
                rightPuffs.Add(CreateCloudPuff("RightPuff_" + i, rightOffsets[i], new Vector2(1400f, rightOffsets[i].y), rightScales[i], (i % 2 == 0 ? -15f : 15f)));
            }

            // Audio setup
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            whooshClip = CreateProceduralWhooshSound();

            containerGo.SetActive(false);
        }

        private CloudPuffData CreateCloudPuff(string name, Vector2 targetPos, Vector2 offscreenPos, float scale, float rot)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(containerRect, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offscreenPos;
            rt.sizeDelta = new Vector2(512f, 512f);
            rt.localScale = Vector3.one * scale;
            rt.localEulerAngles = new Vector3(0f, 0f, rot);

            var img = go.GetComponent<Image>();
            if (cloudSprite != null) img.sprite = cloudSprite;
            img.color = Color.white;
            img.raycastTarget = false;

            return new CloudPuffData
            {
                rect = rt,
                image = img,
                targetPos = targetPos,
                offscreenPos = offscreenPos,
                baseScale = scale,
                baseRotation = rot,
                rotSpeed = (rot >= 0 ? 35f : -35f)
            };
        }

        private IEnumerator CloudTransitionRoutine(Action onScreenCovered, Action onTransitionComplete)
        {
            isTransitioning = true;
            containerRect.gameObject.SetActive(true);
            containerCanvasGroup.alpha = 1f;

            // Play whoosh sound
            if (audioSource != null && whooshClip != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(whooshClip, 0.75f);
            }

            // ----------------------------------------------------
            // PHASE 1: CLOUDS ROLL IN (Left & Right swoop to center)
            // ----------------------------------------------------
            float elapsed = 0f;
            while (elapsed < closeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float norm = Mathf.Clamp01(elapsed / closeDuration);
                // EaseOutCubic: fast swoop, soft decelerate
                float t = 1f - Mathf.Pow(1f - norm, 3f);

                AnimatePuffs(leftPuffs, t, true);
                AnimatePuffs(rightPuffs, t, true);

                // Subtle veil fade-in near peak
                if (centerCover != null)
                {
                    float coverAlpha = Mathf.Clamp01((norm - 0.6f) / 0.4f);
                    centerCover.color = new Color(0.96f, 0.98f, 1f, coverAlpha * 0.98f);
                }

                yield return null;
            }

            // Snap to 100% closed
            AnimatePuffs(leftPuffs, 1f, true);
            AnimatePuffs(rightPuffs, 1f, true);
            if (centerCover != null) centerCover.color = new Color(0.96f, 0.98f, 1f, 0.98f);

            // ----------------------------------------------------
            // MIDPOINT: SCREEN 100% COVERED -> SWAP CONTENT
            // ----------------------------------------------------
            try
            {
                onScreenCovered?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[CloudTransition] Error during onScreenCovered callback: " + ex);
            }

            if (holdDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(holdDuration);
            }

            // Play second soft whoosh for cloud parting
            if (audioSource != null && whooshClip != null)
            {
                audioSource.pitch = 1.15f;
                audioSource.PlayOneShot(whooshClip, 0.6f);
            }

            // ----------------------------------------------------
            // PHASE 2: CLOUDS PART & REVEAL (Sweep out & fade away)
            // ----------------------------------------------------
            elapsed = 0f;
            while (elapsed < openDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float norm = Mathf.Clamp01(elapsed / openDuration);
                // EaseInOutQuad
                float t = norm < 0.5f ? 2f * norm * norm : 1f - Mathf.Pow(-2f * norm + 2f, 2f) / 2f;

                AnimatePuffs(leftPuffs, t, false);
                AnimatePuffs(rightPuffs, t, false);

                // Fade veil quickly
                if (centerCover != null)
                {
                    float coverAlpha = Mathf.Clamp01(1f - norm * 2.5f);
                    centerCover.color = new Color(0.96f, 0.98f, 1f, coverAlpha * 0.98f);
                }

                // Global container alpha fade out in last 30%
                if (norm > 0.6f)
                {
                    containerCanvasGroup.alpha = Mathf.Clamp01((1f - norm) / 0.4f);
                }

                yield return null;
            }

            // Cleanup & complete
            containerCanvasGroup.alpha = 0f;
            containerRect.gameObject.SetActive(false);
            isTransitioning = false;

            try
            {
                onTransitionComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[CloudTransition] Error during onTransitionComplete callback: " + ex);
            }
        }

        private void AnimatePuffs(List<CloudPuffData> puffs, float t, bool closing)
        {
            for (int i = 0; i < puffs.Count; i++)
            {
                var p = puffs[i];
                if (closing)
                {
                    // Move from offscreen to target center
                    p.rect.anchoredPosition = Vector2.Lerp(p.offscreenPos, p.targetPos, t);
                    // Puffy swell effect
                    float scaleMultiplier = Mathf.Lerp(0.85f, 1.15f, t);
                    p.rect.localScale = Vector3.one * (p.baseScale * scaleMultiplier);
                    p.rect.localEulerAngles = new Vector3(0f, 0f, p.baseRotation + (p.rotSpeed * t));
                }
                else
                {
                    // Move from target center outward (parting away)
                    p.rect.anchoredPosition = Vector2.Lerp(p.targetPos, p.offscreenPos * 1.15f, t);
                    float scaleMultiplier = Mathf.Lerp(1.15f, 1.35f, t);
                    p.rect.localScale = Vector3.one * (p.baseScale * scaleMultiplier);
                    p.rect.localEulerAngles = new Vector3(0f, 0f, p.baseRotation + p.rotSpeed + (p.rotSpeed * t * 0.5f));
                }
            }
        }

        private AudioClip CreateProceduralWhooshSound()
        {
            int sampleRate = 44100;
            float duration = 0.85f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            float filter = 0f;
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                // Volume envelope: smooth crescendo then decay
                float env = Mathf.Sin(t * Mathf.PI);
                env = env * env;

                // White noise with simple dynamic lowpass filter sweep
                float noise = (UnityEngine.Random.value * 2f - 1f);
                float cutoff = Mathf.Lerp(0.04f, 0.25f, Mathf.Sin(t * Mathf.PI));
                filter += cutoff * (noise - filter);

                samples[i] = filter * env * 0.45f;
            }

            AudioClip clip = AudioClip.Create("CloudWhooshSFX", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
