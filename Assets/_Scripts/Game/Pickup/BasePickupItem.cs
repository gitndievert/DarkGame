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
using System;
using UnityEngine;

[RequireComponent(typeof(Bobber))]
[RequireComponent(typeof(BoxCollider))]
abstract public class BasePickupItem<T> : BaseEntity, IPickupable where T : Enum
{    
    public bool EnableBobber = true;
    public int Amount = 1;
    public T PickupType;
    public AudioClip PickupSound;

    protected Bobber bobber;
    protected BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;        
    }
    protected override void Start()
    {
        base.Start();
        SetTag();
        if (EnableBobber)
        {
            bobber = GetComponent<Bobber>();
            if (!bobber.AutoStartBob)
            {
                bobber.AutoStartBob = true;
            }
        }
    }   

    public void Consume()
    {        
        PlayPickupSound();        
        Destroy(gameObject);
    }

    public void PlayPickupSound()
    {
        if (PickupSound != null)
        {
            UIManager.Instance.ScreenEffects.Flash();
            SoundManager.PlaySound(PickupSound);
        }
    }

    protected void SetTag()
    {
        Type pickup = typeof(T);        
        switch(pickup.FullName)
        {
            case "AmmoType":
                gameObject.tag = Tags.AMMO_TAG;
                break;
            case "WeaponType":
                gameObject.tag = Tags.WEAPON_TAG;
                break;
            case "ArmorType":
                gameObject.tag = Tags.ARMOR_TAG;
                break;
            case "HealthType":
                gameObject.tag = Tags.HEALTH_TAG;
                break;
            case "PowerUpType":
                gameObject.tag = Tags.POWERUP_TAG;
                break;
        }
    }


}

