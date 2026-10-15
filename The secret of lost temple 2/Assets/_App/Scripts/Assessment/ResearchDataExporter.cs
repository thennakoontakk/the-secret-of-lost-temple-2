using System;
using System.Collections.Generic;
using UnityEngine;
using ResearchData = SecretOfLostTemple.Research.Research;
namespace SecretOfLostTemple.Assessment
{
    [Serializable] public class UnifiedResearchRecord
    {
        public string participantId, sessionId, timestamp;
        public string formalAssessmentStatus = "not_collected";
    }
    public class ResearchDataExporter : MonoBehaviour
    {
        public static ResearchDataExporter Instance { get; private set; }
        void Awake() { if (Instance != null && Instance != this) { Destroy(this); return; } Instance = this; DontDestroyOnLoad(gameObject); }
        public UnifiedResearchRecord CollectAllData(string ignoredAccountId)
        {
            var s = ResearchData.Store.Current ?? ResearchData.Store.Latest;
            return s == null ? null : new UnifiedResearchRecord { participantId = s.participant, sessionId = s.id, timestamp = s.started };
        }
        public string ExportSingleRecordToCSV(UnifiedResearchRecord record, string fileName = "research.csv") => ResearchData.Store.Export();
        public void ExportToCSV(List<UnifiedResearchRecord> records, string fileName) { ResearchData.Store.Export(); }
        public void ExportToFirestore(UnifiedResearchRecord record) { Debug.Log("Research records are local. Cloud sync pending backend implementation."); }
        public string GenerateSessionSummary(string userId) => ResearchData.Summary();
        public string GetResearchMetricsForRQ1() => "Attention and reasoning: not measured";
        public string GetResearchMetricsForRQ2() => "Engagement and motivation: not measured";
        public string GetResearchMetricsForRQ3() => ResearchData.FormalStatus;
    }
}
