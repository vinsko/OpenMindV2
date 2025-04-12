// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Handles the host side of networking.
/// This script can be active and inactive (separate from the active and inactive of objects in unity.)
/// </summary>
public class Host : NetworkObject
{
    private DataListener                 listener;
    private int                          seed;
    private int                          storyID;
    private int maxPlayers;
    private List<List<NetworkPackage>>   notebooks = new ();
    private Action<List<NetworkPackage>> sendFirstNotebook;
    private Action<List<NetworkPackage>> assignNotebookData;
    private Random                       notebookRandom = new Random();//a separate random variable that is distinct from the gamemanager random
    private bool addNormalResponse = false;
    private bool readyToSentFirstClientSecondClientNotebook;
    private List<NetworkPackage> dataToSendSecondClient;
    private bool isListening;
    private bool notebookReceivedPopup;
    private bool startWaitingOnNotebook;
    
    private void Update()
    {
        if (addNormalResponse)
        {
            //wait for async reading to finish
            addNormalResponse = false;
            StartCoroutine(SendDataWithDelay());
        }
        
        if (readyToSentFirstClientSecondClientNotebook)
        {
            Debug.Log("coroutine");
            readyToSentFirstClientSecondClientNotebook = false;
            StartCoroutine(SendDataWithDelay1());
        }

        if (notebookReceivedPopup)
        {
            ReceivedNotebookPopUp();
            notebookReceivedPopup = false;
        }

        if (startWaitingOnNotebook)
        {
            DisplayWaitNotebook();
            startWaitingOnNotebook = false;
        }
    }
    
    /// <summary>
    /// Enforce the player limit.
    /// If too many players are connected, stop listening for connections.
    /// If less players than the limit are connected keep/start listening.
    /// </summary>
    private void ManagePlayerAmount(object obj)
    {
        int playerCount = listener.GetPlayerAmount();
        
        if (isListening && playerCount >= maxPlayers)
        {
            listener.CancelListeningForConnections();
            isListening = false;
        }
        
        if (!isListening && playerCount < maxPlayers)
        {
            StartCoroutine(listener.AcceptIncomingConnections());
            isListening = true;
        }
    }
    
    private IEnumerator SendDataWithDelay()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("Adding normal response");
        listener.AddResponseTo(settings.NotebookDataSignature, ReceiveAndRespondWithNotebook);
    }
    
    private IEnumerator SendDataWithDelay1()
    {
        yield return new WaitForSeconds(1);
        sendFirstNotebook(dataToSendSecondClient);
    }
    
    /// <summary>
    /// Create a classroom code using the IP adress.
    /// </summary>
    public string CreateClassroomCode()
    {
        IPv4Converter converter = new IPv4Converter();
        return converter.ConvertToCode(ownIP);
    }
    
    /// <summary>
    /// Start hosting the game.
    /// Start listening for connections and for incoming data.
    /// Start the notebook exchange.
    /// </summary>
    /// <param name="storyID">the storyID</param>
    /// <param name="seed">the seed</param>
    /// <param name="maxPlayers">the maximum amount of players that can join the game</param>
    public void Lobby(int storyID, int seed, int maxPlayers)
    {
        this.seed = seed;
        this.storyID = storyID;
        this.maxPlayers = maxPlayers;
        
        listener = new DataListener(ownIP, settings.ClientHostPortConnection);
        StartCoroutine(listener.DisplayAnyDebugs(settings.DisplayDebugIntervalSeconds));
        listener.AddResponseTo(settings.InitialisationDataSignature, SendInit);
        
        if (settings.IsDebug)
            AddAdditionalDebugMessagesInit();
        
        StartCoroutine(listener.AcceptIncomingConnections());
        StartCoroutine(listener.ListenForIncomingData(settings.IncomingDataIntervalSeconds));
        StartCoroutine(listener.IsDisconnected(settings.PingDataSignature, settings.DisconnectedIntervalSeconds));
        
        listener.AddOnAcceptConnectionsEvent(ManagePlayerAmount);
        listener.AddOnDisconnectedEvent(ManagePlayerAmount);
        
        ActivateNotebookExchange();

        isListening = true;
        notebookReceivedPopup = false;
        startWaitingOnNotebook = false;
    }

    /// <summary>
    /// Returns the amount of players connected to the host.
    /// </summary>
    public int PlayerAmount() => listener.GetPlayerAmount();
    
    /// <summary>
    /// Send the storyID and the seed to the client.
    /// </summary>
    private List<NetworkPackage> SendInit(List<NetworkPackage> arg)
    {
        if (settings.IsDebug)
            Debug.Log("Host sending seed & storyID.");
        
        return new List<NetworkPackage>
        {
            NetworkPackage.CreatePackage(storyID),
            NetworkPackage.CreatePackage(seed)
        };
    }

    #region Notebook
    /// <summary>
    /// Start the exchanging of notebooks.
    /// </summary>
    private void ActivateNotebookExchange()
    {
        sendFirstNotebook =
            listener.AddDelayedResponseTo(settings.NotebookDataSignature,
                ReceiveFirstNotebookFromClient);
    }
    
    /// <summary>
    /// The host uploads their own notebook
    /// </summary>
    public void AddOwnNotebook(Action<NotebookData> assignNotebookData, NotebookData notebookData, List<CharacterInstance> currentCharacters)
    {
        this.assignNotebookData = package => assignNotebookData(
            new NotebookDataPackage(package[0], currentCharacters).ConvertToNotebookData());
        NotebookDataPackage package = new NotebookDataPackage(notebookData, currentCharacters);
        List<NetworkPackage> listPackage = new List<NetworkPackage> { package.CreatePackage() };
        
        //if host was first upload
        if (notebooks.Count == 0)
        {
            startWaitingOnNotebook = true;
            ReceiveFirstNotebookFromClient(listPackage);
            return;
        }
        
        //if host was second, send data to the first
        if (notebooks.Count == 1)
            sendFirstNotebook(listPackage);
        
        //get new notebook data and assign it to the host
        List<NetworkPackage> notebook = ReceiveAndRespondWithNotebook(listPackage);
        // assignNotebookData(
        //     new NotebookDataPackage(notebook[0], currentCharacters).ConvertToNotebookData());
        this.assignNotebookData(notebook);
        
        // show pop up that the host received a notebook
        notebookReceivedPopup = true;
    }
    
    /// <summary>
    /// The function for receiving the first notebook from the client.
    /// Because the player that sends the first notebook has to wait for a second player to send their notebook,
    /// this is a separate function
    /// </summary>
    private void ReceiveFirstNotebookFromClient(object o)
    {
        //client as second notebook upload
        if (notebooks.Count == 1)
        {
            //if host was first upload
            if (assignNotebookData != null)
            {
                Debug.Log($"Received first notebook from phone.");
                assignNotebookData((List<NetworkPackage>)o);
                
                // Show popup that the host received a notebook
                notebookReceivedPopup = true;
            }
        }
        
        //if not first upload
        if (notebooks.Count > 0)
            return;
        
        Debug.Log($"Received first notebook {((List<NetworkPackage>)o)[0].data}");
        
        AddNotebook((List<NetworkPackage>)o);
        
        addNormalResponse = true;
    }
    
    /// <summary>
    /// Add the received notebook to the notebook list and return a random notebook.
    /// </summary>
    private List<NetworkPackage> ReceiveAndRespondWithNotebook(List<NetworkPackage> o)
    {
        //if client and second upload, assign notebook to the first upload if it was also a client
        if (notebooks.Count == 1 && assignNotebookData == null)
        {
            // dataToSendSecondClient = o;
            // readyToSentFirstClientSecondClientNotebook = true;
            if (settings.IsDebug)
                Debug.Log($"sending first notebook, sending {o[0].data}, {addNormalResponse}");
            sendFirstNotebook(o);
        }
        
        List<NetworkPackage> randomNotebook = GetRandomNotebook();
        AddNotebook(o);
        if (settings.IsDebug)
            Debug.Log($"Obtained {o[0].data} and returned with {randomNotebook[0].data}");
        return randomNotebook;
    }
    
    /// <summary>
    /// Adds the notebook in the list, only if it's not already there.
    /// To prevent duplicate notebooks.
    /// </summary>
    private void AddNotebook(List<NetworkPackage> notebookData)
    {
        if(!notebooks.Contains(notebookData))
            notebooks.Add(notebookData);
    }
    
    /// <summary>
    /// Get a random notebook from the notebook list.
    /// </summary>
    private List<NetworkPackage> GetRandomNotebook() =>
        notebooks[notebookRandom.Next(notebooks.Count)];
    #endregion
    
    #region debugMethods
    
    private void AddAdditionalDebugMessagesInit()
    {
        listener.AddOnAcceptConnectionsEvent(OnConnectionAccepted);
        listener.AddOnDisconnectedEvent(OnDisconnect);
        listener.AddOnDataReceivedEvent(settings.InitialisationDataSignature, OnDataReceived);
        listener.AddOnResponseSentEvent(settings.InitialisationDataSignature, OnResponseSent);
        listener.AddOnResponseSentEvent(settings.NotebookDataSignature, OnResponseSent);
        listener.AddOnAckSentEvent(OnAckSent);
    }
    
    private void OnConnectionAccepted(object obj)
    {
        Debug.Log($"(Host): Connected with client {((Socket)obj).RemoteEndPoint}");
    }
    
    private void OnDisconnect(object obj)
    {
        Debug.Log($"(Host): Client with endpoint {((Socket)obj).RemoteEndPoint} disconnected");   
    }
    
    private void OnDataReceived(object obj)
    {
        Debug.Log($"(Host): Received data {((List<NetworkPackage>)obj)[0].GetData<string>()}");
    }
    
    private void OnResponseSent(object obj)
    {
        Debug.Log($"(Host): Sent response package of {obj} bytes");
    }
    
    private void OnAckSent(object obj)
    {
        Debug.Log($"(Host): Sent ack with signature {obj}");
    }
    #endregion
    
    /// <summary>
    /// Dispose of the host when quitting the game.
    /// </summary>
    public override void Dispose()
    {
        if (listener != null)
        {
            listener.CancelListeningForConnections();
            listener.CancelListeningForData();
            listener.Dispose();
        }
    }
}
