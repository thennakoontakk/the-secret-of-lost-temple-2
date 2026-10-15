using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SecretOfLostTemple.Research
{
    [Serializable] public class ResearchEvent
    {
        public string id, timestamp, scene, kind, context;
        public bool hasAnswer, correct;
        public float seconds;
        public string condition, stage, taskId, attemptId, contentVersion;
        public int attemptNumber, selectedAnswer = -1, difficulty;
    }
    [Serializable] public class ResearchSession
    {
        public string id, participant, started, ended, condition = "unassigned";
        public string protocol, appVersion, contentVersion, learningStarted, learningCompleted;
        public bool adaptive = true, embeddedAssessment = true;
        public string dataLabel, demoPreset;
        public List<FormalAssessment> assessments = new List<FormalAssessment>();
        public List<ResearchEvent> events = new List<ResearchEvent>();
    }
    [Serializable] public class SavedValue { public string key, value; }
    [Serializable] public class Participant
    {
        public string code;
        public List<SavedValue> progress = new List<SavedValue>();
    }
    [Serializable] public class ResearchDatabase
    {
        public int version = 1;
        public List<Participant> participants = new List<Participant>();
        public List<ResearchSession> sessions = new List<ResearchSession>();
    }

    // Every accepted mutation is committed before returning. The previous valid snapshot
    // remains available if a write is interrupted. Unknown/corrupt data is never overwritten.
    public sealed partial class ResearchStore
    {
        readonly string path;
        public bool IsPresentationStore { get; private set; }
        ResearchDatabase data;
        public string ParticipantCode { get; private set; }
        public string ActiveId { get; private set; }
        public string RecoveryNotice { get; private set; }
        public ResearchSession Current => data.sessions.FirstOrDefault(s => s.id == ActiveId);
        public ResearchSession Latest => data.sessions.LastOrDefault(s => s.participant == ParticipantCode);
        public ResearchStore(string path, bool presentationStore = false)
        {
            this.path = path;
            IsPresentationStore = presentationStore;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            foreach (string candidate in new[] { path, path + ".tmp", path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try
                {
                    var json = File.ReadAllText(candidate);
                    if (!json.Contains("\"version\"") || !json.Contains("\"sessions\"") || !json.Contains("\"participants\"")) throw new InvalidDataException();
                    data = JsonUtility.FromJson<ResearchDatabase>(json);
                    if (data == null || data.version != 1 || data.sessions == null || data.participants == null) throw new InvalidDataException();
                    foreach (var s in data.sessions) { if (s.assessments == null) s.assessments = new List<FormalAssessment>(); }
                    if (candidate != path)
                    {
                        RecoveryNotice = "Recovered local data from " + Path.GetFileName(candidate);
                        if (File.Exists(path)) File.Copy(path, path + ".corrupt-" + Guid.NewGuid().ToString("N"));
                        File.Copy(candidate, path, true);
                    }
                    return;
                }
                catch (Exception e) when (e is ArgumentException || e is InvalidDataException) { data = null; }
            }
            if (File.Exists(path) || File.Exists(path + ".bak") || File.Exists(path + ".tmp"))
                throw new InvalidDataException("Research storage is unreadable. Original files preserved; recording is blocked.");
            data = new ResearchDatabase();
        }
        void Mutate(Action change)
        {
            string before = JsonUtility.ToJson(data);
            try
            {
                change();
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
                using (var stream = new FileStream(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
                { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
                else File.Move(path + ".tmp", path);
            }
            catch { data = JsonUtility.FromJson<ResearchDatabase>(before); throw; }
        }
        public void Begin(string code, bool resume)
        {
            code = (code ?? "").Trim().ToUpperInvariant();
            if (!Regex.IsMatch(code, "^P[0-9]{3,8}$")) throw new ArgumentException("Use an anonymous code such as P001 (P plus 3–8 digits).");
            if (Current != null) throw new InvalidOperationException("End the current session before switching participants.");
            var unfinished = data.sessions.LastOrDefault(s => s.participant == code && string.IsNullOrEmpty(s.ended));
            if (resume && unfinished == null) throw new InvalidOperationException("No unfinished session for this code. Choose New session.");
            if (!resume && unfinished != null) throw new InvalidOperationException("An unfinished session exists. Resume and end it before creating another.");
            string id = unfinished != null ? unfinished.id : Guid.NewGuid().ToString("N");
            Mutate(() => {
                if (!data.participants.Any(p => p.code == code)) data.participants.Add(new Participant { code = code });
                if (unfinished == null) data.sessions.Add(new ResearchSession { id = id, participant = code, started = DateTime.UtcNow.ToString("O") });
            });
            ParticipantCode = code; ActiveId = id;
            Record("session_open", resume ? "resumed" : "new");
        }
        public void End()
        {
            var session = Current;
            if (session == null) return;
            Record("visit_end");
            Mutate(() => { if (string.IsNullOrEmpty(session.ended)) session.ended = DateTime.UtcNow.ToString("O"); });
            ActiveId = null;
        }
        public void Practice() { End(); ParticipantCode = null; }
        public void Record(string kind, string context = "", bool hasAnswer = false, bool correct = false, float seconds = 0)
        {
            var session = Current;
            if (session == null) return;
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0) throw new ArgumentException("Invalid response time.");
            var entry = new ResearchEvent { id = Guid.NewGuid().ToString("N"), timestamp = DateTime.UtcNow.ToString("O"),
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, kind = kind, context = context,
                hasAnswer = hasAnswer, correct = correct, seconds = seconds, condition = session.condition,
                stage = Stage, contentVersion = session.contentVersion };
            Mutate(() => session.events.Add(entry));
        }
        public string Get(string key, string fallback)
        {
            return data.participants.FirstOrDefault(p => p.code == ParticipantCode)?.progress.FirstOrDefault(v => v.key == key)?.value ?? fallback;
        }
        public void Set(string key, string value)
        {
            var participant = data.participants.FirstOrDefault(p => p.code == ParticipantCode);
            if (participant == null || Current == null) return;
            Mutate(() => {
                var item = participant.progress.FirstOrDefault(v => v.key == key);
                if (item == null) participant.progress.Add(new SavedValue { key = key, value = value });
                else item.value = value;
            });
        }
        public ResearchDatabase Snapshot() => JsonUtility.FromJson<ResearchDatabase>(JsonUtility.ToJson(data));
        static string Csv(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        public string Export()
        {
            if (data.sessions.Count == 0) throw new InvalidOperationException("No saved research sessions to export.");
            string folder = Path.Combine(Path.GetDirectoryName(path), "exports", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            var sessions = new StringBuilder("data_label,participant,session,started_utc,ended_utc,condition,event_count,answer_count,correct_count,mean_response_seconds,pretest,posttest,retention,learning_gain,formal_status\r\n");
            var events = new StringBuilder("data_label,participant,session,event_id,timestamp_utc,scene,kind,context,correct,seconds,condition,stage,content_version\r\n");
            foreach (var s in data.sessions)
            {
                var answers = s.events.Where(e => e.hasAnswer && e.kind != "formal_answer").ToList();
                sessions.AppendLine(string.Join(",", Csv(s.dataLabel), Csv(s.participant), Csv(s.id), Csv(s.started), Csv(s.ended), Csv(s.condition), s.events.Count, answers.Count,
                    answers.Count == 0 ? "" : answers.Count(e => e.correct).ToString(CultureInfo.InvariantCulture),
                    answers.Count == 0 ? "" : answers.Average(e => e.seconds).ToString("R", CultureInfo.InvariantCulture), Percent(s, "pretest"), Percent(s, "posttest"), Percent(s, "retention"), Gain(s), AssessmentStatus(s)));
                foreach (var e in s.events) events.AppendLine(string.Join(",", Csv(s.dataLabel), Csv(s.participant), Csv(s.id), Csv(e.id), Csv(e.timestamp), Csv(e.scene), Csv(e.kind), Csv(e.context),
                    e.hasAnswer ? (e.correct ? "1" : "0") : "", e.seconds.ToString("R", CultureInfo.InvariantCulture), Csv(e.condition), Csv(e.stage), Csv(e.contentVersion)));
            }
            File.WriteAllText(Path.Combine(folder, "sessions.csv"), sessions.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder, "events.csv"), events.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder, "records.json"), JsonUtility.ToJson(data, true), Encoding.UTF8);
            ExportStudyTables(folder);
            File.WriteAllText(Path.Combine(folder, "DATASET_LABEL.txt"), IsPresentationStore
                ? "[SYNTHETIC DEMO DATA] Presentation examples only. Exclude all records in this export from participant research."
                : "Participant research dataset. Presentation presets are stored and exported separately.");
            return folder;
        }
    }
    public static class Research
    {
        static ResearchStore store;
        static ResearchStore demoStore;
        public static bool DemoMode { get; private set; }
        public static string Error { get; private set; }
        public static ResearchStore Store => DemoMode
            ? demoStore ?? (demoStore = new ResearchStore(Path.Combine(Application.persistentDataPath, "presentation_demo_v1", "records.json"), true))
            : store ?? (store = new ResearchStore(Path.Combine(Application.persistentDataPath, "research_v1", "records.json")));
        public static void UsePresentationData(bool enabled) { DemoMode = enabled; Error = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset() { store = null; demoStore = null; DemoMode = false; Error = null; }
        public static void Record(string kind, string context = "", bool hasAnswer = false, bool correct = false, float seconds = 0)
        {
            try { Store.Record(kind, context, hasAnswer, correct, seconds); }
            catch (Exception e) { Error = e.Message; Debug.LogError("Research data was NOT saved: " + e.Message); }
        }
        public static string Summary()
        {
            try {
                var s = Store.Current ?? Store.Latest;
                if (s == null) return "Practice mode — no research session selected.\nOpen Research setup on the map to start or resume.";
                var answers = s.events.Where(e => e.hasAnswer && e.kind != "formal_answer").ToList();
                return "Participant: " + s.participant + "\nSession: " + s.id + "\n" + (Store.Current == null ? "Ended" : "Recording locally") +
                    " | Local storage\nCondition: " + s.condition + " | Stage: " + Store.Stage + "\nSaved events: " + s.events.Count + "\nFormative attempts: " + answers.Count +
                    (answers.Count == 0 ? "\nAccuracy: not collected" : "\nFormative accuracy: " + (100f * answers.Count(e => e.correct) / answers.Count).ToString("F1") + "%") +
                    "\n" + (Error ?? Store.RecoveryNotice ?? "");
            } catch (Exception e) { return "Recording unavailable: " + e.Message; }
        }
        public static string FormalStatus => Store.FormalSummary();
    }
    // Existing unscoped PlayerPrefs remain untouched. Practice uses a separate namespace.
    public static class ResearchProgress
    {
        public static string GetString(string key, string fallback = "") => Research.Store.ParticipantCode == null ? PlayerPrefs.GetString("Practice_v1_" + key, fallback) : Research.Store.Get(key, fallback);
        public static int GetInt(string key, int fallback = 0) => int.TryParse(GetString(key), out int n) ? n : fallback;
        public static bool HasKey(string key) => GetString(key, null) != null;
        public static void SetString(string key, string value) { if (Research.Store.ParticipantCode == null) { PlayerPrefs.SetString("Practice_v1_" + key, value); PlayerPrefs.Save(); } else Research.Store.Set(key, value); }
        public static void SetInt(string key, int value) => SetString(key, value.ToString(CultureInfo.InvariantCulture));
    }
}
