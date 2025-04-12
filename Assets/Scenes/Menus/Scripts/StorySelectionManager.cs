// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The manager class for the StorySelectScene
/// </summary>
public class StorySelectionManager : MonoBehaviour
{
    [Header("Data")] 
    [SerializeField] public StoryObject[] stories;
    
    // Game Events
    [Header("Events")]
    public GameEvent onIntroLoaded;

    [Header("Buttons")] 
    [SerializeField] private GameButton storyButtonA;
    [SerializeField] private GameButton storyButtonB;
    [SerializeField] private GameButton storyButtonC;

    [Header("Text Objects")] 
    [SerializeField] private Image storyAComplete;
    [SerializeField] private Image storyBComplete;
    [SerializeField] private Image storyCComplete;
    
    private void Awake()
    {
        if(MultiplayerManager.mm)
            StartIntroMultiplayer();
        else
            UpdateButtons();
    }
    
    /// <summary>
    /// Start the correct intro in multiplayer mode. The host has chosen the story already.
    /// </summary>
    private void StartIntroMultiplayer()
    {
        switch (MultiplayerManager.mm.init.story)
        {
            case 0:
                StoryASelected();
                break;
            case 1:
                StoryBSelected();
                break;
            case 2:
                StoryCSelected();
                break;
        }
    }

    /// <summary>
    /// Prepares buttons in StorySelect-scene based on UserData.
    /// </summary>
    public void UpdateButtons()
    {
        // TODO: Preferably find a way to do this without 'GameObject.Find'
        if (FetchUserData.Loader.GetUserDataValue(FetchUserData.UserDataQuery.storyAWon))
        {
            // story b unlocked
            storyButtonB.interactable = true;
            // enable 'complete'-text
            storyAComplete.gameObject.SetActive(true);
        }
        if (FetchUserData.Loader.GetUserDataValue(FetchUserData.UserDataQuery.storyBWon))
        {
            // story c unlocked
            storyButtonC.interactable = true;
            // enable 'complete'-text
            storyBComplete.gameObject.SetActive(true);
        }
        if (FetchUserData.Loader.GetUserDataValue(FetchUserData.UserDataQuery.storyCWon))
        {
            // enable 'complete'-text
            storyCComplete.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Starts Story A
    /// </summary>
    public void StoryASelected()
    {
        StartIntro(0);
    }
    
    /// <summary>
    /// Starts Story B
    /// </summary>
    public void StoryBSelected()
    {
        StartIntro(1);
    }
    
    /// <summary>
    /// Starts Story C
    /// </summary>
    public void StoryCSelected()
    {
        StartIntro(2);
    }

    /// <summary>
    /// Back to main menu (Start Screen)
    /// </summary>
    public void ToMainMenu()
    {
        SceneManager.LoadScene("StartScreenScene", LoadSceneMode.Single);
    }

    /// <summary>
    /// Starts the intro belonging to the story the player chose
    /// </summary>
    /// <param name="storyid">The story that the player chose</param>
    void StartIntro(int storyid)
    {
        StartCoroutine(LoadIntro(storyid));
    }
    
    /// <summary>
    /// Loads the appropriate intro scene
    /// </summary>
    /// <param name="storyid">The story that the player chose</param>
    IEnumerator LoadIntro(int storyid)
    {
        // Start the loadscene-operation
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("IntroStoryScene", LoadSceneMode.Additive);
        
        // Within this while-loop, we wait until the scene is done loading. We check this every frame
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        onIntroLoaded.Raise(this, stories[storyid]);
        
        // Finally, when the data has been sent, we then unload our currentscene
        SceneManager.UnloadSceneAsync("StorySelectScene");  // unload this scene; no longer necessary
    }
    
}
