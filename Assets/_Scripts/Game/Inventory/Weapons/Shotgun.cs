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

public class Shotgun : FiringWeapon
{
    const float SPREAD_DISTANCE = 3f;
    public override WeaponType WeaponType => WeaponType.Shotgun;
    public override AmmoType AmmoType => AmmoType.Shell;

    //Need to add split attack here
    //NOTE: All animations here need to return to idle

    public override void PrimaryAttack()
    {
        PlayPrimaryFireSound();
        DoMuzzleFlash();
        if (!DisableDefaultFireAnimation) Recoil();
        CamShake.Instance.Shake(ShakeIntensity, ShakeDuration);
        bulletHoleGenerator.Generate(WeaponFireType.Primary, WeaponType, SPREAD_DISTANCE);
        animator.Play("Cock");
    }

}
