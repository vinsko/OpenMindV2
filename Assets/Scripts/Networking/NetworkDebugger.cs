// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to process errors and show it to the console.
/// </summary>
public abstract class NetworkDebugger
{
    protected string logError   = "";
    protected string logWarning = "";
    
    private bool isDisplayingDebugs;
    
    
    /// <summary>
    /// Displays any debug messages that were called during any function.
    /// When some async functions throw exception, they aren't caught by unit and go unnoticed.
    /// This function checks every interval whether any Debug messages were called.
    /// </summary>
    public IEnumerator DisplayAnyDebugs(float intervalSeconds)
    {
        isDisplayingDebugs = true;
        
        while (isDisplayingDebugs)
        {
            if (logError != "")
            {
                Debug.LogError(logError);
                logError = "";
            }
            
            if (logWarning != "")
            {
                Debug.LogWarning(logWarning);
                logWarning = "";
            }
            
            yield return new WaitForSeconds(intervalSeconds);
        }
    }
    
    /// <summary>
    /// Whenever no listening for debugs are happening, show warnings to the console.
    /// </summary>
    protected void GiveDisplayWarning()
    {
        if (!isDisplayingDebugs)
            Debug.LogWarning(
                "No debug messages from events are displayed, if any errors are thrown in these events, this will not be displayed in the unity console.");
    }
}
