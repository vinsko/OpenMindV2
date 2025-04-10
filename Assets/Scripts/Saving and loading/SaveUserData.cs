// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class SaveUserData
{
    private static          SaveUserData   _saver;
    private static readonly object _lock = new object();
    public static SaveUserData Saver
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
                        _saver = new SaveUserData();
                    }
                }
            }
            return _saver;
        }
    }

    public void UpdateUserData(UserData userData = null)
    {
        if (userData is null)
            userData = CreateUserData();

        if (userData is null)
            return;
        
        string jsonString = JsonConvert.SerializeObject(userData);
        string folderLocation = FilePathConstants.GetUserDataFolderLocation();
        string fileLocation = FilePathConstants.GetUserDataFileLocation();
        
        if (!Directory.Exists(folderLocation))
            Directory.CreateDirectory(folderLocation);
        
        File.WriteAllText(fileLocation,jsonString);
    }

    /// <summary>
    /// Updates a specific value in the userdata with a provided new bool-value
    /// </summary>
    /// <param name="query"></param>
    /// <param name="newValue"></param>
    public void UpdateUserDataValue(FetchUserData.UserDataQuery query, bool newValue)
    {
        // TODO: Not very pretty. Refactor

        // if no userdata exists yet, create a new userdata and save it.
        if (FetchUserData.Loader.GetUserData() == null)
            UpdateUserData();

        // fetch the userdata (whether it existed before or not)
        UserData currentUserData = FetchUserData.Loader.GetUserData();
        
        switch (query)
        {
            case FetchUserData.UserDataQuery.prologueSeen:
                currentUserData.prologueSeen = newValue;
                break;
            case FetchUserData.UserDataQuery.playedBefore:
                currentUserData.playedBefore = newValue;
                break;
            case FetchUserData.UserDataQuery.storyAWon:
                currentUserData.storyAWon = newValue;
                break;
            case FetchUserData.UserDataQuery.storyBWon:
                currentUserData.storyBWon = newValue;
                break;
            case FetchUserData.UserDataQuery.storyCWon:
                currentUserData.storyCWon = newValue;
                break;
            case FetchUserData.UserDataQuery.storyAIntroSeen:
                currentUserData.storyAIntroSeen = newValue;
                break;
            case FetchUserData.UserDataQuery.storyBIntroSeen:
                currentUserData.storyBIntroSeen = newValue;
                break;
            case FetchUserData.UserDataQuery.storyCIntroSeen:
                currentUserData.storyCIntroSeen = newValue;
                break;
            default:
                Debug.LogError("Invalid UserData query");
                break;
        }
        UpdateUserData(currentUserData);
    }

    private UserData CreateUserData()
    {
        return new UserData
        {
             prologueSeen = false, 
             playedBefore = false,
             storyAWon = false,
             storyBWon = false,
             storyCWon = false,
             storyAIntroSeen = false,
             storyBIntroSeen = false,
             storyCIntroSeen = false,
        };
    }
}
