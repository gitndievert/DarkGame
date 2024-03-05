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

public class SettingsController : MonoBehaviour
{
    [Header("Settings Controls")]
    public Slider SoundVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider ContrastSlider;
    public Slider BrightnessSlider;

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
    

}
