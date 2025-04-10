// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using Assert = UnityEngine.Assertions.Assert;

public class IntroductionManagerPlayTest
{
    private GameManager     gm;
    private IntroductionManager im;
    
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
        
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
       
        gm.StartGame(null, Resources.LoadAll<StoryObject>("Stories")[0]);
        
        SceneManager.LoadScene("IntroStoryScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => SceneManager.GetSceneByName("IntroStoryScene").isLoaded);
        
        im = GameObject.Find("IntroductionManager").GetComponent<IntroductionManager>();
    }
    
    [TearDown]
    public void TearDown()
    {
        // Move toolbox and DDOLs to unload after
        SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), 
            SceneManager.GetSceneByName("IntroStoryScene"));
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"),
            SceneManager.GetSceneByName("IntroStoryScene"));
        
        SceneController.sc.UnloadAdditiveScenes();
    }
    
    #endregion
    
    /// <summary>
    /// Checks some basic properties of the introduction. 
    /// </summary>
    [UnityTest]
    public IEnumerator IntroductionSetUpTest()
    {
        // Check if serialized fields are assigned. 
        Assert.IsNotNull(im.introStoryA);
        Assert.IsNotNull(im.introStoryB);
        Assert.IsNotNull(im.introStoryC);
        // Check if UI elements are assigned.
        Assert.IsNotNull(im.background);
        Assert.IsNotNull(im.continueButton);
        yield return null;
    }
    
    // This region contains test regarding the introduction of story A
    #region IntroductionA
    
    /// <summary>
    /// Checks some basic properties of the introduction of story A. 
    /// </summary>
    [UnityTest]
    public IEnumerator ASetUpTest()
    {
        im.StoryA();
        
        // Lists containing necessary elements should not be empty
        Assert.AreNotEqual(0, im.sprites.Length);
        Assert.AreNotEqual(0, im.storyText.Length);
        Assert.AreNotEqual(0, im.messageLocations.Length);
        // Indices should be 0 
        Assert.AreEqual(0, im.TextMessageIndex);
        Assert.AreEqual(0, im.BackgroundIndex);
        Assert.AreEqual(0, im.TextIndex);
        yield return null;
    }
    
    /// <summary>
    /// Checks if the introduction can be played. 
    /// </summary>
    [UnityTest]
    public IEnumerator APlayIntroTest()
    {
        im.StoryA();
        im.ContinueCurrentTimeline();
        Assert.AreEqual(PlayState.Playing,im.currentTimeline.state);
        // When the introduction is playing the continuebutton should not be visible.
        Assert.IsFalse(im.continueButton.activeSelf);
        yield return null;
    }
    
    /// <summary>
    /// Checks if the introduction of story A can be paused. 
    /// </summary>
    [UnityTest]
    public IEnumerator APauseIntroTest()
    {
        im.StoryA();
        im.PauseCurrentTimeline();
        Assert.AreEqual(PlayState.Paused,im.currentTimeline.state);
        // When the introduction is playing the continuebutton should be visible.
        Assert.IsTrue(im.continueButton.activeSelf);
        yield return null;
    }
    
    /// <summary>
    /// Test that a message is showed on the screen when the SendText() method is called. 
    /// </summary>
    [UnityTest]
    public IEnumerator SendMessageTest()
    {
        im.StoryA();
        int index = im.TextMessageIndex;
        im.SendText();
        Assert.AreEqual(index+1, im.TextMessageIndex);
        Assert.AreEqual(PlayState.Paused,im.currentTimeline.state);
        yield return null;
    }
    
    /// <summary>
    /// Test the TypeAnimation function. This function uses the DialogueAnimator which is already tested elsewhere. 
    /// </summary>
    [UnityTest]
    public IEnumerator TypeAnimationTest()
    {
        im.StoryA();
        im.TypeAnimation();
        // Check some basic properties
        Assert.IsTrue(im.continueButton.activeSelf);
        Assert.IsTrue(im.typingText.IsActive());
        Assert.AreEqual(PlayState.Paused,im.currentTimeline.state);
        // Check if the message that is being typed belongs to the player. 
        Assert.AreEqual( TextMessage.Sender.Player,im.textMessages[im.TextMessageIndex + im.messageLocations.Length - 1].sender);
        yield return null;
    }
    
    #endregion
    
    // This region contains test regarding the introduction of story B
    #region IntroductionB
    
    /// <summary>
    /// Checks some basic properties of the introduction of story A. 
    /// </summary>
    [UnityTest]
    public IEnumerator BSetUpTest()
    {
        im.StoryB();
        
        // Lists containing necessary elements should not be empty
        Assert.AreNotEqual(0, im.sprites.Length);
        Assert.AreNotEqual(0, im.storyText.Length);
        Assert.AreEqual(4, im.TextIndex);
        
        yield return null;
    }
    
    /// <summary>
    /// Checks if the introduction can be played. 
    /// </summary>
    [UnityTest]
    public IEnumerator BPlayIntroTest()
    {
        im.StoryB();
        im.ContinueCurrentTimeline();
        Assert.AreEqual(PlayState.Playing,im.currentTimeline.state);
        // When the introduction is playing the continuebutton should not be visible.
        Assert.IsFalse(im.continueButton.activeSelf);
        yield return null;
    }
    
    /// <summary>
    /// Checks if the introduction of story B can be paused. 
    /// </summary>
    [UnityTest]
    public IEnumerator BPauseIntroTest()
    {
        im.StoryB();
        im.PauseCurrentTimeline();
        Assert.AreEqual(PlayState.Paused,im.currentTimeline.state);
        // When the introduction is playing the continuebutton should be visible.
        Assert.IsTrue(im.continueButton.activeSelf);
        yield return null;
    }
    
    /// <summary>
    /// Checks if the character sprite changes when a vision is appearing. 
    /// </summary>
    [UnityTest]
    public IEnumerator VisionTest()
    {
        im.StoryB();
        Sprite beforeImage = im.character.sprite; 
        im.Vision();
        im.Vision();
        Assert.AreNotEqual(beforeImage, im.character.sprite);
        yield return null; 
    }
    
    #endregion
    
    // This region contains test regarding the introduction of story B
    #region IntroductionC
    
    /// <summary>
    /// Checks some basic properties of the introduction of story A. 
    /// </summary>
    [UnityTest]
    public IEnumerator CSetUpTest()
    {
        im.StoryC();
        
        // Lists containing necessary elements should not be empty
        Assert.AreNotEqual(0, im.sprites.Length);
        Assert.AreNotEqual(0, im.storyText.Length);
        Assert.AreEqual(19, im.TextIndex);
        
        yield return null;
    }
    
    /// <summary>
    /// Checks if the introduction can be played. 
    /// </summary>
    [UnityTest]
    public IEnumerator CPlayIntroTest()
    {
        im.StoryC();
        im.ContinueCurrentTimeline();
        Assert.AreEqual(PlayState.Playing,im.currentTimeline.state);
        // When the introduction is playing the continuebutton should not be visible.
        Assert.IsFalse(im.continueButton.activeSelf);
        yield return null;
    }
    
    /// <summary>
    /// Checks if the introduction of story B can be paused. 
    /// </summary>
    [UnityTest]
    public IEnumerator CPauseIntroTest()
    {
        im.StoryC();
        im.PauseCurrentTimeline();
        Assert.AreEqual(PlayState.Paused,im.currentTimeline.state);
        // When the introduction is playing the continuebutton should be visible.
        Assert.IsTrue(im.continueButton.activeSelf);
        yield return null;
    }
    
    #endregion
    
    
    /// <summary>
    /// Test that the player text is actually changed after the ChangePlayerText method is called 
    /// </summary>
    [UnityTest]
    public IEnumerator ChangePlayerTextTest()
    {
        im.StoryA();
        int index = im.TextIndex;
        im.ChangePlayerText();
        Assert.AreEqual(index+1, im.TextIndex);
        Assert.AreEqual(PlayState.Paused,im.currentTimeline.state);
        yield return null;
    }
    
    /// <summary>
    /// Test that the character text is actually changed after the ChangePlayerText method is called 
    /// </summary>
    [UnityTest]
    public IEnumerator ChangeCharacterTextTest()
    {
        im.StoryB();
        int index = im.TextIndex;
        im.ChangeCharacterText();
        Assert.AreEqual(index+1, im.TextIndex);
        Assert.AreEqual(PlayState.Paused,im.currentTimeline.state);
        Assert.AreEqual("Alex", im.nameTag.text);
        yield return null;
    }
    
    /// <summary>
    /// Test that the computer text is actually changed after the ChangePlayerText method is called 
    /// </summary>
    [UnityTest]
    public IEnumerator ChangeComputerTextTest()
    {
        im.StoryC();
        int index = im.TextIndex;
        im.ChangeComputerText();
        Assert.AreEqual(index+1, im.TextIndex);
        Assert.AreEqual(PlayState.Paused,im.currentTimeline.state);
        Assert.AreEqual("Computer", im.nameTag.text);
        yield return null;
    }
    
}