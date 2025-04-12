using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class FetchUserData
{
    private static          FetchUserData _loader;
    private static readonly object        _lock = new object();
    public static FetchUserData Loader
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
                        _loader = new FetchUserData();
                    }
                }
            }
            return _loader;
        }
    }    
    
    
    public enum UserDataQuery
    {
        prologueSeen,
        playedBefore,
        storyAWon,
        storyBWon,
        storyCWon,
        storyAIntroSeen,
        storyBIntroSeen,
        storyCIntroSeen
    }


    public UserData GetUserData()
    {
        string userDataFileLocation = FilePathConstants.GetUserDataFileLocation();
        string userDataFileJsonContents = FilePathConstants.GetSafeFileContents(userDataFileLocation, "User Data", "Loading");
        if (userDataFileJsonContents is null)
        {
            Debug.LogError("No userdata was found");
            return null;
        }

        return JsonConvert.DeserializeObject<UserData>(userDataFileJsonContents);
    }
    
    /// <summary>
    /// Gets a <see cref="SaveData"/> object from the save file contents
    /// </summary>
    public bool GetUserDataValue(UserDataQuery query)
    {
        string userDataFileLocation = FilePathConstants.GetUserDataFileLocation();
        string userDataFileJsonContents = FilePathConstants.GetSafeFileContents(userDataFileLocation, "User Data", "Loading");
        if (userDataFileJsonContents is null)
        {
            Debug.LogError("No userdata was found");
            return false;
        }
        
        UserData userData = JsonConvert.DeserializeObject<UserData>(userDataFileJsonContents);
        bool output;    // could use returns instead, but with this we can debug.
        
        switch (query)
        {
            case UserDataQuery.prologueSeen:
                output = userData.prologueSeen;
                break;
            case UserDataQuery.playedBefore:
                output =  userData.playedBefore;
                break;
            case UserDataQuery.storyAWon:
                output =  userData.storyAWon;
                break;
            case UserDataQuery.storyBWon:
                output =  userData.storyBWon;
                break;
            case UserDataQuery.storyCWon:
                output =  userData.storyCWon;
                break;
            case UserDataQuery.storyAIntroSeen:
                output =  userData.storyAIntroSeen;
                break;
            case UserDataQuery.storyBIntroSeen:
                output =  userData.storyBIntroSeen;
                break;
            case UserDataQuery.storyCIntroSeen:
                output =  userData.storyCIntroSeen;
                break;
            default:
                Debug.LogError("Invalid UserData query");
                return false;
        }
        
        return output;
    }
}
