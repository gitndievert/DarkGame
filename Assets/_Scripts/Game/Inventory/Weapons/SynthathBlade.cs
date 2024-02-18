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

public class SynthathBlade : Weapon
{
    public override WeaponType WeaponType => WeaponType.SynthathBlade;
    public override AmmoType AmmoType => AmmoType.None;

    private int _maxSwings = 3;
    private int _trackSwings = 1;

    public override void PrimaryAttack()
    {        
        PlayPrimaryFireSound();
        Swing();
        CamShake.Instance.Shake(ShakeIntensity, ShakeDuration);
    }

    public override void SecondaryAttack()
    {
        PlaySecondaryFireSound();
    }

    public void Swing()
    {
        string swingAnim = $"Swing{_trackSwings}";
        animator.Play(swingAnim);
        _trackSwings++;
        if (_trackSwings > _maxSwings)
        {
            _trackSwings = 1;
        }
    }
}
