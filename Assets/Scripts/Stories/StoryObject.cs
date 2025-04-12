// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// A scriptable object which contains the settings pertaining to a Story-type.
/// </summary>
[CreateAssetMenu(fileName = "newStory", menuName = "Story")]
public class StoryObject : ScriptableObject
{
    [SerializeField] private string storyName;
    [SerializeField] public  int    storyID;
    
    [Header("Story Assets")]
    [SerializeField] public GameObject dialogueBackground;
    [SerializeField] public GameObject        hintBackground;
    [SerializeField] public GameObject[]      additionalHintBackgroundObjects;
    [SerializeField] public GameObject        epilogueBackground;
    [SerializeField] public AudioClip         storyIntroMusic;
    [SerializeField] public AudioClip         storyGameMusic;
    [SerializeField] public AudioClip         storyEpilogueMusic;
    [SerializeField] public DialogueContainer storyEpilogueWonDialogue;
    [SerializeField] public DialogueContainer storyEpilogueLossDialogueCulprit;
    [SerializeField] public DialogueContainer storyEpilogueLossDialogueNPC;

    [Header("Game Settings")] 
    [SerializeField] public string victimDialogue;
    [SerializeField] public string[] hintDialogue;
    [SerializeField] public string[] noMoreHintsDialogue;
    [SerializeField] public string[] preEpilogueDialogue;
    [SerializeField] public int numberOfCharacters;            // How many characters each session should have
    [SerializeField] public int numQuestions; // Amount of times the player can ask a question
    [SerializeField] public int minimumRemaining; // The amount of active characters at which the session should end
    [SerializeField] public bool immediateVictim; // Start the first round with an inactive characters
}