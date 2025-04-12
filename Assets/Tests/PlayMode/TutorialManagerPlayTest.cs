// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.UI;
using Assert = UnityEngine.Assertions.Assert;

public class TutorialManagerPlayTest
{
    private GameManager     gm;
    private TutorialManager tm;
    private Button          notebookButton;
    private Button          helpButton;
        
    #region Setup and Teardown
    
    [UnitySetUp]
    public IEnumerator Setup()
    {
        // Start new test with clean slate. 
        foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
        {
            Object.DestroyImmediate(obj);
        }
        
        // Load StartScreenScene in order to put the SettingsManager into DDOL
        SceneManager.LoadScene("StartScreenScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StartScreenScene").isLoaded);
        
        // Load the "Loading" scene in order to get access to the toolbox in DDOL
        SceneManager.LoadScene("Loading");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("Loading").isLoaded);
        
        // Initialize GameManager and start the game. 
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.StartGame(null, Resources.LoadAll<StoryObject>("Stories")[0]);

        // Initialize the TutorialManager
        SceneManager.LoadScene("TutorialScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("TutorialScene").isLoaded);
        tm = GameObject.Find("TutorialManager").GetComponent<TutorialManager>();
        
        // Initialize required buttons
        GameObject notebook = GameObject.Find("Notebook Button");
        notebookButton = notebook.GetComponentInChildren<Button>();
        
        GameObject tutorial = GameObject.Find("HelpButton");
        helpButton = tutorial.GetComponentInChildren<Button>();
    }
    
    [TearDown]
    public void TearDown()
    {
        SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), SceneManager.GetSceneByName("TutorialScene"));
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneByName("TutorialScene"));
        
        GameObject.Destroy(GameObject.Find("Toolbox"));
        GameObject.Destroy(GameObject.Find("DDOLs"));
    }
    
    #endregion
    
    // This region contains tests regarding the starting, stopping, pausing and ending of the tutorial. 
    #region StartPlayPauseStopEnd
    /// <summary>
    /// Checks if the tutorial is correctly set up when it is started. 
    /// </summary>
    [UnityTest]
    public IEnumerator StartTutorialTest()
    {
        // Check whether the tutorial contains text. 
        Assert.AreNotEqual(0, tm.tutorialText.Length);
        tm.StartTutorial();
        // When the tutorial is started it should start at the beginning. 
        Assert.AreEqual(0,tm.TutorialTimeline.time);
        // When the tutorial is started it should be playing. 
        Assert.AreEqual(PlayState.Playing,tm.TutorialTimeline.state);
        // Check that notebook button is disabled at the start of the tutorial.
        Assert.IsFalse(notebookButton.enabled);
        // Check that Notebook is closed when the tutorial is started. 
        Assert.IsFalse(SceneManager.GetSceneByName("NotebookScene").isLoaded);
        yield return null;
    }
    
    /// <summary>
    /// Checks if the tutorial can be played. 
    /// </summary>
    [UnityTest]
    public IEnumerator PlayTutorialTest()
    {
        tm.PlayTutorial();
        Assert.AreEqual(PlayState.Playing,tm.TutorialTimeline.state);
        // When the tutorial is playing the continuebutton should not be visible.
        Assert.IsFalse(tm.continueButton.IsActive());
        yield return null;
    }
    
    /// <summary>
    /// Checks if the tutorial can be paused. 
    /// </summary>
    [UnityTest]
    public IEnumerator PauseTutorialTest()
    { 
        tm.PauseTutorial();
        Assert.AreEqual(PlayState.Paused,tm.TutorialTimeline.state);
        // When the tutorial is playing the continuebutton should be visible.
        Assert.IsTrue(tm.continueButton.IsActive());
        yield return null;
    }
    
    /// <summary>
    /// Checks if the tutorial is properly closed after it has been manually stopped.  
    /// </summary>
    [UnityTest]
    public IEnumerator StopTutorialTest()
    {
        tm.StartTutorial();
        Assert.IsFalse(notebookButton.enabled); // Check that notebook is in tutorial mode. 
        Assert.AreEqual(PlayState.Playing,tm.TutorialTimeline.state);
        helpButton.onClick.Invoke();           // Manually stop the tutorial.
        Assert.IsTrue(notebookButton.enabled); // Check that notebook regains normal functionality.
        yield return null;
    }
    
    /// <summary>
    /// Checks if the tutorial is properly closed after it has ended.  
    /// </summary>
    [UnityTest]
    public IEnumerator EndTutorialTest()
    {
        tm.StartTutorial();
        Assert.IsFalse(notebookButton.enabled); // Check that notebook is in tutorial mode. 
        Assert.AreEqual(PlayState.Playing,tm.TutorialTimeline.state);
        tm.StopTutorial();                      // Method that is called at the end of the tutorial.  
        Assert.IsTrue(notebookButton.enabled);  // Check that notebook regains normal functionality.
        yield return null;
    }
    
    
    #endregion
    
    // This region contains tests regarding the text shown during the tutorial. 
    #region TutorialText
    
    /// <summary>
    /// Checks if the tutorial can be played. 
    /// </summary>
    [UnityTest]
    public IEnumerator UpdateTutorialTest()
    {
        string text = tm.text.text;
        tm.PlayTutorial();
        tm.UpdateTutorialText(); // Default text is the same as the first text, so we need to update 
        tm.UpdateTutorialText(); // the text twice in order to properly test this aspect.
        Assert.AreEqual(PlayState.Paused,tm.TutorialTimeline.state);
        // Check whether the text changed. 
        Assert.AreNotEqual(text, tm.text.text);
        yield return null;
    }
    
    /// <summary>
    /// Checks if the objective that is shown during the tutorial belongs to the story. 
    /// </summary>
    [UnityTest]
    public IEnumerator ObjectiveTextTest()
    {
        tm.ShowObjective();
        Assert.AreEqual(tm.objectives[gm.story.storyID], tm.objectiveText.text);
        yield return null;
    }
    #endregion
    
    /// <summary>
    /// Checks if the notebook can be opened during the notebook tutorial. 
    /// </summary>
    [UnityTest]
    public IEnumerator NotebookTutorialTest()
    {
        tm.ActivateNotebookTutorial();
        // Check that notebook button is the only button that can be clicked.
        Assert.IsFalse(tm.continueButton.IsActive()); 
        Assert.IsTrue(notebookButton.enabled);
        yield return null;
    }
    
}
