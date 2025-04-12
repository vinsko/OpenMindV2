// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = System.Diagnostics.Debug;

/// <summary>
/// Manager class for cutscenes.
/// </summary>
public class PrologueManager : MonoBehaviour
{
    [SerializeField] public PlayableDirector prologueTimeline; // Enables us to manually pause and continue the timeline
    
    // The variables below are the UI components that we want to manipulate during the prologue scene
    [Header("Image refs")]
    [SerializeField] private Image textBubbleImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image illusionImage;
    [SerializeField] private Image receptionistImage; 
    
    [Header("Text refs")]
    [SerializeField] private TMP_Text introText;
    [SerializeField] private TMP_Text nameBoxText;
    [SerializeField] private TMP_Text spokenText;

    [Header("Misc. refs")]
    [SerializeField] private Toggle imageToggler;
    [SerializeField] private Button continueButton;
    [SerializeField] private DialogueAnimator dialogueAnimator;
    
    [Header("Prologue data")]                           // The arrays below store data that is required at a later stadium of the prologue
    [SerializeField] private Sprite[] sprites;          // Stores all the sprites used in the prologue
    [SerializeField] private string[] receptionistText; // Stores all the text spoken by the receptionist

    [Header("Resources")] 
    [SerializeField] private AudioClip prologueMusic;
    
    private int textIndex;                  // Index to keep track of the text that needs to be spoken
    private int backgroundIndex;            // Index to keep track of the background that needs to be used
    private Transform checkmarkTransform;   // This is the checkmark image on the toggler
    private bool receptionistEmotion; 
    
    /// <summary>
    /// This method is called when the scene is started this script belongs to is activated. 
    /// </summary>
    private void Start()
    {
        // Access the Checkmark GameObject via the Toggle's hierarchy
       checkmarkTransform = imageToggler.transform.Find("Background/Checkmark"); 
       
       // Set prologue-music
       SettingsManager.sm.SwitchMusic(prologueMusic,null, true);
       
       // Update UserData
       SaveUserData.Saver.UpdateUserDataValue(FetchUserData.UserDataQuery.prologueSeen, true);
       
       // Reset the timeline (necessary for when the prologue is played again)
       ResetTimeline();
    }
    
    // This region contains methods that directly manipulate the timeline. These methods are called via signal emitters
    #region TimelineManipulators
    /// <summary>
    /// This method is called via a signal receiver when continueButton is clicked and the
    /// timeline has to be resumed.
    /// </summary>
    public void ContinueTimeline()
    {
        if (dialogueAnimator.IsOutputting)
        {
            dialogueAnimator.SkipDialogue();
        }
        else
        {
            imageToggler.gameObject.SetActive(false); // Make sure toggler is removed from the screen.
            continueButton.gameObject.SetActive(false);  // Disable continuebutton
            prologueTimeline.Play(); // Resume timeline.
        }
    }
    
    /// <summary>
    /// This method is called via a signal emitter when the timeline encounters a point where
    /// it has to be paused in order to wait for user interaction.
    /// This method also activates the continuebutton with which the timeline can be resumed. 
    /// </summary>
    public void PauseTimeline()
    {
        prologueTimeline.Pause();
        continueButton.gameObject.SetActive(true); // Make sure timeline can manually be resumed. 
    }
    
    /// <summary>
    /// This method resets the timeline and makes sure it is played from the start. 
    /// </summary>
    private void ResetTimeline()
    {
        textIndex = 0;
        backgroundIndex = 0;
        prologueTimeline.time = 0;
        prologueTimeline.RebuildGraph();
        prologueTimeline.Play();
    }
    #endregion
    
    // This region contains all the methods regarding the text that is shown during the prologue. 
    #region Prologue Text
    /// <summary>
    /// This method updates the introduction text. Since there are only two segments of text, there is no array needed. 
    /// </summary>
    public void UpdateIntroText()
    {
        introText.text = "Not everything is as it seems...";
    }
    
    /// <summary>
    /// This method makes sure the UI for the receptionist dialog is activated. 
    /// </summary>
    public void ActivateDialog()
    {
        // Activate the UI for the spoken text
        textBubbleImage.gameObject.SetActive(true);
        nameBoxText.gameObject.SetActive(true);
        dialogueAnimator.gameObject.SetActive(true);
        
        UpdateText();    // Update the text that is shown
        PauseTimeline(); // Pause the timeline such that the player can read the text. 
    }
    
    /// <summary>
    /// This method makes sure the UI of the receptionist dialog is deactivated. 
    /// </summary>
    public void DeactivateDialog()
    {
        textBubbleImage.gameObject.SetActive(false);
        nameBoxText.gameObject.SetActive(false);
        dialogueAnimator.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// This method updates the text that is shown when the receptionist speaks by using the spokenText array. 
    /// </summary>
    private void UpdateText()
    {
        try
        {
            dialogueAnimator.WriteDialogue(receptionistText[textIndex]);
        }
        catch
        {
            textIndex = 0;
        }
        textIndex++;
    }
    #endregion
    
    // This region contains all methods regarding the 'illusion' assignments of the prologue. 
    #region Illusions
    /// <summary>
    /// This method makes sure the grid illusion is shown. 
    /// </summary>
    public void ActivateGridIllusion()
    {
        imageToggler.gameObject.SetActive(true); // Only the toggler needs to be activated. The image is shown via the timeline. 
    }
    
    /// <summary>
    /// This method is called when the toggler is clicked. Depending on the value of the toggler isOn,
    /// a different image is shown. 
    /// </summary>
    public void OnToggleValueChanged(bool toggleIsOn)
    {
        imageToggler.isOn = toggleIsOn;
        checkmarkTransform.gameObject.SetActive(toggleIsOn);
        if (toggleIsOn)
        {
            illusionImage.sprite = sprites[3];
        }
        else
        {
            illusionImage.sprite = sprites[4];
        }
    }
    
    /// <summary>
    /// This method makes sure the cloud illusion is shown. 
    /// </summary>
    public void ActivateCloudIllusion()
    {
        illusionImage.sprite = sprites[5]; // Sprite 2 is the cloud illusion. 
    }
    #endregion
    
    /// <summary>
    /// This method changes the background by using the backgrounds array. 
    /// </summary>
    public void UpdateBackground()
    {
        backgroundIndex++;
        try
        {
            backgroundImage.sprite = sprites[backgroundIndex];
        }
        catch
        {
            backgroundImage.sprite = sprites[0];
        }
    }
    
    /// <summary>
    /// This method changes the sprite of the receptionist to show her emotions.
    /// </summary>
    public void ChangeReceptionist()
    {
        if (receptionistEmotion)
        {
            receptionistImage.sprite = sprites[7];
        }
        else
        {
            receptionistImage.sprite = sprites[6];
        }
        receptionistEmotion = !receptionistEmotion; 
    }
    
    /// <summary>
    /// This method is called when the timeline reaches the end of the prologue.
    /// When this method is called, the StorySelect scene is loaded. 
    /// </summary>
    public void LoadSelectStory()
    {
        SceneManager.LoadScene("StorySelectScene");
    }
}