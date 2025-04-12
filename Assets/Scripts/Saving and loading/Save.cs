// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class Save
{
    private static          Save   _saver;
    private static readonly object _lock = new object();
    public static Save Saver
    {
        get
        {
            //double-check locking pattern for safety
            if (_saver == null)
            {
                lock (_lock)
                {
                    if (_saver == null)
                    {
                        _saver = new Save();
                    }
                }
            }
            return _saver;
        }
    }    
    
    /// <summary>
    /// Saves data to a file.
    /// All data saved to a file is stored in <see cref="SaveData"/>>.
    /// When accessing the notebook file, all relevant exceptions are caught and result in an error in the debug console and results in nothing being saved.
    /// When the specified directory and or file to save to cannot be found, these will be created anew.
    ///
    /// This method results in an error in the debug console, if the gamemanager is not loaded, after which it will exit the function.
    /// </summary>
    public void SaveGame(SaveData saveData = null)
    {
        if (saveData is null)
            saveData = CreateSaveData();
        
        if (saveData is null)
            return;

        string jsonString = JsonConvert.SerializeObject(saveData);
        string folderLocation = FilePathConstants.GetSaveFolderLocation();
        string fileLocation = FilePathConstants.GetSaveFileLocation();
        
        if (!Directory.Exists(folderLocation))
            Directory.CreateDirectory(folderLocation);
        
        File.WriteAllText(fileLocation,jsonString);
    }
    
    public SaveData CreateSaveData()
    {
        GameManager gameManager = GameManager.gm;
        //check if the gamemanger is loaded
        if (gameManager is null)
        {
            Debug.LogError("Cannot save data when the gamemanger is not loaded.\nSaving failed");
            return null;
        }

        //check if current Characters have been assigned
        if (gameManager.currentCharacters is null)
        {
            Debug.LogError(
                "Cannot save data when gameManager.currentCharacters has not been assigned yet.\nSaving failed");
            return null;
        }

        //check if all characters have a unique id. note: this check is obsolete at this point
        bool allUniqueID = true;
        for (int i = 0; i < gameManager.currentCharacters.Count; i++)
            for (int j = i+1; j < gameManager.currentCharacters.Count; j++)
                if (gameManager.currentCharacters[i].id == gameManager.currentCharacters[j].id)
                    allUniqueID = false;
        if (!allUniqueID)
        {
            Debug.LogError("Not all character ids were unique, this is going to cause issues when loading characters.\nSaving failed.");
            return null;
        }
        
        // Gets all data that needs to be saved.
        CharacterInstance[] active = gameManager.currentCharacters.FindAll(c => c.isActive).ToArray();
        CharacterInstance[] inactive = gameManager.currentCharacters.FindAll(c => !c.isActive).ToArray();
        
        (int, List<Question>)[] remainingQuestions = gameManager.currentCharacters
            .Select(a => (a.id, a.RemainingQuestions)).ToArray();
        (int, string)[] characterNotes = GameManager.gm.currentCharacters
            .Select(c => (c.id, gameManager.notebookData.GetCharacterNotes(c))).ToArray();
        (int, bool)[] charactersGreeted = GameManager.gm.currentCharacters
            .Select(c => (c.id, c.talkedTo)).ToArray();

        return new SaveData
        {
            storyId = gameManager.story.storyID,
            activeCharacterIds = active.Select(c => c.id).ToArray(),
            inactiveCharacterIds = inactive.Select(c => c.id).ToArray(),
            culpritId = gameManager.GetCulprit().id,
            remainingQuestions = remainingQuestions,
            personalNotes = gameManager.notebookData.GetPersonalNotes(),
            characterNotes = characterNotes,
            numQuestionsAsked = gameManager.numQuestionsAsked,
            charactersGreeted = charactersGreeted,
        };
    }
}