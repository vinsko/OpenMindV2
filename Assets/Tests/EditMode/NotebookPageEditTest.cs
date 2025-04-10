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

public class NotebookPageEditTest
{
    private NotebookPage      page;
    private CharacterInstance character;
    private string            notes;
    
    [OneTimeSetUp]
    public void Setup()
    {
        // Get some random character to make the page for
        CharacterData c = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/0_Fatima_Data.asset", typeof(CharacterData));
        
        // Set global variables
        character = new CharacterInstance(c);
        page = new NotebookPage(character);
        notes = "Notes on " + character.characterName + ".\n";
    }
    
    /// <summary>
    /// Tests if notes are correctly retrieved
    /// </summary>
    [Test]
    public void GetNotesTest()
    {
        string newText = "hello";
        
        page.SetNotes(newText);
        
        Assert.AreEqual(newText, page.GetNotes());
    }

    /// <summary>
    /// Tests if notes are set correctly
    /// </summary>
    [Test]
    public void SetNotesTest()
    {
        page.SetNotes("hello");
        
        Assert.AreEqual("hello", page.GetNotes());
        Assert.AreNotEqual(notes, page.GetNotes());
    }

    /*/// <summary>
    /// Tests if the correct string gets put out after invoking QuestionText()
    /// </summary>
    [Test]
    [TestCase(Question.Name, "Name")]
    [TestCase(Question.Age, "Age")]
    [TestCase(Question.LifeGeneral, "Life")]
    [TestCase(Question.Inspiration, "Inspiration")]
    [TestCase(Question.Sexuality, "Sexuality")]
    [TestCase(Question.Wellbeing, "Wellbeing")]
    [TestCase(Question.Political, "Political ideology")]
    [TestCase(Question.Personality, "Personality")]
    [TestCase(Question.Hobby, "Hobbies")]
    [TestCase(Question.CulturalBackground, "Cultural background")]
    [TestCase(Question.Religion, "Religion")]
    [TestCase(Question.Education, "Education level")]
    [TestCase(Question.CoreValues, "Core values")]
    [TestCase(Question.ImportantPeople, "Most important people")]
    [TestCase(Question.PositiveTrait, "Positive trait")]
    [TestCase(Question.NegativeTrait, "Bad trait")]
    [TestCase(Question.OddTrait, "Odd trait")]
    public void QuestionTextTest(Question question, string questionString)
    {
        // The expected answer
        string answer = "\n" + questionString.ToUpper() + "\n" + questionString + " \n \n";
        
        CharacterInstance c = character;
        c.AskedQuestions = new List<Question>();
        c.Answers = new Dictionary<Question, List<string>>();
        
        c.AskedQuestions.Add(question);
        c.Answers[question] = new List<string> { questionString };
        
        Assert.AreEqual(answer, page.QuestionText());
    }*/
   

    
}
