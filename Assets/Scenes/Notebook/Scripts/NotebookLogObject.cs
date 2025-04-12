// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Logs of what characters have said are not saved or printed anymore, so currently this class goes unused.
/// </summary>
public class NotebookLogObject : MonoBehaviour
{
    [Header("Text Component Refs")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text answerText;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="question"></param>
    /// <param name="answer"></param>
    public void SetText(string question, string answer)
    {
        questionText.text = question;
        answerText.text = answer;

        // Set appropriate font sizes
        if (SettingsManager.sm == null) return;
        questionText.fontSize = SettingsManager.sm.GetFontSize() * SettingsManager.M_SMALL_TEXT;
        answerText.fontSize = SettingsManager.sm.GetFontSize() * SettingsManager.M_SMALL_TEXT;
    }
}