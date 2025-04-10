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

public class GameManagerPlayTest
{
    private StoryObject story;
    private GameManager gm;
    
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
        story = stories[1];
        
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        // Start the game with the chosen story.
        gm.StartGame(null, story);
        
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded); // Wait for scene to load.
    }
    
    /// <summary>
    /// Move the toolbox under loading as a child, then remove all scenes. This ensures that the toolbox
    /// gets removed before a new test starts.
    /// </summary>
    [TearDown]
    public void TearDown()
    {

        // Move toolbox and DDOLs to Loading to unload
        if (GameObject.Find("Toolbox") != null)
            SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), SceneManager.GetSceneAt(1));
        
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneAt(1));

        SceneController.sc.UnloadAdditiveScenes();
    }
    
    // Input parameters for testing different inputs.
    static bool[] bools = new bool[] { true, false };
    static int[]  ints  = new int[] { 0, 1, 2 };
    
    /// <summary>
    /// Checks if the character list gets populated.
    /// </summary>
    [UnityTest]
    public IEnumerator PopulateCharactersTest()
    {
        // Set up expected and actual values.
        int expected = gm.currentCharacters.Count;
        // The number of characters at the start of the game.
        int actual = 8;

        // Check if they are equal.
        Assert.AreEqual(expected, actual);
        
        yield return null;
    }
    
    /// <summary>
    /// Checks if the characters all get set to active during populating.
    /// </summary>
    [UnityTest]
    public IEnumerator ActiveCharactersTest()
    {
        // Set up expected and actual values.
        int expected = gm.currentCharacters.Count(c => c.isActive);
        // The number of characters at the start of the game.
        int actual = 8;
        
        // Check if they are equal.
        Assert.AreEqual(expected, actual);
        
        yield return null;
    }
    
    /// <summary>
    /// Checks if one culprit gets chosen during populating.
    /// </summary>
    [UnityTest]
    public IEnumerator ChooseCulpritTest()
    {
        // Set up expected and actual values.
        int expected = gm.currentCharacters.Count(c => c.isCulprit);
        int actual = 1;

        // Check if they are equal.
        Assert.AreEqual(expected, actual);
        
        yield return null;
    }
    
    /// <summary>
    /// Checks if HasQuestionsLeft returns true when numQuestionsAsked is smaller than numQuestions,
    /// or false when numQuestionsAsked is greater than or equal to numQuestions.
    /// </summary>
    [UnityTest]
    public IEnumerator HasQuestionsLeftTest([ValueSource(nameof(ints))] int values)
    {
        gm.numQuestionsAsked = values;
        gm.story.numQuestions = 1;
        if (values < gm.story.numQuestions)
        {
            // Check if HasQuestionsLeft is true when numQuestionsAsked is smaller than numQuestions.
            Assert.IsTrue(gm.HasQuestionsLeft());
        }
        else
        {
            // Check if HasQuestionsLeft returns false when numQuestionsAsked is greater than or equal to numQuestions.
            Assert.IsFalse(gm.HasQuestionsLeft());
        }
        
        yield return null;
    }

    /// <summary>
    /// Checks if the "GetCulpritTest" returns the culprit.
    /// </summary>
    [UnityTest]
    public IEnumerator GetCulpritTest()
    {
        // Set up expected and actual values.
        CharacterInstance expected = gm.currentCharacters.Find(c => c.isCulprit);
        CharacterInstance actual = gm.GetCulprit();

        // Check if they are equal.
        Assert.AreEqual(expected, actual);
        
        yield return null;
    }

    /// <summary>
    /// Checks if the "EnoughCharacters" returns true when there are enough characters remaining, else false.
    /// </summary>
    [UnityTest]
    public IEnumerator EnoughCharactersTest([ValueSource(nameof(bools))] bool enoughCharacters)
    {
        if (enoughCharacters)
        {
            bool hasEnoughCharacters = gm.currentCharacters.Count(c => c.isActive) > 2;
            // Check if when there are enough characters, EnoughCharactersRemaining returns true.
            Assert.IsTrue(hasEnoughCharacters);
            Assert.IsTrue(gm.EnoughCharactersRemaining());
        }
        else
        {
            // Keep removing 1 character which is not the culprit, until there are not enough characters remaining.
            while (gm.currentCharacters.Count(c => c.isActive) > 2)
            {
                // Set this bool to true once a character has been removed.
                bool removedCharacter = false;
                foreach (CharacterInstance c in gm.currentCharacters)
                {
                    // Set a character to not active if it is not a culprit, is active and the bool removedCharacter is false.
                    if (!c.isCulprit && c.isActive && !removedCharacter)
                    {
                        c.isActive = false;
                        removedCharacter = true;
                    }
                }
            }
            // Check if EnoughCharactersRemaining returns false.
            Assert.IsFalse(gm.EnoughCharactersRemaining());
        }
        
        yield return null;
    }
    
    /// <summary>
    /// Checks if the "GetRandomVictimNoCulprit" returns a CharacterInstance that is not the culprit.
    /// </summary>
    [UnityTest]
    public IEnumerator ChooseVictimTest()
    {
        // Get victim.
        CharacterInstance victim = gm.GetRandomVictimNoCulprit();

        // Check if it actually returned a victim.
        Assert.IsTrue(victim != null);
        
        yield return null;
    }
    
    /// <summary>
    /// Checks if the "EndCycle" method sets 1 character to inactive if there are enough characters remaining,
    /// else check if no characters get set to inactive.
    /// </summary>
    [UnityTest]
    public IEnumerator EndCycleTest([ValueSource(nameof(bools))] bool enoughCharacters)
    {
        // If we do not want to have enough characters.
        if (!enoughCharacters)
        {
            // Keep removing 1 character which is not the culprit, until there are not enough characters remaining.
            while (gm.EnoughCharactersRemaining())
            {
                // Set this bool to true once a character has been removed.
                bool removedCharacter = false;
                foreach (CharacterInstance c in gm.currentCharacters)
                {
                    // Set a character to not active if it is not a culprit, is active and the bool removedCharacter is false.
                    if (!c.isCulprit && c.isActive && !removedCharacter)
                    {
                        c.isActive = false;
                        removedCharacter = true;
                    }
                }
            }
        }
        
        // Start the game cycle.
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded); // Wait for scene to load.
        
        // Variable which counts the number of characters before calling EndCycle.
        int nCharactersPrior = gm.currentCharacters.Count(c => c.isActive);
        
        // Set this to maxValue in order to make sure that no more questions can be asked.
        // This will cause the EndCycle method to be called once the dialogue ends.
        gm.numQuestionsAsked = int.MaxValue;
        
        // Start dialogue with a character, then go back to NpcSelect scene in order to apply the changes of the variables.
        CharacterInstance character = gm.currentCharacters[0];
        gm.StartDialogue(character);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("DialogueScene").isLoaded); // Wait for scene to load.

        // Waiting for the DialogueManager to appear.
        yield return new WaitUntil(() => GameObject.Find("DialogueManager") != null);
        
        // Get the DialogueManager.
        var dm = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();

        // End the NpcDialogue.
        dm.currentObject = new TerminateDialogueObject();
        dm.currentObject.Execute();
        
        // Variable which counts the number of characters after calling EndCycle.
        int nCharactersPosterior = gm.currentCharacters.Count(c => c.isActive);
        
        // We test whether a character disappears when EndCycle is called and there are enough characters.
        // If there are not enough characters, then we test if we transition to the culpritSelect gameState
        // and if a character does not disappear.
        if (enoughCharacters)
        {
            // Get the DialogueManager.
            dm = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        
            // End the HintDialogue.
            dm.currentObject = new TerminateDialogueObject();
            dm.currentObject.Execute();
            
            yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded); // Wait for scene to load.
            
            // Check if only 1 character has disappeared.
            Assert.AreEqual(nCharactersPrior - 1, nCharactersPosterior);
            // Check if we go to the HintDialogue gameState.
            Assert.AreEqual(GameManager.GameState.HintDialogue, gm.gameState);
        }
        else
        {
            // Get the DialogueManager.
            dm = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        
            // End the HintDialogue.
            dm.currentObject = new TerminateDialogueObject();
            dm.currentObject.Execute();
            
            yield return new WaitUntil(() => SceneManager.GetSceneByName("EpilogueScene").isLoaded); // Wait for scene to load.
            
            // Check if no characters have disappeared.
            Assert.AreEqual(nCharactersPrior, nCharactersPosterior);
            // Check if the gameState transitions to culpritSelect.
            Assert.AreEqual(GameManager.GameState.Epilogue, gm.gameState);
        }
        
        yield return null;
    }
    
    /// <summary>
    /// Checks if the "StartDialogue" has the correct gameState (NpcDialogue) and checks if the DialogueScene is loaded.
    /// </summary>
    [UnityTest]
    public IEnumerator StartDialogueTest()
    {
        // Get character to start dialogue with.
        CharacterInstance character = gm.currentCharacters[0];
        gm.StartDialogue(character);

        yield return new WaitUntil(() => SceneManager.GetSceneByName("DialogueScene").isLoaded); // Wait for scene to load.

        // Waiting for the DialogueManager to appear, since waiting for the DialogueScene is not enough.
        yield return new WaitUntil(() => GameObject.Find("DialogueManager") != null);
        
        // Check if the gameState is set to NpcDialogue.
        Assert.AreEqual(GameManager.GameState.NpcDialogue, gm.gameState);
        
        // Check if we are in the DialogueScene.
        bool inDialogueScene = SceneManager.GetSceneByName("DialogueScene").isLoaded;
        Assert.IsTrue(inDialogueScene);
        
        yield return null;
    }
    
    /// <summary>
    /// Check if EndDialogue returns to the NPCSelect state when there are questions left.
    /// </summary>
    [UnityTest]
    public IEnumerator EndDialogueGameCycleTest()
    {
        // Start dialogue with a character, then go back to NpcSelect scene in order to apply the changes of the variables.
        CharacterInstance character = gm.currentCharacters[0];
        gm.StartDialogue(character);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("DialogueScene").isLoaded); // Wait for scene to load.
        
        // Waiting for the DialogueManager to appear.
        yield return new WaitUntil(() => GameObject.Find("DialogueManager") != null);
        
        // Check if there are questions left. 
        Assert.IsTrue(gm.HasQuestionsLeft());
        
        // Get the DialogueManager.
        var dm = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();

        // End the dialogue.
        dm.currentObject = new TerminateDialogueObject();
        dm.currentObject.Execute();
        
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded); // Wait for scene to load.
        
        // Waiting for the DialogueManager to appear.
        yield return new WaitUntil(() => GameObject.Find("SelectionManager") != null);
        
        // Check if the NPCSelectScene is loaded.
        bool inNpcSelectScene = SceneManager.GetSceneByName("NPCSelectScene").isLoaded;
        Assert.IsTrue(inNpcSelectScene);
        
        // Check if we are in the correct gameState.
        Assert.AreEqual(GameManager.GameState.NpcSelect, gm.gameState);
        
        yield return null;
    }
    
    /// <summary>
    /// Check if the transition from NpcSelect to NpcDialogue GameState is done correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator NpcSelectToNpcDialogueGameStateTest()
    {
        // Start the game cycle
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
        
        // Check if we are in the correct gameState.
        Assert.AreEqual(GameManager.GameState.NpcSelect, gm.gameState);
        
        // Start dialogue with a character, then go back to NpcSelect scene in order to apply the changes of the variables.
        CharacterInstance character = gm.currentCharacters[0];
        gm.StartDialogue(character);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("DialogueScene").isLoaded); // Wait for scene to load.

        // Waiting for the DialogueManager to appear.
        yield return new WaitUntil(() => GameObject.Find("DialogueManager") != null);
        
        // Check if we are in the correct gameState.
        Assert.AreEqual(GameManager.GameState.NpcDialogue, gm.gameState);
    }
    
    /// <summary>
    /// Check if the transition from NpcDialogue to NpcSelect GameState is done correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator NpcDialogueToNPCSelectGameStateTest()
    {
        // Start dialogue with a character, then go back to NpcSelect scene in order to apply the changes of the variables.
        CharacterInstance character = gm.currentCharacters[0];
        gm.StartDialogue(character);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("DialogueScene").isLoaded); // Wait for scene to load.
        
        // Waiting for the DialogueManager to appear.
        yield return new WaitUntil(() => GameObject.Find("DialogueManager") != null);
        
        // Get the DialogueManager.
        var dm = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        
        // Check if we are in the correct gameState.
        Assert.AreEqual(GameManager.GameState.NpcDialogue, gm.gameState);

        // End the dialogue.
        dm.currentObject = new TerminateDialogueObject();
        dm.currentObject.Execute();
        
        // Waiting for the NpcSelectScene to appear
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded); // Wait for scene to load.
        // Waiting for the DialogueManager to appear.
        yield return new WaitUntil(() => GameObject.Find("SelectionManager") != null);
        
        // Check if we are in the correct gameState.
        Assert.AreEqual(GameManager.GameState.NpcSelect, gm.gameState);
    }
    
    /// <summary>
    /// Check if the transition from NpcDialogue to HintDialogue GameState is done correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator NpcDialogueToHintDialogueGameStateTest()
    {
        // Set this to maxValue in order to make sure that no more questions can be asked.
        // This will cause the EndCycle method to be called once the dialogue ends.
        gm.numQuestionsAsked = int.MaxValue;
        
        // Start dialogue with a character, then go back to NpcSelect scene in order to apply the changes of the variables.
        CharacterInstance character = gm.currentCharacters[0];
        gm.StartDialogue(character);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("DialogueScene").isLoaded); // Wait for scene to load.

        // Waiting for the DialogueManager to appear.
        yield return new WaitUntil(() => GameObject.Find("DialogueManager") != null);
        
        // Get the DialogueManager.
        var dm = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        
        // Check if we are in the correct gameState.
        Assert.AreEqual(GameManager.GameState.NpcDialogue, gm.gameState);

        // End the NpcDialogue
        dm.currentObject = new TerminateDialogueObject();
        dm.currentObject.Execute();
        
        // Check if we are in the correct gameState.
        Assert.AreEqual(GameManager.GameState.HintDialogue, gm.gameState);
    }
}