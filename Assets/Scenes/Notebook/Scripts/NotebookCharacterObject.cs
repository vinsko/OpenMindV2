// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotebookCharacterObject : MonoBehaviour
{
    [Header("Component Refs")]
    [SerializeField] private CharacterIcon characterIcon;
    [SerializeField] private TMP_Text nameText;

    /// <summary>
    /// Print info on character to current notebook
    /// </summary>
    public void SetInfo(CharacterInstance character)
    {
        nameText.text = character.characterName;
        characterIcon.SetAvatar(character);
        characterIcon.BackgroundColor = new Color(0, 0, 0, 0.2f);

        if (SettingsManager.sm != null)
            nameText.fontSize = SettingsManager.sm.GetFontSize() * SettingsManager.M_LARGE_TEXT;
    }
}