// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector2 = System.Numerics.Vector2;

/// <summary>
/// Manager class for the introduction.
/// </summary>
public class IntroductionManager : MonoBehaviour
{
    // PlayableDirectors manage the different timelines for the different stories
    [Header("Stories")]
    public PlayableDirector introStoryA;
    public PlayableDirector introStoryB;
    public PlayableDirector introStoryC;
    
    [Header("Buttons")]
    public GameObject continueButton;
    [SerializeField] Button     sendButton; // Send message button
    [SerializeField] GameObject skipButton;
    
    [Header("Variables for introduction A")]
    private                  GameObject[]  messages;         // Contains the instantiated TextMessage objects
    public                   GameObject[]  messageLocations; // Locations on the screen where TextMessages can be shown
    public                   TMP_Text      typingText;       // Text that will be used for the typing animation
    [SerializeField] public  TextMessage[] textMessages;     // Contains the non-instantiated TextMessage objects
    [SerializeField] private Transform     canvasTransform;  // Necessary to instantiate the textmessage prefabs
    [SerializeField] private Image         phone;            // The phone on which the messages are shown
    
    [Header("Variables for introduction C")]
    [SerializeField] private Image computer;                 // AI sidekick 
    
    [Header("General Variables")]
    public Sprite[]   sprites;      // Stores all the used sprites for the introduction.
    public String[]   storyText;    // Stores all the used text for the introduction. 
    public Image      background;
    private bool   changeSprite;    // This boolean is used to flip between versions of charactersprites
    private string characterName;   // Name of the character that is currently speaking
    
    [SerializeField] public Image      character;
    [SerializeField] public TMP_Text   nameTag;
    [SerializeField]        GameObject nameTagImage; 
    
    // The variables below are the UI components that we want to manipulate during the introduction
    [SerializeField] private DialogueAnimator dialogueAnimator;
    [SerializeField] private DialogueAnimator typingAnimation;
    
    // Variables to keep track of the state of the introduction within this code. 
    public PlayableDirector currentTimeline; // public for testing purposes
    public int BackgroundIndex { get; set; } = 0;    // backgrounds[backgroundIndex] is the currently shown background.
    public int TextIndex { get; set; } = 0;         // text[textIndex] is the currently shown text. 
    public int TextMessageIndex { get; set; } = 0;
    
    // GameEvent, necessary for passing the right story to Loading
    public GameEvent onGameLoaded;
    private StoryObject story;
    
    public GameObject textbubble;
    public TMP_Text   text;
    public TMP_Text   tag;
    
    /// <summary>
    /// Starts the proper intro.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="data">The story that was chosen.</param>
    public void StartIntro(Component sender, params object[] data)
    {
        continueButton.SetActive(true);
        changeSprite = true; 
        // depending on the chosen storyline, play the intro to the story
        if (data[0] is StoryObject storyObject)
        {
            // set story-variable
            story = storyObject;
            // Start the music
            SettingsManager.sm.SwitchMusic(story.storyIntroMusic, null, true);
            // depending on the chosen storyline, play the intro to the story
            switch (storyObject.storyID)
            {
                case 0:
                    StoryA();
                    break;
                case 1:
                    StoryB();
                    break;
                case 2:
                    StoryC();
                    break;
                default:
                    StoryA();
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Illegal data passed to Introduction-scene. Returning to StorySelectScene to retry.");
            // Return to StorySelectScene and try again.
            SceneManager.LoadScene("StorySelectScene");
        }
        // Do behavior based on UserData
        UpdateUserDataByStory(story);
    }
    
    /// <summary>
    /// This method updates the User Data by story. This way the game keeps track of whether or not the player has already
    /// completed this story. Depending on this data, different features may be available (e.g. the skip button). 
    /// </summary>
    private void UpdateUserDataByStory(StoryObject story)
    {
        // initialize with arbitrary value.
        FetchUserData.UserDataQuery query = FetchUserData.UserDataQuery.playedBefore;
        
        // operate
        switch (story.storyID)
        {
            case 0:
                query = FetchUserData.UserDataQuery.storyAIntroSeen;
                break;
            case 1:
                query = FetchUserData.UserDataQuery.storyBIntroSeen;
                break;
            case 2:
                query = FetchUserData.UserDataQuery.storyBIntroSeen;
                break;
            default:
                Debug.LogError("Invalid story, could not fetch userdata.");
                break;
        }
        
        // Do behavior based on query result.
        // In this case, set skipButton active ONLY if query returns true.
        if (FetchUserData.Loader.GetUserDataValue(query))
        {
            skipButton.SetActive(true);
        }
        
        // update userdata
        SaveUserData.Saver.UpdateUserDataValue(query, true);
    }
    
    // This region contains methods that regulate the different storylines. 
    #region StoryLines
    /// <summary>
    /// Method that prepares the scene to play storyline A. 
    /// </summary>
    public void StoryA()
    {
        BackgroundIndex = 0;
        messages = new GameObject[textMessages.Length];
        for(int i = 0; i < textMessages.Length; i++)
        {
            // Instantiate the TextMessages
            GameObject instantiatedMessage = Instantiate(textMessages[i].message, canvasTransform);
            TMP_Text tmpText = instantiatedMessage.GetComponentInChildren<TMP_Text>();
            if (tmpText != null) // When the text component is null, it is an empty text
            {
                tmpText.text = textMessages[i].messageContent; // Change the content to the correct content
            }
            // Make sure the messages end up at the correct location in the hierarchy. 
            // Otherwise, it might be the case that they will overlap with other game objects. 
            instantiatedMessage.transform.SetSiblingIndex(canvasTransform.childCount - 6);
            messages[i] = instantiatedMessage; // Add the instantiated message to the textMessage array
        }
        // Initialize the right timeline and indices for story A. 
        currentTimeline = introStoryA;
        ResetTimeline();
        
        currentTimeline.Play();
        background.sprite = sprites[3];
    }
    
    /// <summary>
    /// Method that prepares the scene to play storyline B. 
    /// </summary>
    public void StoryB()
    {
        character.sprite = sprites[5];
        currentTimeline = introStoryB;
        ResetTimeline();
        
        TextIndex = 4;
        background.sprite = sprites[4];
        characterName = "Alex";
        currentTimeline.Play();
    }
    
    /// <summary>
    /// Method that prepares the scene to play storyline C. 
    /// </summary>
    public void StoryC()
    {
        currentTimeline = introStoryC;
        ResetTimeline();
        TextIndex = 19;
        background.sprite = sprites[11];
        character.sprite = sprites[7];
        characterName = "Receptionist";
        currentTimeline.Play();
    }
    #endregion
    
    // This region contains methods regarding introduction A.
    #region Introduction A
    /// <summary>
    /// This method shows a new text on the screen and makes sure old texts are removed if necessary. 
    /// </summary>
    public void SendText()
    {
        PauseCurrentTimeline();
        sendButton.gameObject.SetActive(false);
        typingText.gameObject.SetActive(false);
        phone.sprite = sprites[2]; // Change the background to the phone background. 
        TextMessageIndex++;
        
        // Make sure the four most recent texts are shown on the screen. 
        HideOrShowTexts(false);     // Old messages need to be removed. 
        for (int i = TextMessageIndex; i < TextMessageIndex + 5; i++)
        {
            messages[i].transform.position = messageLocations[i-TextMessageIndex].transform.position;
            messages[i].SetActive(true);
        }
        HideOrShowTexts(true); // Show the new texts. 
    }
    
    /// <summary>
    /// Depending on the value of show, this method either hides of shows the text messages on the screen.
    /// </summary>
    /// <param name="show"> Determines whether to hide or to show the texts. </param>
    private void HideOrShowTexts(bool show)
    {
        foreach (GameObject location in messageLocations)
        {
            location.SetActive(show);
        }
        if (!show) // If the messages need to be hidden, make sure old messages are hidden as well. 
        {
            foreach(GameObject message in messages)
            {
                message.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// This method is called when the phone needs to come up into the screen. 
    /// </summary>
    public void PhoneUp()
    {
        BackgroundIndex++;
        phone.sprite = sprites[BackgroundIndex];
        PauseCurrentTimeline();
    }
    
    /// <summary>
    /// This method is called when the phone needs to leave the screen. 
    /// </summary>
    public void PhoneDown()
    {
        HideOrShowTexts(false);
        phone.sprite = sprites[0];
    }
    
    /// <summary>
    /// This method creates the animation that the player types in text and can then send it. 
    /// </summary>
    public void TypeAnimation()
    {
        continueButton.SetActive(false); //This button is not necessary now, because we have another button to continue. 
        PauseCurrentTimeline();
        
        // Reset the typing animation object
        typingAnimation.gameObject.SetActive(true);
        typingAnimation.CancelWriting();
        
        // Activate the UI elements for the typing animation
        sendButton.gameObject.SetActive(true);
        typingText.gameObject.SetActive(true);
        
        // Write the next message, '+ messageLocations.Length' is to account for the empty messages. 
        typingText.text = textMessages[TextMessageIndex + messageLocations.Length].messageContent;
        typingAnimation.WriteDialogue(textMessages[TextMessageIndex + messageLocations.Length].messageContent);
    }
    
    #endregion
    
    // This region contains methods regarding introduction B.
    #region Introduction B
    /// <summary>
    /// This method changes the sprite of the character in order to make him have a vision. 
    /// </summary>
    public void Vision()
    {
        if (changeSprite)
        {
            character.sprite = sprites[6];
        }
        else
        {
            character.sprite = sprites[5];
        }
        
        changeSprite = !changeSprite; 
    }
    
    #endregion
    
    // This region contains methods regarding introduction C.
    #region Introduction C
    /// <summary>
    /// This method changes the sprite of the character into the computer. 
    /// </summary>
    public void Computer()
    {
        if (changeSprite)
        {
            computer.sprite = sprites[9];
        }
        else
        {
            computer.sprite = sprites[10];
        }
        changeSprite = !changeSprite; 
    }
    
    public void ReceptionistEmotion()
    {
        if (changeSprite)
        {
            character.sprite = sprites[12];
        }
        else
        {
            character.sprite = sprites[7];
        }
        changeSprite = !changeSprite;
        
    }
    
    /// <summary>
    /// This method changes and shows text the computer is saying. 
    /// </summary>
    public void ChangeComputerText()
    {
        nameTag.text = "Computer";
        computer.sprite = sprites[10];
        UpdateText();
    }
    
    #endregion
    
    // This region contains methods regarding the story text of the introductions
    #region StoryText
    /// <summary>
    /// This method changes and shows text the character is saying. 
    /// </summary>
    public void ChangeCharacterText()
    {
        nameTag.text = characterName;
        nameTagImage.gameObject.SetActive(true);
        UpdateText();
    }
    
    /// <summary>
    /// This method changes and shows text the player is saying. 
    /// </summary>
    public void ChangePlayerText()
    {
        nameTagImage.gameObject.SetActive(false);
        UpdateText();
    }
    
    /// <summary>
    /// This method updates the text that is shown on the screen. 
    /// </summary>
    private void UpdateText()
    {
        PauseCurrentTimeline();
        // Activate UI elements for the player text. 
        dialogueAnimator.gameObject.SetActive(true);
        textbubble.SetActive(true);
        try
        {
            text.text = storyText[TextIndex];
            dialogueAnimator.CancelWriting();
            dialogueAnimator.WriteDialogue(storyText[TextIndex]);
        }
        catch
        {
            TextIndex = 0;
            Debug.LogError("Error: No more text to speak.");
        }
        TextIndex++; // Keep track of which text needs to be shown. 
    }
    
    /// <summary>
    /// This method removes the dialog from the screen. 
    /// </summary>
    public void HideDialog()
    {
        dialogueAnimator.gameObject.SetActive(false);
        textbubble.SetActive(false);
        nameTagImage.gameObject.SetActive(false);
    }
    #endregion
    
    // This region contains methods that directly manipulate the timeline
    #region TimelineManipulators

    /// <summary>
    /// Pauses the timeline.
    /// </summary>
    public void PauseCurrentTimeline()
    {
        continueButton.SetActive(true);
        currentTimeline.Pause();
    }

    /// <summary>
    /// Continues the timeline.
    /// </summary>
    public void ContinueCurrentTimeline()
    {
        if (dialogueAnimator.IsOutputting)
        {
            dialogueAnimator.SkipDialogue();
            typingAnimation.SkipDialogue();
        }
        else
        {
            continueButton.SetActive(false);
            currentTimeline.Play();
        }
    }
    
    /// <summary>
    /// This method makes sure the timeline starts playing from the beginning. 
    /// </summary>
    private void ResetTimeline()
    {
        currentTimeline.time = 0; // Reset the timeline to the start
        currentTimeline.RebuildGraph();
        
    }
    #endregion
    
    // This region contains methods that handle the starting of the game at the end of the introduction
    #region StartGame
    /// <summary>
    /// Starts the game once the intro is over.
    /// </summary>
    public void StartGame()
    {
        StartCoroutine(LoadGame());
    }
    
    /// <summary>
    /// Loads the game.
    /// </summary>
    // TODO: This is duplicate code, also found in Loading.cs. Make this a global thing?
    IEnumerator LoadGame()
    {
        // Start the loadscene-operation
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);
        
        // Within this while-loop, we wait until the scene is done loading. We check this every frame
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        if(MultiplayerManager.mm)
            onGameLoaded.Raise(this, MultiplayerManager.mm.init);
        else
            onGameLoaded.Raise(this, story);
        
        // Finally, when the data has been sent, we then unload our currentscene
        SceneManager.UnloadSceneAsync("IntroStoryScene");  // unload this scene; no longer necessary
        
        // Make sure tutorial is automatically loaded when the game starts..
        // .. IF this is the first time playing. (has not played before)
        if (!FetchUserData.Loader.GetUserDataValue(FetchUserData.UserDataQuery.playedBefore))
        {
            GameObject tutorial = GameObject.Find("HelpButton");
            Button helpButton = tutorial.GetComponentInChildren<Button>();
            helpButton.onClick.Invoke();
        }
    }
    #endregion
    
}