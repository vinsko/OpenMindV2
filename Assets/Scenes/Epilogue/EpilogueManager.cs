// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// � Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EpilogueManager : MonoBehaviour
{
    [FormerlySerializedAs("PortraitContainer")]
    [Header ("Scene Objects")]
    [SerializeField] GameObject portraitContainer;
    
    [Header("Resources")] 
    [SerializeField] private GameObject portraitPrefab;

    [Header("Epilogue")] 
    [SerializeField] private GameEvent onDialogueStart;
    [SerializeField] private GameEvent onEpilogueEnd;
    
    // Private Variables
    private List<CharacterInstance> characters;
    private int                     culpritId;
    private SceneController         sc;
    
    public StoryObject             story;
    
    #region EpilogueFlow
    // 1. Player selects who they think is the culprit
    // 1.5 (Multiplayer only) Player reads other player's notebook and repeats step 1.
    // 2. (Optional) Lose Scenario
    // 3. Win scenario --> Dialogue with Fill in-textboxes.
    // 4. GameWon/Loss screen
    #endregion


    private void Awake()
    {
        sc = SceneController.sc;
    }

    public void StartEpilogue(Component sender, params object[] data)
    {
        // Set variables to the proper data
        foreach (var d in data)
        {
            if (d is StoryObject story)
                this.story = story;
            if (d is List<CharacterInstance> characters)
                this.characters = characters;
            if (d is int culpritId)
                this.culpritId = culpritId;
        }
        
        // Populates grid with all the remaining current characters.
        PopulateGrid(characters.Where(c => c.isActive).ToList());
        
        // (Step 1) The player must now select the culprit in a 'selection'-scene.
        // See region 'Culprit Selection'
    }


    #region Culprit Selection
    
    /// <summary>
    /// For each character provided, instantiate a prefab of a portrait with the correct portrait
    /// linked to the correct id.
    /// We later use this to see if we chose the correct Culprit.
    /// </summary>
    public void PopulateGrid(List<CharacterInstance> characters)
    {
        foreach (Transform child in portraitContainer.transform)
        {
            Destroy(child);
        }
        
        Transform parent = portraitContainer.transform;
        foreach (CharacterInstance character in characters)
        { 
            // Create a new SelectOption object.
            GameObject newOption = Instantiate(portraitPrefab, parent);
            // Set avatar
            newOption.GetComponent<CharacterIcon>().SetAvatar(character);
            // set name - there should only be 1 TMP-component in this prefab.
            newOption.GetComponentInChildren<TMP_Text>().text = character.characterName;
            // Add delegate to onclick-event
            newOption.GetComponent<GameButton>().onClick.AddListener(delegate { CharacterSelected(character); });
        }
    }

    private void CharacterSelected(CharacterInstance chosenCharacter)
    {
        // Set win state
        bool hasWon = chosenCharacter.isCulprit;
        
        StartEpilogueDialogue(chosenCharacter, hasWon, hasWon);
    }
    #endregion


    #region Epilogue Dialogue
    
    /// <summary>
    /// Used to start dialogue in the epilogue scene (talking to the person chosen as the final choice).
    /// </summary>
    /// <param name="character"> The character which has been chosen. </param>
    public async void StartEpilogueDialogue(CharacterInstance character, bool hasWon, bool startCulpritDialogue)
    {
        // Transition to the dialogue scene.
        // If its already loaded, unload it first.
        if (SceneManager.GetSceneByName("DialogueScene").isLoaded)
        {
            await sc.TransitionScene(
                SceneController.SceneName.DialogueScene, 
                SceneController.SceneName.DialogueScene, 
                SceneController.TransitionType.Transition,
                true);
        }
        else
        {
            await sc.TransitionScene(
                SceneController.SceneName.EpilogueScene,
                SceneController.SceneName.DialogueScene,
                SceneController.TransitionType.Additive,
                false);
        }

        // Create the DialogueObject and corresponding children.
        // This background displays the suspected culprit over the Dialogue-background
        var background = DialogueManager.dm.CreateDialogueBackground(story, character, story.epilogueBackground);

        if (hasWon)
        {
            var dialogueObject = story.storyEpilogueWonDialogue.GetDialogue(background);
            onDialogueStart.Raise(this, dialogueObject, character);
            GetComponents<GameEventListener>()[1].response.AddListener(delegate{EndEpilogue(hasWon);});
        }
        else if (!hasWon && !startCulpritDialogue)
        {
            var dialogueObject = story.storyEpilogueLossDialogueNPC.GetDialogue(background);
            onDialogueStart.Raise(this, dialogueObject, character);
            // Lose-scenario, so we add a listener for 'DialogueEnd', where if it ends,
            // we got into StartEpilogueDialogue again, but now for the win-scenario.
            GetComponents<GameEventListener>()[1].response.AddListener(delegate{
                StartEpilogueDialogue(
                    characters.Where(c=> c.isCulprit).ToList()[0], 
                    hasWon, 
                    true);
                
            });
        }
        else
        {
            var dialogueObject = story.storyEpilogueLossDialogueCulprit.GetDialogue(background);
            onDialogueStart.Raise(this, dialogueObject, character);
            GetComponents<GameEventListener>()[1].response.AddListener(delegate{EndEpilogue(hasWon);});
        }
        
    }
    
    #endregion


    #region End Game Logic

    /// <summary>
    /// This method ends the dialogue, and loads the correct GameOver-scene depending on 'hasWon'
    /// </summary>
    public async void EndEpilogue(bool hasWon)
    {
        // Destroy remaining toolbox items (the buttons)
        Destroy(GameObject.Find("Toolbox"));
        
        // Updata UserData
        SaveUserData.Saver.UpdateUserDataValue(FetchUserData.UserDataQuery.playedBefore, true);
        if (hasWon)
            switch (story.storyID)
            {
                case 0:
                    SaveUserData.Saver.UpdateUserDataValue(FetchUserData.UserDataQuery.storyAWon, true);
                    break;
                case 1:
                    SaveUserData.Saver.UpdateUserDataValue(FetchUserData.UserDataQuery.storyBWon, true);
                    break;
                case 2:
                    SaveUserData.Saver.UpdateUserDataValue(FetchUserData.UserDataQuery.storyCWon, true);
                    break;
            }
        
        // TODO: Allegedly this is invalid, but it still gets unloaded. weird.
        // Unload Dialogue
        await sc.TransitionScene(
            SceneController.SceneName.DialogueScene,
            SceneController.SceneName.EpilogueScene,
            SceneController.TransitionType.Unload,
            false);
        
        // Transition to GameOver
        await sc.TransitionScene(
            SceneController.SceneName.EpilogueScene, 
            SceneController.SceneName.GameOverScene,
            SceneController.TransitionType.Transition,
            true);

        // Set the gameState.
        if (hasWon)
            GameManager.gm.gameState = GameManager.GameState.GameWon;
        else
            GameManager.gm.gameState = GameManager.GameState.GameLoss;
        
        // Send the game values to the GameOver-scene. 
        onEpilogueEnd.Raise(this, hasWon, characters, culpritId, story);
    }
    

    #endregion
    
    
    #region Notebook Logic
    // TODO: This is mostly important for Multiplayer, so a 2nd notebook can be opened.
    
    // TODO: Find a way to open NoteBook here in Epilogue as well, while choosing the culprit.
    

    #endregion


    #region Multiplayer Logic

    

    #endregion
    
    
}
