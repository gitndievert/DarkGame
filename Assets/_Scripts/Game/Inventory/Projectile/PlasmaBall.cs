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
public class PlasmaBall : BaseProjectile
{
    public override bool ApplySplashDamage => false;

    public GameObject[] Effects;
    
    //Need to figure out the particles

    protected override void Start()
    {
        base.Start();
        if (Effects.Length > 0)
        {
            foreach (var effect in Effects)
            {
                effect.SetActive(true);
            }
        }
    }



}
