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

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Settings Controls")]
    public Slider SoundVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider ContrastSlider;
    public Slider BrightnessSlider;
    [Header("Settings Labels")]
    public TMP_Text HudToggleLabel;

    private void Awake()
    {
        SoundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
        MusicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        ContrastSlider.onValueChanged.AddListener(SetContrast);
        BrightnessSlider.onValueChanged.AddListener(SetBrightness);
    }


    private void Start()
    {
        /*if (!GameStorage.CheckExistingKey(VisualStorage.Brightness))
        {
            SetBrightness(1f); //change value later
        }
        if (!GameStorage.CheckExistingKey(AudioStorage.MusicVol))
        {
            SetContrast(1f); //change value later
        }*/
        //Load preset values
        if (GameStorage.CheckExistingKey(AudioStorage.SoundVol))
        {
            float savedVol = GameStorage.GetStorageFloat(AudioStorage.SoundVol);
            SoundVolumeSlider.value = savedVol;
        }
        //Preset Music Value
        if (GameStorage.CheckExistingKey(AudioStorage.MusicVol))
        {
            float savedVol = GameStorage.GetStorageFloat(AudioStorage.MusicVol);
            MusicVolumeSlider.value = savedVol;
        }
        /*if(GameStorage.CheckExistingKey(GameSettingsStorage.HudActive))
        {
            HudToggleLabel.text = GameStorage.GetStorageInt(GameSettingsStorage.HudActive) == 1 ? "On" : "Off";
        }
        else
        {            
            GameStorage.SaveValue(GameSettingsStorage.HudActive, 1);
            HudToggleLabel.text = "On";
        }*/
    }

    public void SetSoundVolume(float percentage)
    {
        SoundManager.Volume(percentage);
        GameStorage.SaveValue(AudioStorage.SoundVol, percentage);        
    }
    public void SetMusicVolume(float percentage)
    {
        MusicManager.Volume(percentage);
        GameStorage.SaveValue(AudioStorage.MusicVol, percentage);        
    }
    public void SetBrightness(float percentage)
    {
        GameStorage.SaveValue(VisualStorage.Brightness, percentage);
    }
    public void SetContrast(float percentage)
    {
        GameStorage.SaveValue(VisualStorage.Contrast, percentage);
    }
    /*public void SetGameHud(bool hudStatus)
    {
        UIManager.Instance.ToggleHud(hudStatus);        
        GameStorage.SaveValue(GameSettingsStorage.HudActive, Convert.ToInt32(UIManager.Instance.HUDCanvas.activeSelf));
    }*/
    

}
