// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
using Random = System.Random;
using UnityEditor;

/// <summary>
/// A class that tests scene controller properties:
/// - Make sure the transition graph can be read, that it throws no errors when reading
/// - An invalid scene transition should give an error
/// - A valid scene transition should actual transition the scene 
/// </summary>
public class SceneControllerTests
{
    private GameManager     gm;
    private SceneController sc;
    private static SceneController.SceneName[] transitionSceneNames = new SceneController.SceneName[]
    {
        SceneController.SceneName.Loading,
        SceneController.SceneName.NPCSelectScene,
        SceneController.SceneName.DialogueScene,
        SceneController.SceneName.NotebookScene,
        SceneController.SceneName.GameMenuScene,
        SceneController.SceneName.SettingsScene,
        SceneController.SceneName.GameOverScene,
        SceneController.SceneName.TutorialScene,
        SceneController.SceneName.EpilogueScene
    };

    //all possible scene names
    private static SceneController.SceneName[] sceneNames =
        Enum.GetValues(typeof(SceneController.SceneName)).Cast<SceneController.SceneName>().ToArray();
    
    private static SceneController.TransitionType[] transitionTypes =
        Enum.GetValues(typeof(SceneController.TransitionType)).Cast<SceneController.TransitionType>().ToArray();
    
    
    /// <summary>
    /// Sets up the unit tests:
    /// - Disables event systems
    /// - Disables audio listeners
    /// - Initialises Gamemanager.gm.currentCharacters so loading NPCSelectScene doesn't throw an error
    /// - Initialises Gamemanager.gm.story (through reflection) so loading NPCSelectScene doesn't throw an error
    /// - Initialises Gamemanager.gm.notebookData so loading NotebookScene doesn't throw an error
    /// </summary>
    [UnitySetUp]
    public IEnumerator Setup()
    {
        // Load StartScreenScene in order to put the SettingsManager into DDOL
        SceneManager.LoadScene("StartScreenScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StartScreenScene").isLoaded);

        // Load the "Loading" scene in order to get access to the toolbox in DDOL
        SceneManager.LoadScene("Loading");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("Loading").isLoaded);
        
        gm = GameManager.gm;
        
        GameManager.gm.currentCharacters = new List<CharacterInstance>();
        CharacterData dummyData = (CharacterData)AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/0_Fatima_Data.asset", typeof(CharacterData));
        CharacterInstance dummy = new CharacterInstance(dummyData);
        GameManager.gm.currentCharacters.AddRange(new[] { dummy, dummy, dummy, dummy });

        SetProperty("story", GameManager.gm, ScriptableObject.CreateInstance<StoryObject>());

        GameManager.gm.notebookData = new NotebookData();

        sc = SceneController.sc;
    }
    
    [TearDown]
    public void TearDown()
    {
       
        // Move all toolboxes so that they can be unloaded.
        var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Toolbox");
        foreach (GameObject obj in objects)
            SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetActiveScene());
        
        SceneController.sc.UnloadAdditiveScenes();
    }
    
    /// <summary>
    /// Disables all event systems and audio systems to prevent the debug spam of having multiple of these systems.
    /// </summary>
    private void DisableAllEventAndAudioListeners()
    {
        //disable event systems to prevent debug spam
        EventSystem[] eventSystems = UnityEngine.Object.FindObjectsOfType<EventSystem>();
        foreach(EventSystem eventSystem in eventSystems)
            eventSystem.enabled = false;
        
        //disable event systems to prevent debug spam
        AudioListener[] audioSystems = UnityEngine.Object.FindObjectsOfType<AudioListener>();
        foreach(AudioListener audioSource in audioSystems)
            audioSource.enabled = false;
    }
    
    private void GetValue<TV, TC>(string varName, TC instance, out TV variable) where TV : class
    {
        FieldInfo fieldInfo =
            typeof(TC).GetField(varName, BindingFlags.NonPublic | BindingFlags.Instance);
        variable = fieldInfo.GetValue(instance) as TV;
    }
    
    private void SetProperty<TV, TC>(string varName, TC instance, TV value)
    {
        PropertyInfo propertyInfo = typeof(TC).GetProperty(varName);
        propertyInfo.SetValue(instance, value, null);
    }
    
    /// <summary>
    /// Tests whether the scene graph reading works and no errors are thrown.
    /// Also tests whether the right scene gets loaded from SceneController.StartScene, since both properties are tested with this method.
    /// </summary>
    [UnityTest, Order(1)]
    public IEnumerator TestSceneGraphReading([ValueSource(nameof(transitionSceneNames))] SceneController.SceneName sceneName)
    {
        if (sc == null) throw new ArgumentException("SceneController was null");
        //creates scene graph
        sc.StartScene(sceneName);

        int i = 0;
        const int timeout = 1000;
        bool finished = true;
        
        //wait for the scene to load or a timeout. If the timeout is hit, assume the scene never loaded.
        yield return new WaitUntil(() =>
        {
            i = i++;
            if (i > timeout)
            {
                finished = false;
                return true;
            }
            
            return SceneManager.GetSceneByName(sceneName.ToString()).isLoaded;
        });
        
        Assert.IsTrue(finished);
    }
    

    /// <summary>
    /// Tests whether an invalid scene transition throws an error.
    /// Tests whether transitioning from an unloaded scene throws an error
    ///
    /// note: from loading to NPCSelect with additive transition and from loading to Notebook with additive transition both fail
    /// This is because Gamemanager.gm.currentCharacters is null, but this variable is initialised in the srtup
    /// The check where the tests checks if loading from an unloaded scene makes GameManager.gm.currentCharacters null, despite no scenes being loaded or unloaded
    /// A comment is placed with this specific piece of code
    /// </summary>
    [UnityTest, Order(3)]
    public IEnumerator TestSceneTransitionValidity(
        [ValueSource(nameof(transitionSceneNames))] SceneController.SceneName from,
        [ValueSource(nameof(transitionSceneNames))] SceneController.SceneName to,
        [ValueSource(nameof(transitionTypes))] SceneController.TransitionType tt
        )
    {
        if (sc == null) throw new ArgumentException("SceneController is null");

        GetValue("sceneGraph", sc, out object value1);
        GetValue("sceneToID", sc, out object value2);
        if (value1 is null || value2 is null)
            sc.StartScene(from);
        else
            SceneManager.LoadScene(from.ToString(), LoadSceneMode.Additive);
        
        yield return new WaitUntil(() => SceneManager.GetSceneByName(from.ToString()).isLoaded);
        
        //reassign Gamemanager.gm, since loading the Loading scene overwrites it
        if (from == SceneController.SceneName.Loading)
            GameManager.gm = gm;
        
        List<List<(int, SceneController.TransitionType)>> sceneGraph;
        Dictionary<string, int> sceneToID;
        
        GetValue("sceneGraph", sc, out sceneGraph);
        GetValue("sceneToID", sc, out sceneToID);
        
        //check if an error is thrown when transitioning from an unloaded scene
        if (!SceneManager.GetSceneByName(to.ToString()).isLoaded)
        {
            LogAssert.Expect(LogType.Error,
                $"Cannot make a transition from the scene {to}, since it's not loaded.");
            
            //this transition, which does nothing besides throw the error that is asserted above, causes Gamemanager.gm.CurrentCharacters to become null for some reason.
            Task task1 = sc.TransitionScene(to, from, tt, false);
            yield return new WaitUntil(() => task1.IsCompleted);
            
            Assert.IsFalse(DidTransitionHappen(from, to));
        }

        //checks whether the scene is invalid, if it is valid, check if the right transition happened
        int fromID = sceneToID[from.ToString()];
        int toID = sceneToID[to.ToString()];
        if (sceneGraph[fromID].Contains((toID, tt)))
        {
            if (tt == SceneController.TransitionType.Unload)
            {
                //if the transition is an unload, make sure the to is loaded
                SceneManager.LoadScene(to.ToString(), LoadSceneMode.Additive);
                yield return new WaitUntil(
                    () => SceneManager.GetSceneByName(to.ToString()).isLoaded);
            }
            //do the transition
            Task task = sc.TransitionScene(from, to, tt, false);
            yield return new WaitUntil(() => task.IsCompleted);
            
            //the transition has different results based on the type of transition
            switch (tt)
            {
                case SceneController.TransitionType.Additive:
                    Assert.IsTrue(SceneManager.GetSceneByName(from.ToString()).isLoaded);
                    Assert.IsTrue(SceneManager.GetSceneByName(to.ToString()).isLoaded);
                    break;
                
                case SceneController.TransitionType.Transition:
                    //edge case for when to == from
                    if (to != from)
                        Assert.IsFalse(SceneManager.GetSceneByName(from.ToString()).isLoaded);
                    Assert.IsTrue(SceneManager.GetSceneByName(to.ToString()).isLoaded);
                    break;
                
                case SceneController.TransitionType.Unload:
                    Assert.IsFalse(SceneManager.GetSceneByName(from.ToString()).isLoaded);
                    Assert.IsTrue(SceneManager.GetSceneByName(to.ToString()).isLoaded);
                    break;
                
                default:
                    throw new Exception($"There is no test for this type of transition yet: {tt}");
            }
        }
        else
        {
            //invalid transition
            LogAssert.Expect(LogType.Error,
                $"Current scene {from} cannot make a {tt}-transition to {to}");
            
            Task task2 = sc.TransitionScene(from, to, tt, false);
            yield return new WaitUntil(() => task2.IsCompleted);
            
            Assert.IsFalse(DidTransitionHappen(from, to));
        }
    }
    
    /// <summary>
    /// Tests whether a transition occured
    /// Assumes:
    /// from was active
    /// to was not active
    /// Loading was active
    /// </summary>
    private bool DidTransitionHappen(SceneController.SceneName from, SceneController.SceneName to)
    {
        //if from == to, from should be loaded, which means to is also loaded and no transition happened
        if (from == to)
            return !SceneManager.GetSceneByName(from.ToString()).isLoaded;
        
        bool transitionHappened = false;
        // Loading will always be active during these transition
        if (to != SceneController.SceneName.Loading)
            transitionHappened |= SceneManager.GetSceneByName(to.ToString()).isLoaded;

        //if from is not active, a transition happened
        transitionHappened |= !SceneManager.GetSceneByName(from.ToString()).isLoaded;
        
        return transitionHappened;
    }
}
