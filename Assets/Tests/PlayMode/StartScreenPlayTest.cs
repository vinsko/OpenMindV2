// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class StartScreenPlayTest
{
    private StartMenuManager sm;
    
    #region Setup and TearDown
    
    [UnitySetUp]
    public IEnumerator Setup()
    {
        SceneManager.LoadScene("StartScreenScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StartScreenScene").isLoaded);

        sm = GameObject.Find("StartMenuManager").GetComponent<StartMenuManager>();
    }
    
    [TearDown]
    public void TearDown()
    {
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneAt(0));
        GameObject.Destroy(GameObject.Find("DDOLs"));
    }

    #endregion
    
    /// <summary>
    /// Checks if the scene is set up correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator StartScreenStartTest()
    {
        // Checks to see if the right buttons are active from the start
        Assert.IsTrue(GameObject.Find("MainMenuOptions").activeSelf);
        Assert.IsTrue(GameObject.Find("NewGameButton").activeSelf);
        // Checks to see if the right buttons are inactive from the start
        Assert.IsTrue(GameObject.Find("SkipPrologueWindow") == null);
        yield return null;
    }

    /// <summary>
    /// Checks if the skipprologue prompt is set up correctly
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator SkipPrologueTest()
    {
        sm.StartPrologueOrPrompt();
        // Checks to see if the right buttons are active
        Assert.IsTrue(GameObject.Find("SkipPrologueWindow").activeSelf);
        // Checks to see if the right buttons ar inactive
        Assert.IsTrue(GameObject.Find("MainMenuOptions") == null);
        yield return null;
    }

    /// <summary>
    /// Checks if the prologue is loaded correctly
    /// </summary>
    /// <returns></returns>
    [UnityTest, Order(1)]
    public IEnumerator PrologueTest()
    {
        sm.StartPrologue();
        yield return new WaitUntil(() => SceneManager.GetSceneByName("PrologueScene").isLoaded);
        var s = SceneManager.GetActiveScene();
        Assert.IsTrue(s.name == "PrologueScene");
        
        yield return null;
    }
    
    /// <summary>
    /// Checks if storyscene is loaded properly
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator ChooseStoryTest()
    {
        sm.SkipPrologue();
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StorySelectScene").isLoaded);
        var s = SceneManager.GetActiveScene();
        Assert.IsTrue(s.name == "StorySelectScene");
        
        yield return null;
    }
}
