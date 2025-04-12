// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCSelectOverview : MonoBehaviour
{
    [Header("Color settings")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color inactiveColor;

    [Header("References")]
    [SerializeField] private NPCSelectScroller scroller;

    [Header("Prefabs")]
    [SerializeField] private GameObject iconPrefab;

    // GameManager instance for easy access
    private GameManager gm = GameManager.gm;
    private List<CharacterIcon> icons = new();
    private int selectedCharacter = -1; // Set to -1, the code will set a correct value later

    void Start()
    {
        // Get character spaces
        foreach (var child in scroller.Children)
        {
            var character = child.GetComponentInChildren<SelectOption>().character;
            
            // Instantiate new character icon
            var iconInstantiation = Instantiate(iconPrefab, transform);
            var background = iconInstantiation.GetComponent<Image>();

            // Set appropriate background color
            background.color = character.isActive ?
                defaultColor : inactiveColor;

            // Add to list of icons
            var icon = new CharacterIcon(iconInstantiation, background, character);
            icons.Add(icon);
        }

        scroller.OnCharacterSelected.AddListener(SelectCharacter);
    }

    /// <summary>
    /// What to do when a new character is selected. 
    /// Should be a listener of <see cref="NPCSelectScroller.OnCharacterSelected"/>.
    /// </summary>
    private void SelectCharacter()
    {
        // If there is no previously selected character, skip this
        if (selectedCharacter >= 0)
        {
            var prevIcon = icons[selectedCharacter];
            prevIcon.background.color = prevIcon.character.isActive ?
                defaultColor : inactiveColor;
        }

        selectedCharacter = scroller.SelectedChild;
        var icon = icons[selectedCharacter];
        icon.background.color = selectedColor;   
    }

    /// <summary>
    /// Struct to hold data for character icons.
    /// Contains a reference to the object, the background/mask image, and the icon's character.
    /// </summary>
    private struct CharacterIcon
    {
        public CharacterIcon(GameObject gameObject, Image background, CharacterInstance character)
        {
            this.gameObject = gameObject;
            this.background = background;
            this.character = character;
        }

        public GameObject gameObject;
        public Image background;
        public CharacterInstance character;
    }
}

