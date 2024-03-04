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

using Dark.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : DSingle<UIManager>
{   
    public GameObject HUDCanvas;
    public GameObject MainMenu;

    [Header("Game GUI")]
    public TMP_Text HealthLabel;
    public TMP_Text ArmorLabel;
    public TMP_Text AmmoLabel;    
    public TMP_Text PowerupLabel;
    public TMP_Text LevelNameLabel;
    public TMP_Text SecretText;

    [Header("Menu Panels")]
    public GameObject MainOptionsPanel;
    public GameObject StartGameOptionsPanel;
    public GameObject GamePanel;
    public GameObject ControlsPanel;
    public GameObject GfxPanel;
    public GameObject LoadGamePanel;

    public ScreenEffects ScreenEffects { get; private set; }  

    protected override void DAwake()
    {
        if (HUDCanvas == null)
            throw new System.Exception("Missing canvas in this scene");

        HUDCanvas.SetActive(true);
        MainMenu.SetActive(false);
    }

    private void Start()
    {
        ScreenEffects = GetComponent<ScreenEffects>();             
        var labels = HUDCanvas.GetComponentsInChildren<TMP_Text>();
        if (labels.Length > 0)
        {
            foreach (TMP_Text label in labels)
            {
                switch (label.name)
                {
                    case "Health":
                        HealthLabel = label;
                        break;
                    case "Armor":
                        ArmorLabel = label;
                        break;
                    case "Ammo":
                        AmmoLabel = label;
                        break;
                    case "Powerup":
                        PowerupLabel = label;
                        break;
                    case "LevelName":
                        LevelNameLabel = label;
                        break;
                    case "SecretText":
                        SecretText = label;
                        break;
                }
            }
        }        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))        
        {
            bool menuactive = !MainMenu.activeSelf;
            DeActivateMainMenu(menuactive);
        }
    }

    public void CloseMainMenu()
    {
        DeActivateMainMenu(false);
        DeActivateAllSubMenus();
    }

    public void CloseOptionsMenu()
    {
        DeActivateAllSubMenus();
        StartGameOptionsPanel.SetActive(true);
        MainOptionsPanel.SetActive(false);
    }    

    public void ActivateMenu(string menuName)
    {
        DeActivateAllSubMenus();
        switch (menuName.ToLower())
        {
            case "mainoptionpanel":
                MainOptionsPanel.SetActive(true);
                break;
            case "startgameoptionspanel":
                StartGameOptionsPanel.SetActive(true);
                break;
            case "gamepanel":
                GamePanel.SetActive(true);
                break;
            case "controlspanel":
                ControlsPanel.SetActive(true);
                break;
            case "gfxpanel":
                GfxPanel.SetActive(true);
                break;
            case "loadgamepanel":
                LoadGamePanel.SetActive(true);
                break;

        }
    }

    private void DeActivateMainMenu(bool menuactive)
    {
        MainMenu.SetActive(menuactive);
        StartGameOptionsPanel.SetActive(menuactive);
        HUDCanvas.SetActive(!menuactive);
        if (menuactive)
        {
            PauseGame();            
        }
        else
        {
            UnPauseGame();
        }
    }    

    private void DeActivateAllSubMenus()
    {
        StartGameOptionsPanel.SetActive(false);
        GamePanel.SetActive(false);
        ControlsPanel.SetActive(false);
        GfxPanel.SetActive(false);
        LoadGamePanel.SetActive(false);
    }

    public static void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        GameManager.Instance.GamePaused = true;
    }

    public static void UnPauseGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        GameManager.Instance.GamePaused = false;
    }


}
