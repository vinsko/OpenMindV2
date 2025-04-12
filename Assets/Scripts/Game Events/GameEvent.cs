// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which handles events, 
/// </summary>
[CreateAssetMenu(menuName = "GameEvent")]
public class GameEvent : ScriptableObject
{
   public List<GameEventListener> listeners = new List<GameEventListener>();
   
   /// <summary>
   /// Raises an event through different methods signatures
   /// </summary>
   /// <param name="sender">The object which the method that is called belongs to</param>
   /// <param name="data">The parameters of the method that is called</param>
   public void Raise(Component sender, params object[] data)
   {
      for (int i = 0; i < listeners.Count; i++)
      {
         listeners[i].OnEventRaised(sender, data);
      }
   }
   
   /// <summary>
   /// Registers a new eventlistener
   /// </summary>
   /// <param name="listener">An instance of <see cref="GameEventListener"/></param>
   public void RegisterListener(GameEventListener listener)
   {
      if (!listeners.Contains(listener))
         listeners.Add(listener);
   }
   
   /// <summary>
   /// Unregisters an eventlistener
   /// </summary>
   /// <param name="listener">An instance of <see cref="GameEventListener"/> from <see cref="listeners"/></param>
   public void UnregisterListener(GameEventListener listener)
   {
      if (listeners.Contains(listener))
         listeners.Remove(listener);
   }
}
