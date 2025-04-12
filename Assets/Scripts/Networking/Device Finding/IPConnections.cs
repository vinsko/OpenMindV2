// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// A class that can retrieve the ips for this device and loop through the local ip ranges of any given ip.
/// </summary>
public static class IPConnections
{
    /// <summary>
    /// The port every connection uses
    /// </summary>
    public const ushort Port = 64413;
    
    /// <summary>
    /// Gets all local ips assigned to the current device
    /// </summary>
    public static IPAddress[] GetOwnIps()
    {
        List<IPAddress> ipAddrList = new List<IPAddress>();
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            if (item.OperationalStatus == OperationalStatus.Up)
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        ipAddrList.Add(ip.Address);
        
        // Remove localhosts
        ipAddrList.RemoveAll(ip => ip.ToString() == "127.0.0.1");
        
        return ipAddrList.ToArray();
    }
    
    /// <summary>
    /// Loops through a local ip range. The given ip must be a local ip address.
    /// If given the local ip 192.168.21.45 for example, it loops through all ips from 192.168.0.0 to 192.168.255.255.
    /// </summary>
    /// <exception cref="Exception">Exception thrown if the given ip is not a local ip address.</exception>
    public static IEnumerable<IPAddress> LoopThroughLocalRange(IPAddress ipAddress)
    {
        string ip = ipAddress.ToString();
        //check for the 3 ranges
        if (ip.StartsWith("10."))
            return LoopThrough10();
        if (ip.StartsWith("172."))
            return LoopThrough172();
        if (ip.StartsWith("192.168."))
            return LoopThrough192();
        
        throw new Exception("Invalid local ipaddress");
    }
    
    private static IEnumerable<IPAddress> LoopThrough10()
    {
        byte[] ipAddress = new byte[4];
        ipAddress[0] = 10;
        foreach (byte n1 in LoopRange(0, 255))
        {
            ipAddress[1] = n1;
            foreach (byte n2 in LoopRange(0, 255))
            {
                ipAddress[2] = n2;
                foreach (byte n3 in LoopRange(0, 255))
                {
                    ipAddress[3] = n3;
                    yield return new IPAddress(ipAddress);
                }
            }
        }
        
    }
    
    private static IEnumerable<IPAddress> LoopThrough172()
    {
        byte[] ipAddress = new byte[4];
        ipAddress[0] = 172;
        foreach (byte n1 in LoopRange(16, 31))
        {
            ipAddress[1] = n1;
            foreach (byte n2 in LoopRange(0, 255))
            {
                ipAddress[2] = n2;
                foreach (byte n3 in LoopRange(0, 255))
                {
                    ipAddress[3] = n3;
                    yield return new IPAddress(ipAddress);
                }
            }
        }
    }
    
    private static IEnumerable<IPAddress> LoopThrough192()
    {
        byte[] ipAddress = new byte[4];
        ipAddress[0] = 192;
        ipAddress[1] = 168;
        foreach (byte n2 in LoopRange(0, 255))
        {
            ipAddress[2] = n2;
            foreach (byte n3 in LoopRange(0, 255))
            {
                ipAddress[3] = n3;
                yield return new IPAddress(ipAddress);
            }
        }
    }
    
    private static IEnumerable<byte> LoopRange(byte min, byte max)
    {
        for (int i = min; i <= max; i++)
            yield return (byte)i;
    }
}
