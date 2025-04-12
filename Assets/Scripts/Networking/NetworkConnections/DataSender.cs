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
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Facilitates sending data over a network.
/// </summary>
public class DataSender : DataNetworker
{
    private bool                      isListeningForResponse;
    private bool                      connected;
    private List<AcknowledgementTime> acknowledgementTimes;
    
    
    /// <summary> when a connection is made with the listener, input is the task associated with making the connection </summary>
    private NetworkEvents onConnectEvents;
    /// <summary> when a connection timed out, input is nothing </summary>
    private NetworkEvents onConnectionTimeoutEvents;
    /// <summary> when data is sent to the listener, input is the amount of bytes that were sent </summary>
    private NetworkEvents onDataSentEvents;
    /// <summary> when a response is received from the listener, input is the response obtained </summary>
    private NetworkEvents onReceiveResponseEvents;
    /// <summary> when an acknowledgement is received from the listener about a sent message,
    /// input is the message received with the ack: this is always the signature of the received message by the listener before sending the ack</summary>
    private NetworkEvents onAckReceievedEvents;
    /// <summary> when an acknowlegement is not received within the timeout period, input is the signature of the timeout message </summary>
    private NetworkEvents onAckTimeoutEvents;
    /// <summary> when an attempt is made to listen for a response, but the sender was disconnected from the host, input is nothing </summary>
    private NetworkEvents onNotConnectedListeningEvents;

    
    
    /// <summary>
    /// Creates a data sender object using an IPAddress and a port to create an endpoint.
    /// The ipAddress should point to the device you want to send the message to.
    /// </summary>
    public DataSender([DisallowNull] IPAddress ipAddress, ushort port, string pingSignature) : base(ipAddress, port)
    {
        onDataSentEvents = new NetworkEvents();
        onReceiveResponseEvents = new NetworkEvents();
        onConnectEvents = new NetworkEvents();
        onAckReceievedEvents = new NetworkEvents();
        onAckTimeoutEvents = new NetworkEvents();
        onConnectionTimeoutEvents = new NetworkEvents();
        onNotConnectedListeningEvents = new NetworkEvents();
        
        acknowledgementTimes = new List<AcknowledgementTime>();
        
        //when an ack is received, untrack all ackTimes that match the received signature.
        // onAckReceievedEvents.Subscribe("ACK", signature =>
        //     acknowledgementTimes = acknowledgementTimes.FindAll(at => at.Signature != (string)signature));
        
        onAckTimeoutEvents.Subscribe(pingSignature, _ =>
        {
            if (connected)
                onDisconnectedEvents.Raise("Disconnect", null, false, "onDisconnectedEvents");
        });
        AddOnDisconnectedEvent(_ => connected = false);
    }
    
    /// <summary>
    /// Starts an attempt to connect with the given endpoint.
    /// </summary>
    /// <param name="timeoutSeconds">
    /// A debug error is thrown when the sender is not connected with the given endpoint after the given timeout amount.
    /// </param>
    /// <param name="clearDataSentEvents">If set to true, the actions called after connecting with the host are removed from the event.</param>
    public IEnumerator Connect(float timeoutSeconds, bool clearDataSentEvents = false)
    {
        bool timeout = false;
        GiveDisplayWarning();
        
        //check if you are already connected
        if (!connected)
        {
            Task connecting = socket.ConnectAsync(endPoint)
                .ContinueWith(t =>
                {
                    if (!timeout && t.IsCompletedSuccessfully)
                    {
                        connected = true;
                        logError = onConnectEvents.Raise("Connect", t, clearDataSentEvents,
                            "onDataSentEvent");
                    }
                });

            DateTime start = DateTime.Now;
            while (!connecting.IsCompleted)
            {
                double diff = (DateTime.Now - start).TotalMilliseconds;
                if (diff > timeoutSeconds * 1000)
                {
                    Debug.Log("while loop timeout");
                    onConnectionTimeoutEvents.Raise("Timeout", null, false, "onConnectionTimeoutEvent");
                    timeout = true;
                    break;
                }
                
                yield return null;
            }

            if (!connected && !timeout)
            {
                Debug.Log("completed task loop timeout");
                onConnectionTimeoutEvents.Raise("Timeout", null, false, "onConnectionTimeoutEvent");
            }
        }
        else
            logWarning = "Socket was already connected, so nothing happened.";
        
    }
    
    /// <summary>
    /// Sends the data in an async way to the target.
    /// Raises the actions that are subscribed to the data sent events.
    /// </summary>
    /// <param name="signature">
    /// The signature to give this package. A response, if any is sent, will have the same signature.
    /// This signature is used to identify the response and use the right function on it.
    /// </param>
    /// <param name="payload">The data to send through the network.</param>
    /// <param name="acknowledgementTimeoutSeconds">The amount of time to wait for an acknowledge response from the host to confirm the message has been received.</param>
    /// <param name="clearDataSentEvents">If set to true, all events connected to the given signature will be cleared after they are called.</param>
    public void SendDataAsync(string signature, IEnumerable<NetworkPackage> payload, float acknowledgementTimeoutSeconds, bool clearDataSentEvents = false)
    {
        //cannot send data when not connected
        if (!socket.Connected)
        {
            Debug.LogWarning("Cannot send data when the socket is not connected.");
            return;
        }
        
        if (!TryCreatePackage(signature, payload, out byte[] bytes))
            return;
        
        socket.SendAsync(bytes, SocketFlags.None).ContinueWith(t =>
        {
            logError = onDataSentEvents.Raise(signature, t.Result, clearDataSentEvents, "onDataSentEvent");
            if (logError != "")
                return;
            
            acknowledgementTimes.Add(new AcknowledgementTime(signature, acknowledgementTimeoutSeconds));
        });
    }
    
    /// <summary>
    /// Alias for <see cref="SendDataAsync(string,System.Collections.Generic.IList{NetworkPackage},bool)"/>,
    /// but with a single network package as the payload.
    /// </summary>
    public void SendDataAsync(string signature, NetworkPackage payload, float acknowledgementTimeoutSeconds, bool clearEvents = false) =>
        SendDataAsync(signature, new List<NetworkPackage> { payload }, acknowledgementTimeoutSeconds, clearEvents);
    
    /// <summary>
    /// Alias for <see cref="SendDataAsync(string,System.Collections.Generic.IList{NetworkPackage},bool)"/>,
    /// but with no payload.
    /// </summary>
    public void SendDataAsync(string signature, float acknowledgementTimeoutSeconds, bool clearEvents = false) =>
        SendDataAsync(signature, new List<NetworkPackage>(), acknowledgementTimeoutSeconds, clearEvents);
    
    /// <summary>
    /// Listens for responses in a coroutine.
    /// Events relating to receiving a response will be called when a response is received.
    /// After sending a message, an acknowledgement will be sent from the host to confirm the message reached the host.
    /// No events will be called when the response contains the ACK signature.
    /// A special onAcknowledgementFail event will be called when no ACK with the signature of the sent messages was received within the timeout.
    /// </summary>
    /// <param name="clearResponseEvents">If set to true, the actions called after receiving a response are removed from the event.</param>
    public IEnumerator ListenForResponse(float listenNotConnectedInterval, bool clearResponseEvents = false)
    {
        isListeningForResponse = true;

        while (isListeningForResponse)
        {
            if (!socket.Connected)
            {
                onNotConnectedListeningEvents.Raise("Disconnect", null, false, "onNotConnectedListeningEvents");
                yield return new WaitForSeconds(listenNotConnectedInterval);
            }
            else
            {
                byte[] buffer = new byte[NetworkPackage.MaxPackageSize];
                Task task = socket.ReceiveAsync(buffer, SocketFlags.None).ContinueWith(
                    receivedByteAmount =>
                    {
                        //catch all just in case an error slips through
                        try
                        {
                            if (!TryGetConvertData(buffer, receivedByteAmount,
                                    out List<List<NetworkPackage>> networkData))
                                return;
                            
                            foreach (var networkPackage in networkData)
                                HandleReceivedData(networkPackage, clearResponseEvents);
                        }
                        catch (AggregateException e)
                        {
                            Debug.Log("Client: " + e);
                            onDisconnectedEvents.Raise("Disconnect", null, true, "onDisconnectedEvents");
                        }
                        catch (Exception e)
                        {
                            logError = "Receiving message failed with error: " + e;
                        }
                    });
                
                yield return new WaitUntil(() =>
                {
                    CheckForTimeouts();
                    return task.IsCompleted;
                });
            }
        }
    }
    
    /// <summary>
    /// Stops the sender from listening for responses
    /// </summary>
    public void StopListeningForResponses() => isListeningForResponse = false;
    
    private void HandleReceivedData(List<NetworkPackage> networkData, bool clearResponseEvents)
    {
        string signature = networkData[0].GetData<string>();
        
        List<NetworkPackage> receivedTailPackages =
            networkData.Skip(1).ToList();
        
        if (signature == "ACK")
        {
            //check whether the received message is a signature: 
            if (receivedTailPackages.Count != 1)
            {
                logError = "ACK was not received with a signature";
                return;
            }
            
            try
            {
                logError = onAckReceievedEvents.Raise(signature,
                    receivedTailPackages[0].GetData<string>(), clearResponseEvents, "onAckReceivedEvent");
                
                //Debug.Log($"Received ack: {receivedTailPackages[0].GetData<string>()}");
                
                acknowledgementTimes.RemoveAll(ackt => ackt.Signature == receivedTailPackages[0].GetData<string>());
            }
            catch (InvalidCastException e)
            {
                logError =
                    $"ACK was not received with a signature, received error {e}";
                return;
            }
        }
        else
            logError = onReceiveResponseEvents.Raise(signature,
                receivedTailPackages,
                clearResponseEvents, "onResponseEvent");
    }
    
    /// <summary>
    /// Checks if any acks have timed out, meaning no ack was received in time.
    /// </summary>
    private void CheckForTimeouts()
    {
        List<AcknowledgementTime> timeouts =
            acknowledgementTimes.FindAll(ackt => ackt.HasTimedOut());

        foreach (var acknowledgementTime in timeouts)
        {
            logWarning = onAckTimeoutEvents.Raise(acknowledgementTime.Signature,
                acknowledgementTime.Signature, false, "onAckTimeoutEvents");
        }

        acknowledgementTimes.RemoveAll(timeouts.Contains);
    }
    
    /// <summary>
    /// Adds an action to the event of connecting with a host.
    /// When connecting to a host, the given action is called.
    /// The object is the task created when attempting to connect with a host.
    /// </summary>
    public void AddOnConnectEvent(Action<object> action) =>
        onConnectEvents.Subscribe("Connect", action);
    
    /// <summary>
    /// Adds an action to the event of timeing out while trying to connect to the host
    /// The object is null.
    /// </summary>
    public void AddOnConnectionTimeoutEvent(Action<object> action) =>
        onConnectionTimeoutEvents.Subscribe("Timeout", action);
    
    /// <summary>
    /// Adds an action to the event of completing the send action.
    /// When the sending of a package with the given signature completes, the given action is called.
    /// The object parameter of the action is the amount of bytes that were sent.
    /// </summary>
    public void AddOnDataSentEvent(string signature, Action<object> action) =>
        onDataSentEvents.Subscribe(signature, action);
    
    /// <summary>
    /// Adds an action to the event of receiving a response.
    /// When the receiving a package with the given signature, the given action is called.
    /// The object parameter of the action is the data received.
    /// </summary>
    public void AddOnReceiveResponseEvent(string signature, Action<object> action) =>
        onReceiveResponseEvents.Subscribe(signature, action);
    
    /// <summary>
    /// Adds an action to the event of connecting with a host.
    /// When connecting to a host, the given action is called.
    /// The object is the task created when attempting to connect with a host.
    /// </summary>
    public void AddOnAckReceivedEvent(Action<object> action) =>
        onAckReceievedEvents.Subscribe("ACK", action);
    
    /// <summary>
    /// Adds an action to the event of not receiving an ack within the timeout period.
    /// When not receiving an ack from the host within the given timeout period, the given action is called.
    /// The object is the signature of the message who was not acknowledged by the host.
    /// </summary>
    public void AddOnAckTimeoutEvent(string signature, Action<object> action) =>
        onAckTimeoutEvents.Subscribe(signature, action);
    
    /// <summary>
    /// Adds an action to the event of listening for a response from the host while not being connected to the host.
    /// The object is the signature of the message who was not acknowledged by the host.
    /// </summary>
    public void AddOnNotConnectedListeningEvents(Action<object> action) =>
        onNotConnectedListeningEvents.Subscribe("Disconnect", action);

    /// <summary>
    /// Tests if the client is still connected to the host.
    /// <param name="info">Return true if the client just got disconnected.</param>
    /// </summary>
    protected override bool IsDisconnected(string signature, int interval, out Socket info)
    {
        info = null;
        if (!connected)
            return false;
        
        SendDataAsync(signature, NetworkPackage.CreatePackage("Plz give ping!"), interval/500f);
        return false;
    }
}
