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

public class BulletHoleGenerator : MonoBehaviour
{    
    public float maxDistance = 1000000;    
    public GameObject BulletHolePrefab;
    public GameObject BulletHoleSecondaryPrefab;
    public GameObject WallContactPrefab;
    public float FloatInfrontOfWall = 0.02f;

    [SerializeField]
    private LayerMask _ignoreLayer;
    private Camera _playerCamera;

    private void Start()
    {
        _playerCamera = Camera.main;
    }


    public void Generate(WeaponFireType fireType,WeaponType weaponType)
    {
        if (weaponType == WeaponType.SynthathBlade) return;

        Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);        
        Ray ray = _playerCamera.ScreenPointToRay(center);
        if (Physics.Raycast(ray,out RaycastHit hit, maxDistance, ~_ignoreLayer))
        {
            switch (hit.transform.tag)
            {
                case Tags.PLAYER_TAG:
                case Tags.ACTION_TAG:
                case Tags.AMMO_TAG:
                case Tags.ARMOR_TAG:
                case Tags.POWERUP_TAG:
                case Tags.WEAPON_TAG:
                case Tags.HEALTH_TAG:
                case Tags.PROJECTILE_TAG:
                    return;                    
            }
            switch(fireType)
            {
                case WeaponFireType.Primary:
                    if(BulletHolePrefab != null) 
                    {
                        GameObject hitPrefab = null;
                        if (hit.transform.CompareTag(Tags.ENEMY_TAG))
                        {
                            var enemy = hit.transform.GetComponent<Enemy>();
                            if (enemy != null && enemy.BloodEffects.Length > 0)
                            {
                                int hitIndex = Random.Range(0, enemy.BloodEffects.Length - 1);
                                hitPrefab = enemy.BloodEffects[hitIndex];
                            }
                        }
                        else
                        {
                            hitPrefab = BulletHolePrefab;
                            if (WallContactPrefab != null)
                            {                                
                                Instantiate(WallContactPrefab, hit.point + hit.normal * (FloatInfrontOfWall + 0.1f), Quaternion.LookRotation(hit.normal));
                            }
                        }

                        if (hitPrefab != null)
                        {
                            Instantiate(hitPrefab, hit.point + hit.normal * FloatInfrontOfWall, Quaternion.LookRotation(hit.normal));
                        }
                    }
                    break;
                case WeaponFireType.Secondary:
                    break;
            }           

        }
    }
}
