// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// A child of DialogueObject.
/// Executing this object places a text field on the screen, so that the player can answer an open question.
/// </summary>
public class OpenResponseDialogueObject : DialogueObject
{
    // The answer of the open question.
    public              string       answer = "";
    [CanBeNull] public  Sprite       image;
    [CanBeNull] public List<string> dialogue;
    [CanBeNull] private Emotion      emotion;

    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="background">The background</param>
    public OpenResponseDialogueObject([CanBeNull] List<string> dialogue, [CanBeNull] Sprite image, GameObject[] background, Emotion emotion)
    {
        // Set this object's local variables to match the parameter-values of the constructor
        this.background = background;
        this.image = image;
        this.emotion = emotion;
        this.dialogue = dialogue;
    }

    /// <summary>
    /// Creates on open text field in which the player can type their response
    /// </summary>
    public override void Execute()
    {
        // TODO: Print segments/text at the same time.. or print, and when its done typewriting, open the openanswerbox below.
        var dm = DialogueManager.dm;
        
        dm.ReplaceBackground(background, emotion);
        dm.PrintImage(image);
        // Asks Dialoguemanager to open an openquestion-textbox
        dm.CreateOpenQuestion(dialogue);
    }
}
