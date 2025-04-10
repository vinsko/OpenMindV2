// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

public class MultiplayerMenuPlayTests : MonoBehaviour
{
    private MultiplayerMenuManager mm;
    
    #region Setup and Teardown

    [UnitySetUp]
    public IEnumerator Setup()
    {
        SceneManager.LoadScene("StartScreenScene");
        SceneManager.LoadScene("MultiplayerScreenScene");
        yield return new WaitUntil(() =>
            SceneManager.GetSceneByName("MultiplayerScreenScene").isLoaded);

        mm = GameObject.Find("MultiplayerMenuManager").GetComponent<MultiplayerMenuManager>();
    }
    
    [TearDown]
    public void Teardown()
    {
        SceneManager.MoveGameObjectToScene(GameObject.Find("MultiplayerManager"), 
            SceneManager.GetSceneAt(0));
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), 
            SceneManager.GetSceneAt(0));
        
        Destroy(GameObject.Find("MultiplayerManager"));
        Destroy(GameObject.Find("Canvas"));
        Destroy(GameObject.Find("MultiplayerMenuManager"));
        Destroy(GameObject.Find("DDOLs"));
        
        if (SceneManager.GetSceneByName("SettingsScene").isLoaded)
            SceneManager.UnloadSceneAsync("SettingsScene");
    }
    
    #endregion


    /// <summary>
    /// Tests if the menu starts up correctly.
    /// </summary>
    [Test]
    public void MultiplayerStartTest()
    {
        // check if the correct canvas is active
        Assert.IsTrue(GameObject.Find("MultiplayerMenuOptions").activeSelf);
        Assert.IsNull(GameObject.Find("HostMenuOptions"));
        Assert.IsNull(GameObject.Find("JoinMenuOptions"));
        Assert.IsNull(GameObject.Find("ChooseStory"));
    }

    /// <summary>
    /// tests if the host menu is loaded correctly.
    /// </summary>
    [Test]
    public void HostGameTest()
    {
        mm.OpenHostMenu();
        // check if the correct canvas is active
        Assert.IsNull(GameObject.Find("MultiplayerMenuOptions"));
        Assert.IsTrue(GameObject.Find("HostMenuOptions").activeSelf);
        Assert.IsNull(GameObject.Find("JoinMenuOptions"));
        Assert.IsNull(GameObject.Find("ChooseStory"));
    }
    
    /// <summary>
    /// tests if the join menu is loaded correctly
    /// </summary>
    [Test]
    public void JoinGameTest()
    {
        mm.OpenJoinMenu();
        // check if the correct canvas is active
        Assert.IsNull(GameObject.Find("MultiplayerMenuOptions"));
        Assert.IsNull(GameObject.Find("HostMenuOptions"));
        Assert.IsTrue(GameObject.Find("JoinMenuOptions").activeSelf);
        Assert.IsNull(GameObject.Find("ChooseStory"));
    }
    
    /// <summary>
    /// Tests if the choose story menu is loaded correctly.
    /// </summary>
    [Test]
    public void ChooseStoryTest()
    {
        mm.OpenHostMenu();
        mm.CreateAsHost();
        // check if the correct canvas is active
        Assert.IsNull(GameObject.Find("MultiplayerMenuOptions"));
        Assert.IsNull(GameObject.Find("HostMenuOptions"));
        Assert.IsNull(GameObject.Find("JoinMenuOptions"));
        Assert.IsTrue(GameObject.Find("ChooseStory").activeSelf);
    }

    /// <summary>
    /// Checks if the settings are loaded correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator SettingsTest()
    {
        mm.OpenSettings();
        yield return new WaitUntil(() => SceneManager.GetSceneByName("SettingsScene").isLoaded);
        yield return null;
        
        Assert.IsTrue(SceneManager.GetSceneByName("SettingsScene").isLoaded);
        yield return null;
    }
    
}
