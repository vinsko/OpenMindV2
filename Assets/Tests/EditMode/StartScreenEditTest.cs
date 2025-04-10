// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;

public class StartScreenTest : MonoBehaviour
{
    private StartMenuManager sm;
    
    [OneTimeSetUp]

    public void Setup()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Menus/StartScreenScene.unity");
        sm = GameObject.Find("StartMenuManager").GetComponent<StartMenuManager>();
    }
    
}
