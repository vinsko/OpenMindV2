// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class StorySelectionManagerPlayTest
{
    private GameManager           gm;
    private StorySelectionManager sm;
    
    #region Setup and Teardown
    
    /// <summary>
    /// PreCondition: removes all other scenes. Does not allow for DontDestroyOnLoad objects to be active.
    /// </summary>
    [UnitySetUp]
    public IEnumerator Setup()
    {
        // Load StartScreenScene
        SceneManager.LoadScene("StartScreenScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StartScreenScene").isLoaded);
        
        // Load the "Loading" scene in order to get access to the toolbox in DDOL
        SceneManager.LoadScene("Loading");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("Loading").isLoaded);

        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        //gm.StartGame(null, Resources.LoadAll<StoryObject>("Stories")[0]);

        SceneManager.LoadScene("StorySelectScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StorySelectScene").isLoaded);

        sm = GameObject.Find("StorySelectionManager").GetComponent<StorySelectionManager>();
    }

    /// <summary>
    /// PostCondition: no DDOL objects are active
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Move toolbox and DDOLs to Loading to unload
        GameObject.Destroy(GameObject.Find("Toolbox"));
        GameObject.Destroy(GameObject.Find("DDOLs"));
    }
    
    #endregion

    /// <summary>
    /// Checks if the StorySelectionManager is correctly set up. 
    /// </summary>
    [UnityTest, Order(1)]
    public IEnumerator StartStorySelectionManagerTest()
    {
        // Check if there are stories that can be selected
        Assert.IsNotEmpty(sm.stories); 
        yield return null;
    }
    
    /// <summary>
    /// Checks if the story becomes story A when A is selected
    /// </summary>
    [UnityTest, Order(2)]
    public IEnumerator ChooseStoryATest()
    {
        sm.StoryASelected(); // This method also loads the introduction scene.
        yield return new WaitUntil(() => SceneManager.GetSceneByName("IntroStoryScene").isLoaded);
        // In IntroductionManager the introduction is determined by the StorySelect scene.
        IntroductionManager im = GameObject.Find("IntroductionManager").GetComponent<IntroductionManager>();
        // We therefore check if the loaded introduction is indeed the correct one. 
        Assert.AreEqual(im.introStoryA,im.currentTimeline);
    }
    
    /// <summary>
    /// Checks if the story becomes story B when B is selected
    /// </summary>
    [UnityTest, Order(3)]
    public IEnumerator ChooseStoryBTest()
    {
        // This test works exactly the same as ChooseStoryATest
        sm.StoryBSelected();
        yield return new WaitUntil(() => SceneManager.GetSceneByName("IntroStoryScene").isLoaded);
         
        IntroductionManager tm = GameObject.Find("IntroductionManager").GetComponent<IntroductionManager>();
        
        Assert.AreEqual(tm.introStoryB,tm.currentTimeline);
        yield return null;
    }
    
    /// <summary>
    /// Checks if the story becomes story C when C is selected
    /// </summary>
    [UnityTest, Order(4)]
    public IEnumerator ChooseStoryCTest()
    {
        // This test works exactly the same as ChooseStoryATest
        sm.StoryCSelected();
        yield return new WaitUntil(() => SceneManager.GetSceneByName("IntroStoryScene").isLoaded);
        
        IntroductionManager im = GameObject.Find("IntroductionManager").GetComponent<IntroductionManager>();
        
        Assert.AreEqual(im.introStoryC, im.currentTimeline);
        yield return null;
    }
    
}