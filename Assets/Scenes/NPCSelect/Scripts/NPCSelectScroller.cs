// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NPCSelectScroller : MonoBehaviour
{
    private Transform scrollable;

    private bool isNavigating = false;
    private Coroutine scrollCoroutine;

    // References to the navigation buttons
    private GameObject navButtonLeft;
    private GameObject navButtonRight;

    private SwipeDetector swipeDetector;

    /// <summary>
    /// The character which is currently selected in the scroller.
    /// </summary>
    public CharacterInstance SelectedCharacter { get; private set; }

    /// <summary>
    /// The transforms of all the characterspace children
    /// </summary>
    public Transform[] Children { get; private set; }

    /// <summary>
    /// How long should one navigation cycle take?
    /// </summary>
    public float scrollDuration;

    /// <summary>
    /// This event is invoked when the selected character is in the center of the screen.
    /// </summary>
    [NonSerialized] public UnityEvent OnCharacterSelected = new();

    /// <summary>
    /// This event is invoked when the screen moves away from the selected character.
    /// </summary>
    [NonSerialized] public UnityEvent NoCharacterSelected = new();

    private int selectedChild;
    public int SelectedChild
    {
        get { return selectedChild; }
        private set
        {
            // Make sure the target value is not too big or small
            selectedChild = Mathf.Clamp(value, 0, Children.Length - 1);

            // Set selected character
            SelectedCharacter = Children[selectedChild].GetComponentInChildren<SelectOption>().character;

            // Remove button if the child is on either edge
            if (selectedChild == 0)
            {
                navButtonLeft.SetActive(false);
            }
            else if (selectedChild == Children.Length - 1)
                navButtonRight.SetActive(false);
            else
            {
                navButtonLeft.SetActive(true);
                navButtonRight.SetActive(true);
            }
        }
    }

    private void Awake()
    {
        // Add swipe listeners
        swipeDetector = GetComponent<SwipeDetector>();
        swipeDetector.OnSwipeLeft.AddListener(NavigateRight);
        swipeDetector.OnSwipeRight.AddListener(NavigateLeft);
        
        // Get references to the nav button objects
        try
        {
            navButtonLeft = transform.GetChild(1).gameObject;
            navButtonRight = transform.GetChild(2).gameObject;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "NPC Select navigation buttons were not found.\n" +
                "They should be child index 1 and 2 of the scroller.\n" +
                e);
        }

        // Get reference to scroll background
        scrollable = transform.GetChild(0);
        var layout = scrollable.GetChild(0);

        // Populate list of children
        Children = new Transform[GameManager.gm.currentCharacters.Count];
        for (int i = 0; i < Children.Length; i++)
            Children[i] = layout.GetChild(i);
    }

    private void Start()
    {
        // Set the initially selected child
        SelectedChild = 0;
    }

    /// <summary>
    /// Smoothly move one child to the left.
    /// </summary>
    public void NavigateLeft()
    {
        if (SettingsManager.sm?.IsPaused == true)
            return;

        if (SelectedChild > 0)
        {
            SelectedChild -= 1;
            NavigateToChild(SelectedChild);
        }
    }

    /// <summary>
    /// Smoothly move one child to the right.
    /// </summary>
    public void NavigateRight()
    {
        if (SettingsManager.sm?.IsPaused == true)
            return;

        if (SelectedChild < Children.Length - 1)
        {
            SelectedChild += 1;
            NavigateToChild(SelectedChild);
        }
    }

    /// <summary>
    /// Navigate to the given child.
    /// </summary>
    /// <param name="childIndex">The index of the child to navigate to.</param>
    private void NavigateToChild(int childIndex)
    {
        if (isNavigating)
            StopCoroutine(scrollCoroutine);

        scrollCoroutine = StartCoroutine(NavigationCoroutine(childIndex));
    }

    /// <summary>
    /// Does the same thing as NavigationCoroutine(), but in a single frame 
    /// (the same frame in which the coroutine is called).
    /// </summary>
    /// <param name="childIndex">The index of the child to navigate to.</param>
    private IEnumerator InstantNavigate(int childIndex)
    {
        // Wait until the parent objects are scaled properly
        yield return new WaitForEndOfFrame();
        
        scrollable.localPosition = GetTargetPos(childIndex);
        OnCharacterSelected.Invoke();
    }

    /// <summary>
    /// Smoothly move the first child so that the target child is in the center of the screen.
    /// </summary>
    /// <param name="childIndex">The index of the child to navigate to.</param>
    private IEnumerator NavigationCoroutine(int childIndex)
    {
        NoCharacterSelected.Invoke();

        isNavigating = true;
        float time = 0;

        var startPos = scrollable.localPosition;
        var endPos = GetTargetPos(childIndex);
        

        // This loop containts the actual movement code
        while (time < scrollDuration)
        {
            time += Time.unscaledDeltaTime;

            // Mathf.SmoothStep makes the "animation" ease in and out
            float t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(time / scrollDuration));

            // Vector3.Lerp creates a linear interpolation between the given positions
            scrollable.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        OnCharacterSelected.Invoke();
        isNavigating = false;
    }

    /// <summary>
    /// Returns the position which the first child needs to have in order to center the target child.
    /// </summary>
    /// <param name="childIndex"></param>
    /// <returns>A Vector3 containing the appropriate x-value.</returns>
    private Vector3 GetTargetPos(int childIndex)
    {
        return new Vector2(
            -Children[childIndex].localPosition.x,
            scrollable.localPosition.y);
    }

    /// <summary>
    /// Get the index of the character in the list of children.
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    private int GetCharacterIndex(CharacterInstance character)
    {
        int index = 0;
        for (int i = 0; i < Children.Length; i++)
        {
            CharacterInstance characterChild = Children[i].GetComponentInChildren<SelectOption>().character;
            // Check if the character id matches the character given as the parameter.
            if (characterChild.id == character.id)
            {
                index = i;
            }
        }

        return index;
    }
    
    /// <summary>
    /// Set the selected character to be the last character which was talked to during the dialogue.
    /// </summary>
    public async void SetSelectedCharacter(Component sender, params object[] data)
    {
        if (data.Length > 0 && data[0] is CharacterInstance recipient)
        {
            SelectedChild = GetCharacterIndex(recipient);
        }
        else
        {
            SelectedChild = 0;
        }
        
        StartCoroutine(InstantNavigate(SelectedChild));
    }

    #region Test Variables
#if UNITY_INCLUDE_TESTS
    public void Test_InstantNavigate(int childIndex) => StartCoroutine(InstantNavigate(childIndex));

    public Transform[] Test_Children 
    { 
        get { return Children; } 
    }

    public int Test_SelectedChild
    {
        get { return SelectedChild; }
        set { SelectedChild = value; }
    }

    public float Test_ScrollDuration 
    { 
        get { return scrollDuration; }
        set { scrollDuration = value; }
    }

    public void Test_NavigateToChild(int childIndex) => NavigateToChild(childIndex);
#endif
    #endregion
}
