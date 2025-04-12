// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
#if DEBUG && UNITY_EDITOR

using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if DEBUG && UNITY_EDITOR
using static UnityEditor.EditorUtility;
#endif

/// <summary>
/// Manages all debug options. Every debug message and error handling should go through this class
/// </summary>
public class DebugManager : MonoBehaviour
{
    /// <summary>
    /// This bool, if set to true, fully disables popups
    /// This value can be changed only through reflection, thus it must only be used in a testing environment
    /// That is why this is private, so the programmer knows what they are doing when changing this value.
    /// </summary>
    private static bool FullyDisablePopups = false;
    
    /// <summary>
    /// A boolean that shows whether the game is in debug mode or not. This boolean should be used when displaying any debug messages or other debug stuff.
    /// The value of this boolean is determined by whether the current branch is the main branch or not.
    /// </summary>
    public static bool IsDebug { get; private set; }
    
    /// <summary>
    /// A list of all conditions to be ignored.
    /// </summary>
    private HashSet<string> ignores = new HashSet<string>();

    /// <summary>
    /// A bool that determines whether pops are disabled
    /// </summary>
    private bool DisablePopups;
    
    private void Awake()
    {
        #if DEBUG && UNITY_EDITOR
        if (FullyDisablePopups)
            Destroy(this);
        
        IsDebug = true;
        #endif
        
        Debug.unityLogger.logEnabled = IsDebug;
    }
    
    void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }
    
    //Called when there is an exception
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        #if DEBUG && UNITY_EDITOR
        if (!FullyDisablePopups)
        {
            if (!(DisablePopups || ignores.Contains(condition) || type == LogType.Log))
            {
                /*int result = DisplayDialogComplex(type.ToString(),
                    $"Condition: {condition}\nStacktrace:\n{stackTrace}",
                    "OK", "Ignore this message from now on.", "Disable all pop-ups");*/
                int result = 0;
                if (result == 1)
                    ignores.Add(condition);
                else if (result == 2)
                    DisablePopups = true;
            }
        }
        #endif
    }
    
    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }
}
#endif