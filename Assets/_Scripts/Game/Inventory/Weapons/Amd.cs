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

public class Amd : FiringWeapon
{
    public override WeaponType WeaponType => WeaponType.AMDD;
    public override AmmoType AmmoType => AmmoType.IonizedOrbs;

    private bool _attacking = false;

    public override void PrimaryAttack()
    {
        if(!_attacking)
            StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        _attacking = true;
        animator.Play("RaiseOrb");
        yield return new WaitForSeconds(2f);
        //base.PrimaryAttack();
        yield return new WaitForSeconds(1f);
        animator.Play("Reload");
        _attacking = false;
    }
}
