// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] public PlayableDirector TutorialTimeline;
    
    [SerializeField] public  Button     continueButton;
    [SerializeField] private GameObject notebookHighlight;
    [SerializeField] private GameObject helpHighlight;
    
    // Variables for the tutorial text
    [SerializeField] private Image    textBox;      // Background of the text. 
    [SerializeField] public string[] tutorialText;  // Contains the text that will be shown. 
    [SerializeField] public TMP_Text text;          // The gameobject that will show the text on the screen. 
    private int textIndex;                          // Keeps track of which text to show.  
    
    // Variables for showing the objective of the game. 
    [SerializeField] public string[] objectives;
    [SerializeField] private Image    objectiveBox;
    [SerializeField] public TMP_Text objectiveText;
    
    private Button tutorialButton;
    private Button notebookButton;
    
    /// <summary>
    /// This method is called when the tutorial is started. It decides which hint needs to be shown. 
    /// </summary>
    private void Start()
    {
        ResetTutorial(); // Make sure all hints start with a clean slate. 
        
        // The type of tutorial that is shown, depends on the scene that is currently loaded.
        if (SceneManager.GetSceneByName("NotebookScene").isLoaded) // Since the notebook is loaded on top of another scene, this check has to come first. 
        {
            NotebookHint();
        }
        else if (SceneManager.GetSceneByName("DialogueScene").isLoaded)
        {
            DialogueHint();
        }
        else if (SceneManager.GetSceneByName("EpilogueScene").isLoaded)
        {
            EpilogueHint();
        }
        else // In all other cases (namely the NPCSelectScene) the actual tutorial needs to be played. 
        {
            ActivateTutorial();
        }
    }
    
    // This region contains the methods that are called in the different scenes. 
    #region Hint methods
    /// <summary>
    /// This method is responsible for activating a hint. The content of the hint is determined by the methods below. 
    /// </summary>
    private void ActivateHint()
    {
        tutorialButton.enabled = true;
        textBox.gameObject.SetActive(true);
        // Make sure the hint can be closed by tapping the screen
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.AddListener(StopTutorial); // Add the stop tutorial feature from the continue button
    }
    
    /// <summary>
    /// This method is responsible for showing the hint during the Dialogue scene. 
    /// </summary>
    private void DialogueHint()
    {
        // First try to determine the gamestate to distinguish between different types of dialogue. 
        try
        {
            GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
            switch (gm.gameState)
            {
                case GameManager.GameState.NpcDialogue:
                    text.text = "Ask this person a question to find out if they are the culprit!";
                    break;
                case GameManager.GameState.HintDialogue:
                    text.text = "Try to match the hint to a character in the game.";
                    break;
                case GameManager.GameState.Epilogue:
                    text.text = "Did you choose the right character?";
                    break;
            }
        }
        catch
        {
            // At the end of the game, the gamemanager does not exist anymore, so there is no gamestate.
            // In that case you are confronting the culprit. 
            text.text = "Did you find the culprit?";
        }
        ActivateHint();
    }
    
    /// <summary>
    /// This method is responsible for showing the hint during the Epilogue scene. 
    /// </summary>
    private void EpilogueHint()
    {
        try
        {
            EpilogueManager em = GameObject.Find("EpilogueManager").GetComponent<EpilogueManager>();
            switch (em.story.storyID)
            {
                case 0: // The mysterious contact
                    text.text =
                        "Select the person you think is sending you messages. When in doubt, consult your notebook.";
                    break;
                case 1: // The psychic sidekick
                    text.text =
                        "Select the person you think is responsible for the disappearances. When in doubt, consult your notebook.";
                    break;
                case 2: // The stolen paper
                    text.text =
                        "Select the person you think is the rightful author. When in doubt, consult your notebook.";
                    break;
            }
        }
        catch
        {
            text.text = "Select the character you think is the culprit";
            Debug.LogError("Unable to detect which story is currently played.");
        }
        
        ActivateHint();
    }
    
    /// <summary>
    /// This method is responsible for showing the hint during the Notebook scene. 
    /// </summary>
    private void NotebookHint()
    {
        // The hint the player should receive during this scene. 
        text.text = "In your Notebook you can write down gathered information.";
        ActivateHint();
    }
    #endregion
    
    // This region contains methods regarding the tutorial in the NPCSelect scene.
    #region Tutorial
    /// <summary>
    /// This method resets the tutorial to the beginning state (useful for when tutorial is already played) 
    /// and initializes necessary components. 
    /// </summary>
    private void ResetTutorial()
    {
        // Make sure nothing from 'previous' tutorial is active. 
        helpHighlight.SetActive(false);
        textBox.gameObject.SetActive(false);
        TutorialTimeline.Pause();
        
        // Initialize the tutorial button
        GameObject tutorial = GameObject.Find("HelpButton");
        tutorialButton = tutorial.GetComponentInChildren<Button>();
        
        // When the player watches the tutorial for the first time, it cannot be skipped. 
        if (!FetchUserData.Loader.GetUserDataValue(FetchUserData.UserDataQuery.playedBefore))
        {
            tutorialButton.enabled = false;
        }
        
        // Remove the stop tutorial feature from the continue button for the duration of the tutorial.
        continueButton.onClick.RemoveListener(StopTutorial);
    }
    
    /// <summary>
    /// This method initializes necessary variables for the tutorial and then activates the tutorial. 
    /// </summary>
    private void ActivateTutorial()
    {
        // Initialize the notebook for the notebook tutorial
        GameObject notebook = GameObject.Find("Notebook Button");
        
        try
        {
            notebookButton = notebook.GetComponentInChildren<GameButton>();
        }
        catch
        {
            Debug.LogError("There is no notebookbutton");
        }
        
        // Close notebook if it is already opened.
        if (SceneManager.GetSceneByName("NotebookScene").isLoaded)
        {
            notebookButton.onClick.Invoke();
        }
        
        notebookButton.enabled = false;                             // Make sure the notebook can not be (de)activated during the tutorial. 
        tutorialButton.onClick.AddListener(EnableNotebookButton);   // When the tutorial is stopped, the notebook button should regain normal functionality. 
        
        // Start the tutorial
        StartTutorial();
    }
    
    /// <summary>
    /// This method is called when the help button is clicked.
    /// It makes sure game elements enter a 'tutorial mode'. 
    /// </summary>
    public void StartTutorial()
    {
        TutorialTimeline.time = 0;  // When the button is clicked again, the tutorial has to play from the start. 
        textIndex = 0;
        TutorialTimeline.Play();
    }
    
    /// <summary>
    /// This method resumes the tutorial timeline when it is paused. 
    /// </summary>
    public void PlayTutorial()
    {
        TutorialTimeline.Play();
        continueButton.gameObject.SetActive(false); // Continue button is not necessary now. 
    }
    
    /// <summary>
    /// This method pauses the tutorial timeline when it is playing. 
    /// </summary>
    public void PauseTutorial()
    {
        TutorialTimeline.Pause();
        continueButton.gameObject.SetActive(true); // Activate continue button such that the scene can be continued. 
    }
    
    /// <summary>
    /// This method closes the tutorial scene. 
    /// </summary>
    public void StopTutorial()
    {
        tutorialButton.enabled = true;
        tutorialButton.onClick.Invoke(); // Clicking the tutorial button again closes the scene. 
    }
    
    
    /// <summary>
    /// This method activates the help button highlight
    /// </summary>
    public void HighlightHelp()
    {
        helpHighlight.gameObject.SetActive(true);
    }
    #endregion
    
    // This region contains methods regarding the part of the tutorial where the notebook is involved.
    #region NotebookTutorial
    /// <summary>
    /// This method activates the part of the tutorial that takes place in the notebook. 
    /// </summary>
    public void ActivateNotebookTutorial()
    {
        PauseTutorial();
        UpdateTutorialText();
        notebookHighlight.SetActive(true); // Indicate that the player has to open the notebook. 
        notebookButton.enabled = true;     // Make sure player can open the notebook. 
        
        // Make sure the player has to open the notebook. 
        continueButton.gameObject.SetActive(false);                     // Only the notebook button can be clicked. 
        notebookButton.onClick.AddListener(UpdateTutorialText);         // Temporarily add the ability to update the text to the notebookbutton. 
        notebookButton.onClick.AddListener(DisableNotebookHighlight);   // Temporarily add the ability to deactivate the highlight to the notebookbutton. 
    }
    
    /// <summary>
    /// This method deactivates the tutorial in the notebook. 
    /// </summary>
    private void DeactivateNotebookTutorial()
    {
        notebookButton.enabled = false;                 // Make sure notebook can not be (de)activated during the tutorial. 
        notebookButton.onClick
            .RemoveListener(UpdateTutorialText);        // Remove the ability to update the text
        notebookButton.onClick
            .RemoveListener(DisableNotebookHighlight);  // Remove the ability to deactivate the notebook highlight
        continueButton.gameObject.SetActive(true);      // Make sure the player can tap the screen to continue again. 
    }
    
    /// <summary>
    /// Method that deactivates the notebook highlight.
    /// </summary>
    private void DisableNotebookHighlight()
    {
        notebookHighlight.SetActive(false);
    }
    
    /// <summary>
    /// This method (re)activates the notebook button. 
    /// </summary>
    private void EnableNotebookButton()
    {
        notebookButton.enabled = true; 
    }
    
    #endregion
    
    // This region contains methods regarding the text that is shown during the tutorial. 
    #region TutorialText
    
    /// <summary>
    /// This method is called after the player clicks the screen such that new text will be shown.
    /// This method is for the normal tutorial, not for the notebook tutorial. 
    /// </summary>
    public void UpdateTutorialText()
    {
        PauseTutorial();                            // Pause timeline such that player has time to read the text. 
        objectiveBox.gameObject.SetActive(false);   // Hide objective. 
        DeactivateNotebookTutorial();               // Make sure normal functionality is regained. 
        // Update the tutorial text. 
        try
        {
            text.text = tutorialText[textIndex];
        }
        catch
        {
            Debug.LogError("No more tutorial text.");
            text.text = tutorialText[0];
            textIndex = 0;
        }
        // Make sure text is visible. 
        textBox.gameObject.SetActive(true);
        text.gameObject.SetActive(true);
        
        textIndex++; // Increase textindex in order to prepare for the next text. 
    }
    
    /// <summary>
    /// This method is called at the start of the tutorial.
    /// When the player has already played one cycle, the objective does not need to be shown again. 
    /// </summary>
    public void ShowObjective()
    {
        text.gameObject.SetActive(false);                              // Make sure the tutorialtext is hidden. 
        PauseTutorial();                                               // Pause the tutorial, such that the player can read the objective. 
        objectiveText.text = objectives[GameManager.gm.story.storyID]; // Objective text should be that of the current story. 
        objectiveBox.gameObject.SetActive(true);                       // Show objective. 
    }
    
    #endregion
   }