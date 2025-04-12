// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Method in this class are raised by Game Events to start and stop the loading icon animation
/// </summary>
public class LoadingScreenManager : MonoBehaviour
{
    public Canvas popUpCanvas; // canvas that will show the loading icon
    public float guaranteeLoadDuration; // duration to plug into WaitForSeconds to guarantee a certain length of the loading animation


    /// <summary>
    /// Enables canvas to show loading animation.
    /// </summary>
    public void OpenPopUp()
    {
        popUpCanvas.enabled = true;
    }

    /// <summary>
    /// Waits for certain amount of second before stopping load animation.
    /// </summary>
    public void ClosePopUp()
    {
        StartCoroutine(IconWait());
    }

    // Waits certain amount of seconds to give player feeling of something happening
    IEnumerator IconWait()
    {
        if (popUpCanvas.enabled)
            yield return new WaitForSeconds(guaranteeLoadDuration);

        popUpCanvas.enabled = false;
    }
}
