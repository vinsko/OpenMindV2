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
using UnityEditor.Compilation;

public class CharacterInstanceEditTest
{
    private List<CharacterInstance> characters;
    private CharacterData           mainCharacterData;
    private CharacterInstance       mainCharacter;
    
    [SetUp]
    public void Setup()
    {
        // Get some random characters to set up the tests
        CharacterData c1 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/0_Fatima_Data.asset", typeof(CharacterData)); // This will be the "main" character during the tests
        mainCharacterData = c1;
        CharacterData c2 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/1_Giulietta_Data.asset", typeof(CharacterData)); // This will be the culprit
        CharacterData c3 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/2_Willow_Data.asset", typeof(CharacterData)); // This will be the chosen culprit
        CharacterData c4 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/3_Olivier_Data.asset", typeof(CharacterData));
        
        characters = new List<CharacterInstance>();
        mainCharacter = new CharacterInstance(c1);
        characters.Add(mainCharacter);
        
        // Other dummy characters
        characters.Add(new CharacterInstance(c2));
        characters.Add(new CharacterInstance(c3));
        characters.Add(new CharacterInstance(c4));
        
        // Load "Loading scene" and find GameManager to set it up
        EditorSceneManager.OpenScene("Assets/Scenes/Loading/Loading.unity");
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.currentCharacters = new List<CharacterInstance>();
        
        foreach (CharacterInstance c in characters)
            gm.currentCharacters.Add(c);

        // Set culprit and chosen culprit
        gm.currentCharacters[1].isCulprit = true;
        gm.FinalChosenCuplrit = gm.currentCharacters[2];
        
        GameManager.gm = gm;
    }

    /// <summary>
    /// Tests if the constructor for CharacterInstance behaves correctly
    /// </summary>
    [Test]
    public void CharacterInstanceInitTest()
    {
        // Set CharacterInstance fields
        Assert.AreEqual(mainCharacter.data, mainCharacterData);
        Assert.AreEqual(mainCharacter.characterName, mainCharacterData.characterName);
        Assert.AreEqual(mainCharacter.id, mainCharacterData.id);
        Assert.AreEqual(mainCharacter.GetAvatar(), mainCharacterData.neutralAvatar);
        Assert.AreEqual(mainCharacter.pitch, mainCharacterData.voicePitch);

        // Check if InitializeQuestions() goes correctly
        foreach (var kvp in mainCharacterData.answers)
        {
            Assert.AreEqual(mainCharacter.Answers[kvp.question], kvp.answer);
            Assert.AreEqual(mainCharacter.Traits[kvp.question], kvp.trait);
            Assert.IsTrue(mainCharacter.RemainingQuestions.Contains(kvp.question));
        }
    }

    /// <summary>
    /// Tests if the GetGreeing method behaves correctly
    /// </summary>
    /// <param name="greetings">We should also test if the data.greetings list is empty / null</param>
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetGreetingTest(bool greetings)
    {
        // Test if character has no greetings
        if (!greetings)
        {
            CharacterData data = mainCharacterData;
            ContentDialogueObject dialogueObject = (ContentDialogueObject)mainCharacter.GetGreeting(null);
            
            // The only thing returned should be "Hello"
            Assert.AreEqual(4, DialogueContainer.TreeLength(dialogueObject));
            Assert.AreEqual(1, dialogueObject.dialogue.Count);
            Assert.AreEqual("Hello. I'm Fatima.", dialogueObject.dialogue[0]);
            
            mainCharacter.data = data; // Put data with segmentslines back for other tests
        }
        else
        {
            var dialogueObject = mainCharacter.GetGreeting(null);
            Assert.IsNotNull(dialogueObject); // Test if some segmentslines get returned
        }

    }
    
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetRandomTraitTest(bool anyQuestions)
    {
        if (anyQuestions)
        {
            var retval = mainCharacter.GetRandomTrait();
            
            Assert.IsNotNull(retval);
            
            GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();

            var question = mainCharacter.Traits.FirstOrDefault(x => x.Value == retval).Key;

            foreach (CharacterInstance c in GameManager.gm.currentCharacters)
            {
                Assert.IsFalse(c.RemainingQuestions.Contains(question));
            }
        }
        else
        {
            mainCharacter.RemainingQuestions = new List<Question>();

            var retval = mainCharacter.GetRandomTrait();
            
            LogAssert.Expect(LogType.Error, "GetRandomTrait(), but there are no more traits remaining");
            Assert.IsNull(retval);
        }
    }
}