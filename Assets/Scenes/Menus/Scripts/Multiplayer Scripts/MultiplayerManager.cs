// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

/// <summary>
/// Handles the multiplayer for both the host and client side of the game.
/// </summary>
public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager mm;
    
    public GameEvent       onGameLoaded;
    public NetworkSettings settings;
    public GameEvent       doPopup;
    
    private Host                 host;
    private Client               client;
    public  MultiplayerInit      init;
    private bool                 isSeedInitialized;
    private bool                 isStoryInitialized;
    public  Action<NotebookData> notebookAction;
    [NonSerialized]public bool                  playerReceivedNotebook;
    
    void Awake()
    {
        mm = this;
        DontDestroyOnLoad(gameObject);
        init = new MultiplayerInit();
        playerReceivedNotebook = false;
    }
    
    /// <summary>
    /// Start hosting a game using the story ID and the max amount of players.
    /// The seed is created here as well.
    /// </summary>
    /// <param name="storyID">the chosen story</param>
    /// <param name="maxPlayers">the amount of players that are able the join the game</param>
    public void HostGame(int storyID, int maxPlayers)
    {
        init.story = storyID;
        
        // Create a seed
        init.seed = new Random().Next(int.MaxValue);
        
        // Let clients connect to the game
        host.Lobby(storyID, init.seed, maxPlayers);
    }
    
    /// <summary>
    /// Create a classroom code, as the host.
    /// </summary>
    public string GetClassCode()
    {
        // Create and activate the host
        host = gameObject.AddComponent<Host>();
        
        // Assign the settings
        host.AssignSettings(doPopup, settings);
        
        return host.CreateClassroomCode();
    }

    /// <summary>
    /// Join the game as the client using the classcode.
    /// </summary>
    /// <param name="classCode">the classcode from the host</param>
    /// <param name="reactivateJoinButton">the action of reactvating the join button</param>
    public void JoinGame(string classCode, Action reactivateJoinButton)
    {
        // Create the client
        client = gameObject.AddComponent<Client>();
        
        // Assign the settings
        client.AssignSettings(doPopup, settings);
        
        // Connect to the host using the code
        client.EnterClassroomCode(classCode, AssignSeed, AssignStory, reactivateJoinButton);
    }
    
    /// <summary>
    /// Assign the seed received from the host.
    /// </summary>
    /// <param name="seed">the seed received from the host</param>
    private void AssignSeed(int seed)
    {
        init.seed = seed;
        isSeedInitialized = true;
    }
    
    /// <summary>
    /// Assign the story id received from the host.
    /// </summary>
    /// <param name="story">the storyID received from the host</param>
    private void AssignStory(int story)
    {
        init.story = story;
        isStoryInitialized = true;
    }
    
    private void Update()
    {
        // Start the game only when both the seed and the storyID are received and initialized
        if (isSeedInitialized && isStoryInitialized)
        {
            isStoryInitialized = false;
            isSeedInitialized = false;
            SceneManager.LoadScene("PrologueScene");
        }
    }
    
    /// <summary>
    /// Start the game.
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("PrologueScene");
    }
    
    /// <summary>
    /// Send the notebook from either the host or client to the host.
    /// </summary>
    public void SendNotebook()
    {
        notebookAction = receivedNotebook =>
        {
            GameManager.gm.multiplayerNotebookData = receivedNotebook;
            playerReceivedNotebook = true;
        };
            
        if (client == null)
        {
            host.AddOwnNotebook(notebookAction, 
                GameManager.gm.notebookData,
                GameManager.gm.currentCharacters);
        }
        else
        {
            client.SendNotebookData(notebookAction, 
                GameManager.gm.notebookData,
                GameManager.gm.currentCharacters);
        }
    }
    
    /// <summary>
    /// Get the amount of players that are connected to the host.
    /// </summary>
    public int GetPlayerAmount()
    {
        return host.PlayerAmount();
    }
    
    /// <summary>
    /// Destroy the socket and host/client.
    /// If
    /// </summary>
    public void KillMultiplayer(bool destroyMultiplayerManager = false)
    {
        client?.Dispose();
        host?.Dispose();
        
        if(destroyMultiplayerManager)
            Destroy(FindObjectOfType<MultiplayerManager>().gameObject);
    }
}
