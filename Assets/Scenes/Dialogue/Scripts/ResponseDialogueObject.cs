// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// A child of DialogueObject. Executing this object writes a response to the given question to the screen.
/// A response can be either a new QuestionDialogueObject, or a TerminateDialogueObject if there are no more questions available.
/// </summary>
public class ResponseDialogueObject : DialogueObject
{
    public Question question;

    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="question">The question that this is a response to</param>
    /// <param name="background">The background</param>
    public ResponseDialogueObject(Question question, GameObject[] background)
    {
        this.question = question;
        this.background = background;
    }

    /// <summary>
    /// Gives a response to the question that was just asked
    /// </summary>
    public override void Execute()
    {
        var dm = DialogueManager.dm;

        // Get the answer to this question out of CharacterInstance
        DialogueObject answer = GetQuestionResponse(question);
        
        // TODO: Rewrite this.
        if (GameManager.gm.HasQuestionsLeft() &&
            DialogueManager.dm.currentRecipient.RemainingQuestions.Count > 0)
        {
            DialogueContainer.RemoveLeaf(answer);
            DialogueContainer.AddLeaf(answer,
                new QuestionDialogueObject(background));
        }

        
        dm.ReplaceBackground(background);
        
        // TODO: We dont want to try and write empty dialogue. This is a work around that breaks image-only dialogue segments.
        Responses.Add(answer);
        dm.WriteDialogue(null, 1);
    }

    /// <summary>
    /// Gets character's response to the given question
    /// </summary>
    /// <param name="question">The question that needs a response.</param>
    /// <returns>The answer to the given question.</returns>
    // 
    private DialogueObject GetQuestionResponse(Question question)
    {
        // Player asked a question, so increment this value in gamemanager
        // TODO: Preferably done with gameevent
        GameManager.gm.numQuestionsAsked++;

        // Remove question from  this character's list of remaining questions
        // TODO: Preferably done with gameevent (same event as above)
        CharacterInstance character = DialogueManager.dm.currentRecipient;
        character.RemainingQuestions.Remove(question);
        
        // Return answer to the question
        return character.Answers[question].GetDialogue();
    }
}
