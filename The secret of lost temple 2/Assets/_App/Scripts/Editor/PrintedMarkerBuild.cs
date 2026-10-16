using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.ARSubsystems;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace SecretOfLostTemple.EditorScripts
{
    public static class PrintedMarkerBuild
    {
        [MenuItem("Lost Temple/Repair Lever Research Scene")]
        public static void RepairScene()
        {
            const string scenePath = "Assets/_App/Scenes/MainARScene.unity";
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            if (GameObject.Find("AR Main Canvas/LeverAR_UIRoot") == null)
            {
                SecretOfLostTemple.Editor.LeverARUISetup.BuildLeverARUI();
                SecretOfLostTemple.Editor.LeverARUISetup.SetupARImageTrackingInMainARScene();
            }
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var canvas = GameObject.Find("AR Main Canvas");
            var origin = GameObject.Find("XR Origin (Mobile AR)");
            if (canvas == null || origin == null) throw new InvalidOperationException("AR scene is missing its canvas or XR origin.");
            foreach (var legacy in UnityEngine.Object.FindObjectsByType<SecretOfLostTemple.AR.ARSceneUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                legacy.enabled = false;
            foreach (var legacy in origin.GetComponents<SecretOfLostTemple.AR.ARTrackedImageMachineAnchor>()) UnityEngine.Object.DestroyImmediate(legacy);
            foreach (var legacy in origin.GetComponents<SecretOfLostTemple.AR.QuestARImageAnchorController>()) UnityEngine.Object.DestroyImmediate(legacy);
            foreach (var legacy in origin.GetComponents<SecretOfLostTemple.AR.WebCamMarkerDetector>()) UnityEngine.Object.DestroyImmediate(legacy);
            var manager = origin.GetComponent<UnityEngine.XR.ARFoundation.ARTrackedImageManager>() ?? origin.AddComponent<UnityEngine.XR.ARFoundation.ARTrackedImageManager>();
            manager.referenceLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>("Assets/XR/ReferenceImageLibraries/SimpleMachinesReferenceImageLibrary.asset");
            manager.enabled = true;
            var spawner = origin.GetComponent<SecretOfLostTemple.AR.QuestARMarkerSpawner>() ?? origin.AddComponent<SecretOfLostTemple.AR.QuestARMarkerSpawner>();
            var serialized = new SerializedObject(spawner);
            serialized.FindProperty("trackedImageManager").objectReferenceValue = manager;
            serialized.FindProperty("referenceLibrary").objectReferenceValue = manager.referenceLibrary as XRReferenceImageLibrary;
            serialized.FindProperty("questLeverPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_App/Prefabs/Quest_Lever_MarkerModel.prefab");
            serialized.FindProperty("scanningMarkerOverlay").objectReferenceValue = canvas.transform.Find("ScanningMarkerOverlay")?.gameObject;
            serialized.FindProperty("leverARUIRoot").objectReferenceValue = canvas.transform.Find("LeverAR_UIRoot")?.gameObject;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            foreach (var name in new[] {"TopNavBar", "Component1_ModePanel", "BottomMachineBar", "Text_MechanicalAdvantage", "Button_RequestHint", "Button_StartQuiz", "TelemetryPanel", "StoryDialoguePanel"})
                canvas.transform.Find(name)?.gameObject.SetActive(false);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }
        public static void Configure()
        {
            const string path = "Assets/XR/ReferenceImageLibraries/SimpleMachinesReferenceImageLibrary.asset";
            var library = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(path);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Targets/lever_printed_target.png");
            if (library == null || texture == null) throw new InvalidOperationException("Printed lever target or library missing.");
            int index = Enumerable.Range(0, library.count).First(i => library[i].name == "lever_target");
            library.SetTexture(index, texture, false);
            // The book may be printed at different sizes. Let ARCore estimate its size.
            library.SetSpecifySize(index, false);
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("Printed lever marker registered: " + texture.width + "x" + texture.height);
        }

        [MenuItem("Lost Temple/Build Android with Printed Lever Marker")]
        public static void BuildAndroid()
        {
            Configure();
            RepairScene();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "Builds/LeverPrintedMarker.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Android build failed: " + report.summary.result);
        }
    }
}
