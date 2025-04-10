// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// Converts an IPv4 address to a code and back.
/// </summary>
public class IPv4Converter
{
    private PossibleCharacterDefinition definition;
    private int                         codeLength;
    
    /// <summary>
    /// Creates an IPConverter instance
    /// </summary>
    public IPv4Converter()
    {
        definition = DefaultDefinition();
        codeLength = CalculateMinimumCodeLength(definition);
        
        if (!CheckDefinition(codeLength, definition))
            throw new ArgumentException(
                $"Cannot encode an IPv4 address into a code of length {codeLength}. Please use a longer code length or a larger character definition.");
    }
    
    /// <summary>
    /// Creates an IPConverter instance
    /// </summary>
    /// <param name="definition">The definition to use for the encoding.</param>
    public IPv4Converter(PossibleCharacterDefinition definition)
    {
        this.definition = definition;
        codeLength = CalculateMinimumCodeLength(definition);
        
        if (!CheckDefinition(codeLength, definition))
            throw new ArgumentException(
                $"Cannot encode an IPv4 address into a code of length {codeLength}. Please use a longer code length or use a custom character definition.");
    }
    
    /// <summary>
    /// Creates an IPConverter instance
    /// </summary>
    /// <param name="codeLength">The length of the code to generate.</param>
    public IPv4Converter(int codeLength)
    {
        this.codeLength = codeLength;
        definition = DefaultDefinition();
        
        if (!CheckDefinition(codeLength, definition))
            throw new ArgumentException(
                $"Cannot encode an IPv4 address into a code of length {codeLength}. Please use a longer code length or use a custom character definition.");
    }
    
    /// <summary>
    /// Creates an IPConverter instance
    /// </summary>
    /// <param name="codeLength">The length of the code to generate.</param>
    /// <param name="definition">The definition to use for the encoding.</param>
    public IPv4Converter(int codeLength, PossibleCharacterDefinition definition)
    {
        this.codeLength = codeLength;
        this.definition = definition;
        
        if (!CheckDefinition(codeLength, definition))
            throw new ArgumentException(
                $"Cannot encode an IPv4 address into a code of length {codeLength}. Please use a longer code length or a larger character definition.");
    }
    
    private PossibleCharacterDefinition DefaultDefinition()
    {
        PossibleCharacterDefinition definition = new PossibleCharacterDefinition();
        //add all numbers
        definition.AddCharRange(48, 57);
        //add all normal lowercase letters
        definition.AddCharRange(97, 122);
        
        //remove 0 and O to avoid confusion
        definition.RemoveChar('o');
        definition.RemoveChar('0');
        
        //remove i and l to avoid confusion
        definition.RemoveChar('i');
        definition.RemoveChar('l');
        return definition;
    }
    
    private int CalculateMinimumCodeLength(PossibleCharacterDefinition definition)
    {
        long totalPossibleIPv4Addresses = uint.MaxValue + (long)1;
        return (int)Math.Log(totalPossibleIPv4Addresses, definition.Count) + 1;
    }
    
    /// <summary>
    /// Checks whether a definition is large enough to encode an IPv4 address with the requested code length
    /// </summary>
    private bool CheckDefinition(int codeLength, PossibleCharacterDefinition definition)
    {
        long totalPossibleIPv4Addresses = uint.MaxValue + (long)1;
        long totalPossibleCodes = (long)Math.Pow(definition.Count, codeLength);
        return totalPossibleCodes >= totalPossibleIPv4Addresses;
    }
    
    /// <summary>
    /// Converts an IPv4 address to a code. If the given address is IPv6 it will be mapped to IPv4 if possible, otherwise an error is thrown
    /// </summary>
    /// <param name="address">The ipaddress to convert to a code</param>
    /// <param name="shorten">If set to false, if the resulting code is smaller than codeLength, it will be filled with 0 values to match codeLength</param>
    /// <exception cref="ArgumentException"></exception>
    public string ConvertToCode(IPAddress address, bool shorten = false)
    {
        //if ip is ipv6 and it cannot be converted to ipv4, return an error
        if (address.AddressFamily == AddressFamily.InterNetworkV6 && !address.IsIPv4MappedToIPv6)
            throw new ArgumentException(
                "Conversion only accepts IPv4 address and no IPv6 addresses");
        
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
            address = address.MapToIPv4();
        
        //convert the ipv4 address to a single number
        long total = 0;
        byte[] addressBytes = address.GetAddressBytes();
        for (int i = 0; i < addressBytes.Length; i++)
            total += (long)Math.Pow(byte.MaxValue + 1, i) * addressBytes[i];
        
        //convert it to a code
        string code = "";
        if (total == 0)
            code = definition[0].ToString();
        while (total > 0)
        {
            total = Math.DivRem(total, definition.Count, out long remainder);
            code = definition[(ushort)remainder] + code;
        }
        
        //pad the code if needed
        while (!shorten && code.Length < codeLength)
            code = definition[0] + code;
        
        return code;
    }
    
    /// <summary>
    /// Converts a code to an IPv4 address. If the code is invalid, and error will be thrown
    /// </summary>
    public IPAddress ConvertToIPAddress(string code)
    {
        if (code.Length > codeLength)
            throw new ArgumentException(
                "Invalid code. The code was longer than allowed.");
        
        if (!code.All(definition.DoesCharacterExist))
            throw new ArgumentException(
                "Invalid code. Some characters were not found in the given definition.");
        
        long total = 0;
        for (int i = 0; i < code.Length; i++)
            total += (long)Math.Pow(definition.Count, code.Length - i - 1) * definition[code[i]];
        
        //uint maxValue is the max value of an IPv4Address, check if the value of the code doesn't exceed it.
        if (total > uint.MaxValue)
            throw new ArgumentException(
                $"Invalid code. The total value of the code {total} was larger than the maximum allowed value for ipv4 addresses {uint.MaxValue}.");
        
        //convert the total into bytes
        byte[] addressBytes = new byte[4];
        for (int i = addressBytes.Length - 1; i >= 0; i--)
            addressBytes[i] = (byte)(total >> i * 8);
        
        return new IPAddress(addressBytes);
    }
}

/// <summary>
/// Defines a list of possible characters used for encoding and decoding in an IPConverter.
/// </summary>
public class PossibleCharacterDefinition
{
    private List<char> possibleCharacters = new();
    
    public ushort this[char index] => (ushort)possibleCharacters.IndexOf(index);
    public char this[ushort index] => possibleCharacters[index];
    public ushort Count => (ushort)possibleCharacters.Count;
    public bool DoesCharacterExist(char character) => possibleCharacters.Contains(character);
    
    public enum ModifyType
    {
        Add,
        Remove
    }
    
    /// <summary>
    /// Adds or removes a range of chars to the current definition.
    /// UTF-16 definition can be found <a href="https://www.fileformat.info/info/charset/UTF-16/list.htm">here</a>
    /// </summary>
    /// <param name="start">Starting number of the definition as defined by UTF-16, inclusive</param>
    /// <param name="end">Ending number of the definition as defined by UTF-16, inclusive</param>
    /// <param name="modifyType">Whether to remove or add the given range</param>
    public void ModifyCharRange(ushort start, ushort end, ModifyType modifyType)
    {
        for (int i = start; i <= end; i++)
            if (modifyType == ModifyType.Add)
                AddChar((char)i);
            else
                RemoveChar((char)i);
    }
    
    /// <summary>
    /// For documentation: see <see cref="ModifyCharRange"/>>
    /// </summary>
    public void AddCharRange(ushort start, ushort end) =>
        ModifyCharRange(start, end, ModifyType.Add);
    /// <summary>
    /// For documentation: see <see cref="ModifyCharRange"/>>
    /// </summary>
    public void RemoveCharRange(ushort start, ushort end) =>
        ModifyCharRange(start, end, ModifyType.Remove);
    
    /// <summary>
    /// Removes a single character to the current definition
    /// </summary>
    /// <param name="character">The character to remove. If it wasn't contained in the definition, nothing happens.</param>
    public void RemoveChar(char character)
    {
        possibleCharacters.Remove(character);
    }
    
    /// <summary>
    /// Adds a single character to the current definition
    /// </summary>
    /// <param name="character">The character to add. If the given character already exists in the definition, nothing happens.</param>
    public void AddChar(char character)
    {
        //if the definition already contains this character, do nothing.
        if (possibleCharacters.Contains(character))
            return;
        
        possibleCharacters.Add(character);
    }
}
