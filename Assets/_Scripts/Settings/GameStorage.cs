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

public static class GameStorage
{    
    /// <summary>
    /// Saves value to PlayerPrefs by type
    /// </summary>
    /// <param name="storageKey">Key Saved</param>
    /// <param name="value">Value</param>
    public static void SaveValue(string storageKey, object value)
    {
        //TODO. Might make this smarter and check the keys and types and
        //what is previously stored to validate data integrity
        if(value != null)
        {
            if(value is float)
            {
                PlayerPrefs.SetFloat(storageKey, (float)value);
            }
            else if (value is int)
            {
                PlayerPrefs.SetInt(storageKey, (int)value);
            }
            else
            {
                PlayerPrefs.SetString(storageKey, (string)value);
            }

            SaveStorage();
        }
    }

    public static void SaveStorage()
    {
        PlayerPrefs.Save();
    }

    public static float GetStorageFloat(string storageKey) => PlayerPrefs.GetFloat(storageKey);
    public static int GetStorageInt(string storageKey) => PlayerPrefs.GetInt(storageKey);
    public static string GetStorageString(string storageKey) => PlayerPrefs.GetString(storageKey);
    public static bool CheckExistingKey(string storageKey) => PlayerPrefs.HasKey(storageKey);    

}

public static class GameSettingsStorage
{
    public const string HudActive = "HudActive";
}


public static class VisualStorage
{
    public const string GraphicsQuality = "GraphicsQuality";
    public const string Brightness = "Brightness";
    public const string Contrast = "Contrast";
}

public static class AudioStorage
{
    public const string SoundVol = "SoundVolume";
    public const string MusicVol = "MusicVolume";
}

public static class MouseStorage
{
    public const string MouseSpeed = "MouseSpeed";    
}

public static class KeyBinds
{    
    public const string KeyForward = "KeyForward";


}
