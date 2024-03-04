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

public class WalkBob : MonoBehaviour
{
    public float BobbingSpeed = 0.1f;
    public float BobbingAmount = 0.1f;
    public float Midpoint = 1.5f;

    private float _timer = 0.0f;

    private void Update()
    {
        if (GameManager.Instance.GamePaused) return;
        if (GameManager.Instance.MyPlayer.IsFiringWweapon) return;
        float waveslice = 0.0f;

        if (Mathf.Abs(Input.GetAxis("Horizontal")) == 0 && Mathf.Abs(Input.GetAxis("Vertical")) == 0)
        {
            _timer = 0.0f;
        }
        else
        {
            waveslice = Mathf.Sin(_timer);
            _timer += BobbingSpeed;

            if (_timer > Mathf.PI * 2)
            {
                _timer = _timer - (Mathf.PI * 2);
            }
        }

        if (waveslice != 0)
        {
            float finalBob = BobbingAmount;
            if(GameManager.Instance.MyPlayer.PlayerController.IsRunning)
            {
                finalBob = BobbingAmount * 2;
            }
            float translateChange = waveslice * finalBob;
            float totalAxes = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
            totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
            translateChange = totalAxes * translateChange;

            Vector3 localPosition = transform.localPosition;
            localPosition.y = Midpoint + translateChange;
            transform.localPosition = localPosition;
        }
        else
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.y = Midpoint;
            transform.localPosition = localPosition;
        }
    }
}
