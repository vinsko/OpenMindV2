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
/// Acts as a parent class for DataSender and DataListener.
/// This class maintains and displays any error messages caught in events.
/// This class also contains some utility functions for shared code between DataSender and DataListener.
/// </summary>
public abstract class DataNetworker : NetworkDebugger, IDisposable
{
    protected Socket     socket;
    protected IPEndPoint endPoint;

    protected NetworkEvents onDisconnectedEvents;

    private bool isCheckingForDisconnection;
    
    protected DataNetworker([DisallowNull] IPAddress ipAddress, ushort port)
    {
        try
        {
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
        catch (SocketException e)
        {
            Debug.LogError("Created an invalid socket, got error: " + e);
            socket = null;
            return;
        }
        
        endPoint = new IPEndPoint(ipAddress, port);
        onDisconnectedEvents = new NetworkEvents();
    }
    
    /// <summary>
    /// Attempts to deconstruct network bytes into a list of networkpackages
    /// </summary>
    /// <returns>true if the conversion succeeded, otherwise false</returns>
    protected bool TryGetConvertData(byte[] buffer, Task<int> receivedByteAmount, out List<List<NetworkPackage>> networkData)
    {
        networkData = null;
        
        if (receivedByteAmount.Result > NetworkPackage.MaxPackageSize)
        {
            logWarning = $"Received package was too large, expected a package of " +
                         $"{NetworkPackage.MaxPackageSize} bytes or less, but got " +
                         $"{receivedByteAmount.Result} bytes. The incoming data was rejected.";
            return false;
        }
        
        string rawData = Encoding.UTF8.GetString(buffer);
        rawData = rawData.TrimEnd('\0');
        string[] receivedRawDatas = rawData.Split('\u0004');
        networkData = new List<List<NetworkPackage>>();
        
        foreach (string receivedRawData in receivedRawDatas)
        {
            if (receivedRawData == "")
                continue;
            try
            {
                networkData.Add(JsonConvert.DeserializeObject<List<NetworkPackage>>(receivedRawData));
            }
            catch (JsonException e)
            {
                logWarning = "Reading received response with json failed: " + e;
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Attempts to construct network bytes from a list of networkpackages
    /// </summary>
    /// <returns>true if the conversion succeeded, otherwise false</returns>
    protected bool TryCreatePackage(string signature, IEnumerable<NetworkPackage> data, out byte[] buffer)
    {

        List<NetworkPackage> networkData = new List<NetworkPackage> { NetworkPackage.CreatePackage(signature) };
        networkData.AddRange(data);

        string rawData = JsonConvert.SerializeObject(networkData) + '\u0004';
        buffer = Encoding.UTF8.GetBytes(rawData);
        if (buffer.Length > NetworkPackage.MaxPackageSize)
        {
            Debug.LogError("Package was too large.");
            buffer = null;
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Tests if the client and host are still connected, if not call the disconnection events.
    /// </summary>
    /// <param name="signature">signature of the messages</param>
    /// <param name="intervalSeconds">the interval for checking for disconnections, in seconds</param>
    public IEnumerator IsDisconnected(string signature, float intervalSeconds)
    {
        isCheckingForDisconnection = true;
        
        while (isCheckingForDisconnection)
        {
            if (IsDisconnected(signature,(int)(intervalSeconds * 1000), out Socket disconnectedSocket))
            {
                logError = onDisconnectedEvents.Raise("Disconnect", disconnectedSocket, false, "onDisconnectedEvents");
            }
            
            if (isCheckingForDisconnection)
                yield return new WaitForSeconds(intervalSeconds);
        }
    }

    protected abstract bool IsDisconnected(string signature, int interval, out Socket info);
    
    /// <summary>
    /// Adds an event to the action of a socket disconnecting. The input is the socket that got disconnected.
    /// </summary>
    public void AddOnDisconnectedEvent(Action<object> action) =>
        onDisconnectedEvents.Subscribe("Disconnect", action);
    
    /// <summary>
    /// Dispose of the socket when quitting the game.
    /// </summary>
    public void Dispose()
    {
        socket.Dispose();
    }
}
