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
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    //[MenuItem("EMM/Add/Main Menu Canvas  &#M", false)]    
    public static float MusicFadeDuration = 1.0f;    

    private static AudioSource _audioSource;
    private static MusicManager _instance;
    
    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MusicManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(MusicManager).Name;
                    _instance = obj.AddComponent<MusicManager>();
                }
            }
            return _instance;
        }
    }

    // Start is called before the first frame update
    private void Awake()
    {        
        _audioSource = GetComponent<AudioSource>();        
    }

    private void Start()
    {
        //Preset Music Value
        if (GameStorage.CheckExistingKey(AudioStorage.MusicVol))
        {
            float savedVol = GameStorage.GetStorageFloat(AudioStorage.MusicVol);
            UIManager.Instance.SettingsController.MusicVolumeSlider.value = savedVol;
        }

        if (SceneSwapper.Instance.LoadedMap != null && SceneSwapper.Instance.LoadedMap.MapMusic != null)
        {
            StartMusic(SceneSwapper.Instance.LoadedMap.MapMusic);
        }
        else if(_audioSource.clip != null)
        {
            StartMusic(_audioSource.clip);
        }
    }    
   
    public static void StartMusic(AudioClip track, bool loopmusic = true)
    {        
        Instance.StartCoroutine(FadeMusic(track, loopmusic));
    }

    public static void Volume(float percent)
    {
        percent = Mathf.Clamp01(percent);
        _audioSource.volume = percent;
    }    

    private static IEnumerator FadeMusic(AudioClip track, bool loopmusic)
    {
        float startVolume = _audioSource.volume;
        
        while (_audioSource.volume > 0)
        {
            _audioSource.volume -= startVolume * Time.deltaTime / MusicFadeDuration;
            yield return null;
        }
                
        _audioSource.clip = track;
        _audioSource.loop = loopmusic;
        _audioSource.Play();
        
        while (_audioSource.volume < startVolume)
        {
            _audioSource.volume += startVolume * Time.deltaTime / MusicFadeDuration;
            yield return null;
        }
    }

    public static void StopMusic()
    {
        _audioSource.Stop();
    }    
}
