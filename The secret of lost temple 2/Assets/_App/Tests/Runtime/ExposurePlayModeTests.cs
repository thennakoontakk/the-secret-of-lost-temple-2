using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SecretOfLostTemple.Research;
using Data = SecretOfLostTemple.Research.Research;

public class ExposurePlayModeTests
{
    GameObject root;
    ResearchStore store, previous;
    bool demo;
    string folder;
    Type runtimeType, policy;
    Component runtime;
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    [SetUp] public void Setup()
    {
        var sync = Type.GetType("SecretOfLostTemple.Research.ResearchCloudSync, Assembly-CSharp", true);
        foreach (var obj in UnityEngine.Object.FindObjectsByType(sync, FindObjectsSortMode.None))
        { var behaviour = (MonoBehaviour)obj; behaviour.StopAllCoroutines(); behaviour.enabled = false; }
        demo = Data.DemoMode;
        var field = typeof(Data).GetField("demoStore", BindingFlags.Static | BindingFlags.NonPublic);
        previous = (ResearchStore)field.GetValue(null);
        folder = Path.Combine(Application.temporaryCachePath,"ExposurePlayTests",Guid.NewGuid().ToString("N"));
        store = new ResearchStore(Path.Combine(folder,"records.json"),true);
        field.SetValue(null,store); Data.UsePresentationData(true);
        runtimeType = Type.GetType("StudyRuntime, Assembly-CSharp",true);
        policy = Type.GetType("StudyPolicy, Assembly-CSharp",true);
    }
    [TearDown] public void Cleanup()
    {
        if(root != null) UnityEngine.Object.DestroyImmediate(root);
        policy.GetField("SimulatedTracking").SetValue(null,false);
        typeof(Data).GetField("demoStore",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,previous);
        Data.UsePresentationData(demo);
        if(Directory.Exists(folder)) Directory.Delete(folder,true);
    }
    [UnityTest] public IEnumerator InteractiveCameraLockCountsOnlyActiveFrames() { return Exercise("interactive"); }
    [UnityTest] public IEnumerator PassiveCameraLockCountsOnlyActiveFrames() { return Exercise("passive"); }
    IEnumerator Exercise(string condition)
    {
        store.BeginStudy("P999", "synthetic-runtime-test",condition,false,false);
        store.StartTest("pretest"); foreach(var q in LeverTestBank.Questions) store.AnswerFormal(q.correct,1);
        store.StartLearning();
        root = new GameObject("Exposure runtime probe"); runtime = root.AddComponent(runtimeType);
        yield return null;
        policy.GetField("Tracking").SetValue(null,true);
        policy.GetField("SimulatedTracking").SetValue(null,true);
        runtimeType.GetMethod("OnApplicationFocus",Private).Invoke(runtime,new object[]{true});
        yield return new WaitForSecondsRealtime(.2f);
        runtimeType.GetMethod("FlushActive").Invoke(null,null);
        float measured = store.ComputeMetrics().learningExposureSeconds;
        Assert.Greater(measured,0);
        runtimeType.GetMethod("OnApplicationPause",Private).Invoke(runtime,new object[]{true});
        float paused = store.ComputeMetrics().learningExposureSeconds;
        yield return new WaitForSecondsRealtime(.1f);
        runtimeType.GetMethod("FlushActive").Invoke(null,null);
        Assert.AreEqual(paused,store.ComputeMetrics().learningExposureSeconds);
        runtimeType.GetMethod("OnApplicationPause",Private).Invoke(runtime,new object[]{false});
        policy.GetField("Tracking").SetValue(null,false);
        yield return new WaitForSecondsRealtime(.1f);
        runtimeType.GetMethod("FlushActive").Invoke(null,null);
        Assert.AreEqual(paused,store.ComputeMetrics().learningExposureSeconds);
        Assert.AreEqual(condition=="passive",policy.GetProperty("Passive").GetValue(null));
        Assert.DoesNotThrow(()=>store.CompleteLearning(true));
    }
}
