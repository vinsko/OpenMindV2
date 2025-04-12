using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.TextCore.Text;
using TMPro;

public class DialogueManagerEditTest
{
    private DialogueManager dm;
    
    [OneTimeSetUp]
    public void Setup()
    {
        // Load DialogueScene and find DialogueManager to set it up
        EditorSceneManager.OpenScene("Assets/Scenes/Dialogue/DialogueScene.unity");
        dm = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
    }
    
    /// <summary>
    /// Checks if the "GetPromptText" function gives the correct answer to a given question.
    /// </summary>
    /// <param name="question">The type of question that is being asked.</param>
    /// <param name="expected">The answer that the function should put out.</param>
    [Test]
    [TestCase(Question.Name, "What's your name?")]
    [TestCase(Question.Age, "How old are you?")]
    [TestCase(Question.Wellbeing, "How are you doing?")]
    [TestCase(Question.Political, "What are your political thoughts?")]
    [TestCase(Question.Personality, "Can you describe your personality?")]
    [TestCase(Question.Hobby, "What are some of your hobbies?")]
    [TestCase(Question.CulturalBackground, "What is your cultural background?")]
    [TestCase(Question.Education, "What is your education level?")]
    [TestCase(Question.CoreValues, "What core values are important to you?")]
    [TestCase(Question.ImportantPeople, "Who matters most to you?")]
    [TestCase(Question.PositiveTrait, "What is your best trait?")]
    [TestCase(Question.NegativeTrait, "What is a bad trait you may have?")]
    [TestCase(Question.OddTrait, "Do you have any odd traits?")]
    public void GetPromptTextTest(Question question, string expected)
    {
        // Get the actual answer
        string actual = dm.GetPromptText(question);
        
        // Watch if it the actual output is equal to the expected output
        Assert.AreEqual(expected, actual);
    }
}