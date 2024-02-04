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
using Dark.Utility;
using Dark.Utility.Sound;
using Dark.Settings;
using UnityEngine.SceneManagement;

public class GameManager : PSingle<GameManager>
{
    public GameObject CurrentPlayer;    
    public Player MyPlayer;    

    protected override void PAwake()
    {
        base.PAwake();
        var soundManager = new GameObject("SoundManager");
        soundManager.AddComponent<SoundManager>();
        soundManager.AddComponent<AudioSource>();
        var player = Instantiate(CurrentPlayer);
        //Add spawn and save logic here       
        MyPlayer = player.GetComponent<Player>();
        MyPlayer.transform.position = Vector3.zero;
    }    

    private void Update()
    {
        //TODO: Replace all the actions below in another component

        if(Input.GetKeyDown(KeyBinds.RELOAD_SCENE))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        /*if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("Start",LoadSceneMode.Single);
        }*/
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }
    }
    

    
}
