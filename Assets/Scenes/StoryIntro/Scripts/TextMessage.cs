// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A scriptable object to store all data involving a single character.
/// </summary>
[CreateAssetMenu(fileName = "newTextmessage", menuName = "TextMessage")]

public class TextMessage : ScriptableObject
{
    [SerializeField] public GameObject message;
    [SerializeField] public string     messageContent;
    public                  Sender     sender;
    
    public enum Sender
    {
        Player,
        Culprit,
        Empty
    }
}
