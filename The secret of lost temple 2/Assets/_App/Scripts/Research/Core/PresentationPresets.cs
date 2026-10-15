using System;
using System.Linq;
using UnityEngine;

namespace SecretOfLostTemple.Research
{
    public sealed partial class ResearchStore
    {
        public const string DemoLabel = "[SYNTHETIC DEMO DATA]";

        // Only the physically separate presentation store may create simulated records.
        public void LoadPresentationPreset(string preset, bool restart = false)
        {
            if (!IsPresentationStore) throw new InvalidOperationException("Demo presets require the separate presentation dataset.");
            string code;
            switch (preset)
            {
                case "kalish_passive": code = "P990001"; break;
                case "kalish_interactive": code = "P990002"; break;
                case "abeywickrama": code = "P990003"; break;
                case "pathirana": code = "P990004"; break;
                case "retention_example": code = "P990005"; break;
                default: throw new ArgumentException("Unknown presentation preset.");
            }
            // Suspend the previous demo, rather than ending a real or unfinished visit.
            var saved = data.sessions.LastOrDefault(s => s.demoPreset == preset && string.IsNullOrEmpty(s.ended));
            if (restart && saved != null)
                Mutate(() => saved.ended = UtcNow().ToString("O"));
            ActiveId = null;
            ParticipantCode = null;
            if (!restart && saved != null)
            {
                ActiveId = saved.id; ParticipantCode = saved.participant;
                Record("demo_resume", DemoLabel + ";" + preset);
                return;
            }
            BeginStudy(code, DemoLabel + "/presentation-v1/" + preset,
                preset == "kalish_passive" ? "passive" : "interactive",
                preset == "abeywickrama", preset == "abeywickrama" || preset == "pathirana");
            Mutate(() => { Current.dataLabel = DemoLabel; Current.demoPreset = preset; });
            Set("AdaptiveDifficultyTier", "1"); Set("AdaptiveSuccesses", "0"); Set("AdaptiveFailures", "0"); Set("StoryNode", "");
            if (preset == "retention_example") SeedRetentionExample();
            else if (preset != "pathirana")
                EnterExternalScore("pretest", "synthetic-baseline-v1", DemoLabel + " automatic baseline for presentation", 3, 6, true);
            Record("demo_preset", DemoLabel + ";" + preset);
        }

        void SeedRetentionExample()
        {
            // Explicit historical example, never a bypass of the real seven-day gate.
            var completed = UtcNow().AddDays(-7).AddMinutes(-1);
            Mutate(() => {
                var s = Current;
                s.started = completed.AddMinutes(-12).ToString("O");
                s.learningStarted = completed.AddMinutes(-10).ToString("O");
                s.learningCompleted = completed.ToString("O");
                string[] stages = { "pretest", "posttest", "retention" };
                int[] scores = { 2, 5, 4 };
                DateTime[] dates = { completed.AddMinutes(-11), completed.AddSeconds(20), UtcNow() };
                for (int i = 0; i < stages.Length; i++) s.assessments.Add(new FormalAssessment {
                    id = Guid.NewGuid().ToString("N"), stage = stages[i], version = "synthetic-example-v1",
                    source = DemoLabel + " generated example, not administered", score = scores[i], denominator = 6,
                    administratorConfirmed = true, started = dates[i].ToString("O"), completed = dates[i].ToString("O"),
                    recordedAt = UtcNow().ToString("O")
                });
            });
            Record("synthetic_timeline", DemoLabel + ";historical timestamps generated for seven-day presentation;no real exposure or item responses claimed");
        }
    }
}
