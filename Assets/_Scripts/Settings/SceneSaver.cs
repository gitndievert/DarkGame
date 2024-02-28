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
        public List<string> SavedPickupData;
        public List<string> SavedEnemyData;
        public List<ExistingEnemyData> ExistingEnemyData;
    }


    [System.Serializable]
    public class AmmoData
    {
        public string Ammo;
        public int Amount;
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
        public List<string> Weapons;
        public List<AmmoData> Ammo;
        public int CurrentWeaponIndex;
    }

    public Transform EnemyCollection;
    public Transform PickupCollection;          

    private readonly List<string> _enemyCollection = new();
    private readonly List<string> _pickupCollection = new();    

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
        StartCoroutine(SaveRoutine(savename));        
    }
       
    private IEnumerator SaveRoutine(string savename)
    {
        try
        {
            //General Data
            var gameObjectData = CreateNewObjectData();
            gameObjectData.SaveName = savename;
            var dt = System.DateTime.Now;
            gameObjectData.Date = dt.ToShortDateString();
            gameObjectData.Time = dt.ToShortTimeString();

            //Player Data
            Player player = GameManager.Instance.MyPlayer;
            List<string> currentWeapons = new();
            foreach (var weapon in player.PlayerInventory.Weapons)
            {
                if (!weapon.FoundPickup) continue;
                currentWeapons.Add(weapon.WeaponType.ToString());
            }
            List<AmmoData> currentAmmo = new();
            foreach(var ammo in player.PlayerInventory.PlayerAmmo)
            {
                currentAmmo.Add(new AmmoData
                {
                    Ammo = ammo.AmmoType.ToString(),
                    Amount = ammo.Amount
                });
            }            
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
                Weapons = currentWeapons,
                Ammo = currentAmmo
            };
            /*List<ExistingEnemyData> eData = new();
            foreach (Transform tEnemy in EnemyCollection)
            {
                eData.Add(new ExistingEnemyData
                {
                    ObjName = tEnemy.name,
                    X_Position = tEnemy.transform.position.x,
                    Y_Position = tEnemy.transform.position.y,
                    Z_Position = tEnemy.transform.position.z,
                    Health = tEnemy.GetComponent<Enemy>().Health              
                });
            }*/
            List<string> currentEnemies = new();
            foreach (Transform tEnemy in EnemyCollection)
            {
                currentEnemies.Add(tEnemy.gameObject.name);
            }
            List<string> currentPickups = new();
            foreach (Transform tPickup in PickupCollection)
            {
                currentPickups.Add(tPickup.gameObject.name);
            }
            gameObjectData.LevelData = new LevelData
            {
                Level = SceneSwapper.Instance.CurrentLoadedSceneName,
                SavedPickupData = currentPickups,
                SavedEnemyData = currentEnemies
                //ExistingEnemyData = eData
            };

            string json = JsonConvert.SerializeObject(gameObjectData);
            string path = FilePath;
            File.WriteAllText(path, json);
            Debug.Log("File written to: " + path);
            StartCoroutine(TextAlert("Saved ...", 2f));
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing file: " + e.Message);
        }
        
        yield return null;

    }

    //TODO: Okay so each level save need to be unique, so will need to put a unique id in there somewhere
    /*public void LoadGame(int loadslot)
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
    }*/

    private string FilePath => Path.Combine(Application.persistentDataPath, "savedata.json");

    private void ProcessEnemyCollection()
    {
        //async?
        if (EnemyCollection == null)
        {
            EnemyCollection = GameObject.Find("EnemyCollection").transform;            
        }
        _enemyCollection.Clear();
        foreach (Transform tEnemy in EnemyCollection)
        {
            _enemyCollection.Add(tEnemy.gameObject.name);
        }            
        
    }
    
    private void ProcessPickupCollection()
    {
        if (PickupCollection == null)
        {
            PickupCollection = GameObject.Find("PickupCollection").transform;
        }
        _pickupCollection.Clear();
        foreach (Transform tPickup in PickupCollection)
        {
            _pickupCollection.Add(tPickup.gameObject.name);
        }        
    }
    private IEnumerator TextAlert(string text, float seconds)
    {
        UIManager.Instance.SecretText.text = text;
        yield return new WaitForSeconds(seconds);
        UIManager.Instance.SecretText.text = "";
    }

}