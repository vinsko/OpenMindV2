// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;

public class PrologueManagerPlayTest
{
    private PrologueManager   pm; 
    
    #region Setup and TearDown
    [UnitySetUp]
    public IEnumerator Setup()
    {
        // Load startscreen in order to properly set up the game. 
        SceneManager.LoadScene("StartScreenScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StartScreenScene").isLoaded);
        
        
        // Load prologue
        SceneManager.LoadScene("PrologueScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("PrologueScene").isLoaded);
        
        pm = GameObject.Find("PrologueManager").GetComponent<PrologueManager>();
    }
    
    [TearDown]
    public void TearDown()
    {
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneByName("PrologueScene"));
        GameObject.Destroy(GameObject.Find("DDOLs"));
        GameObject.Destroy(GameObject.Find("PrologueManager"));
    }

    #endregion
    
    /// <summary>
    /// Checks some basic properties of the prologue. 
    /// </summary>
    [UnityTest]
    public IEnumerator IntroductionSetUpTest()
    {
        Assert.IsNotNull(pm.prologueTimeline);
        // Check if the timeline has started from the beginning. Since it's is already playing it will be bigger than 0. 
        Assert.IsTrue(pm.prologueTimeline.time < 1); 
        Assert.AreEqual(PlayState.Playing, pm.prologueTimeline.state);
        yield return null;
    }
    
    /// <summary>
    /// Checks that prologue is paused when the pause method is called.  
    /// </summary>
    [UnityTest]
    public IEnumerator PausePrologueTest()
    {
        pm.PauseTimeline();
        Assert.AreEqual(pm.prologueTimeline.state, PlayState.Paused);
        yield return null; 
    }
    
    /// <summary>
    /// Checks that prologue starts playing again when it is paused and the continue method is called.  
    /// </summary>
    [UnityTest]
    public IEnumerator ContinuePrologueTest()
    {
        pm.PauseTimeline();
        Assert.AreEqual(pm.prologueTimeline.state, PlayState.Paused);
        pm.ContinueTimeline();
        Assert.AreEqual(pm.prologueTimeline.state, PlayState.Playing);
        yield return null;
    }
    
    /// <summary>
    /// Checks that prologue is paused when the dialog is shown. Otherwise player can't read the dialogue.   
    /// </summary>
    [UnityTest]
    public IEnumerator ActivateDialogTest()
    {
        pm.ActivateDialog();
        Assert.AreEqual(pm.prologueTimeline.state, PlayState.Paused);
        yield return null;
    }
    
}
