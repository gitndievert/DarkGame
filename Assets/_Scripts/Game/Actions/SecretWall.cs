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

using Dark.Utility.Sound;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Please note, the piece colliders need to exclude the player layer
/// </summary>
public class SecretWall : BaseAction
{
    const float DESTROY_PIECES = 5f;

    public float ExplosiveForce = 30f;
    //public float ExplosiveRadius = 5f;
    public AudioClip BreakingWall;
    
    private readonly List<Rigidbody> _bodies = new List<Rigidbody>();
    private BoxCollider _parentCollider;
    
    
    protected override void Start()
    {        
        _parentCollider = GetComponent<BoxCollider>();
        base.Start();
        RequiresPlayerOpen = true;
        var rBodies = GetComponentsInChildren<Rigidbody>();
        foreach(var body in rBodies)
        {
            body.isKinematic = true;
            _bodies.Add(body);
        }
    }


    public override void DoAction()
    {
        Debug.Log("You broke down a secret wall!");
        _parentCollider.enabled = false;
        foreach (var body in _bodies)
        {
            body.isKinematic = false;            
            Vector3 explosionDirection = (body.transform.position - GameManager.Instance.CurrentPlayer.transform.position).normalized;
            //rb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
            body.AddForce(explosionDirection * ExplosiveForce, ForceMode.Impulse);
            //body.AddExplosionForce(ExplosiveForce, transform.position, ExplosiveRadius);
        }
        SoundManager.PlaySound(BreakingWall);
        //Destroy Parent
        Destroy(gameObject, DESTROY_PIECES);
    }
    


}
