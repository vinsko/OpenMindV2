// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)


using System.Collections.Generic;
using Newtonsoft.Json;

public class NotebookDataPackage
{
    private List<CharacterInstance> characters;
    
    public Dictionary<int, string> characterNotes;
    public string                  personalNotes;
    
    [JsonConstructor]
    public NotebookDataPackage(Dictionary<int, string> characterNotes, string personalNotes)
    {
        this.characterNotes = characterNotes;
        this.personalNotes = personalNotes;
    }
    
    public NotebookDataPackage(NotebookData data, List<CharacterInstance> characters)
    {
        characterNotes = new();
        this.characters = characters;
        foreach (CharacterInstance character in characters)
            characterNotes.Add(character.id, data.GetCharacterNotes(character));
        
        personalNotes = data.GetPersonalNotes();
    }
    
    public NotebookDataPackage(NetworkPackage data, List<CharacterInstance> characters)
    {
        this.characters = characters;
        NotebookDataPackage notebookDataPackage = data.GetData<NotebookDataPackage>();
        characterNotes = notebookDataPackage.characterNotes;
        personalNotes = notebookDataPackage.personalNotes;
    }
    
    public NetworkPackage CreatePackage()
    {
        return NetworkPackage.CreatePackage(this);
    }
    
    public NotebookData ConvertToNotebookData()
    {
        Dictionary<CharacterInstance, NotebookPage> pages =
            new Dictionary<CharacterInstance, NotebookPage>();
        
        foreach (var keyValuePair in characterNotes)
        {
            CharacterInstance instance =
                characters.Find(cc => cc.id == keyValuePair.Key);
            pages.Add(instance, new NotebookPage(instance));
            pages[instance].SetNotes(keyValuePair.Value);
        }
        
        return new NotebookData(pages, personalNotes);
    }
}
