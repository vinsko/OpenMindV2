// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Class used for swapping scenes.
/// If anywhere in the code, you need to swap scenes. Use a static instance of this class to do it.
/// </summary>
public class SceneController : MonoBehaviour
{
    [Header("Game Events")]
    [SerializeField] GameEvent onStartSceneTransition;
    [SerializeField] GameEvent onEndSceneTransition;
    
    /// <summary>
    /// All scenes in the project.
    /// </summary>
    public enum SceneName
    {
        Loading,
        NPCSelectScene,
        DialogueScene,
        NotebookScene,
        GameMenuScene,
        SettingsScene,
        GameOverScene,
        TutorialScene,
        EpilogueScene,
        StartScreenScene,
        StorySelectScene
    }

    /// <summary>
    /// The type of transition between scenes.
    /// </summary>
    public enum TransitionType
    {
        Transition,
        Additive,
        Unload
    }
    
    // A static instance of this class
    public static SceneController sc;

    //read from a file
    private List<List<(int, TransitionType)>> sceneGraph;

    //inferred from reading the same file, what scene is matched to what id doesn't matter, as long as they are all assigned to a unique ID
    private Dictionary<string, int> sceneToID;

    private const string TransitionGraphLocation = "Transition Graph/Transition Graph";
    private string GetTransitionGraphFilePath() => Path.Combine(Application.dataPath, "../Assets/Resources/") + TransitionGraphLocation;

    /// <summary>
    /// When loaded, initialize the static instance of this class.
    /// </summary>
    public void Awake()
    {
        sc = this;
    }

    /// <summary>
    /// Unloads all scenes(as all are opened additively), other than the 'Loading' scene.
    /// </summary>
    public void UnloadAdditiveScenes()
    {
        //Get the story scene
        Scene loadingScene = SceneManager.GetSceneByName("Loading");
        
        // Unload all loaded scenes that are not the story scene
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene != loadingScene) SceneManager.UnloadSceneAsync(loadedScene.name);
        }
    }

    /// <summary>
    /// read the scene graph from the file and assign <see cref="sceneGraph"/> and <see cref="sceneToID"/>
    /// </summary>
    //
    private void ReadSceneGraph()
    {
        // Load the scene graph file
        TextAsset file = (TextAsset)Resources.Load(TransitionGraphLocation);

        // Check if the file exists
        if (file == null)
        {
            Debug.LogError("Couldn't read the scene graph, the file on filepath " +
                $"Assets/Resources/{TransitionGraphLocation} was not found.");
            return;
        }

        // Split into lines
        string[] fileGraphContentLines = Regex.Split(file.text, "\r\n|\r|\n");

        sceneGraph = new List<List<(int, TransitionType)>>(fileGraphContentLines.Length);
        sceneToID = new Dictionary<string, int>();
        
        //check if all scenes are correctly written into the file
        List<string> buildScenes = new List<string>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = scenePath.Split("/").Last().Split(".").First();
            buildScenes.Add(sceneName);
        }
        
        //check if the file is valid, if not, sceneGraph and sceneToID will be emptied
        bool validReading = true;
        
        //example: NPCSelectScene --> DialogueScene(T), NotebookScene(A), GameOverScene(T), GameWinScene(T)
        const string arrowSeparator = " --> ";
        const string sceneSeparator = ", ";
        
        // Check the scene before the arrowSeparator is correctly written.
        for(int i = 0; i < fileGraphContentLines.Length; i++)
        {
            string sceneName = fileGraphContentLines[i].Split(arrowSeparator)[0];
            //check if the scene name is correctly written
            if (!buildScenes.Contains(sceneName))
            {
                Debug.LogError($"The scene with the name {sceneName} on line {i} of the scene graph file does not exist. Please check this file for typos. Scene transitions won't be checked unless this is fixed.");
                validReading = false;
                fileGraphContentLines = Array.Empty<string>();
                break;
            }
            sceneToID.Add(sceneName, sceneToID.Count);
        }
        
        // Check if all scene names are correctly written.
        for (int i = 0; i < fileGraphContentLines.Length; i++)
        {
            string[] fromTo = fileGraphContentLines[i].Split(arrowSeparator);
            string[] tos = fromTo[1].Split(sceneSeparator);
            
            sceneGraph.Add(new List<(int, TransitionType)>());

            foreach (string to in tos)
            {
                string toScene = to.Substring(0, to.Length - 3);
                //check if the scene name is correctly written
                if (!buildScenes.Contains(toScene))
                {
                    Debug.LogError($"The scene with the name {toScene} on line {i} of the scene graph file after the arrow does not exist. Please check this file for typos. Scene transitions won't be checked unless this is fixed.");
                    validReading = false;
                    break;
                }
                
                // Set the correct transitiontype
                bool found = false;
                char trans = to[^2];
                foreach (TransitionType enumValue in Enum.GetValues(typeof(TransitionType)))
                {
                    if (enumValue.ToString()[0] == trans)
                    { 
                        sceneGraph[i].Add((sceneToID[toScene], enumValue));
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    Debug.LogError($"The transition with the letter {trans} on line {i} of the scene graph file belonging to the scene transition {fromTo[0]} --> {toScene} does not exist. Please check this file for typos. Scene transitions won't be checked unless this is fixed.");
                    validReading = false;
                }
            }
        }
        
        // Empty sceneGraph and sceneToID if the transition is invalid.
        if (!validReading)
        {
            sceneGraph = new List<List<(int, TransitionType)>>();
            sceneToID = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// transitions to a new scene.
    /// </summary>
    /// <param name="currentScene">The scene the game is currently in.</param>
    /// <param name="targetScene">The scene that needs to be loaded.</param>
    /// <param name="transitionType">The type of transition to use.</param>
    private async Task Transitioning(string currentScene, string targetScene, TransitionType transitionType, bool playAnimation)
    {
        switch (transitionType)
        {
            case TransitionType.Additive:
                if (playAnimation) 
                    await TransitionAnimator.i.PlayStartAnimation(TransitionAnimator.AnimationType.Fade, 3); // Fade out and wait for animation to complete
                await LoadScene(targetScene);
                if (playAnimation) 
                    await TransitionAnimator.i.PlayEndAnimation(TransitionAnimator.AnimationType.Fade, 3); // Fade out and wait for animation to complete
                break;
            
            case TransitionType.Unload:
                if (playAnimation) 
                    await TransitionAnimator.i.PlayStartAnimation(TransitionAnimator.AnimationType.Fade, 3); // Fade out and wait for animation to complete
                SceneManager.UnloadSceneAsync(currentScene);
                if (playAnimation) 
                    await TransitionAnimator.i.PlayEndAnimation(TransitionAnimator.AnimationType.Fade, 3); // Fade out and wait for animation to complete
                break;
            
            case TransitionType.Transition:
                await TransitionAnimator.i.PlayStartAnimation(TransitionAnimator.AnimationType.Fade, 3); // Fade out and wait for animation to complete
                SceneManager.UnloadSceneAsync(currentScene); // Unload old scene
                await LoadScene(targetScene); // Load new scene
                _ = TransitionAnimator.i.PlayEndAnimation(TransitionAnimator.AnimationType.Fade, 3); // Fade back into game
                break;
        }
    }

    #region Async Scene Loading Helper Functions
    /// <summary>
    /// Converts SceneManager.LoadSceneAsync() from an AsyncOperation to a Task so that it is awaitable.
    /// </summary>
    /// <param name="targetScene">The name of the scene to be loaded.</param>
    /// <returns></returns>
    private Task LoadScene(string targetScene)
    {
        // Create a TaskCompletionSource to return as a Task
        // TaskCompletionSource is used to define when an "await" is finished
        var tcs = new TaskCompletionSource<bool>();

        // Start the coroutine
        StartCoroutine(LoadSceneCoroutine(targetScene, tcs));
        
        // Return the task that will complete when the coroutine ends
        return tcs.Task;
    }

    /// <summary>
    /// Uses a coroutine to load a scene and wait for it to finish loading.
    /// </summary>
    /// <param name="targetScene">The name of the scene to be loaded.</param>
    /// <param name="tcs">A reference to the TaskCompletionSource.</param>
    private IEnumerator LoadSceneCoroutine(string targetScene, TaskCompletionSource<bool> tcs)
    {
        // The async operation which loads the scene, we can wait for this to complete
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
            yield return null;

        // Mark the TaskCompletionSource as completed
        tcs.SetResult(true);
    }
    #endregion

    /// <summary>
    /// Additional method to let a scene determine the transition itself.
    /// This method can be directly called to override the transition code.
    /// </summary>
    /// <param name="from">The current scene.</param>
    /// <param name="to">The scene that needs to be loaded.</param>
    /// <param name="transitionType">The type of transition.</param>
    /// <param name="loadCode"></param>
    public async Task TransitionScene(SceneName from, SceneName to, TransitionType transitionType, bool playAnimation, Func<string, string, TransitionType, bool, Task> loadCode)
    {
        string currentScene = from.ToString();
        string targetScene = to.ToString();
        
        //some extra checks will be made about the validity of the variables and the file contents
        //for example, making a new scene, but forgetting to put it into the scene graph file, should result in an error here.
        if (!sceneToID.ContainsKey(currentScene))
        {
            Debug.LogError($"The scene with the name {currentScene} cannot be found in the transition graph. Please add it to the transition graph.");
            return;
        }
        
        if (!sceneToID.ContainsKey(targetScene))
        {
            Debug.LogError($"The scene with the name {targetScene} cannot be found in the transition graph. Please add it to the transition graph.");
            return;
        }
        
        //cannot load from an unloaded scene
        if (!SceneManager.GetSceneByName(currentScene).isLoaded)
        {
            Debug.LogError($"Cannot make a transition from the scene {currentScene}, since it's not loaded.");
            return;
        }
        
        //checks, does currentScene point to nextScene in the graph?
        int currentSceneID = sceneToID[currentScene];
        int targetSceneID = sceneToID[targetScene];
        if (!sceneGraph[currentSceneID].Contains((targetSceneID, transitionType)))
        {
            //invalid transition
            Debug.LogError($"Current scene {currentScene} cannot make a {transitionType}-transition to {targetScene}");
            return;
        }

        await loadCode(currentScene, targetScene, transitionType, playAnimation);
    }
    
    //args is the data to transfer
    public async Task TransitionScene(SceneName from, SceneName to, TransitionType transitionType, bool playAnimation) 
        => await TransitionScene(from, to, transitionType, playAnimation, Transitioning);
        
    /// <summary>
    /// Function to be called when loading the first cycle
    /// </summary>
    /// <param name="start"></param>
    public async Task StartScene(SceneName start)
    {
        TransitionAnimator.i.PlayEndAnimation(TransitionAnimator.AnimationType.Fade, 0.75f);
        ReadSceneGraph();

        string currentScene = start.ToString();
        await LoadScene(currentScene);
    }

    public SceneName? FindLoadedSceneOfSelection(params SceneName[] scenes)
    {
        foreach (SceneName sceneName in scenes)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName.ToString());
            if (scene.isLoaded)
            {
                return sceneName;
            }
        }

        return null;
    }
    
    /// <summary>
    /// Function to load the notebook.
    /// </summary>
    // this method is not tested
    public async void ToggleNotebookScene(Button button, GameObject menuButton)
    {
        var crossOverlay = button.transform.GetChild(0).gameObject;
        
        // If notebook is already open, close it
        if (SceneManager.GetSceneByName("NotebookScene").isLoaded)
        {
            SettingsManager.sm.UnpauseGame();

            // The button should not be clickable again while the notebook is closing
            button.interactable = false;

            // Find the NotebookManager & do the animation
            var nm = FindObjectOfType<NotebookManager>();
            await nm.ShoveOutNotebook();

            // Make the button interactable again
            button.interactable = true;

            menuButton.SetActive(true);
            crossOverlay.SetActive(false);
            _ = TransitionScene(SceneName.NotebookScene, SceneName.Loading,
                TransitionType.Unload, false);
        }
        else // Notebook is NOT loaded.. so open it
        {
            menuButton.SetActive(false);
            crossOverlay.SetActive(true);
            SettingsManager.sm.PauseGame();
            _ = TransitionScene(SceneName.Loading, SceneName.NotebookScene,
                TransitionType.Additive, false);
        }
    }
    
    /// <summary>
    /// Function to load the tutorial.
    /// </summary>
    // this method is not tested
    public void ToggleTutorialScene(Button button)
    { 
       // If tutorial is already open, close it
       if (SceneManager.GetSceneByName("TutorialScene").isLoaded)
       {
           Scene activeScene;
           // Check which scene is currently loaded.
           if (SceneManager.GetSceneByName("DialogueScene").isLoaded)
           {
               activeScene = SceneManager.GetSceneByName("DialogueScene");
           }
           else if (SceneManager.GetSceneByName("NPCSelectScene").isLoaded)
           {
               activeScene = SceneManager.GetSceneByName("NPCSelectScene");
           }
           else if (SceneManager.GetSceneByName("GameLossScene").isLoaded)
           {
               activeScene = SceneManager.GetSceneByName("GameLossScene");
           }
           else if (SceneManager.GetSceneByName("GameWinScene").isLoaded)
           {
               activeScene = SceneManager.GetSceneByName("GameWinScene");
           }
           else
           {
               activeScene = SceneManager.GetSceneByName("Loading");
           }
           
           // Get the SceneName enum from the activeScene.
           SceneName baseScene = SceneName.Loading;
           
           SettingsManager.sm.UnpauseGame();
           _ = TransitionScene(SceneName.TutorialScene, baseScene, TransitionType.Unload, false);
       }
       else
       {
           SettingsManager.sm.PauseGame();
           _ = TransitionScene(SceneName.Loading, SceneName.TutorialScene, TransitionType.Additive, false);
       }
       
    }

    /// <summary>
    /// Converts the given scene to the corresponding value in the SceneName enum.
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    public SceneName GetSceneName(Scene scene)
    {
        try
        {
            return (SceneName)Enum.Parse(typeof(SceneName), scene.name, true);
        }
        catch (ArgumentException)
        {
            // If scene name is not found, throw an error
            Debug.LogError($"'{scene.name}' is not a valid enum name for {typeof(SceneName).Name}.");
            throw;
        }
    }
}