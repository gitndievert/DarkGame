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
using Dark.Utility;
using Dark.Utility.Sound;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using PampelGames.GoreSimulator;

[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerController))]
public class Player : BaseEntity, IAttackable
{   
    public int Health = 0;
    public int Armor = 0;
    public float CriticalStrikeChance = 0.15f;

    public PowerUpType ActivatedPlayerPowerup = PowerUpType.None;

    public PlayerInventory PlayerInventory;
    public float DetectionRadius = 5f;
    public float FallDistance = -25f;
    public GameObject ProjectileSpot;
    public PlayerController PlayerController { get; private set; }

    public Transform AttackTarget { get { return transform; } }


    public AudioClip[] TakingDamageSounds;
    public AudioClip[] DeathSounds;
    public AudioClip WallPushSound;

    private Collider[] _actionBuffer;
    [SerializeField]
    private bool _IsDead = false;
    private float _nextShotTime = 1f;
    private Vector3 _entryPoint;
    private UnityEngine.UI.Image _runningIconImage;

    private void Awake()
    {
        PlayerInventory = GetComponent<PlayerInventory>();
        PlayerController = GetComponent<PlayerController>();
        Health = InventoryLimits.MaxHealth;
        Armor = InventoryLimits.MaxArmor;
        gameObject.tag = Tags.PLAYER_TAG;
        DontDestroyOnLoad(gameObject);
        _entryPoint = Vector3.zero;
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        transform.position = _entryPoint;        
        _runningIconImage = GameObject.Find("RunningIcon").GetComponent<UnityEngine.UI.Image>();
        _runningIconImage.enabled = false;
        _actionBuffer = new Collider[5];       
    }

    private void Update()
    {
        if (Input.GetMouseButton(LEFT_MOUSE) && Time.time > _nextShotTime)
        {
            PrimaryAttack();
        }
        else if (Input.GetMouseButtonDown(LEFT_MOUSE))
        {
            PrimaryAttack();
        }
        else if (Input.GetMouseButton(RIGHT_MOUSE))
        {
            //Fire Secondary Weapon
            SecondaryAttack();
        }

        // Check for continuous fire (mouse button held down)
       

        ActionScan();        
                        
        //Update players health
        UIManager.Instance.HealthLabel.text = Health.ToString();
        //Update players armor
        UIManager.Instance.ArmorLabel.text = Armor.ToString();

        SmashPowerUp();

        //NOTE: Need to have it where if a distance falled is a fall > FallDistance
        if (transform.position.y <= FallDistance && !_IsDead)
        {            
            Dead();
        }

        _runningIconImage.enabled = PlayerController.IsRunning;

    }

    public void ActionScan()
    {
        //int actionLayer = LayerMask.NameToLayer(BaseItem.ACTION_TAG); MIGHT USE LAYERS LATER
        bool insideActionLayer = false;
        bool spaceHit = Input.GetKeyDown(KeyCode.Space);
        int actions = Physics.OverlapSphereNonAlloc(transform.position, DetectionRadius, _actionBuffer);
        // Check for colliders within the specified radius around the player        
        for (int i = 0; i < actions; i++)
        {
            if (_actionBuffer[i].gameObject.tag == Tags.ACTION_TAG)
            {
                var action = _actionBuffer[i].gameObject.GetComponent<BaseAction>();
                if (action.RequiresPlayerOpen)
                {
                    if (spaceHit && !action.SwitchOnly)
                    {
                        action.DoAction();
                    }
                }
                else
                {
                    action.DoAction();
                }
                insideActionLayer = true;                
                break;
            }            
        }

        if (spaceHit && !insideActionLayer && WallPushSound != null)
        {
            SoundManager.PlaySound(WallPushSound);
        }        
    }

    public void SetArmor(int amount)
    {
        if ((Armor + amount) >= InventoryLimits.MaxArmor) return;
        Armor += amount;
    }

    public void TakeDamage(int amount) 
    {
        int calcDmg = amount;
        if(Armor > 0)
        {
            int ArmorChange = 20;
            calcDmg = Mathf.RoundToInt(calcDmg / 2);
            if ((Armor - ArmorChange) <= 0)
            {
                Armor = 0;
            }
            else
            {
                Armor -= (Armor - ArmorChange) > 0 ? ArmorChange : 0;
            }
        }
        
        if (TakingDamageSounds != null)
        {
            SoundManager.PlaySound(TakingDamageSounds);
        }

        UIManager.Instance.ScreenEffects.DamageFlash();
        Health -= calcDmg;

        if(Health <= 0)
        {
            Dead();
        }

        Debug.Log($"Took {calcDmg} damage and health is at {Health}");
    }
       

    /*private IEnumerator Explosive(int amount) 
    {
        //Need to figue out an explosion thing
        //Maybe add a cam shake
        yield return new WaitForSeconds(amount);
    }*/

    public void Heal(int amount)
    {
        if ((amount + Health) >= InventoryLimits.MaxHealth)
        {
            Health = InventoryLimits.MaxHealth;
        }
        else
        {
            Health += amount;
        }

        UIManager.Instance.ScreenEffects.HealFlash();
    }       

    public void AddArmor(int amount)
    {
        if((amount + Armor) >= InventoryLimits.MaxArmor)
        {
            Armor = InventoryLimits.MaxArmor;
        }
        else
        {
            Armor += amount;
        }

        UIManager.Instance.ScreenEffects.Flash();
    }

    public void Dead()
    {
        _IsDead = true;
        Health = 0;
        PlayerController.GetRigidBody().isKinematic = true;
        if (DeathSounds != null)
        {
            SoundManager.PlaySound(DeathSounds);
        }        
        StartCoroutine(FallOverDead());
    }

    private IEnumerator FallOverDead()
    {
        Debug.Log("Imma be dead!");
        yield return new WaitForSeconds(3f);
        Respawn();
    }

    private void Respawn()
    {        
        _IsDead = false;
        PlayerController.GetRigidBody().isKinematic = false;
        //TODO: Will need to change this to save points later
        transform.position = _entryPoint;
        Health = InventoryLimits.STARTING_MAX_HEALTH;
        Armor = InventoryLimits.STARTING_MAX_ARMOR;        
    }

    public void PrimaryAttack()
    {
        if (PlayerInventory.CurrentWeapon == null) return;
        Weapon weapon = PlayerInventory.CurrentWeapon;
        int actualDamage = weapon.PrimaryFireDamage;
        float roundDelay = weapon.RoundDelay;
        bool criticalStike = false;

        _nextShotTime = Time.time + roundDelay;

        //Double Damage Powerup
        if (ActivatedPlayerPowerup == PowerUpType.DoubleDamage)
        {
            actualDamage *= 2;
        }

        //Factor critical hit chance
        if(CriticalStrikeChance > 0)
        {
            if(Random.value < CriticalStrikeChance)
            {
                actualDamage *= 3;
                criticalStike = true;
            }
        }

        if (weapon.AmmoType == AmmoType.None)
        {            
            TargetEnemy(actualDamage, weapon.AttackDistance);
            weapon.PrimaryAttack();
            //TODO: Come back!!!
            CamShake.Instance.Shake(weapon.ShakeIntensity, weapon.ShakeDuration);
            Debug.Log($"Swinging for {actualDamage}");
            if (criticalStike)
            {
                Debug.Log($"Critical Hit!");
            }
        }
        else
        {
            FiringWeapon fireWeapon = (FiringWeapon)weapon;            
            int shotsLeft = PlayerInventory.SpendAmmo(fireWeapon.AmmoType, 1);
            //Check ammo
            if (shotsLeft > 0)
            {   
                if (fireWeapon.UsesProjectile)
                {                    
                    TargetEnemy(fireWeapon.Projectile);
                }
                else
                {
                    TargetEnemy(actualDamage, fireWeapon.AttackDistance);                    
                }

                fireWeapon.PrimaryAttack();
                CamShake.Instance.Shake(fireWeapon.ShakeIntensity, fireWeapon.ShakeDuration);
                //Animator here?
                Debug.Log($"Shot fired for {actualDamage} damage with {shotsLeft} shots left");
                if (criticalStike)
                {
                    Debug.Log($"Critical Hit!");
                }
            }
            else
            {
                fireWeapon.PlayEmptyClipSound();
                Debug.Log("Out of Ammo");
            }
        }
    }

    public void SecondaryAttack()
    {
        Debug.Log("Firing secondary fire");
    }

    private void TargetEnemy(int damage, float raycastDistance)
    {
        Ray ray = PlayerController.PlayerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 0.1f);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
        {            
            if (hit.collider.TryGetComponent<IGoreObject>(out var goreObject))
            {
                var enemyScript = hit.collider.GetComponentInParent<Enemy>();
                enemyScript.TakeDamage(damage, goreObject, hit);                
            }
        }
    }

    /// <summary>
    /// Note for later, the projectiles will have their own damage
    /// </summary>
    /// <param name="projectile"></param>
    private void TargetEnemy(GameObject projectile)
    {
        Ray ray = PlayerController.PlayerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));        
        Vector3 direction = ray.direction.normalized;        
        GameObject genP = Instantiate(projectile, ProjectileSpot.transform.position, transform.rotation);
        genP.transform.rotation = Quaternion.LookRotation(direction);       
    }

    public void SmashPowerUp()
    {
      /*if(ActivatedPlayerPowerup == PowerUpType.Smash)
      {
          StartCoroutine(Explosive(100));
          ActivatedPlayerPowerup = PowerUpType.None; //might need to solve later
      }*/
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);
    }


}
