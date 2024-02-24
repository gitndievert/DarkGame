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
using UnityEngine;

public class AcidShot : FiringWeapon
{
    public override WeaponType WeaponType => WeaponType.AcidShot;

    public override AmmoType AmmoType => AmmoType.Acid;

    public GameObject AcidStreamPrefab;    

    /*protected override void Update()
    {
        base.Update();
        GameObject acid = null;
        if (Input.GetMouseButtonDown(LEFT_MOUSE))
        {
            //SoundManager.PlaySoundOnLoop(PrimaryFireSound, 2);
            acid = Instantiate(AcidStreamPrefab, MuzzelSpawn.transform.position, transform.rotation);
        }
        if (Input.GetMouseButton(LEFT_MOUSE))
        {
            //AcidStreamPrefab.SetActive(true);
            CamShake.Instance.Shake(ShakeIntensity, ShakeDuration);
        }
        if (Input.GetMouseButtonUp(LEFT_MOUSE))
        {
            if(acid != null) 
            {
                Destroy(acid);
            }
            //AcidStreamPrefab.SetActive(false);
            //SoundManager.StopAllSound(2);
        }
    }*/

    public override void PrimaryAttack()
    {
        Instantiate(AcidStreamPrefab, MuzzelSpawn.transform.position, transform.rotation);
    }

    public override void SecondaryAttack()
    {
        
    }

}
