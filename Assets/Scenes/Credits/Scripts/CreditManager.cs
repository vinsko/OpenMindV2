using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditManager : MonoBehaviour
{
    [SerializeField] private RectTransform creditTransform;
    [SerializeField] private RectTransform thanksTransform;
    [SerializeField] private GameButton button;

    private float duration = 20;

    // Start is called before the first frame update
    async void Start()
    {
        await ScrollCredits();
        button.interactable = true;
    }


    /// <summary>
    /// Calls DoCredits to animate credits crolling through screen
    /// </summary>
    public Task ScrollCredits()
    {
        var tcs = new TaskCompletionSource<bool>();

        float creditStartY = creditTransform.position.y;
        float creditEndY = Screen.height + creditTransform.rect.height / 2;
        float thanksStartY = thanksTransform.position.y;
        float thanksEndY = Screen.height * 0.75f;

        DoCredits(creditStartY, creditEndY, thanksStartY, thanksEndY, tcs);

        return tcs.Task;
    }

    /// <summary>
    /// Starts animation Coroutine by calling AnimateScroll
    /// </summary>
    public void DoCredits(float creditStartY, float creditEndY, float thanksStartY, float thanksEndY, TaskCompletionSource<bool> tcs = null)
    {
        StartCoroutine(AnimateScroll(duration, creditStartY, creditEndY, thanksStartY, thanksEndY, tcs));
    }

    private IEnumerator AnimateScroll(
        float duration, 
        float creditStartY, float creditEndY, 
        float thanksStartY, float thanksEndY, 
        TaskCompletionSource<bool> tcs)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;

            // calc new y offset and set new credits position
            float y = (creditEndY - creditStartY) * (time / duration);
            creditTransform.position = new Vector2(creditTransform.position.x, creditStartY + y);
            
            // only giving offset to thanksTranform if it has not reached the middle of the screen
            // so it stops in the middle of the screen while the rest of the credits continue
            if (thanksTransform.position.y < thanksEndY)
            {
                thanksTransform.position = new Vector2(thanksTransform.position.x, thanksStartY + y);
            }

            yield return null;
        }

        tcs?.SetResult(true);
    }

    public void CloseCredits()
    {
        if (SceneManager.GetSceneByName("StartScreenScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("CreditsScene");
        }
    }
}
