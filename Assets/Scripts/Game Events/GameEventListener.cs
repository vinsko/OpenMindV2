// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class CustomGameEvent : UnityEvent<Component, object[]> { }

/// <summary>
/// A class that listens for events. 
/// <see cref="GameEvent"/> uses instances of this class to handle events
/// </summary>
public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;

    public CustomGameEvent response;
    
    /// <summary>
    /// Registers this listener in the list of active listeners in <see cref="GameEvent"/>
    /// </summary>
    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    /// <summary>
    /// Unregisters this listener from the list of active listeners in <see cref="GameEvent"/>
    /// </summary>
    private void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }

    /// <summary>
    /// Invokes the method when the event that this listener is waiting for is raised
    /// </summary>
    /// <param name="sender">The object which the method that is called belongs to</param>
    /// <param name="data">The parameters of the method that is called</param>
    public void OnEventRaised(Component sender, params object[] data)
    {
        response.Invoke(sender, data);
    }

}
