﻿// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
public class CharacterIcon : MonoBehaviour
{
    private const float ZOOM_FACTOR = 1.8f;

    [SerializeField] private Image avatarImageRef;
    private Image backgroundImageRef;

    private CharacterInstance character;

    /// <summary>
    /// The color of the icon's background.
    /// </summary>
    public Color BackgroundColor
    { 
        get { return backgroundImageRef.color; }
        set { backgroundImageRef.color = value; }
    }

    /// <summary>
    /// The overlay color on the avatar image.
    /// </summary>
    public Color OverlayColor
    {
        get { return avatarImageRef.color; }
        set { avatarImageRef.color = value; }
    }

    private void Awake()
    {
        backgroundImageRef = GetComponent<Image>();
    }

    public void SetAvatar(CharacterInstance character)
    {
        this.character = character;

        // Set the correct sprite
        avatarImageRef.sprite = character.avatarEmotions.Where(es => es.Item1 == Emotion.Neutral).First().Item2;

        // Set the image location to the center of the face
        var rectTransform = avatarImageRef.GetComponent<RectTransform>();
        rectTransform.pivot = character.data.facePivot;
        rectTransform.anchoredPosition = Vector2.zero;

        SetAvatarSize();
    }

    private void OnRectTransformDimensionsChange()
    {
        // Check edge case
        if (character == null) 
            return;

        // Rescale the avatar if the icon scale has changed
        SetAvatarSize();
    }

    /// <summary>
    /// Set the size of the avatar to match the size of the icon.
    /// </summary>
    private void SetAvatarSize()
    {
        // The ratio between the width & height of the character's sprite
        float ratio = character.avatarEmotions.Where(
            es => es.Item1 == Emotion.Neutral).First().Item2.rect.width /
            character.avatarEmotions.Where(
            es => es.Item1 == Emotion.Neutral).First().Item2.rect.height;

        // Set the avatar size according to the icon's size
        float width = ZOOM_FACTOR * Mathf.Abs(GetComponent<RectTransform>().rect.height);
        float height = width / ratio;
        avatarImageRef.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    }

#if UNITY_INCLUDE_TESTS
    public CharacterInstance Test_Character { get { return character; } }
#endif
}
