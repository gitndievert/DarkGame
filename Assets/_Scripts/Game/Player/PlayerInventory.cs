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
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Player))]
public class PlayerInventory : BaseEntity
{
    public Weapon[] Weapons;
    public Ammo[] PlayerAmmo;    

    public int CountWeapons
    {
        get
        {
            return Weapons.Length;
        }
    }

    public Weapon CurrentWeapon;

    private int _currentWeaponIndex;
    private Player _player;
    private const int _healRepeatLimit = 4;
    private int _healRepeatAmount;
    private int _healRepeatCount = 0;
    private UnityEngine.UI.Image _ammoIconImage;

    private void Awake()
    {
        //Deactivate the weapons and armors
        //These should be a part of the player default object
        if (Weapons.Length == 0 || Weapons == null)
        {
            Weapons = GetComponentsInChildren<Weapon>(true);
        }
        if (CountWeapons > 0)
        {
            foreach (Weapon weapon in Weapons)
            {
                if (weapon == null) continue;
                weapon.gameObject.SetActive(false);
            }
        }
        _player = GetComponent<Player>();
    }
      
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _ammoIconImage = GameObject.Find("AmmoIcon").GetComponent<UnityEngine.UI.Image>();
        _ammoIconImage.enabled = false;
        UIManager.Instance.AmmoLabel.text = "";
        UIManager.Instance.PowerupLabel.text = "";
    }

    private void Update()
    {
        // Switch weapons using the mouse wheel
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        if (mouseWheel != 0f)
        {
            int wScroll = (mouseWheel > 0f ? 1 : -1) + _currentWeaponIndex;
            int idxStop = CountWeapons - 1;
            wScroll = wScroll > idxStop ? 0 : wScroll;
            wScroll = wScroll < 0 ? idxStop : wScroll;
            ChangeWeapon(wScroll);
        }

        // Switch weapons using keyboard numbers (1-9)
        for (int i = 1; i <= Weapons.Length; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                ChangeWeapon(i - 1);
            }
        }

        //This will track ammo for the current weapon
        UIManager.Instance.AmmoLabel.text = "";
        if (CurrentWeapon != null)
        {
            if (CurrentWeapon.AmmoType == AmmoType.None)
            {
                UIManager.Instance.AmmoLabel.text = "\u221E";
                _ammoIconImage.enabled = false;
            }
            else
            {
                UIManager.Instance.AmmoLabel.text = GetAmmo(CurrentWeapon.AmmoType).Amount.ToString();
                if (CurrentWeapon.AmmoIcon != null)
                {
                    _ammoIconImage.enabled = true;
                    _ammoIconImage.sprite = CurrentWeapon.AmmoIcon;
                }
            }
        }
        else
        {
            _ammoIconImage.enabled = false;
        }
    }    

    public int SpendAmmo(AmmoType ammoType, int amount)
    {
        var ammo = GetAmmo(ammoType);
        if(ammo.Amount > 0)
        {
            ammo.Amount -= amount;            
            return ammo.Amount;
        }
        return 0;        
    }

    public Ammo GetAmmo(AmmoType ammoType)
    {
        foreach(Ammo ammo in PlayerAmmo)
        {
            if (ammo == null) continue;
            if (ammo.AmmoType == ammoType)
            {
                return ammo;
            }            
        }

        return null;        
    }

    public Weapon EquipWeapon(Weapon weapon, int weaponslot)
    {
        ChangeWeapon(weaponslot);
        CurrentWeapon = weapon;
        //Load up the gun with starting ammo
        var ammo = GetAmmo(weapon.AmmoType);
        if (ammo.Amount == 0)
        {
            int startAmmo = weapon.StartingAmmo;
            if (weapon.StartingAmmo > weapon.AmmoType.GetAmmoLimit())
            {
                startAmmo = weapon.AmmoType.GetAmmoLimit();
            }            
            ammo.Amount += startAmmo;
        }        
        return CurrentWeapon;
    }

    /// <summary>
    /// Collision with a pickupable item
    /// </summary>
    /// <param name="col"></param>
    private void OnTriggerEnter(Collider col)
    {
        if (col.transform.TryGetComponent<IPickupable>(out var pickup))
        {
            switch (pickup.GetTag)
            {
                case Tags.AMMO_TAG:
                    SortAmmo((PickupAmmo)pickup);
                    break;
                case Tags.ARMOR_TAG:
                    SortArmor((PickupArmor)pickup);
                    break;
                case Tags.POWERUP_TAG:
                    SortPowerup((PickupPowerup)pickup);
                    break;
                case Tags.WEAPON_TAG:
                    SortWeapon((PickupWeapon)pickup);
                    break;
                case Tags.HEALTH_TAG:
                    SortHealth((PickupHealth)pickup);
                    break;
                default:
                    Debug.Log("Unknown pickup " + pickup.ToString() + " check if prefab has proper tag!");
                    break;
            }
        }
    }

    private void SortHealth(PickupHealth health)
    {
        if (_player.Health == InventoryLimits.MaxHealth) return;
        if(health.PickupType == HealthType.HealOverTime && _healRepeatAmount == 0)
        {
            _healRepeatAmount = Mathf.RoundToInt(health.Amount / _healRepeatLimit);
            InvokeRepeating("PlayerHealOverTime", 1f,_healRepeatLimit);
        }
        else
        {
            Debug.Log(health.PickupType.ToString() + " healing for amount " + health.Amount);
            _player.Heal(health.Amount);
        }

        health.Consume();
    }

    private void SortWeapon(PickupWeapon weapon)
    {
        Debug.Log(weapon.PickupType.ToString() + " " + weapon.Amount);        
        for (int i = 0; i < CountWeapons; i++)
        {
            Weapon item = Weapons[i];
            if (item.WeaponType == weapon.PickupType)
            {                
                if(!item.gameObject.activeSelf)
                {
                    item.FoundPickup = true;
                    EquipWeapon(item, i);
                }               
                break;
            }
        }        

        weapon.Consume();
    }
    private void SortAmmo(PickupAmmo ammo)
    {
        Debug.Log(ammo.PickupType.ToString() + " " + ammo.Amount);        
        bool consumeResource = true;
        foreach (Ammo item in PlayerAmmo)
        {
            if (item == null) continue;
            if (item.AmmoType == ammo.PickupType)
            {
                int ammoLimit = ammo.PickupType.GetAmmoLimit();
                int aCombined = item.Amount + ammo.Amount;
                if (ammoLimit >= aCombined)
                {
                    item.Amount += ammo.Amount;
                }
                else
                {
                    consumeResource = item.Amount < 300;
                    item.Amount = ammoLimit;
                }

                Debug.Log($"Total {ammo.PickupType} " + item.Amount);
                break;
            }
            
        }

        if (consumeResource)
        {            
            ammo.Consume();
        }
        
    }

    private void SortArmor(PickupArmor armor)
    {             
        if (_player.Armor == InventoryLimits.MaxArmor) return;
        Debug.Log(armor.PickupType.ToString() + " added " + armor.Amount + " armor");
        _player.AddArmor(armor.Amount);
        armor.Consume();
    }

    private void SortPowerup(PickupPowerup powerup)
    {
        Debug.Log(powerup.PickupType.ToString());

        //Sort out the health and armor powerups, they are one time activations for the level
        if (powerup.PickupType == PowerUpType.DoubleHealth)
        {
            InventoryLimits.MaxHealth = InventoryLimits.STARTING_MAX_HEALTH * 2;
            _player.Health = InventoryLimits.MaxHealth;
        }
        else if (powerup.PickupType == PowerUpType.DoubleArmor)
        {
            InventoryLimits.MaxArmor = InventoryLimits.STARTING_MAX_ARMOR * 2;
            _player.Armor = InventoryLimits.MaxArmor;
        }
        else
        {
            bool hasPowerType = _player.ActivatedPlayerPowerup != PowerUpType.None;
            if (!hasPowerType)
            {
                _player.ActivatedPlayerPowerup = powerup.PickupType;
                var powerLabel = UIManager.Instance.PowerupLabel;
                powerLabel.text = powerup.Name;
                StartCoroutine(WarnPowerUpTimer(powerLabel, (powerup.EffectTimer - 5f), 5f));
                StartCoroutine(StopPowerupTimer(powerup.EffectTimer));         
            }
        }

        powerup.Consume();
    }

    private IEnumerator WarnPowerUpTimer(TMP_Text text,float time, float flashDuration)
    {
        yield return new WaitForSeconds(time);
        float startTime = Time.time;
        float flashSpeed = 1f;

        while (Time.time - startTime < flashDuration)
        {            
            text.alpha = Mathf.Lerp(0f, 1f, Mathf.PingPong(Time.time * flashSpeed, 1f));            
            yield return null;
        }
    }

    private IEnumerator StopPowerupTimer(float time)
    { 
        yield return new WaitForSeconds(time);
        _player.ActivatedPlayerPowerup = PowerUpType.None;
        UIManager.Instance.PowerupLabel.text = "";
    }  

    private void PlayerHealOverTime()
    {
        Debug.Log("Heal over time for amount " + _healRepeatAmount);
        //Stop overheal
        if ((_player.Health + _healRepeatAmount) >= InventoryLimits.MaxHealth)
        {
            _player.Heal(InventoryLimits.MaxHealth);
            CancelInvoke("PlayerHealOverTime");
            return;
        }
        _player.Heal(_healRepeatAmount);
        
        //maybe play heal effect here
        _healRepeatCount++;

        if (_healRepeatCount >= _healRepeatLimit)
        {
            _healRepeatAmount = 0;
            _healRepeatCount = 0;
            CancelInvoke("PlayerHealOverTime");            
        }
    }

    private void ChangeWeapon(int weaponslot)
    {
        Weapons[_currentWeaponIndex].gameObject.SetActive(false);
        _currentWeaponIndex = weaponslot;
        //_currentWeaponIndex = (_currentWeaponIndex + weaponslot + Weapons.Length) % Weapons.Length;       
        if (Weapons[_currentWeaponIndex].FoundPickup)
        {
            Weapons[_currentWeaponIndex].gameObject.SetActive(true);
        }
        CurrentWeapon = Weapons[_currentWeaponIndex].gameObject.activeSelf ? Weapons[_currentWeaponIndex] : null;
    }


}
