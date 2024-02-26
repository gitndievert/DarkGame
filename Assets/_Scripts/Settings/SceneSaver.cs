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

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System;

public class SceneSaver : MonoBehaviour
{
    [System.Serializable]
    public class GameObjectData
    {        
        public string SaveName;
        public string Date;
        public string Time;
        public PlayerData PlayerData;
        public LevelData LevelData;
    }

    [System.Serializable]
    public class LevelData
    {
        public string Level;
        public SubPickupData[] SubPickupData;
        public SubEnemyData[] SubEnemyData;
        public ExistingEnemyData[] ExistingEnemyData;
    }

    [System.Serializable]
    public class SubPickupData
    {
        public string ObjName;
    }

    [System.Serializable]
    public class SubEnemyData
    {
        public string ObjName;
    }

    [System.Serializable]
    public class ExistingEnemyData
    {
        public string ObjName;
        public float X_Position;
        public float Y_Position;
        public float Z_Position;
        public int Health;
    }

    [System.Serializable]
    public class PlayerData
    {
        public int Health;
        public int Armor;        
        public float X_Position;
        public float Y_Position;
        public float Z_Position;
        public float X_Rotation;
        public float Y_Rotation;
        public float Z_Rotation;
        //Inventory
        //public Weapon[] Weapons;
        //public Ammo[] Ammo;
        public int CurrentWeaponIndex;
    }

    public Transform EnemyCollection;
    public Transform PickupCollection;          

    private readonly List<Enemy> _enemyCollection = new();
    private readonly List<PickupAmmo> _pickupAmmoCollection = new();
    private readonly List<PickupWeapon> _pickupWeaponCollection = new();
    private readonly List<PickupHealth> _pickupHealthCollection = new();
    private readonly List<PickupArmor> _pickupArmorCollection = new();
    private readonly List<PickupPowerup> _pickupPowerupCollection = new();

    private void Awake()
    {
        //Initialize Collections for level
        ProcessEnemyCollection();
        ProcessPickupCollection();
    }

    public GameObjectData CreateNewObjectData()
    {
        return new GameObjectData();
    }

    public void SaveGame(string savename)
    {
        //General Data
        var gameObjectData = CreateNewObjectData();
        gameObjectData.SaveName = savename;
        var dt = System.DateTime.Now;
        gameObjectData.Date = dt.ToShortDateString();
        gameObjectData.Time = dt.ToShortTimeString();
        
        //Player Data
        Player player = GameManager.Instance.MyPlayer;
        gameObjectData.PlayerData = new PlayerData
        {
            X_Position = player.transform.position.x,
            Y_Position = player.transform.position.y,
            Z_Position = player.transform.position.z,
            X_Rotation = player.transform.rotation.x,
            Y_Rotation = player.transform.rotation.y,
            Z_Rotation = player.transform.rotation.z,
            Health = player.Health,
            CurrentWeaponIndex = player.PlayerInventory.CurrentWeaponIndex,
            //Weapons = player.PlayerInventory.Weapons,
            //Ammo = player.PlayerInventory.PlayerAmmo
        };
        gameObjectData.LevelData = new LevelData
        {
            Level = SceneSwapper.Instance.CurrentLoadedSceneName
        };        
        SaveGame(gameObjectData);        
    }

    public void SaveGame(GameObjectData saveData)
    {        
        try
        {
            string json = JsonConvert.SerializeObject(saveData);
            string path = FilePath;
            File.WriteAllText(path, json);
            Debug.Log("File written to: " + path);
            StartCoroutine(TextAlert("Saved ...", 2f));
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing file: " + e.Message);
        }        
    }    

    //TODO: Okay so each level save need to be unique, so will need to put a unique id in there somewhere
    public void LoadGame(int loadslot)
    {        
        try
        {
            string path = FilePath;
            string json = File.ReadAllText(path);
            var loadData = JsonConvert.DeserializeObject<GameObjectData>(json);
            Debug.Log("File loaded: " + path);
            StartCoroutine(TextAlert("Loading ...", 2f));
        }
        catch(Exception e)
        {
            Debug.LogError("Error loading file: " + e.Message);
        }
    }

    private string FilePath => Path.Combine(Application.persistentDataPath, "savedata.json");

    private void ProcessEnemyCollection()
    {
        //async?
        if (EnemyCollection == null)
        {
            EnemyCollection = GameObject.Find("EnemyCollection").transform;            
        }        
        ClearEnemyCollection();
        foreach (Transform tEnemy in EnemyCollection)
        {
            _enemyCollection.Add(tEnemy.GetComponent<Enemy>());
        }            
        
    }

    private IEnumerator TextAlert(string text, float seconds)
    {
        UIManager.Instance.SecretText.text = text;
        yield return new WaitForSeconds(seconds);
        UIManager.Instance.SecretText.text = "";
    }

    private void ProcessPickupCollection()
    {
        if (PickupCollection == null)
        {
            PickupCollection = GameObject.Find("PickupCollection").transform;
        }       
        ClearPickupCollection();
        foreach (Transform tPickup in PickupCollection)
        {
            tPickup.TryGetComponent(out PickupAmmo ammo);
            if (ammo != null)
            {
                _pickupAmmoCollection.Add(ammo);
                break;
            }
            tPickup.TryGetComponent(out PickupWeapon weapon);
            if (weapon != null)
            {
                _pickupWeaponCollection.Add(weapon);
                break;
            }
            tPickup.TryGetComponent(out PickupHealth health);
            if (health != null)
            {
                _pickupHealthCollection.Add(health);
                break;
            }
            tPickup.TryGetComponent(out PickupArmor armor);
            if (armor != null)
            {
                _pickupArmorCollection.Add(armor);
                break;
            }
            tPickup.TryGetComponent(out PickupPowerup powerup);
            if (powerup != null)
            {
                _pickupPowerupCollection.Add(powerup);
                break;
            }
        }        
    }

    private void ClearEnemyCollection()
    {
        _enemyCollection.Clear();
    }

    private void ClearPickupCollection()
    {
        _pickupAmmoCollection.Clear();
        _pickupWeaponCollection.Clear();
        _pickupHealthCollection.Clear();
        _pickupArmorCollection.Clear();
        _pickupPowerupCollection.Clear();
    }
}