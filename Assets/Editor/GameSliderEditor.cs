using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(GameSlider))]
public class GameSliderEditor : Editor
{
    #region Serialized Properties
    private SerializedProperty step;
    private SerializedProperty valueRounding;
    private SerializedProperty valueText;
    private SerializedProperty valueInfo;
    private SerializedProperty defaultValueEnabled;
    private SerializedProperty defaultValue;
    private SerializedProperty sliderComponentRef;
    private SerializedProperty partitionSpace;
    private SerializedProperty partitionLinePrefab;
    private SerializedProperty partitionsFieldRect;

    private bool valueTextGroup = false;
    private bool defaultValueGroup = false;
    private bool partitionGroup = false;
    #endregion

    private void OnEnable()
    {
        step = serializedObject.FindProperty(nameof(step));
        valueText = serializedObject.FindProperty(nameof(valueText));
        valueInfo = serializedObject.FindProperty(nameof(valueInfo));
        valueRounding = serializedObject.FindProperty(nameof(valueRounding));
        defaultValue = serializedObject.FindProperty(nameof(defaultValue));
        defaultValueEnabled = serializedObject.FindProperty(nameof(defaultValueEnabled));
        sliderComponentRef = serializedObject.FindProperty(nameof(sliderComponentRef));
        partitionSpace = serializedObject.FindProperty(nameof(partitionSpace));
        partitionLinePrefab = serializedObject.FindProperty(nameof(partitionLinePrefab));
        partitionsFieldRect = serializedObject.FindProperty(nameof(partitionsFieldRect));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Add a custom field for the GameEvent
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Game Slider Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(step);
        EditorGUILayout.PropertyField(sliderComponentRef);

        valueTextGroup = EditorGUILayout.BeginFoldoutHeaderGroup(valueTextGroup, "Value Text Settings");
        if (valueTextGroup)
        {
            EditorGUILayout.PropertyField(valueText, new GUIContent("Text Object Ref"));
            EditorGUILayout.PropertyField(valueInfo, new GUIContent("Unit Text"));
            EditorGUILayout.IntSlider(valueRounding, 0, 5, new GUIContent("Decimal Rounding"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        defaultValueGroup = EditorGUILayout.BeginFoldoutHeaderGroup(defaultValueGroup, "Default Value Settings");
        if (defaultValueGroup)
        {
            EditorGUILayout.PropertyField(defaultValueEnabled, new GUIContent("Enable Default Value"));
            EditorGUILayout.PropertyField(defaultValue, new GUIContent("Default Value Suggestion"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        partitionGroup = EditorGUILayout.BeginFoldoutHeaderGroup(partitionGroup, "Partition Line Settings");
        if (partitionGroup)
        {
            EditorGUILayout.PropertyField(partitionSpace, new GUIContent("Partition Line Distance"));
            EditorGUILayout.PropertyField(partitionLinePrefab, new GUIContent("Partition Line Prefab"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Required Fields & References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(partitionsFieldRect, new GUIContent("Partitions Field Rect Transform"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
