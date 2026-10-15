using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SecretOfLostTemple.Adaptive
{
    public enum HintTier { VisualNudge = 1, CharacterClue = 2, DirectGuidance = 3 }

    [Serializable]
    public class HintData
    {
        public HintTier tier;
        public string hintText;
        public string characterVoiceLine;
        public string animationTrigger;
        public string targetObjectName;
    }

    [Serializable]
    public class HintUsageData
    {
        public string taskId;
        public int hintsGiven;
        public HintTier maxTierReached;
        public int tier1Count;
        public int tier2Count;
        public int tier3Count;
    }

    /// <summary>
    /// 3-tier progressive hint system.
    /// Provides tiered guidance based on time elapsed and error rates.
    /// </summary>
    public class HintSystem : MonoBehaviour
    {
        public static HintSystem Instance { get; private set; }

        [SerializeField] private float idleTimeBeforeHint = 15f;
        [SerializeField] private int errorsBeforeHint = 3;
        
        [Header("UI Elements")]
        [SerializeField] private Text hintDisplayText;
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private GameObject tier1GlitterEffect;
        [SerializeField] private GameObject tier3WalkthroughOverlay;
        [SerializeField] private Image characterPortraitImage;
        [SerializeField] private AudioClip hintSoundEffect;

        private HintTier currentHintTier = HintTier.VisualNudge;
        private int hintsGivenThisTask = 0;
        private float lastInteractionTime;
        private int currentErrors = 0;
        
        private string currentTaskId;
        private List<HintData> currentHints;

        // Per-tier tracking for research analytics
        private int tier1Count;
        private int tier2Count;
        private int tier3Count;
        
        public event Action<HintTier, string> OnHintShown;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            CheckAutoHint();
        }

        public void RegisterTask(string taskId, List<HintData> hints)
        {
            currentTaskId = taskId;
            currentHints = hints;
            ResetForNewTask();
        }

        public void RequestHint()
        {
            ShowHint(currentHintTier);
        }

        public void RecordError()
        {
            currentErrors++;
            lastInteractionTime = Time.time;
        }

        public void RecordInteraction()
        {
            lastInteractionTime = Time.time;
        }

        private void CheckAutoHint()
        {
            if (string.IsNullOrEmpty(currentTaskId) || currentHints == null || currentHints.Count == 0) return;

            if (Time.time - lastInteractionTime > idleTimeBeforeHint || currentErrors >= errorsBeforeHint)
            {
                ShowHint(currentHintTier);
                lastInteractionTime = Time.time;
                currentErrors = 0;
            }
        }

        private void ShowHint(HintTier tier)
        {
            if (currentHints == null) return;
            
            var hint = currentHints.Find(h => h.tier == tier);
            if (hint == null) return;

            if (hintPanel != null) hintPanel.SetActive(true);
            if (hintDisplayText != null) hintDisplayText.text = hint.hintText;

            if (tier == HintTier.VisualNudge)
            {
                if (tier1GlitterEffect != null) tier1GlitterEffect.SetActive(true);
                tier1Count++;
            }
            else if (tier == HintTier.CharacterClue)
            {
                if (characterPortraitImage != null) characterPortraitImage.gameObject.SetActive(true);
                tier2Count++;
            }
            else if (tier == HintTier.DirectGuidance)
            {
                if (tier3WalkthroughOverlay != null) tier3WalkthroughOverlay.SetActive(true);
                tier3Count++;
            }

            if (hintSoundEffect != null)
            {
                AudioSource.PlayClipAtPoint(hintSoundEffect, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            }

            hintsGivenThisTask++;
            
            if (currentHintTier < HintTier.DirectGuidance)
            {
                currentHintTier++;
            }

            OnHintShown?.Invoke(tier, currentTaskId);
            SecretOfLostTemple.Research.Research.Record("hint", currentTaskId + ":" + tier);
        }

        public void DismissHint()
        {
            if (hintPanel != null) hintPanel.SetActive(false);
            if (tier1GlitterEffect != null) tier1GlitterEffect.SetActive(false);
            if (characterPortraitImage != null) characterPortraitImage.gameObject.SetActive(false);
            if (tier3WalkthroughOverlay != null) tier3WalkthroughOverlay.SetActive(false);
        }

        public void ResetForNewTask()
        {
            currentHintTier = HintTier.VisualNudge;
            hintsGivenThisTask = 0;
            currentErrors = 0;
            lastInteractionTime = Time.time;
            DismissHint();
        }

        public void ResetParticipant()
        {
            ResetForNewTask();
            tier1Count = tier2Count = tier3Count = 0;
            currentTaskId = null;
            currentHints = null;
        }

        /// <summary>
        /// Returns structured hint usage data for research analytics (Component 2 - RQ2).
        /// </summary>
        public HintUsageData GetHintUsageData()
        {
            return new HintUsageData
            {
                taskId = currentTaskId,
                hintsGiven = hintsGivenThisTask,
                maxTierReached = currentHintTier,
                tier1Count = tier1Count,
                tier2Count = tier2Count,
                tier3Count = tier3Count
            };
        }
    }
}

