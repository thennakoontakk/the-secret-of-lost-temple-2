using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SecretOfLostTemple.AR;

public static class LeverModuleFinalize
{
    const string PrefabPath = "Assets/_App/Prefabs/Quest_Lever_MarkerModel.prefab";

    static void Layout(RectTransform rect, float x0, float y0, float x1, float y1)
    {
        rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    [MenuItem("Lost Temple/Finalize Lever Module")]
    public static void Run()
    {
        var original = EditorSceneManager.GetSceneManagerSetup();
        var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try {
            var beam = prefab.transform.Find("Lever_Beam");
            beam.localRotation = Quaternion.identity;
            beam.Find("Beam_3D_Model").localRotation = Quaternion.identity;
            // Beam.glb is authored along Z; keep the mechanism wrapper aligned to X.
            beam.Find("Beam_3D_Model/Model_Instance_Beam").localRotation = Quaternion.Euler(0, 90, 0);
            beam.Find("Load_Stone_DoorWeight").localPosition = new Vector3(-.9f, .15f, 0);
            beam.Find("Effort_Point_Handle").localPosition = new Vector3(.9f, .15f, 0);
            prefab.transform.Find("Fulcrum_Pivot").localPosition = Vector3.zero;
            // Fit the imported support's apex to the underside of the beam.
            var support = prefab.transform.Find("Fulcrum_Pivot/Model_Instance_Fulcrum2");
            var supportRenderers = support.GetComponentsInChildren<Renderer>();
            var beamRenderers = beam.Find("Beam_3D_Model").GetComponentsInChildren<Renderer>();
            var load = beam.Find("Load_Stone_DoorWeight");
            var loadRenderers = load.GetComponentsInChildren<Renderer>();
            if (load.childCount > 0 && loadRenderers.Length > 0 && beamRenderers.Length > 0) {
                float bottom = loadRenderers.Min(r => r.bounds.min.y);
                float top = beamRenderers.Max(r => r.bounds.max.y);
                load.GetChild(0).position += Vector3.up * (top - bottom);
            }
            if (supportRenderers.Length > 0 && beamRenderers.Length > 0) {
                float apex = supportRenderers.Max(r => r.bounds.max.y);
                float underside = beamRenderers.Min(r => r.bounds.min.y);
                support.position += Vector3.up * (underside - apex);
            }
            foreach (var mesh in prefab.GetComponentsInChildren<MeshFilter>(true))
                if (mesh.sharedMesh != null && mesh.GetComponent<Collider>() == null)
                    mesh.gameObject.AddComponent<BoxCollider>();
            PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);

            var scene = EditorSceneManager.OpenScene("Assets/_App/Scenes/MainARScene.unity");
            var root = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).First(g => g.name == "SimpleMachine_Lever");
            foreach (var name in new[] { "Lever_Beam", "Fulcrum_Pivot" }) {
                var target = root.Find(name);
                var source = prefab.transform.Find(name);
                // Preserve scene object IDs referenced by controllers.
                target.localPosition = source.localPosition;
                target.localRotation = source.localRotation;
                target.localScale = source.localScale;
                foreach (var renderer in target.GetComponents<Renderer>()) UnityEngine.Object.DestroyImmediate(renderer);
                foreach (var mesh in target.GetComponents<MeshFilter>()) UnityEngine.Object.DestroyImmediate(mesh);
                foreach (var collider in target.GetComponents<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
                foreach (Transform child in source) {
                    var existing = target.Find(child.name);
                    if (existing != null && (child.name == "Load_Stone_DoorWeight" || child.name == "Effort_Point_Handle")) {
                        existing.localPosition = child.localPosition;
                        existing.localRotation = child.localRotation;
                        existing.localScale = child.localScale;
                        foreach (var renderer in existing.GetComponents<Renderer>()) UnityEngine.Object.DestroyImmediate(renderer);
                        foreach (var mesh in existing.GetComponents<MeshFilter>()) UnityEngine.Object.DestroyImmediate(mesh);
                        foreach (Transform nested in child) {
                            var old = existing.Find(nested.name);
                            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                            UnityEngine.Object.Instantiate(nested.gameObject, existing, false).name = nested.name;
                        }
                    } else {
                        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
                        UnityEngine.Object.Instantiate(child.gameObject, target, false).name = child.name;
                    }
                }
            }
            ValidateScene(scene);
            EditorSceneManager.SaveScene(scene);
            scene = EditorSceneManager.OpenScene("Assets/_App/Scenes/Physics3DLabScene.unity");
            var lab = UnityEngine.Object.FindFirstObjectByType<Physics3DLabController>(FindObjectsInactive.Include);
            if (lab == null) throw new InvalidOperationException("Missing physics lab controller");
            lab.enabled = true; lab.gameObject.SetActive(true);
            var so = new SerializedObject(lab);
            var ma = (Text)so.FindProperty("mechanicalAdvantageValueText").objectReferenceValue;
            var torque = (Text)so.FindProperty("torqueEquationText").objectReferenceValue;
            if (torque == null || torque == ma) {
                torque = UnityEngine.Object.Instantiate(ma, ma.transform.parent, false);
                torque.name = "Text_Torque";
                so.FindProperty("torqueEquationText").objectReferenceValue = torque;
            }
            Layout(ma.rectTransform, 0, .66f, 1, 1);
            Layout(torque.rectTransform, 0, .26f, 1, .66f);
            var required = (Text)so.FindProperty("requiredForceValueText").objectReferenceValue;
            Layout(required.rectTransform, 0, 0, 1, .26f);
            foreach (var text in new[] { ma, torque, required }) {
                text.fontSize = 18; text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 12; text.resizeTextMaxSize = 18;
            }
            var reset = (Button)so.FindProperty("resetSimulationButton").objectReferenceValue;
            if (reset == null) {
                var preset = (Button)so.FindProperty("presetEqualArmsButton").objectReferenceValue;
                reset = UnityEngine.Object.Instantiate(preset, preset.transform.parent, false);
                reset.name = "Button_ResetSimulation";
                var rect = (RectTransform)reset.transform;
                rect.anchoredPosition += new Vector2(0, -65);
                var text = reset.GetComponentInChildren<Text>();
                if (text != null) text.text = "RESET";
                reset.onClick = new Button.ButtonClickedEvent();
                so.FindProperty("resetSimulationButton").objectReferenceValue = reset;
            }
            string[] fields = { "presetHighMAButton", "presetEqualArmsButton", "presetLowMAButton", "resetSimulationButton" };
            string[] labels = { "HIGH MA", "EQUAL", "LOW MA", "RESET" };
            for (int i = 0; i < fields.Length; i++) {
                var button = (Button)so.FindProperty(fields[i]).objectReferenceValue;
                Layout((RectTransform)button.transform, .68f + i * .076f, .08f, .75f + i * .076f, .4f);
                var label = button.GetComponentInChildren<Text>();
                if (label != null) { label.text = labels[i]; label.resizeTextForBestFit = true; label.resizeTextMinSize = 12; }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            var physics = UnityEngine.Object.FindFirstObjectByType<LeverPhysicsController>(FindObjectsInactive.Include);
            var po = new SerializedObject(physics);
            po.FindProperty("boulderMassKg").floatValue = 100f / 9.81f;
            po.ApplyModifiedPropertiesWithoutUndo();
            ValidateScene(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("LEVER_FINALIZE_OK: prefab, review model, lab controller and reset repaired.");
        } finally {
            PrefabUtility.UnloadPrefabContents(prefab);
            EditorSceneManager.RestoreSceneManagerSetup(original);
        }
    }

    static void ValidateScene(UnityEngine.SceneManagement.Scene scene)
    {
        int missing = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
            .Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
        if (missing != 0) throw new InvalidOperationException(scene.name + " contains " + missing + " missing scripts.");
    }
}
