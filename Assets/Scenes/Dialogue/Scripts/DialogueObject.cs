// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// An abstract class containing the blueprint for the possible dialogue options.
/// Possible children are: ContentDialogueObject, DialogueDialogueQuestionObject, ResponseDialogueObject, and TerminateDialogueObject.
/// </summary>
public abstract class DialogueObject
{
    protected GameObject[] background;

    /// <summary>
    /// The possible responses to the dialogue object (when picturing a tree structure, these are the children of the object)
    /// </summary>
    public List<DialogueObject> Responses { get; set; } = new();

    /// <summary>
    /// Executes the logic of the given dialogue object
    /// </summary>
    public abstract void Execute();
}

/// <summary>
/// A child of DialogueObject. Executing this object simply writes its text to the screen.
/// A response must be set manually, otherwise the response is a TerminateDialogueObject.
/// </summary>
public class ContentDialogueObject : DialogueObject
{
    [CanBeNull] public List<string> dialogue;
    [CanBeNull] public Sprite       image;
    [CanBeNull] public Emotion      emotion;

    /// <summary>
    /// The constructor for <see cref="ContentDialogueObject"/>.
    /// </summary>
    /// <param name="dialogue">The text</param>
    /// <param name="background">The background</param>
    public ContentDialogueObject([CanBeNull] object dialogue, [CanBeNull] Sprite image, GameObject[] background, Emotion? emotion = null)
    {
        // Set this object's local variables to match the parameter-values of the constructor
        // Handle dialogue as string or List<string>
        this.dialogue = dialogue switch
        {
            string singleDialogue => new List<string> { singleDialogue },
            List<string> dialogueList => dialogueList,
            _ => new List<string>() // Default to an empty list if null or unsupported type
        };
        this.image = image; 
        this.background = background;
        if (emotion.HasValue)
            this.emotion = emotion.Value;
    }

    /// <summary>
    /// Writes the text to the screen
    /// </summary>
    public override void Execute()
    {
        var dm = DialogueManager.dm;
        
        dm.ReplaceBackground(background,emotion);
        dm.PrintImage(image);
        dm.WriteDialogue(dialogue);

        // If no response is given, terminate dialogue
        if (Responses.Count <= 0)
            Responses.Add(new TerminateDialogueObject());
    }
}

/// <summary>
/// A child of DialogueObject. Executing this object unloads the dialogue scene with no response.
/// </summary>
public class TerminateDialogueObject : DialogueObject
{
    /// <summary>
    /// Unloads the scene and loads NPCSelect
    /// </summary>
    public override void Execute()
    {
        // Invokes event, listener invokes CheckEndCycle, which loads NPCSelect.
        // Pass along the currentObject, which is used for the Epilogue scene and
        // pass along the currentrecipient, which is used to set the npc which was last talked to in NpcSelect.
        DialogueManager.dm.onEndDialogue.Raise(DialogueManager.dm, DialogueManager.dm.currentObject, DialogueManager.dm.currentRecipient);
    }
}

/// <summary>
/// A child of DialogueObject. Executing this object will show the 
/// previousMessages and the first element of remainingMessages.
/// A response for the next messages is automatically created.
/// </summary>
public class PhoneDialogueObject : DialogueObject
{
    private List<string> remainingMessages;
    private List<string> previousMessages;

    public PhoneDialogueObject(List<string> remainingMessages, List<string> previousMessages, GameObject[] background)
    {
        this.background = background;
        this.remainingMessages = remainingMessages;
        
        // Create an empty list of messages if there were no previous messages
        this.previousMessages = previousMessages ?? new List<string>();
        this.previousMessages.Add(remainingMessages[0]);

        // Remove the new message from the list
        this.remainingMessages.RemoveAt(0);
    }

    /// <summary>
    /// Write previousMessages and the first remainingMessage to the screen.
    /// Automatically adds next messages as response object.
    /// </summary>
    public override void Execute()
    {
        var dm = DialogueManager.dm;

        dm.ReplaceBackground(background);
        dm.WritePhoneDialogue(previousMessages);

        if (remainingMessages.Count <= 0)
            Responses.Add(new TerminateDialogueObject());
        else
            Responses.Add(new PhoneDialogueObject(remainingMessages, previousMessages, background));
    }
}