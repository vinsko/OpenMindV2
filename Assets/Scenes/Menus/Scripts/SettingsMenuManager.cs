// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using JetBrains.Annotations;
using TMPro;
using UnityEditor;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("Audio References")]
    [SerializeField] private GameObject audioSliderGroup;

    private GameSlider musicVolumeSlider;
    private GameSlider sfxVolumeSlider;

    // The active size for the text settings (default: medium)
    private GameButton activeButton;
    
    // Chosen text size
    private SettingsManager.TextSize textSize;
    
    // The dropwdown menu
    [SerializeField] private TMP_Dropdown dropdown;

    // Example tmp_text objects
    [SerializeField] private TMP_Text characterNameField;
    [SerializeField] private TMP_Text dialogueBox;
    [SerializeField] private TMP_Text dropdownText;
    [SerializeField] private TMP_Text dropdownLabel;
    
    // Events
    [SerializeField] GameEvent onTextSizeChanged; 
    
    void Awake()
    {
        // Set the active text size button
        textSize = SettingsManager.sm.textSize;

        // Set the default value of the dropdown to the current textsize
        switch (textSize)
        {
            case (SettingsManager.TextSize.Small):
                dropdown.value = 0;
                break;
            case (SettingsManager.TextSize.Medium):
                dropdown.value = 1;
                break;
            case (SettingsManager.TextSize.Large):
                dropdown.value = 2;
                break;
        }
        dropdown.onValueChanged.AddListener(delegate {dropdownValueChanged(dropdown); });
        
        // Change the text size
        characterNameField.GetComponentInChildren<TMP_Text>().enableAutoSizing = true;
        dialogueBox.GetComponentInChildren<TMP_Text>().enableAutoSizing = true;
        dropdown.GetComponentInChildren<TMP_Text>().enableAutoSizing = true;
        ChangeTextSize();
    }

    [Header("Accessibility References")]
    [SerializeField] private GameSlider textSpeedSlider;
    [SerializeField] private Toggle textToSpeechToggle;

    private void Start()
    {
        // Get the sliders
        GameSlider[] sliders = audioSliderGroup.GetComponentsInChildren<GameSlider>();
        musicVolumeSlider = sliders[0];
        sfxVolumeSlider = sliders[1];

        // Set the values on the UI elements
        musicVolumeSlider.UpdateSlider(SettingsManager.sm.musicVolume);
        sfxVolumeSlider.UpdateSlider(SettingsManager.sm.sfxVolume);
        textSpeedSlider.slider.SetValueWithoutNotify(SettingsManager.sm.talkingSpeed);        
    }

    /// <summary>
    /// Called when the scene is unloaded.
    /// Saves the values which were set in the settings screen.
    /// </summary>
    private void OnDestroy()
    {
        SettingsManager.sm.SaveSettings();
    }

    /// <summary>
    /// Called to exit the settingsmenu. It sets any other menu overlays to 'active',
    /// such as the notebook- and menu-buttons in the game, and unloads its own scene.
    /// </summary>
    public void ExitSettings()
    {
        if (SceneManager.GetSceneByName("StartScreenScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("SettingsScene");
        }
        else if (SceneManager.GetSceneByName("Loading").isLoaded)
        {
            // '_ =' throws away the await
            _ = SceneController.sc.TransitionScene(SceneController.SceneName.SettingsScene,
                SceneController.SceneName.Loading,
                SceneController.TransitionType.Unload,
                true);
        }
    }
    
    /// <summary>
    /// Called to set the volume of the Master-channel of the Audiomixer, using dynamic floats.
    /// </summary>
    /// <param name="volume"></param>
    public void SetMasterVolume(float volume)
    {
        SettingsManager.sm.SetMasterVolume(volume);
    }
    
    /// <summary>
    /// Called to set the volume of the Music-channel of the Audiomixer, using dynamic floats.
    /// </summary>
    /// <param name="volume"></param>
    public void SetMusicVolume(float volume)
    {
        SettingsManager.sm.SetMusicVolume(volume);
    }
    
    /// <summary>
    /// Called to set the volume of the Sfx-channel of the Audiomixer, using dynamic floats.
    /// </summary>
    /// <param name="volume"></param>
    public void SetSfxVolume(float volume)
    {
        SettingsManager.sm.SetSfxVolume(volume);        
    }

    public void SetTalkingSpeed(float multiplier)
    {
        SettingsManager.sm.SetTalkingSpeed(multiplier);
    }
    
    #region TextSettings
    
    // TODO: add a dialogue box showing the difference in text sizes.

    void dropdownValueChanged(TMP_Dropdown change)
    {
        textSize = GetTextSize(change.value);
        
        // Change the textSize from SettingsManager
        SettingsManager.sm.textSize = textSize;
        SettingsManager.sm.OnTextSizeChanged.Invoke();
        
        // Change the fontSize of the example
        ChangeTextSize();
        
        // Raise the onTextSizeChanged event to change the text size
        onTextSizeChanged.Raise(null, SettingsManager.sm.GetFontSize());
    }
    
    
    /// <summary>
    /// Change the fontSize of the tmp_text components
    /// </summary>
    private void ChangeTextSize()
    {
        int fontSize = SettingsManager.sm.GetFontSize();
        // Change the fontSize of the confirmSelectionButton
        characterNameField.GetComponentInChildren<TMP_Text>().fontSizeMax = fontSize;
        
        // Change the fontSize of the headerText
        dialogueBox.GetComponentInChildren<TMP_Text>().fontSizeMax = fontSize;
        
        // Change the fontSize of the dropdown
        dropdown.GetComponentInChildren<TMP_Text>().fontSizeMax = fontSize;
        dropdownLabel.GetComponentInChildren<TMP_Text>().fontSizeMax = fontSize;
    }

    /// <summary>
    /// Get the TextSize corresponding to the dropdown
    /// </summary>
    /// <returns></returns>
    private SettingsManager.TextSize GetTextSize(int index)
    {
        switch (index)
        {
            case 0:
                return SettingsManager.TextSize.Small;
            case 1:
                return SettingsManager.TextSize.Medium;
            case 2:
                return SettingsManager.TextSize.Large;
        }

        return SettingsManager.TextSize.Medium;
    }

    #endregion
}