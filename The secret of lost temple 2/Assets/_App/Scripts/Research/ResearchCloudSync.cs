using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SecretOfLostTemple.Research;
using Data = SecretOfLostTemple.Research.Research;

namespace SecretOfLostTemple.Research
{
    /// <summary>
    /// Provides real-time 2-way synchronization between the Unity AR Phone App and the Laptop Web Dashboard:
    /// 1) Streams live session metrics (RQ1 Kalish, RQ2 Abeywickrama, RQ3 Pathirana) and device status to Firestore.
    /// 2) Polls remote researcher commands from the Web Dashboard so researchers can set up participants,
    ///    assign conditions, launch Pre-Test / AR Learning / Post-Test / Review / Retention, or trigger Demo Presets
    ///    remotely from their laptop while the student holds the phone.
    /// </summary>
    public class ResearchCloudSync : MonoBehaviour
    {
        public static ResearchCloudSync Instance { get; private set; }

        private const string ProjectId = "the-secret-of-lost-temple";
        private static string BaseDocsUrl => $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";

        private string lastExecutedCommandId = "";
        private string lastStatusMessage = "Ready";
        private float nextPollTime = 0f;
        private float nextPushTime = 0f;
        private bool isPolling = false;
        private bool isPushing = false;
        private string lastSyncedSessionSignature = "";

        private sealed class BypassCertificateHandler : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData) => true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            Debug.developerConsoleVisible = false;
            if (Instance == null)
            {
                var go = new GameObject("ResearchCloudSync");
                Instance = go.AddComponent<ResearchCloudSync>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            Debug.developerConsoleVisible = false;
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            lastExecutedCommandId = PlayerPrefs.GetString("ResearchCloudSync_LastCmdId", "");
        }

        private void Update()
        {
            if (Debug.developerConsoleVisible) Debug.developerConsoleVisible = false;

            if (Time.unscaledTime >= nextPollTime && !isPolling)
            {
                nextPollTime = Time.unscaledTime + 2.0f;
                StartCoroutine(PollRemoteCommandRoutine());
            }

            if (Time.unscaledTime >= nextPushTime && !isPushing)
            {
                nextPushTime = Time.unscaledTime + 3.0f;
                StartCoroutine(PushLiveTelemetryRoutine(false));
            }
        }

        private bool pendingForceSessionPush = false;

        public void TriggerImmediateSync(string statusMsg = null)
        {
            if (!string.IsNullOrEmpty(statusMsg))
            {
                lastStatusMessage = statusMsg;
            }
            pendingForceSessionPush = true;
            if (!isPushing && gameObject.activeInHierarchy)
            {
                StartCoroutine(PushLiveTelemetryRoutine(true));
            }
        }

        private IEnumerator SyncThenQuitRoutine()
        {
            yield return PushAllSessionsRoutine();
            yield return PushLiveTelemetryRoutine(true);
            yield return new WaitForSecondsRealtime(0.4f);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SyncAllLocalSessionsToCloud()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(PushAllSessionsRoutine());
            }
        }

        private IEnumerator PollRemoteCommandRoutine()
        {
            isPolling = true;
            string url = $"{BaseDocsUrl}/research_commands/remote_control";
            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.certificateHandler = new BypassCertificateHandler();
                req.disposeCertificateHandlerOnDispose = true;
                req.timeout = 5;
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    string json = req.downloadHandler.text;
                    string cmdId = ExtractFirestoreString(json, "commandId");
                    if (!string.IsNullOrEmpty(cmdId) && cmdId != lastExecutedCommandId)
                    {
                        lastExecutedCommandId = cmdId;
                        PlayerPrefs.SetString("ResearchCloudSync_LastCmdId", cmdId);
                        PlayerPrefs.Save();

                        string action = ExtractFirestoreString(json, "action");
                        string participant = ExtractFirestoreString(json, "participant");
                        string protocol = ExtractFirestoreString(json, "protocol");
                        string condition = ExtractFirestoreString(json, "condition");
                        bool adaptive = ExtractFirestoreString(json, "adaptive") == "true";
                        bool embedded = ExtractFirestoreString(json, "embedded") == "true";
                        string stage = ExtractFirestoreString(json, "stage");
                        string preset = ExtractFirestoreString(json, "preset");
                        string scoreStr = ExtractFirestoreString(json, "score");
                        string maxStr = ExtractFirestoreString(json, "maxScore");
                        string source = ExtractFirestoreString(json, "source");
                        string reason = ExtractFirestoreString(json, "reason");

                        ExecuteRemoteCommand(action, participant, protocol, condition, adaptive, embedded, stage, preset, scoreStr, maxStr, source, reason);
                    }
                }
            }
            isPolling = false;
        }

        private void ExecuteRemoteCommand(
            string action, string participant, string protocol, string condition,
            bool adaptive, bool embedded, string stage, string preset,
            string scoreStr, string maxStr, string source, string reason)
        {
            try
            {
                var ui = ResearchSetupUI.Instance ?? FindFirstObjectByType<ResearchSetupUI>();
                if (string.IsNullOrEmpty(protocol)) protocol = "lever-pilot-1";
                if (string.IsNullOrEmpty(condition)) condition = "interactive";

                switch (action)
                {
                    case "begin_study":
                        if (ui != null) ui.ExecuteRemoteBeginStudy(participant, protocol, condition, adaptive, embedded, stage == "auto_pretest");
                        else Data.Store.BeginStudy(participant, protocol, condition, adaptive, embedded);
                        lastStatusMessage = $"Started study for {participant} ({condition}, adaptive={adaptive}, embedded={embedded})";
                        break;

                    case "resume_study":
                        if (ui != null) ui.ExecuteRemoteResume(participant);
                        else Data.Store.Begin(participant, true);
                        lastStatusMessage = $"Resumed study for {participant}";
                        break;

                    case "open_followup":
                        if (ui != null) ui.ExecuteRemoteFollowUp(participant);
                        else Data.Store.OpenFollowUp(participant);
                        lastStatusMessage = $"Opened follow-up visit for {participant}";
                        break;

                    case "start_test":
                        if (ui != null) ui.ExecuteRemoteStartTest(stage);
                        else Data.Store.StartTest(stage);
                        lastStatusMessage = $"Opened in-app {stage} on phone";
                        break;

                    case "start_learning":
                        if (ui != null) ui.ExecuteRemoteStartLearning();
                        else { Data.Store.StartLearning(); SceneManager.LoadScene("MainARScene"); }
                        lastStatusMessage = "Started AR Lever Learning on phone";
                        break;

                    case "complete_learning":
                        if (ui != null) ui.ExecuteRemoteCompleteLearning(stage == "auto_posttest");
                        else Data.Store.CompleteLearning();
                        lastStatusMessage = "Completed AR Learning stage";
                        break;

                    case "open_review":
                        if (ui != null) ui.ExecuteRemoteOpenReview();
                        lastStatusMessage = "Opened AR Review Mode on phone";
                        break;

                    case "enter_external_score":
                        int.TryParse(scoreStr, out int raw);
                        if (!int.TryParse(maxStr, out int max) || max <= 0) max = 6;
                        Data.Store.EnterExternalScore(stage, string.IsNullOrEmpty(protocol) ? "paper-v1" : protocol, string.IsNullOrEmpty(source) ? "remote-dashboard" : source, raw, max, true, reason ?? "");
                        if (ui != null) ui.RefreshOverlayIfOpen();
                        lastStatusMessage = $"Recorded external {stage} score: {raw}/{max}";
                        break;

                    case "load_demo":
                        if (ui != null) ui.ExecuteRemoteLoadDemo(preset, stage != "no_jump");
                        else { Data.UsePresentationData(true); Data.Store.LoadPresentationPreset(preset); }
                        lastStatusMessage = $"Loaded demo preset: {preset}";
                        break;

                    case "return_participant_data":
                        Data.UsePresentationData(false);
                        if (ui != null) ui.ExecuteRemoteCloseOverlay();
                        lastStatusMessage = "Switched to real participant store";
                        break;

                    case "end_visit":
                        StudyRuntime.FlushActive();
                        Data.Store.End();
                        if (ui != null) ui.ExecuteRemoteCloseOverlay();
                        if (stage == "return_home" && SceneManager.GetActiveScene().name != "HomeMapScene")
                        {
                            SceneManager.LoadScene("HomeMapScene");
                        }
                        lastStatusMessage = "Saved & Ended current student visit";
                        break;

                    case "end_and_close_app":
                        StudyRuntime.FlushActive();
                        Data.Store.End();
                        if (ui != null) ui.ExecuteRemoteCloseOverlay();
                        lastStatusMessage = "Saved session & closing app for next student";
                        StartCoroutine(SyncThenQuitRoutine());
                        break;

                    case "sync_all":
                        SyncAllLocalSessionsToCloud();
                        lastStatusMessage = "Synced all local sessions to cloud dashboard";
                        break;

                    case "close_overlay":
                        if (ui != null) ui.ExecuteRemoteCloseOverlay();
                        lastStatusMessage = "Closed researcher overlay on phone";
                        break;
                }
            }
            catch (Exception ex)
            {
                lastStatusMessage = "Command error: " + ex.Message;
                Debug.LogWarning("[ResearchCloudSync] Remote command failed: " + ex.Message);
            }

            TriggerImmediateSync();
        }

        private IEnumerator PushLiveTelemetryRoutine(bool forceSessionPush)
        {
            isPushing = true;

            var s = Data.Store.Current ?? Data.Store.Latest;
            var m = s != null ? ResearchStore.ComputeMetrics(s) : null;
            string metricsJson = m != null ? SanitizeJsonNumbers(JsonUtility.ToJson(m)) : "{}";
            string sceneName = SceneManager.GetActiveScene().name;
            string pendingStage = Data.Store.PendingTest != null ? Data.Store.PendingTest.stage : "";
            int pendingIdx = Data.Store.PendingTest != null ? Data.Store.PendingTest.answers.Count : -1;

            // 1. Push active phone heartbeat to research_devices/active_phone
            string deviceDocJson = BuildDeviceFirestoreJson(s, m, sceneName, pendingStage, pendingIdx, metricsJson);
            using (UnityWebRequest devReq = new UnityWebRequest($"{BaseDocsUrl}/research_devices/active_phone", "PATCH"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(deviceDocJson);
                devReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
                devReq.downloadHandler = new DownloadHandlerBuffer();
                devReq.certificateHandler = new BypassCertificateHandler();
                devReq.disposeCertificateHandlerOnDispose = true;
                devReq.SetRequestHeader("Content-Type", "application/json");
                devReq.timeout = 5;
                yield return devReq.SendWebRequest();
            }

            // 2. Push active session to research_sessions/{id} if modified or forced
            if (s != null && !string.IsNullOrEmpty(s.id))
            {
                int totalAnswers = 0;
                if (s.assessments != null)
                {
                    for (int i = 0; i < s.assessments.Count; i++)
                        if (s.assessments[i]?.answers != null) totalAnswers += s.assessments[i].answers.Count;
                }
                string sig = $"{s.id}:{s.events.Count}:{s.assessments.Count}:{totalAnswers}:{s.learningStarted}:{s.learningCompleted}:{s.ended}:{Data.DemoMode}";
                if (forceSessionPush || pendingForceSessionPush || sig != lastSyncedSessionSignature)
                {
                    pendingForceSessionPush = false;
                    lastSyncedSessionSignature = sig;
                    string sessionDocJson = BuildSessionFirestoreJson(s, m, metricsJson);
                    string safeDocId = SanitizeDocId(s.id);
                    using (UnityWebRequest sesReq = new UnityWebRequest($"{BaseDocsUrl}/research_sessions/{safeDocId}", "PATCH"))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(sessionDocJson);
                        sesReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
                        sesReq.downloadHandler = new DownloadHandlerBuffer();
                        sesReq.certificateHandler = new BypassCertificateHandler();
                        sesReq.disposeCertificateHandlerOnDispose = true;
                        sesReq.SetRequestHeader("Content-Type", "application/json");
                        sesReq.timeout = 5;
                        yield return sesReq.SendWebRequest();
                    }
                }
            }

            isPushing = false;
            if (pendingForceSessionPush && gameObject.activeInHierarchy)
            {
                StartCoroutine(PushLiveTelemetryRoutine(true));
            }
        }

        private IEnumerator PushAllSessionsRoutine()
        {
            var all = Data.Store.AllSessions;
            if (all == null) yield break;
            for (int i = 0; i < all.Count; i++)
            {
                var s = all[i];
                if (s == null || string.IsNullOrEmpty(s.id)) continue;
                var m = ResearchStore.ComputeMetrics(s);
                string metricsJson = SanitizeJsonNumbers(JsonUtility.ToJson(m));
                string sessionDocJson = BuildSessionFirestoreJson(s, m, metricsJson);
                string safeDocId = SanitizeDocId(s.id);
                using (UnityWebRequest sesReq = new UnityWebRequest($"{BaseDocsUrl}/research_sessions/{safeDocId}", "PATCH"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(sessionDocJson);
                    sesReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    sesReq.downloadHandler = new DownloadHandlerBuffer();
                    sesReq.certificateHandler = new BypassCertificateHandler();
                    sesReq.disposeCertificateHandlerOnDispose = true;
                    sesReq.SetRequestHeader("Content-Type", "application/json");
                    sesReq.timeout = 5;
                    yield return sesReq.SendWebRequest();
                }
            }
        }

        private static string SanitizeJsonNumbers(string json)
        {
            if (string.IsNullOrEmpty(json)) return "{}";
            return json
                .Replace(":NaN", ":null")
                .Replace(":-Infinity", ":null")
                .Replace(":Infinity", ":null");
        }

        private string BuildDeviceFirestoreJson(ResearchSession s, SessionComputedMetrics m, string sceneName, string pendingStage, int pendingIdx, string metricsJson)
        {
            var sb = new StringBuilder();
            sb.Append("{\"fields\":{");
            AppendField(sb, "deviceId", SystemInfo.deviceUniqueIdentifier ?? "unity_device", true);
            AppendField(sb, "deviceModel", SystemInfo.deviceModel ?? Application.platform.ToString(), true);
            AppendField(sb, "scene", sceneName, true);
            AppendField(sb, "stage", Data.Store.Stage, true);
            AppendField(sb, "participant", s != null ? s.participant : "", true);
            AppendField(sb, "sessionId", s != null ? s.id : "", true);
            AppendField(sb, "condition", s != null ? s.condition : "passive", true);
            AppendField(sb, "adaptive", s != null && s.adaptive ? "true" : "false", true);
            AppendField(sb, "embedded", s != null && s.embeddedAssessment ? "true" : "false", true);
            AppendField(sb, "demoMode", Data.DemoMode ? "true" : "false", true);
            AppendField(sb, "pendingTestStage", pendingStage, true);
            AppendField(sb, "pendingQuestionIndex", pendingIdx.ToString(CultureInfo.InvariantCulture), true);
            AppendField(sb, "lastCommandId", lastExecutedCommandId, true);
            AppendField(sb, "statusMessage", lastStatusMessage, true);
            AppendField(sb, "lastSeenUtc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
            AppendField(sb, "metricsJson", metricsJson, false);
            sb.Append("}}");
            return sb.ToString();
        }

        private string BuildSessionFirestoreJson(ResearchSession s, SessionComputedMetrics m, string metricsJson)
        {
            var sb = new StringBuilder();
            sb.Append("{\"fields\":{");
            AppendField(sb, "sessionId", s.id ?? "", true);
            AppendField(sb, "participant", s.participant ?? "", true);
            AppendField(sb, "protocol", s.protocol ?? "", true);
            AppendField(sb, "condition", s.condition ?? "", true);
            AppendField(sb, "adaptive", s.adaptive ? "true" : "false", true);
            AppendField(sb, "embeddedAssessment", s.embeddedAssessment ? "true" : "false", true);
            AppendField(sb, "isSyntheticDemo", s.dataLabel == "synthetic_demo" ? "true" : "false", true);
            AppendField(sb, "demoPreset", s.dataLabel ?? "", true);
            AppendField(sb, "started", s.started ?? "", true);
            AppendField(sb, "learningStarted", s.learningStarted ?? "", true);
            AppendField(sb, "learningCompleted", s.learningCompleted ?? "", true);
            AppendField(sb, "ended", s.ended ?? "", true);
            AppendField(sb, "sessionJson", JsonUtility.ToJson(s), true);
            AppendField(sb, "updatedUtc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), true);
            AppendField(sb, "metricsJson", metricsJson, false);
            sb.Append("}}");
            return sb.ToString();
        }

        private static void AppendField(StringBuilder sb, string key, string val, bool comma)
        {
            sb.Append("\"").Append(key).Append("\":{\"stringValue\":\"").Append(EscapeJson(val ?? "")).Append("\"}");
            if (comma) sb.Append(",");
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private static string SanitizeDocId(string id)
        {
            if (string.IsNullOrEmpty(id)) return "unknown_session";
            return id.Replace("/", "_").Replace(" ", "_");
        }

        private static string ExtractFirestoreString(string json, string fieldName)
        {
            if (string.IsNullOrEmpty(json)) return "";
            string keyPattern = $"\"{fieldName}\"";
            int keyIdx = json.IndexOf(keyPattern, StringComparison.Ordinal);
            if (keyIdx < 0) return "";
            int strValIdx = json.IndexOf("\"stringValue\"", keyIdx, StringComparison.Ordinal);
            if (strValIdx < 0 || strValIdx > keyIdx + 200) return "";
            int colonIdx = json.IndexOf(':', strValIdx);
            if (colonIdx < 0) return "";
            int firstQuote = json.IndexOf('"', colonIdx + 1);
            if (firstQuote < 0) return "";
            var sb = new StringBuilder();
            for (int i = firstQuote + 1; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '\\' && i + 1 < json.Length)
                {
                    char next = json[i + 1];
                    if (next == '"' || next == '\\' || next == '/') { sb.Append(next); i++; continue; }
                    if (next == 'n') { sb.Append('\n'); i++; continue; }
                    if (next == 'r') { sb.Append('\r'); i++; continue; }
                }
                else if (c == '"')
                {
                    break;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
