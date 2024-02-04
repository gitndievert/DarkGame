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

public class Map : MonoBehaviour
{
    [Tooltip("This is the link to the Scene Name")]
    public string SceneName;
    [Header("Campaign Header")]
    public int CampaignId;
    public int Mapid;
    [Header("Map Information")]    
    public string MapName;
    public string MapDescription;
    public AudioClip MapMusic;
}
