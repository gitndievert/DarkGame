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

public class Elevator : MonoBehaviour
{
    public Transform Floor1;
    public Transform Floor2;
    public float MoveSpeed = 2f;
    public float DelayTime = 2f;
    public AudioClip MoveSound;
    public AudioClip StopSound;

    private Vector3 _targetPosition;
    [SerializeField]
    private bool _isMoving = true;

    private void Start()
    {    
        SetTarget(Floor2.position);
    }   

    private void Update()
    {
        if (_isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, MoveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
            {
                _isMoving = false;
                Invoke("ChangeDirection", DelayTime);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Debug.Log("player on the elevation");
        }        
    }


    private void ChangeDirection()
    {        
        if (_targetPosition == Floor1.position)
        {
            SetTarget(Floor2.position);
        }
        else
        {
            SetTarget(Floor1.position);
        }

        _isMoving = true;
    }

    private void SetTarget(Vector3 target)
    {
        _targetPosition = target;
    }

}
