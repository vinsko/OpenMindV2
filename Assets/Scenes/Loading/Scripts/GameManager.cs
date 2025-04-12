// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

/// <summary>
/// The manager for the entire game, where most of the magic happens.
/// instances of this class can be passed to all other classes.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Resources")]
    [SerializeField] private List<CharacterData> characters; // The full list of characters in the game

    [Header("Events")] 
    public                    GameEvent   onDialogueStart;
    public                    GameEvent   onEpilogueStart;
    public                    GameEvent   onNPCSelectLoad;

    // GAME VARIABLES
    [NonSerialized] public int numQuestionsAsked;   // The amount of times  the player has talked, should be 0 at the start of each cycle
    public List<CharacterInstance> currentCharacters;   // The list of the characters in the current game. This includes both active and inactive characters
    [NonSerialized] public GameState gameState;     // This gamestate is tracked to do transitions properly and work the correct behaviour of similar methods
    public CharacterInstance FinalChosenCuplrit;    // Save the character that has been chosen at the end of the game.
    public bool hasWon;     // Set this bool to true if the correct character has been chosen at the end, else false.
    
    public StoryObject
        story { get; private set; } // Contains information about the current game pertaining to the story
    
    // Instances
    public        Random random = new Random(); //random variable is made global so it can be reused
    public static GameManager gm;       // static instance of the gamemanager
    private       SceneController sc;
    public NotebookData notebookData;
    public  NotebookData multiplayerNotebookData;
    public  bool         multiplayerEpilogue;

    // Enumerations
    #region Enumerations
    // This enumeration defines all the possible GameStates, which we can use to test correct behavior
    public enum GameState
    {
        // Is there a gamestate for when the game is loading in?
        Loading,        //      --> NPCSelect, HintDialogue(immediate victim)
        NpcSelect,      //      --> NpcDialogue
        CulpritSelect,  //      --> GameWon, GameLoss
        NpcDialogue,    //      --> NpcSelect, CulpritSelect
        HintDialogue,   //      --> NpcSelect
        GameLoss,       //      --> Loading (restart/retry)
        GameWon,        //      --> Loading (restart/retry)
        Epilogue
    }
    #endregion
    
    /// <summary>
    /// When loaded, make a static instance of this class so it can be reached from other places.
    /// Also make the toolbox persistent.
    /// </summary>
    private void Awake()
    {
        gm = this;
        DontDestroyOnLoad(gameObject.transform.parent);
        
        // Set the target frame rate to the screen's refresh rate
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        
        gameState = GameState.Loading;
    }
    
    /// <summary>
    /// Starts the game.
    /// If a story is passed along, starts a new game with that story.
    /// If savedata is passed along loads the game with that data.
    /// </summary>
    /// <param name="sender">The sender, sends either a <see cref="StoryObject"/> or some <see cref="SaveData"/>.</param>
    /// <param name="data">The story that the player has chosen, or the savedata that needs to be loaded.</param>
    public void StartGame(Component sender, params object[] data)
    {
        // Set reference to static SceneController
        sc = SceneController.sc;
        // Empty some variables so they can be initialized later.
        currentCharacters = new List<CharacterInstance>();
        
        // If the game is loaded (from StartMenuManager)
        if (sender is StartMenuManager)
        {
            if (data[0] is SaveData saveData)
            {
                LoadGame(saveData);
            }
            else
            {
                Debug.LogError("SaveData incorrectly parsed.");
            }
        }
        else if (data[0] is MultiplayerInit multiplayerInit)
        {
            story = Resources.LoadAll<StoryObject>("Stories")
                .First(story => story.storyID == multiplayerInit.story);
            random = new Random(multiplayerInit.seed);
            NewGame();
        }
        // Else, set the values passed to the correct variables below.
        else
        {
            int culpritID = -1;     // set to an impossible value of -1
            foreach (var d in data)
            {
                switch (d)
                {
                    case StoryObject story:
                        this.story = story;
                        break;
                    case List<CharacterInstance> characters:
                        currentCharacters = characters;
                        foreach (CharacterInstance c in currentCharacters)
                        {
                            c.isActive = true;
                            // if the character list was found after the culpritID, set it here.
                            // If it was not yet found, it will always be false.
                            c.isCulprit = (c.id == culpritID);
                        }
                        break;
                    case int id:
                        culpritID = id;
                        // if this id was found after the currentCharacters, set the culprit ID
                        if (currentCharacters != null)
                            foreach (CharacterInstance c in currentCharacters)
                                if (c.id == culpritID)
                                    c.isCulprit = true;
                        break;
                }
            }
            
            NewGame();
        }
        
    }

    /// <summary>
    /// Loads the game using savedata passed along.
    /// </summary>
    /// <param name="saveData">savedata that needs to be loaded.</param>
    public async void LoadGame(SaveData saveData)
    {
        // Fetch all storyobjects from the Resources/Stories-folder
        StoryObject[] stories = Resources.LoadAll<StoryObject>("Stories");
        // Set this game's story to the storyobject (which we fetched) which the correct ID (as per the SaveData)
        story = stories.First(s => s.storyID == saveData.storyId);
        
        //assign numQuestionsAsked
        numQuestionsAsked = saveData.numQuestionsAsked;

        //clear all current characters
        currentCharacters.Clear();
        //create all current characters
        List<CharacterInstance> newCurrentCharacters = characters.FindAll(c => 
            saveData.activeCharacterIds.Contains(c.id) ||
            saveData.inactiveCharacterIds.Contains(c.id)).
                Select(c => new CharacterInstance(c)).ToList();
        
        //then assign each instance in the same order they were saved. Even if the order doesn't matter, it may still matter in the future.
        //the order of askedQuestionsPerCharacter is a copy of the order of the old currentCharacters
        foreach (var valueTuple in saveData.remainingQuestions)
            currentCharacters.Add(newCurrentCharacters.Find(ncc => ncc.id == valueTuple.Item1));
        
        //assign all data to the current characters
        currentCharacters = currentCharacters.Select(c =>
        {
            c.isActive = saveData.activeCharacterIds.Contains(c.id);
            c.isCulprit = saveData.culpritId == c.id;

            c.RemainingQuestions = saveData.remainingQuestions.First(qs => qs.Item1 == c.id).Item2;
            c.talkedTo = saveData.charactersGreeted.First(qs => qs.Item1 == c.id).Item2;
            return c;
        }).ToList();
        
        //assign notebook data
        Dictionary<CharacterInstance, NotebookPage> notebookDataPerCharacter = saveData.characterNotes.Select(cn =>
        {
            CharacterInstance instance = currentCharacters.First(c => c.id == cn.Item1);
            return new KeyValuePair<CharacterInstance, NotebookPage>(instance, new NotebookPage(cn.Item2, instance));
        }).ToDictionary(pair => pair.Key, pair => pair.Value);
        notebookData = new NotebookData(notebookDataPerCharacter, saveData.personalNotes);
        
        //unload all scenes
        SceneController.sc.UnloadAdditiveScenes();
        
        // Start the game music
        SettingsManager.sm.SwitchMusic(story.storyGameMusic, 1, true);
        
        //load npcSelect scene
        await sc.StartScene(SceneController.SceneName.NPCSelectScene);
        
        //update gamestate
        gameState = GameState.NpcSelect;

        // Set the selected character.
        onNPCSelectLoad.Raise(this);
    }

    /// <summary>
    /// Initializes a new game.
    /// </summary>
    private void NewGame()
    {
        // Populate the list of characters, unless its empty and contains a culprit
        // (It could be instantiated if the game was restarted)
        if (!(currentCharacters.Count > 0 && 
              currentCharacters.All(c => c.isActive) && 
              currentCharacters.Any(c => c.isCulprit)))
            PopulateCharacters();
        // Create new notebook
        notebookData = new NotebookData();
        // Start the game music
        SettingsManager.sm.SwitchMusic(story.storyGameMusic, null, true);
        FirstCycle();
    }
    
    // This region contains methods that start or end the cycles.
    #region Cycles
    /// <summary>
    /// Works like StartCycle, but can optionally skip the immediate victim.
    /// This method starts the game with a special SceneTransition-invocation for the first scene of the game.
    /// </summary>
    private async void FirstCycle()
    {
        if (story.immediateVictim)
        {
            // Choose a victim, make them inactive, and print the hints to the console.
            string victimName = ChooseVictim();
        }
        // Reset number of times the player has talked
        numQuestionsAsked = 0;
        
        // Start the game at the first scene; the NPC Selection scene
        await sc.StartScene(SceneController.SceneName.NPCSelectScene);
        
        // Change the gamestate
        gameState = GameState.NpcSelect;
        
        // Set the selected character.
        onNPCSelectLoad.Raise(this);
    }
    
    /// <summary>
    /// The main cycle of the game.
    /// This should loop everytime the player speaks to an NPC until a certain number of NPCs have been spoken to,
    /// at that point the cycle ends and the player has to choose which NPC they think is the culprit
    /// </summary>
    private void StartCycle(CharacterInstance recipient)
    {
        // Choose a victim, make them inactive, and print the hints to the console.
        string victimName = ChooseVictim();
        // Reset number of times the player has talked
        numQuestionsAsked = 0;

        // Check if there are enough hints
        string[] hintDialogue;
        if (GetCulprit().RemainingQuestions.Count > 0)
        {
            // Add the new hint to the dictionary
            wordReplacements["hint"] = string.Join(" ", GetCulprit().GetRandomTrait());
            hintDialogue = story.hintDialogue;
        }
        else
        {
            hintDialogue = story.noMoreHintsDialogue;
        }

        // Process dialogue (replace <> words)
        List<string> dialogue = new();
        for (int i = 0; i < hintDialogue.Length; i++)
        {            
            string line = ProcessDialogue(hintDialogue[i]);
            dialogue.Add(line);
        }

        // Creates Dialogue that says who disappeared and provides a new hint.
        StartHintDialogue(dialogue, recipient);
    }

    /// <summary>
    /// Checks for keywords in <paramref name="inputLine"/> and replaces them with proper values.
    /// </summary>
    /// <param name="inputLine">The line to be altered.</param>
    /// <returns>The inputLine with its keywords replaced with proper values.</returns>
    public string ProcessDialogue(string inputLine)
    {
        // Regular expression to find placeholders in the format <keyword>
        string pattern = @"\<(\w+)\>";
        var regex = new Regex(pattern);

        // Replace matches with corresponding values from the replacements dictionary
        return regex.Replace(inputLine, match =>
        {
            string key = match.Groups[1].Value; // Extract the keyword (e.g., "name", "hint")
            return wordReplacements.TryGetValue(key, out string replacement) ? replacement : match.Value;
        });
    }

    /// <summary>
    /// The dictionary containing replacements for certain keywords.
    /// </summary>
    public Dictionary<string, string> wordReplacements = new()
    {
        { "victimName", "Placeholder name" },
        { "hint", "Placeholder hint dialogue." }
    };

    /// <summary>
    /// Ends the cycle when all questions have been asked.
    /// If we have too few characters remaining, we must select the culprit,
    /// otherwise we start a new cycle.
    /// </summary>
    public void EndCycle(CharacterInstance recipient) 
    {
        // Start Cycle as normal
        if (EnoughCharactersRemaining())
            StartCycle(recipient);
        // Start the Epilogue
        else
        {
            // Change the gamestate
            gameState = GameState.CulpritSelect;
            
            StartPreEpilogueDialogue();
            // Start the epilogue music
            SettingsManager.sm.SwitchMusic(story.storyEpilogueMusic, null, true);
        }
    }
    
    #endregion

    #region InstantiateGameOrCycles
    /// <summary>
    /// Makes a randomized selection of characters for this loop of the game, from the total database of all characters.
    /// Also makes sure they are all set to 'Active', and selects a random culprit.
    /// </summary>
    private void PopulateCharacters()
    {
        // Start by emptying the list
        currentCharacters = new List<CharacterInstance>();
        
        // Create a random population of 'numberOfCharacters' number, initialize them, and choose a random culprit.

        // Create array to remember what indices we have already visited, so we don't get doubles.
        // Because this empty array is initiated with 0's, we need to offset our number generated below with +1.
        // When we use this index to retrieve a character from the characters-list, we reverse the offset with -1.
        int[] visitedIndices = new int[story.numberOfCharacters];

        // We iterate over a for-loop to find a specific number of characters to populate our game with.
        // We clamp it down to the smallest value, in case numberOfCharacters is more than the number we have generated.
        story.numberOfCharacters = Math.Min(characters.Count, story.numberOfCharacters);
        for (int i = 0; i < story.numberOfCharacters; i++)
        {
            bool foundUniqueInt = false; // We use this bool to exist the while-loop when we find a unique index
            while (!foundUniqueInt)
            {
                int index = random.Next(0, characters.Count) + 1; // offset by 1 to check existence

                string arrayString = "";
                for (int j = 0; j < visitedIndices.Length; j++)
                    arrayString += (visitedIndices[j] + ", ");

                if (!visitedIndices.Contains(index))
                {
                    var toAdd = characters[index - 1]; // correct the offset
                    currentCharacters.Add(
                        new CharacterInstance(toAdd)); // add the character we found to the list of current characters
                    visitedIndices[i] = index; // add the index with the offset to the array of visited indices
                    foundUniqueInt = true; // change the boolean-value to exit the while-loop
                }
            }
        }

        // Make sure all the characters are 'active'
        foreach (var c in currentCharacters)
        {
            c.isActive = true;
            c.isCulprit = false;
        }
        //Randomly select a culprit
        int culrpitId = random.Next(0, story.numberOfCharacters);
        currentCharacters[culrpitId].isCulprit = true;
    }

    /// <summary>
    /// Chooses a victim, changes the isActive bool to 'false' and randomly selects a trait from both the culprit and
    /// the victim that is removed from their list of questions and prints to to the debuglog
    /// </summary>
    private string ChooseVictim()
    {
        CharacterInstance culprit = GetCulprit();
        CharacterInstance victim = GetRandomVictimNoCulprit();

        // Victim put on inactive so we cant ask them questions
        victim.isActive = false;
        wordReplacements["victimName"] = victim.characterName;
        return victim.characterName;
    }
    
    /// <summary>
    /// Returns the culprit, used to give hints for the companion
    /// Assumes a culprit exists
    /// </summary>
    public CharacterInstance GetCulprit() => currentCharacters.Find(c => c.isCulprit);

    /// <summary>
    /// Returns a random (non-culprit and active) character, used to give hints for the companion
    /// Assumes there is only 1 culprit
    /// </summary>
    public CharacterInstance GetRandomVictimNoCulprit()
    {
        List<CharacterInstance> possibleVictims = currentCharacters.FindAll(c => !c.isCulprit && c.isActive); 
        return possibleVictims[random.Next(possibleVictims.Count- 1)];
    }

    #endregion
    
    // This region contains methods that directly change the Game State.
    #region ChangeGameState

    private async void StartPreEpilogueDialogue()
    {
        gameState = GameState.CulpritSelect;

        await sc.TransitionScene(
            SceneController.SceneName.DialogueScene,
            SceneController.SceneName.DialogueScene,
            SceneController.TransitionType.Transition,
            true);

        DialogueObject dialogueObject;
        if (story.storyID == 0) // Create dialogueObject for phone story
        {
            dialogueObject = new PhoneDialogueObject(story.preEpilogueDialogue.ToList(), null,
                DialogueManager.dm.CreateDialogueBackground(story, null, story.hintBackground));
        }
        else if (story.storyID == 1) // Psychic story
        {
            dialogueObject = new ContentDialogueObject(story.preEpilogueDialogue.ToList(), null,
                DialogueManager.dm.CreateDialogueBackground(story, null, 
                story.hintBackground, story.additionalHintBackgroundObjects[0]));
        }
        else if (story.storyID == 2) // AI story
        {
            dialogueObject = new ContentDialogueObject(story.preEpilogueDialogue.ToList(), null,
                DialogueManager.dm.CreateDialogueBackground(story, null,
                story.hintBackground, story.additionalHintBackgroundObjects[0]));
        }
        else
        {
            dialogueObject = new ContentDialogueObject(
                story.preEpilogueDialogue.ToList(), null,
                DialogueManager.dm.CreateDialogueBackground(story, null, story.hintBackground));
        }

        onDialogueStart.Raise(this, dialogueObject);
    }

    /// <summary>
    /// Starts the Epilogue
    /// </summary>
    private async void StartEpilogue()
    {
        gameState = GameState.Epilogue;

        // Wait for the scene transition
        await sc.TransitionScene(
            SceneController.SceneName.DialogueScene,
            SceneController.SceneName.EpilogueScene, 
            SceneController.TransitionType.Transition,
            true);
        
        // Raise the EpilogueStart-event and pass all the necessary data
        onEpilogueStart.Raise(this, story, currentCharacters, GetCulprit().id);
        
        // Then, destroy the gamemanager (but not the UImanager component)
        Destroy(this);
    }
    
    /// <summary>
    /// Closes the game.
    /// </summary>
    public void EndGame()
    {
        Application.Quit();
    }
    
    
    /// <summary>
    /// Performs a visual fade-in/out when called,
    /// displaying the victim's name and their fate, depending on the Story we are currently in.
    /// </summary>
    /// <param name="victimName">The name of the victim.</param>
    private void CycleTransition(string victimName)
    {
        string victimFate = story.victimDialogue;
        gameObject.GetComponent<UIManager>().Transition(victimName + " " + victimFate);
    }
    
    #endregion

    // This region contains methods regarding dialogue
    #region Dialogue
    /// <summary>
    /// Starts a new hint dialogue.
    /// </summary>
    /// <param name="dialogueObject">The object that needs to be passed along to the dialogue manager.</param>
    public async void StartHintDialogue(List<string> dialogue, CharacterInstance recipient)
    {
        // Change the gamestate
        gameState = GameState.HintDialogue;
        
        // TODO: Review the originscene 'GetActiveScene'. This is called by StartCycle, where we go Dialogue --> Dialogue.
        // Transition to dialogue scene and await the loading operation
        await sc.TransitionScene(
            SceneController.SceneName.DialogueScene,
            SceneController.SceneName.DialogueScene,
            SceneController.TransitionType.Transition,
            true);

        // Create the appropriate DialogueObject
        DialogueObject dialogueObject;
        string alternateName;
        if (story.storyID == 0) // 0 corresponds to the phone story
        {
            dialogueObject = new PhoneDialogueObject(dialogue, null, DialogueManager.dm.CreateDialogueBackground(story, null, story.hintBackground));
            alternateName = "";
        }
        else if (story.storyID == 1) // 1 corresponds to the sidekick story
        {
            dialogueObject = new ContentDialogueObject(
                dialogue[0], null,
                DialogueManager.dm.CreateDialogueBackground(story, null,
                story.hintBackground, story.additionalHintBackgroundObjects[0]));

            var object2 = new ContentDialogueObject(
                dialogue[1], null,
                DialogueManager.dm.CreateDialogueBackground(story, null,
                story.hintBackground, story.additionalHintBackgroundObjects[1]
                ));
            dialogueObject.Responses.Add(object2);

            object2.Responses.Add(new ContentDialogueObject(
                dialogue[2], null,
                DialogueManager.dm.CreateDialogueBackground(story, null,
                story.hintBackground, story.additionalHintBackgroundObjects[0]
                )));

            alternateName = "Alex";
        }
        else if (story.storyID == 2) // 2 corresponds to the AI story
        {
            dialogueObject = new ContentDialogueObject(
                dialogue, null,
                DialogueManager.dm.CreateDialogueBackground(story, null,
                story.hintBackground, story.additionalHintBackgroundObjects[0]));

            alternateName = "Computer";
        }
        else
        {
            dialogueObject = new ContentDialogueObject(
                dialogue, null, 
                DialogueManager.dm.CreateDialogueBackground(story, null, 
                story.hintBackground));

            alternateName = "";
        }

        // The gameevent here should pass the information to Dialoguemanager
        // ..at which point dialoguemanager will start.
        onDialogueStart.Raise(this, dialogueObject, recipient, (false, alternateName));
    }

    /// <summary>
    /// Can be called to start Dialogue with a specific character, taking a CharacterInstance as parameter.
    /// This toggles-off the NPCSelectScene,
    /// and switches the dialogueRecipient-variable to the characterInstance that is passed as a parameter.
    /// Then, it loads the DialogueScene.
    /// </summary>
    /// <param name="character">The character that the dialogue will be with.</param>
    ///  TODO: Should use the id of a character instead of the CharacterInstance.
    public async void StartDialogue(CharacterInstance character)
    {
        // Transition to dialogue scene and await the loading operation
        await sc.TransitionScene(
            SceneController.SceneName.NPCSelectScene,
            SceneController.SceneName.DialogueScene,
            SceneController.TransitionType.Transition,
            true);
        
        GameObject[] background = DialogueManager.dm.CreateDialogueBackground(story, character, story.dialogueBackground);
        var dialogueObject = character.GetGreeting(background);
        dialogueObject.Responses.Add(new QuestionDialogueObject(background));
        
        // Until DialogueManager gets its information, it shouldnt do anything there.
        var dialogueRecipient = character;
        
        // Change the gamestate
        gameState = GameState.NpcDialogue;
        
        // The gameevent here should pass the information to Dialoguemanager
        // ..at which point dialoguemanager will start.
        onDialogueStart.Raise(this, dialogueObject, dialogueRecipient);
    }
        
    /// <summary>
    /// Called by DialogueManager when dialogue is ended, by execution of a DialogueTerminateObject.
    /// Checks if questions are remaining:
    /// .. if no, end cycle.
    /// .. if yes, 'back to NPCSelect'-button was clicked, so don't end cycle.
    /// </summary>
    public async void EndDialogue(Component sender, params object[] data)
    {
        // If we are coming from pre epilogue dialogue,
        // start epilogue and don't do anything else
        if (gameState == GameState.CulpritSelect)
        {
            StartEpilogue();
            
            // Start exchanging notebooks if in multiplayer mode  
            if(MultiplayerManager.mm)  
                MultiplayerNotebookExchange();
            return;
        }
        
        // Start the game music
        SettingsManager.sm.SwitchMusic(story.storyGameMusic, null, true);
        if (!HasQuestionsLeft())
        {
            // No questions left, so we end the cycle, and pass along the character which was last talked to.
            if (data[1] is CharacterInstance recipient)
                EndCycle(recipient);
        }
        else
        {
            await sc.TransitionScene(
                SceneController.SceneName.DialogueScene, 
                SceneController.SceneName.NPCSelectScene, 
                SceneController.TransitionType.Transition,
                true);
            
            gameState = GameState.NpcSelect;
            
            // Set the character which was last talked to.
            if (data[1] is CharacterInstance recipient)
                onNPCSelectLoad.Raise(this, recipient);
        }
    }    
    
    private void MultiplayerNotebookExchange()
    {
        // Send notebook
        MultiplayerManager.mm.SendNotebook();
        multiplayerEpilogue = true;
    }
    #endregion

    // This region contains methods that check certain properties that affect the Game State.
    #region CheckProperties
    /// <summary>
    /// Checks if the number of characters in the currently active game (selected in the 'currentCharacters'-list)
    /// that are also 'active' (the isActive-bool of these CharacterInstances)
    /// is more than the gamemanager variable 'numberOfActiveCharacters', which is the minimum amount of characters
    /// that should be remaining until we transition into selecting a culprit.
    /// </summary>
    /// <returns>True if more, False if not.</returns>
    public bool EnoughCharactersRemaining()
    {
        int numberOfActiveCharacters = currentCharacters.Count(c => c.isActive);
        return numberOfActiveCharacters > story.minimumRemaining;
    }
    
    /// <summary>
    /// Checks if the player can ask more questions this cycle.
    /// </summary>
    /// <returns>True if player can ask more questions, otherwise false.</returns>
    public bool HasQuestionsLeft()
    {
        return numQuestionsAsked < story.numQuestions;
    }

    public int AmountCharactersGreeted()
    {
        return currentCharacters.Count(c => c.talkedTo);
    }
    
    #endregion

    // This region contains methods necessary purely for debugging-purposes.
    #region DebuggingMethods
    /// <summary>
    /// Prints the name of all characters in the current game to the console, for debugging purposes.
    /// </summary>
    private void Test_CharactersInGame()
    {
        string output = "";
        for (int i = 0; i < currentCharacters.Count; i++)
        {
            // If its the second last, surfix is "and", if its the last, there is no surfix. If its any other, its ", "
            output += (currentCharacters[i].characterName + (i + 1 == currentCharacters.Count
                ? "."
                : (i + 2 == currentCharacters.Count ? " and " : ", ")));
        }
        Debug.Log("The " + currentCharacters.Count + " characters currently in game are " + output);
        //dialogueRecipient = currentCharacters[id];
        SceneManager.LoadScene("DialogueScene", LoadSceneMode.Additive);
    }    
    
    #endregion
}