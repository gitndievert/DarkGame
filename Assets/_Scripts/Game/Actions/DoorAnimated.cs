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
using System.Collections;
using Dark.Utility.Sound;

public class DoorAnimated : BaseAction
{
    public AudioClip DoorOpenSound;
    public AudioClip DoorCloseSound;

    public float DoorDelay = 3f;
    public float DoorSpeed = 0.5f;
    public bool StayOpen = false;

    private Animator _animator;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
    }

    public override void DoAction()
    {
        _animator.SetFloat("speed", DoorSpeed);
        StartCoroutine(SlideDoor());
    }
    protected virtual IEnumerator SlideDoor()
    {
        _animator.SetBool("activate", true);
        if (DoorOpenSound != null)
        {
            SoundManager.PlaySound(DoorOpenSound);
        }
        yield return new WaitForSeconds(DoorDelay);
        if (StayOpen) yield break;
        _animator.SetBool("activate", false);
        if (DoorCloseSound != null)
        {
            SoundManager.PlaySound(DoorCloseSound);
        }
    }
}
