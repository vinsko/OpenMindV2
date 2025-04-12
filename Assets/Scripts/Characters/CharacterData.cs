// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object to store all data involving a single character.
/// </summary>
[CreateAssetMenu(fileName = "newCharacter", menuName = "Character")]

public class CharacterData : ScriptableObject
{
    public                   string characterName;
    public                   int    id;
    public                   Sprite neutralAvatar;
    public                   Sprite happyAvatar;
    public                   Sprite unhappyAvatar;
    [Range(0.5f, 2f)] public float  voicePitch = 1;
    public Vector2 facePivot = new(0.5f, 0.8f);

    [SerializeField] public DialogueContainer firstGreeting;
    [SerializeField] public DialogueContainer greeting;
    [SerializeField] public KeyValuePair[]    answers;
    [SerializeField] public DialogueLines[]   greetings;
}

/// <summary>
/// KeyValuePair & DialogueLines must be individual objects in order to show up in the inspector
/// </summary>
[Serializable] public struct KeyValuePair
{
    [SerializeField] public Question question;
    [SerializeField] public DialogueContainer answer;
    [SerializeField] public List<string> trait;
}

/// <summary>
/// KeyValuePair & DialogueLines must be individual objects in order to show up in the inspector
/// </summary>
[Serializable] public struct DialogueLines
{
    [SerializeField]
    public List<string> lines;
}

/// <summary>
/// KeyValuePair & DialogueLines must be individual objects in order to show up in the inspector
/// </summary>
[Serializable] public struct DialogueLine
{
    [SerializeField]
    public string line;
}
