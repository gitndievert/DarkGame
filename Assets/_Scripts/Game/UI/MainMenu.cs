// ********************************************************************
// CONFIDENTIAL - DO NOT DISTRIBUTE
// COPYRIGHT 2019-2024 Wacky Potato Games, LLC. All Rights Reserved.
// 
// If you send, receive, or use this file for any purpose other than
// internal use by Wacky Potato Games, it is without permission and an act of theft.
// Report any misuse of this file immediately to contact@wackypotato.com
// Misuse or failure to report misuse will subject you to legal action.
// 
// The intellectual and technical concepts contained herein are
// proprietary and are protected by trade secret and/or copyright law.
// Dissemination or reproduction of this material is forbidden.
// ********************************************************************


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{    
    public GameObject LoadMenu;
    public GameObject SaveMenu;
    public GameObject OptionsMenu;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        DeActivateAllSubMenus();
    }

    public void ActivateMenu(string menuName)
    {
        DeActivateAllSubMenus();
        switch (menuName.ToLower())
        {
            case "load":
                LoadMenu.SetActive(true);
                break;
            case "save":
                SaveMenu.SetActive(true);
                break;
            case "options":
                OptionsMenu.SetActive(true);
                break;
        }
    }

    public void CloseMenu()
    {
        DeActivateAllSubMenus();
        UIManager.Instance.UnlockGame();
        gameObject.SetActive(false);
    }


    private void DeActivateAllSubMenus()
    {
        LoadMenu.SetActive(false);
        SaveMenu.SetActive(false);
        OptionsMenu.SetActive(false);
    }

   
}
