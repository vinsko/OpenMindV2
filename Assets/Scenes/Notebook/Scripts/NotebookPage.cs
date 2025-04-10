// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single page in the notebook.
/// </summary>
public class NotebookPage
{
    private readonly CharacterInstance _character;
    private string _notes;
    private string _placeholder;

    /// <summary>
    /// Constructor for making a new empty page.
    /// </summary>
    /// <param name="character">The character the page is on.</param>
    public NotebookPage(CharacterInstance character)
    {
        _character = character;
        _notes = string.Empty;
    }

    /// <summary>
    /// Constructor for making a page that already has writing.
    /// </summary>
    /// <param name="notes">The notes that have already been written.</param>
    /// <param name="character">The character the page is on.</param>
    public NotebookPage(string notes, CharacterInstance character)
    {
        _character = character;
        _notes = notes;
    }
    
    /// <summary>
    /// Method that fetches placeholder text in case player hasn't written any notes yet.
    /// </summary>
    /// <returns></returns>
    public string GetPlaceholder()
    {
        _placeholder = "Notes on " + _character.characterName + ".\n";
        return _placeholder;
    }


    /// <summary>
    /// Method which gets the notes contained on this page.
    /// For external use.
    /// </summary>
    /// <returns>The notes written on this page.</returns>
    public string GetNotes()
    {
        return _notes;
    }

    /// <summary>
    /// Method which sets the notes on this page to the input.
    /// For external use.
    /// </summary>
    /// <param name="input">New set of notes.</param>
    public void SetNotes(string input)
    {
        _notes = input;
    }
}
