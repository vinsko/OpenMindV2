using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDOLManager : MonoBehaviour
{
    public static DDOLManager i;

    private void Awake()
    {
        if (i != null)
        {
            Destroy(gameObject);
        }
        else
        {
            i = this;

            // Make this group-object DDOL, and therefore also its children become DDOL
            DontDestroyOnLoad(this);

            // Set all its children to be active
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}
