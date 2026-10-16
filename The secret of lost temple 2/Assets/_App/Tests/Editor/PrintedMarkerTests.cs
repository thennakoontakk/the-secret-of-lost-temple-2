using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.XR.ARSubsystems;

public class PrintedMarkerTests
{
    [Test]
    public void LeverReferenceUsesPrintedArtworkWithoutAssumingPrintSize()
    {
        var library = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(
            "Assets/XR/ReferenceImageLibraries/SimpleMachinesReferenceImageLibrary.asset");
        Assert.IsNotNull(library);
        var lever = Enumerable.Range(0, library.count).Select(i => library[i])
            .Single(image => image.name == "lever_target");
        Assert.AreEqual(AssetDatabase.AssetPathToGUID("Assets/Art/Targets/lever_printed_target.png"),
            lever.textureGuid.ToString("N"));
        Assert.IsFalse(lever.specifySize);
    }
}
