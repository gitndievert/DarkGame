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
using UnityEngine;

public class DoorBasic : BaseAction
{
    public DoorDirection DoorDirection = DoorDirection.Up;
    public bool StayOpen = false;    

    public float DoorSpeed = 3f;
    public float CloseSpeed = 1f;
    public float DoorDelay = 1f;
    public float SlideDistance = 5f;

    public AudioClip DoorOpenSound;
    public AudioClip DoorCloseSound;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool _openingState = false;

    //Setup Coroutines to open and close

    public override void DoAction()
    {
        if (_openingState) return;
        Debug.Log("Opening Door");
        // Store the original position of the door
        originalPosition = transform.position;

        // Calculate the target position for sliding up        
        targetPosition = originalPosition + SlideDirection(DoorDirection) * SlideDistance;        
        // Start the sliding coroutine        
        StartCoroutine(SlideDoor());
    }   
    
    protected Vector3 SlideDirection(DoorDirection direction)
    {
        switch(direction)
        {
            case DoorDirection.Up:
                return Vector3.up;
            case DoorDirection.Down:
                return Vector3.down;
            case DoorDirection.Right:
                return Vector3.forward;
            case DoorDirection.Left:
                return Vector3.back;
            default:
                return Vector3.up;
        }
    }

    protected virtual IEnumerator SlideDoor()
    {        
        //boxCollider.enabled = false; Removed for now
        // Slide the door up
        SoundManager.PlaySound(DoorOpenSound);
        _openingState = true;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * DoorSpeed);
            yield return null;
        }

        // Wait for the specified delay time
        yield return new WaitForSeconds(DoorDelay);

        if (!StayOpen)
        {
            SoundManager.PlaySound(DoorCloseSound);
            // Return the door down slowly
            while (Vector3.Distance(transform.position, originalPosition) > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * CloseSpeed);
                yield return null;
            }
        }
        _openingState = false;
        //boxCollider.enabled = true; removed for now
    }


}

public enum DoorDirection
{
    Up,
    Down,
    Left,
    Right
}