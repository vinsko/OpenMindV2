// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Random = System.Random;

/// <summary>
/// This class tests all read and write related stuff while the game is running. This class:
/// - Assigns specific values for each variable in the SaveData class & saves these contents
/// - Then checks whether the loaded contents are the same
/// - Then checks whether every variable is assigned correctly
/// - Then saves gain and loads again and checks whether every variable is assigned correctly, this tests whether saving is correct
/// </summary>
public class SavingLoadingTestValueReadAndWrite
{
    Random       random = new Random();
    private Save saving  => Save.Saver;
    private Load loading => Load.Loader;
   

    /// <summary>
    /// Set up the game so that each test starts at the NPCSelectScene with the chosen story.
    /// Note: the repeat attribute only calls SetUp and TearDown after every function, but not UnitySetUp nor UnityTearDown,
    /// this is why this SetUp function is called in every test instead of having a [UnitySetUp] attribute
    /// </summary>
    private IEnumerator SetUp()
    {
        // Load StartScreenScene in order to put the SettingsManager into DDOL
        SceneManager.LoadScene("StartScreenScene");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("StartScreenScene").isLoaded);

        // Load the "Loading" scene in order to get access to the toolbox in DDOL
        SceneManager.LoadScene("Loading");
        yield return new WaitUntil(() => SceneManager.GetSceneByName("Loading").isLoaded);

        // Get a StoryObject.
        StoryObject[] stories = Resources.LoadAll<StoryObject>("Stories");
        StoryObject story = stories[0];

        GameManager.gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Start the game with the chosen story.
        GameManager.gm.StartGame(null, story);

        yield return new WaitUntil(() => SceneManager.GetSceneByName("NPCSelectScene").isLoaded); // Wait for scene to load.
    }

    [TearDown]
    public void Teardown()
    {
        // Move toolbox and DDOLs to Loading to unload
        SceneManager.MoveGameObjectToScene(GameObject.Find("Toolbox"), SceneManager.GetSceneByName("Loading"));
        SceneManager.MoveGameObjectToScene(GameObject.Find("DDOLs"), SceneManager.GetSceneByName("Loading"));

        SceneController.sc.UnloadAdditiveScenes();
        GameObject.Destroy(GameObject.Find("DDOLs"));
        GameObject.Destroy(GameObject.Find("Toolbox"));
    }
    
    private bool RandB() => RandI(2) == 0;
    private int RandI(int max) => (int)(random.NextDouble() * max);
    private T RandL<T>(IList<T> list) => list[RandI(list.Count)];
    private char RandC() => (char)RandI(128);
    private string RandS(int length) => length <= 1 ? RandC().ToString(): RandC() + RandS(length - 1); 
    
    /// <summary>
    /// Creates a test variant of the savedata with randomly assigned values
    /// </summary>
    private SaveData CreateSaveData()
    {
        StoryObject[] stories = Resources.LoadAll<StoryObject>("Stories");
        int storyId = RandI(stories.Length);
        
        int[] possibleCharacterIds =  
            (typeof(GameManager)
                .GetField("characters", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(GameManager.gm) as List<CharacterData>).Select(cd => cd.id).ToArray();
        
        RandList<int> possibleCharacterIdsRandom = new RandList<int>(random, possibleCharacterIds);
        int totalCharacters = stories[storyId].numberOfCharacters;
        
        int[] currentCharacters = new int[totalCharacters];
        List<int> activeCharacterIds = new List<int>();
        List<int> inactiveCharacterIds = new List<int>();
        for (int i = 0; i < totalCharacters; i++)
        {
            currentCharacters[i] = possibleCharacterIdsRandom.GetNext();
            if (RandB())
                activeCharacterIds.Add(currentCharacters[i]);
            else
                inactiveCharacterIds.Add(currentCharacters[i]);
        }
            
        int culpritId = RandL(currentCharacters);
        
        Question[] possibleQuestions = Enum.GetValues(typeof(Question)).Cast<Question>().ToArray();
        
        int numQuestionsAsked = 0;
        
        List<(int, List<Question>)> askedQuestions = new List<(int, List<Question>)>();
        List<(int, List<Question>)> remainingQuestions = new List<(int, List<Question>)>();
        List<(int, string)> characterNotes = new List<(int, string)>();
        List<(int,bool)> charactersGreeted = new List<(int,bool)>();
        
        foreach (int characterId in currentCharacters)
        {
            int totalQuestions = stories[storyId].numQuestions;
            Question[] questions = new Question[totalQuestions];
            for (var i = 0; i < questions.Length; i++)
                questions[i] = RandL(possibleQuestions);
            
            List<Question> asked = new List<Question>();
            List<Question> remaining = new List<Question>();
            
            for (int i = 0; i < totalQuestions; i++)
                if (RandB())
                    asked.Add(questions[i]);
                else
                    remaining.Add(questions[i]);
            
            askedQuestions.Add((characterId, asked));
            remainingQuestions.Add((characterId, remaining));
            characterNotes.Add((characterId, RandS(128)));
            charactersGreeted.Add((characterId, (asked.Count > 0)));
            
            numQuestionsAsked += asked.Count;
        }
        
        SaveData saveData = new SaveData
        {
            storyId = storyId,
            activeCharacterIds = activeCharacterIds.ToArray(),
            inactiveCharacterIds = inactiveCharacterIds.ToArray(),
            culpritId = culpritId,
            remainingQuestions = remainingQuestions.ToArray(),
            personalNotes = RandS(128),
            numQuestionsAsked = numQuestionsAsked,
            characterNotes = characterNotes.ToArray(),
            charactersGreeted = charactersGreeted.ToArray()
        };
        
        return saveData;
    }
    
    private void ListEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2, string msg, Action<T, T, string> comparer = null)
    {
        List<T> l1 = list1.ToList();
        List<T> l2 = list2.ToList();
        Assert.AreEqual(l1.Count, l2.Count, msg + ": count");
        
        for (int i = 0; i < l1.Count; i++)
        {
            if (comparer is null)
                Assert.AreEqual(l1[i], l2[i], msg + ": item " + i);
            else
                comparer(l1[i], l2[i], msg);
        }            
    }
    
    private void CompareQuestionList((int, List<Question>) item1, (int, List<Question>) item2, string msg)
    {
        Assert.AreEqual(item1.Item1, item2.Item1, msg + ": characterId");
        ListEquals(item1.Item2, item2.Item2, msg + ": questionList");
    }
    
    private void CompareStringTuple((int, string) item1, (int, string) item2, string msg)
    {
        Assert.AreEqual(item1.Item1, item2.Item1, msg + ": characterID");
        Assert.AreEqual(item1.Item2, item2.Item2, msg + ": characterNotes");
    }
    
    private void CompareBoolTuple((int, bool) item1, (int, bool) item2, string msg)
    {
        Assert.AreEqual(item1.Item1, item2.Item1, msg + ": characterID");
        Assert.AreEqual(item1.Item2, item2.Item2, msg + ": talkedTo");
    }
    
    private void CompareSaveData(SaveData sd1, SaveData sd2, string msg)
    {
        Assert.AreEqual(sd1.storyId, sd2.storyId, msg + ":storyId");
        ListEquals(sd1.activeCharacterIds, sd2.activeCharacterIds, msg + ":activeCharacterIds");
        ListEquals(sd1.inactiveCharacterIds, sd2.inactiveCharacterIds, msg + ":inactiveCharacterIds");
        Assert.AreEqual(sd1.culpritId, sd2.culpritId, msg + ":culpritId");
        ListEquals(sd1.remainingQuestions, sd2.remainingQuestions, msg + ":remainingQuestions",
            CompareQuestionList);
        Assert.AreEqual(sd1.personalNotes, sd2.personalNotes, msg + ":personalNotes");
        ListEquals(sd1.characterNotes, sd2.characterNotes, msg + ":characterNotes", CompareStringTuple);
        Assert.AreEqual(sd1.numQuestionsAsked, sd2.numQuestionsAsked, msg + ":numQuestionsAsked");
        ListEquals(sd1.charactersGreeted, sd2.charactersGreeted, msg + ":charactersGreeted", CompareBoolTuple);
    }
    
    
    /// <summary>
    /// Tests whether saving and loading a SaveData object returns the same object
    /// </summary>
    [UnityTest]
    public IEnumerator SavingLoadingDoesNotChangeContents()
    {
        yield return SetUp();
        SaveData saveData = CreateSaveData();
        saving.SaveGame(saveData);
        SaveData loaded = loading.GetSaveData();
        
        CompareSaveData(saveData, loaded, "compare change");
    }
    
    /// <summary>
    /// Tests whether loading a SaveData object into the gamemanager returns no errors
    /// </summary>
    [UnityTest]
    public IEnumerator LoadingIntoGamemanagerReturnsNoErrors()
    {
        yield return SetUp();
        SaveData saveData = CreateSaveData();
        GameManager.gm.StartGame(null, saveData);
    }
    
    /// <summary>
    /// Tests whether retrieving a SaveData object from the gamemanager returns no errors
    /// </summary>
    [UnityTest]
    public IEnumerator RetrievingFromGamemanagerReturnsNoErrors()
    {
        yield return SetUp();
        saving.CreateSaveData();
    }
    
    /// <summary>
    /// Tests whether saving and loading repeatedly does nothing to change the savedata or the gamestate
    /// </summary>
    [UnityTest]
    [Repeat(10)]
    public IEnumerator SavingLoadingDoesNotChangeGameState()
    {
        yield return SetUp();
        SaveData saveData = CreateSaveData();
        
        for (int i = 0; i < 5; i++)
        {
            saving.SaveGame(saveData);
            SaveData loaded = loading.GetSaveData();
            GameManager.gm.StartGame(new StartMenuManager(), loaded);
            
            yield return new WaitUntil(
                () => SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
            
            SaveData retrieved = saving.CreateSaveData();
            
            CompareSaveData(loaded, retrieved, "compare " + i);
            
            SceneManager.UnloadSceneAsync("NPCSelectScene");
            yield return new WaitUntil(
                () => !SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
        }
    }
    
    /// <summary>
    /// Tests whether the current saved file is not corrupted and is a valid save file, without changing it's contents
    /// If no save file exists, this test succeeds
    /// </summary>
    [UnityTest]
    public IEnumerator DoesCurrentSaveFileWork()
    {
        yield return SetUp();
        //if the file doesn't exist, we can't test it
        if (File.Exists(FilePathConstants.GetSaveFileLocation()))
        {
            SaveData saveData = loading.GetSaveData();
            Assert.IsNotNull(saveData);

            SaveData saveDataCopy = loading.GetSaveData();
            for (int i = 0; i < 5; i++)
            {
                GameManager.gm.StartGame(new StartMenuManager(), saveDataCopy);
                yield return new WaitUntil(
                    () => SceneManager.GetSceneByName("NPCSelectScene").isLoaded);

                SaveData retrieved = saving.CreateSaveData();
                saveDataCopy = retrieved;
                
                SceneManager.UnloadSceneAsync("NPCSelectScene");
                yield return new WaitUntil(
                    () => !SceneManager.GetSceneByName("NPCSelectScene").isLoaded);
            }
            
            CompareSaveData(saveData, saveDataCopy, "compare");
        }
    }
}

/// <summary>
/// A collection of unique random items
/// </summary>
class RandList<T> : IEnumerable<T>
{
    private Random  random;
    private List<T> remaining = new ();
    
    public RandList(Random random, IEnumerable<T> collection)
    {
        this.random = random;
        remaining.AddRange(collection);
    }
    
    public T GetNext()
    {
        int index = random.Next(remaining.Count);
        T item = remaining[index];
        remaining[index] = remaining[^1];
        remaining.RemoveAt(remaining.Count-1);
        return item;
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        while (remaining.Count > 0)
            yield return GetNext();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
