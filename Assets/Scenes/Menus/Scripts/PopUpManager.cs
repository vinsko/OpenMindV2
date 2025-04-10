using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    public  Canvas          popUpCanvas;
    public  GameObject      popUpButtonCanvas;
    public  TextMeshProUGUI popUpTitleText;
    public  TextMeshProUGUI popUpText;
    public  Image           background;
    private DateTime        startTime;
    private bool            closeOnReceivedNotebook;

    public void OpenPopUp(Component sender, params object[] data)
    {
        // set popup text
        string text = "no text found.";
        if (data[0] is string) 
            text = (string)data[0];
        popUpText.text = text;
        
        // set background color and opacity
        Color color = new Color(0,0,0, 0.9f);
        background.GetComponentInChildren<Image>().color = color;
        
        // set popup title
        popUpTitleText.text = (string)data[1];
        
        // set popup type
        if ((bool)data[2])
        {
            // popup with button
            closeOnReceivedNotebook = false;
            popUpButtonCanvas.SetActive(true);
            startTime = DateTime.Now;
        }
        else
        {
            // popup without button, closes when another notebook has been received
            closeOnReceivedNotebook = true;
            popUpButtonCanvas.SetActive(false);
        }
        
        popUpCanvas.enabled = true;
    }

    public void ClosePopUp() 
    {
        // make sure the player doesn't accidentally click the popup away before reading it.
        if (!closeOnReceivedNotebook && DateTime.Now.Subtract(startTime).Seconds >= 1)
        {
            popUpText.text = string.Empty;
            popUpCanvas.enabled = false;
            popUpButtonCanvas.SetActive(false);
        }
    }

    public void Update()
    {
        if (closeOnReceivedNotebook && MultiplayerManager.mm.playerReceivedNotebook)
        {
            popUpText.text = string.Empty;
            popUpCanvas.enabled = false;
            popUpButtonCanvas.SetActive(false);
        }
    }
}
