// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // SettingsManager has a static instance, so that we can fetch its settings from anywhere.
    public static SettingsManager sm;

    [Header("Component References")]
    public AudioMixer audioMixer; // The audiomixer that contains all soundchannels
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Settings (?)")]
    [SerializeField] float defaultMusicFadeInTime = 0.5f;
    [SerializeField] AudioClip defaultButtonClickSound;
    
    #region Pausing
    private int  pauseStack = 0;
    public  bool IsPaused { get { return pauseStack > 0; } }
    public void PauseGame() => pauseStack++;
    public void UnpauseGame() => pauseStack--;
    #endregion
    
    #region Text Size Variables
    // Text size to be used for the text components
    [NonSerialized] public TextSize textSize;
    
    public int maxLineLength
    {
        get
        {
            return textSize == TextSize.Small ? smallTextLineLength
                : textSize == TextSize.Medium ? mediumextLineLength 
                : largeTextLineLength;
        }
    }
    
    private int smallTextLineLength = 90;
    private int mediumextLineLength = 70;
    private int largeTextLineLength = 50;
    
    public enum TextSize
    {
        Small,
        Medium,
        Large
    }

    // Multipliers for different text sizes
    public const float M_SMALL_TEXT = 0.8f;
    public const float M_LARGE_TEXT = 1.4f;

    public UnityEvent OnTextSizeChanged;
    #endregion

    #region Settings Variables

    private                float defaultTalkingDelay = 0.05f;
    [NonSerialized] public float musicVolume         = 0;
    [NonSerialized] public float sfxVolume           = 0;
    [NonSerialized] public float talkingSpeed        = 1;

    public UnityEvent OnAudioSettingsChanged;
    #endregion
    
    public float TalkingDelay {  get; private set; }

    private Coroutine musicFadeCoroutine;
    private bool musicIsFading = false;
    
    private void Awake()
    {
        // create static instance of settingsmanager and make it DDOL
        sm = this;
        
        // Set the default textSize to medium.
        textSize = TextSize.Medium;
    }
    
    private void Start()
    {
        if (audioMixer != null)
            ApplySavedSettings();
    }

    private void ApplySavedSettings()
    {
        // Get the saved values
        musicVolume = PlayerPrefs.GetFloat(nameof(musicVolume), 80);
        sfxVolume = PlayerPrefs.GetFloat(nameof(sfxVolume), 80);
        talkingSpeed = PlayerPrefs.GetFloat(nameof(talkingSpeed), 1);
        textSize = (TextSize)PlayerPrefs.GetInt(nameof(textSize), 1);

        // Apply the saved values
        SetMusicVolume(musicVolume);
        SetSfxVolume(sfxVolume);
        SetTalkingSpeed(talkingSpeed);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(nameof(musicVolume), musicVolume);
        PlayerPrefs.SetFloat(nameof(sfxVolume), sfxVolume);
        PlayerPrefs.SetFloat(nameof(talkingSpeed), talkingSpeed);
        PlayerPrefs.SetInt(nameof(textSize), (int)textSize);
    }

    public void OnClick(Component sender, params object[] data)
    {
        AudioClip clip;
        if (data[0] is AudioClip audioClip)
            clip = audioClip;
        else
            clip = defaultButtonClickSound;

        PlaySfxClip(clip);
    }

    public void PlaySfxClip(AudioClip clip)
    {
        sfxSource.clip = clip;
        sfxSource.Play();
    }

    #region Audio
    /// <summary>
    /// Called through SettingsMenuManager to set the volume of the Master-channel of the Audiomixer.
    /// </summary>
    /// <param name="volume"></param>
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", volume);
    }

    /// <summary>
    /// Called through SettingsMenuManager to set the volume of the Music-channel of the Audiomixer.
    /// </summary>
    /// <param name="volume"></param>
    public void SetMusicVolume(float volume)
    {
        float adjustedVolume;

        if (volume <= 0)
            adjustedVolume = -80;
        else
            adjustedVolume = (volume - 100) * 0.5f;


        audioMixer.SetFloat(nameof(musicVolume), adjustedVolume);
        musicVolume = volume;

        OnAudioSettingsChanged.Invoke();
    }

    /// <summary>
    /// Called through SettingsMenuManager to set the volume of the SFX-channel of the Audiomixer.
    /// </summary>
    /// <param name="volume"></param>
    public void SetSfxVolume(float volume)
    {
        float adjustedVolume;

        if (volume <= 0)
            adjustedVolume = -80;
        else
            adjustedVolume = (volume - 100) * 0.5f;

        audioMixer.SetFloat(nameof(sfxVolume), adjustedVolume);
        sfxVolume = volume;

        OnAudioSettingsChanged.Invoke();
    }
    
    /// <summary>
    /// this method should fade-out the previous track, then fade-in the new track
    /// </summary>
    public void SwitchMusic(AudioClip newClip, float? fadeTime, bool loop)
    {
        // checks if newclip passed has a value
        if (newClip != null)
        {
            // If the passed fadeTime is null, we use the default music fade-in time
            float _fadeTime = fadeTime ?? defaultMusicFadeInTime;

            if (musicIsFading)
                StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = StartCoroutine(FadeOutMusic(newClip, _fadeTime));
        }
        
        // Set the music loop to the given parameter.
        musicSource.loop = loop;
    }

    /// <summary>
    /// Fades out currently playing Audioclip, then fades in Audioclip passed as argument.
    /// Speed of fade depends on fadetime.
    /// </summary>
    /// <param name="newClip"></param>
    /// <param name="fadeTime"></param>
    /// <returns></returns>
    private IEnumerator FadeOutMusic(AudioClip newClip, float fadeTime)
    {
        musicIsFading = true;

        // Don't fade out if it's the same clip
        if (newClip != musicSource.clip)
        {
            // Start loading the new clip
            // In the clip settings, "Load in Background" should be enabled,
            // otherwise the game could freeze until loading is done
            if (!newClip.loadInBackground)
                Debug.LogWarning(
                    $"{newClip.name} has {nameof(newClip.loadInBackground)} " +
                    $"set to {newClip.loadInBackground}. " +
                    $"This could cause freezes while the clip is loading.");
            newClip.LoadAudioData();

            while (musicSource.volume > 0)
            {
                musicSource.volume -= Time.deltaTime / fadeTime;
                yield return null;
            }
            musicSource.Stop();

            // Unload the old clip (Unity does not do this automatically)
            if (musicSource.clip != null)
                musicSource.clip.UnloadAudioData();

            // Wait for the new clip to finish loading
            while (!newClip.loadState.Equals(AudioDataLoadState.Loaded))
                yield return null;

            musicSource.clip = newClip;
        }

        // Ensure the clip is playing
        if (!musicSource.isPlaying)
            musicSource.Play();

        // Fade in the clip
        while (musicSource.volume < 1)
        {
            musicSource.volume += Time.deltaTime / fadeTime;
            yield return null;
        }

        musicIsFading = false;
    }
    #endregion

    #region Accessibility
    /// <summary>
    /// Get the fontSize of the dialogue text.
    /// </summary>
    public int GetFontSize()
    {
        switch (textSize)
        {
            case TextSize.Small:
                return 55;
            case TextSize.Medium:
                return 70;
            default:
                return 85;
        }
    }
    
    /// <summary>
    /// Takes a multiplier that changes the talking speed.
    /// If the multiplier is '3', it should be 3 times as fast.
    /// </summary>
    /// <param name="multiplier"></param>
    public void SetTalkingSpeed(float multiplier) 
    { 
        TalkingDelay = defaultTalkingDelay / multiplier;
        talkingSpeed = multiplier;
    }
    #endregion
}