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

using Dark.Utility.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigun : FiringWeapon
{
    public override WeaponType WeaponType => WeaponType.Minigun;

    public override AmmoType AmmoType => AmmoType.Bullet;    

    private bool _soundPlaying;

    //Need to move all firing logic to weapons, where it should be anyways

    //So, need to have mouse button do continuous fire and record that as such on weapon
    //Need audio if able to, loop during fire and stop when firing is done
    protected override void Start()
    {
        base.Start();        
    }

    protected override void Update()
    {
        base.Update();
        if(_soundPlaying && !IsFiring)
        {
            _soundPlaying = false;
            StopSound();            
        }
    }
    
    public override void PlayPrimaryFireSound()
    {
        if (IsFiring && !_soundPlaying)
        {
            SoundManager.PlaySoundOnLoop(PrimaryFireSound, 2);
            _soundPlaying = true;
        }
    }

    public void StopSound()
    {
        _soundPlaying = false;
        SoundManager.StopAllSound(2);
    }    


}
