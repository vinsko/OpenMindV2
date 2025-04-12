// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using UnityEditor;
using UnityEngine.UI;
using Scene = UnityEngine.SceneManagement.Scene;

public class SystemTests
{
    private List<bool> greeted = new();
    
    #region start
    //clicking new game at the start of the game
    private const string Start_NewButtonName              = "NewGameButton";
    //clicking new game at the start of the game
    private const string Start_ContinueButtonName         = "ContinueButton";
    //saying yes to view the prologue
    private const string Start_YesPrologueButtonName      = "YesButton";
    private const string Start_PrologueSceneName          = "PrologueScene";
    //clicking this button skips dialogue and progresses the prologue
    private const string Start_PrologueContinueButtonName = "Button parent";
    private const string Start_StorySelectSceneName       = "StorySelectScene";
    #endregion
    
    #region story intro
    //button to select story A
    private const string Intro_StoryAButtonName             = "StoryA_Button";
    //button to select story B
    private const string Intro_StoryBButtonName             = "StoryB_Button";
    //button to select story C
    private const string Intro_StoryCButtonName             = "StoryC_Button";
    private const string Intro_IntroStorySceneName          = "IntroStoryScene";
    //clicking this button progresses the intro
    private const string Intro_IntroStoryContinueButtonName = "ContinueButton";
    private const string Intro_NPCSelectSceneName           = "NPCSelectScene";
    #endregion
    
    #region tutorial and menu
    private const string Tutorial_TutorialSceneName      = "TutorialScene";
    //button 1 for continuing the tutorial
    private const string Tutorial_ContinueButton1Name    = "ContinueButton";
    //button 2 for continuing the tutorial
    private const string Tutorial_ContinueButton2Name    = "Notebook Button";
    //the menu button in npcSelect
    private const string Tutorial_MenuButtonName         = "Menu Button";
    private const string Tutorial_GameMenuSceneName      = "GameMenuScene";
    //the button to save the game
    private const string Tutorial_SaveGameButtonName     = "SaveButton";
    //closes the menu
    private const string Tutorial_ReturnToGameButtonName = "ReturnButton";
    #endregion
    
    #region notebook scene constants
    //the name of the gameobject representing character notes
    private const string Notebook_CharacterInputFieldName = "Character Info Field";
    //the name of the gameobject representing personal notes
    private const string Notebook_PersonalInputFieldName  = "Personal Info Field";
    //clicking this button opens the notebook
    private const string Notebook_NotebookButtonName      = "Notebook Button";
    private const string Notebook_NotebookSceneName       = "NotebookScene";
    //clicking this button opens the personal notes
    private const string Notebook_PersonalNotesButtonName = "PersonalButton";
    //gameobject representing the buttons at the top row of the notebook
    private const string Notebook_ButtonsTopRowName       = "Buttons Top Row";
    //gameobject representing the buttons at the bottom row of the notebook
    private const string Notebook_ButtonsBottomRowName    = "Buttons Bottom Row";
    #endregion
    
    #region dialogue and npcSelect
    //The NPCSelect scroller responsible for scrolling in the npcselect screne
    private const string Dialogue_ScrollerObjectName        = "Scroller";
    //the left arrow in npcselect scene
    private const string Dialogue_NavLeftName               = "NavLeft";
    //the right arrow in npcselect scene
    private const string Dialogue_NavRightName              = "NavRight";
    //button in npcSelect confirming to talk to a character
    private const string Dialogue_ConfirmTalkButtonName     = "Confirm Selection Button";
    private const string Dialogue_DialogueSceneName         = "DialogueScene";
    private const string Dialogue_DialogueManagerName       = "DialogueManager";
    //the button to continue talking to a character in dialogue
    private const string Dialogue_ContinueTalkingButtonName = "Skip Dialogue Button";
    //the button to ask a question to a character
    private const string Dialogue_QuestionButtonName        = "questionButton";
    //the gameobject representing the phone in the hint scene of story A
    private const string Dialogue_PhoneFieldName            = "Phone Dialogue Field";
    //the button to continue the hint scene in story A
    private const string Dialogue_PhoneContinueButtonName   = "Next Dialogue Button";
    //the question field which contains all 3 buttons in dialogue
    private const string Dialogue_QuestionFieldName         = "Questions Field";
    #endregion
    
    #region epilogue
    private const string Epilogue_EpilogueSceneName        = "EpilogueScene";
    //the button corresponding to a portrait
    private const string Epilogue_PortraitButtonName       = "Portrait Button(Clone)";
    //the field where the reflection question can be answered
    private const string Epilogue_ReflectionFieldName      = "InputField";
    //the button to submit the reflection question
    private const string Epilogue_SubmitInputButtonName    = "Submit input Button";
    private const string Epilogue_GameOverSceneName        = "GameOverScene";
    #endregion
    
    /// <summary>
    /// Set up for each of the system level tests.
    /// </summary>
    /// <returns></returns>
    [UnitySetUp]
    public IEnumerator Setup()
    {
        // Load the StartScreenScene
        SceneManager.LoadScene("StartScreenScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StartScreenScene").isLoaded);
        
        // Move DebugManager and copyright back to StartScreenScene so that 
        //SceneManager.MoveGameObjectToScene(GameObject.Find("DebugManager"), SceneManager.GetSceneByName("StartScreenScene"));
        //SceneManager.MoveGameObjectToScene(GameObject.Find("Copyright"), SceneManager.GetSceneByName("StartScreenScene"));
                
        yield return null;
    }

    /// <summary>
    /// Tear down for each of the system level tests. Move the toolbox under loading as a child,
    /// then remove all scenes. This ensures that the toolbox gets removed before a new test starts.
    /// </summary>
    
    [TearDown]
    public void TearDown()
    {
        // TODO: perhaps check if there is anything under ddol, then move the objects if so.
        // Move the Toolbox and the SettingsManager to be not in ddol
        if(GameObject.Find("Toolbox"))
            SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), SceneManager.GetSceneAt(0));    
        
        //if(GameObject.Find("SettingsManager"))
        //    SceneManager.MoveGameObjectToScene(GameObject.Find("SettingsManager"), SceneManager.GetSceneAt(0));
        
        // Unload additive scenes.
        if(SceneController.sc != null)
            SceneController.sc.UnloadAdditiveScenes();
        
        greeted.Clear();
        Debug.Log(SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
    }
    
    static int[] stories = new int[] { 0,1, 2 };
    
    [UnityTest, Timeout(100000000)]
    public IEnumerator PlayTheGame([ValueSource(nameof(stories))] int storyID)
    {
        yield return PlayUntilStorySelect();
        yield return SelectStoryAndPlayUntilNPCSelect(storyID);

        // Number of characters in the game
        int numCharacters = GameManager.gm.story.numberOfCharacters;
        
        // Number of characters that are left when you have to choose the culprit
        int charactersLeft = GameManager.gm.story.minimumRemaining;

        // List of characters that have no questions left.
        List<CharacterInstance> emptyQuestionCharacters = new List<CharacterInstance>();
        
        yield return new WaitUntil(() =>
            SceneManager.GetSceneByName(Intro_NPCSelectSceneName).isLoaded);
        yield return PlayTutorial();
        
        // Play the main loop of the game
        for (int i = 0; i <= (numCharacters - charactersLeft); i++)
        {
            yield return new WaitUntil(() =>
                SceneManager.GetSceneByName(Intro_NPCSelectSceneName).isLoaded);
            
            // Check if we are in the NPC Select scene
            Assert.AreEqual(SceneManager.GetSceneByName(Intro_NPCSelectSceneName),
                SceneManager.GetSceneAt(1));

            // Select a npc that has questions left.
            yield return SelectNpc(emptyQuestionCharacters, GameManager.gm.currentCharacters);

            // Check if we are in the Dialogue scene
            Assert.IsTrue(SceneManager.GetSceneByName(Dialogue_DialogueSceneName).isLoaded);

            int numQuestions = GameManager.gm.story.numQuestions;

            yield return FullConversation(numQuestions, emptyQuestionCharacters, GameManager.gm.currentCharacters);
        }
        
        yield return PlayThroughEpilogue();
    }

    /// <summary>
    /// System level test for saving the game.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"> Occurs when there are no active characters in the game. (should never occur)</exception>
    // TODO: check for isLoaded instead of using GetSceneAt() (refactoring).
    [UnityTest, Timeout(100000000), Order(1)]
    public IEnumerator SaveGame()
    {
        yield return PlayUntilStorySelect();
        yield return SelectStoryAndPlayUntilNPCSelect(0);
        yield return PlayTutorial();

        // List of characters that have no questions left.
        List<CharacterInstance> emptyQuestionCharacters = new List<CharacterInstance>();
        
        // Select a npc that has questions left.
        yield return SelectNpc(emptyQuestionCharacters, GameManager.gm.currentCharacters);
        
        yield return FullConversation(GameManager.gm.story.numQuestions, emptyQuestionCharacters, GameManager.gm.currentCharacters);
        
        // Open Notebook
        GameObject.Find(Tutorial_ContinueButton2Name).GetComponent<Button>().onClick.Invoke();

        // Wait until loaded
        yield return new WaitUntil(() => SceneManager.GetSceneByName(Notebook_NotebookSceneName).isLoaded);
                
        // Check if we are in the Notebook scene
        Assert.AreEqual(SceneManager.GetSceneByName(Notebook_NotebookSceneName), SceneManager.GetSceneAt(2));
                
        // Open personal notes
        GameObject.Find(Notebook_PersonalNotesButtonName).GetComponent<Button>().onClick.Invoke();
        

        //write character notes
        string notebookTextPrior = "hello";

        TMP_InputField personalNotes = GameObject.Find(Notebook_PersonalInputFieldName).GetComponentInChildren<TMP_InputField>();
        personalNotes.text = notebookTextPrior;
        foreach (var button in 
                 GameObject.Find(Notebook_ButtonsTopRowName).GetComponentsInChildren<Button>().Union(
                     GameObject.Find(Notebook_ButtonsBottomRowName).GetComponentsInChildren<Button>()))
        {
            if (button.name == Notebook_PersonalNotesButtonName)
                continue;
            
            button.onClick.Invoke();
            yield return null;
            
            yield return new WaitWhile(() => GameObject.Find(Notebook_CharacterInputFieldName) is null);
            yield return new WaitWhile(() =>
                GameObject.Find(Notebook_CharacterInputFieldName).GetComponentInChildren<TMP_InputField>() is null);
            TMP_InputField TMPcharacterNotes = GameObject.Find(Notebook_CharacterInputFieldName).GetComponentInChildren<TMP_InputField>();
            TMPcharacterNotes.text = notebookTextPrior + " " + button.name;
        }
        
        
        // Close notebook
        GameObject.Find(Notebook_NotebookButtonName).GetComponent<Button>().onClick.Invoke();
        yield return new WaitUntil(() =>
            SceneManager.GetSceneByName(Intro_NPCSelectSceneName).isLoaded);
        
        // Check if we are back in the NPC Select scene
        Assert.AreEqual(SceneManager.GetSceneByName(Intro_NPCSelectSceneName), SceneManager.GetSceneAt(1));

        yield return new WaitWhile(() => GameObject.Find(Tutorial_MenuButtonName) is null);

        // Open the menu
        GameObject.Find(Tutorial_MenuButtonName).GetComponent<Button>().onClick.Invoke();
        
        // Wait until loaded
        yield return new WaitUntil(() =>
            SceneManager.GetSceneByName(Tutorial_GameMenuSceneName).isLoaded);
        
        // Save the game
        GameObject.Find(Tutorial_SaveGameButtonName).GetComponent<Button>().onClick.Invoke();
        
        bool saveFileExists = FilePathConstants.DoesSaveFileLocationExist();
        Assert.IsTrue(saveFileExists);
        
        // Retrieve the data from the save file
        SaveData saveData = Load.Loader.GetSaveData();
        
        // Check if the following variables are equal to the saved data
        var gm = GameManager.gm;
        
        // Check if storyID is equal
        Assert.AreEqual(gm.story.storyID, saveData.storyId);
        
        // Check if the activeCharacterIds are equal by checking the following 2 properties:
        // 1: Check if the array of activeCharacterIds has the same length as the array of
        // activeCharacterIds from the saveData.
        // 2: Check if both arrays contain the same elements.
        int[] activeCharIdArray = gm.currentCharacters.FindAll(c => c.isActive).ToArray().Select(c => c.id).ToArray();
        // Check if the arrays have the same length
        Assert.AreEqual(activeCharIdArray.Length, saveData.activeCharacterIds.Length);
        // Check if both arrays contain the same elements
        Assert.IsTrue(activeCharIdArray.All(saveData.activeCharacterIds.Contains));
        
        // Check if the inactiveCharacterIds are equal by checking the following 2 properties:
        // 1: Check if the array of inactiveCharacterIds has the same length as the array of
        // inactiveCharacterIds from the saveData.
        // 2: Check if both arrays contain the same elements.
        int[] inactiveCharIdArray = gm.currentCharacters.FindAll(c => !c.isActive).ToArray().Select(c => c.id).ToArray();
        // Check if the arrays have the same length
        Assert.AreEqual(inactiveCharIdArray.Length, saveData.inactiveCharacterIds.Length);
        // Check if both arrays contain the same elements
        Assert.IsTrue(inactiveCharIdArray.All(saveData.inactiveCharacterIds.Contains));
        
        // Check if the culpritId is equal
        Assert.AreEqual(gm.GetCulprit().id, saveData.culpritId);
        
        // Check if the remainingQuestions are equal by checking the following 2 properties:
        // 1: Check if the array of remainingQuestions has the same length as the array of
        // remainingQuestions from the saveData.
        // 2: Check if both arrays contain the same elements.
        (int, List<Question>)[] remainingQuestionsArray = gm.currentCharacters.Select(a => (a.id, a.RemainingQuestions)).ToArray();
        // Check if the arrays have the same length
        Assert.AreEqual(remainingQuestionsArray.Length, saveData.remainingQuestions.Length);
        // Check if both arrays contain the same elements
        for (int i = 0; i < remainingQuestionsArray.Length; i++)
        {
            // Check if the first elements of the pairs are equal
            Assert.AreEqual(remainingQuestionsArray[i].Item1, saveData.remainingQuestions[i].Item1);
            // Check if the second elements of the pairs (question list) are equal
            Assert.IsTrue(remainingQuestionsArray[i].Item2.All(saveData.remainingQuestions[i].Item2.Contains));
        }
        
        // Check if the personalNotes are equal
        Assert.AreEqual(gm.notebookData.GetPersonalNotes(), saveData.personalNotes);
        
        // Check if the characterNotes are equal by checking the following 2 properties:
        // 1: Check if the array of characterNotes has the same length as the array of
        // characterNotes from the saveData.
        // 2: Check if both arrays contain the same elements.
        (int, string)[] characterNotes = gm.currentCharacters
            .Select(c => (c.id, gm.notebookData.GetCharacterNotes(c))).ToArray();
        // Check if the arrays have the same length
        Assert.AreEqual(characterNotes.Length, saveData.characterNotes.Length);
        // Check if both arrays contain the same elements
        for (int i = 0; i < characterNotes.Length; i++)
        {
            // Check if the first elements of the pairs are equal
            Assert.AreEqual(characterNotes[i].Item1, saveData.characterNotes[i].Item1);
            // Check if the second elements of the pairs are equal
            Assert.AreEqual(characterNotes[i].Item2, saveData.characterNotes[i].Item2);
        }
        
        // Check if the numQuestionsAsked is equal
        Assert.AreEqual(gm.numQuestionsAsked, saveData.numQuestionsAsked);
        
        // Close the menu
        GameObject.Find(Tutorial_ReturnToGameButtonName).GetComponent<Button>().onClick.Invoke();
    }

    /// <summary>
    /// System level test for loading the game.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"> Occurs when no save file exists. </exception>
    /// // TODO: check for isLoaded instead of using GetSceneAt() (refactoring).
    [UnityTest, Timeout(100000000), Order(2)]
    public IEnumerator LoadGame()
    {
        if (!FilePathConstants.DoesSaveFileLocationExist())
            throw new Exception("No save file exists");
        
        // Find the New Game button and click it
        GameObject.Find(Start_ContinueButtonName).GetComponent<Button>().onClick.Invoke();
        
        // Check if we are in the Dialogue scene
        yield return new WaitUntil(() => SceneManager.GetSceneByName(Intro_NPCSelectSceneName).isLoaded);
        
        // Retrieve the data from the save file
        SaveData saveData = Load.Loader.GetSaveData();
        
        // Number of active characters in the game
        int numActiveCharacters = saveData.activeCharacterIds.Length;
        
        // Number of characters that are left when you have to choose the culprit
        int charactersLeft = GameManager.gm.story.minimumRemaining;
        
        // List of characters that have no questions left.
        List<CharacterInstance> emptyQuestionCharacters = new List<CharacterInstance>();
        
        
        // Play the main loop of the game
        for (int i = 0; i <= (numActiveCharacters - charactersLeft); i++)
        {
            yield return new WaitUntil(() => SceneManager.GetSceneByName(Intro_NPCSelectSceneName).isLoaded);
            
            // Check if we are in the NPC Select scene
            Assert.AreEqual(SceneManager.GetSceneByName(Intro_NPCSelectSceneName), SceneManager.GetSceneAt(1));
            
            // Select a npc that has questions left.
            yield return SelectNpc(emptyQuestionCharacters, GameManager.gm.currentCharacters);
            
            yield return FullConversation(GameManager.gm.story.numQuestions,
                emptyQuestionCharacters, GameManager.gm.currentCharacters);
        }

        yield return PlayThroughEpilogue();
    }

    #region playing through the game
    /// <summary>
    /// Plays the game from the start (clicking new story) until the story select screen
    /// </summary>
    private IEnumerator PlayUntilStorySelect()
    {
        // Find the New Game button and click it
        GameObject.Find(Start_NewButtonName).GetComponent<Button>().onClick.Invoke();

        // Choose to view the prologue
        GameObject.Find(Start_YesPrologueButtonName).GetComponent<Button>().onClick.Invoke();
        yield return new WaitUntil(() => SceneManager.GetSceneByName(Start_PrologueSceneName).isLoaded);

        // Check if we are in the prologue
        Assert.AreEqual(SceneManager.GetSceneByName(Start_PrologueSceneName), SceneManager.GetActiveScene());

        yield return PlayThroughScene(Start_PrologueSceneName, new List<string>(){Start_PrologueContinueButtonName});
        
        yield return new WaitUntil(() => SceneManager.GetSceneByName(Start_StorySelectSceneName).isLoaded);
        
        // Check if we are in the StorySelect scene
        Assert.AreEqual(SceneManager.GetSceneByName(Start_StorySelectSceneName), SceneManager.GetActiveScene());
        
    }
    
    /// <summary>
    /// Selects a story and plays through the story intro until npcSelect
    /// </summary>
    private IEnumerator SelectStoryAndPlayUntilNPCSelect(int storyID)
    {
        string buttonName = "";

        switch (storyID)
        {
            case 0:
                buttonName = Intro_StoryAButtonName;
                break;
            case 1:
                buttonName = Intro_StoryBButtonName;
                break;
            case 2:
                buttonName = Intro_StoryCButtonName;
                break;
        }

        // Select story
        GameObject.Find(buttonName).GetComponent<Button>().onClick.Invoke();
        yield return new WaitUntil(() => SceneManager.GetSceneByName(Intro_IntroStorySceneName).isLoaded);
        
        // Check if we are in the story intro
        Assert.AreEqual(SceneManager.GetSceneByName(Intro_IntroStorySceneName), SceneManager.GetActiveScene());
        
        yield return PlayThroughScene(Intro_IntroStorySceneName, new List<string>(){Intro_IntroStoryContinueButtonName});
        
        yield return new WaitUntil(() => SceneManager.GetSceneByName(Intro_NPCSelectSceneName).isLoaded);
        //this frame is required for the scenes to update
        yield return null;
        
        // Check if we are in the NPC Select scene
        Assert.AreEqual(SceneManager.GetSceneByName(Intro_NPCSelectSceneName), SceneManager.GetSceneAt(1));
    }

    private IEnumerator PlayThroughEpilogue()
    {
        yield return null;
        
        // Check if we have to choose a culprit
        Assert.AreEqual(SceneManager.GetSceneByName(Epilogue_EpilogueSceneName), SceneManager.GetSceneAt(1));

        yield return new WaitWhile(() => GameObject.Find(Epilogue_PortraitButtonName) is null);
        yield return new WaitWhile(() => GameObject.Find(Epilogue_PortraitButtonName).GetComponent<Button>() is null);
        // Choose one of the characters
        GameObject.Find(Epilogue_PortraitButtonName).GetComponent<Button>().onClick.Invoke();

        // Check if we are in the Dialogue scene
        yield return new WaitUntil(() => SceneManager.GetSceneByName(Dialogue_DialogueSceneName).isLoaded);
        Assert.AreEqual(SceneManager.GetSceneByName(Dialogue_DialogueSceneName), SceneManager.GetSceneAt(2));
        
        // Skip dialogue until first reflection moment
        while (GameObject.Find(Epilogue_ReflectionFieldName) == null)
        {
            yield return null;
                
            if (GameObject.Find(Dialogue_ContinueTalkingButtonName) != null)
                GameObject.Find(Dialogue_ContinueTalkingButtonName).GetComponent<Button>().onClick.Invoke();
        }

        TMP_InputField answer = GameObject.Find(Epilogue_ReflectionFieldName)
            .GetComponentInChildren<TMP_InputField>();
        answer.text = "hello there";
        // Click on submit input button
        GameObject.Find(Epilogue_SubmitInputButtonName).GetComponent<Button>().onClick.Invoke();
        
        // Skip dialogue until second reflection moment
        while (GameObject.Find(Epilogue_ReflectionFieldName) == null)
        {
            yield return null;
                
            if (GameObject.Find(Dialogue_ContinueTalkingButtonName) != null)
                GameObject.Find(Dialogue_ContinueTalkingButtonName).GetComponent<Button>().onClick.Invoke();
        }
        
        answer = GameObject.Find(Epilogue_ReflectionFieldName)
            .GetComponentInChildren<TMP_InputField>();
        answer.text = "hello there";
        
        // Click on submit input button
        GameObject.Find(Epilogue_SubmitInputButtonName).GetComponent<Button>().onClick.Invoke();
        
        // Skip dialogue until GameOver
        while (SceneManager.GetSceneAt(1) != SceneManager.GetSceneByName(Epilogue_GameOverSceneName))
        {
            yield return null;
                
            if (GameObject.Find(Dialogue_ContinueTalkingButtonName) != null)
                GameObject.Find(Dialogue_ContinueTalkingButtonName).GetComponent<Button>().onClick.Invoke();
        }

        // Check if we are in the GameOver scene
        Assert.AreEqual(SceneManager.GetSceneAt(1), SceneManager.GetSceneByName(Epilogue_GameOverSceneName));
    }

    private IEnumerator PlayTutorial() => PlayThroughScene(Tutorial_TutorialSceneName,
        new List<string>() { Tutorial_ContinueButton1Name, Tutorial_ContinueButton2Name });

    /// <summary>
    /// plays through a scene by clicking the given buttons
    /// </summary>
    private IEnumerator PlayThroughScene(string sceneName, List<string> buttonsToClick)
    {
        while (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            // Wait a second, otherwise the test crashes
            yield return null;

            for (int i = 0; i < buttonsToClick.Count; i++)
                if (GameObject.Find(buttonsToClick[i]) != null)
                {
                    GameObject.Find(buttonsToClick[i]).GetComponent<Button>().onClick.Invoke();
                    break;
                }
        }
    }
    #endregion
    
    #region dialogue
    /// <summary>
    /// Select a npc that has questions left.
    /// </summary>
    /// <param name="emptyQuestionCharacters"> The list of characters that have no questions left. </param>
    /// <param name="currentCharacters"> The list of currentCharacters. </param>
    public IEnumerator SelectNpc(List<CharacterInstance> emptyQuestionCharacters, List<CharacterInstance> currentCharacters)
    {
        //initialise greeted
        if (greeted.Count == 0)
            greeted.AddRange(currentCharacters.Select(_ => false));
        
        float swipeDuration = GameObject.Find(Dialogue_ScrollerObjectName).GetComponent<NPCSelectScroller>()
            .scrollDuration;
        // Start at the leftmost character
        while (GameObject.Find(Dialogue_NavLeftName))
        {
            GameObject.Find(Dialogue_NavLeftName).GetComponent<Button>().onClick.Invoke(); 
            yield return new WaitForSeconds(swipeDuration);
        }

        
        
        // Find an active character and click to choose them
        foreach (CharacterInstance c in currentCharacters)
        {
            yield return new WaitUntil(() =>
                GameObject.Find(Dialogue_ConfirmTalkButtonName) is not null &&
                GameObject.Find(Dialogue_ConfirmTalkButtonName).GetComponent<GameButton>() is not null);
            if (c.isActive && emptyQuestionCharacters.All(emptyC => emptyC.characterName != c.characterName))
            {
                GameObject.Find(Dialogue_ConfirmTalkButtonName).GetComponent<GameButton>().onClick.Invoke();
                break;
            }

            if (GameObject.Find(Dialogue_NavRightName))
            {
                GameObject.Find(Dialogue_NavRightName).GetComponent<Button>().onClick.Invoke();
                yield return new WaitForSeconds(swipeDuration);
            }
            else
            {
                throw new Exception("There are no active characters");
            }
        }
        
        //yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => SceneManager.GetSceneByName(Dialogue_DialogueSceneName).isLoaded);

        // Check if we are in the Dialogue scene
        Assert.IsTrue(SceneManager.GetSceneByName(Dialogue_DialogueSceneName).isLoaded);
    }
    
    /// <summary>
    /// In dialogueScene, keep talking 
    /// </summary>
    /// <returns></returns>
    private IEnumerator KeepTalking()
    {
        Button continueButton = null;
        // Wait until you can ask a question, or the dialogue ends
        while (GameObject.Find(Dialogue_QuestionFieldName) == null && SceneManager.GetSceneByName(Dialogue_DialogueSceneName).isLoaded)
        {
            yield return null;
            if (continueButton != null ||
                GameObject.Find(Dialogue_ContinueTalkingButtonName) != null)
            {
                if (continueButton is null)
                    continueButton = GameObject.Find(Dialogue_ContinueTalkingButtonName).GetComponent<Button>(); 
                continueButton.onClick.Invoke();
            }
        }
    }

    private IEnumerator FullConversation(int numQuestions, List<CharacterInstance> emptyQuestionCharacters, List<CharacterInstance> currentCharacters)
    {
        bool hasGreeting = true;
        // Ask a certain number of questions
        for (int j = 0; j < numQuestions; j++)
        {
            // Wait until you can ask a question
            yield return KeepTalking();
            
            //wait for the button to load
            yield return new WaitWhile(() =>
            {
                if (GameObject.Find(Dialogue_QuestionButtonName) is not null)
                    return GameObject.Find(Dialogue_QuestionButtonName)
                        .GetComponent<Button>() is null;
                return !SceneManager.GetSceneByName(Intro_NPCSelectSceneName).isLoaded;
            });
            //check if talking was a greeting
            if (SceneManager.GetSceneByName(Intro_NPCSelectSceneName).isLoaded)
                break;

            hasGreeting = false;
            GameObject.Find(Dialogue_QuestionButtonName).GetComponent<Button>().onClick.Invoke();

            // Get the DialogueManager
            var dm = GameObject.Find(Dialogue_DialogueManagerName).GetComponent<DialogueManager>();
            
            // If the character has no more questions remaining, add the character to the list of emptyQuestionCharacters.
            if (dm.currentRecipient.RemainingQuestions.Count == 0)
                emptyQuestionCharacters.Add(dm.currentRecipient);
            
            // The final iteration of the loop should continue to the hint scene
            if (j == numQuestions - 1)
            {
                // Wait for hint scene to be over
                while (!SceneManager.GetSceneByName(Intro_NPCSelectSceneName).isLoaded &&
                       !SceneManager.GetSceneByName(Epilogue_EpilogueSceneName).isLoaded)
                {
                    yield return null;

                    // Go through hint scene if it's active, else go through dialogue scene
                    if (GameObject.Find(Dialogue_PhoneFieldName) != null)
                    {
                        if (GameObject.Find(Dialogue_PhoneContinueButtonName) != null)
                            GameObject.Find(Dialogue_PhoneContinueButtonName).GetComponent<Button>()
                                .onClick
                                .Invoke();
                    }
                    else if (GameObject.Find(Dialogue_ContinueTalkingButtonName) != null)
                        GameObject.Find(Dialogue_ContinueTalkingButtonName).GetComponent<Button>()
                            .onClick
                            .Invoke();
                }
            }
        }

        if (hasGreeting)
        {
            yield return null;
            yield return SelectNpc(emptyQuestionCharacters, currentCharacters);
            yield return FullConversation(numQuestions, emptyQuestionCharacters, currentCharacters);
        }
    }
    #endregion
}