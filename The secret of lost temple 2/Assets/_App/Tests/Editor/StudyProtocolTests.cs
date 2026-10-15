using System;
using System.IO;
using NUnit.Framework;
using SecretOfLostTemple.Research;

public class StudyProtocolTests
{
    string dir,path; ResearchStore store; DateTime now;
    [SetUp] public void Setup()
    {
        dir=Path.Combine(Path.GetTempPath(),"LostTempleProtocolTests",Guid.NewGuid().ToString("N"));path=Path.Combine(dir,"records.json");
        store=new ResearchStore(path);now=new DateTime(2026,10,1,12,0,0,DateTimeKind.Utc);store.UtcNow=()=>now;
        store.BeginStudy("P001","approved-test-v1","passive",true,true);
    }
    [TearDown] public void Cleanup(){if(Directory.Exists(dir))Directory.Delete(dir,true);}
    void Test(string stage)
    {
        store.StartTest(stage);
        foreach(var q in LeverTestBank.Questions)store.AnswerFormal(q.correct,1.25f);
    }
    void Learn(){Test("pretest");store.StartLearning();store.Record("learning_exposure","synthetic",false,false,10);store.CompleteLearning();}
    [Test] public void StageOrderAndDelayedGateAreEnforced()
    {
        Assert.Throws<InvalidOperationException>(()=>store.StartLearning());
        Assert.Throws<InvalidOperationException>(()=>store.StartTest("posttest"));
        Learn();Test("posttest");now=now.AddDays(6.999);
        Assert.Throws<InvalidOperationException>(()=>store.StartTest("retention"));
        now=now.AddDays(.001);Test("retention");
        Assert.AreEqual(6,ResearchStore.LatestTest(store.Current,"retention").score);
    }
    [Test] public void FormalTestSurvivesRestartWithoutDuplicateAnswers()
    {
        var a=store.StartTest("pretest");string id=a.id;store.AnswerFormal(0,1);
        var reopen=new ResearchStore(path);reopen.Begin("P001",true);
        Assert.AreEqual(id,reopen.StartTest("pretest").id);Assert.AreEqual(1,reopen.PendingTest.answers.Count);
        Assert.Throws<InvalidOperationException>(()=>reopen.StartTest("posttest"));
    }
    [Test] public void MissingTestsRemainMissingAndZeroIsARealScore()
    {
        Assert.That(store.FormalSummary(),Does.Contain("pretest: not collected"));
        store.EnterExternalScore("pretest","paper-v1","paper",0,10,true);
        Assert.That(store.FormalSummary(),Does.Contain("0/10"));Assert.That(store.FormalSummary(),Does.Contain("posttest: not collected"));
    }
    [Test] public void CorrectionsPreserveOriginalAndRequireReason()
    {
        store.EnterExternalScore("pretest","paper-v1","paper",3,10,true);
        string first=ResearchStore.LatestTest(store.Current,"pretest").id;
        Assert.Throws<ArgumentException>(()=>store.EnterExternalScore("pretest","paper-v1","paper",4,10,true));
        store.EnterExternalScore("pretest","paper-v1","paper",4,10,true,"transcription correction");
        Assert.AreEqual(2,store.Current.assessments.Count);Assert.AreEqual(first,ResearchStore.LatestTest(store.Current,"pretest").replaces);
    }
    [Test] public void CorrectionPreservesTestDateAndRecordsCorrectionDate()
    {
        store.EnterExternalScore("pretest","paper-v1","paper",3,10,true);
        string measured=ResearchStore.LatestTest(store.Current,"pretest").completed;
        now=now.AddDays(2);
        store.EnterExternalScore("pretest","paper-v1","paper",4,10,true,"transcription");
        var revised=ResearchStore.LatestTest(store.Current,"pretest");
        Assert.AreEqual(measured,revised.completed);Assert.AreNotEqual(measured,revised.recordedAt);
    }
    [Test] public void ReopenedQuestionHasAnExplicitTimingFlag()
    {
        var a=store.StartTest("pretest");
        store.Record("formal_question_presented",a.id+":0");
        store.Record("formal_question_presented",a.id+":0");
        store.AnswerFormal(1,2);
        Assert.IsTrue(a.answers[0].timingRestarted);
    }
    [Test] public void ConditionIsRestoredAndFollowupKeepsOriginalSession()
    {
        Learn();Test("posttest");string id=store.ActiveId;store.End();now=now.AddDays(8);
        store.OpenFollowUp("P001");Assert.AreEqual(id,store.ActiveId);Assert.IsFalse(store.CanManipulate);
        Test("retention");Assert.AreEqual(8,store.DaysSinceLearning(store.Current),.001);
    }
    [Test] public void AttemptsAreIdempotentAndParticipantSpecific()
    {
        Test("pretest");store.StartLearning();store.RecordAttempt("lever","attempt1",1,0,false,1,1,"hint none");
        store.RecordAttempt("lever","attempt1",1,0,false,1,1,"duplicate");store.RecordAttempt("lever","attempt2",2,1,true,1,1,"hint1");
        Assert.AreEqual(2,store.Current.events.FindAll(x=>x.hasAnswer).Count);
        store.End();store.BeginStudy("P002","test","interactive",false,false);
        Assert.AreEqual(0,store.Current.assessments.Count);Assert.IsTrue(store.CanManipulate);
    }
    [Test] public void ExportsReproduceScoresAndLateInterval()
    {
        Learn();Test("posttest");now=now.AddDays(9);Test("retention");
        string folder=store.Export();
        Assert.That(File.ReadAllText(Path.Combine(folder,"sessions.csv")),Does.Contain(",100.0000,100.0000,100.0000,0.0000,complete"));
        Assert.That(File.ReadAllText(Path.Combine(folder,"formal_tests.csv")),Does.Contain("9.000000"));
        Assert.AreEqual(19,File.ReadAllLines(Path.Combine(folder,"formal_answers.csv")).Length);
    }
}
