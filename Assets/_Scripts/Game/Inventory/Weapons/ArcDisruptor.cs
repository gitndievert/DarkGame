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
using GAP_LaserSystem;
using UnityEngine;

public class ArcDisruptor : FiringWeapon
{
    public override WeaponType WeaponType => WeaponType.ArcDisruptor;
    public override AmmoType AmmoType => AmmoType.IonizedOrbs;

    public GameObject LaserBeamPrefab;

    private LaserScript _beam;    
  
    protected override void Start()
    {
        base.Start();
        _beam = LaserBeamPrefab.GetComponent<LaserScript>();
        //Start with beam being disabled
        LaserBeamPrefab.SetActive(false);
        _beam.endPoint = Camera.main.gameObject;
    }

    protected override void Update()
    {
        base.Update();
        if(Input.GetMouseButtonDown(LEFT_MOUSE))
        {
            _beam.EnableLaser();
            SoundManager.PlaySoundOnLoop(PrimaryFireSound, 2);
        }
        if(Input.GetMouseButton(LEFT_MOUSE))
        {
            _beam.UpdateLaser();
            CamShake.Instance.Shake(ShakeIntensity, ShakeDuration);
        }
        if(Input.GetMouseButtonUp(LEFT_MOUSE))
        {
            _beam.DisableLaserCaller(_beam.disableDelay);
            SoundManager.StopAllSound(2);
        }
    }

    public override void PrimaryAttack()
    {
        
    }

    public override void SecondaryAttack()
    {
        
    }
}
