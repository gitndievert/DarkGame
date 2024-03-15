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
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;


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
        public List<PickupSceneData> SavedPickupData;
        public List<EnemySceneData> SavedEnemyData;        
    }

    [System.Serializable]
    public class AmmoData
    {
        public string Ammo;
        public int Amount;
    }

    [System.Serializable]
    public class EnemySceneData
    {
        public string EnemyType;
        public float X_Position;
        public float Y_Position;
        public float Z_Position;
    }

    [System.Serializable]
    public class PickupSceneData
    {
        public string PickupType;
        public float X_Position;
        public float Y_Position;
        public float Z_Position;
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

    public GameObjectData CreateNewObjectData()
    {
        return new GameObjectData();
    }

    public void SaveGame(string savename)
    {              
        StartCoroutine(SaveRoutine(savename));        
    }

    public void QuickSave()
    {
        var dt = DateTime.Now;
        string saveName = "QuickSave " + dt.ToShortTimeString();
        StartCoroutine(SaveRoutine(saveName));
    }
       
    private IEnumerator SaveRoutine(string savename)
    {
        try
        {
            string saveId = Guid.NewGuid().ToString();
            
            if (EnemyCollection == null)
            {
                EnemyCollection = GameObject.Find("EnemyCollection").transform;
            }
            if (PickupCollection == null)
            {
                PickupCollection = GameObject.Find("PickupCollection").transform;
            }

            //General Data
            var gameObjectData = CreateNewObjectData();
            gameObjectData.SaveName = savename.Trim();
            var dt = DateTime.Now;
            gameObjectData.Date = dt.ToShortDateString();
            gameObjectData.Time = dt.ToShortTimeString();
            gameObjectData.Id = saveId;

            var screenshot = GameScreenshot();            
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
                Armor = player.Armor,
                CurrentWeaponIndex = player.PlayerInventory.CurrentWeaponIndex,
                Weapons = currentWeapons,
                Ammo = currentAmmo
            };           
            List<EnemySceneData> currentEnemies = new();
            foreach (Transform tEnemy in EnemyCollection)
            {                
                var enemy = tEnemy.GetComponent<Enemy>();
                if(enemy == null) continue;
                currentEnemies.Add(new EnemySceneData
                {
                    EnemyType = enemy.EnemyType.ToString(),
                    X_Position = transform.position.x,
                    Y_Position = transform.position.y,
                    Z_Position = transform.position.z
                });
            }
            List<PickupSceneData> currentPickups = new();
            foreach (Transform tPickup in PickupCollection)
            {                
                var pickup = tPickup.GetComponent<IPickupable>();
                if(pickup == null) continue;
                currentPickups.Add(new PickupSceneData
                {
                    PickupType = pickup.GetType().ToString(),
                    X_Position = transform.position.x,
                    Y_Position = transform.position.y,
                    Z_Position = transform.position.z
                });
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
            Debug.LogError("Error writing file: " + e.StackTrace);
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
                    SaveName = gameObjectData.SaveName                   
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
                    //SceneManager.LoadSceneAsync(gameObjectData.LevelData.Level);
                    var player = GameManager.Instance.MyPlayer;
                    player.Health = gameObjectData.PlayerData.Health;
                    player.Armor = gameObjectData.PlayerData.Armor;
                    player.transform.position = new Vector3(gameObjectData.PlayerData.X_Position,
                        gameObjectData.PlayerData.Y_Position,
                        gameObjectData.PlayerData.Z_Position);


                    //Set Enemy Instances
                    var enemyList = gameObjectData.LevelData.SavedEnemyData;
                    //Set Pickup Instances
                    var pickupList = gameObjectData.LevelData.SavedPickupData;

                    Debug.Log("File loaded: " + path);
                    StartCoroutine(TextAlert("Loading ...", 2f));
                    break;
                }
            }
            
        }
        catch(Exception e)
        {
            Debug.LogError("Error loading file: " + e.Message);
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

    private Texture2D GameScreenshot()
    {        
        UIManager.Instance.HideMainMenu();     
        
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        Camera.main.targetTexture = renderTexture;
        Camera.main.Render();
        
        Texture2D texture = new (Screen.width, Screen.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();
        
        Camera.main.targetTexture = null;
        RenderTexture.active = currentRT;
        Destroy(renderTexture);
        UIManager.Instance.ShowMainMenu();

        return texture;
    }

}