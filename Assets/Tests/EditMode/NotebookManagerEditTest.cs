using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.TextCore.Text;
using TMPro;

public class NotebookManagerEditTest
{
    private NotebookManager         nm;
    private List<CharacterInstance> characters;
    
    [OneTimeSetUp]
    public void Setup()
    {
        // Get some random characters to make the notebook for
        CharacterData c1 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/0_Fatima_Data.asset", typeof(CharacterData));
        CharacterData c2 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/1_Giulietta_Data.asset", typeof(CharacterData));
        CharacterData c3 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/2_Willow_Data.asset", typeof(CharacterData));
        CharacterData c4 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/3_Olivier_Data.asset", typeof(CharacterData));
        CharacterData c5 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/4_Aiden_Data.asset", typeof(CharacterData));
        CharacterData c6 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/5_Youssef_Data.asset", typeof(CharacterData));
        CharacterData c7 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/6_Ana_Data.asset", typeof(CharacterData));
        CharacterData c8 = (CharacterData) AssetDatabase.LoadAssetAtPath("Assets/Data/Character Data/7_Arslan_Data.asset", typeof(CharacterData));

        characters = new List<CharacterInstance>{
            new CharacterInstance(c1),
            new CharacterInstance(c2),
            new CharacterInstance(c3),
            new CharacterInstance(c4),
            new CharacterInstance(c5),
            new CharacterInstance(c6),
            new CharacterInstance(c7),
            new CharacterInstance(c8)
        };

        // Load "Loading scene" and find GameManager to set it up
        EditorSceneManager.OpenScene("Assets/Scenes/Loading/Loading.unity");
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.currentCharacters = new List<CharacterInstance>();
        
        foreach (CharacterInstance c in characters)
            gm.currentCharacters.Add(c);
        
        GameManager.gm = gm;

        EditorSceneManager.OpenScene("Assets/Scenes/Notebook/NotebookScene.unity");
        nm = GameObject.Find("NotebookManager").GetComponent<NotebookManager>();
    }
}