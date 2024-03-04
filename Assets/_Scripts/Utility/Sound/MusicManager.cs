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
using Dark.Utility;
using System.Collections;
using UnityEditor;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : PSingle<MusicManager>
{
    //[MenuItem("EMM/Add/Main Menu Canvas  &#M", false)]
    public AudioClip[] MusicTracks;
    public float MusicFadeDuration = 1.0f;
    public float VolumeLevel { get; set; }

    private AudioSource _audioSource;

    // Start is called before the first frame update
    protected override void PAwake()
    {
        base.PAwake();
        _audioSource = GetComponent<AudioSource>();         
    }

    private void Start()
    {
        if (GameStorage.CheckExistingKey(AudioStorage.MusicVol))
        {
            Volume(GameStorage.GetStorageFloat(AudioStorage.MusicVol));
        }
        if (SceneSwapper.Instance.LoadedMap != null && SceneSwapper.Instance.LoadedMap.MapMusic != null)
        {
            StartMusic(SceneSwapper.Instance.LoadedMap.MapMusic);
        }
    }

    public void StartMusic(int trackIndex, bool loopmusic = true)
    {        
        StartCoroutine(FadeMusic(trackIndex, loopmusic));
    }

    public void StartMusic(AudioClip track, bool loopmusic = true)
    {        
        StartCoroutine(FadeMusic(track, loopmusic));
    }

    public void Volume(float percent)
    {
        percent = Mathf.Clamp01(percent);
        _audioSource.volume = percent;
        VolumeLevel = percent;
    }

    private IEnumerator FadeMusic(int trackIndex, bool loopmusic)
    {        
        float startVolume = _audioSource.volume;        
        
        while (_audioSource.volume > 0)
        {
            _audioSource.volume -= startVolume * Time.deltaTime / MusicFadeDuration;
            yield return null;
        }
                
        _audioSource.clip = MusicTracks[trackIndex];
        _audioSource.loop = loopmusic;
        _audioSource.Play();
        
        while (_audioSource.volume < startVolume)
        {
            _audioSource.volume += startVolume * Time.deltaTime / MusicFadeDuration;
            yield return null;
        }
    }

    private IEnumerator FadeMusic(AudioClip track, bool loopmusic)
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

    public void StopMusic()
    {
        _audioSource.Stop();
    }    
}
