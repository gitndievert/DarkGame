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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum EnemyType
{
    Land = 0,
    Fly
}


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(EnemyLootDrop))]
/*
 * NOTES: 
 * Do not attach a parent rigid body or collider!
 * Gore Simulator will break that out on each limb
 *  
 */
public class Enemy : BaseEntity, IAttackable
{
    public int Health;
    public EnemyType EnemyType = EnemyType.Land;
    public bool TargetLimbs = false;

    [Range(0, 30)]
    public int MinAttackDamage = 1;
    [Range(0, 100)]
    public int MaxAtackDamage;
    public float FieldOfView = 120f;    

    [Header("Attack")]
    public float AggroRange = 10f;
    public float AttackRange = 2f;    
    public float AttackCoolDown = 2f;

    [Space(5)]
    [Header("AI Properties")]
    public bool IsWandering = false;
    public float wanderRadius = 5f;
    public float wanderTimer = 5f;

    [Space(5)]
    [Header("Sounds")]
    public AudioClip[] IdleSounds;
    public AudioClip[] AgroSounds;
    public AudioClip[] AttackSounds;
    public AudioClip[] PainSounds;
    public AudioClip[] DeathSounds;
    
    public GameObject[] BloodEffects;    
        
    public bool IsDead = false;    
    public Transform AttackTarget { get { return transform; } }

    [Space(5)]
    [Header("Projectiles")]    
    public GameObject Projectile;
    public float FiringDistance = 30f;
    public GameObject ProjectileFiringPoint;    

    private Player _player;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private bool isAttackCooldown = false;
    private float _distanceToPlayer;
    private AudioSource _audioSource;
    private GoreSimulator _goreSimulator;
    private List<GameObject> _gibPool;
    private bool _hasGibs { get { return _gibPool.Count > 0; } }
    [SerializeField]
    private bool _isAggro = false;
    [SerializeField]
    private float _interestedTimer = 30f; //come back later
    private bool _idleSounds = false;
    private EnemyLootDrop _enemyLootDrop;
    

    protected virtual void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;                
        _goreSimulator = GetComponent<GoreSimulator>();
        _enemyLootDrop = GetComponent<EnemyLootDrop>();
    }

    protected override void Start()
    {
        _player = GameManager.Instance.MyPlayer;       
        gameObject.layer = Tags.enemyLayer;
        gameObject.tag = Tags.ENEMY_TAG;        
        _gibPool = new List<GameObject>();
        InvokeRepeating("ClearGibs", 1f, 10f);
        //_goreSimulator.OnDeath += Dead;
        _idleSounds = true;
        InvokeRepeating("StartIdleSounds", 5f, 20f);
    }
    
    void Update()
    {
        if (IsDead) return;
        if (_navMeshAgent.enabled &&
            IsWandering &&
            !_navMeshAgent.pathPending && 
            _navMeshAgent.remainingDistance < 0.5f &&
            !_isAggro)
        {
            Wander();
        }

        _distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);       

        /*if (_interestedTimer > 0 && _isAggro)
        {
            _interestedTimer -= Time.deltaTime;
        }
        else
        {
            Debug.Log($"{Name} lost intested in attacking you!");
         
        }*/

        if (_isAggro)
        {            
            //Keep enemy moving towards player
            if (_idleSounds)
            {
                StopIdleSounds();
            }

            if (_distanceToPlayer >= FiringDistance && Projectile != null)
            {
                _navMeshAgent.isStopped = true;
                _animator.SetFloat("Speed", 0);
                if (!isAttackCooldown)
                {
                    StartCoroutine(ProjectileAttackCooldown());
                }

            }
            else if (_distanceToPlayer > AttackRange)
            {
                _navMeshAgent.isStopped = false;
                MoveTowardsPlayer();
            }
            else
            {
                _navMeshAgent.isStopped = true;
                _animator.SetFloat("Speed", 0);
                if (!isAttackCooldown)
                {
                    StartCoroutine(AttackCooldown());
                }
            }
        }
        else if (!_isAggro && ((_distanceToPlayer <= AggroRange && NearPlayer()) || (CanSeePlayer() && _distanceToPlayer <= (AggroRange * 2))))
        {   
            if (AgroSounds != null && !_isAggro)
            {                
                PlaySound(AgroSounds);
            }
            _isAggro = true;
        }
        else
        {
            _isAggro = false;            
        }

        //Test Keys for Gore Simulator
        if (Input.GetKeyDown(KeyCode.I))
        {
            _goreSimulator.ExecuteRagdoll();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            _goreSimulator.ExecuteExplosion();
        }

    }

    protected void Wander()
    {        
        _animator.Play("Walk");
        _animator.SetFloat("Speed", 1f);
        _navMeshAgent.speed = 5f;
        Vector3 randomPoint = RandomNavSphere(transform.position, wanderRadius, -1);
        _navMeshAgent.SetDestination(randomPoint);
    }

    private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layermask);
        return navHit.position;
    }

    protected bool CanSeePlayer()
    {
       
        Vector3 targetDirection = _player.transform.position - transform.position;
        float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
        if (angleToPlayer >= -FieldOfView && angleToPlayer <= FieldOfView)
        {
            Ray ray = new(transform.position, targetDirection);
            
            Debug.DrawRay(ray.origin, ray.direction * AggroRange, Color.blue, 0.1f);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, AggroRange))
            {
                if (hitInfo.transform.CompareTag(Tags.PLAYER_TAG))
                {
                    Debug.Log("I can see player for attack!");
                    return true;
                }
            }
        }       

        return false;
    }

    private bool NearPlayer()
    {
        Vector3 targetDirection = _player.transform.position - transform.position;
        Ray ray = new(transform.position, targetDirection);

        Debug.DrawRay(ray.origin, ray.direction * AggroRange, Color.green, 0.1f);

        if (Physics.Raycast(ray, out RaycastHit hit, AggroRange))
        {
            if (hit.collider.CompareTag(Tags.PLAYER_TAG))
            {
                //Debug.Log(hit.collider.name);
                Debug.Log("I am close for attack!");
                return true;
            }
        }

        return false;
    }

    private bool LooseInterest()
    {
        return false;
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = (_player.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        _navMeshAgent.SetDestination(_player.transform.position);
        _animator.SetFloat("Speed", 2f);        
    }

    private void Attack()
    {
        if (IsDead) return;
        //If enemy has no arms, dont attack
        if (_hasGibs)
        {
            foreach (GameObject gib in _gibPool)
            {
                //revisit this logic
                if (gib.name.Contains("arm"))
                {
                    Debug.Log($"A {Name} cannot attack, he has no arms!");
                    _animator.SetFloat("Speed", 0);
                    return;
                }
            }
        }
        if (_player.Health > 0 && NearPlayer())
        {
            int dmgRange = Random.Range(MinAttackDamage, MaxAtackDamage);
            _player.TakeDamage(dmgRange);
            _animator.SetFloat("Speed",0);
            _animator.Play("Attack");            
            if (AttackSounds != null)
            {
                PlaySound(AttackSounds);
            }
            Debug.Log($"A {Name} is moving in for the attack for {dmgRange} damage!");
        }
    }

    private void ProjectileAttack()
    {
        if (IsDead) return;
        if (_player.Health > 0)
        {
            _animator.SetFloat("Speed", 0);
            _animator.Play("Attack");            
            var projectileObject = Instantiate(Projectile, ProjectileFiringPoint.transform.position, transform.rotation);
            var projectile = projectileObject.GetComponent<IProjectile>();
            Vector3 direction = (_player.transform.position - transform.position);
            transform.rotation = Quaternion.LookRotation(direction);
            projectile.SetMovement(direction);
            projectile.WhoFiredTag = GetTag;
        }
    }

    private IEnumerator AttackCooldown()
    {
        isAttackCooldown = true;
        Attack();
        yield return new WaitForSeconds(AttackCoolDown);
        isAttackCooldown = false;
    }

    private IEnumerator ProjectileAttackCooldown()
    {
        isAttackCooldown = true;
        ProjectileAttack();
        yield return new WaitForSeconds(AttackCoolDown);
        isAttackCooldown = false;
    }

    #region Damage Layer

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        int totalDamage = Health - amount;        
        if (totalDamage <= 0)
        {
            if (amount >= 50)
            {
                _goreSimulator.ExecuteRagdoll();
                _goreSimulator.ExecuteExplosion(out var gibs);
                foreach (GameObject gib in gibs)
                {
                    _gibPool.Add(gib);
                }
                CamShake.Instance.Shake(3f, .4f);
                Debug.Log($"A {Name} exploded to bits!");
                Dead();                
            }
            else
            {
                Debug.Log($"A {Name} took {amount} damage!");
                Dead();
            }            
        }
        else
        {
            if (amount >= 75)
            {
                _goreSimulator.ExecuteRagdoll();
                _goreSimulator.ExecuteExplosion(10f);
                CamShake.Instance.Shake(3f, .4f);
                Debug.Log($"A {Name} exploded to bits!");
                Dead();
                return;
            }
            if (PainSounds != null)
            {
                PlaySound(PainSounds);
            }
            Health -= amount;
            Debug.Log($"A {Name} took {amount} damage and is at {Health} Health!");
            _isAggro = true;           
        }
    }

    public void TakeDamage(int amount, IGoreObject goreObject, RaycastHit hit)
    {
        if (IsDead) return;
        Vector3 hitPoint = hit.point;        
       
        int totalDamage = Health - amount;
        if (totalDamage <= 0)
        {
            Debug.Log($"A {Name} took {amount} damage!");
            if (amount >= 50)
            {
                _goreSimulator.ExecuteRagdoll();
                _goreSimulator.ExecuteExplosion(out var gibs);
                foreach (GameObject gib in gibs)
                {
                    _gibPool.Add(gib);
                }
                CamShake.Instance.Shake(3f, .4f);
                Debug.Log($"A {Name} exploded to bits!");
                Dead();                
            }
            else
            {
                Dead();
            }
        }
        else
        {
            //Gore Simulator Cut Randomizer
            int rndRoll = Random.Range(1, 100);
            if(amount >= 75)
            {
                _goreSimulator.ExecuteRagdoll();
                _goreSimulator.ExecuteExplosion(out var gibs);
                foreach (GameObject gib in gibs)
                {
                    _gibPool.Add(gib);
                }
                CamShake.Instance.Shake(3f, .4f);
                Debug.Log($"A {Name} exploded to bits!");
                Dead();
                return;
            }
            else if((amount >= 30 || rndRoll >= 85) && TargetLimbs)
            {
                goreObject.ExecuteCut(hitPoint, out GameObject gib);
                _gibPool.Add(gib);
            }
            if (PainSounds != null)
            {
                PlaySound(PainSounds);
            }
            Health -= amount;
            _isAggro = true;
            Debug.Log($"A {Name} took {amount} damage and is at {Health} Health!");
        }
    }

    private void ClearGibs()
    {
        return;
        //Fix later

        /*if (!_hasGibs) return;
        foreach (GameObject gibs in _gibPool)
        {
            _gibPool.Remove(gibs);
            Destroy(gibs);
        }*/
    }

    protected void Dead()
    {
        if (DeathSounds != null)
        {
            PlaySound(DeathSounds);
        }
        _animator.SetFloat("Speed", 0);
        Health = 0;
        IsDead = true;
        _isAggro = false;
        Debug.Log($"A {Name} died!");

        //If there has been a limb split, then destory limbs and drop the torso
        //else play regular death animation
        if (_hasGibs)
        {
            _animator.StopPlayback();
            ClearGibs();
        }
        else
        {
            _animator.Play("Dead");
        }

        CancelInvoke();
        StopAllCoroutines();

        if(_enemyLootDrop != null)
        {
            //Just Random For Now
            _enemyLootDrop.DropRandom();
        }
    }

    #endregion

  
    private void StartIdleSounds()
    {
        PlaySound(IdleSounds);
    }

    private void StopIdleSounds()
    {
        CancelInvoke("StartIdleSounds");        
        _idleSounds = false;
    }

    private void PlaySound(AudioClip[] clips)
    {        
        if (clips.Length > 1)
        {
            int rand = Random.Range(0, clips.Length - 1);
            _audioSource.PlayOneShot(clips[rand]);
        }
        else
        {
            _audioSource.PlayOneShot(clips[0]);
        }
    }   




}