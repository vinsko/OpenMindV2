// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ResizingTextBox : MonoBehaviour
{
    [Header("Text")]
    [TextArea(2, 10)]
    public string textContent;
    public int textSize;

    [Header("Box width")]
    public float minWidth;
    public float maxWidth;

    [Header("Box sprite")]
    public Sprite sprite;

    [Header("Component references")]
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private Image image;

    public void SetText(string text)
    {
        textComponent.text = text;
        AdjustFontSize();
    }

    public void AdjustFontSize() => textComponent.fontSize = SettingsManager.sm.GetFontSize();
}