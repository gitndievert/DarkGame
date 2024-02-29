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
using System.IO;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;


public class SceneSaver : MonoBehaviour
{
    [System.Serializable]
    public class GameObjectData
    {
        public string Id;
        public string SaveName;
        public string Date;
        public string Time;
        public PlayerData PlayerData;
        public LevelData LevelData;        
        public byte[] RawImage;
    }

    [System.Serializable]
    public class LevelData
    {
        public string Level;
        public List<string> SavedPickupData;
        public List<string> SavedEnemyData;        
    }

    [System.Serializable]
    public class AmmoData
    {
        public string Ammo;
        public int Amount;
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
            string saveId = Guid.NewGuid().ToString();

            //General Data
            var gameObjectData = CreateNewObjectData();
            gameObjectData.SaveName = savename;
            var dt = System.DateTime.Now;
            gameObjectData.Date = dt.ToShortDateString();
            gameObjectData.Time = dt.ToShortTimeString();
            gameObjectData.Id = saveId;
            var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            gameObjectData.RawImage = screenshot.EncodeToPNG();

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
            };           
                       
            string path = GetSaveFilePath(saveId);
            SerializeData(gameObjectData, path);            
            Debug.Log("File written to: " + path);
            StartCoroutine(TextAlert("Saved ...", 2f));
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing file: " + e.Message);
        }
        
        yield return null;

    }

    public string GetSaveFilePath(string saveId) => Path.Combine(Application.persistentDataPath, $"{saveId}.sav");
    public string GetSaveImageFilePath(string saveId) => Path.Combine(Application.persistentDataPath, $"{saveId}.png");

    public Texture2D GetImageFromSave(GameObjectData data)
    {        
        Texture2D deserializedTexture = new(2, 2);
        deserializedTexture.LoadImage(data.RawImage);
        return deserializedTexture;
    }

    public List<GameObjectData> ReturnSaveGames()
    {
        List<GameObjectData> data = new();
        foreach (string filePath in Directory.GetFiles(Application.persistentDataPath, "*.sav"))
        {
            string saveId = Path.GetFileNameWithoutExtension(filePath);
            //string image = GetSaveImageFilePath(saveId);
            string gameData = GetSaveFilePath(saveId);
            if (File.Exists(gameData))
            {               
                // Deserialize data
                GameObjectData gameObjectData = DeserializeData<GameObjectData>(gameData);                
                //Conver the texture into a RawImage.texture
                data.Add(new GameObjectData
                {
                    RawImage = gameObjectData.RawImage,
                    Id = gameObjectData.Id,
                    Date = gameObjectData.Date,
                    Time = gameObjectData.Time,
                    SaveName= gameObjectData.SaveName                   
                });                
            }
        }

        return data;
    }

    //TODO: Okay so each level save need to be unique, so will need to put a unique id in there somewhere
    public void LoadGame(string loadId)
    {        
        try
        {
            string path = Application.persistentDataPath;
            GameObjectData gameObjectData = null;
            foreach (string filePath in Directory.GetFiles(path, "*.sav"))
            {
                string file = Path.GetFileNameWithoutExtension(filePath);                
                if (file == loadId)
                {
                    gameObjectData = DeserializeData<GameObjectData>(filePath);                    
                    Debug.Log("File loaded: " + path);
                    StartCoroutine(TextAlert("Loading ...", 2f));
                    break;
                }
            }
            if(gameObjectData != null)
            {
                var blah = gameObjectData;
            }
            else
            {
                Debug.LogError("ERROR: Could not load file " + loadId);
            }            
        }
        catch(Exception e)
        {
            Debug.LogError("Error loading file: " + e.Message);
        }
    } 

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
   
    private void SerializeData<T>(T data, string filePath)
    {
        BinaryFormatter formatter = new();
        FileStream stream = new(filePath, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    private T DeserializeData<T>(string filePath)
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter formatter = new();
            FileStream stream = new(filePath, FileMode.Open);
            T data = (T)formatter.Deserialize(stream);
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
            return default(T);
        }
    }

}