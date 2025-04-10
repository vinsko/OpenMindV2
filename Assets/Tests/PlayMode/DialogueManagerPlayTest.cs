// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WaitUntil = UnityEngine.WaitUntil;

public class DialogueManagerPlayTest
{
    private GameManager       gm;
    private DialogueManager   dm;
    private StoryObject       story;
    private CharacterInstance character;
    
    #region Setup
    
    /// <summary>
    /// Set up the game so that each test starts at the DialogueScene, talking to an NPC
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
        
        // Set global variables
        story = Resources.LoadAll<StoryObject>("Stories")[0];
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        // Start NPC select scene
        gm.StartGame(null, story);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
        
        // Start dialogue scene
        character = gm.currentCharacters[0];
        gm.StartDialogue(character);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("DialogueScene").isLoaded);

        // Wait for the DialogueManager to appear.
        yield return new WaitUntil(() => GameObject.Find("DialogueManager") != null);
        
        // Set global variable
        dm = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        
        // Complete the greeting.
        while (!SceneManager.GetSceneByName("NPCSelectScene").isLoaded)
        {
            dm.OnDialogueComplete();
            yield return null;
        }
        
        // Start dialogue scene.
        gm.StartDialogue(character);
        
        yield return new WaitUntil(() => SceneManager.GetSceneByName("DialogueScene").isLoaded);

        // Wait for the DialogueManager to appear.
        yield return new WaitUntil(() => GameObject.Find("DialogueManager") != null);
        
        // Set global variable
        dm = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        
    }

    /// <summary>
    /// Move the toolbox under loading as a child, then remove all scenes. This ensures that the toolbox
    /// gets removed before a new test starts.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Move toolbox and DDOLs to Loading to unload
        SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), SceneManager.GetSceneAt(1));
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneAt(1));

        SceneController.sc.UnloadAdditiveScenes();
    }
    
    #endregion
    
    /// <summary>
    /// Tests if responses get correctly loaded after dialogue has been completed.
    /// </summary>
    [UnityTest]
    public IEnumerator OnDialogueCompleteTest()
    {
        // Simulate that dialogue gets completed.
        dm.OnDialogueComplete();
        
        // There should be at least one response.
        Assert.IsTrue(dm.currentObject.Responses.Count > 0);
        yield return null;
    }

    /// <summary>
    /// Tests if the WriteDialogue method works correctly, so if the dialogueField is activated.
    /// </summary>
    [UnityTest]
    public IEnumerator WriteDialogueTest()
    {
        // Find dialogueField and see if it is currently active.
        var dialogueField = GameObject.Find("Dialogue Field");
        Assert.IsTrue(dialogueField.activeSelf);
        
        yield return null;
    }
    
    /// <summary>
    /// Check if the ReplaceBackground method works as intended.
    /// </summary>
    /*[UnityTest]
    public IEnumerator ReplaceBackgroundTest()
    {
        // Get current background.
        var backgroundField = new [] { GameObject.Find("BackgroundField") };

        // Replace the background.
        dm.ReplaceBackground(backgroundField);

        // Check if the background has changed.
        // TODO: change background names / tags so that we can easily check whether or not the background is actually being replaced
        Assert.IsTrue(GameObject.Find("BackgroundField").transform.childCount > 0); // BackgroundField should have the new background as a child

        yield return null;
    }*/

    /// <summary>
    /// Tests if the buttons to ask questions get correctly loaded in after completing the dialogue.
    /// </summary>
    [UnityTest]
    public IEnumerator PromptButtonsTest()
    {
        // Complete the dialogue.
        dm.OnDialogueComplete();

        // Get number of question and back buttons.
        int numQuestionButtons = GameObject.FindGameObjectsWithTag("Button").Where(b => b.name == "questionButton").Count();
        int numBackButtons = GameObject.FindGameObjectsWithTag("Button").Where(b => b.name == "backButton").Count();

        // There should be more than zero question buttons, while there should only be one back button.
        Assert.IsTrue(numQuestionButtons > 0);
        Assert.IsTrue(numBackButtons == 1);

        // The questions field should be active
        var questionsField = GameObject.Find("Questions Field");
        Assert.IsTrue(questionsField.activeSelf);
        
        yield return null;
    }

    /// <summary>
    /// Tests if buttons get destroyed if a question button gets clicked on, and if the questionsField gets set to non-active
    /// </summary>
    [UnityTest]
    public IEnumerator OnButtonClickTest()
    {
        // Complete dialogue
        dm.OnDialogueComplete();
        
        // Find question button and invoke it
        Button button = GameObject.Find("questionButton").GetComponent<Button>();
        button.onClick.Invoke();

        // Find number of buttons in scene
        int buttons = GameObject.FindGameObjectsWithTag("Button").Count();

        // Find questions field
        var questionsField = GameObject.Find("Questions Field");

        // There should be no buttons left, and questionsField should be null
        Assert.IsTrue(buttons == 0);
        Assert.IsNull(questionsField);

        yield return null;
    }
    
    // TODO: CreateOpenQuestion test? Depends on epilogue --> should fix that first
    
    // TODO: AnswerOpenQuestion test? Depends on epilogue --> should fix that first

    /// <summary>
    /// Check if the back button works as intended when there are enough characters and questions left.
    /// </summary>
    [UnityTest]
    public IEnumerator BackButtonTest()
    {
        // Complete the dialogue and move to the BackButton screen.
        while (GameObject.Find("backButton") == null)
        {
            dm.OnDialogueComplete();
            yield return null;
        }
        
        // Check if we are currently in the gameState NpcDialogue
        Assert.AreEqual(GameManager.GameState.NpcDialogue, gm.gameState);
        bool inDialogueScene = SceneManager.GetSceneByName("DialogueScene").isLoaded;
        Assert.IsTrue(inDialogueScene);
        
        // Return to the NpcSelect scene by pressing the BackButton.
        Button backButton = GameObject.Find("backButton").GetComponent<Button>();
        backButton.onClick.Invoke();
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
        
        // Check if we are currently in the gameState NpcDialogue
        Assert.AreEqual(GameManager.GameState.NpcDialogue, gm.gameState);
        bool inNpcSelectScene = SceneManager.GetSceneByName("NPCSelectScene").isLoaded;
        Assert.IsTrue(inNpcSelectScene);

        yield return null;
    }
    
    /// <summary>
    /// Test whether the text scales correctly based on the textSize from the SettingsManager.
    /// </summary>
    /// <returns></returns>
    /*[UnityTest]
    public IEnumerator ChangeTextSizeTest()
    {
        // Set the textSize to small.
        SettingsManager.sm.textSize = SettingsManager.TextSize.Small;
        int fontSizePrior = SettingsManager.sm.GetFontSize();
        
        // Change the text size of the components.
        dm.ChangeTextSize();
        
        // Find the objects that contain tmp_text component.
        GameObject characterNameField = GameObject.Find("Character Name Field");
        TMP_Text dialogueText = GameObject.Find("Text (TMP)").GetComponent<TMP_Text>();
        float fontSizeMaxOld = dialogueText.fontSizeMax;
        
        // Set the fontSizes to small
        characterNameField.GetComponentInChildren<TMP_Text>().fontSize = fontSizePrior;
        dialogueText.fontSize = fontSizePrior;
        
        // Set the textSize to medium
        SettingsManager.sm.textSize = SettingsManager.TextSize.Medium;
        
        // Change the text size of the components.
        dm.ChangeTextSize();
        
        // Search for the components.
        characterNameField = GameObject.Find("Character Name Field");
        dialogueText = GameObject.Find("Text (TMP)").GetComponent<TMP_Text>();
        
        // Check if the fontSizes are bigger than before.
        //Assert.Greater(characterNameField.GetComponentInChildren<TMP_Text>().fontSize, fontSizePrior);
        Assert.Greater(dialogueText.fontSizeMax, fontSizeMaxOld);
        
        yield return null;
    }*/
    
    /// <summary>
    /// Test whether the text scales correctly when the TextSize is changed in the SettingsManager.
    /// </summary>
    /// <returns></returns>
    /*[UnityTest]
    public IEnumerator OnChangedTextSizeTest()
    {
        // Set the textSize to small.
        SettingsManager.sm.textSize = SettingsManager.TextSize.Small;
        int fontSizePrior = SettingsManager.sm.GetFontSize();
        
        // Find the objects that contain tmp_text component.
        GameObject characterNameField = GameObject.Find("Character Name Field");
        TMP_Text dialogueText = GameObject.Find("Text (TMP)").GetComponent<TMP_Text>();
        
        // Set the fontSizes to small
        characterNameField.GetComponentInChildren<TMP_Text>().fontSize = fontSizePrior;
        dialogueText.fontSize = fontSizePrior;
        
        // Set the textSize to medium
        SettingsManager.sm.textSize = SettingsManager.TextSize.Medium;
        
        // Change the text size of the components.
        dm.OnChangedTextSize(null, SettingsManager.sm.GetFontSize());
        
        // Search for the components.
        characterNameField = GameObject.Find("Character Name Field");
        dialogueText = GameObject.Find("Text (TMP)").GetComponent<TMP_Text>();
        
        // Check if the fontSizes are bigger than before.
        Assert.Greater(characterNameField.GetComponentInChildren<TMP_Text>().fontSize, fontSizePrior);
        Assert.Greater(dialogueText.fontSize, fontSizePrior);
        
        yield return null;
    }*/
}