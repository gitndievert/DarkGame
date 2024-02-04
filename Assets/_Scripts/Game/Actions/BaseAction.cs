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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
abstract public class BaseAction : BaseEntity
{    
    public bool RequiresPlayerOpen = true;
    public bool SwitchOnly = false;    

    protected BoxCollider boxCollider;

    protected void Awake()
    {        
        boxCollider = GetComponent<BoxCollider>();
    }

    protected override void Start()
    {
        base.Start();
        gameObject.tag = Tags.ACTION_TAG;        
        if(SwitchOnly)
        {
            RequiresPlayerOpen = true;
        }
            
    }

    public abstract void DoAction();

}