// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotebookTabButton : MonoBehaviour
{
    private Coroutine animationCoroutine;

    /// <summary>
    /// Smoothly set this tab to the given height.
    /// </summary>
    /// <param name="newHeight">The height the tab will have by the end of the animation.</param>
    /// <param name="duration">The duration of the animation.</param>
    public void AnimateTab(float newHeight, float duration)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(
            TabAnimationCoroutine(newHeight, duration));
    }

    /// <summary>
    /// The coroutine which animates the tab expanding/collapsing.
    /// </summary>
    private IEnumerator TabAnimationCoroutine(float newHeight, float duration)
    {
        var rect = GetComponent<RectTransform>();

        // Set the pivot to the object's bottom
        rect.pivot = new Vector2(rect.pivot.x, 0);

        float originalHeight = rect.sizeDelta.y;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;

            // Use SmoothStep to create a dampened interpolation
            float timeStep = Mathf.SmoothStep(0, 1, time / duration);
            float height = Mathf.Lerp(originalHeight, newHeight, timeStep);

            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);

            yield return null;
        }

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);
    }
}
