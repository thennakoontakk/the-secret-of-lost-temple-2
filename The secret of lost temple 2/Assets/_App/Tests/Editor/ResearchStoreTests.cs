using System;
using System.IO;
using System.Globalization;
using NUnit.Framework;
using SecretOfLostTemple.Research;

public class ResearchStoreTests
{
    string folder, path;
    [SetUp] public void Setup() { folder = Path.Combine(Path.GetTempPath(), "LostTempleSyntheticTests", Guid.NewGuid().ToString("N")); path = Path.Combine(folder, "records.json"); }
    [TearDown] public void Cleanup() { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    [Test] public void ParticipantsAndAnswersAreIsolated()
    {
        var s = new ResearchStore(path); s.Begin("P001", false); s.Set("tier", "3"); s.Set("review", "question1"); s.Record("answer", "q1", true, true, 2); s.End();
        s.Begin("P002", false); Assert.AreEqual("1", s.Get("tier", "1")); Assert.AreEqual("", s.Get("review", "")); Assert.AreEqual(1, s.Current.events.Count);
        s.End(); s.Begin("P001", false); Assert.AreEqual("3", s.Get("tier", "1")); Assert.AreEqual("question1", s.Get("review", ""));
    }
    [Test] public void RestartRequiresResumeAndPreservesSessionId()
    {
        var s = new ResearchStore(path); s.Begin("P001", false); string id = s.ActiveId; s.Record("answer", "q1", true, false, 1.25f);
        var reopened = new ResearchStore(path); Assert.IsNull(reopened.Current); reopened.Begin("P001", true);
        Assert.AreEqual(id, reopened.ActiveId); Assert.IsTrue(reopened.Current.events.Exists(e => e.hasAnswer && !e.correct));
    }
    [Test] public void CorruptPrimaryRecoversBackupAndPreservesEvidence()
    {
        var s = new ResearchStore(path); s.Begin("P001", false); s.Set("tier", "2"); s.Record("interaction");
        File.WriteAllText(path, "corrupt"); var recovered = new ResearchStore(path); recovered.Begin("P001", true);
        Assert.AreEqual("2", recovered.Get("tier", "1")); Assert.IsNotNull(recovered.RecoveryNotice);
        Assert.AreEqual(1, Directory.GetFiles(folder, "*.corrupt-*").Length);
    }
    [Test] public void UnrecoverableDataBlocksWithoutOverwriting()
    {
        Directory.CreateDirectory(folder); File.WriteAllText(path, "broken");
        Assert.Throws<InvalidDataException>(() => new ResearchStore(path)); Assert.AreEqual("broken", File.ReadAllText(path));
    }
    [Test] public void ExportKeepsIdsMissingScoresAndInvariantDecimals()
    {
        var old = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            var s = new ResearchStore(path); s.Begin("P001", false); s.Record("answer", "quoted,\"context\"\nline", true, false, 1.25f);
            var export = s.Export(); string rows = File.ReadAllText(Path.Combine(export, "sessions.csv"));
            Assert.That(rows, Does.Contain(s.ActiveId)); Assert.That(rows, Does.Contain(",1.25,,,,,not_collected"));
            Assert.That(File.ReadAllText(Path.Combine(export, "events.csv")), Does.Contain("\"quoted,\"\"context\"\"\nline\""));
            Assert.AreNotEqual(export, s.Export());
        } finally { CultureInfo.CurrentCulture = old; }
    }
    [Test] public void PracticeAndEndedSessionsDoNotAcceptEvents()
    {
        var s = new ResearchStore(path); s.Record("practice"); Assert.AreEqual(0, s.Snapshot().sessions.Count);
        s.Begin("P001", false); s.End(); int count = s.Latest.events.Count; s.Record("after_end"); Assert.AreEqual(count, s.Latest.events.Count);
    }
    [Test] public void InvalidCodesAndAccidentalSessionSwitchAreRejected()
    {
        var s = new ResearchStore(path); Assert.Throws<ArgumentException>(() => s.Begin("student name", false));
        s.Begin("P001", false); Assert.Throws<InvalidOperationException>(() => s.Begin("P002", false));
        Assert.AreEqual("P001", s.ParticipantCode);
    }
}
