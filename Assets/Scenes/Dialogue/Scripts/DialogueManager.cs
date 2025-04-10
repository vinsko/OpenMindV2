// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// The manager for the dialogue scene
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset customFont;

    [Header("Dialogue animator reference")]
    [SerializeField] private DialogueAnimator animator;

    [Header("Fields")]
    [SerializeField] private GameObject dialogueField;
    [SerializeField] private GameObject imageField;
    [SerializeField] private GameObject questionsField;
    [SerializeField] private GameObject inputField;
    [SerializeField] private GameObject backgroundField;
    [SerializeField] private GameObject characterNameField;
    [SerializeField] private GameObject phoneField;

    [Header("Prefabs")]
    [SerializeField] private EventSystem eventSystemPrefab;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject avatarPrefab; // A prefab containing a character
    [SerializeField] private GameObject phoneDialogueBoxPrefab;

    [Header("Events")]
    public GameEvent onEndDialogue;
    public UnityEvent onEpilogueEnd;
    public GameEvent stopLoadIcon;

    [Header("Miscellaneous")]
    [SerializeField] private AudioClip phoneNotificationClip;
    [SerializeField] private float phoneAnimationDuration = 1f;
        
    [FormerlySerializedAs("testDialogueObject")]
    [Header("Test variables")]
    [SerializeField] private DialogueContainer testDialogueContainer;
    
    // Variables
    [NonSerialized] public        string            inputText;
    [NonSerialized] public        List<string>      playerAnswers;
    [NonSerialized] public static DialogueManager   dm;
    [NonSerialized] public        CharacterInstance currentRecipient;
    [NonSerialized] public        DialogueObject    currentObject;

    private bool useRecipient = true;
    private string alternateName;
    
    // In this awake, we initialize some components in case it is loaded in isolation.
    // It does not need to rely on GameManager to be active, but it needs an eventsystem
    private void Awake()
    {
        if (GameObject.Find("EventSystem") == null)
        {
            Instantiate(eventSystemPrefab);
            StartDialogue(null, testDialogueContainer.GetDialogue());
        }

        SettingsManager.sm.OnTextSizeChanged.AddListener(OnTextSizeChanged);

        // Set static DialogueManager instance
        dm = this;
    }

    /// <summary>
    /// Sets DialogueManager variables (currentObject & dialogueRecipient) and executes the starting DialogueObject.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="data">Should be an array where element 0 is the dialogue recipient, 
    /// and element 1 is the starting dialogue object.</param>
    public void StartDialogue(Component sender, params object[] data)
    {        
        // Change the text size
        characterNameField.GetComponentInChildren<TMP_Text>().enableAutoSizing = true;
        
        // Retrieve and set the dialogue object
        if (data[0] is DialogueObject dialogueObject)
        {
            currentObject = dialogueObject;
        }
        // Retrieve and set the dialogue recipient (if given)
        if (data.Length > 1 && data[1] is CharacterInstance recipient)
        {
            currentRecipient = recipient;
            characterNameField.SetActive(true);
        }
        if (data.Length > 2 && data[2] is (bool useRecipient, string alternateName))
        {
            this.useRecipient = useRecipient;
            this.alternateName = alternateName;
        }
        // No dialogue recipient given, so we remove the character name field
        else
        {
            characterNameField.SetActive(false);
        }

        // Set text size
        OnTextSizeChanged();

        // Add event listener to check when dialogue is complete
        animator.OnDialogueComplete.AddListener(OnDialogueComplete);
        
        // Execute the starting object to begin dialogue
        currentObject.Execute();
    }

    /// <summary>
    /// Executed when the dialogue animator has finished writing the dialogue.
    /// </summary>
    public void OnDialogueComplete()
    {
        // If the current object is a TerminateDialogueObject, don't do anything,
        // just wait for the scene to unload.
        if (currentObject is TerminateDialogueObject)
            return;

        // Close dialogue field
        dialogueField.SetActive(false);
        characterNameField.SetActive(false);

        ExecuteNextObject();
    }

    /// <summary>
    /// Gets the current object's first response and executes it.
    /// </summary>
    private void ExecuteNextObject()
    {
        currentObject = currentObject.Responses.First();

        // If dialogue will end, do some additional things
        if (currentObject is TerminateDialogueObject)
        {
            // If phone is active, animate it going down
            if (phoneField.activeSelf)
                StartCoroutine(PhoneAnimation(phoneField.transform.GetChild(0), -80, -1900));
        }
        
        currentObject.Execute();
    }

    /// <summary>
    /// Write the given dialogue to the screen using the dialogue animator.
    /// </summary>
    /// <param name="dialogue">The text that needs to be written</param>
    /// <param name="pitch">The pitch of the character</param>
    public void WriteDialogue(List<string> dialogue, float pitch = 1)
    {
        // The text of this dialogue object is null, so we dont open the window.
        // This can be in case of a pause, an image, etc.
        if (dialogue == null)
        {
            dialogueField.SetActive(false);
            ExecuteNextObject();
        }
        else
        {
            // Enable the dialogue field
            dialogueField.SetActive(true);

            // Adjust the box containing the character's name
            if (currentRecipient != null)
            {
                characterNameField.SetActive(true);
                var text = characterNameField.GetComponentInChildren<TMP_Text>();
                text.text = useRecipient ? currentRecipient.characterName : alternateName;
            }

            // Animator write dialogue to the screen.
            pitch = currentRecipient == null ? 1 : currentRecipient.pitch;
            animator.WriteDialogue(dialogue, pitch);
        }
    }

    /// <summary>
    /// Writes a list of messages to a phone screen.
    /// </summary>
    /// <param name="messages">The list of messages to be written on the phone.</param>
    public void WritePhoneDialogue(List<string> messages)
    {
        // Store the layout in which the messages will be placed
        var phoneImage = phoneField.transform.GetChild(0);
        var phoneLayout = phoneImage.GetChild(0).GetChild(0);

        // If the phone is not open yet, animate it opening
        if (!phoneField.activeSelf)
        {
            SettingsManager.sm.PlaySfxClip(phoneNotificationClip);
            StartCoroutine(PhoneAnimation(phoneImage, -1900, -80, 0.8f));
        }

        // Adjust appropriate fields
        imageField.SetActive(false);
        questionsField.SetActive(false);
        dialogueField.SetActive(false);
        phoneField.SetActive(true);

        // Remove previous messages
        foreach (Transform child in phoneLayout)
            Destroy(child.gameObject);

        // Write the messages
        foreach (string message in messages)
            AddPhoneMessage(message, phoneLayout);

        // Rebuild the layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(phoneLayout.GetComponent<RectTransform>());
    }

    /// <summary>
    /// The animation for the phone being pulled up or down.
    /// <para>I have found -1900 and -80 to be good values for the heights, 1 for the duration.</para>
    /// </summary>
    private IEnumerator PhoneAnimation(Transform transform, float startingHeight, float finalHeight, float additionalWait = 0f)
    {
        var nextMessageButton = phoneField.transform.GetChild(1).gameObject;
        nextMessageButton.SetActive(false);
        transform.localPosition = new Vector2(transform.localPosition.x, startingHeight);

        // Wait before starting animation
        yield return new WaitForSeconds(additionalWait);

        float time = 0f;
        while (time < phoneAnimationDuration)
        {
            time += Time.deltaTime;

            // Use SmoothStep to create a dampened interpolation
            float timeStep = Mathf.SmoothStep(0, 1, time / phoneAnimationDuration);
            float height = Mathf.Lerp(startingHeight, finalHeight, timeStep);

            transform.localPosition = new Vector2(transform.localPosition.x, height);

            yield return null;
        }

        transform.localPosition = new Vector2(transform.localPosition.x, finalHeight);
        nextMessageButton.SetActive(true);
    }

    /// <summary>
    /// Creates a new message object in the given layout.
    /// </summary>
    /// <param name="message">The text be written in the message.</param>
    /// <param name="phoneLayout">The parent transform for the message.</param>
    private void AddPhoneMessage(string message, Transform phoneLayout)
    {
        var messageBox = Instantiate(phoneDialogueBoxPrefab, phoneLayout).GetComponent<ResizingTextBox>();
        messageBox.SetText(message);
    }

    public void PrintImage(Sprite newImage)
    {
        if (newImage == null)
            imageField.SetActive(false);
        else
        {
            // Enable the image field
            imageField.SetActive(true);

            // Set the content of the image
            imageField.GetComponent<Image>().sprite = newImage;
        }
    }
    
    /// <summary>
    /// Creates a background for the coming dialogue.
    /// </summary>
    /// <param name="story">The storyobject that contains the backgrounds for the dialogue.</param>
    /// <param name="character">The character the dialogue will be with.</param>
    /// <param name="background">The background for the dialogue.</param>
    /// <returns></returns>
    public GameObject[] CreateDialogueBackground(StoryObject story, CharacterInstance character = null, params GameObject[] background)
    {
        List<GameObject> background_ = new();

        // If the passed background is null, we use 'dialogueBackground' as the default. Otherwise, we use the passed one.
        if (background.Length == 0)
            background_.Add(story.dialogueBackground);
        else
            background_.AddRange(background);

        // If a character is given, add that as well with the proper emotion
        if (character != null)
        {
            avatarPrefab.GetComponent<Image>().sprite = 
                character.avatarEmotions.First(es => es.Item1 == Emotion.Neutral).Item2;
            background_.Add(avatarPrefab);
        }

        return background_.ToArray();
    }
    
    /// <summary>
    /// Replaces the current dialogue background with the given background.
    /// </summary>
    /// <param name="newBackground">The background that will replace the current background.</param>
    public void ReplaceBackground(GameObject[] newBackground, Emotion? emotion = null)
    {
        if (newBackground != null)
        {
            if (newBackground.Length > 0)
            {
                Transform parent = backgroundField.transform;

                // Remove old background
                foreach (Transform child in parent)
                    Destroy(child.gameObject);

                // Instantiate new background
                foreach (GameObject prefab in newBackground)
                {
                    var image = Instantiate(prefab).GetComponent<Image>();
                    image.rectTransform.SetParent(parent, false);                    
                }
            }
        }
        // Set emotion
        if (emotion.HasValue)
        {
            foreach (Transform child in backgroundField.transform)
            {
                if (child.gameObject.name == "Character Avatar(Clone)")
                {
                    if (currentRecipient != null)
                        child.GetComponent<Image>().sprite =
                            currentRecipient.avatarEmotions.First(es => es.Item1 == emotion.Value).Item2;
                }
            }
        }
    }

    /// <summary>
    /// Instantiates question (and return) buttons to the screen.
    /// </summary>
    /// <param name="questionDialogueObject">A <see cref="QuestionDialogueObject"/> containing the questions and responses</param>
    public void InstantiatePromptButtons(QuestionDialogueObject questionDialogueObject)
    {
        // Instantiate button containing each responseDialogue
        foreach (ResponseDialogueObject response in questionDialogueObject.Responses)
        {
            // Instantiate and set parent
            Button button = Instantiate(buttonPrefab, questionsField.transform).GetComponent<Button>();

            // Set Unity inspector values
            button.name = "questionButton";
            button.gameObject.tag = "Button";

            // Set button text in question form
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            buttonText.text = GetPromptText(response.question);

            // Set styling for button
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMax = SettingsManager.sm.GetFontSize();
            buttonText.font = customFont;

            // Add event when clicking the button
            button.onClick.AddListener(() => OnButtonClick(response));
        }

        // Add the button to return to the other characters
        CreateBackButton();

        // Set the buttons to be visible
        questionsField.SetActive(true);
    }

    /// <summary>
    /// Executed when a question button is pressed.
    /// </summary>
    /// <param name="responseDialogue">A <see cref="ResponseDialogueObject"/> containing the responseDialogue</param>
    public void OnButtonClick(ResponseDialogueObject responseDialogue)
    {
        // Remove buttons from screen
        DestroyButtons();

        // Remove questions field
        questionsField.SetActive(false);

        // Write dialogue when button is pressed
        currentObject = responseDialogue;
        currentObject.Execute();
    }

    /// <summary>
    /// Make required adjustments when text size was changed.
    /// </summary>
    private void OnTextSizeChanged()
    {
        if (phoneField.activeInHierarchy)
        {
            // Resize all message boxes
            foreach (var messageBox in phoneField.GetComponentsInChildren<ResizingTextBox>())
                messageBox.AdjustFontSize();

            // Rebuild layout
            foreach (var rectTransform in phoneField.GetComponentsInChildren<RectTransform>())
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        if (dialogueField.activeInHierarchy)
        {
            foreach (var text in dialogueField.GetComponentsInChildren<TMP_Text>())
                text.fontSizeMax = SettingsManager.sm.GetFontSize();
        }

        if (inputField.activeInHierarchy)
        {
            inputField.GetComponentInChildren<TMP_InputField>().pointSize = SettingsManager.sm.GetFontSize();
        }
    }

    /// <summary>
    /// Creates the buttons and the text field for the open questions.
    /// </summary>
    public void CreateOpenQuestion(List<string> dialogue)
    {
        // Enable the input field.
        inputField.SetActive(true);
        var inputFieldComponent = inputField.GetComponentInChildren<TMP_InputField>();
        inputFieldComponent.text = "";
        inputFieldComponent.pointSize = SettingsManager.sm.GetFontSize();

        animator.InOpenQuestion = true;        
        WriteDialogue(dialogue);

        // TODO: Save answer somewhere?
    }
    
    /// <summary>
    /// Creates the button to go back to the NPCSelect screen.
    /// </summary>
    private void CreateBackButton()
    {
        Button backButton = Instantiate(buttonPrefab, questionsField.transform).GetComponent<Button>();
        backButton.name = "backButton";
        backButton.gameObject.tag = "Button";

        TMP_Text buttonText = backButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = "Talk to someone else";
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMax = SettingsManager.sm.GetFontSize();
        buttonText.font = customFont;
        backButton.onClick.AddListener(() => BacktoNPCScreen());
    }

    /// <summary>
    /// Continues the dialogue after answering the open question.
    /// </summary>
    public void AnswerOpenQuestion()
    {
        TMP_InputField input = inputField.GetComponentInChildren<TMP_InputField>();
        if (input.text != string.Empty)
        {
            // Assign the text from the inputField to inputText and add it to the list of answers.
            inputText = input.text;
            // Can use this to write the inputText to somewhere, here..

            // Disable the input field.
            inputField.SetActive(false);

            // Reset the text from the input field.
            inputText = "";

            animator.InOpenQuestion = false;
            ExecuteNextObject();
        }
        else
        {
            input.placeholder.GetComponentInChildren<TMP_Text>().text = "Please enter text to continue...";
        }
    }

    
    /// <summary>
    /// Helper function for CreateBackButton.
    /// Sends the player back to the NPCSelect scene
    /// TODO: This button should take into account other situations dialgoue may appear beside in the gameloop
    /// </summary>
    private void BacktoNPCScreen()
    {
        DestroyButtons();
        // TODO: Combineer met het unloaden van Dialoguescene
        currentObject = new TerminateDialogueObject();
        currentObject.Execute();
    }

    #region [DEPRECATED] Continue Button
    /// <summary>
    /// Creates the button to ask another question to the same NPC
    /// </summary>
    private void CreateContinueButton()
    {
        Button button = Instantiate(buttonPrefab, dialogueField.transform).GetComponent<Button>();
        button.gameObject.tag = "Button";
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

        buttonText.text = "Ask another question";
        buttonText.enableAutoSizing = false;
        buttonText.fontSizeMax = 40;
        button.onClick.AddListener(() => ContinueTalking());
    }

    /// <summary>
    /// Helper function for CreateContinueButton.
    /// Lets the player ask a question to the NPC
    /// </summary>
    private void ContinueTalking()
    {
        DestroyButtons();
        for (int i = 0; i < 2; i++)
        {
            currentObject.Execute();
        }

        CreateBackButton();
    }
    #endregion

    /// <summary>
    /// Destroys all buttons with the "Button" tag currently in the scene.
    /// If a button should not be destroyed do not give it the "Button" tag.
    /// </summary>
    private void DestroyButtons()
    {
        var buttons = GameObject.FindGameObjectsWithTag("Button");
        for (int i = 0; i < buttons.Length; i++)
            Destroy(buttons[i]);
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
            // Change the characterNameField fontSize
            characterNameField.GetComponentInChildren<TMP_Text>().fontSize = fontSize;
            // Change the question and return button fontSize if they are present.
            foreach (Button b in questionsField.GetComponentsInChildren<Button>())
            {
                TMP_Text buttonText = b.GetComponentInChildren<TMP_Text>();
                buttonText.fontSize = fontSize;
            }
        }
    }
    #endregion
    
    /// <summary>
    /// Gets the text for the buttons that prompt specific questions.
    /// </summary>
    /// <param name="questionType">The type of question that is being prompted.</param>
    /// <returns></returns>
    public string GetPromptText(Question questionType)
    {
        return questionType switch
        {
            Question.Name => "What's your name?",
            Question.Age => "How old are you?",
            Question.LifeGeneral => "How's life?",
            Question.Inspiration => "Who inspires you?",
            Question.Sexuality => "What is your sexual orientation?",
            Question.Wellbeing => "How are you doing?",
            Question.Political => "What are your political thoughts?",
            Question.Personality => "Can you describe your personality?",
            Question.Hobby => "What are some of your hobbies?",
            Question.CulturalBackground => "What is your cultural background?",
            Question.Religion => "Are you religious?",
            Question.Education => "What is your education level?",
            Question.CoreValues => "What core values are important to you?",
            Question.ImportantPeople => "Who matters most to you?",
            Question.PositiveTrait => "What is your best trait?",
            Question.NegativeTrait => "What is a bad trait you may have?",
            Question.OddTrait => "Do you have any odd traits?",
            Question.SocialIssues => "What social issues are you interested in?",
            Question.EducationSystem => "Your thoughts on the Dutch school system?",
            Question.Lottery => "If you win the lottery, what would you do?",
            Question.Diet => "Do you have any dietary restrictions?",
            _ => "",
        };
    }
}

/// <summary>
/// An enum containing all possible questions in the game.
/// </summary>
public enum Question
{
    Name,
    Age,
    LifeGeneral,
    Inspiration,
    Sexuality,
    Wellbeing,
    Political,
    Hobby,
    CulturalBackground,
    Religion,
    Education,
    CoreValues,
    Personality,
    ImportantPeople,
    PositiveTrait,
    NegativeTrait,
    OddTrait,
    SocialIssues,
    EducationSystem,
    Lottery,
    Diet
}