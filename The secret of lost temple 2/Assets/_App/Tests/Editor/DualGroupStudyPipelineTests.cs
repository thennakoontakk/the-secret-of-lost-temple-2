using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SecretOfLostTemple.Research;
using Data = SecretOfLostTemple.Research.Research;

public class DualGroupStudyPipelineTests
{
    ResearchStore store, previous;
    bool previousDemo;
    GameObject root;
    string folder;
    Type runtimeType, policyType;
    Component runtime;
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    [SetUp] public void Setup()
    {
        folder = Path.Combine(Path.GetTempPath(), "DualGroupStudy", Guid.NewGuid().ToString("N"));
        store = new ResearchStore(Path.Combine(folder, "records.json"), true);
        previousDemo = Data.DemoMode;
        var field = typeof(Data).GetField("demoStore", BindingFlags.Static | BindingFlags.NonPublic);
        previous = (ResearchStore)field.GetValue(null);
        field.SetValue(null, store); Data.UsePresentationData(true);
        root = new GameObject("Isolated study pipeline test");
        runtimeType = Type.GetType("StudyRuntime, Assembly-CSharp", true);
        policyType = Type.GetType("StudyPolicy, Assembly-CSharp", true);
        runtime = root.AddComponent(runtimeType);
        policyType.GetField("Tracking").SetValue(null, true);
        policyType.GetField("SimulatedTracking").SetValue(null, true);
    }
    [TearDown] public void Cleanup()
    {
        UnityEngine.Object.DestroyImmediate(root);
        policyType.GetField("SimulatedTracking").SetValue(null, false);
        typeof(Data).GetField("demoStore", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, previous);
        Data.UsePresentationData(previousDemo);
        Directory.Delete(folder, true);
    }
    void Answers(string stage, int correct)
    {
        store.StartTest(stage);
        for (int i = 0; i < LeverTestBank.Questions.Length; i++)
        {
            var q = LeverTestBank.Questions[i];
            store.AnswerFormal(i < correct ? q.correct : (q.correct + 1) % q.options.Length, 1);
        }
    }
    [TestCase("P101", "interactive", 5, 50f)]
    [TestCase("P102", "passive", 3, 16.66667f)]
    public void BothGroupsCompleteRemotePipeline(string code, string condition, int score, float gain)
    {
        bool interactive = condition == "interactive";
        store.BeginStudy(code, "lever-pilot-1", condition, interactive, interactive);
        store.Current.dataLabel = ResearchStore.DemoLabel;
        Answers("pretest", 2); store.StartLearning();
        store.Record("marker_locked", "synthetic Page 9");
        Assert.AreEqual(!interactive, policyType.GetProperty("Passive").GetValue(null));
        Assert.AreEqual(interactive, policyType.GetProperty("InputAllowed").GetValue(null));
        if (interactive)
        {
            for (int i = 0; i < 3; i++) store.Record("fulcrum_committed", "synthetic");
            for (int i = 0; i < 2; i++) store.Record("adjust_force", "synthetic");
            store.Record("inspect_part", "synthetic exploded view");
            store.Record("hint_requested", "tier=1;synthetic");
            store.Record("difficulty_change", "to=2;synthetic");
            store.RecordAttempt("lever", "synthetic-1", 1, 1, true, 2, 2, "synthetic embedded quiz");
        }
        else
        {
            var controllerType = Type.GetType("SecretOfLostTemple.AR.LeverARScreenController, Assembly-CSharp", true);
            var controller = root.AddComponent(controllerType);
            controllerType.GetMethod("AdjustForce").Invoke(controller, new object[]{10f});
            controllerType.GetMethod("MoveFulcrumStep").Invoke(controller, new object[]{.1f});
            Assert.AreEqual(0, controllerType.GetField("forceChangeCount", Private).GetValue(controller));
            Assert.AreEqual(0, controllerType.GetField("fulcrumChangeCount", Private).GetValue(controller));
        }
        // Drive the production Update gate while camera-locked tracking is enabled.
        runtimeType.GetField("seconds", Private).SetValue(runtime, 25f);
        runtimeType.GetMethod("Update", Private).Invoke(runtime, null);
        Assert.That(store.ComputeMetrics().learningExposureSeconds, Is.GreaterThanOrEqualTo(25));
        var uiType = Type.GetType("ResearchSetupUI, Assembly-CSharp", true);
        var ui = root.AddComponent(uiType);
        uiType.GetMethod("Start", Private).Invoke(ui, null);
        Assert.DoesNotThrow(() => uiType.GetMethod("ExecuteRemoteCompleteLearning").Invoke(ui, new object[]{true}));
        Assert.AreEqual("posttest", store.PendingTest.stage);
        Answers("posttest", score);
        var metrics = store.ComputeMetrics();
        Assert.That(metrics.learningGainPct, Is.EqualTo(gain).Within(.001f));
        Assert.AreEqual(interactive ? 6 : 0, metrics.totalManipulations);
        Assert.AreEqual(interactive ? 1 : 0, metrics.totalHints);
        Assert.AreEqual(interactive ? 100 : 0, metrics.firstAttemptAccuracyPct);
        Assert.AreEqual(interactive ? 1 : 0, metrics.difficultyChanges);
        var export = store.Export();
        Assert.AreEqual(3, File.ReadAllLines(Path.Combine(export,"formal_tests.csv")).Length);
        Assert.AreEqual(13, File.ReadAllLines(Path.Combine(export,"formal_answers.csv")).Length);
        Assert.AreEqual(2, File.ReadAllLines(Path.Combine(export,"protocols.csv")).Length);
        Assert.AreEqual(interactive ? 2 : 1, File.ReadAllLines(Path.Combine(export,"attempts.csv")).Length);
        string evidence = Path.GetFullPath("build/verification/" + code);
        Directory.CreateDirectory(evidence);
        foreach (var file in Directory.GetFiles(export)) File.Copy(file, Path.Combine(evidence, Path.GetFileName(file)), true);
    }
    [Test] public void RemoteFallbackIsLabeledBoundedAndIdempotent()
    {
        store.BeginStudy("P103", "lever-pilot-1", "passive", false, false);
        Answers("pretest", 2); store.StartLearning();
        store.UtcNow = () => DateTime.UtcNow.AddHours(1);
        store.CompleteLearning(true); store.CompleteLearning(true);
        var exposure = store.Current.events.Single(e => e.kind == "learning_exposure");
        Assert.AreEqual(600, exposure.seconds);
        Assert.That(exposure.context, Does.Contain("estimated_wall_clock"));
    }
    [Test] public void FlushPreservesShortExposureBeforeCompletion()
    {
        store.BeginStudy("P104", "lever-pilot-1", "interactive", true, true);
        Answers("pretest", 2); store.StartLearning();
        runtimeType.GetField("seconds", Private).SetValue(runtime, 3f);
        runtimeType.GetMethod("FlushActive").Invoke(null, null);
        store.CompleteLearning(true);
        Assert.AreEqual(3, store.ComputeMetrics().learningExposureSeconds);
        Assert.AreEqual(1, store.Current.events.Count(e => e.kind == "learning_exposure"));
    }
}
