// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class DialogueAnimatorPlayTest
{
    private TMP_Text textField;
    private DialogueAnimator animator;

    #region Setup & Teardown
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Create a new GameObject & add text component & animator
        GameObject textObject = new GameObject("TestTextObject");

        textField = textObject.AddComponent<TextMeshProUGUI>();
        animator = textObject.AddComponent<DialogueAnimator>();
        animator.Test_SetTextComponent(textField);

        animator.Test_DelayInSeconds = 0.01f;
        animator.Test_DelayAfterSentence = 0.05f;
        animator.Test_IgnoreSkipDelay = true;

        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        animator.CancelWriting();
        GameObject.Destroy(textField.gameObject);
    }
    #endregion

    /// <summary>
    /// Tests if each character is being written at the right time.
    /// </summary>
    [UnityTest]
    public IEnumerator SingleLineWritingDelayTest()
    {
        // The different cases to test for
        float[] delays = new float[] { 0.01f, 0.003f };

        string text = "Hello, World!";

        foreach (float delay in delays)
        {
            animator.Test_DelayInSeconds = delay;
            animator.WriteDialogue(text);

            string expectedText = "";
            for (int i = 0; i < text.Length; i++)
            {
                expectedText += text[i];

                Assert.AreEqual(expectedText, textField.text);

                yield return new WaitForSeconds(delay);
            }
        }
    }

    /// <summary>
    /// Checks whether each letter is typed at the right moment when multiple segmentslines are used.
    /// </summary>
    [UnityTest]
    public IEnumerator MultiLineWritingDelayTest()
    {
        animator.Test_DelayInSeconds = 0.01f;
        animator.Test_DelayAfterSentence = 0.02f;

        List<string> lines = new List<string> { "Hello, World!", "foo", "bar" };
        animator.WriteDialogue(lines);

        // Go through each letter and check if it is typed when expected
        for (int i = 0; i < lines.Count; i++)
        {
            string line = "";
            for (int j = 0; j < lines[i].Length; j++)
            {
                line += lines[i][j];
                Assert.AreEqual(line, textField.text);

                // Await next letter
                yield return new WaitForSeconds(animator.Test_DelayInSeconds);
            }

            // Await next line start
            yield return new WaitForSeconds(animator.Test_DelayAfterSentence);
            animator.SkipDialogue();
        }
    }

    /// <summary>
    /// Checks if the InDialogue check is working as expected
    /// </summary>
    [UnityTest]
    public IEnumerator InDialogueCheckTest()
    {

        string text = "Hello, World!";

        // InDialogue should be false before we start writing
        Assert.IsFalse(animator.InDialogue);

        // InDialogue should be true right after we start writing
        animator.WriteDialogue(text);
        Assert.IsTrue(animator.InDialogue);

        // InDialogue should be true before we should be finished writing
        yield return new WaitForSeconds(animator.Test_DelayInSeconds * text.Length * 0.5f);
        Assert.IsTrue(animator.InDialogue);

        // InDialogue should be true when the dialogue is skipped before it was finished (it is still on the screen)
        animator.SkipDialogue();
        Assert.IsTrue(animator.InDialogue);

        // InDialogue should be false when we skip to close the dialogue
        yield return new WaitForSeconds(animator.inputDelay);
        animator.SkipDialogue();
        Assert.IsFalse(animator.InDialogue);

        // InDialogue should be true even if we suddenly cancel writing
        animator.WriteDialogue(text);
        yield return new WaitForSeconds(animator.Test_DelayInSeconds * 2);
        animator.CancelWriting();
        Assert.IsTrue(animator.InDialogue);


    }

    /// <summary>
    /// Checks if the InDialogue check is working as expected
    /// </summary>
    [UnityTest]
    public IEnumerator IsOutputtingCheckTest()
    {
        string text = "Hello, World!";

        // IsOutputting should be false before we start writing
        Assert.IsFalse(animator.IsOutputting);

        // IsOutputting should be true right after we start writing
        animator.WriteDialogue(text);
        Assert.IsTrue(animator.IsOutputting);

        // IsOutputting should be true before we have finished writing
        yield return new WaitForSeconds(animator.Test_DelayInSeconds * text.Length * 0.5f);
        Assert.IsTrue(animator.IsOutputting);

        // IsOutputting should be false when we skip dialogue
        animator.SkipDialogue();
        Assert.IsFalse(animator.IsOutputting);

        // IsOutputting should be false when we cancel the writing coroutine
        animator.WriteDialogue(text);
        yield return new WaitForSeconds(animator.Test_DelayInSeconds * 2);
        animator.CancelWriting();
        Assert.IsFalse(animator.IsOutputting);
    }

    /// <summary>
    /// Checks if CancelWriting() cancels at the correct point
    /// </summary>
    [UnityTest]
    public IEnumerator CancelWritingTest()
    {
        string text = "Hello, World!";
        animator.WriteDialogue(text);

        int n = 3;
        string expectedText = text[..n]; // Take the first n letters
        yield return new WaitForSeconds(animator.Test_DelayInSeconds * n);

        animator.CancelWriting();

        yield return new WaitForSeconds(animator.Test_DelayInSeconds);

        Assert.AreEqual(expectedText, textField.text);
    }

    /// <summary>
    /// Tests whether or not the OnDialogueComplete event is properly invoked.
    /// </summary>
    [UnityTest]
    public IEnumerator EndDialogueEventTest()
    {
        string text = "Hello, World!";
        bool dialogueEndCheck = false;
        animator.OnDialogueComplete.AddListener(() => SetBool(ref dialogueEndCheck));
        animator.WriteDialogue(text);

        // End the dialogue
        yield return new WaitForSeconds(animator.inputDelay);
        animator.SkipDialogue();
        yield return new WaitForSeconds(animator.inputDelay);
        animator.SkipDialogue();
        
        Assert.IsTrue(dialogueEndCheck);

        yield return null;
    }

    /// <summary>
    /// Helper function which simply sets a bool reference to its opposite value.
    /// </summary>
    private void SetBool(ref bool p) => p = !p;
}
