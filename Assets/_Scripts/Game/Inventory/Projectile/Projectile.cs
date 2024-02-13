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

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
abstract public class Projectile : BaseEntity
{
    public int DirectDamage = 25;
    public int SplashDamage = 15;
    public bool ApplySplashDamage = true;
    
    public float Speed = 100f;
    public float TimeToLive = 5f;    
    public GameObject ExplosionPrefab;    
    
    protected bool targetHit;

    protected Vector3 _direction;

    protected override void Start()
    {
        base.Start();
        gameObject.tag = Tags.PROJECTILE_TAG;
        _direction = Vector3.forward;
        // Destroy the projectile after a specified lifetime        
        Destroy(gameObject, TimeToLive);
    }

    public void SetMovement(Vector3 direction)
    {
        _direction = direction;
    }

    protected virtual void Update()
    {
        transform.Translate(Speed * Time.deltaTime * _direction);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {   
        if(!enabled) return;                      
        if(collision.gameObject.tag == Tags.DEFAULT_TAG || collision.gameObject.tag == Tags.ENEMY_TAG)
        {
            Explode();
        }
        if (collision.transform.TryGetComponent<IAttackable>(out var attackTarget))
        {
            if (attackTarget.GetTag == Tags.PLAYER_TAG) return;            
            Debug.Log($"Looks like I hit {collision.transform.name}");
            attackTarget.TakeDamage(DirectDamage);
            //Need to do a bounding sphere to check all surrouding for enemie,
            //loop and apply splash damage
            //use the splashdamage to refactor player

        }
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }
        targetHit = true;
        Destroy(gameObject);
    }

    protected virtual void Explode()
    {
        if (ExplosionPrefab == null) return;
        GameObject newExplosion = Instantiate(ExplosionPrefab, transform.position, ExplosionPrefab.transform.rotation, null);
        Destroy(newExplosion, 1f);
    }

    /*private void Explode()
    {
        if (ExplosionPrefab == null) return;
        Instantiate(ExplosionPrefab, transform.position, ExplosionPrefab.transform.rotation, null);
    }*/
}