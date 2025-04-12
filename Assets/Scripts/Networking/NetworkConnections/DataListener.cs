// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// A class to facilitate listening for data.
/// </summary>
public class DataListener : DataNetworker
{
    private List<Socket>   connections;
    private List<bool>     isConnectionReceiving;
    private List<DateTime> lastReceveivedMessage;
    
    ///<summary>when a connection is made, input is the socket the connect is made with</summary>
    private NetworkEvents onAcceptConnectionEvents;
    ///<summary>when any data is received, input is the data received</summary>
    private NetworkEvents onDataReceivedEvents;
    ///<summary>when a response is sent back to the sender of the received data, input is the amount of bytes of the return message</summary>
    private NetworkEvents onResponseSentEvents;
    ///<summary>when an ack is sent to the sender to confirm their message has been received, input is a tuple of an int and the received message</summary>
    private NetworkEvents onAckSentEvents;
    ///<summary>used to convert received data into a new message</summary>
    private NetworkEvents respondEvents;
    ///<summary>used to respond in a delayed matter</summary>
    private NetworkDelayedEvents delayedRespondEvents;
    
    private bool isConnectionListening, isDataListening;
    
    ///<summary>amount of players that joined</summary>
    public int GetPlayerAmount() => connections.Count;
    
    /// <summary>
    /// Creates a data listening object using an IPAddress and a port to create an endpoint.
    /// The ipAddress should point to an ipAddress on the local device.
    /// </summary>
    public DataListener([DisallowNull] IPAddress ipAddress, ushort port) : base(ipAddress, port)
    {
        onAcceptConnectionEvents = new NetworkEvents();
        onDataReceivedEvents = new NetworkEvents();
        onResponseSentEvents = new NetworkEvents();
        onAckSentEvents = new NetworkEvents();
        respondEvents = new NetworkEvents();
        delayedRespondEvents = new NetworkDelayedEvents();
        lastReceveivedMessage = new List<DateTime>();
        
        if (!IPConnections.GetOwnIps().Contains(ipAddress))
        {
            Debug.LogError("You can only bind to your own IPAddress.");
            return;
        }
        
        socket.Bind(endPoint);
        socket.Listen(255);
        connections = new List<Socket>();
        isConnectionReceiving = new List<bool>();
        
        //create the ack respond event with the signature as the message
        onAckSentEvents.Subscribe("ACK",
            o =>
            {
                (int, string) data = ((int, string))o;
                SendResponseTo("ACK", signature => signature, 
                    (data.Item1, new List<NetworkPackage>{NetworkPackage.CreatePackage(data.Item2)}));
            });
    }
    
    /// <summary>
    /// Starts a loop in a coroutine to accept incoming connections. This function will run infinitely unless cancelled.
    /// While this function is running, any incoming connections will be accepted.
    /// </summary>
    /// <param name="clearOnAcceptConnectionEvents">If set to true, the actions called after connecting with a client are removed from the event.</param>
    public IEnumerator AcceptIncomingConnections(bool clearOnAcceptConnectionEvents = false)
    {
        GiveDisplayWarning();
        isConnectionListening = true;
        
        while (isConnectionListening)
        {
            Task task = null;
            try
            {
                task = socket.AcceptAsync().ContinueWith(t =>
                {
                    connections.Add(t.Result);
                    isConnectionReceiving.Add(false);
                    lastReceveivedMessage.Add(DateTime.Now);
                    logWarning = onAcceptConnectionEvents.Raise("Connect", t.Result, clearOnAcceptConnectionEvents, "onAcceptConnectionEvent");
                });
            }
            catch (ObjectDisposedException e)
            {
                Debug.LogError("The socket was closed: " + e);
                isConnectionListening = false;
            }
            catch (Exception e)
            {
                Debug.LogError("Something went wrong when listening: " + e);
                isConnectionListening = false;
            }
            
            yield return new WaitUntil(() => task is null || task.IsCompleted);
        }
    }
    
    /// <summary>
    /// Makes the listener stop listening for incoming connections.
    /// </summary>
    public void CancelListeningForConnections() => isConnectionListening = false;
    
    
    /// <summary>
    /// Listens for incoming data in a coroutine.
    /// When data is received, the signature is read and the actions connected to the signature are called.
    /// Actions will be added by adding events using the relevant functions.
    ///
    /// This loop will run infinitely unless cancelled by calling the relevant cancel function.
    /// <para>Note: A response with the signature "ACK" will always be sent after receiving a message. These ACK events will never be cleared.</para>
    /// </summary>
    /// <param name="intervalSeconds">After each interval a check is made whether any incoming data was received.</param>
    /// <param name="clearDataReceivedEvents">If set to true, the actions called after receiving a package are removed from the event.</param>
    /// <param name="clearRespondEvents">If set to true, the actions called after responding with a message are removed from the event.</param>
    /// <param name="clearAckSentEvents">If set to true, the actions called after responding with an ack (acknowledement) are removed from the event.</param>
    public IEnumerator ListenForIncomingData(float intervalSeconds, bool clearDataReceivedEvents = false, bool clearRespondEvents = false, bool clearAckSentEvents = false)
    {
        GiveDisplayWarning();
        isDataListening = true;
        
        while (isDataListening)
        {
            for (var i = 0; i < connections.Count; i++)
            {
                //if this socket is already receiving data, ignore it.
                if (isConnectionReceiving[i])
                    continue;
                
                ReceiveData(i, clearDataReceivedEvents, clearRespondEvents);
            }
            
            if (isDataListening)
                yield return new WaitForSeconds(intervalSeconds);
        }
    }
    
    /// <summary>
    /// Makes the listener stop listening for incoming data.
    /// </summary>
    public void CancelListeningForData() => isDataListening = false;
    
    /// <summary>
    /// A function for handling incoming data, see <see cref="ListenForIncomingData"/> for details.
    /// </summary>
    private void ReceiveData(int index, bool clearDataReceivedEvents, bool clearRespondEvents)
    {
        isConnectionReceiving[index] = true;
        byte[] buffer = new byte[NetworkPackage.MaxPackageSize];
        connections[index].ReceiveAsync(buffer, SocketFlags.None).ContinueWith(
            receivedByteAmount =>
            {
                //catch all just in case an error slips through
                try
                {
                    if (!TryGetConvertData(buffer, receivedByteAmount, out List<List<NetworkPackage>> networkData))
                        return;
                    
                    foreach (var networkPackage in networkData)
                        HandleReceivedData(networkPackage, index, clearDataReceivedEvents, clearRespondEvents);
                }
                catch (Exception e)
                {
                    logError = "Receiving message failed with error: " + e;
                }
            });
    }
    
    private void HandleReceivedData(List<NetworkPackage> networkData, int index, bool clearDataReceivedEvents, bool clearRespondEvents)
    {
        lastReceveivedMessage[index] = DateTime.Now;
        string signature = networkData[0].GetData<string>();
        
        List<NetworkPackage> receivedTailPackages = networkData.Skip(1).ToList();
        //Debug.Log($"Received data in listening {receivedTailPackages[0].data}");
        
        //Debug.Log("onDataReceivedEvent");
        logError = onDataReceivedEvents.Raise(signature, receivedTailPackages, clearDataReceivedEvents, "onDataReceivedEvent");
        if (logError != "")
            return;
        
        //Debug.Log("onAckSentEvent");
        //respond with an ack
        logError = onAckSentEvents.Raise("ACK", (index, signature), false, "onAckSentEvent");
        if (logError != "")
            return;
        
        //Debug.Log("respondEvent");
        //responds to the sender
        logError = respondEvents.Raise(signature, (index, receivedTailPackages),
            clearRespondEvents, "respondEvent");
        if (logError != "")
            return;
        
        //Debug.Log("delayedRespondEvent");
        logError = delayedRespondEvents.InputData(signature, (index, receivedTailPackages),
            clearRespondEvents, "delayedRespondEvent");
        if (logError != "")
            return;
        
        isConnectionReceiving[index] = false;
        //Debug.Log("finish reading");
    }
    
    /// <summary>
    /// Adds an action to the event of a sender connecting with this listener.
    /// When receiving a connection from a socket (DataSender), the given action is called.
    /// The object parameter of the action is the socket that the connection was made with.
    /// </summary>
    public void AddOnAcceptConnectionsEvent(Action<object> action) =>
        onAcceptConnectionEvents.Subscribe("Connect", action);
    
    /// <summary>
    /// Adds an action to the event of receiving a data package.
    /// When receiving a package with the given signature, the given action is called.
    /// The object parameter of the action is the data that was received.
    /// </summary>
    public void AddOnDataReceivedEvent([DisallowNull] string signature, Action<object> action) =>
        onDataReceivedEvents.Subscribe(signature, action);
    
    /// <summary>
    /// Adds an action to the event of sending a response.
    /// When receiving a package and then responding by sending a new package to the sender, the given action is called.
    /// The object parameter of the action is the amount of byte that a response was sent with.
    /// </summary>
    public void AddOnResponseSentEvent([DisallowNull] string signature, Action<object> action) =>
        onResponseSentEvents.Subscribe(signature, action);
    
    /// <summary>
    /// Adds an action to the event of sending an ACK (acknowledgement message).
    /// When receiving a package and then responding by sending an ACK to the sender, the given action is called.
    /// The object parameter of the action is the signature of the received message.
    /// </summary>
    public void AddOnAckSentEvent(Action<object> action) =>
        onAckSentEvents.Subscribe("ACK", action);
    
    /// <summary>
    /// Adds a response to receiving a message with the given signature.
    /// Response events with the signature "ACK" will always be called regardless of the signature of the received message.
    /// </summary>
    /// <param name="signature">The messages with this signature will be responded to.</param>
    /// <param name="response">Given the received data, create a response with it.</param>
    public void AddResponseTo([DisallowNull] string signature, Func<List<NetworkPackage>, List<NetworkPackage>> response) =>
        respondEvents.Subscribe(signature,
            o => SendResponseTo(signature, response, ((int, List<NetworkPackage>))o));
    
    /// <summary>
    /// Same as <see cref="AddResponseTo(string,System.Func{System.Collections.Generic.List{NetworkPackage},System.Collections.Generic.List{NetworkPackage}})"/>
    /// but with a single package as the response.
    /// </summary>
    public void AddResponseTo([DisallowNull] string signature,
        Func<List<NetworkPackage>, NetworkPackage> response) =>
        respondEvents.Subscribe(signature,
            o => SendResponseTo(signature, d => new List<NetworkPackage> { response(d) },
                ((int, List<NetworkPackage>))o));
    
    public Action<List<NetworkPackage>> AddDelayedResponseTo([DisallowNull] string signature,
        Action<List<NetworkPackage>> receiveData)
    {
        onDataReceivedEvents.Subscribe(signature, o => receiveData((List<NetworkPackage>)o));
        
        return response =>
        {
            delayedRespondEvents.Subscribe(signature,
                o => SendResponseTo(signature, _ => response, ((int, List<NetworkPackage>))o));
            delayedRespondEvents.Raise(signature, false, "delayedRespondEvent");
        };
    }
    
    /// <summary>
    /// Sends a response after receiving a message.
    /// </summary>
    private void SendResponseTo(
        [DisallowNull] string signature, 
        Func<List<NetworkPackage>,List<NetworkPackage>> response, 
        (int, List<NetworkPackage>) socketIndexAndMessage, 
        bool clearResponseSentEvents = false)
    {
        //ACK responses contain the signature as the message
        List<NetworkPackage> resp = response(socketIndexAndMessage.Item2);
        
        if (!TryCreatePackage(signature, resp, out byte[] bytes))
            return;
        //Debug.Log("Connection list length " + connections.Count + " socket index " + socketIndexAndMessage.Item1);
        //Debug.Log($"sending: {resp[0].data} to {connections[socketIndexAndMessage.Item1].RemoteEndPoint} signature {signature}");
        connections[socketIndexAndMessage.Item1].SendAsync(bytes, SocketFlags.None).ContinueWith(
            t => logError = onResponseSentEvents.Raise(signature, t.Result, clearResponseSentEvents, "onResponseSentEvent"));
    }

    /// <summary>
    /// Tests if the clients are still connected, if not the sockets are removed from the connection list.
    /// <param name="signature">Signature of the message.</param>
    /// <param name="interval">The maximum time between messages.</param>
    /// <param name="info">Return true if a client just disconnected.</param>
    /// </summary>
    protected override bool IsDisconnected(string signature, int interval, out Socket info)
    {
        for (var i = 0; i < connections.Count; i++)
        {
            DateTime now = DateTime.Now;
            if (now.Subtract(lastReceveivedMessage[i]).TotalMilliseconds >= interval * 2)
            {
                info = connections[i];
                connections.RemoveAt(i);
                isConnectionReceiving.RemoveAt(i);
                lastReceveivedMessage.RemoveAt(i);
                return true;
            }
        }
        info = null;
        return false;
    }
}
