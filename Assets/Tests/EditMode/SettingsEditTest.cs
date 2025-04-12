using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SettingsEditTest
{
    SettingsManager sm;

    [OneTimeSetUp]
    public void Setup()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Menus/StartScreenScene.unity");
        sm = GameObject.FindObjectOfType<SettingsManager>(true);
    }

    /// <summary>
    /// Test if setting the talking speed slider results in the correct talking delay.
    /// </summary>
    /// <param name="multiplier">The value by which the default talking delay will be multiplied</param>
    [Test]
    [TestCase(1)]
    [TestCase(3)]
    [TestCase(0.1f)]
    [TestCase(0.5f)]
    [TestCase(1.5f)]
    public void SetTalkingSpeedTest(float multiplier)
    {
        sm.SetTalkingSpeed(multiplier);
        Assert.AreEqual(0.05f / multiplier, sm.TalkingDelay);
    }

    // TODO: Make it so that the audio settings are 0-100, where 0 means there is no sound at all
}
