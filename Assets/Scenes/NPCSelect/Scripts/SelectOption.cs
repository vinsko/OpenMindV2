// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Instances of this class act as special buttons for the NPCSelect scene.
/// </summary>
public class SelectOption : MonoBehaviour
{
    public CharacterInstance character;

    [SerializeField] private Image     avatarImage;
    [SerializeField] private Component selectionManager;

    /// <summary>
    /// On startup, set the sprite and name to that of the proper character and check whether it is active or not.
    /// </summary>
    void Start()
    {
        avatarImage.sprite =  
            character.avatarEmotions.First(se => se.Item1 == Emotion.Neutral).Item2;

        if (!character.isActive)
            SetInactive();
    }

    /// <summary>
    /// If a character is inactive grey out the button.
    /// </summary>
    private void SetInactive()
    {
        avatarImage.color = new Color(0.6f,0.6f,0.6f,0.6f);
    }
}
