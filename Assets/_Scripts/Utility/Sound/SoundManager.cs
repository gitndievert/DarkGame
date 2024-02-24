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

namespace Dark.Utility.Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : PSingle<SoundManager>
    {
        private static AudioSource _audio;
        private static AudioSource _audio2;

        public static bool IsPlaying
        {
            get { return _audio.isPlaying; }
        }

        public static float VolumeLevel { get; set; }

        private void Start()
        {
            var audioSources = GetComponents<AudioSource>();
            if (audioSources.Length != 2)
                throw new System.Exception("Missing Audio Source Components");
            _audio = audioSources[0];
            _audio2 = audioSources[1];
            _audio.playOnAwake = false;
            _audio2.playOnAwake = false;
        }      
              
        public static void PlaySound(AudioClip clip, int channel = 1)
        {
            AudioChan(channel).PlayOneShot(clip);
        }

        public static void PlaySound(AudioClip[] clips, int channel = 1)
        {
            int rand = Random.Range(0, clips.Length - 1);
            PlaySound(clips[rand], channel);
        }

        public static void PlaySoundOnGameObject(GameObject obj, AudioClip clip)
        {
            AudioSource.PlayClipAtPoint(clip, obj.transform.position);
        }

        public static void PlaySoundOnGameObject(GameObject obj, AudioClip[] clips)
        {
            int rand = Random.Range(0, clips.Length - 1);
            PlaySoundOnGameObject(obj, clips[rand]);
        }

        public static void PlaySoundWithDelay(AudioClip[] clips, float delaySec, int channel = 1)
        {
            int rand = Random.Range(0, clips.Length);
            PlaySoundWithDelay(clips[rand], delaySec, channel);
        }

        public static void PlaySoundWithDelay(AudioClip clip, float delaySec, int channel = 1)
        {
            var audio = AudioChan(channel);
            audio.clip = clip;
            audio.PlayDelayed(delaySec);
        }

        public static void PlaySoundOnLoop(AudioClip clip, int channel = 1)
        {
            var audio = AudioChan(channel);
            audio.clip = clip;
            audio.loop = true;
            audio.Play();
        }

        public static void QueueSounds(AudioClip[] clips, float delayBetween = 0.0f, int channel = 1)
        {
            Instance.Queue(clips, delayBetween);
        }

        public void Queue(AudioClip[] clips, float delay, int channel = 1)
        {
            StartCoroutine(PlayQueue(clips, delay, channel));
        }

        public static void StopAllSound()
        {            
            _audio.Stop();
            _audio2.Stop();
            _audio.loop = false;
            _audio2.loop = false;
        }

        public static void StopAllSound(int channel)
        {            
            var audio = AudioChan(channel);
            audio.Stop();
            audio.loop = false;
        }

        public static void Volume(float percent)
        {
            _audio.volume = percent;
            _audio2.volume = percent;            
            VolumeLevel = percent;
        }

        private IEnumerator PlayQueue(AudioClip[] clips, float delayBetween, int channel)
        {
            foreach (var clip in clips)
            {
                var audio = AudioChan(channel);
                audio.clip = clip;
                audio.Play();
                yield return new WaitForSeconds(audio.clip.length);
                yield return new WaitForSeconds(delayBetween);
            }
        }

        private static AudioSource AudioChan(int channel)
        {
            if (_audio == null && _audio2 == null)
                throw new System.Exception("Missing Audio Channels from Game Scene");
            switch (channel)
            {
                default:
                case 1:
                    return _audio;
                case 2:
                    return _audio2;
            }
        }
    }
}