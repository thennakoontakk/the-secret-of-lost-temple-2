using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SecretOfLostTemple.Research
{
    [Serializable] public class FormalAnswer
    {
        public string questionId, timestamp;
        public int selected, correctIndex;
        public float seconds;
        public bool timingRestarted;
    }
    [Serializable] public class FormalAssessment
    {
        public string id, stage, version, source, started, completed, replaces, correctionReason, recordedAt;
        public int score, denominator;
        public bool administratorConfirmed;
        public List<FormalAnswer> answers = new List<FormalAnswer>();
    }
    public sealed partial class ResearchStore
    {
        public Func<DateTime> UtcNow = () => DateTime.UtcNow;
        public string Stage => Current == null ? "none" : PendingTest != null ? PendingTest.stage :
            string.IsNullOrEmpty(Current.learningStarted) ? "baseline" : string.IsNullOrEmpty(Current.learningCompleted) ? "learning" : "followup";
        public FormalAssessment PendingTest => Current?.assessments.LastOrDefault(a => string.IsNullOrEmpty(a.completed));
        public bool IsStudy => Current != null && !string.IsNullOrEmpty(Current.protocol);
        public bool CanManipulate => !IsStudy || (Current.condition == "interactive" && PendingTest == null);
        public void BeginStudy(string code, string protocol, string condition, bool adaptive, bool embedded)
        {
            if (string.IsNullOrWhiteSpace(protocol)) throw new ArgumentException("Enter the approved protocol/version or an explicit pilot version.");
            if (condition != "passive" && condition != "interactive") throw new ArgumentException("Choose passive or interactive.");
            Begin(code, false);
            Mutate(() => { Current.protocol = protocol.Trim(); Current.condition = condition; Current.adaptive = adaptive;
                Current.embeddedAssessment = embedded; Current.appVersion = Application.version; Current.contentVersion = LeverTestBank.Version; });
            Record("study_assignment", "researcher_selected;adaptive=" + adaptive + ";embedded=" + embedded);
        }
        public void OpenFollowUp(string code)
        {
            if (Current != null) throw new InvalidOperationException("End the current visit first.");
            var s = data.sessions.LastOrDefault(x => x.participant == (code ?? "").Trim().ToUpperInvariant() && !string.IsNullOrEmpty(x.learningCompleted));
            if (s == null) throw new InvalidOperationException("No completed learning session for this participant.");
            ParticipantCode = s.participant; ActiveId = s.id; Record("followup_open");
        }
        void RequireStudy() { if (!IsStudy) throw new InvalidOperationException("Start or resume a configured study session first."); }
        public void StartLearning(bool remoteForce = false)
        {
            RequireStudy();
            if (!remoteForce)
            {
                if (PendingTest != null || LatestTest(Current, "pretest") == null) throw new InvalidOperationException("Complete the baseline test first.");
                if (!string.IsNullOrEmpty(Current.learningCompleted)) throw new InvalidOperationException("Learning already completed. Use Review.");
            }
            else
            {
                // Close any half-finished pending test if researcher forces jump to AR Learning
                if (PendingTest != null)
                {
                    var pt = PendingTest;
                    Mutate(() => { pt.score = pt.answers.Count(x => x.selected == x.correctIndex); pt.completed = UtcNow().ToString("O"); pt.recordedAt = pt.completed; });
                }
                if (!string.IsNullOrEmpty(Current.learningCompleted))
                {
                    Mutate(() => Current.learningCompleted = "");
                }
            }
            if (string.IsNullOrEmpty(Current.learningStarted)) Mutate(() => Current.learningStarted = UtcNow().ToString("O"));
            Record("learning_open");
        }
        public void CompleteLearning(bool remoteFallback = false)
        {
            RequireStudy();
            if (PendingTest != null || string.IsNullOrEmpty(Current.learningStarted)) throw new InvalidOperationException("Start and finish the learning activity first.");
            if (!string.IsNullOrEmpty(Current.learningCompleted)) return;
            if (!Current.events.Any(e => e.kind == "learning_exposure" && e.seconds > 0))
            {
                float elapsed = 0;
                if (DateTime.TryParse(Current.learningStarted, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var started))
                    elapsed = (float)(UtcNow() - started.ToUniversalTime()).TotalSeconds;
                if (!remoteFallback && elapsed <= .5f)
                    throw new InvalidOperationException("No tracked learning exposure recorded yet.");
                // Wall-clock recovery is an estimate, not evidence of continuous marker tracking.
                Record("learning_exposure", "active_session_completion;estimated_wall_clock", false, false,
                    elapsed > .5f ? Mathf.Clamp(elapsed, 1f, 600f) : 5f);
            }
            Mutate(() => Current.learningCompleted = UtcNow().ToString("O"));
            Record("learning_complete");
        }
        public double DaysSinceLearning(ResearchSession session) => string.IsNullOrEmpty(session?.learningCompleted) ? double.NaN :
            (UtcNow() - DateTime.Parse(session.learningCompleted, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)).TotalDays;
        void ValidateStage(string stage, bool correction)
        {
            RequireStudy();
            if (stage != "pretest" && stage != "posttest" && stage != "retention") throw new ArgumentException("Unknown assessment stage.");
            if (!correction && LatestTest(Current, stage) != null) throw new InvalidOperationException("This stage already has a result. Use a documented correction instead.");
            if (stage == "pretest" && !string.IsNullOrEmpty(Current.learningStarted) && !correction) throw new InvalidOperationException("Baseline must precede learning.");
            if (stage != "pretest" && string.IsNullOrEmpty(Current.learningCompleted)) throw new InvalidOperationException("Complete learning first.");
            if (stage == "retention" && (LatestTest(Current, "posttest") == null || DaysSinceLearning(Current) < 7))
                throw new InvalidOperationException("Retention requires a post-test and at least seven days since learning completion.");
        }
        public FormalAssessment StartTest(string stage, bool remoteForce = false)
        {
            RequireStudy();
            if (PendingTest != null)
            {
                if (PendingTest.stage == stage) return PendingTest;
                if (!remoteForce) throw new InvalidOperationException("Finish the open test first.");
                var pt = PendingTest;
                Mutate(() => { pt.score = pt.answers.Count(x => x.selected == x.correctIndex); pt.completed = UtcNow().ToString("O"); pt.recordedAt = pt.completed; });
            }
            if (!remoteForce)
            {
                ValidateStage(stage, false);
            }
            else if (stage == "posttest" && string.IsNullOrEmpty(Current.learningCompleted))
            {
                if (string.IsNullOrEmpty(Current.learningStarted)) Mutate(() => Current.learningStarted = UtcNow().ToString("O"));
                CompleteLearning(true);
            }
            var a = new FormalAssessment { id = Guid.NewGuid().ToString("N"), stage = stage, version = LeverTestBank.Version,
                source = "in_app", started = UtcNow().ToString("O"), denominator = LeverTestBank.Questions.Length };
            Mutate(() => Current.assessments.Add(a)); Record("formal_start", a.id + ":" + stage); return a;
        }
        public void AnswerFormal(int selected, float seconds)
        {
            var a = PendingTest ?? throw new InvalidOperationException("No open test.");
            if (a.version != LeverTestBank.Version) throw new InvalidOperationException("Test content version changed; do not mix versions.");
            int index = a.answers.Count;
            if (index >= LeverTestBank.Questions.Length) throw new InvalidOperationException("Test already answered.");
            var q = LeverTestBank.Questions[index];
            if (selected < 0 || selected >= q.options.Length || seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) throw new ArgumentException("Invalid answer.");
            Mutate(() => {
                a.answers.Add(new FormalAnswer { questionId = q.id, selected = selected, correctIndex = q.correct, seconds = seconds, timestamp = UtcNow().ToString("O"),
                    timingRestarted = Current.events.Count(e => e.kind == "formal_question_presented" && e.context == a.id + ":" + index) > 1 });
                if (a.answers.Count == a.denominator) { a.score = a.answers.Count(x => x.selected == x.correctIndex); a.completed = UtcNow().ToString("O"); a.recordedAt = a.completed; }
            });
        }
        public void EnterExternalScore(string stage, string version, string source, int score, int denominator, bool confirmed, string correctionReason = "")
        {
            if (PendingTest != null) throw new InvalidOperationException("An in-app test is in progress.");
            var previous = Current == null ? null : LatestTest(Current, stage);
            ValidateStage(stage, previous != null);
            if (!confirmed || string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Supply test version, source and administrator confirmation.");
            if (denominator <= 0 || score < 0 || score > denominator) throw new ArgumentException("Score must be between zero and a positive denominator.");
            if (previous != null && string.IsNullOrWhiteSpace(correctionReason)) throw new ArgumentException("A correction reason is required. The original result will be retained.");
            var a = new FormalAssessment { id = Guid.NewGuid().ToString("N"), stage = stage, version = version, source = source,
                score = score, denominator = denominator, administratorConfirmed = true, started = previous?.started ?? UtcNow().ToString("O"),
                completed = previous?.completed ?? UtcNow().ToString("O"), recordedAt = UtcNow().ToString("O"),
                replaces = previous?.id, correctionReason = correctionReason };
            Mutate(() => Current.assessments.Add(a));
        }
        public static FormalAssessment LatestTest(ResearchSession s, string stage) => s?.assessments?.LastOrDefault(a => a.stage == stage && !string.IsNullOrEmpty(a.completed));
        static string Percent(ResearchSession s, string stage) { var a = LatestTest(s, stage); return a == null ? "" : (100.0 * a.score / a.denominator).ToString("F4", CultureInfo.InvariantCulture); }
        static string Gain(ResearchSession s) { var pre = LatestTest(s, "pretest"); var post = LatestTest(s, "posttest"); return pre == null || post == null ? "" : (100.0 * post.score / post.denominator - 100.0 * pre.score / pre.denominator).ToString("F4", CultureInfo.InvariantCulture); }
        static string AssessmentStatus(ResearchSession s) => LatestTest(s, "retention") != null ? "complete" : LatestTest(s, "posttest") != null ? "retention_pending" : LatestTest(s, "pretest") != null ? "baseline_collected" : "not_collected";
        public string FormalSummary()
        {
            var s = Current ?? Latest;
            string summary = "";
            foreach (var stage in new[] { "pretest", "posttest", "retention" }) { var a = LatestTest(s, stage); summary += stage + ": " + (a == null ? "not collected" : a.score + "/" + a.denominator + " (" + Percent(s, stage) + "%)") + "\n"; }
            summary += "Change (percentage points): " + (string.IsNullOrEmpty(Gain(s)) ? "not available" : Gain(s));
            if (!string.IsNullOrEmpty(s?.learningCompleted)) summary += "\nRetention due UTC: " + DateTime.Parse(s.learningCompleted, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).AddDays(7).ToString("u") + "\nDays since learning: " + DaysSinceLearning(s).ToString("F2");
            return summary;
        }
        public void RecordAttempt(string task, string attemptId, int number, int selected, bool correct, float seconds, int difficulty, string context)
        {
            if (Current == null) return;
            if (Current.events.Any(e => e.attemptId == attemptId)) return;
            if (PendingTest != null) throw new InvalidOperationException("Learning attempts cannot be recorded during a formal test.");
            if (number < 1 || seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) throw new ArgumentException("Invalid attempt.");
            var e = new ResearchEvent { id = Guid.NewGuid().ToString("N"), timestamp = UtcNow().ToString("O"), scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                kind = "formative_answer", condition = Current.condition, stage = Stage, taskId = task, attemptId = attemptId, attemptNumber = number,
                selectedAnswer = selected, correct = correct, hasAnswer = true, seconds = seconds, difficulty = difficulty, contentVersion = Current.contentVersion, context = context };
            Mutate(() => Current.events.Add(e));
        }
        void ExportStudyTables(string folder)
        {
            var formal = new StringBuilder("data_label,participant,session,assessment,stage,version,source,started_utc,completed_utc,score,denominator,percent,replaces,correction_reason,confirmed,days_since_learning,recorded_at_utc\r\n");
            var answers = new StringBuilder("data_label,participant,session,assessment,question,selected,correct_index,response_seconds,timestamp_utc,timing_restarted\r\n");
            var attempts = new StringBuilder("data_label,participant,session,event,task,attempt_id,attempt_number,selected,correct,response_seconds,difficulty,condition,stage,content_version,context\r\n");
            var protocol = new StringBuilder("data_label,participant,session,protocol,app_version,content_version,condition,adaptive,embedded_assessment,learning_started,learning_completed\r\n");
            foreach (var s in data.sessions)
            {
                protocol.AppendLine(string.Join(",", Csv(s.dataLabel), Csv(s.participant), Csv(s.id), Csv(s.protocol), Csv(s.appVersion), Csv(s.contentVersion), Csv(s.condition), s.adaptive, s.embeddedAssessment, Csv(s.learningStarted), Csv(s.learningCompleted)));
                foreach (var a in s.assessments)
                {
                    string interval = string.IsNullOrEmpty(s.learningCompleted) || string.IsNullOrEmpty(a.completed) ? "" : (DateTime.Parse(a.completed, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) - DateTime.Parse(s.learningCompleted, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)).TotalDays.ToString("F6", CultureInfo.InvariantCulture);
                    formal.AppendLine(string.Join(",", Csv(s.dataLabel), Csv(s.participant), Csv(s.id), Csv(a.id), Csv(a.stage), Csv(a.version), Csv(a.source), Csv(a.started), Csv(a.completed), string.IsNullOrEmpty(a.completed) ? "" : a.score.ToString(), a.denominator, string.IsNullOrEmpty(a.completed) ? "" : (100.0*a.score/a.denominator).ToString("F4", CultureInfo.InvariantCulture), Csv(a.replaces), Csv(a.correctionReason), a.administratorConfirmed, interval, Csv(a.recordedAt)));
                    foreach (var answer in a.answers) answers.AppendLine(string.Join(",", Csv(s.dataLabel), Csv(s.participant), Csv(s.id), Csv(a.id), Csv(answer.questionId), answer.selected, answer.correctIndex, answer.seconds.ToString("R", CultureInfo.InvariantCulture), Csv(answer.timestamp), answer.timingRestarted));
                }
                foreach (var e in s.events.Where(x => x.hasAnswer)) attempts.AppendLine(string.Join(",", Csv(s.dataLabel), Csv(s.participant), Csv(s.id), Csv(e.id), Csv(e.taskId), Csv(e.attemptId), e.attemptNumber, e.selectedAnswer, e.correct ? 1 : 0, e.seconds.ToString("R", CultureInfo.InvariantCulture), e.difficulty, Csv(e.condition), Csv(e.stage), Csv(e.contentVersion), Csv(e.context)));
            }
            File.WriteAllText(Path.Combine(folder,"formal_tests.csv"), formal.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder,"formal_answers.csv"), answers.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder,"attempts.csv"), attempts.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder,"protocols.csv"), protocol.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder,"README.txt"), "Scores are percentages; learning_gain is post minus pre in percentage points. Blank means missing. Formal corrections append records: use the last completed record per stage, preserving replaces. Response times exclude application pauses. Timestamps use device UTC; verify device clocks. Content lever-pilot-v1 requires teacher/protocol review. Interaction counts are behaviors, not psychological scores. records.json contains the full event context. No child names or contacts belong in these files.");
        }
    }
    public sealed class LeverQuestion
    {
        public string id, text; public string[] options; public int correct;
        public LeverQuestion(string id, string text, int correct, params string[] options) { this.id=id; this.text=text; this.correct=correct; this.options=options; }
    }
    public static class LeverTestBank
    {
        public const string Version = "lever-pilot-v1";
        public static readonly LeverQuestion[] Questions = {
            new LeverQuestion("L1", "What is the turning point of a lever called?", 1, "Load", "Fulcrum", "Effort", "Handle"),
            new LeverQuestion("L2", "Which part is the object the lever lifts?", 2, "Fulcrum", "Effort", "Load", "Pivot"),
            new LeverQuestion("L3", "For the same load, moving the fulcrum nearer the load usually makes the effort needed...", 0, "Smaller", "Larger", "Always zero", "Unchanged"),
            new LeverQuestion("L4", "In a first-class lever, which part lies between effort and load?", 3, "Rope", "Wheel", "Weight", "Fulcrum"),
            new LeverQuestion("L5", "Ignoring friction: effort arm 2 m, load arm 1 m, load 60 N. What effort balances it?", 1, "120 N", "30 N", "60 N", "15 N"),
            new LeverQuestion("L6", "What happens when effort torque exceeds load torque in the lifting direction?", 2, "The load gets heavier", "The fulcrum disappears", "The load begins to rise", "The beam cannot turn")
        };
    }
}
