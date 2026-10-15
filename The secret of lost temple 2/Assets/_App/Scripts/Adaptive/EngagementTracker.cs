using System;
using UnityEngine;

namespace SecretOfLostTemple.Adaptive
{
    [Serializable]
    public class EngagementData
    {
        public string sessionId;
        public string userId;
        public float totalSessionTimeSeconds;
        public float activeInteractionTimeSeconds;
        public float idleTimeSeconds;
        public int hintRequestCount;
        public int voluntaryReplays;
        public int sandboxVisits;
        public int storyBranchesExplored;
        public int totalDecisionsMade;
        public DifficultyTier highestDifficultyMaintained;
        public float motivationScore;
        public float engagementScore;
        public string difficultyProgression;
    }

    /// <summary>
    /// Measures engagement and motivation metrics for RQ2.
    /// Logs telemetry about user interactions and behavior.
    /// </summary>
    public class EngagementTracker : MonoBehaviour
    {
        public static EngagementTracker Instance { get; private set; }

        [SerializeField] private float idleThresholdSeconds = 10f;
        
        private EngagementData data;
        private bool isActive;
        private float lastActiveTime;
        private float sessionStartTime;
        private float lastScoreUpdateTime;

        public event Action<float> OnEngagementScoreUpdated;

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
            if (!isActive) return;

            data.totalSessionTimeSeconds = Time.time - sessionStartTime;

            if (Time.time - lastActiveTime > idleThresholdSeconds)
            {
                data.idleTimeSeconds += Time.deltaTime;
            }
            else
            {
                data.activeInteractionTimeSeconds += Time.deltaTime;
            }

            if (Time.time - lastScoreUpdateTime >= 30f)
            {
                CalculateEngagementScore();
                lastScoreUpdateTime = Time.time;
            }
        }

        public void StartTracking(string userId)
        {
            if (SecretOfLostTemple.Research.Research.Store.Current == null) { isActive = false; data = null; return; }
            data = new EngagementData
            {
                sessionId = SecretOfLostTemple.Research.Research.Store.ActiveId,
                userId = SecretOfLostTemple.Research.Research.Store.ParticipantCode,
                difficultyProgression = "1"
            };
            
            isActive = true;
            sessionStartTime = Time.time;
            lastActiveTime = Time.time;
            lastScoreUpdateTime = Time.time;
        }

        public void RecordInteraction()
        {
            lastActiveTime = Time.time;
        }

        public void RecordIdlePeriod(float duration)
        {
            if (data != null)
                data.idleTimeSeconds += duration;
        }

        public void RecordHintRequest()
        {
            if (data != null)
                data.hintRequestCount++;
            RecordInteraction();
        }

        public void RecordVoluntaryReplay()
        {
            if (data != null)
                data.voluntaryReplays++;
            RecordInteraction();
        }

        public void RecordSandboxVisit()
        {
            if (data != null)
                data.sandboxVisits++;
            RecordInteraction();
        }

        public void RecordStoryDecision(string choiceId)
        {
            if (data != null)
            {
                data.storyBranchesExplored++;
                data.totalDecisionsMade++;
            }
            RecordInteraction();
        }

        public void CalculateEngagementScore()
        {
            if (data == null || data.totalSessionTimeSeconds <= 0) return;

            float activeRatio = data.activeInteractionTimeSeconds / data.totalSessionTimeSeconds;
            data.engagementScore = Mathf.Clamp(activeRatio * 100f, 0, 100f);
            
            OnEngagementScoreUpdated?.Invoke(data.engagementScore);
        }

        public void CalculateMotivationScore()
        {
            if (data == null) return;
            float score = 50f;
            score += data.voluntaryReplays * 5f;
            score += data.sandboxVisits * 5f;
            score += (int)data.highestDifficultyMaintained * 10f;
            score -= data.hintRequestCount * 2f;
            
            data.motivationScore = Mathf.Clamp(score, 0, 100f);
        }

        public void StopTracking()
        {
            isActive = false;
            CalculateEngagementScore();
            CalculateMotivationScore();
            SyncToFirestore();
        }

        public EngagementData GetEngagementData()
        {
            return data;
        }

        private void SyncToFirestore()
        {
            // Syncs to SecretOfLostTemple.Auth.FirestoreService in a real implementation
            Debug.Log("Syncing engagement data to Firestore.");
        }
    }
}
