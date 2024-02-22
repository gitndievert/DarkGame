using System.Collections;
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

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{    
    [Header("Sound")]
    public Slider SoundVolume;    
    public TMP_Text SoundVol;
    [Space(5)]
    [Header("Music")]
    public Slider MusicVolume;    
    public TMP_Text MusicVol;    

    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        SoundVol.text = "100";
        MusicVol.text = "100";

    }

    private void Slider()
    {
        PlayerPrefs.SetFloat("SoundVolume", 100f);
        PlayerPrefs.SetFloat("MusicVolume", 100f);
        PlayerPrefs.Save();
    }

    //NEED TO PUT IN AN ABSTRACT
}

/*
 * // Save a float value
PlayerPrefs.SetFloat("Volume", volumeValue);
PlayerPrefs.SetInt("GraphicsQuality", graphicsQualityValue);

// Save a string value
PlayerPrefs.SetString("ControlScheme", controlScheme);

// Save the settings to disk
PlayerPrefs.Save();
*/

/*
 * // Load a float value
float volumeValue = PlayerPrefs.GetFloat("Volume", defaultVolume);

// Load an int value
int graphicsQualityValue = PlayerPrefs.GetInt("GraphicsQuality", defaultGraphicsQuality);

// Load a string value
string controlScheme = PlayerPrefs.GetString("ControlScheme", defaultControlScheme);
*/