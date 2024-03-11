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

using PampelGames.GoreSimulator;
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

    public void Generate(WeaponFireType fireType,WeaponType weaponType, float spreadDistance = 0f)
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
                        bool isBlood = false;
                        bool hasSpread = spreadDistance > 0;

                        Vector3[] randomOffset = new Vector3[6];
                        if (hasSpread)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                randomOffset[i] = RandomOffset(spreadDistance);
                            }
                        }

                        if (hit.transform.root.CompareTag(Tags.ENEMY_TAG))
                        {
                            if (hit.collider.TryGetComponent<IGoreObject>(out var goreObject))
                            {
                                var enemy = hit.collider.GetComponentInParent<Enemy>();
                                if (enemy != null && enemy.BloodEffects.Length > 0)
                                {
                                    int hitIndex = Random.Range(0, enemy.BloodEffects.Length - 1);
                                    hitPrefab = enemy.BloodEffects[hitIndex];
                                    isBlood = true;
                                }                                
                            }
                        }
                        else
                        {
                            hitPrefab = BulletHolePrefab;
                            if (WallContactPrefab != null)
                            {   
                                Instantiate(WallContactPrefab, hit.point + hit.normal * (FloatInfrontOfWall + 0.1f), Quaternion.LookRotation(hit.normal));
                                if(hasSpread)
                                {
                                    foreach (Vector3 offset in randomOffset)
                                    {
                                        Instantiate(WallContactPrefab, hit.point + hit.normal * (FloatInfrontOfWall + 0.1f) + offset, Quaternion.LookRotation(hit.normal));
                                    }
                                }
                            }
                        }

                        if (hitPrefab != null)
                        {
                            if(isBlood)
                            {                                
                                var direction = hit.normal;
                                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 180;
                                var hitEffect = Instantiate(hitPrefab, hit.point, Quaternion.Euler(0, angle + 90, 0));  
                                if (Physics.Raycast(new Vector3(10, 100, 10), Vector3.down, out RaycastHit gHit, 200f))
                                {
                                    float groundHeight = gHit.point.y;
                                    hitEffect.GetComponent<BFX_BloodSettings>().AnimationSpeed = 10f;
                                    hitEffect.GetComponent<BFX_BloodSettings>().GroundHeight = groundHeight;
                                }
                                if(hasSpread)
                                {
                                    foreach(Vector3 offset in randomOffset)
                                    {
                                        var rHitEffect = Instantiate(hitPrefab, hit.point + offset, Quaternion.Euler(0, angle + 90, 0));
                                        if (Physics.Raycast(new Vector3(10, 100, 10), Vector3.down, out RaycastHit rgHit, 200f))
                                        {
                                            float groundHeight = rgHit.point.y;
                                            hitEffect.GetComponent<BFX_BloodSettings>().AnimationSpeed = 10f;
                                            rHitEffect.GetComponent<BFX_BloodSettings>().GroundHeight = groundHeight;
                                        }
                                    }
                                }

                            }
                            else
                            {                                
                                var hitEffect = Instantiate(hitPrefab, hit.point + hit.normal * FloatInfrontOfWall, Quaternion.LookRotation(hit.normal));
                                Destroy(hitEffect, 1f);
                                foreach(Vector3 offset in randomOffset)
                                {
                                    var rHitEffect = Instantiate(hitPrefab, hit.point + hit.normal * FloatInfrontOfWall + offset, Quaternion.LookRotation(hit.normal));
                                    Destroy(rHitEffect, 1f);
                                }
                            }                            
                        }
                    }
                    break;
                case WeaponFireType.Secondary:
                    break;
            }           

        }
    }

    private Vector3 RandomOffset(float spreadDistance)
    {
        return new Vector3(Random.Range(-spreadDistance, spreadDistance),
                                Random.Range(-spreadDistance, spreadDistance),
                                Random.Range(-spreadDistance, spreadDistance));
    }
}
