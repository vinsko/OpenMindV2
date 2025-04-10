// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Class which handles transitions.
/// </summary>
public class TransitionAnimator : MonoBehaviour
{
    /// <summary>
    /// The static instance of the transition animator
    /// </summary>
    public static TransitionAnimator i;

    [SerializeField] private Animator animator;

    /// <summary>
    /// Different kinds of animations
    /// </summary>
    public enum AnimationType
    {
        Fade = 0,
        Wipe = 1
    }

    /// <summary>
    /// On startup, initialize the static instance of this class, and make it DDOL
    /// </summary>
    private void Awake()
    {
        i = this;
    }
    
    /// <summary>
    /// The function that should be called to start the fade animation.
    /// Only fades to black.
    /// Can be awaited.
    /// </summary>
    public Task PlayStartAnimation(AnimationType type = AnimationType.Fade, float timeScale = 1)
    {
        var tcs = new TaskCompletionSource<bool>();
        PlayAnimation("SceneLoading", type, timeScale, tcs);
        return tcs.Task;
    }

    /// <summary>
    /// The function that should be called to end the fade animation.
    /// Only fades to black.
    /// Can be awaited.
    /// </summary>
    public Task PlayEndAnimation(AnimationType type = AnimationType.Fade, float timeScale = 1)
    {
        var tcs = new TaskCompletionSource<bool>();
        PlayAnimation("SceneLoaded", type, timeScale, tcs);
        return tcs.Task;
    }

    /// <summary>
    /// Function which plays the transition animation.
    /// </summary>
    private Task PlayAnimation(string trigger, AnimationType type, float timeScale, TaskCompletionSource<bool> tcs)
    {
        // Set animator vars
        animator.SetTrigger(trigger); // Set trigger
        animator.SetInteger("AnimationType", (int)type); // Set animation type
        animator.speed = timeScale; // Set speed

        // Wait for animation to finish & return completion
        StartCoroutine(AnimationCoroutine(tcs));
        return tcs.Task;
    }

    /// <summary>
    /// Helper coroutine for the animation.
    /// </summary>
    /// <param name="tcs"></param>
    private IEnumerator AnimationCoroutine(TaskCompletionSource<bool> tcs)
    {
        float seconds = 0;
        // Wait for the animator to update clip
        yield return new WaitUntil(() =>
        {
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length == 0)
                return false;
            
            seconds = clipInfo[0].clip.length;
            return true;
        });
        
        // Await the length of the animation
        yield return new WaitForSeconds(seconds);

        tcs.SetResult(true);
    }
}
