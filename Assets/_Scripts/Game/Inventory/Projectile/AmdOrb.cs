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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmdOrb : BaseProjectile
{
    const float STOP_ORB_TIMER = 3f;

    public override bool ApplySplashDamage => true;

    private bool _stopOrb;

    protected override void Start()
    {
        base.Start();
        _stopOrb = false;
        StartCoroutine(StopAndExplodeOrb());
    }

    protected override void Update()
    {
        if (_stopOrb)
        {
            rb.isKinematic = true;            
        }
        else
        {
            transform.Translate(Speed * Time.deltaTime * _direction);
        }
    }

    private IEnumerator StopAndExplodeOrb()
    {
        yield return new WaitForSeconds(STOP_ORB_TIMER);
        _stopOrb = true;
        Explode();
    }


    protected override void Explode()
    {
        //base.Explode();
        //This is the crazy explode things for the AMDD

    }




}
