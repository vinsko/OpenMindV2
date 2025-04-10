// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script containing methods to be used on button click in the gamewin scene.
/// Each method calls a method from <see cref="GameManager"/>.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameObject gameWonCanvas;
    [SerializeField] private GameObject gameLossCanvas;
    [SerializeField] private GameObject[] singleplayerButtons;

    [Header("Game Events")] 
    [SerializeField] private GameEvent onGameLoaded;
    
    // Game Variables
    private bool hasWon;
    private List<CharacterInstance> characters;
    private int culpritID;
    private StoryObject story;
    private SceneController sc;
    
    
    public void StartGameOver(Component sender, params object[] data)
    {
        // Set variables
        foreach (var d in data)
        {
            switch (d)
            {
                case bool hasWon:
                    this.hasWon = hasWon;
                    break;
                case List<CharacterInstance> characters:
                    this.characters = characters;
                    break;
                case int culpritID:
                    this.culpritID = culpritID;
                    break;
                case StoryObject story:
                    this.story = story;
                    break;
                default:
                    Debug.LogError("Unknown Data received.");
                    break;
            }
        }
        sc = SceneController.sc;
        
        SetStatusCanvas();
        SetButtons();
    }

    /// <summary>
    /// Set GameWin or GameLoss
    /// </summary>
    private void SetStatusCanvas()
    {
        if (hasWon)
        {
            gameLossCanvas.SetActive(false);
            gameWonCanvas.SetActive(true);
        }
        else
        {
            gameLossCanvas.SetActive(true);
            gameWonCanvas.SetActive(false);
        }
    }
    
    /// <summary>
    /// Only show the retry and restart buttons during singleplayer and not during multiplayer.
    /// </summary>
    private void SetButtons()
    {
        // Don't let the player restart if we are in multiplayer
        foreach (var button in singleplayerButtons)
            button.SetActive(MultiplayerManager.mm == null);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("StartScreenScene");
        if (MultiplayerManager.mm)
            MultiplayerManager.mm.KillMultiplayer(true);
    }
    
    /// <summary>
    /// Retry the game with the same characters, culprit and story.
    /// </summary>
    public async void Retry()
    {
        await sc.TransitionScene(
            SceneController.SceneName.GameOverScene,
            SceneController.SceneName.Loading,
            SceneController.TransitionType.Transition,
            true);
        
        onGameLoaded.Raise(this, characters, culpritID, story);
    }
    
    /// <summary>
    /// Restart the game with different characters and the same culprit, but the same story.
    /// </summary>
    public async void Restart()
    {
        await sc.TransitionScene(
            SceneController.SceneName.GameOverScene,
            SceneController.SceneName.Loading,
            SceneController.TransitionType.Transition,
            true);
        
        onGameLoaded.Raise(this, story);
    }

}
