// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using NUnit.Framework;

public class SelectionManagerTest
{
    private StoryObject story;

    /// <summary>
    /// Set up the game so that each test starts at the NPCSelectScene with the chosen story.
    /// </summary>
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Load StartScreenScene in order to put the SettingsManager into DDOL
        SceneManager.LoadScene("StartScreenScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StartScreenScene").isLoaded);

        // Load the "Loading" scene in order to get access to the toolbox in DDOL
        SceneManager.LoadScene("Loading");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("Loading").isLoaded);

        // Get a StoryObject.
        StoryObject[] stories = Resources.LoadAll<StoryObject>("Stories");
        story = stories[0];

        GameManager.gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Start the game with the chosen story.
        GameManager.gm.StartGame(null, story);

        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded); // Wait for scene to load.
    }

    /// <summary>
    /// Move the toolbox under loading as a child, then remove all scenes. This ensures that the toolbox
    /// gets removed before a new test starts.
    ///
    /// PostCondition: Loading scene without any gameobjects is loaded
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), SceneManager.GetSceneByName("NPCSelectScene"));
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneByName("NPCSelectScene"));
        SceneController.sc.UnloadAdditiveScenes();
    }

    /// <summary>
    /// Test if gamestate is set correctly
    /// </summary>
    [UnityTest]
    public IEnumerator GameStateTest()
    {
        Assert.AreEqual(GameManager.GameState.NpcSelect, GameManager.gm.gameState);
        yield return null;
    }

    /// <summary>
    /// Test if enough options are generated.
    /// </summary>
    [UnityTest]
    public IEnumerator OptionsTest()
    {
        GameObject parent = GameObject.Find("Layout");
        int numOfCharacters = GameManager.gm.currentCharacters.Count;

        int c = 0;
        for (int i = 0; i < 8;  i++)
        {
            if (parent.transform.GetChild(i).childCount > 0) c++;
        }

        Assert.AreEqual(numOfCharacters, c);

        yield return null;
    }
}
