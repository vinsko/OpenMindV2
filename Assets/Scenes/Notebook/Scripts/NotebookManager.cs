// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

/// <summary>
/// Manager class for the notebook scene.
/// </summary>
public class NotebookManager : MonoBehaviour
{
    private                TMP_InputField        characterCustomInput;
    private                TMP_InputField    personalCustomInput;
    private                CharacterInstance currentCharacter;
    private                int               currentCharacterIndex;
    private                int               currentPageIndex;
    private                Button            selectedButton;
    [NonSerialized] public NotebookData      notebookData;
    private Coroutine shoveAnimationCoroutine;
    private Coroutine fadeAnimationCoroutine;
    private bool isTransitioningNotebook = false;

    [Header("Settings")]
    [SerializeField] private float tabAnimationDuration;
    [SerializeField] private float shoveAnimationDuration;
    [SerializeField] private AnimationCurve shoveAnimationCurve;
    [SerializeField] private float expandedTabHeight;
    [SerializeField] private float collapsedTabHeight;
    [SerializeField] private string ownNotebookSwipeText;
    [SerializeField] private string otherNotebookSwipeText;

    [Header("Field References")]
    [SerializeField] private GameObject characterInfo;
    [SerializeField] private GameObject personalInfo;

    [Header("Tab Select Button References")]
    [SerializeField] private GameButton personalButton;
    [SerializeField] private GameButton[] nameButtons;

    [Header("Component References")]
    [SerializeField] private TMP_Text currentTabText;
    [SerializeField] private TMP_Text multiplayerInstructionText;
    [SerializeField] private RectTransform notebookTransform;
    [SerializeField] private Image backgroundImage;

    [Header("Prefab References")]
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] private GameObject inactiveNotePrefab;
    [SerializeField] private GameObject introObjectPrefab;
    [SerializeField] private GameObject inputObjectPrefab;
    [SerializeField] private GameObject logObjectPrefab;
    
    private bool showingMultiplayerNotebook;
    private bool justSwitchedBetweenNormalAndMultiplayerNotebook;
    private bool isClosing = false;

    private SwipeDetector swipeDetector;
        
    [SerializeField] private GameObject titleObjectPrefab;

    /// <summary>
    /// On startup, go to the personal notes and make sure the correct data is shown
    /// </summary>
    private void Start()
    {
        InitializeTabButtons();
        notebookData = GameManager.gm.notebookData;
        
        // Create personal notes
        CreatePersonalPage();

        // Open custom notes page
        OpenPersonalNotes();

        // Add listener to recreate tab when font size is changed
        SettingsManager.sm.OnTextSizeChanged.AddListener(OnTextSizeChanged);

        // Animate notebook moving in
        float startPos = Screen.width / 2 + notebookTransform.rect.width / 2;
        ShoveAnimation(startPos, 0);
        FadeAnimation(true);

        if (GameManager.gm.multiplayerEpilogue)
        {
            // Get swipe detector & add listeners
            swipeDetector = GetComponent<SwipeDetector>();
            swipeDetector.OnSwipeLeft.AddListener(OpenOwnNotebook);
            swipeDetector.OnSwipeRight.AddListener(OpenOtherNotebook);

            multiplayerInstructionText.transform.parent.gameObject.SetActive(true);
            multiplayerInstructionText.text = ownNotebookSwipeText;
    }
        else
        {
            multiplayerInstructionText.transform.parent.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Opens own notebook if it is not yet open
    /// </summary>
    private void OpenOwnNotebook()
    {
        if (!showingMultiplayerNotebook || isTransitioningNotebook)
            return;

        StartCoroutine(SwitchNotebooks());
    }

    /// <summary>
    /// Opens other person's notebook if it is not yet open
    /// </summary>
    private void OpenOtherNotebook()
    {
        if (showingMultiplayerNotebook || isTransitioningNotebook)
            return;

        StartCoroutine(SwitchNotebooks());
    }

    /// <summary>
    /// A coroutine which closes the current notebook & opens the other.
    /// </summary>
    private IEnumerator SwitchNotebooks()
    {
        isTransitioningNotebook = true;

        float screenEdge = Screen.width / 2 + notebookTransform.rect.width / 2;
        float endPos = showingMultiplayerNotebook ? -screenEdge : screenEdge;
        ShoveAnimation(0, endPos);
        
        // Wait for animation to finish
        yield return new WaitForSeconds(shoveAnimationDuration);

        // If the notebook is in the process of closing, don't do anything more
        if (isClosing)
            yield break;

        // Switch notebook data
        ToggleMultiplayerNotebook();

        ShoveAnimation(-endPos, 0);

        yield return new WaitForSeconds(shoveAnimationDuration);
        isTransitioningNotebook = false;
    }

    /// <summary>
    /// Switches between whatever notebook is currently on the screen.
    /// </summary>
    public void ToggleMultiplayerNotebook()
    {
        justSwitchedBetweenNormalAndMultiplayerNotebook = true;
        if (showingMultiplayerNotebook)
        {
            showingMultiplayerNotebook = false;
            
            notebookData = GameManager.gm.notebookData;
            
            // Open personal notes and update the text
            personalCustomInput.text = notebookData.GetPersonalNotes();
            multiplayerInstructionText.text = ownNotebookSwipeText;
            
            // Add listener to recreate tab when font size is changed
            SettingsManager.sm.OnTextSizeChanged.AddListener(OnTextSizeChanged);
        }
        else
        {
            showingMultiplayerNotebook = true; 
            multiplayerInstructionText.text = otherNotebookSwipeText;
            if (GameManager.gm.multiplayerNotebookData != null)
            {
                notebookData = GameManager.gm.multiplayerNotebookData;
                
                // Open personal notes and update the text
                personalCustomInput.text = notebookData.GetPersonalNotes();                

                // Add listener to recreate tab when font size is changed
                SettingsManager.sm.OnTextSizeChanged.AddListener(OnTextSizeChanged);
            }
            else
            {
                Debug.LogError("Tried to open multiplayer notebook, but no such multiplayer notebook exists.");
            }
        }

        OpenPersonalNotes();

        justSwitchedBetweenNormalAndMultiplayerNotebook = false;
    }

    /// <summary>
    /// Initialize the tab buttons (custom notes & character tabs), 
    /// For characters, use their names as the button text and add the button event.
    /// </summary>
    public void InitializeTabButtons()
    {
        // Initialise all buttons for which there are characters
        var characters = GameManager.gm.currentCharacters;
        for (int i = 0; i < characters.Count; i++)
        {
            int id = i;
            var button = nameButtons[i];
            
            // Set the icon avatar
            var icon = button.GetComponentInChildren<CharacterIcon>();
            icon.SetAvatar(characters[i]);

            // Inactive characters should have a different looking icon
            if (!characters[i].isActive)
            {
                icon.BackgroundColor = new Color(0.7f, 0.7f, 0.7f);
                icon.OverlayColor = new Color(0.7f, 0.7f, 0.7f);
            }

            button.onClick.AddListener(()=>OpenCharacterTab(id));
        }

        // Set any remaining buttons to inactive
        for (int i = characters.Count; i < nameButtons.Length; i++)
        {
            nameButtons[i].gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Open the personal notes tab and load the notes.
    /// </summary>
    public void OpenPersonalNotes()
    {
        // An id of -1 signifies the custom notes tab
        currentCharacterIndex = -1;

        // Close the character tab 
        characterInfo.SetActive(false);

        // Activate written personal notes  
        personalInfo.SetActive(true);

        // Make button clickable
        ChangeButtons(personalButton);
        
        // Set the appropriate footer text
        currentTabText.text = "Personal Notes";
    }
    
    /// <summary>
    /// Create the personal notes page.
    /// The page consists of the "personal notes" title and the inputfield.
    /// </summary>
    private void CreatePersonalPage()
    {
        Queue<GameObject> allPersonalInfo = new();
        
        // Create personal notes title object  
        var titleObject = Instantiate(titleObjectPrefab);  
        titleObject.GetComponent<NotebookTitleObject>().SetInfo("Personal Notes"); 
        
        allPersonalInfo.Enqueue(titleObject);
  
        // Create the custom input field object  
        var inputObject = Instantiate(inputObjectPrefab);  
        var inputObjectField = inputObject.GetComponent<TMP_InputField>();

        // Set the saved text to the notebook
        inputObjectField.text = notebookData.GetPersonalNotes();
        inputObjectField.onSelect.AddListener(_ => inputObjectField.MoveTextEnd(false));
        inputObjectField.placeholder.GetComponentInChildren<TMP_Text>().text
            = "Write down your thoughts!";
        
        allPersonalInfo.Enqueue(inputObject);
        
        inputObjectField.onEndEdit.AddListener(_ => SavePersonalData());  
  
        inputObjectField.pointSize = SettingsManager.sm.GetFontSize() * SettingsManager.M_SMALL_TEXT;
        inputObjectField.interactable = !GameManager.gm.multiplayerEpilogue;
        personalCustomInput = inputObjectField; // Also set the reference so that it can be saved  
        
        // Create the page using the allpersonalinfo queue
        CreatePage(allPersonalInfo, personalInfo);
    }
    
    /// <summary>
    /// Open a character tab and load and display the notes on that character.
    /// </summary>
    private void OpenCharacterTab(int id)
    {
        // If id is out of bounds, open personal notes
        if (id < 0 || id >= GameManager.gm.currentCharacters.Count)
        {
            OpenPersonalNotes();
            return;
        }

        currentCharacterIndex = id;
        
        // Destroy info from the previous character
        foreach (Transform page in characterInfo.transform)
            Destroy(page.gameObject);

        // Deactivate the personal notes tab if it's opened
        if (personalInfo.gameObject.activeInHierarchy)
            personalInfo.SetActive(false);
        
        // Activate written character notes
        characterInfo.SetActive(true);

        // Get the character instance
        currentCharacter = GameManager.gm.currentCharacters[id];

        // The queue which will hold all the character's info
        Queue<GameObject> allCharacterInfo = new();

        // If character is inactive, create note object
        if (!currentCharacter.isActive)
        {
            var inactiveNoteObject = Instantiate(inactiveNotePrefab);
            var noteText = inactiveNoteObject.GetComponentInChildren<TMP_Text>();

            noteText.fontSize = SettingsManager.sm.GetFontSize() * SettingsManager.M_SMALL_TEXT;
            noteText.text = $"Note: {currentCharacter.characterName} {GameManager.gm.story.victimDialogue}";

            allCharacterInfo.Enqueue(inactiveNoteObject);
        }

        // Create icon & name object
        var introObject = Instantiate(introObjectPrefab);
        introObject.GetComponent<NotebookCharacterObject>().SetInfo(currentCharacter);
        allCharacterInfo.Enqueue(introObject);

        // Create the custom input field object
        var inputObject = Instantiate(inputObjectPrefab);
        var inputObjectField = inputObject.GetComponent<TMP_InputField>();
        
        // Set the notebook text
        inputObjectField.text = notebookData.GetCharacterNotes(currentCharacter);
        inputObjectField.onSelect.AddListener(_ => inputObjectField.MoveTextEnd(false)); // On select, move caret to end of text
        inputObjectField.interactable = !GameManager.gm.multiplayerEpilogue; // Disable editing in multiplayer epilogue
        inputObjectField.placeholder.GetComponentInChildren<TMP_Text>().text
            = notebookData.GetCharacterPlaceholder(currentCharacter);
        
        inputObjectField.onValueChanged.AddListener(_ => SaveCharacterData());
        // Save notes on end edit
        inputObjectField.onEndEdit.AddListener(_ => SaveCharacterData());
        
        inputObjectField.pointSize = SettingsManager.sm.GetFontSize() * SettingsManager.M_SMALL_TEXT;
        characterCustomInput = inputObjectField; // Also set the reference so that it can be saved
        allCharacterInfo.Enqueue(inputObject);

        CreatePage(allCharacterInfo, characterInfo);

        // Make button clickable
        ChangeButtons(nameButtons[id]);

        // Set appropriate footer text
        currentTabText.text = 
            currentCharacter.characterName;
    }

    /// <summary>
    /// Navigates to the page which is <paramref name="direction"/> pages away
    /// from the current page. Will navigate to the previous/next character tab
    /// if the page index is not reachable.
    /// </summary>
    public void NavigatePages(int direction)
    {
        int newIndex = currentPageIndex + direction;

        if (newIndex < 0)
        {
            // The index is less than 0, so navigate to previous character
            NavigateCharacters(currentCharacterIndex - 1);
        }
        else
        {
            // Navigate to next character
            NavigateCharacters(currentCharacterIndex + 1);
        }
    }

    /// <summary>
    /// Navigates to the character tab with the given id.
    /// </summary>
    private void NavigateCharacters(int id)
    {
        // Set the id so that we remain within the correct bounds
        int characterCount = GameManager.gm.currentCharacters.Count;
        if (id > characterCount)
            id = -1;
        else if (id < -1)
            id = characterCount - 1;

        OpenCharacterTab(id);
    }

    /// <summary>
    /// Create a page from the queue. Creates multiple GameObjects each containing
    /// a part of the notebook data regarding the character/ personal notes.
    /// </summary>
    private void CreatePage(Queue<GameObject> dataQueue, GameObject panel)
    {
        // Create the page
        var page = Instantiate(pagePrefab, panel.transform);

        // While there are still notebook objects to be placed
        while (dataQueue.Count > 0)
        {
            // Dequeue an object
            var go = dataQueue.Dequeue();

            // Set its parent with a vertical layout group component
            go.GetComponent<RectTransform>().SetParent(page.transform, false);

            // Force rebuild the layout so the height values are correct
            LayoutRebuilder.ForceRebuildLayoutImmediate(page.GetComponent<RectTransform>());
        }
    }

    /// <summary>
    /// Save the notes of the character inputfield to the notebookdata.
    /// </summary>
    public void SaveCharacterData()
    {
        if (justSwitchedBetweenNormalAndMultiplayerNotebook)
        {
            return;
        }
        // Save the written character text to the notebook data
        notebookData.UpdateCharacterNotes(currentCharacter, 
            characterCustomInput.GetComponent<TMP_InputField>().text);
    }

    /// <summary>
    /// Save the notes of the personal inputfield to the notebookdata.
    /// </summary>
    public void SavePersonalData()
    {
        if (justSwitchedBetweenNormalAndMultiplayerNotebook)
        {
            return;
        }
        // Save the written character text to the notebook data
        notebookData.UpdatePersonalNotes(personalCustomInput.GetComponent<TMP_InputField>().text);
    }
    
    /// <summary>
    /// Make the clicked button non-interactable and make the last clicked buttons interactable again.
    /// </summary>
    private void ChangeButtons(Button clickedButton)
    {
        if (selectedButton != null)
        {
            selectedButton.interactable = true;

            // Collapse the previously clicked button
            selectedButton.GetComponent<NotebookTabButton>().AnimateTab(
                collapsedTabHeight, tabAnimationDuration);
        }

        // Expand the clicked button
        clickedButton.GetComponent<NotebookTabButton>().AnimateTab(
            expandedTabHeight, tabAnimationDuration);

        selectedButton = clickedButton;
        selectedButton.interactable = false;
    }

    /// <summary>
    /// What happens when the player changes text size settings.
    /// Resets current page and apply new values.
    /// </summary>
    private void OnTextSizeChanged()
    {
        // Reopen character tab (automatically applies settings)
        OpenCharacterTab(currentCharacterIndex);
    }

    /// <summary>
    /// Animates notebook moving from the <paramref name="startPos"/> to the <paramref name="endPos"/>.
    /// </summary>
    /// <param name="startPos">The position from which the animation will start.</param>
    /// <param name="endPos">The position in which the animation will end.</param>
    /// <param name="fadingIn">Is the background going to fade in our out?</param>
    /// <param name="tcs">The TaskCompletionSource to define when the animation is complete.</param>
    public void ShoveAnimation(float startPos, float endPos, TaskCompletionSource<bool> tcs = null)
    {
        if (shoveAnimationCoroutine != null) 
            StopCoroutine(shoveAnimationCoroutine);

        shoveAnimationCoroutine = StartCoroutine(AnimateNotebook(startPos, endPos, tcs));
    }

    public void FadeAnimation(bool fadingIn)
    {
        if (fadeAnimationCoroutine != null)
            StopCoroutine(fadeAnimationCoroutine);

        fadeAnimationCoroutine = StartCoroutine(AnimateBackground(fadingIn));
    }

    /// <summary>
    /// Animates notebook shoving out of the screen. 
    /// Has a TaskCompletionSource so that it is awaitable.
    /// </summary>
    public Task ShoveOutNotebook()
    {
        var tcs = new TaskCompletionSource<bool>();
        isClosing = true;

        ShoveAnimation(
            notebookTransform.localPosition.x, 
            Screen.width / 2 + notebookTransform.rect.width / 2,
            tcs);

        FadeAnimation(false);

        return tcs.Task;
    }

    /// <summary>
    /// The coroutine which animates the notebook moving horizontally.
    /// This coroutine shouldn't be started directly, instead, call <see cref="ShoveAnimation(float, float, TaskCompletionSource{bool})"/>
    /// </summary>
    private IEnumerator AnimateNotebook(float startPos, float endPos, TaskCompletionSource<bool> tcs)
    {
        float time = 0;
        while (time < shoveAnimationDuration)
        {
            time += Time.deltaTime;

            // Move notebook
            float timeStep = shoveAnimationCurve.Evaluate(time / shoveAnimationDuration);
            float x = Mathf.Lerp(startPos, endPos, timeStep);
            notebookTransform.localPosition = new Vector2(x, notebookTransform.localPosition.y);

            yield return null;
        }

        notebookTransform.localPosition = new Vector2(endPos, notebookTransform.localPosition.y);
        tcs?.SetResult(true);
    }

    private IEnumerator AnimateBackground(bool fadingIn)
    {
        float time = 0;
        while (time < shoveAnimationDuration)
        {
            time += Time.deltaTime;
            float timeStep = time / shoveAnimationDuration;

            // Fade background
            var color = backgroundImage.color;
            color.a = fadingIn ? Mathf.Lerp(0, 0.9f, timeStep) : Mathf.Lerp(0.9f, 0, timeStep);
            backgroundImage.color = color;

            yield return null;
        }
    }

    #region Test Variables
#if UNITY_INCLUDE_TESTS
    public TMP_InputField Test_PersonalInputField { get { return personalCustomInput; } }
    public Button Test_GetPersonalButton() => personalButton;
    public Button[] Test_GetNameButtons() => nameButtons;
    public GameObject Test_CharacterInfoField { get { return characterInfo; } }
    #endif
    #endregion
}
