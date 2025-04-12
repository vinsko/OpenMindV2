// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManagerPlayTests
{
    private StoryObject story;
    private GameManager gm;
    private GameOverManager gom;
    
    /// <summary>
    /// Set up the game so that each test starts at the GameOverScene.
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
        story = stories[1];
        
        // Get the GameManager.
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Start the game with the chosen story.
        gm.StartGame(null, story);
        
        // Load the GameOverScene.
        SceneController.sc.StartScene(SceneController.SceneName.GameOverScene);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("GameOverScene").isLoaded); // Wait for scene to load.
        
        // Waiting for the GameOverManager to appear.
        yield return new WaitUntil(() => GameObject.Find("GameOverManager") != null);
        
        // Get the GameOverManager.
        gom = GameObject.Find("GameOverManager").GetComponent<GameOverManager>();
        
        // Set the private variables in GameOverManager using this function.
        gom.StartGameOver(gm, false, gm.currentCharacters, gm.GetCulprit().id, story);
        
        // Move toolbox and DDOLs to NPCSelectScene to unload.
        if (GameObject.Find("Toolbox") != null)
            SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), SceneManager.GetSceneByName("NPCSelectScene"));
        
        // Unload the NPCSelectScene.
        SceneManager.UnloadSceneAsync("NPCSelectScene");
    }

    /// <summary>
    /// Move the toolbox under loading as a child, then remove all scenes. This ensures that the toolbox
    /// gets removed before a new test starts.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Move all toolboxes so that they can be unloaded.
        var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Toolbox");
        foreach (GameObject obj in objects)
            SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneAt(1));

        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneAt(1));

        // Unload the additive scenes.
        SceneController.sc.UnloadAdditiveScenes();

    }
    
    /// <summary>
    /// Checks if the "RestartStoryScene" correctly resets the variables.
    /// </summary>
    [UnityTest]
    public IEnumerator RestartStoryTest()
    {
        // Holds the characters prior to calling retry.
        List<CharacterInstance> charactersPrior = new List<CharacterInstance>();
        charactersPrior.AddRange(gm.currentCharacters);
        
        // Restart the game.
        gom.Restart();

        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded); // Wait for scene to load.
        SceneManager.UnloadSceneAsync("Loading");
        
        // Get the new GameManager.
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        // Check if the new list has the same amount of characters as before.
        bool actual = gm.currentCharacters.Count(c => c.isActive) == charactersPrior.Count();
        Assert.IsTrue(actual);
        
        //wait until the scene is loaded, only after will the gamestate be updated
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
        
        // Check if we are in the NpcSelect gameState and if the following 2 scenes exist,
        // namely NpcSelectScene and Loading.
        Assert.AreEqual(GameManager.GameState.NpcSelect, gm.gameState);
        Assert.IsTrue(SceneManager.GetSceneByName("Loading").isLoaded);
        Assert.IsTrue(SceneManager.GetSceneByName("NPCSelectScene").isLoaded);

        yield return null;
    }
    
    /// <summary>
    /// Checks if the "RetryStoryScene" resets all characters to be active.
    /// </summary>
    [UnityTest]
    public IEnumerator RetryStoryTest()
    {
        // Holds the characters prior to calling retry.
        List<CharacterInstance> charactersPrior = new List<CharacterInstance>();
        charactersPrior.AddRange(gm.currentCharacters);
        
        // Retry the game.
        gom.Retry();
        
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded); // Wait for scene to load.
        SceneManager.UnloadSceneAsync("Loading");
        
        // Get the new GameManager.
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        // Check if the new list has the same amount of characters as before.
        bool actual = gm.currentCharacters.Count(c => c.isActive) == charactersPrior.Count();
        Assert.IsTrue(actual);
        
        bool hasSameCharacters = true;
        // Check if all characters are active and if the same characters are used.
        foreach (CharacterInstance character in gm.currentCharacters)
        {
            if (charactersPrior.All(c => c.characterName != character.characterName))
                hasSameCharacters = false;
        }

        // Check if the bool hasSameCharacters returns true.
        Assert.IsTrue(hasSameCharacters);

        //wait until the scene is loaded, only after will the gamestate be updated
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
        
        // Check if we are in the NpcSelect gameState and if the following 2 scenes exist,
        // namely NpcSelectScene and Loading.
        Assert.AreEqual(GameManager.GameState.NpcSelect, gm.gameState);
        Assert.IsTrue(SceneManager.GetSceneByName("Loading").isLoaded);
        Assert.IsTrue(SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
    }
}