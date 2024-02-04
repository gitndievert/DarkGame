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
using Dark.Utility.Sound;

public enum WeaponType
{    
    None,
    SynthathBlade = 0,
    Pistol,
    Shotgun,
    Minigun,
    Slicer,
    Shattercaster,
    ArcDisruptor,
    DarkReaper,
    AMDD    
}

public enum WeaponFireType
{
    Primary,
    Secondary
}

/*
Fists
Synthath Blade (basic fast beatdown blade)
Pistol(single shots)(Bullets)
DB Shotgun(Shells)
Minigun(Bullets)
Slicer(Blades)
Shattercaster(rockets)
Arc Disruptor(laser) (Orbs)
Dark Reaper(rockets)
A.M.D.D. (Orbs) Astral Matrix Disintegration Device
*/

abstract public class Weapon : BaseEntity
{
    public float RoundDelay = 3f;
    public int StartingAmmo = 30;
    [HideInInspector]
    public bool FoundPickup = false;
    [HideInInspector]
    public float AttackDistance;

    public abstract WeaponType WeaponType { get; }
    public abstract AmmoType AmmoType { get; }
    
    public Sprite AmmoIcon;    
        
    public AudioClip PrimaryFireSound;
    public AudioClip SecondaryFireSound;    

    [Header("Damage Properties")]
    public int PrimaryFireDamage = 10;
    public int SecondaryFireDamage = 0;

    public float RecoilForce = 0.2f;
    public float RecoilSmoothness = 6f;

    public float SwayAmount = 0.02f;
    public float SwaySmoothness = 4f;

    protected Vector3 initialPosition;
    


    protected override void Start()
    {
        AttackDistance = 15f;
        initialPosition = transform.localPosition;
    }

    private void Update()
    {
        // Check if the player is moving        
        // Get input for horizontal and vertical movement
        float moveX = -Input.GetAxis("Horizontal");
        float moveY = -Input.GetAxis("Vertical");       

        if (moveX != 0 || moveY != 0)
        {
            float calcSway = GameManager.Instance.MyPlayer.PlayerController.IsRunning ? SwayAmount * 2 : SwayAmount;
            float offsetX = Mathf.Sin(Time.time * SwaySmoothness) * calcSway * moveX;
            float offsetY = Mathf.Sin(Time.time * SwaySmoothness) * calcSway * moveY;
            offsetX = offsetY != 0 ? offsetY : offsetX;            
            Vector3 targetPosition = initialPosition + new Vector3(offsetX, 0f, 0f);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10f);
        }
        else
        {
            //transform.localPosition = _initialPosition;
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, Time.deltaTime * RecoilSmoothness);
        }

        //transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition, Time.deltaTime * RecoilSmoothness);

    }
       
    private void OnEnable()
    {
        Debug.Log($"Switched to {Name}");
    }

    public abstract void PrimaryAttack();   
    public abstract void SecondaryAttack();

    public void PlayPrimaryFireSound()
    {
        SoundManager.PlaySound(PrimaryFireSound);
    }

    public void PlaySecondaryFireSound()
    {
        SoundManager.PlaySound(SecondaryFireSound);
    }

}