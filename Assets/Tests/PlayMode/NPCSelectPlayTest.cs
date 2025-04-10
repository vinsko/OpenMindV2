// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class NPCSelectPlayTest
{
    private GameManager gm;
    private SelectionManager sm;
    private NPCSelectScroller scroller;

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

        SceneManager.LoadScene("NPCSelectScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded);

        sm = GameObject.Find("SelectionManager").GetComponent<SelectionManager>();
        scroller = GameObject.Find("Scroller").GetComponent<NPCSelectScroller>();
        scroller.scrollDuration = 0.1f;

    }

    [TearDown]
    public void TearDown()
    {
        // Move toolbox and DDOLs to scene to unload after
        SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), SceneManager.GetSceneByName("NPCSelectScene"));
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneByName("NPCSelectScene"));
        GameObject.Destroy(GameObject.Find("DDOLs"));
        GameObject.Destroy(GameObject.Find("Toolbox"));
        GameObject.Destroy(GameObject.Find("Canvas"));
        GameObject.Destroy(GameObject.Find("SelectionManager"));
        
        
        SceneManager.UnloadSceneAsync("NPCSelectScene");
        //note: for some reason unity really refuses to unload the npcSelectScene (Loading is still active) and refuses to destroy the SelectionManager and Canvas objects.
    }

    /// <summary>
    /// Makes sure the NPC Select Scroller is properly structured.
    /// </summary>
    [UnityTest]
    public IEnumerator StartNPCSelectTest()
    {
        // Check if the scroller children are what they should be
        Assert.AreEqual("Scrollable", scroller.transform.GetChild(0).gameObject.name);
        Assert.AreEqual("NavLeft", scroller.transform.GetChild(1).gameObject.name);
        Assert.AreEqual("NavRight", scroller.transform.GetChild(2).gameObject.name);

        yield return null;
    }

    /// <summary>
    /// Test whether the text scales correctly based on the textSize from the SettingsManager.
    /// </summary>
    /// <returns></returns>
    /*[UnityTest]
    public IEnumerator ChangeTextSizeTest()
    {
        // Set the textSize to small.
        SettingsManager.sm.textSize = SettingsManager.TextSize.Small;
        int fontSizePrior = SettingsManager.sm.GetFontSize();
        
        // Find the objects that contain tmp_text component.
        GameObject confirmSelectionButton = GameObject.Find("Confirm Selection Button");
        TextMeshProUGUI headerText = GameObject.Find("HeaderText").GetComponent<TextMeshProUGUI>();

        // Set the fontSizes to small
        headerText.fontSize = fontSizePrior;
        confirmSelectionButton.GetComponentInChildren<TMP_Text>().fontSize = fontSizePrior;
        
        // Set the textSize to medium
        SettingsManager.sm.textSize = SettingsManager.TextSize.Medium;
        
        // Change the text size of the components.
        sm.ChangeTextSize();
        
        // Search for the components.
        confirmSelectionButton = GameObject.Find("Confirm Selection Button");
        headerText = GameObject.Find("HeaderText").GetComponent<TextMeshProUGUI>();
        
        // Check if the fontSizes are bigger than before.
        Assert.Greater(confirmSelectionButton.GetComponentInChildren<TMP_Text>().fontSize, fontSizePrior);
        Assert.Greater(headerText.fontSize, fontSizePrior);
        
        yield return null;
    }*/
    
    /// <summary>
    /// Test whether the text scales correctly when the TextSize is changed in the SettingsManager.
    /// </summary>
    /// <returns></returns>
    /*[UnityTest]
    public IEnumerator OnChangedTextSizeTest()
    {
        // Set the textSize to small.
        SettingsManager.sm.textSize = SettingsManager.TextSize.Small;
        int fontSizePrior = SettingsManager.sm.GetFontSize();
        
        // Find the objects that contain tmp_text component.
        GameObject confirmSelectionButton = GameObject.Find("Confirm Selection Button");
        TextMeshProUGUI headerText = GameObject.Find("HeaderText").GetComponent<TextMeshProUGUI>();

        // Set the fontSizes to small
        headerText.fontSize = fontSizePrior;
        confirmSelectionButton.GetComponentInChildren<TMP_Text>().fontSize = fontSizePrior;
        
        // Set the textSize to medium
        SettingsManager.sm.textSize = SettingsManager.TextSize.Medium;
        
        // Change the text size of the components.
        sm.OnChangedTextSize(null, SettingsManager.sm.GetFontSize());
        
        // Search for the components.
        confirmSelectionButton = GameObject.Find("Confirm Selection Button");
        headerText = GameObject.Find("HeaderText").GetComponent<TextMeshProUGUI>();
        
        // Check if the fontSizes are bigger than before.
        Assert.Greater(confirmSelectionButton.GetComponentInChildren<TMP_Text>().fontSize, fontSizePrior);
        Assert.Greater(headerText.fontSize, fontSizePrior);
        
        yield return null;
    }*/
    
    /// <summary>
    /// Check if button fading in takes the correct amount of time.
    /// </summary>
    [UnityTest]
    public IEnumerator ButtonFadeInTest()
    {
        var testObject = new GameObject();
        var cg = testObject.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        float duration = 0.2f;

        // Make sure the starting values are correct
        Assert.AreEqual(0, cg.alpha);

        // Start the fade
        sm.FadeIn_Test(cg, duration);

        // Make sure something is happening
        yield return new WaitForSeconds(duration / 2);
        Assert.Greater(cg.alpha, 0);
        Assert.Less(cg.alpha, 1);

        // Check if the value is correct at the end
        yield return new WaitForSeconds(duration / 2);
        Assert.AreEqual(1, cg.alpha);
    }

    /// <summary>
    /// Check if button fading out takes the correct amount of time.
    /// </summary>
    [UnityTest] 
    public IEnumerator ButtonFadeOutTest()
    {
        var testObject = new GameObject();
        var cg = testObject.AddComponent<CanvasGroup>();
        cg.alpha = 1;
        float duration = 0.2f;

        // Make sure the starting values are correct
        Assert.AreEqual(1, cg.alpha);

        // Start the fade
        sm.FadeOut_Test(cg, duration);

        // Make sure something is happening
        yield return new WaitForSeconds(duration / 2);
        Assert.Greater(cg.alpha, 0);
        Assert.Less(cg.alpha, 1);

        // Check if the value is correct at the end
        yield return new WaitForSeconds(duration / 2);
        Assert.AreEqual(0, cg.alpha);
    }

    #region Scroller Tests
    /// <summary>
    /// Check if the selection button contains the correct text based on the selected character.
    /// </summary>
    [UnityTest]
    public IEnumerator SelectedCharacterNameTest()
    {
        for (int i = 0; i < scroller.Test_Children.Length; i++)
        {
            scroller.Test_InstantNavigate(i);
            scroller.Test_SelectedChild = i;

            yield return null;

            var character = scroller.Test_Children[i].GetComponentInChildren<SelectOption>().character;
            TMP_Text text = sm.Test_GetSelectionButtonRef().GetComponentInChildren<TMP_Text>();
            Assert.IsTrue(text.text.Contains(character.characterName),
                $"Expected the string to contain {character.characterName}, but the string was {text.text}");
        }
    }

    /// <summary>
    /// Tests if the selected child is updated properly when navigating.
    /// </summary>
    [UnityTest]
    public IEnumerator NavigateToChildSelectsCorrectChildTest()
    {
        scroller.Test_SelectedChild = 0;
        scroller.NavigateRight();

        yield return new WaitForSeconds(scroller.Test_ScrollDuration);

        Assert.AreEqual(1, scroller.Test_SelectedChild);
        scroller.NavigateLeft();

        yield return new WaitForSeconds(scroller.Test_ScrollDuration);
        Assert.AreEqual(0, scroller.Test_SelectedChild);
    }

    /// <summary>
    /// Tests if the child is moved to the center of the screen.
    /// </summary>
    [UnityTest]
    public IEnumerator NavigateToChildSetsCorrectPositionTest()
    {
        float prevDuration = scroller.Test_ScrollDuration;

        scroller.Test_ScrollDuration = 0.05f;
        // Repeat x times
        int childCount = scroller.Test_Children.Length;
        for (int i = 0; i < childCount * 8; i++)
        {
            int childIndex = UnityEngine.Random.Range(0, childCount - 1);
            scroller.Test_NavigateToChild(childIndex);

            yield return new WaitForSeconds(scroller.scrollDuration);
            
            // Only test x value, as y value is irrelevant
            Assert.AreEqual((double)Screen.width / 2, Math.Round(scroller.Test_Children[childIndex].position.x, 3),
                $"The child is at x position {scroller.Test_Children[childIndex].position.x}, " +
                $"but the center of the screen is at {Screen.width / 2}");
        }

        // Reset scroll duration
        scroller.Test_ScrollDuration = prevDuration;
    }

    /// <summary>
    /// Makes sure there is never an unavailable child selected.
    /// </summary>
    [UnityTest]
    public IEnumerator ScrollerChildBoundsTest()
    {
        int minBound = 0;
        int maxBound = scroller.Test_Children.Length - 1;

        // Repeat x times
        int random;
        for (int i = 0; i < 100; i++)
        {
            random = UnityEngine.Random.Range(-100, 100);

            scroller.Test_SelectedChild = random;
            Assert.GreaterOrEqual(scroller.Test_SelectedChild, minBound,
                $"{random} is less than the minimum bound of {minBound}");
            Assert.LessOrEqual(scroller.Test_SelectedChild, maxBound,
                $"{random} is more than the maximum bound of {maxBound}");
        }

        yield return null;
    }
    #endregion

    /// <summary>
    /// Tests if button text is correctly changed when a character is inactive.
    /// </summary>
    [UnityTest]
    public IEnumerator CharacterInactiveTest()
    {
        gm.currentCharacters[0].isActive = false;

        scroller.Test_SelectedChild = 0;
        scroller.Test_InstantNavigate(0);

        yield return null;

        var text = sm.Test_GetSelectionButtonRef().GetComponentInChildren<TMP_Text>();
        Assert.IsTrue(text.text.Contains(gm.story.victimDialogue),
            $"Expected the string to contain {gm.story.victimDialogue}, but the string was {text.text}");
    }
}
