using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SecretOfLostTemple.Assessment
{
    [Serializable]
    public class RetentionAttempt
    {
        public string timestamp;
        public bool correct;
        public float responseTimeSeconds;
        public float daysSinceFirstAttempt;
        public string intervalLabel;
    }

    [Serializable]
    public class RetentionRecord
    {
        public string questionId;
        public bool firstAttemptCorrect;
        public float firstAttemptTime;
        public string firstAttemptTimestamp;
        public List<RetentionAttempt> retentionAttempts = new List<RetentionAttempt>();
    }

    [Serializable]
    public class RetentionSummary
    {
        public string userId;
        public int totalQuestionsTracked;
        public float immediateRetentionRate;
        public float shortTermRetentionRate;
        public float longTermRetentionRate;
        public float overallRetentionRate;
        public float averageRetentionDecay;
        public bool hasPostTest, hasRetentionTest;
    }

    [Serializable]
    public class RetentionDataStore
    {
        public List<RetentionRecord> records = new List<RetentionRecord>();
    }

    /// <summary>
    /// Implements spaced retrieval testing for knowledge retention research.
    /// </summary>
    public class LearningRetentionTracker : MonoBehaviour
    {
        public static LearningRetentionTracker Instance { get; private set; }

        [SerializeField] private float[] retentionIntervalsDays = { 0, 1, 3, 7 };
        
        private RetentionDataStore dataStore = new RetentionDataStore();
        private string prefsKey = "RetentionDataStore";
        private string loadedParticipant;
        void EnsureParticipant() { if (loadedParticipant != (ResearchData.Store.ParticipantCode ?? "practice")) LoadFromPlayerPrefs(); }

        public event Action<RetentionSummary> OnRetentionUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromPlayerPrefs();
        }

        public void RecordFirstAttempt(string questionId, bool correct, float responseTime)
        {
            EnsureParticipant();
            var record = dataStore.records.FirstOrDefault(r => r.questionId == questionId);
            if (record == null)
            {
                record = new RetentionRecord
                {
                    questionId = questionId,
                    firstAttemptCorrect = correct,
                    firstAttemptTime = responseTime,
                    firstAttemptTimestamp = DateTime.UtcNow.ToString("o")
                };
                dataStore.records.Add(record);
                SaveToPlayerPrefs();
            }
        }

        public void RecordRetentionAttempt(string questionId, bool correct, float responseTime)
        {
            EnsureParticipant();
            var record = dataStore.records.FirstOrDefault(r => r.questionId == questionId);
            if (record != null)
            {
                DateTime firstDate = DateTime.Parse(record.firstAttemptTimestamp);
                float daysSince = (float)(DateTime.UtcNow - firstDate).TotalDays;
                
                ResearchData.Record("review_answer", questionId, true, correct, responseTime);
                string label = "immediate";
                if (daysSince >= 7) label = "7day";
                else if (daysSince >= 3) label = "3day";
                else if (daysSince >= 1) label = "1day";

                record.retentionAttempts.Add(new RetentionAttempt
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    correct = correct,
                    responseTimeSeconds = responseTime,
                    daysSinceFirstAttempt = daysSince,
                    intervalLabel = label
                });

                SaveToPlayerPrefs();
                OnRetentionUpdated?.Invoke(CalculateOverallRetention());
            }
        }

        public List<string> GetDueRetentionQuestions()
        {
            var session = ResearchData.Store.Current ?? ResearchData.Store.Latest;
            if (session == null || ResearchStore.LatestTest(session, "posttest") == null || ResearchStore.LatestTest(session, "retention") != null || ResearchData.Store.DaysSinceLearning(session) < 7) return new List<string>();
            return LeverTestBank.Questions.Select(q => q.id).ToList();
        }

        public float CalculateRetentionRate(string questionId)
        {
            EnsureParticipant();
            var record = dataStore.records.FirstOrDefault(r => r.questionId == questionId);
            if (record == null || record.retentionAttempts.Count == 0) return 0f;

            int correctCount = record.retentionAttempts.Count(a => a.correct);
            return (float)correctCount / record.retentionAttempts.Count;
        }

        public RetentionSummary CalculateOverallRetention()
        {
            var session = ResearchData.Store.Current ?? ResearchData.Store.Latest;
            var post = ResearchStore.LatestTest(session, "posttest");
            var delayed = ResearchStore.LatestTest(session, "retention");
            return new RetentionSummary {
                userId = session?.participant ?? "practice", totalQuestionsTracked = delayed?.denominator ?? 0,
                hasPostTest = post != null, hasRetentionTest = delayed != null,
                immediateRetentionRate = post == null ? float.NaN : (float)post.score / post.denominator,
                shortTermRetentionRate = float.NaN,
                longTermRetentionRate = delayed == null ? float.NaN : (float)delayed.score / delayed.denominator,
                overallRetentionRate = delayed == null ? float.NaN : (float)delayed.score / delayed.denominator,
                averageRetentionDecay = post == null || delayed == null ? float.NaN : (float)post.score/post.denominator - (float)delayed.score/delayed.denominator
            };
        }

        public List<Vector2> GetRetentionCurveData()
        {
            var result = new List<Vector2>();
            var session = ResearchData.Store.Current ?? ResearchData.Store.Latest;
            if (session == null || string.IsNullOrEmpty(session.learningCompleted)) return result;
            var completed = DateTime.Parse(session.learningCompleted, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
            foreach (string stage in new[] { "posttest", "retention" }) {
                var test = ResearchStore.LatestTest(session, stage);
                if (test != null) result.Add(new Vector2((float)(DateTime.Parse(test.completed, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind) - completed).TotalDays, 100f*test.score/test.denominator));
            }
            return result;
        }

        public void SyncToFirestore()
        {
            // Placeholder: SecretOfLostTemple.Auth.FirestoreService.Instance.Sync(dataStore);
            Debug.Log("Retention data is local; cloud sync is not implemented.");
        }

        private void LoadFromPlayerPrefs()
        {
            loadedParticipant = ResearchData.Store.ParticipantCode ?? "practice";
            string json = ResearchProgress.GetString(prefsKey, "");
            dataStore = string.IsNullOrEmpty(json) ? new RetentionDataStore() : JsonUtility.FromJson<RetentionDataStore>(json);
            if (dataStore == null) dataStore = new RetentionDataStore();
        }

        private void SaveToPlayerPrefs()
        {
            string json = JsonUtility.ToJson(dataStore);
            ResearchProgress.SetString(prefsKey, json);
            PlayerPrefs.Save();
        }
    }
}
