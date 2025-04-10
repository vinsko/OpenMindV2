// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// An instance of a character. Takes a set of <see cref="CharacterData"/>
/// Each character in the game is a different a separate instance of this class. With its own <see cref="CharacterData"/>.
/// </summary>
public class CharacterInstance
{
    public CharacterData data;

    public Dictionary<Question, DialogueContainer> Answers = new();
    public Dictionary<Question, List<string>> Traits = new();
    public List<Question> RemainingQuestions = new();

    public string       characterName;
    public int          id;
    public List<(Emotion, Sprite)> avatarEmotions;
    public float        pitch;

    public bool isCulprit;      // This character is the culprit and a random characteristic is revealed every cycle
    public bool isActive;       // If they havent yet been the victim, should be true. Use this to track who is "alive" and you can talk to, and who can be removed by the culprit
    public bool talkedTo;       // If the player has already talked to this NPC in the current cycle, should be false at the start of every cycle and set to true once the player has talked with them
    
    /// <summary>
    /// The constructor for <see cref="CharacterInstance"/>.
    /// Sets this instances variables to the information from <see cref="data"/> 
    /// </summary>
    /// <param name="data">A set of <see cref="CharacterData"/></param>
    public CharacterInstance(CharacterData data)
    {
        this.data = data;
        characterName = data.characterName;
        id = data.id;
        ParseEmotionSprites(data.neutralAvatar, data.happyAvatar, data.unhappyAvatar);
        pitch = data.voicePitch;

        InitializeQuestions();
    }

    /// <summary>
    /// Get a random greeting from the character's list of greetings.
    /// </summary>
    /// <returns>A greeting in the form of dialogue segments.</returns>
    public DialogueObject GetGreeting([CanBeNull] GameObject[] background)
    {
        // If we havent talked to the NPC before..
        if (!talkedTo)
        {
            // Get greeting
            talkedTo = true;
            return (data.firstGreeting != null 
                ? data.firstGreeting.GetDialogue(background)
                : new ContentDialogueObject("Hello.", null, background));
        }
        
        // If we have talked to the NPC before, try to get the greeting-dialopgue.
        // if its null, default to "Hello".
        return (data.greeting != null 
            ? data.greeting.GetDialogue(background) 
            : new ContentDialogueObject("Hello.", null, background));
        
    }

    private void ParseEmotionSprites(Sprite neutralSprite, Sprite happySprite, Sprite unhappySprite)
    {
        avatarEmotions = new List<(Emotion, Sprite)>();
        TryAdd(avatarEmotions, Emotion.Neutral, neutralSprite, neutralSprite);
        TryAdd(avatarEmotions, Emotion.Happy, happySprite, neutralSprite);
        TryAdd(avatarEmotions, Emotion.Unhappy, unhappySprite, neutralSprite);
    }

    private void TryAdd(List<(Emotion,Sprite)> list, Emotion emotion, Sprite newAvatar, Sprite defaultAvatar)
    {
        if (newAvatar != null)
        {
            list.Add((emotion, newAvatar));
        }
        else if (defaultAvatar != null)
        {
            list.Add((emotion, defaultAvatar));
        }
        else
        {
            Debug.LogError("Default avatar is null.");
        }
    }

    public Sprite GetAvatar()
    {
        
        return avatarEmotions.First(se => se.Item1 == Emotion.Neutral).Item2;
    }

    /// <summary>
    /// Gets all traits of this character, can be modified later if traits are stored differently
    /// </summary>
    /// <returns>A list of type string containing all traits of this character</returns>
    private List<string>[] GetAllTraits()
    {
        return Traits.Values.ToArray();
    }

    /// <summary>
    /// Helper function for the constructor.
    /// Places character data (answers & traits) in their respective dictionaries.
    /// </summary>
    public void InitializeQuestions()
    {
        foreach (var kvp in data.answers)
        {
            Answers[kvp.question] = kvp.answer;
            Traits[kvp.question] = kvp.trait;
            RemainingQuestions.Add(kvp.question);
        }
    }
    
    /// <summary>
    /// The logic for obtaining a random trait and removing it from the list of available questions for all characters.
    /// If the random variable is left null, it will be obtained from gameManager, but it can be provided for slight optimization.
    /// This method is used for obtaining hints about the victim and the culprit at the start of each cycle.
    /// </summary>
    /// <returns>A List of strings containing a random trait of this character.</returns>
    public List<string> GetRandomTrait()
    {
        // If there are any questions remaining
        if (RemainingQuestions.Count > 0)
        {
            // Find a random question
            int randomInt = GameManager.gm.random.Next(RemainingQuestions.Count);
            Question question = RemainingQuestions[randomInt];
            
            // Remove question from all characters so that it can not be asked to anyone, if RemainingQuestions contains it.
            // TODO: discuss how we're gonna implement this feature --> for now, this leads to a bug where a character has not enough questions
            foreach (CharacterInstance character in GameManager.gm.currentCharacters)
            {
                if (character.RemainingQuestions.Contains(question))
                {
                    character.RemainingQuestions.Remove(question);
                }
            }

            // Return the answer to the question in trait form
            return Traits[question];
        }
        else
        {
            // In a normal game loop, this should never occur
            Debug.LogError("GetRandomTrait(), but there are no more traits remaining");

            return null;
        }
    }
}
