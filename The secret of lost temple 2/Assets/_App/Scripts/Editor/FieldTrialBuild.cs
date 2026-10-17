using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class FieldTrialBuild
{
    public static void Android()
    {
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName = "build/the secret of lost temple.apk",
            target = BuildTarget.Android, options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("Field trial APK failed: " + report.summary.result);
    }
}
