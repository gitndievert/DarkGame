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
using System;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    public SpriteSequence[] SpriteList;
        
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _parentSprite;

    private float animationStartTime;
    private float frameRate;
    private Sprite[] currentSequence;

    private void Awake()
    {        
        _spriteRenderer = GetComponent<SpriteRenderer>();        
    }

    public bool HasPlayList
    {
        get
        {
            return SpriteList.Length > 0;
        }
    }

    public void Play(string sequence)
    {
        _parentSprite = transform.parent.GetComponent<SpriteRenderer>();
        IEnumerable<SpriteSequence> spriteSet = SpriteList.Where(x => x.SequenceType.ToLower() == sequence.Trim().ToLower());
        currentSequence = spriteSet.Select(x => x.Sprites).FirstOrDefault();
        frameRate = spriteSet.Select(x => x.FrameRate).FirstOrDefault();

        if (currentSequence != null && frameRate > 0)
        {
            if (_parentSprite != null)
            {
                _parentSprite.enabled = false;
            }
            _spriteRenderer.enabled = true;

            // Set the animation start time
            animationStartTime = Time.time;

            // Update the sprite renderer with the first sprite
            _spriteRenderer.sprite = currentSequence[0];

            // Update the sprite renderer in the Update loop
            enabled = true;
        }
        else
        {
            Debug.LogError("Sequence not found: " + sequence);
        }
    }

    private void Update()
    {
        if (currentSequence == null || frameRate <= 0)
        {
            // If there is no current sequence or frameRate is invalid, disable the update loop
            enabled = false;
            return;
        }

        // Calculate the elapsed time since the animation started
        float elapsedTime = Time.time - animationStartTime;

        // Calculate the current frame index based on the elapsed time
        int currentFrame = Mathf.FloorToInt(elapsedTime * frameRate);

        // Check if it's time to end the animation
        if (currentFrame >= currentSequence.Length)
        {
            if (_parentSprite != null)
            {
                _parentSprite.enabled = true;
            }
            _spriteRenderer.enabled = false;
            enabled = false; // Disable the update loop
            return;
        }

        // Update the sprite renderer with the current sprite
        _spriteRenderer.sprite = currentSequence[currentFrame];
    }
}

[Serializable]
public class SpriteSequence
{
    public float FrameRate;
    public string SequenceType;
    public Sprite[] Sprites;
}
