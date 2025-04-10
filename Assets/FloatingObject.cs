using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    private RectTransform rectTransform;
    private float amplitude;
    private Vector2 initialPosition;

    [Header("Settings")]
    [SerializeField] private float bobbingFactor = 0.5f;
    [SerializeField] private float bobbingFrequency = 1f;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        amplitude = bobbingFactor * rectTransform.rect.height;
        initialPosition = rectTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the new y position using a sine wave
        float newY = Mathf.Sin(Time.time * bobbingFrequency) * amplitude;

        // Apply the new position while keeping other axes unchanged
        rectTransform.localPosition = new Vector2(initialPosition.x, initialPosition.y + newY);

    }
}
