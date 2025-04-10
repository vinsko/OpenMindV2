// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// This is a container for dialogueobjects. it has a list of dialogueobjects, rather than a
/// dialogueobject having 'responses', it could be sequential
/// </summary>
[CreateAssetMenu(fileName = "newDialogue", menuName = "Dialogue")]
[Serializable]
public class DialogueContainer : ScriptableObject
{
    // Voicepitch of the dialogue
    [Range(0.5f, 2f)] public float voicePitch = 1;
    // segments of the dialogue in an array
    [SerializeField] public DialogueData[] segments;
    
    private          int          defaultLineLength = 30;
    private          char[]       punctuations      = {',','.', '!', '?', ':'};
    
    /// <summary>
    /// Takes an array of DialogueData, creates DialogueObjects of these,
    /// and strings them each sequentially as responses of the previous.
    /// This creates a sequential dialogue, during which backgrounds,
    /// images and dialogue-segments can vary.
    /// </summary>
    /// <returns></returns>
    public DialogueObject GetDialogue([CanBeNull] GameObject[] background = null, int? startIndex = null, int? endIndex = null)
    {
        if (background != null) 
            SetBackground(background, startIndex, endIndex);
        
        // Create the first piece of dialogue and initialize it as output.
        // we will add on to its responses in the for-loop below.
        DialogueObject output = SegmentDialogue(segments[0]);
        for (int i = 1; i < segments.Length; i++)
        {
            // Get dialogue-data from the correct line
            var data = segments[i];
            AppendToLeaf(output, SegmentDialogue(data));
        }
        
        // End with TerminateDialogueObject
        AppendToLeaf(output, new TerminateDialogueObject());
        
        return output;
    }

    /// <summary>
    /// Gets strings from segmented data
    /// </summary>
    public List<string> GetStrings()
    {
        List<string> output = new List<string>();
        foreach (var segment in segments)
            output.Add(segment.text);
        return output;
    }
    
    /// <summary>
    /// Appends a dialogueobject to the lead of a dialogue-tree.
    /// It finds the lowest-level node with no responses and adds
    /// the given dialogueobject as a response of that.
    /// </summary>
    public void AppendToLeaf(DialogueObject node, DialogueObject newLeaf)
    {
        // if the node has no responses, add the newleaf to its responses
        DialogueObject currentNode = node;
        while (currentNode.Responses.Count > 0)
        {
            currentNode = currentNode.Responses.First();
        }
        currentNode.Responses.Add(newLeaf);
    }

    public DialogueObject CreateDialogueObject(DialogueData data)
    {
        DialogueType dialogueType = data.type;
        GameObject[] background = data.background;
        
        List<string> text = new List<string>();
        text.Add(data.text);
        var emotion = data.emotion;
        switch (dialogueType)
        {
            case DialogueType.ContentDialogue:
                // We create a new ContentDialogueObject, containing text, image or background
                // The first of these two could be null, but that is handled by the ContentDialogueObject.
                return new ContentDialogueObject(text, data.image, background, emotion);
            case DialogueType.OpenResponseDialogue:
                // TODO: Only make the last one of this segment an openresponsedialogue, and keep the rest as 'contentdialogue'.
                return new OpenResponseDialogueObject(text, data.image, background, emotion);
            default:
                Debug.LogError("Could not create DialogueObject from DialogueContainer: invalid type");
                return null;
        }
        
    }

    /// <summary>
    /// Takes one dialogue data, splits it by the characterl creates separate dialogueobjects 
    /// </summary>
    private DialogueObject SegmentDialogue(DialogueData data)
    {
        int maxLineLength = SettingsManager.sm == null ? defaultLineLength : SettingsManager.sm.maxLineLength;
        string remainingText = data.text;
        DialogueType dialogueType = data.type;
        DialogueObject output = null;

        // see if text is too long; take the first X characters of the text, then find the last punctuation,
        // and split it there and pass it to make a dialogueobject
        // recurse this on the rest.
        // In case of double punctuations that just didnt fit in the maxlinelength, we remove them from the start of the string
        while (remainingText.Length > 0)
        {
            DialogueObject newDialogue = null;
            string segmentText = "";
            // If the remainingText fits within maxLineLength..
            if (remainingText.Length <= maxLineLength)
            {
                 segmentText = RemovePunctuationFromStart(remainingText);
                 remainingText = "";
            }
            // If the remainingtext needs to be segmented.. 
            else
            {
                // takes a substring of the full string containing the first X characters
                // Then finds the index of the last punctuation-character in that string.
                // Trim the remaining text, to the last found punctuation in the remainingText-string
                // + 1 at the end to fix missing punctuation (otherwise it would remove the final punctuation)
                int textLength = 
                    FindLastPunctuation(
                        remainingText.Substring(
                            0, Mathf.Min(remainingText.Length, maxLineLength))) + 1;
                
                // We create two substrings;
                // One for the first segment of dialogue that we will create a dialogueobject of.
                // And a second for the remainingtext, which we re-assign.
                segmentText = remainingText.Substring(0, textLength);
                remainingText = remainingText.Substring(textLength);
                // Drop the first characters of remainingtext if they are part of punctuation or a space
                remainingText = RemovePunctuationFromStart(remainingText);
            }
            
            // If this segment is an OpenResponse-question but we are not finished asking the question,
            // ..we will instead create this segment as a ContentDialogueObject
            if (data.type == DialogueType.OpenResponseDialogue && remainingText.Length > 0)
                dialogueType = DialogueType.ContentDialogue;
            if (data.type == DialogueType.OpenResponseDialogue && remainingText.Length == 0)
                dialogueType = DialogueType.OpenResponseDialogue;

            // With the found segment, we now create a DialogueObject.    
            newDialogue = CreateDialogueObject(new DialogueData(dialogueType, data.emotion, segmentText, data.image, data.background));
            
            // We set the ouput to include the newDialogue
            if (output == null)
                output = newDialogue;
            else
                AppendToLeaf(output, newDialogue);

            // If the remainingText is not empty, we repeat the while-loop until it is, and we have
            // segmented all of the text in separate dialogueobjects.
        }

        return output;
    }

    private string RemovePunctuationFromStart(string input)
    {
        string output = input;
        while (output.Length > 0 && (output[0] == ' ' ||  punctuations.Contains(output[0])))
        {
            // Drop first element of remainingtext if its punctuation
            output = output.Remove(0, 1);
        }

        return output;
    }

    /// <summary>
    /// Finds the index of the last punctuation-symbol of the passed string
    /// Punctuations include the following  ",.?!"
    /// </summary>
    private int FindLastPunctuation(string input)
    {
        // If there is no punctuation in the input..
        if (!input.Intersect(punctuations).Any())
        {
            // See if it contains a space..
            // If so, return the index of the last space.
            // Otherwise, return the full length
            if (input.Contains(' '))
                return input.LastIndexOf(' ');
            return input.Length;
        }
        // If there is punctuation in the input, return the index of the last one.
        int output = input.LastIndexOfAny(punctuations);
        return output;
    }

    /// <summary>
    /// Sets the background of the dialogue. The range for this change can be set with indices.
    /// If no range is given, we apply it to all DialogueData-segments
    /// </summary>
    /// <param name="background">The background to set to the dialogue.</param>
    /// <param name="startRange">The start of the range of the operation.</param>
    /// <param name="endRange">The end of the range of the operation.</param>
    private void SetBackground(GameObject[] background, int? startRange = null, int? endRange = null)
    {
        int start = startRange ?? 0;
        int end = endRange.HasValue ? Math.Min(segments.Length, endRange.Value + 1) : segments.Length;
        for (int i = start; i < end; i++)
        {
            segments[i].background = background;
        }
    }


    #region Static Methods

    /// <summary>
    /// Static variant of AppendToLeaf, which adds a response to the end of a tree.
    /// If the leaf is a terminatedialogueobject, we replace it, and set it as leaf of the new Response.
    /// </summary>
    public static void AddLeaf(DialogueObject treeHead, DialogueObject newLeaf)
    {
        // If treehead has no responses, then add newleaf to it and return.
        if (treeHead.Responses.Count == 0)
        {
            treeHead.Responses.Add(newLeaf);
            return;
        }
        // else..
        
        // Currentnode starts at TreeHead. LastNode starts as null.
        DialogueObject lastNode = null;
        DialogueObject currentNode = treeHead;
        // If the currentNode has atleast 1 response, set the first response to currentNode and loop
        // until we find a currentNode that has no responses.
        while (currentNode.Responses.Count > 0)
        {
            lastNode = currentNode;
            currentNode = currentNode.Responses.First();
        }
        
        // Now, if currentNode is a TerminateDialogueObject, we want to replace it with newLeaf, 
        // and set TerminateDialogueObject to be the newleaf's first response.
        if (lastNode.Responses.First() is TerminateDialogueObject)
        {
            lastNode.Responses.Clear();
            newLeaf.Responses.Add(new TerminateDialogueObject());
            lastNode.Responses.Add(newLeaf);
        }
        // If its not a TerminateDialogueObject, we add newLeaf as a response of currentNode
        else
        {
            currentNode.Responses.Add(newLeaf);
        }
    }
    
    /// <summary>
    /// Finds the leaf of the DialogueObject passed as an argument, and removes it.
    /// </summary>
    /// <param name="treeHead"></param>
    public static void RemoveLeaf(DialogueObject treeHead)
    {
        // Currentnode starts at TreeHead. LastNode starts as null.
        DialogueObject lastNode = null;
        DialogueObject currentNode = treeHead;
        // If the currentNode has atleast 1 response, set the first response to currentNode and loop
        // until we find a currentNode that has no responses.
        // Dont forget to set LastNode to currentNode first, so we know which node has the leaf as response.
        while (currentNode.Responses.Count > 0)
        {
            lastNode = currentNode;
            currentNode = currentNode.Responses.First();
        }
        // Now, remove the responses (leave(s)) from the lastNode. 
        lastNode.Responses.Clear();
    }

    public static int TreeLength(DialogueObject treeHead)
    {
        int counter = 1;    // counter starts at 1, because the last node is empty but does still count.
        DialogueObject currentNode = treeHead;
        while (currentNode.Responses.Count > 0)
        {
            counter++;
            currentNode = currentNode.Responses.First();
        }
        
        
        return counter;
    }

    public static void PrintDialogue(DialogueObject treeHead)
    {
        DialogueObject currentNode = treeHead;
        while (currentNode.Responses.Count > 0)
        {
            PrintDialogueHelper(currentNode);
            currentNode = currentNode.Responses.First();
        }
        // print the last one too
        PrintDialogueHelper(currentNode);
        
        
    }

    private static void PrintDialogueHelper(DialogueObject node)
    {
        if (node is ContentDialogueObject cdo)
        {
            string output = "";
            foreach (string s in cdo.dialogue)
                output += s + " ";
            Debug.Log(output);
        }
        else if (node is OpenResponseDialogueObject ordo)
        {
            string output = "";
            foreach (string s in ordo.dialogue)
                output += s + " ";
            Debug.Log(output);
        }
        else
        {
            Debug.Log($"Dialogueobject is [{node.GetType()}]");
        }
    }

    #endregion
    
}

[Serializable]
public enum DialogueType
{
    ContentDialogue,
    OpenResponseDialogue
}

[Serializable]
public enum Emotion
{
    Neutral,
    Happy,
    Unhappy
}

/// <summary>
/// A DialogueData object can take both a text and and image.
/// </summary>
[Serializable]
public class DialogueData
{
    [SerializeField] public DialogueType type;
    [SerializeField] public Emotion      emotion;
    [SerializeField] public string       text;
    [SerializeField] public Sprite       image;
    [SerializeField] public GameObject[] background;
    
    // TODO: If it is a branching dialogue, add list of child-dialoguedata here?

    // Constructor to assign the data to the object
    public DialogueData(DialogueType type, Emotion emotion, string text, Sprite image, GameObject[] background)
    {
        this.type = type;
        this.emotion = emotion;
        this.text = text;
        this.image = image;
        this.background = background;
    }

}