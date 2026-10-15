using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SecretOfLostTemple.AR
{
    [Serializable]
    public class InteractionEvent
    {
        public string eventType;
        public string machineId;
        public string timestamp;
        public float responseTimeSeconds;
        public string contextData;
    }

    [Serializable]
    public class ARSessionData
    {
        public string sessionId;
        public string userId;
        public string timestamp;
        public InteractionMode mode;
        public MachineType machineType;
        public int totalInteractions;
        public int errorCount;
        public int hintRequests;
        public float timeToFirstInteraction;
        public float totalDwellTimeSeconds;
        public float averageResponseTime;
        public float attentionScore;
        public float reasoningAccuracy;
        public float errorRate;
        public List<InteractionEvent> events = new List<InteractionEvent>();
    }

    /// <summary>
    /// Singleton logger for recording per-session cognitive research data.
    /// Handles syncing telemetry through the FirestoreService.
    /// </summary>
    public class CognitiveTelemetryLogger : MonoBehaviour
    {
        private static CognitiveTelemetryLogger instance;
        public static CognitiveTelemetryLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("CognitiveTelemetryLogger");
                    instance = obj.AddComponent<CognitiveTelemetryLogger>();
                    if (Application.isPlaying) DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        private ARSessionData currentSession;
        private float lastInteractionTime;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initializes a new telemetry session.
        /// </summary>
        public void StartSession(string userId)
        {
            if (ResearchData.Store.Current == null) { currentSession = null; return; }
            currentSession = new ARSessionData
            {
                sessionId = ResearchData.Store.ActiveId,
                userId = ResearchData.Store.ParticipantCode,
                timestamp = DateTime.UtcNow.ToString("O"),
                events = new List<InteractionEvent>()
            };
            lastInteractionTime = Time.time;
        }

        /// <summary>
        /// Logs a generic interaction event.
        /// </summary>
        public void LogInteraction(string eventType, SimpleMachine machine, string context = "")
        {
            ResearchData.Record(eventType, context);
            if (currentSession == null || currentSession.sessionId != ResearchData.Store.ActiveId) StartSession("");
            if (currentSession == null) return;

            float responseTime = Time.time - lastInteractionTime;
            lastInteractionTime = Time.time;

            currentSession.totalInteractions++;
            
            if (machine != null && currentSession.timeToFirstInteraction == 0)
            {
                currentSession.timeToFirstInteraction = responseTime;
            }

            var evt = new InteractionEvent
            {
                eventType = eventType,
                machineId = machine != null ? machine.GetInstanceID().ToString() : "none",
                timestamp = DateTime.UtcNow.ToString("O"),
                responseTimeSeconds = responseTime,
                contextData = context
            };

            currentSession.events.Add(evt);
        }

        /// <summary>
        /// Logs when the user switches interaction modes.
        /// </summary>
        public void LogModeSwitch(InteractionMode from, InteractionMode to)
        {
            LogInteraction("mode_switch", null, $"{{\"from\":\"{from}\", \"to\":\"{to}\"}}");
            if (currentSession != null)
            {
                currentSession.mode = to;
            }
        }

        /// <summary>
        /// Logs an error made by the user.
        /// </summary>
        public void LogError(SimpleMachine machine, string errorType)
        {
            if (currentSession != null)
            {
                currentSession.errorCount++;
                LogInteraction("error", machine, $"{{\"errorType\":\"{errorType}\"}}");
            }
        }

        /// <summary>
        /// Calculates final cognitive metrics and closes the session.
        /// </summary>
        public void EndSession()
        {
            if (currentSession == null) return;
            
            CalculateCognitiveMetrics();
            // Research export is local until the backend milestone.
            
            currentSession = null;
        }

        private void CalculateCognitiveMetrics()
        {
            if (currentSession.totalInteractions > 0)
            {
                int correctActions = currentSession.totalInteractions - currentSession.errorCount;
                currentSession.reasoningAccuracy = (float)correctActions / currentSession.totalInteractions;
                currentSession.errorRate = (float)currentSession.errorCount / currentSession.totalInteractions;
                
                // Simplified attention score: depends on dwell time and interactions
                currentSession.attentionScore = Mathf.Clamp01((currentSession.totalInteractions * 2f + currentSession.totalDwellTimeSeconds / 60f) / 10f);
            }
        }

        private void SyncToFirestore()
        {
            string json = JsonUtility.ToJson(currentSession);
            Debug.Log($"[CognitiveTelemetryLogger] Syncing AR session to Firestore: {currentSession.sessionId}");

            string userId = PlayerPrefs.GetString("LostTemple_Auth_LocalId", "");
            string idToken = PlayerPrefs.GetString("LostTemple_Auth_Token", "");

            if (!string.IsNullOrEmpty(userId))
            {
                // Build Firestore REST document from session data
                string firestoreJson = $@"{{
  ""fields"": {{
    ""sessionId"": {{ ""stringValue"": ""{currentSession.sessionId}"" }},
    ""userId"": {{ ""stringValue"": ""{currentSession.userId}"" }},
    ""timestamp"": {{ ""stringValue"": ""{currentSession.timestamp}"" }},
    ""interactionMode"": {{ ""stringValue"": ""{currentSession.mode}"" }},
    ""totalInteractions"": {{ ""integerValue"": ""{currentSession.totalInteractions}"" }},
    ""errorCount"": {{ ""integerValue"": ""{currentSession.errorCount}"" }},
    ""timeToFirstInteraction"": {{ ""doubleValue"": {currentSession.timeToFirstInteraction} }},
    ""totalDwellTimeSeconds"": {{ ""doubleValue"": {currentSession.totalDwellTimeSeconds} }},
    ""attentionScore"": {{ ""doubleValue"": {currentSession.attentionScore} }},
    ""reasoningAccuracy"": {{ ""doubleValue"": {currentSession.reasoningAccuracy} }},
    ""errorRate"": {{ ""doubleValue"": {currentSession.errorRate} }}
  }}
}}";
                SecretOfLostTemple.Auth.FirestoreService.Instance.LogARSession(userId, idToken, firestoreJson);
            }
            else
            {
                Debug.LogWarning("[CognitiveTelemetryLogger] No userId found, storing locally only.");
            }
        }

        /// <summary>
        /// Returns the current session data.
        /// </summary>
        public ARSessionData GetCurrentSessionData()
        {
            return currentSession;
        }
    }
}
