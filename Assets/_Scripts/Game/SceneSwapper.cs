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
using Dark.Utility;
using UnityEngine.SceneManagement;
using System.Linq;

[RequireComponent(typeof(SceneSaver))]
public class SceneSwapper : PSingle<SceneSwapper>
{
    public string CurrentLoadedSceneName;
    
    public bool IsMainMenu = false;
    public bool DoFadeToScene = true;

    public Transform Campaign1;
    public Transform Campaign2;
    public Transform Campaign3;
    public Transform Campaign4;

    [Tooltip("Currently Loaded Map")]
    public Map LoadedMap;
    [HideInInspector]
    public SceneSaver SceneSaver;

    private List<Map> _campaign1 = null;
    private List<Map> _campaign2 = null;
    private List<Map> _campaign3 = null;
    private List<Map> _campaign4 = null;    

    protected override void PAwake()
    {
        if(Campaign1 != null)
        {
            _campaign1 = Campaign1.GetComponentsInChildren<Map>().ToList();
        }
        if(Campaign2 != null)
        {
            _campaign2 = Campaign2.GetComponentsInChildren<Map>().ToList();
        }
        if(Campaign3 != null)
        {
            _campaign3 = Campaign3.GetComponentsInChildren<Map>().ToList();
        }
        if(Campaign4 != null)
        {
            _campaign4 = Campaign4.GetComponentsInChildren<Map>().ToList();
        }
        SceneSaver = GetComponent<SceneSaver>();
    }

    private void Start()
    {
        if(IsMainMenu && MusicManager.Instance.MusicTracks != null)
        {
            MusicManager.Instance.StartMusic(0);
        }
    }

    public void Save()
    {
        SceneSaver.SaveGame("This is my test save");
    }

    public void Load()
    {

    }

    public void StartScene(int campaignId, int mapId)
    {   
        List<Map> maplist = null;
        switch(campaignId)
        {
            case 1:
                maplist = _campaign1;
                break;
            case 2: 
                maplist = _campaign2;
                break;
            case 3:
                maplist = _campaign3;
                break;
            case 4:
                maplist = _campaign4;
                break;
        }
        if(maplist != null)
        {
            LoadedMap = maplist.Where(x => x.Mapid == mapId).FirstOrDefault();            
            string sceneName = LoadedMap.SceneName.Trim();
            CurrentLoadedSceneName = sceneName;
            SceneManager.LoadScene(sceneName);
            //Reset the player to 0
            var player = FindSinglePlayer();
            if(player)
            {
                player.transform.position = Vector3.zero;
            }
            MusicManager.Instance.StartMusic(LoadedMap.MapMusic);
            //Come back later
            /*if (DoFadeToScene && !IsMainMenu)
            {                
                UIManager.Instance.ScreenEffects.FadeToScene(sceneName,LoadedMap.MapName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            } */

        }

    }
  
    public void StartScene(string level)
    {   
        if (level.Length == 4)
        {
            int.TryParse(level.Substring(1, 1), out int campaignId);
            int.TryParse(level.Substring(3, 1), out int mapId);
            StartScene(campaignId, mapId);
        }   
        else
        {
            throw new System.Exception("Invalid Scene Name");
        }
    }    

    public void Quit()
    {
        Application.Quit();
    }

    private GameObject FindSinglePlayer() => GameObject.FindGameObjectWithTag(Tags.PLAYER_TAG);



}
