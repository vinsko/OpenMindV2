// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TMPro.Examples;

public class NotebookManagerPlayTest
{
    private GameManager     gm;
    private NotebookManager nm;
    
    #region Setup and Teardown
    
    [UnitySetUp]
    public IEnumerator Setup()
    {
        // Load StartScreenScene in order to put the SettingsManager into DDOL
        SceneManager.LoadScene("StartScreenScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StartScreenScene").isLoaded);
        
        // Load the "Loading" scene in order to get access to the toolbox in DDOL
        SceneManager.LoadScene("Loading");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("Loading").isLoaded);
        
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        gm.StartGame(null, Resources.LoadAll<StoryObject>("Stories")[0]);

        SceneManager.LoadScene("NotebookScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NotebookScene").isLoaded);
        
        nm = GameObject.Find("NotebookManager").GetComponent<NotebookManager>();
    }
    
    [TearDown]
    public void TearDown()
    {
        // Move toolbox and DDOLs to scene to unload after
        SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), SceneManager.GetSceneByName("NotebookScene"));
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneByName("NotebookScene"));
        SceneController.sc.UnloadAdditiveScenes();
    }
    
    #endregion

    /// <summary>
    /// Checks if the notebook is set up correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator StartNotebookTest()
    {
        // Check if some basic properties hold
        Assert.IsFalse(nm.Test_CharacterInfoField.activeSelf);
        Assert.IsTrue(nm.Test_PersonalInputField.gameObject.activeSelf);
        Assert.AreEqual(nm.notebookData, gm.notebookData);
        Assert.IsFalse(nm.Test_GetPersonalButton().interactable);
        
        yield return null;
    }

    /// <summary>
    /// Checks if the notes are correctly opened
    /// </summary>
    [UnityTest]
    public IEnumerator OpenPersonalNotesTest()
    {
        // Set up fields
        string textBefore = nm.Test_PersonalInputField.text;
        
        string newText = "hello";

        nm.OpenPersonalNotes();
        nm.Test_PersonalInputField.GetComponent<TMP_InputField>().text = newText;

        var textAfter = nm.Test_PersonalInputField.GetComponent<TMP_InputField>().text;
        
        // Check if SaveNotes works correctly
        
        // Check if text has changed
        Assert.AreNotEqual(textBefore, textAfter);
        
        bool active = nm.Test_PersonalInputField.gameObject.activeInHierarchy;
        nm.SavePersonalData();
        
        // Check if the new text is equal to the dummy text
        if (active)
            Assert.AreEqual(nm.notebookData.GetPersonalNotes(), newText);
        else
        {
            var prop = nm.GetType().GetField("currentCharacter", System.Reflection.BindingFlags.NonPublic
                                                                 | System.Reflection.BindingFlags.Instance);
            prop.SetValue(nm, gm.currentCharacters[0]);
            
            Assert.AreEqual(nm.notebookData.GetCharacterNotes(gm.currentCharacters[0]), newText);
        }
        
        // Personal notes should be printed on the screen
        Assert.AreEqual(nm.notebookData.GetPersonalNotes(), nm.Test_PersonalInputField.GetComponent<TMP_InputField>().text);
        
        yield return null;
    }

    /// <summary>
    /// Checks if the notes get saved correctly
    /// </summary>
    [UnityTest]
    public IEnumerator SaveNotesTest()
    {
        string textBefore = nm.Test_PersonalInputField.GetComponent<TMP_InputField>().text;
        
        // Write dummy text to input field
        string newText = "hello";
        nm.Test_PersonalInputField.GetComponent<TMP_InputField>().text = newText;
        
        nm.SavePersonalData();
        
        var textAfter = nm.Test_PersonalInputField.GetComponent<TMP_InputField>().text;
        
        // Check if text has changed
        Assert.AreNotEqual(textBefore, textAfter);

        bool active = nm.Test_PersonalInputField.gameObject.activeInHierarchy;
        
        // Check if the new text is equal to the dummy text
        if (active)
            Assert.AreEqual(nm.notebookData.GetPersonalNotes(), newText);
        else
        {
            var prop = nm.GetType().GetField("currentCharacter", System.Reflection.BindingFlags.NonPublic
                                                  | System.Reflection.BindingFlags.Instance);
            prop.SetValue(nm, gm.currentCharacters[0]);
            
            Assert.AreEqual(nm.notebookData.GetCharacterNotes(gm.currentCharacters[0]), newText);
        }
        
        yield return null;
    }

    [UnityTest]
    public IEnumerator TabButtonsTest()
    {
        var bottomRow = GameObject.Find("Buttons Bottom Row").transform;
        var topRow = GameObject.Find("Buttons Top Row").transform;

        // Check for a CharacterIcon component on the first button
        // It shouldn't have one, as this should be the Personal Notes button
        int firstChildIcons = bottomRow.GetChild(0).GetComponentsInChildren<CharacterIcon>().Length;
        Assert.AreEqual(0, firstChildIcons,
            "The first button should not have any children with the CharacterIcon component, but " +
            firstChildIcons + " was/were found.");

        // Check if the rest of the bottom row is in the correct order
        // (should contain the first characters)
        for (int i = 1; i < bottomRow.childCount; i++)
        {
            var icon = bottomRow.GetChild(i).GetComponentInChildren<CharacterIcon>();
            Assert.AreEqual(gm.currentCharacters[i - 1].characterName, 
                icon.Test_Character.characterName);
        }

        // Check the top row as well (should contain the rest of the characters)
        for (int i = 0; i < topRow.childCount; i++)
        {
            var icon = topRow.GetChild(i).GetComponentInChildren<CharacterIcon>();
            Assert.AreEqual(gm.currentCharacters[i + bottomRow.childCount - 1].characterName, 
                icon.Test_Character.characterName);
        }

        yield return null;
    }
}