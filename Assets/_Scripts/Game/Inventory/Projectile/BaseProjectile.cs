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
using PampelGames.GoreSimulator;
using Dark.Utility.Sound;
using Unity.VisualScripting;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
abstract public class BaseProjectile : BaseEntity, IProjectile
{       
    public int DirectDamage;    
    public int SplashDamage;
    public abstract bool ApplySplashDamage { get; }
    public float DetectionRadius = 100f;

    public GameObject ProjectileGameObject => gameObject;

    public float Speed = 100f;
    public float TimeToLive = 5f;    
    public GameObject ExplosionPrefab;    
    public AudioClip ExplosionSound;
    
    protected bool targetHit;
    protected int splashLimit = 10;
    protected Vector3 _direction;
    protected Rigidbody rb;

    private AudioSource _audioSource;
    
    
    protected virtual void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = 1;
        rb = GetComponent<Rigidbody>();
    }

    protected override void Start()
    {
        base.Start();
        gameObject.tag = Tags.PROJECTILE_TAG;
        _direction = Vector3.forward;
        // Destroy the projectile after a specified lifetime        
        Destroy(gameObject, TimeToLive);
    }
    protected virtual void Update()
    {
        transform.Translate(Speed * Time.deltaTime * _direction);        
    }

    public void SetMovement(Vector3 direction)
    {
        _direction = direction;
    }   

    public void SetDirectDamage(int damage)
    {
        DirectDamage = damage;
    }

    public string WhoFiredTag { get; set; }
    
    protected List<Enemy> GetPoolEnemiesForSplash()
    {
        var enemyPool = new HashSet<Enemy>();            
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, DetectionRadius,Vector3.up);        
        foreach (RaycastHit hit in hits)
        {   
            if (hit.transform.root.CompareTag(Tags.ENEMY_TAG))
            {
                enemyPool.Add(hit.transform.gameObject.GetComponentInParent<Enemy>());                
            }
        }       

        return enemyPool.ToListPooled();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {   
        if(!enabled) return;
        //Ignore the firing source for projectiles
        if (collision.transform.root.CompareTag(Tags.PLAYER_TAG) && WhoFiredTag == Tags.PLAYER_TAG) return;
        if (collision.transform.root.CompareTag(Tags.ENEMY_TAG) && WhoFiredTag == Tags.ENEMY_TAG) return;
        
        if (collision.gameObject.tag == Tags.DEFAULT_TAG || 
            (collision.transform.root.tag == Tags.ENEMY_TAG && WhoFiredTag != Tags.ENEMY_TAG))
        {
            Explode();
        }        

        if (collision.transform.TryGetComponent<IAttackable>(out var attackTarget))
        {            
            Debug.Log($"Looks like I hit {collision.transform.name}");
            attackTarget.TakeDamage(DirectDamage);
            
        } 
        else if(collision.transform.TryGetComponent<IGoreObject>(out var goreObject))
        {
            if (goreObject != null)
            {
                var enemyScript = collision.transform.GetComponentInParent<Enemy>();
                enemyScript.TakeDamage(DirectDamage);                
            }
        }

        ExplodeSplashDamage(DirectDamage);

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
        if(ExplosionSound != null)
        {            
            _audioSource.PlayOneShot(ExplosionSound);
        }
        Destroy(newExplosion, 1f);
    }

    protected void ExplodeSplashDamage(int damage)
    {
        if (!ApplySplashDamage) return;
        var enemyPool = GetPoolEnemiesForSplash();        
        //We want to scale down damage a bit to distribute
        if (enemyPool.Count > 0)
        {
            //damage = enemyPool.Count >= 5 ? Mathf.RoundToInt(damage / 2) : damage;
            damage = Mathf.RoundToInt(damage / 2); //just half the dmg for now
            foreach(Enemy enemy in enemyPool)
            {
                if (enemy == null || !enemy.gameObject.activeSelf) continue;
                enemy.TakeDamage(damage);
            }

        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere around the object to visualize the detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);
    }

}