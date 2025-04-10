// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Load
{
    private static          Load   _loader;
    private static readonly object _lock = new object();
    public static Load Loader
    {
        get
        {
            //double-check locking pattern for safety
            if (_loader == null)
            {
                lock (_lock)
                {
                    if (_loader == null)
                    {
                        _loader = new Load();
                    }
                }
            }
            return _loader;
        }
    }    
    /// <summary>
    /// Loads the game by retrieving savedata, by reloading the game in Gamemanager and passing the savedata.
    /// </summary>
    public void LoadButtonPressed()
    {
        // retrieve savedata, if there is any.
        SaveData saveData = GetSaveData();
        if (saveData is null)
        {
            Debug.LogError("Make sure there is savedata before loading a game");
            return;
        }

        if (GameManager.gm is null)
        {
            Debug.LogError("Please activate the gamemanager before loading a game");
            return;
        }
        
        GameManager.gm.LoadGame(saveData);
    }

    /// <summary>
    /// Gets a <see cref="SaveData"/> object from the save file contents
    /// </summary>
    public SaveData GetSaveData()
    {
        string saveFileLocation = FilePathConstants.GetSaveFileLocation();
        string saveFileJsonContents = FilePathConstants.GetSafeFileContents(saveFileLocation, "Save Data", "Loading");
        if (saveFileJsonContents is null)
        {
            Debug.Log("No savedata was found");
            return null;
        }

        return JsonConvert.DeserializeObject<SaveData>(saveFileJsonContents);
    }
}