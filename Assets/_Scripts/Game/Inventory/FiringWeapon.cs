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

[RequireComponent(typeof(BulletHoleGenerator))]
abstract public class FiringWeapon : Weapon
{
    public bool DisplayCrossHairs = true;

    public GameObject[] MuzzelFlashPrefabs;
    public Transform MuzzelSpawn;    
    
    public AudioClip EmptyClipSound;

    public GameObject Projectile = null;
    public bool UsesProjectile
    {
        get { return Projectile != null; }
    }

    public float GunShakeDuration = 0.2f;
    public float GunShakeIntensity = 8f;

    private float _crosshairSize = 10f;
    private BulletHoleGenerator _bulletHoleGenerator;

    protected override void Start()
    {
        base.Start();
        AttackDistance = 1000000;
        _bulletHoleGenerator = GetComponent<BulletHoleGenerator>();
    }

    public override void PrimaryAttack()
    {
        PlayPrimaryFireSound();
        DoMuzzleFlash();
        Recoil();
        _bulletHoleGenerator.Generate(WeaponFireType.Primary, WeaponType);
    }

    public override void SecondaryAttack()
    {
        PlaySecondaryFireSound();
        DoMuzzleFlash();
        Recoil();
        _bulletHoleGenerator.Generate(WeaponFireType.Secondary, WeaponType);
    } 

    public void PlayEmptyClipSound()
    {
        SoundManager.PlaySound(EmptyClipSound);
    }

    private void DoMuzzleFlash()
    {
        if (MuzzelFlashPrefabs.Length == 0 && MuzzelSpawn != null) return;
        int randomIndex = Random.Range(0, MuzzelFlashPrefabs.Length - 1);
        var selectedMuzzleFlashPrefab = MuzzelFlashPrefabs[randomIndex];
        var flash = Instantiate(selectedMuzzleFlashPrefab, MuzzelSpawn.position, MuzzelSpawn.rotation);
        flash.transform.parent = transform.parent;
    }

    private void Recoil()
    {
        transform.localPosition -= Vector3.forward * RecoilForce;
        transform.localPosition = new Vector3(initialPosition.x, initialPosition.y, Mathf.Max(transform.localPosition.z, initialPosition.z - RecoilForce));
    }

    private void BulletHole()
    {
        //var pSpotTransform = GameManager.Instance.MyPlayer.ProjectileSpot.transform;
        //Instantiate(_bulletHoleGenerator.BulletHolePrefab, pSpotTransform.position, pSpotTransform.rotation);        
    }

    private void OnGUI()
    {
        if (DisplayCrossHairs)
        {            
            Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);            
            GUI.DrawTexture(new Rect(center.x - (_crosshairSize / 2), center.y - 1, _crosshairSize, 2), Texture2D.grayTexture);            
            GUI.DrawTexture(new Rect(center.x - 1, center.y - (_crosshairSize / 2), 2, _crosshairSize), Texture2D.grayTexture);
        }
    }
}
