using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

public class LeverPhysicsRegressionTests
{
    GameObject root;
    Component physics;
    Type type;

    [SetUp] public void Setup()
    {
        type = Type.GetType("SecretOfLostTemple.AR.LeverPhysicsController, Assembly-CSharp", true);
        root = new GameObject("LeverPhysicsTest");
        physics = root.AddComponent(type);
    }
    [TearDown] public void Cleanup() { UnityEngine.Object.DestroyImmediate(root); }
    void Call(string method, float value) => type.GetMethod(method).Invoke(physics, new object[] { value });
    float Value(string property) => (float)type.GetProperty(property).GetValue(physics);
    void Field(string field, object value) => type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(physics, value);

    [Test] public void MarkerPrefabUsesHorizontalBeamAndRequestedAnchors()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_App/Prefabs/Quest_Lever_MarkerModel.prefab");
        var prefab = UnityEngine.Object.Instantiate(asset, root.transform);
        var beam = prefab.transform.Find("Lever_Beam");
        var renderer = beam.Find("Beam_3D_Model").GetComponentInChildren<Renderer>();
        var mf = renderer != null ? renderer.GetComponent<MeshFilter>() : null;
        if (mf != null && mf.sharedMesh == null)
        {
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Models/Beam.glb"))
            {
                if (obj is Mesh m) { mf.sharedMesh = m; break; }
            }
            if (mf.sharedMesh == null)
            {
                var fallback = new Mesh();
                fallback.vertices = new[] { new Vector3(-0.1f, -0.1f, -1f), new Vector3(0.1f, 0.1f, 1f), new Vector3(0.1f, -0.1f, 1f) };
                fallback.triangles = new[] { 0, 1, 2 };
                fallback.RecalculateBounds();
                mf.sharedMesh = fallback;
            }
        }
        Assert.That(Quaternion.Angle(beam.Find("Beam_3D_Model").localRotation, Quaternion.identity), Is.LessThan(.01f));
        Assert.That(Vector3.Distance(beam.Find("Load_Stone_DoorWeight").localPosition, new Vector3(-.9f, .15f, 0)), Is.LessThan(.001f));
        Assert.That(Vector3.Distance(beam.Find("Effort_Point_Handle").localPosition, new Vector3(.9f, .15f, 0)), Is.LessThan(.001f));
        Assert.That(prefab.transform.Find("Fulcrum_Pivot").localPosition.sqrMagnitude, Is.LessThan(.001f));
        Assert.That(renderer.bounds.size.x, Is.GreaterThan(renderer.bounds.size.z * 2));
    }

    [TestCase(.5f, 3.8f, 26.31579f)]
    [TestCase(1.2f, 1f, 100f)]
    [TestCase(1.6f, .5f, 200f)]
    public void PresetsMatchIdealLeverAndBalance(float fulcrum, float ma, float effort)
    {
        Call("SetFulcrumPosition", fulcrum);
        Assert.That(Value("LoadForceNewtons"), Is.EqualTo(100f).Within(.001f));
        Assert.That(Value("MechanicalAdvantage"), Is.EqualTo(ma).Within(.001f));
        Assert.That(Value("RequiredEffortForce"), Is.EqualTo(effort).Within(.001f));
        Call("SetAppliedEffortForce", effort);
        Assert.That(Value("NetTorque"), Is.EqualTo(0f).Within(.001f));
        Call("SetAppliedEffortForce", effort + 50);
        Assert.That((bool)type.GetProperty("IsBoulderLifted").GetValue(physics), Is.True);
    }

    [Test] public void TiltKeepsBeamInContactWithMovingFulcrum()
    {
        var beam = new GameObject("Beam").transform; beam.SetParent(root.transform);
        var support = new GameObject("Support").transform; support.SetParent(root.transform);
        Field("beamRoot", beam); Field("fulcrumRoot", support);
        beam.localRotation = Quaternion.Euler(0, 0, -14);
        Call("SetFulcrumPosition", .5f);
        var contact = beam.localPosition + beam.localRotation * new Vector3(-.7f, 0, 0);
        Assert.That(contact.x, Is.EqualTo(support.localPosition.x).Within(.001f));
        Assert.That(contact.y, Is.EqualTo(.35f).Within(.001f));
        Assert.That(support.localPosition.x, Is.LessThan(0), "High MA support must move toward the left load.");
    }
}
