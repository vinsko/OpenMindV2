// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using UnityEngine;
using UnityEngine.Playables;

public class ResetPlayableDirector : MonoBehaviour
{
    private PlayableDirector director;
    
    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }
    
    private void OnEnable()
    {
        // Reset the timeline state when the game starts
        if (director != null)
        {
            director.Stop();
            director.time = 0;
            director.Evaluate(); // Force the timeline to reset to its starting state
        }
    }
}