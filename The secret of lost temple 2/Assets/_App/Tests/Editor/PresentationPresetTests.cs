using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SecretOfLostTemple.Research;

public class PresentationPresetTests
{
    string folder; ResearchStore store;
    [SetUp] public void Setup() {
        folder=Path.Combine(Path.GetTempPath(),"LostTempleDemoAudit",Guid.NewGuid().ToString("N"));
        store=new ResearchStore(Path.Combine(folder,"records.json"),true);
    }
    [TearDown] public void Cleanup(){if(Directory.Exists(folder))Directory.Delete(folder,true);}
    [Test] public void RealStoreRejectsSyntheticPresets(){
        var real=new ResearchStore(Path.Combine(folder,"real.json"));
        Assert.Throws<InvalidOperationException>(()=>real.LoadPresentationPreset("retention_example"));
        Assert.AreEqual(0,real.Snapshot().sessions.Count);
    }
    [Test] public void KalishModesHaveIdenticalTeachingSettingsAndResume(){
        store.LoadPresentationPreset("kalish_passive");store.StartLearning();var id=store.ActiveId;
        Assert.IsFalse(store.CanManipulate);Assert.IsFalse(store.Current.adaptive);Assert.IsFalse(store.Current.embeddedAssessment);
        store.LoadPresentationPreset("kalish_interactive");store.StartLearning();Assert.IsTrue(store.CanManipulate);
        Assert.IsFalse(store.Current.adaptive);Assert.IsFalse(store.Current.embeddedAssessment);
        store.LoadPresentationPreset("kalish_passive");Assert.AreEqual(id,store.ActiveId);
    }
    [Test] public void AdaptivePresetStartsAtOneAndRestartResetsStreak(){
        store.LoadPresentationPreset("abeywickrama");Assert.IsTrue(store.Current.adaptive);Assert.IsTrue(store.Current.embeddedAssessment);
        Assert.AreEqual("1",store.Get("AdaptiveDifficultyTier",""));store.Set("AdaptiveDifficultyTier","3");store.Set("AdaptiveSuccesses","2");
        var id=store.ActiveId;store.LoadPresentationPreset("abeywickrama",true);
        Assert.AreNotEqual(id,store.ActiveId);Assert.AreEqual("1",store.Get("AdaptiveDifficultyTier",""));Assert.AreEqual("0",store.Get("AdaptiveSuccesses",""));
    }
    [Test] public void PathiranaRequiresPretestAndRealTrackedLearning(){
        store.LoadPresentationPreset("pathirana");Assert.IsTrue(store.Current.embeddedAssessment);
        Assert.Throws<InvalidOperationException>(()=>store.StartLearning());
        store.StartTest("pretest");foreach(var q in LeverTestBank.Questions)store.AnswerFormal(q.correct,1);
        store.StartLearning();Assert.Throws<InvalidOperationException>(()=>store.CompleteLearning());
        store.Record("learning_exposure","test",false,false,10);store.CompleteLearning();store.StartTest("posttest");
        foreach(var q in LeverTestBank.Questions)store.AnswerFormal(q.correct,1);
        Assert.Throws<InvalidOperationException>(()=>store.StartTest("retention"));
    }
    [Test] public void SyntheticRetentionHasLabelScoresIntervalAndNoInventedAnswers(){
        store.LoadPresentationPreset("retention_example");
        Assert.AreEqual(ResearchStore.DemoLabel,store.Current.dataLabel);Assert.GreaterOrEqual(store.DaysSinceLearning(store.Current),7);
        CollectionAssert.AreEqual(new[]{2,5,4},store.Current.assessments.Select(a=>a.score).ToArray());
        Assert.IsTrue(store.Current.assessments.All(a=>a.answers.Count==0 && a.source.Contains(ResearchStore.DemoLabel)));
        var id=store.ActiveId;store.LoadPresentationPreset("retention_example");Assert.AreEqual(id,store.ActiveId);
        var export=store.Export();Assert.That(File.ReadAllText(Path.Combine(export,"DATASET_LABEL.txt")),Does.Contain(ResearchStore.DemoLabel));
        Assert.That(File.ReadAllText(Path.Combine(export,"sessions.csv")),Does.Contain(ResearchStore.DemoLabel));
    }
}
