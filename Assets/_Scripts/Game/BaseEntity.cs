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
using UnityEngine.SceneManagement;

abstract public class BaseEntity : MonoBehaviour
{
    public string Name;
    public string Description;

    protected const int LEFT_MOUSE = 0;
    protected const int RIGHT_MOUSE = 1;
    protected const int MOUSE_WHEEL = 2;

    public string GetTag
    {
        get { return tag; }
    }
        
    protected virtual void Start()
    {
        
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Method called when a scene is loaded
    protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

}
