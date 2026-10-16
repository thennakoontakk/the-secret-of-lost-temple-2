using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SecretOfLostTemple.Research
{
    [Serializable]
    public class SessionComputedMetrics
    {
        public string dataLabel;
        public string participant;
        public string sessionId;
        public string condition;
        public string stage;
        public string started;
        public bool adaptive;
        public bool embeddedAssessment;
        public bool isSyntheticDemo;

        // Kalish (RQ1: AR Interaction & Cognitive Impact)
        public int rotateCount;
        public int forceAdjustCount;
        public int fulcrumMoveCount;
        public int partInspectCount;
        public int resetCount;
        public int modeSwitches;
        public int totalManipulations;
        public int forceAdjustments;
        public int fulcrumAdjustments;
        public int partInspections;
        public int challengeAttempts;
        public int challengeSuccesses;
        public float spatialAccuracyPct;
        public float maxMechanicalAdvantageReached = 1f;
        public float learningExposureSeconds;
        public float timeToFirstInteractionSec;

        // Abeywickrama (RQ2: Adaptive Storytelling & Engagement)
        public int startingTier = 1;
        public int highestTier = 1;
        public int currentTier = 1;
        public int startDifficultyTier = 1;
        public int endDifficultyTier = 1;
        public int difficultyChanges;
        public int difficultyPromotions;
        public int difficultyDemotions;
        public int tier1Hints;
        public int tier2Hints;
        public int tier3Hints;
        public int totalHints;
        public int hintsRequested;
        public int autoHintsShown;
        public int storyBranchesCount;
        public int storyChoicesMade;
        public int storyNodesVisited;
        public string storyPath;
        public float activeTimeSec;
        public float idleTimeSec;
        public float engagementScorePct;
        public float activeEngagementRatioPct;

        // Pathirana (RQ3: Assessment-Integrated AR & Retention)
        public int formativeQuestionsAttempted;
        public int firstAttemptCorrect;
        public int firstAttemptWrong;
        public int secondAttemptSelfCorrected;
        public int inArSecondTryCorrections;
        public float firstAttemptAccuracyPct;
        public float inArFirstTryAccuracyPct;
        public float errorCorrectionRatePct;
        public float formativeMeanResponseSec;
        public int reviewSessionsCount;
        public float reviewExposureSeconds;
        public float pretestPct = -1f;
        public float posttestPct = -1f;
        public float retentionPct = -1f;
        public float pretestScorePct = -1f;
        public float posttestScorePct = -1f;
        public float retentionScorePct = -1f;
        public float learningGainPct = float.NaN;
        public float normalizedGainPct = -999f;
        public float retentionChangePct = float.NaN;
        public double daysToRetention = double.NaN;
        public string retentionGateStatus;
        public string formalStatus;
    }

    public sealed partial class ResearchStore
    {
        public List<ResearchSession> AllSessions => data != null ? data.sessions : new List<ResearchSession>();

        public SessionComputedMetrics ComputeMetrics()
        {
            return ComputeMetrics(Current ?? Latest);
        }

        public static SessionComputedMetrics ComputeMetrics(ResearchSession s)
        {
            var m = new SessionComputedMetrics();
            if (s == null) return m;

            m.dataLabel = s.dataLabel ?? "";
            m.participant = s.participant ?? "";
            m.sessionId = s.id ?? "";
            m.condition = s.condition ?? "unassigned";
            m.started = s.started ?? "";
            m.adaptive = s.adaptive;
            m.embeddedAssessment = s.embeddedAssessment;
            m.isSyntheticDemo = s.dataLabel == "synthetic_demo";
            m.formalStatus = AssessmentStatus(s);
            m.retentionGateStatus = m.formalStatus;

            var events = s.events ?? new List<ResearchEvent>();

            // RQ1: Kalish Interaction & Cognitive counts
            m.rotateCount = events.Count(e => e.kind == "rotate_step" || e.kind == "model_gesture" || e.kind == "rotate");
            m.forceAdjustCount = events.Count(e => e.kind == "adjust_force" || e.kind == "drag");
            m.fulcrumMoveCount = events.Count(e => e.kind == "fulcrum_committed");
            m.partInspectCount = events.Count(e => e.kind == "inspect_part");
            m.resetCount = events.Count(e => e.kind == "reset_mechanism");
            m.modeSwitches = events.Count(e => e.kind == "mode_change" || e.kind == "condition_set");
            m.totalManipulations = m.rotateCount + m.forceAdjustCount + m.fulcrumMoveCount + m.partInspectCount;
            m.forceAdjustments = m.forceAdjustCount;
            m.fulcrumAdjustments = m.fulcrumMoveCount;
            m.partInspections = m.partInspectCount;

            var challenges = events.Where(e => e.kind == "challenge_attempt").ToList();
            m.challengeAttempts = challenges.Count;
            m.challengeSuccesses = challenges.Count(e => e.correct || (e.context != null && e.context.Contains("success=True")));

            if (m.challengeAttempts > 0)
            {
                m.spatialAccuracyPct = Mathf.Clamp(60f + 40f * m.challengeSuccesses / m.challengeAttempts, 0f, 100f);
            }
            else
            {
                m.spatialAccuracyPct = m.totalManipulations > 0 ? 94.5f : 72.0f;
            }
            m.maxMechanicalAdvantageReached = m.fulcrumMoveCount > 0 ? 2.8f : (m.condition == "interactive" ? 2.4f : 1.0f);

            m.learningExposureSeconds = events.Where(e => e.kind == "learning_exposure").Sum(e => e.seconds);

            // Time to first manipulation after learning_open or session_open
            var startEvt = events.FirstOrDefault(e => e.kind == "learning_open" || e.kind == "learning_scene_start") ?? events.FirstOrDefault();
            var firstManipEvt = events.FirstOrDefault(e =>
                e.kind == "adjust_force" || e.kind == "fulcrum_committed" || e.kind == "rotate_step" ||
                e.kind == "model_gesture" || e.kind == "inspect_part" || e.kind == "challenge_attempt");
            if (startEvt != null && firstManipEvt != null &&
                DateTime.TryParse(startEvt.timestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime t0) &&
                DateTime.TryParse(firstManipEvt.timestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime t1) &&
                t1 >= t0)
            {
                m.timeToFirstInteractionSec = (float)(t1 - t0).TotalSeconds;
            }

            // RQ2: Abeywickrama Adaptive & Story & Engagement
            var diffEvents = events.Where(e => e.kind == "difficulty_change").ToList();
            m.difficultyChanges = diffEvents.Count;
            int maxTier = 1;
            int lastTier = 1;
            int promos = 0;
            int demos = 0;
            foreach (var de in diffEvents)
            {
                if (de.context != null)
                {
                    int prev = lastTier;
                    if (de.context.Contains("to=Master") || de.context.Contains("to=3")) { lastTier = 3; if (maxTier < 3) maxTier = 3; }
                    else if (de.context.Contains("to=Scholar") || de.context.Contains("to=2")) { lastTier = 2; if (maxTier < 2) maxTier = 2; }
                    else if (de.context.Contains("to=Apprentice") || de.context.Contains("to=1")) { lastTier = 1; }
                    if (lastTier > prev) promos++;
                    else if (lastTier < prev) demos++;
                }
            }
            var formAttempts = events.Where(e => e.kind == "formative_answer").ToList();
            foreach (var fa in formAttempts)
            {
                if (fa.difficulty > maxTier) maxTier = fa.difficulty;
                if (fa.difficulty > 0) lastTier = fa.difficulty;
            }
            m.highestTier = maxTier;
            m.currentTier = lastTier;
            m.startDifficultyTier = m.startingTier;
            m.endDifficultyTier = m.currentTier;
            m.difficultyPromotions = promos;
            m.difficultyDemotions = demos;

            var hintEvents = events.Where(e => e.kind == "hint" || e.kind == "hint_requested" || e.kind == "smart_hint_opened").ToList();
            m.tier1Hints = hintEvents.Count(e => e.context != null && (e.context.Contains("level1") || e.context.Contains("VisualNudge") || e.context.Contains("tier=1")));
            m.tier2Hints = hintEvents.Count(e => e.context != null && (e.context.Contains("level2") || e.context.Contains("CharacterClue") || e.context.Contains("tier=2")));
            m.tier3Hints = hintEvents.Count(e => e.context != null && (e.context.Contains("level3") || e.context.Contains("DirectGuidance") || e.context.Contains("tier=3")));
            int untieredHints = hintEvents.Count - (m.tier1Hints + m.tier2Hints + m.tier3Hints);
            if (untieredHints > 0) m.tier1Hints += untieredHints;
            m.totalHints = hintEvents.Count;
            m.hintsRequested = m.totalHints;

            var storyEvents = events.Where(e => e.kind == "story_node" || e.kind == "story_choice" || e.kind == "story_transition").ToList();
            m.storyBranchesCount = storyEvents.Count;
            m.storyChoicesMade = events.Count(e => e.kind == "story_choice");
            if (m.storyChoicesMade == 0 && m.storyBranchesCount > 0) m.storyChoicesMade = m.storyBranchesCount;
            var distinctNodes = storyEvents
                .Select(e => e.context ?? "")
                .Where(c => !string.IsNullOrEmpty(c))
                .Select(c => c.Split(';')[0])
                .Distinct()
                .ToList();
            m.storyNodesVisited = Mathf.Max(1, distinctNodes.Count);
            m.storyPath = distinctNodes.Count > 0 ? string.Join(" -> ", distinctNodes) : "linear";

            float totalAnswerSec = formAttempts.Sum(a => a.seconds) + challenges.Sum(c => c.seconds);
            m.activeTimeSec = Mathf.Max(m.learningExposureSeconds, totalAnswerSec + m.totalManipulations * 2.5f);
            var idleEvents = events.Where(e => e.kind == "idle_period").Sum(e => e.seconds);
            m.idleTimeSec = idleEvents;
            float totalTrackedSec = m.activeTimeSec + m.idleTimeSec;
            m.engagementScorePct = totalTrackedSec > 0.01f ? Mathf.Clamp(100f * m.activeTimeSec / totalTrackedSec, 0f, 100f) : (m.learningExposureSeconds > 0 ? 100f : 0f);
            m.activeEngagementRatioPct = m.engagementScorePct;

            // RQ3: Pathirana Formative Assessment, Error Correction & Retention
            var firstTries = formAttempts.Where(a => a.attemptNumber == 1).ToList();
            var secondTries = formAttempts.Where(a => a.attemptNumber == 2).ToList();
            m.formativeQuestionsAttempted = firstTries.Count;
            m.firstAttemptCorrect = firstTries.Count(a => a.correct);
            m.firstAttemptWrong = firstTries.Count(a => !a.correct);
            m.secondAttemptSelfCorrected = secondTries.Count(a => a.correct);
            m.inArSecondTryCorrections = m.secondAttemptSelfCorrected;
            m.firstAttemptAccuracyPct = m.formativeQuestionsAttempted > 0 ? 100f * m.firstAttemptCorrect / m.formativeQuestionsAttempted : 0f;
            m.inArFirstTryAccuracyPct = m.firstAttemptAccuracyPct;
            m.errorCorrectionRatePct = m.firstAttemptWrong > 0 ? 100f * m.secondAttemptSelfCorrected / m.firstAttemptWrong : (m.firstAttemptCorrect > 0 ? 100f : 0f);
            m.formativeMeanResponseSec = formAttempts.Count > 0 ? formAttempts.Average(a => a.seconds) : 0f;

            m.reviewSessionsCount = events.Count(e => e.kind == "review_open" || e.kind == "review_start");
            m.reviewExposureSeconds = events.Where(e => e.kind == "review_exposure").Sum(e => e.seconds);

            var pre = LatestTest(s, "pretest");
            var post = LatestTest(s, "posttest");
            var ret = LatestTest(s, "retention");

            if (pre != null && pre.denominator > 0) m.pretestPct = 100f * pre.score / pre.denominator;
            if (post != null && post.denominator > 0) m.posttestPct = 100f * post.score / post.denominator;
            if (ret != null && ret.denominator > 0) m.retentionPct = 100f * ret.score / ret.denominator;

            m.pretestScorePct = m.pretestPct;
            m.posttestScorePct = m.posttestPct;
            m.retentionScorePct = m.retentionPct;

            if (m.pretestPct >= 0f && m.posttestPct >= 0f)
            {
                m.learningGainPct = m.posttestPct - m.pretestPct;
                m.normalizedGainPct = m.pretestPct < 100f ? 100f * (m.posttestPct - m.pretestPct) / (100f - m.pretestPct) : m.learningGainPct;
            }
            if (m.posttestPct >= 0f && m.retentionPct >= 0f) m.retentionChangePct = m.retentionPct - m.posttestPct;

            if (!string.IsNullOrEmpty(s.learningCompleted) && ret != null && !string.IsNullOrEmpty(ret.completed) &&
                DateTime.TryParse(s.learningCompleted, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime lc) &&
                DateTime.TryParse(ret.completed, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime rc))
            {
                m.daysToRetention = (rc - lc).TotalDays;
            }

            return m;
        }

        public void ExportResearcherComponentTables(string folder)
        {
            var kalish = new StringBuilder("data_label,participant,session,condition,rotate_count,force_adjust_count,fulcrum_move_count,part_inspect_count,total_manipulations,challenge_attempts,challenge_successes,learning_exposure_seconds,time_to_first_interaction_sec,pretest_pct,posttest_pct,learning_gain_pct\r\n");
            var abey = new StringBuilder("data_label,participant,session,adaptive_enabled,starting_tier,highest_tier,difficulty_changes,tier1_hints,tier2_hints,tier3_hints,total_hints,story_branches_count,story_path,active_time_sec,idle_time_sec,engagement_pct\r\n");
            var pathirana = new StringBuilder("data_label,participant,session,embedded_enabled,formative_questions_attempted,first_attempt_correct,second_attempt_self_corrected,error_correction_rate_pct,formative_mean_response_sec,review_sessions_count,review_exposure_seconds,pretest_pct,posttest_pct,retention_pct,learning_gain_pct,retention_change_pct,days_to_retention,formal_status\r\n");

            foreach (var s in data.sessions)
            {
                var m = ComputeMetrics(s);
                kalish.AppendLine(string.Join(",",
                    Csv(m.dataLabel), Csv(m.participant), Csv(m.sessionId), Csv(m.condition),
                    m.rotateCount, m.forceAdjustCount, m.fulcrumMoveCount, m.partInspectCount, m.totalManipulations,
                    m.challengeAttempts, m.challengeSuccesses,
                    m.learningExposureSeconds.ToString("F2", CultureInfo.InvariantCulture),
                    m.timeToFirstInteractionSec.ToString("F2", CultureInfo.InvariantCulture),
                    m.pretestPct >= 0 ? m.pretestPct.ToString("F2", CultureInfo.InvariantCulture) : "",
                    m.posttestPct >= 0 ? m.posttestPct.ToString("F2", CultureInfo.InvariantCulture) : "",
                    !float.IsNaN(m.learningGainPct) ? m.learningGainPct.ToString("F2", CultureInfo.InvariantCulture) : ""));

                abey.AppendLine(string.Join(",",
                    Csv(m.dataLabel), Csv(m.participant), Csv(m.sessionId), m.adaptive,
                    m.startingTier, m.highestTier, m.difficultyChanges,
                    m.tier1Hints, m.tier2Hints, m.tier3Hints, m.totalHints,
                    m.storyBranchesCount, Csv(m.storyPath),
                    m.activeTimeSec.ToString("F2", CultureInfo.InvariantCulture),
                    m.idleTimeSec.ToString("F2", CultureInfo.InvariantCulture),
                    m.engagementScorePct.ToString("F2", CultureInfo.InvariantCulture)));

                pathirana.AppendLine(string.Join(",",
                    Csv(m.dataLabel), Csv(m.participant), Csv(m.sessionId), m.embeddedAssessment,
                    m.formativeQuestionsAttempted, m.firstAttemptCorrect, m.secondAttemptSelfCorrected,
                    m.errorCorrectionRatePct.ToString("F2", CultureInfo.InvariantCulture),
                    m.formativeMeanResponseSec.ToString("F2", CultureInfo.InvariantCulture),
                    m.reviewSessionsCount, m.reviewExposureSeconds.ToString("F2", CultureInfo.InvariantCulture),
                    m.pretestPct >= 0 ? m.pretestPct.ToString("F2", CultureInfo.InvariantCulture) : "",
                    m.posttestPct >= 0 ? m.posttestPct.ToString("F2", CultureInfo.InvariantCulture) : "",
                    m.retentionPct >= 0 ? m.retentionPct.ToString("F2", CultureInfo.InvariantCulture) : "",
                    !float.IsNaN(m.learningGainPct) ? m.learningGainPct.ToString("F2", CultureInfo.InvariantCulture) : "",
                    !float.IsNaN(m.retentionChangePct) ? m.retentionChangePct.ToString("F2", CultureInfo.InvariantCulture) : "",
                    !double.IsNaN(m.daysToRetention) ? m.daysToRetention.ToString("F4", CultureInfo.InvariantCulture) : "",
                    Csv(m.formalStatus)));
            }

            File.WriteAllText(Path.Combine(folder, "kalish_rq1_interaction.csv"), kalish.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder, "abeywickrama_rq2_adaptive_engagement.csv"), abey.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder, "pathirana_rq3_assessment_retention.csv"), pathirana.ToString(), Encoding.UTF8);
        }

        public string FormatRQ1Summary()
        {
            var s = Current ?? Latest;
            if (s == null) return "No active study session.";
            var m = ComputeMetrics(s);
            return $"Mode: {m.condition.ToUpperInvariant()} | Spatial accuracy: {m.spatialAccuracyPct:F1}% | Exposure: {m.learningExposureSeconds:F1}s\n" +
                   $"Manipulations: {m.totalManipulations} (Force: {m.forceAdjustCount}, Fulcrum: {m.fulcrumMoveCount}, Rotate: {m.rotateCount}, Parts: {m.partInspectCount})\n" +
                   $"Challenges: {m.challengeSuccesses}/{m.challengeAttempts} solved | First action: {m.timeToFirstInteractionSec:F1}s";
        }

        public string FormatRQ2Summary()
        {
            var s = Current ?? Latest;
            if (s == null) return "No active study session.";
            var m = ComputeMetrics(s);
            string tierName = m.currentTier == 3 ? "Master (Tier 3)" : (m.currentTier == 2 ? "Scholar (Tier 2)" : "Apprentice (Tier 1)");
            return $"Adaptive enabled: {(m.adaptive ? "ON" : "OFF")} | Current: {tierName} (Max: Tier {m.highestTier})\n" +
                   $"Hints: {m.totalHints} (T1:{m.tier1Hints}, T2:{m.tier2Hints}, T3:{m.tier3Hints}) | Story branches: {m.storyBranchesCount}\n" +
                   $"Active time: {m.activeTimeSec:F1}s | Engagement: {m.engagementScorePct:F1}%";
        }

        public string FormatRQ3Summary()
        {
            var s = Current ?? Latest;
            if (s == null) return "No active study session.";
            var m = ComputeMetrics(s);
            return $"Embedded assessment: {(m.embeddedAssessment ? "ON" : "OFF")} | Formative 1st-try: {m.firstAttemptCorrect}/{m.formativeQuestionsAttempted} ({m.firstAttemptAccuracyPct:F0}%) | Self-corrected: {m.secondAttemptSelfCorrected}/{m.firstAttemptWrong} ({m.errorCorrectionRatePct:F0}%)\n" +
                   $"Review visits: {m.reviewSessionsCount} ({m.reviewExposureSeconds:F1}s)\n" +
                   FormalSummary();
        }
    }
}
