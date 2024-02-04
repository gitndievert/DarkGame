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


using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class Enemy : BaseEntity, IAttackable
{

    public int Health;
    public EnemyType EnemyType = EnemyType.Land;

    [Range(0, 30)]
    public int MinAttackDamage = 1;
    [Range(0, 100)]
    public int MaxAtackDamage;

    public GameObject[] BloodEffects;

    [Header("Attack")]
    public float AggroRange = 10f;
    public float AttackRange = 2f;
    public float MoveSpeed = 3f;
    public float AttackCoolDown = 2f;

    [Space(5)]
    [Header("AI Properties")]
    public bool IsWandering = false;
    public float wanderRadius = 5f;
    public float wanderTimer = 5f;
    
    [Space(5)]
    [Header("Sounds")]
    public AudioClip[] AgroSounds;
    public AudioClip[] AttackSounds;
    public AudioClip[] PainSounds;
    public AudioClip[] DeathSounds;

    [HideInInspector]
    public bool IsDead = false;

    public Transform AttackTarget { get { return transform; } }

    private Player _player;
    //private float _fieldOfView = 85f;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private bool _isAggro = false;
    //private float FieldOfView = 85f;
    private bool isAttackCooldown = false;
    private BoxCollider _collider;
    private AudioSource _audioSource;


    protected override void Start()
    {
        _player = GameManager.Instance.MyPlayer;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider>();
        _audioSource = GetComponent<AudioSource>();
        gameObject.layer = Tags.enemyLayer;
        gameObject.tag = Tags.ENEMY_TAG;                
    }

    void Update()
    {
        if (IsDead) return;
        if (_navMeshAgent.enabled &&
            IsWandering &&
            !_navMeshAgent.pathPending && _navMeshAgent.remainingDistance < 0.5f)
        {
            Wander();
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);        
        if (distanceToPlayer < AggroRange)        
        {
            if (AgroSounds != null && !_isAggro)
            {                
                PlaySound(AgroSounds);
            }
            _isAggro = true;            
            if (distanceToPlayer > AttackRange)
            {
                _navMeshAgent.isStopped = false;
                MoveTowardsPlayer();
            }
            else
            {                
                _navMeshAgent.isStopped = true;
                if (!isAttackCooldown)
                {
                    StartCoroutine(AttackCooldown());
                }
            }           
        }
        else
        {
            _isAggro = false;
            Idle();
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        int totalDamage = Health - amount;
        if (totalDamage <= 0)
        {
            Debug.Log($"A {Name} took {amount} damage!");
            Dead();
        }
        else
        {
            if (PainSounds != null)
            {
                PlaySound(PainSounds);                
            }
            Health -= amount;
            Debug.Log($"A {Name} took {amount} damage and is at {Health} Health!");
        }
    }

    private bool SeePlayer()
    {
        Vector3 targetDirection = _player.transform.position - transform.position;
        Ray ray = new Ray(transform.position, targetDirection);

        Debug.DrawRay(ray.origin, ray.direction * AggroRange, Color.green, 0.1f);

        if (Physics.Raycast(ray, out RaycastHit hit, AggroRange))
        {
            if(hit.collider.CompareTag(Tags.PLAYER_TAG))
            {
                Debug.Log(hit.collider.name);
                return true;
            }           
        }

        return false;
    }

    private void PlaySound(AudioClip[] clips)
    {
        if(clips.Length > 1)
        {
            int rand = Random.Range(0, clips.Length - 1);
            _audioSource.PlayOneShot(clips[rand]);
        }
        else
        {
            _audioSource.PlayOneShot(clips[0]);
        }
    }
    

    /*protected bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, _player.transform.position) <= AggroRange)
        {
            Vector3 targetDirection = _player.transform.position - transform.position;
            float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
            if (angleToPlayer >= -FieldOfView && angleToPlayer <= FieldOfView)
            {
                Ray ray = new Ray(transform.position, targetDirection);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, AggroRange))
                {
                    if (hitInfo.transform.CompareTag("Player"))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }*/

    /*void MoveTowardsPlayer()
    {
        _navMeshAgent.SetDestination(player.position);
    }*/

    private void MoveTowardsPlayer()
    {        
        Vector3 direction = (_player.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        //transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime); OLD CODE
        _navMeshAgent.SetDestination(_player.transform.position);

        //if no line of site need to turn on the navmesh to move him
        //float speed = _navMeshAgent.velocity.magnitude;
        _animator.SetFloat("Speed", MoveSpeed);
        _animator.Play("Walk");
    }

    private void Idle()
    {        
        _animator.SetFloat("Speed", 0);
        _animator.Play("Idle");
    }

    private void Attack()
    {
        if (IsDead) return;
        if (_player.Health > 0 && SeePlayer())
        {            
            int dmgRange = Random.Range(MinAttackDamage, MaxAtackDamage);
            _player.TakeDamage(dmgRange);            
            _animator.Play("Attack");
            if(AttackSounds != null)
            {                
                PlaySound(AttackSounds);
            }
            Debug.Log($"A {Name} is moving in for the attack for {dmgRange} damage!");            
        }
    }

    private IEnumerator AttackCooldown()
    {
        isAttackCooldown = true;        
        Attack();        
        yield return new WaitForSeconds(AttackCoolDown);
        isAttackCooldown = false;
    }

    protected void Dead()
    {
        if (DeathSounds != null)
        {
            PlaySound(DeathSounds);
        }        
        Health = 0;
        IsDead = true;
        _collider.enabled = false;
        Debug.Log($"A {Name} died!");
        //Destroy(gameObject); //TODO: THIS IS TEMP WE NEED BODIES TO STAY
        //Add death animations
        _animator.Play("Dead");
    }

    protected void Wander()
    {        
        _animator.SetFloat("Speed", MoveSpeed);        
        _animator.Play("Walk");
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



}
public enum EnemyType
{
    Land = 0,
    Fly
}
