// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Net;
using NUnit.Framework;

public class IPConverterTests
{
    private Random random = new ();
    
    [Test, Order(0)]
    public void AddCharacterTest()
    {
        PossibleCharacterDefinition definition = new PossibleCharacterDefinition();
        //check if no characters exist yet
        Assert.AreEqual(0, definition.Count);
        
        //check if a is contained within the definition
        definition.AddChar('a');
        Assert.IsTrue(definition.DoesCharacterExist('a'));
        
        //check if only 1 character got added
        Assert.AreEqual(1, definition.Count);
        
        //check if a is still contained within the definition
        definition.AddChar('a');
        Assert.IsTrue(definition.DoesCharacterExist('a'));
        
        //check if still only 1 character is in the definition
        Assert.AreEqual(1, definition.Count);
    }
    
    [Test, Order(1)]
    public void RemoveCharacterTest()
    {
        PossibleCharacterDefinition definition = new PossibleCharacterDefinition();
        //add a character
        definition.AddChar('a');
        //remove the same character
        definition.RemoveChar('a');
        
        //check if the definition has no characters and does not contain the character 'a'
        Assert.AreEqual(0, definition.Count);
        Assert.IsFalse(definition.DoesCharacterExist('a'));
        
        //should do nothing and throw no errors
        definition.RemoveChar('a');
    }
    
    [Test, Order(2)]
    public void AddCharacterRangeTest()
    {
        PossibleCharacterDefinition definition = new PossibleCharacterDefinition();
        definition.AddCharRange(0, 100);
        
        //check if definition contains 101 characters
        Assert.AreEqual(101, definition.Count);
        
        //check if adding the same range does nothing
        definition.AddCharRange(0, 100);
        Assert.AreEqual(101, definition.Count);
    }
    
    [Test, Order(3)]
    public void RemoveCharacterRangeTest()
    {
        PossibleCharacterDefinition definition = new PossibleCharacterDefinition();
        definition.AddCharRange(0, 100);
        definition.RemoveCharRange(0, 100);
        
        //check if definition contains 0 characters
        Assert.AreEqual(0, definition.Count);
        
        //check if removing the same range does nothing
        definition.RemoveCharRange(0, 100);
        Assert.AreEqual(0, definition.Count);
    }
    
    [Test, Order(4)]
    public void CheckIndexMappingTest()
    {
        PossibleCharacterDefinition definition = new PossibleCharacterDefinition();
        definition.AddCharRange(0, 100);
        definition.RemoveChar('\u0000');
        char a = definition[69];
        int result = definition[a];
        Assert.AreEqual(69, result);
    }
    
    [Test, Order(5)]
    public void IPv4EncodingAndDecodingTest()
    {
        IPv4Converter converter = new IPv4Converter();
        //convert to code and back
        string code = converter.ConvertToCode(IPAddress.Broadcast);
        IPAddress result = converter.ConvertToIPAddress(code);
        
        Assert.AreEqual(IPAddress.Broadcast, result);
        
        //check the shorten boolean
        code = converter.ConvertToCode(IPAddress.Any, true);
        Assert.AreEqual(1, code.Length);
    }
    
    [Test, Order(6), Repeat(1000)]
    public void RandomIPv4EncodingAndDecodingTest()
    {
        IPAddress randomIP = GetRandomIP();
        
        IPv4Converter converter = new IPv4Converter();
        //convert to code and back
        string code = converter.ConvertToCode(randomIP);
        IPAddress result = converter.ConvertToIPAddress(code);
        
        Assert.AreEqual(randomIP, result);
    }
    
    private IPAddress GetRandomIP()
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = (byte)random.Next(byte.MaxValue + 1);
        
        return new IPAddress(bytes);
    }
}
