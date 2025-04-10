// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Acts similar to <see cref="NetworkEvents"/>, but with a delay.
/// When both raise is called and an input is supplied, the event is actually raised
/// </summary>
public class NetworkDelayedEvents
{
    private Dictionary<string, object> inputData = new();
    private NetworkEvents              events    = new();
    private HashSet<string>            raised    = new();
    
    /// <summary>
    /// Raises all events that are subscribed to the given signature, if an input has been supplied for this signature.
    /// If clear is set to true, all events regarding the given signature will be removed after called.
    /// </summary>
    public string Raise(string signature, bool clear, string eventName)
    {
        raised.Add(signature);
        if (inputData.ContainsKey(signature))
        {
            object value = inputData[signature];
            raised.Remove(signature);
            inputData.Remove(signature);
            return events.Raise(signature, value, clear, eventName);
        }
        
        return "";
    }
    
    
    /// <summary>
    /// Adds data to the given signature for when the event can be called. If the event was already raised, call the event.
    /// If data has already been given to a signature, it is overridden with the new data.
    /// </summary>
    public string InputData(string signature, object data, bool clear, string eventName)
    {
        inputData.TryAdd(signature, data);
        
        if (raised.Contains(signature))
        {
            object value = inputData[signature];
            raised.Remove(signature);
            inputData.Remove(signature);
            return events.Raise(signature, value, clear, eventName);
        }
        
        return "";
    }
        
    
    /// <summary>
    /// Subscribes the given action to the given signature.
    /// </summary>
    public void Subscribe(string signature, Action<object> action) =>
        events.Subscribe(signature, action);
    
}
