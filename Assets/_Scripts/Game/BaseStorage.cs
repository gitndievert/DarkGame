using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BaseStorage : MonoBehaviour
{
    
    public class VisualStorage
    {
        public const string GraphicsQuality = "GraphicsQuality";
        public const string Gamma = "Gamma";
    }

    public class AudioStorage
    {
        public const string SoundVol = "SoundVolume";
        public const string MusicVol = "MusicVolume";
    }

    protected void SaveToStorage(string storageKey, object value)
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

            PlayerPrefs.Save();
        }
    }

    protected float GetStorageFloat(string storageKey) => PlayerPrefs.GetFloat(storageKey);
    protected int GetStorageInt(string storageKey) => PlayerPrefs.GetInt(storageKey);
    protected string GetStorageString(string storageKey) => PlayerPrefs.GetString(storageKey);

    

    
    


}