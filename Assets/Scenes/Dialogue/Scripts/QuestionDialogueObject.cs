// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A child of DialogueObject. Executing this object places questions on the screen, with ResponseObjects as responses.
/// </summary>
public class QuestionDialogueObject : DialogueObject
{
    public List<Question> questions = new();
    
    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="background">The background</param>
    public QuestionDialogueObject(GameObject[] background)
    {
        this.background = background;
    }

    /// <summary>
    /// Creates the question buttons and adds their responses
    /// </summary>
    public override void Execute()
    {
        var dm = DialogueManager.dm;

        GenerateQuestions();
        
        // Add response to each question to list of responses
        foreach (Question question in questions)
            Responses.Add(new ResponseDialogueObject(question, background));
        
        dm.ReplaceBackground(background);
        dm.InstantiatePromptButtons(this);
    }

    /// <summary>
    /// Helper function for <see cref="Execute"/>. Generates a random list of questions 
    /// </summary>
    private void GenerateQuestions()
    {
        // The number of question options to give the player
        // (This value should possibly be public and adjustable from the GameManager)
        int questionsOnScreen = 2;
        
        // Generate random list of questions
        if (GameManager.gm.HasQuestionsLeft())
        {
            List<Question> possibleQuestions = new(DialogueManager.dm.currentRecipient.RemainingQuestions);
            for (int i = 0; i < questionsOnScreen; i++)
            {
                if (possibleQuestions.Count <= 0)
                    continue;

                int questionIndex = new System.Random().Next(possibleQuestions.Count);
                questions.Add(possibleQuestions[questionIndex]);
                possibleQuestions.RemoveAt(questionIndex);
            }
        }
    }
}
