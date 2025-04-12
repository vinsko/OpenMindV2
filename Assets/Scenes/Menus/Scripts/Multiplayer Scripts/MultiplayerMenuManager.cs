// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class MultiplayerMenuManager : MonoBehaviour
{
    [Header("Canvases")] 
    [SerializeField] private GameObject multiplayerCanvas;
    [SerializeField] private GameObject hostCanvas;
    [SerializeField] private GameObject joinCanvas;
    [SerializeField] private GameObject storyCanvas;
    [SerializeField] private GameObject lobbyCanvas;


    [Header("Paramaters")] 
    [SerializeField] private int maxPlayers;
    [SerializeField] private TextMeshProUGUI code;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button          joinButton;
    
    private string classCode;
    private int    storyid;

    /// <summary>
    /// Opens the settings menu.
    /// </summary>
    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Goes back to the main menu.
    /// </summary>
    public void ReturnMain()
    {
        SceneManager.UnloadSceneAsync("MultiplayerScreenScene");
        MultiplayerManager.mm.KillMultiplayer(true);
    }
    
    /// <summary>
    /// Goes back to the main multiplayer menu from the host or join menu.
    /// </summary>
    public void BackToMultiplayer()
    {
        hostCanvas.SetActive(false);
        joinCanvas.SetActive(false);
        lobbyCanvas.SetActive(false);
        multiplayerCanvas.SetActive(true);
        MultiplayerManager.mm.KillMultiplayer();
    }

    #region Host

    /// <summary>
    /// Activates the buttons for hosting a game.
    /// </summary>
    public void OpenHostMenu()
    {
        multiplayerCanvas.SetActive(false);
        hostCanvas.SetActive(true);
    }
    
    /// <summary>
    /// Sets the max players a session can have using a slider.
    /// </summary>
    /// <param name="num">The value of the slider.</param>
    public void SetMaxPlayers(Slider slider)
    {
        maxPlayers = (int)slider.value;
    }

    /// <summary>
    /// Starts the game as the host.
    /// The host has to choose a story first
    /// </summary>
    public void CreateAsHost()
    {
        hostCanvas.SetActive(false);
        storyCanvas.SetActive(true);
    }
    
    public void StoryA()
    {
        HostGame(0);
    }

    public void StoryB()
    {
        HostGame(1);
    }

    public void StoryC()
    {
        HostGame(2);
    }

    /// <summary>
    /// Enter a waiting lobby to allow players to join before starting the game.
    /// </summary>
    /// <param name="storyid">the chosen story</param>
    public void HostGame(int id)
    {
        storyid = id;
        storyCanvas.SetActive(false);
        lobbyCanvas.SetActive(true);
        
        // Create a code
        classCode = MultiplayerManager.mm.GetClassCode();
        
        // Start hosting
        MultiplayerManager.mm.HostGame(storyid, maxPlayers);
    }

    /// <summary>
    /// Starts the game when you're in the waiting lobby
    /// </summary>
    public void StartAsHost()
    {
        MultiplayerManager.mm.StartGame();
    }

    #endregion


    #region Join

    /// <summary>
    /// Activates the buttons for joining a game.
    /// </summary>
    public void OpenJoinMenu()
    {
        multiplayerCanvas.SetActive(false);
        joinCanvas.SetActive(true);
    }

    /// <summary>
    /// Sets the class code to the input from the player.
    /// </summary>
    /// <param name="code">The contents of the input field.</param>
    public void SetCode(GameObject inputField)
    {
        classCode = inputField.GetComponent<TMP_InputField>().text;
    }

    private void ReactivateJoinButton()
    {
        if(SceneManager.GetSceneByName("MultiplayerScreenScene").isLoaded)
            joinButton.interactable = true;
    }
    
    public void JoinGame()
    {
        joinButton.interactable = false;
        MultiplayerManager.mm.JoinGame(classCode, ReactivateJoinButton);
    }
    #endregion
    
    public void Update()
    {
        if (lobbyCanvas.activeInHierarchy)
        {
            code.text = classCode;
            playerCountText.text = MultiplayerManager.mm.GetPlayerAmount() + " of " + maxPlayers + " players in game";
        }
    }
}
