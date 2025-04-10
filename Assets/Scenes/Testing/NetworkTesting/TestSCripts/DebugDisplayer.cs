using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugDisplayer : MonoBehaviour
{
    [SerializeField]
    public TMP_Text textfield;
    
    private       TextLog log          = new TextLog(10);
    private const bool    DisplayTrace = false;
    
    void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }
    
    //Called when there is an exception
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        string s = $"Type: {type}\n" +
                   $"Condition: {condition}\n";
        if (DisplayTrace)
            s += $"Trace: {stackTrace}\n\n";
        
        log.AddLog(s);
        textfield.text = log.DisplayLog();
    }
    
    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }
}

class TextLog
{
    private int      maxSize;
    private int      pointer = 0;
    private string[] log;
    
    public TextLog(int maxSize)
    {
        this.maxSize = maxSize;
        log = new string[maxSize];
    }
    
    public void AddLog(string inLog)
    {
        log[pointer] = inLog;
        pointer++;
        pointer %= maxSize;
    }
    
    public string DisplayLog()
    {
        string full = "";
        for (int i = 0; i < log.Length; i++)
            full += log[(i+pointer)% maxSize] + "\n";
        
        return full;
    }
}