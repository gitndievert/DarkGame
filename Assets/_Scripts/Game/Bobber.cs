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

public enum AxisToRotate
{
    X, Y, Z
}
public class Bobber : MonoBehaviour
{
    [Range(0, 3f)]
    public float BobHeight = 0.3f;
    [Range(1f, 60f)]
    public float RotateSpeed = 15f;
    [Range(0, 10f)]
    public float BobSpeed = 2.5f;

    public bool AutoStartBob = false;
    public bool Rotate = false;
    public AxisToRotate rotationAxis = AxisToRotate.Y;

    //private Rigidbody _rb;
    private bool _isBobbing;    
    private float _originY;

    private void Start()
    {
        _originY = transform.position.y;
        if (AutoStartBob)
            StartBob(Rotate);
    }

    private void Update()
    {
        if (_isBobbing)
        {
            if (Rotate)
            {
                float rotationAmount = Time.deltaTime * RotateSpeed;
                Vector3 rotationVector = Vector3.zero;

                switch (rotationAxis)
                {
                    case AxisToRotate.X:
                        rotationVector = Vector3.right;
                        break;
                    case AxisToRotate.Y:
                        rotationVector = Vector3.up;
                        break;
                    case AxisToRotate.Z:
                        rotationVector = Vector3.forward;
                        break;
                }

                transform.Rotate(rotationVector, rotationAmount);                
            }
            float sinY = _originY + Mathf.Sin(Time.time * BobSpeed) * BobHeight;
            transform.position = new Vector3(transform.position.x, sinY, transform.position.z);
        }
    }

    public void StartBob(bool rotate = false)
    {
        Rotate = rotate;
        _isBobbing = true;
    }

    public void StopBob()
    {
        _isBobbing = false;
    }

}