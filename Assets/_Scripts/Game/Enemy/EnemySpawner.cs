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

public class EnemySpawner : MonoBehaviour
{
    const float SPAWNER_DISTANCE = 25f;
    
    public GameObject[] Enemies;
    public float SpawnTimer;    
    [Range(0,100)]
    public int MinSpawnAmount = 1;
    [Range(0, 100)]
    [Tooltip("If set to > 1 then randoms")]
    public int MaxSpawnAmount = 1;
    public bool StartTimer;

    private bool _spawning;   
       
    private void Update()
    {
        if (Enemies == null) return;
        if(StartTimer && !_spawning)
        {
            _spawning = true;
            InvokeRepeating("DoSpawn", 0, SpawnTimer);
        }
        else if(!StartTimer && _spawning)
        {
            _spawning = false;
            CancelInvoke("DoSpawn");
        }        
    }

    private void DoSpawn()
    {
        //Need to distance check the player
        var player = GameManager.Instance.MyPlayer;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > SPAWNER_DISTANCE) return;
        int mIndex = Enemies.Length > 1 ? Random.Range(0, Enemies.Length - 1) : 0;
        GameObject monster = Instantiate(Enemies[mIndex]);
        monster.transform.position = transform.position;
    }
    
}
