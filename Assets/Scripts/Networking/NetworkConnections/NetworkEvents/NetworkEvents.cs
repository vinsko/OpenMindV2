// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A class to handle events regarding networking.
/// Every event is a signature matched with an action.
/// If the events are raised, all actions with the matching signature will be called.
/// Every action has an object as an input.
///
/// <para>Note: While it is possible to add multiple actions to the same signature, this should be avoided,
/// since it makes tracking network packets more difficult.</para>
/// </summary>
public class NetworkEvents
{
    /// <summary>
    /// This dictionary stores all events.
    /// </summary>
    private Dictionary<string, List<Action<object>>> events = new ();
    
    /// <summary>
    /// Raises all events that are subscribed to the given signature.
    /// If clear is set to true, all events regarding the given signature will be removed after called.
    /// </summary>
    public string Raise(string signature, object data, bool clear, string eventName)
    {
        if (!events.ContainsKey(signature))
            return "";
        
        try
        {
            foreach (var action in events[signature])
                action(data);
        }
        catch (Exception e)
        {
            return $"{eventName} with signature {signature} returned exception: {e}";
        }
        
        if (clear)
            events[signature] = new List<Action<object>>();
        
        return "";
    }
    
    /// <summary>
    /// Subscribes the given action to the given signature.
    /// </summary>
    public void Subscribe(string signature, Action<object> action)
    {
        if (!events.ContainsKey(signature))
            events.Add(signature, new List<Action<object>>());
        
        events[signature].Add(action);
    }
}
