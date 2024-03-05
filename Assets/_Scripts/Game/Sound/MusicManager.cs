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
using Dark.Utility;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : PSingle<MusicManager>
{
    //[MenuItem("EMM/Add/Main Menu Canvas  &#M", false)]    
    public static float MusicFadeDuration = 1.0f;    

    private static AudioSource _audioSource;
    

    // Start is called before the first frame update
    protected override void PAwake()
    {        
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
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
