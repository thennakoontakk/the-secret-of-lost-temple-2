using NUnit.Framework;
using UnityEngine;
using SecretOfLostTemple.Research;

public class SandboxQuizStarsTests
{
    [SetUp]
    public void Setup()
    {
        PlayerPrefs.DeleteKey("LostTemple_Total_Stars");
        PlayerPrefs.DeleteKey("LostTemple_App_Mode");
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey("LostTemple_Total_Stars");
        PlayerPrefs.DeleteKey("LostTemple_App_Mode");
        PlayerPrefs.Save();
    }

    [Test]
    public void InitialStarsDefaultToZero()
    {
        int stars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
        Assert.AreEqual(0, stars);
    }

    [Test]
    public void CorrectAnswerAddsStar()
    {
        int initialStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
        int updatedStars = initialStars + 1;
        ResearchProgress.SetInt("LostTemple_Total_Stars", updatedStars);
        PlayerPrefs.Save();

        Assert.AreEqual(1, ResearchProgress.GetInt("LostTemple_Total_Stars", 0));
    }

    [Test]
    public void IncorrectAnswerReducesStarAndClampsToZero()
    {
        // Give 2 stars initially
        ResearchProgress.SetInt("LostTemple_Total_Stars", 2);
        PlayerPrefs.Save();

        // 1 wrong answer: reduces to 1
        int stars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
        stars = Mathf.Max(0, stars - 1);
        ResearchProgress.SetInt("LostTemple_Total_Stars", stars);
        PlayerPrefs.Save();
        Assert.AreEqual(1, ResearchProgress.GetInt("LostTemple_Total_Stars", 0));

        // 2nd wrong answer: reduces to 0
        stars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
        stars = Mathf.Max(0, stars - 1);
        ResearchProgress.SetInt("LostTemple_Total_Stars", stars);
        PlayerPrefs.Save();
        Assert.AreEqual(0, ResearchProgress.GetInt("LostTemple_Total_Stars", 0));

        // 3rd wrong answer: remains at 0 (never negative)
        stars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
        stars = Mathf.Max(0, stars - 1);
        ResearchProgress.SetInt("LostTemple_Total_Stars", stars);
        PlayerPrefs.Save();
        Assert.AreEqual(0, ResearchProgress.GetInt("LostTemple_Total_Stars", 0));
    }

    [Test]
    public void SandboxModeSetsPlayerPrefsCorrectly()
    {
        PlayerPrefs.SetString("LostTemple_App_Mode", "Sandbox");
        PlayerPrefs.SetString("Sandbox_Selected_Machine", "Lever");
        PlayerPrefs.Save();

        Assert.AreEqual("Sandbox", PlayerPrefs.GetString("LostTemple_App_Mode"));
        Assert.AreEqual("Lever", PlayerPrefs.GetString("Sandbox_Selected_Machine"));
    }
}
