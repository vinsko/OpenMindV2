// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotebookTitleObject : MonoBehaviour
{
    [Header("Component Refs")]
    [SerializeField] private TMP_Text titleText;

    /// <summary>
    /// Set name of current page of the notebook
    /// </summary>
    public void SetInfo(string title)
    {
        titleText.text = title;

        if (SettingsManager.sm != null)
            titleText.fontSize = SettingsManager.sm.GetFontSize() * SettingsManager.M_LARGE_TEXT;
    }
}
