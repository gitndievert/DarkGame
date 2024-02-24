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
using Unity.VisualScripting;
using Dark.Utility.Sound;

public class OptionsMenu : MonoBehaviour
{    
    [Header("Sound")]
    public Slider SoundVolume;    
    public TMP_Text SoundVol;
    [Space(5)]
    [Header("Music")]
    public Slider MusicVolume;    
    public TMP_Text MusicVol;

    private void Awake()
    {
        float soundSave = 1f;
        float musicSave = 1f;
        if (GameStorage.CheckExistingKey(AudioStorage.SoundVol))
        {
            soundSave = GameStorage.GetStorageFloat(AudioStorage.SoundVol);
        }        
        SoundVolume.value = soundSave;        
        if(GameStorage.CheckExistingKey(AudioStorage.MusicVol))
        {
            musicSave = GameStorage.GetStorageFloat(AudioStorage.MusicVol);
        }            
        MusicVolume.value = musicSave;
    }

    private void Start()
    {
        SoundVolume.onValueChanged.AddListener(SoundSliderChange);
        MusicVolume.onValueChanged.AddListener(MusicSliderChange);
    }

    // Update is called once per frame
    private void Update()
    {
        int sVol = (int)(SoundVolume.value * 100);
        int mVol = (int)(MusicVolume.value * 100);
        SoundVol.text = sVol.ToString();
        MusicVol.text = mVol.ToString();
    }

    private void SoundSliderChange(float change)
    {
        GameStorage.SaveToStorage(AudioStorage.SoundVol, change);
        SoundVolume.value = change;
        SoundManager.Volume(change);
    }

    private void MusicSliderChange(float change)
    {
        GameStorage.SaveToStorage(AudioStorage.MusicVol, change);
        MusicVolume.value = change;
        MusicManager.Instance.Volume(change);
    }
    
}

