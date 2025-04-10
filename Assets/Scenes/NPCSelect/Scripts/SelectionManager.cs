// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager class for the NPCSelect scene.
/// </summary>
public class SelectionManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float buttonFadeDuration;
    [SerializeField] private float scrollDuration;

    [Header("Prefabs")]
    [SerializeField] private GameObject optionPrefab;

    [Header("References")]
    [SerializeField] private GameButton confirmSelectionButton;
    [SerializeField] private NPCSelectScroller scroller;
    [SerializeField] private TextMeshProUGUI headerText;

    [Header("Events")]
    [SerializeField] private GameEvent stopLoadIcon;

    private Coroutine fadeCoroutine;

    private Transform scrollable;
    private Transform layout;

    /// <summary>
    /// On startup, set the selectionType of the scene, set the headertext and generate the selectable options.
    /// </summary>
    private void Awake()
    {
        // Change the text size
        confirmSelectionButton.GetComponentInChildren<TMP_Text>().enableAutoSizing = true;
        headerText.GetComponentInChildren<TMP_Text>().enableAutoSizing = true;
        ChangeTextSize();
        
        // stop loading animation (if it is playing)
        stopLoadIcon.Raise(this);

        scrollable = scroller.transform.GetChild(0);
        layout = scrollable.GetChild(0);

        SetHeaderText();
        GenerateOptions();
        
        scroller.OnCharacterSelected.AddListener(EnableSelectionButton);
        scroller.NoCharacterSelected.AddListener(DisableSelectionButton);
        scroller.scrollDuration = scrollDuration;
    }
    
    #region TextSize

    /// <summary>
    /// Change the fontSize of the tmp_text components when a different textSize is chosen in the settings menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="data"></param>
    // TODO: could be made private.
    public void OnChangedTextSize(Component sender, params object[] data)
    {
        // Set the fontSize.
        if (data[0] is int fontSize)
        {
            // Change the fontSize of the confirmSelectionButton
            confirmSelectionButton.GetComponentInChildren<TMP_Text>().fontSizeMax = fontSize;
            // Change the fontSize of the headerText
            headerText.GetComponentInChildren<TMP_Text>().fontSizeMax = fontSize;
        }
    }
    
    /// <summary>
    /// Change the fontSize of the tmp_text components
    /// </summary>
    // TODO: could be made private.
    public void ChangeTextSize()
    {
        int fontSize = SettingsManager.sm.GetFontSize();
        // Change the fontSize of the confirmSelectionButton
        confirmSelectionButton.GetComponentInChildren<TMP_Text>().fontSizeMax = fontSize;
        
        // Change the fontSize of the headerText
        headerText.GetComponentInChildren<TMP_Text>().fontSizeMax = fontSize;
    }

    #endregion

  
    /// <summary>
    /// Change the Header text if the culprit needs to be chosen.
    /// </summary>
    /// <param name="sceneType"> Can take "dialogue" or "decidecriminal" as value. </param>
    private void SetHeaderText()
    {
        // TODO: There must be a more efficient way to do this, using a 'NPCIntroduction'-gamestate
        if (GameManager.gm.AmountCharactersGreeted() < GameManager.gm.currentCharacters.Count)
            UpdatePeopleGreeted();
        else 
            headerText.text = "Who do you want to talk to?";
    }
    
    /// <summary>
    /// Generates the selectOption objects for the characters.
    /// </summary>
    private void GenerateOptions()
    {
        // Create a SelectOption object for every character in currentCharacters.
        for (int i = 0; i < GameManager.gm.currentCharacters.Count; i++)
        {
            var character = GameManager.gm.currentCharacters[i];

            // Create a new SelectOption object.
            SelectOption newOption = Instantiate(optionPrefab).GetComponent<SelectOption>();
            newOption.character = character;

            // Set the parent & position of the object
            newOption.transform.SetParent(layout.GetChild(i), false);
            newOption.transform.position = newOption.transform.parent.position;
        }
    }


    #region Greetings
    /// <summary>
    /// Called every time NPCSelectScene opens (and this script awakens).
    /// It updates the header text with the amount of NPCs that have been greeted.
    /// If all NPCs have been greeted, change gamestate.
    /// </summary>
    public void UpdatePeopleGreeted()
    {
        int currentGreeted = GameManager.gm.AmountCharactersGreeted();
        int total = GameManager.gm.currentCharacters.Count;
        headerText.text = $"People greeted: {currentGreeted}/{total}";
        if (currentGreeted == total)
            GameManager.gm.gameState = GameManager.GameState.NpcSelect;
    }

    #endregion

    #region Selection Button Logic
    /// <summary>
    /// Event for when a character is selected.
    /// </summary>
    /// <param name="option"> The character space on which has been clicked on. </param>
    /// TODO: Add an intermediate choice for the culprit. (if everyone agrees with the storyline epilogue)
    public void SelectionButtonClicked(CharacterInstance character)
    {
        // Only active characters can be talked to.
        if (!character.isActive)
            return;

        // No special gamestate, so we start dialogue with the given character
        GameManager.gm.StartDialogue(character);
    }

    /// <summary>
    /// Enables the character selection button & sets it to the selected character.
    /// </summary>
    private void EnableSelectionButton()
    {
        var button = confirmSelectionButton;

        // Set appropriate text whether or not selected character is active
        var text = button.GetComponentInChildren<TMP_Text>();
        string characterName = scroller.SelectedCharacter.characterName;

        // Button text should not be interactable if character is not active
        if (scroller.SelectedCharacter.isActive)
        {
            button.interactable = true;

            // Set the button text depending on the gameState
            text.text = GameManager.gm.gameState == GameManager.GameState.CulpritSelect ?
                    $"Approach {characterName}" : $"Talk to {characterName}";
        }
        else
        {
            button.interactable = false;
            text.text = $"{characterName} {GameManager.gm.story.victimDialogue}";
        }
        
        // Add appropriate "start dialogue" button for selected character
        button.onClick.AddListener(() => SelectionButtonClicked(scroller.SelectedCharacter));

        // Fade the button in
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeIn(button.GetComponent<CanvasGroup>(), buttonFadeDuration));
    }

    /// <summary>
    /// Disables the character selection button & removes its listeners.
    /// </summary>
    private void DisableSelectionButton()
    {
        var button = confirmSelectionButton;
        button.onClick.RemoveAllListeners();

        // Fade the button out
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOut(button.GetComponent<CanvasGroup>(), buttonFadeDuration));
    }
    #endregion

    #region Fade Logic
    /// <summary>
    /// Fades an object in using a canvas group.
    /// </summary>
    /// <param name="cg">The canvas group of the object to be faded</param>
    /// <param name="duration">The duration of the fade</param>
    private IEnumerator FadeIn(CanvasGroup cg, float duration)
    {
        // First, set the given gameObject to active
        cg.gameObject.SetActive(true);

        // Make the object interactable in advance to make it more responsive
        cg.interactable = true;

        float startingAlpha = cg.alpha;
        float time = 0;
        while (time <= duration)
        {
            time += Time.deltaTime;

            // Use lerp to interpolate the fade
            cg.alpha = Mathf.Lerp(startingAlpha, 1, time / duration);

            yield return null;
        }
    }

    /// <summary>
    /// Fades an object out using a canvas group.
    /// </summary>
    /// <param name="cg">The canvas group of the object to be faded</param>
    /// <param name="duration">The duration of the fade</param>
    private IEnumerator FadeOut(CanvasGroup cg, float duration)
    {
        // Make the object non-interactable to prevent player mistakes
        cg.interactable = true;

        float startingAlpha = cg.alpha;
        float time = 0;
        while (time <= duration)
        {
            time += Time.deltaTime;

            // Use lerp to interpolate the fade
            cg.alpha = Mathf.Lerp(startingAlpha, 0, time / duration);

            yield return null;
        }

        // Finally, disable the given gameObject
        cg.gameObject.SetActive(false);
    }
    #endregion

    #region Test Variables
    #if UNITY_INCLUDE_TESTS

    public void FadeIn_Test(CanvasGroup cg, float duration) => StartCoroutine(FadeIn(cg, duration));
    public void FadeOut_Test(CanvasGroup cg, float duration) => StartCoroutine(FadeOut(cg, duration));

    public GameButton Test_GetSelectionButtonRef() => confirmSelectionButton;

    #endif
    #endregion
}
