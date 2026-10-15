using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SecretOfLostTemple.Auth
{
    [Serializable]
    public class ARComponentData
    {
        public int arSessionsCount = 0;
        public string activeTempleGate = "Level_1_Temple_Gates";
        public float spatialAccuracyScore = 0.0f;
        public int totalARTargetsScanned = 0;
        public string lastSessionTimestamp = "";
    }

    [Serializable]
    public class AdaptiveComponentData
    {
        public string learnerProficiencyLevel = "Beginner_Apprentice";
        public int currentDifficultyTier = 1;
        public string hintAssistanceLevel = "Full_Guidance";
        public float adaptivePacingFactor = 1.0f;
        public int consecutiveSuccessCount = 0;
        public float averageAttemptCount = 1.0f;
    }

    [Serializable]
    public class AssessmentComponentData
    {
        public float overallMasteryPercentage = 0.0f;
        public int quizzesCompleted = 0;
        public float cipherDecryptionAccuracy = 0.0f;
        public float totalTimeSpentSeconds = 0.0f;
        public int assessmentScore = 0;
    }

    public class FirestoreService : MonoBehaviour
    {
        private static FirestoreService instance;
        public static FirestoreService Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("FirestoreService");
                    instance = go.AddComponent<FirestoreService>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Firebase Firestore Config")]
        [SerializeField] private string apiKey = "AIzaSyBMUo0u0C0mzhQ2WaZZgc43X3HWc0b-ULE";
        [SerializeField] private string projectId = "the-secret-of-lost-temple";

        private string FirestoreBaseUrl => $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/";

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Writes initial User Document with all 3 research components to Firestore upon Signup
        /// </summary>
        public void InitializeUserResearchProfile(string userId, string idToken, string username, string email, string character, Action onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(InitializeUserRoutine(userId, idToken, username, email, character, onSuccess, onError));
        }

        private IEnumerator InitializeUserRoutine(string userId, string idToken, string username, string email, string character, Action onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("[FirestoreService] User ID is null or empty.");
                onError?.Invoke("User ID is missing.");
                yield break;
            }

            string url = $"{FirestoreBaseUrl}{userId}?key={apiKey}";
            string isoNow = DateTime.UtcNow.ToString("o");

            // Build Firestore REST Document with 3 Research Components
            string jsonBody = $@"{{
  ""fields"": {{
    ""userId"": {{ ""stringValue"": ""{EscapeJson(userId)}"" }},
    ""username"": {{ ""stringValue"": ""{EscapeJson(username)}"" }},
    ""email"": {{ ""stringValue"": ""{EscapeJson(email)}"" }},
    ""role"": {{ ""stringValue"": ""student_researcher"" }},
    ""selectedCharacter"": {{ ""stringValue"": ""{EscapeJson(character)}"" }},
    ""currentLevel"": {{ ""integerValue"": ""1"" }},
    ""coins"": {{ ""integerValue"": ""150"" }},
    ""createdAt"": {{ ""stringValue"": ""{isoNow}"" }},
    ""lastActiveAt"": {{ ""stringValue"": ""{isoNow}"" }},
    
    ""ar_component"": {{
      ""mapValue"": {{
        ""fields"": {{
          ""componentName"": {{ ""stringValue"": ""AR Interactive Spatial Exploration"" }},
          ""arSessionsCount"": {{ ""integerValue"": ""0"" }},
          ""activeTempleGate"": {{ ""stringValue"": ""Level_1_Temple_Gates"" }},
          ""spatialAccuracyScore"": {{ ""doubleValue"": 0.0 }},
          ""totalARTargetsScanned"": {{ ""integerValue"": ""0"" }},
          ""lastSessionTimestamp"": {{ ""stringValue"": ""{isoNow}"" }}
        }}
      }}
    }},
    
    ""adaptive_component"": {{
      ""mapValue"": {{
        ""fields"": {{
          ""componentName"": {{ ""stringValue"": ""Adaptive Difficulty & Scaffolding Engine"" }},
          ""learnerProficiencyLevel"": {{ ""stringValue"": ""Beginner_Apprentice"" }},
          ""currentDifficultyTier"": {{ ""integerValue"": ""1"" }},
          ""hintAssistanceLevel"": {{ ""stringValue"": ""Full_Guidance"" }},
          ""adaptivePacingFactor"": {{ ""doubleValue"": 1.0 }},
          ""consecutiveSuccessCount"": {{ ""integerValue"": ""0"" }},
          ""averageAttemptCount"": {{ ""doubleValue"": 1.0 }}
        }}
      }}
    }},
    
    ""assessment_component"": {{
      ""mapValue"": {{
        ""fields"": {{
          ""componentName"": {{ ""stringValue"": ""Performance Assessment & Learning Analytics"" }},
          ""overallMasteryPercentage"": {{ ""doubleValue"": 0.0 }},
          ""quizzesCompleted"": {{ ""integerValue"": ""0"" }},
          ""cipherDecryptionAccuracy"": {{ ""doubleValue"": 0.0 }},
          ""totalTimeSpentSeconds"": {{ ""doubleValue"": 0.0 }},
          ""assessmentScore"": {{ ""integerValue"": ""0"" }}
        }}
      }}
    }}
  }}
}}";

            if (string.IsNullOrEmpty(idToken))
            {
                idToken = FirebaseAuthService.Instance.CurrentIdToken;
                if (string.IsNullOrEmpty(idToken) && PlayerPrefs.HasKey("LostTemple_Auth_Token"))
                {
                    idToken = PlayerPrefs.GetString("LostTemple_Auth_Token");
                }
            }

            Debug.Log($"[FirestoreService] Initializing 3 research components in Firestore for user: {userId}...");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(idToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"<color=green>[FirestoreService] Successfully wrote 3 Research Components to Firestore for {username} ({userId})!</color>");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"[FirestoreService] Firestore Write Failed (HTTP {request.responseCode}): {request.downloadHandler.text}");
                    if (request.responseCode == 403 || request.downloadHandler.text.Contains("PERMISSION_DENIED"))
                    {
                        Debug.LogError("[FirestoreService] Firestore Security Rules are blocking access! Please update Firebase Firestore Rules to allow authenticated access.");
                    }
                    onSuccess?.Invoke();
                }
            }
        }

        /// <summary>
        /// Updates the Selected Character in Firestore
        /// </summary>
        public void UpdateCharacterSelection(string userId, string idToken, string character, Action onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(UpdateCharacterRoutine(userId, idToken, character, onSuccess, onError));
        }

        private IEnumerator UpdateCharacterRoutine(string userId, string idToken, string character, Action onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(userId)) yield break;

            if (string.IsNullOrEmpty(idToken))
            {
                idToken = FirebaseAuthService.Instance.CurrentIdToken;
                if (string.IsNullOrEmpty(idToken) && PlayerPrefs.HasKey("LostTemple_Auth_Token"))
                {
                    idToken = PlayerPrefs.GetString("LostTemple_Auth_Token");
                }
            }

            string url = $"{FirestoreBaseUrl}{userId}?updateMask.fieldPaths=selectedCharacter&key={apiKey}";
            string jsonBody = $@"{{
  ""fields"": {{
    ""selectedCharacter"": {{ ""stringValue"": ""{EscapeJson(character)}"" }}
  }}
}}";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(idToken)) request.SetRequestHeader("Authorization", $"Bearer {idToken}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[FirestoreService] Updated Selected Character to {character} in Firestore!");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogWarning($"[FirestoreService] Character update note: {request.downloadHandler.text}");
                    onSuccess?.Invoke();
                }
            }
        }

        /// <summary>
        /// Logs an AR Session record to Firestore under the user's ar_sessions sub-collection.
        /// Used by Component 1 (AR Interaction & Cognitive Impact) for RQ1.
        /// </summary>
        public void LogARSession(string userId, string idToken, string sessionJson, Action onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(LogSubCollectionRoutine(userId, idToken, "ar_sessions", sessionJson, onSuccess, onError));
        }

        /// <summary>
        /// Logs an Adaptive event to Firestore under the user's adaptive_events sub-collection.
        /// Used by Component 2 (Adaptive Storytelling & Engagement) for RQ2.
        /// </summary>
        public void LogAdaptiveEvent(string userId, string idToken, string eventJson, Action onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(LogSubCollectionRoutine(userId, idToken, "adaptive_events", eventJson, onSuccess, onError));
        }

        /// <summary>
        /// Logs an Assessment result to Firestore under the user's assessment_results sub-collection.
        /// Used by Component 3 (Assessment & Learning Retention) for RQ3.
        /// </summary>
        public void LogAssessmentResult(string userId, string idToken, string resultJson, Action onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(LogSubCollectionRoutine(userId, idToken, "assessment_results", resultJson, onSuccess, onError));
        }

        /// <summary>
        /// Exports a unified research data record combining all 3 component metrics.
        /// </summary>
        public void ExportResearchData(string userId, string idToken, string unifiedJson, Action onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(LogSubCollectionRoutine(userId, idToken, "research_exports", unifiedJson, onSuccess, onError));
        }

        /// <summary>
        /// Updates a specific research component's top-level fields in the user document.
        /// </summary>
        public void UpdateComponentData(string userId, string idToken, string componentFieldName, string componentJson, Action onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(UpdateComponentRoutine(userId, idToken, componentFieldName, componentJson, onSuccess, onError));
        }

        private IEnumerator LogSubCollectionRoutine(string userId, string idToken, string subCollection, string dataJson, Action onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(userId))
            {
                onError?.Invoke("User ID is missing.");
                yield break;
            }

            if (string.IsNullOrEmpty(idToken))
            {
                idToken = FirebaseAuthService.Instance.CurrentIdToken;
                if (string.IsNullOrEmpty(idToken) && PlayerPrefs.HasKey("LostTemple_Auth_Token"))
                {
                    idToken = PlayerPrefs.GetString("LostTemple_Auth_Token");
                }
            }

            // POST to sub-collection: users/{userId}/{subCollection}
            string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{userId}/{subCollection}?key={apiKey}";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(dataJson);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(idToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"<color=green>[FirestoreService] Successfully logged to {subCollection} for user {userId}</color>");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogWarning($"[FirestoreService] {subCollection} log note: {request.downloadHandler.text}");
                    onSuccess?.Invoke(); // Non-blocking
                }
            }
        }

        private IEnumerator UpdateComponentRoutine(string userId, string idToken, string fieldName, string componentJson, Action onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(userId)) yield break;

            if (string.IsNullOrEmpty(idToken))
            {
                idToken = FirebaseAuthService.Instance.CurrentIdToken;
                if (string.IsNullOrEmpty(idToken) && PlayerPrefs.HasKey("LostTemple_Auth_Token"))
                {
                    idToken = PlayerPrefs.GetString("LostTemple_Auth_Token");
                }
            }

            string url = $"{FirestoreBaseUrl}{userId}?updateMask.fieldPaths={fieldName}&key={apiKey}";
            string jsonBody = $@"{{
  ""fields"": {{
    ""{fieldName}"": {{ ""mapValue"": {componentJson} }}
  }}
}}";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(idToken)) request.SetRequestHeader("Authorization", $"Bearer {idToken}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[FirestoreService] Updated {fieldName} in Firestore!");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogWarning($"[FirestoreService] {fieldName} update note: {request.downloadHandler.text}");
                    onSuccess?.Invoke();
                }
            }
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }
}
