using UnityEngine;

[CreateAssetMenu(menuName = "NetworkSettings")]
public class NetworkSettings : ScriptableObject
{
    #region Shared
    [Header("Shared variables")]
    [Tooltip("If set to true, debug messages will be displayed in the console, which will aid in debugging.")]
    public bool IsDebug;
    
    [Tooltip("After each interval, a check is made whether any errors were thrown, if so, they are displayed to the console.")]
    public float DisplayDebugIntervalSeconds = 1f;
    
    [Tooltip("After each interval, a check is made whether the user (can be both client or host) was disconnected.")]
    public float DisconnectedIntervalSeconds = 1f;
    
    [Tooltip("The signature used when sending and receiving the initialisation data: the storyID and seed.")]
    public string InitialisationDataSignature = "Initializer";
    
    [Tooltip("The signature used when sending and receiving the notebook data.")]
    public string NotebookDataSignature = "NotebookData";
    
    [Tooltip("The signature used when pinging the host to check whether you are still connected.")]
    public string PingDataSignature = "Ping";
    
    [Tooltip("The port used to send messages from the client to the host and back.")]
    public ushort ClientHostPortConnection = 42069;
    #endregion
    
    #region Client
    [Header("Client variables")]
    [Tooltip("After each interval, a check is made whether any data was sent to the host.")]
    public float IncomingDataIntervalSeconds = 1f;
    
    [Tooltip("When not connected to a host, but listening for a response, every interval a check is made whether or not a connection is made with the host. If the client is connected, proceed with listening for response data.")]
    public float ListeningWhileNotConnectedIntervalSeconds = 2f;
    
    [Tooltip("If no connection is made after the given amount of time, a popup is displayed to the user that no connection could be made.")]
    public float ConnectionTimeoutSeconds = 5f;
    
    [Tooltip("The amount of time after which a timeout event occurs if no ack was received from the host after sending a message")]
    public float AcknowledgementTimeoutSeconds = 10f;
    #endregion
}
