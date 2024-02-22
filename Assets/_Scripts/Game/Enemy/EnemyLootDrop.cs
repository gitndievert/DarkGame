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

public class EnemyLootDrop : MonoBehaviour
{
    public GameObject[] AmmoTypes;
    public GameObject[] HealthTypes;

    [SerializeField]
    private float _lootDropChance = 0.4f;

    public void DropRandom()
    {              
        float rand = UnityEngine.Random.value;
        if(rand <= _lootDropChance)
        {
            DropItemAmmo(AmmoType.Bullet);
        }
        else
        {
            //Something here, come back
        }
    }

    public void DropItemAmmo(AmmoType type) 
    {
        foreach(var item in AmmoTypes)
        {   
            if (item.GetComponent<PickupAmmo>().PickupType == type)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * 2f;
                var spawn = Instantiate(item, spawnPosition, Quaternion.identity);
                spawn.TryGetComponent<Rigidbody>(out var rb);
                if (rb != null)
                {
                    Vector3 tossDirection = Vector3.back;
                    rb.AddForce(tossDirection * 1f, ForceMode.Impulse);
                }
                return;
            }
        }
    }

    public void DropItemHealth(HealthType type)
    {
        foreach (var item in HealthTypes)
        {            
            if (item.GetComponent<PickupHealth>().PickupType == type)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * 2f;
                var spawn = Instantiate(item, spawnPosition, Quaternion.identity);
                spawn.TryGetComponent<Rigidbody>(out var rb);
                if (rb != null)
                {
                    Vector3 tossDirection = Vector3.back;
                    rb.AddForce(tossDirection * 1f, ForceMode.Impulse);
                }
                return;
            }
        }
    }

    

}