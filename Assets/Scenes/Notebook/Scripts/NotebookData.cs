// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that stores the data written in the notebook
/// </summary>
public class NotebookData
{
    private Dictionary<CharacterInstance, NotebookPage> _pages = 
        new Dictionary<CharacterInstance, NotebookPage>();

    private string _personalNotes;

    /// <summary>
    /// Constructor for creating a new empty notebook
    /// </summary>
    public NotebookData()
    {
        // Empty all pages when we create new notebookdata
        _pages = new Dictionary<CharacterInstance, NotebookPage>();

        // TODO: create a method that lets us fill it up based on the characters, instead of hiding it in the constructor
        // TODO: Or create checks for this, in case there are no characters yet
        foreach (CharacterInstance character in GameManager.gm.currentCharacters)
        {
            NotebookPage page = new NotebookPage(character);
            _pages[character] = page;
        }
    }

    /// <summary>
    /// Constructor for creating a notebook that already has writing 
    /// </summary>
    /// <param name="pages">The pages in the notebook</param>
    /// <param name="personalNotes"> The notes the player has taken</param>
    public NotebookData(Dictionary<CharacterInstance, NotebookPage> pages, string personalNotes)
    {
        _pages = pages;
        _personalNotes = personalNotes;
    }

    /// <summary>
    /// Gets placeholder text from character
    /// </summary>
    public string GetCharacterPlaceholder(CharacterInstance character)
    {
        return _pages[character].GetPlaceholder();
    }


    /// <summary>
    /// Get the notes the player has written about a character.
    /// </summary>
    public string GetCharacterNotes(CharacterInstance character)
    {
        return _pages[character].GetNotes();
    }
    
    /// <summary>
    /// Save the text that the player has written about a character to the notebookpage.
    /// </summary>
    public void UpdateCharacterNotes(CharacterInstance character, string notes)
    {
        _pages[character].SetNotes(notes);
    }
    
    /// <summary>
    /// Write the player's personal notes to the notebookdata.
    /// </summary>
    public void UpdatePersonalNotes(string input)
    {
        _personalNotes = input;
    }
    
    /// <summary>
    /// Get the player's written personal notes from the notebookdata.
    /// </summary>
    public string GetPersonalNotes()
    {
        return _personalNotes;
    }

    public override string ToString()
    {
        string output = $"personal: {_personalNotes}, characterNotes:[";
        foreach (var kv in _pages)
        {
            output += $"{kv.Key.characterName}: {kv.Value.GetNotes()}, ";
        }
        output = output.Substring(0, output.Length - 2);
        output += "]";
        return output;
    }
}
