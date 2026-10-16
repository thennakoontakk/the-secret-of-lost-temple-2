using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System;
using UnityEngine;

namespace SecretOfLostTemple.Adaptive
{
    /// <summary>
    /// Represents the difficulty tier for the adaptive difficulty system.
    /// </summary>
    public enum DifficultyTier { Apprentice = 1, Scholar = 2, Master = 3 }

    /// <summary>
    /// Configuration for a specific difficulty tier.
    /// </summary>
    [Serializable]
    public class DifficultyProfile
    {
        public DifficultyTier tier;
        public float puzzleComplexityMultiplier;
        public int maxForceVectors;
        public float timerDurationSeconds;
        public int machineParts;
        public bool showForceLabels;
        public bool showHintButton;
    }

    /// <summary>
    /// 3-tier adaptive difficulty system.
    /// Tracks student performance and adjusts difficulty parameters accordingly.
    /// </summary>
    public class AdaptiveDifficultyEngine : MonoBehaviour
    {
        private static AdaptiveDifficultyEngine instance;
        public static AdaptiveDifficultyEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AdaptiveDifficultyEngine>();
                    if (instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("AdaptiveDifficultyEngine");
                        instance = go.AddComponent<AdaptiveDifficultyEngine>();
                    }
                }
                return instance;
            }
            private set { instance = value; }
        }
        
        [SerializeField] private DifficultyTier currentTier = DifficultyTier.Apprentice;
        public DifficultyTier CurrentTier => currentTier;
        [SerializeField] private int consecutiveSuccessToPromote = 2;
        [SerializeField] private int consecutiveFailuresToDemote = 2;

        private int consecutiveSuccesses;
        private int consecutiveFailures;
        private int totalAttempts;
        private float totalTime;
        
        public event Action<DifficultyTier, DifficultyTier> OnDifficultyChanged;

        private DifficultyProfile[] profiles = new DifficultyProfile[]
        {
            new DifficultyProfile { tier = DifficultyTier.Apprentice, puzzleComplexityMultiplier = 1.0f, maxForceVectors = 2, timerDurationSeconds = 120, machineParts = 3, showForceLabels = true, showHintButton = true },
            new DifficultyProfile { tier = DifficultyTier.Scholar, puzzleComplexityMultiplier = 1.5f, maxForceVectors = 4, timerDurationSeconds = 90, machineParts = 5, showForceLabels = true, showHintButton = true },
            new DifficultyProfile { tier = DifficultyTier.Master, puzzleComplexityMultiplier = 2.0f, maxForceVectors = 6, timerDurationSeconds = 60, machineParts = 7, showForceLabels = false, showHintButton = false }
        };

        private void Awake()
        {
            if (instance == null || instance == this)
            {
                instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Records the result of a task and evaluates if a difficulty change is needed.
        /// </summary>
        public void RecordTaskResult(bool success, float timeSpent)
        {
            if (ResearchData.Store.IsStudy && !ResearchData.Store.Current.adaptive) return;
            totalAttempts++;
            totalTime += timeSpent;

            if (success)
            {
                consecutiveSuccesses++;
                consecutiveFailures = 0;
            }
            else
            {
                consecutiveFailures++;
                consecutiveSuccesses = 0;
            }

            EvaluateAndAdjust();
            SaveProgress();
        }

        /// <summary>
        /// Evaluates current performance and adjusts tier if thresholds are met.
        /// </summary>
        private void EvaluateAndAdjust()
        {
            if (consecutiveSuccesses >= consecutiveSuccessToPromote && currentTier < DifficultyTier.Master)
            {
                SetTier(currentTier + 1);
                consecutiveSuccesses = 0;
            }
            else if (consecutiveFailures >= consecutiveFailuresToDemote && currentTier > DifficultyTier.Apprentice)
            {
                SetTier(currentTier - 1);
                consecutiveFailures = 0;
            }
        }

        /// <summary>
        /// Retrieves the profile data for the current difficulty tier.
        /// </summary>
        public DifficultyProfile GetCurrentProfile()
        {
            return Array.Find(profiles, p => p.tier == currentTier);
        }

        /// <summary>
        /// Manually override the current difficulty tier.
        /// </summary>
        public void SetTier(DifficultyTier tier)
        {
            if (currentTier != tier)
            {
                var oldTier = currentTier;
                currentTier = tier;
                ResearchData.Record("difficulty_change", "from=" + oldTier + ";to=" + tier + ";success_streak=" + consecutiveSuccesses + ";failure_streak=" + consecutiveFailures + ";rule=lever-pilot-1");
                SaveProgress();
                OnDifficultyChanged?.Invoke(oldTier, tier);
            }
        }

        /// <summary>
        /// Resets the progression tracking and sets tier to Apprentice.
        /// </summary>
        public void ResetProgress()
        {
            consecutiveSuccesses = 0;
            consecutiveFailures = 0;
            totalAttempts = 0;
            totalTime = 0;
            SetTier(DifficultyTier.Apprentice);
        }
        
        public float GetAverageTimePerTask()
        {
            return totalAttempts > 0 ? totalTime / totalAttempts : 0;
        }

        private void SaveProgress()
        {
            ResearchProgress.SetInt("AdaptiveDifficultyTier", (int)currentTier);
            ResearchProgress.SetInt("AdaptiveSuccesses", consecutiveSuccesses);
            ResearchProgress.SetInt("AdaptiveFailures", consecutiveFailures);
        }

        public void ReloadParticipant() { totalAttempts = 0; totalTime = 0; LoadProgress(); }

        private void LoadProgress()
        {
            currentTier = (DifficultyTier)Mathf.Clamp(ResearchProgress.GetInt("AdaptiveDifficultyTier", 1), 1, 3);
            consecutiveSuccesses = ResearchProgress.GetInt("AdaptiveSuccesses", 0);
            consecutiveFailures = ResearchProgress.GetInt("AdaptiveFailures", 0);
        }
    }
}
