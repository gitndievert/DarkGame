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
using Dark.Utility.Sound;

public class SecretArea : BaseAction
{
    const float TEXT_TIMER = 2f;
    
    public string SecretMessage = "You discovered a secret area!";
    public AudioClip SecretDiscoverySound;

    private bool _isAction = false;

    protected override void Start()
    {
        base.Start();
        RequiresPlayerOpen = false;
        boxCollider.isTrigger = true;
    }

    public override void DoAction()
    {
        if (_isAction) return;
        _isAction = true;
        if(!string.IsNullOrEmpty(SecretMessage))
        {
            if(SecretDiscoverySound != null)
            {
                SoundManager.PlaySound(SecretDiscoverySound);
            }
            
            UIManager.Instance.SecretText.text = SecretMessage.Trim();            
        }

        Destroy(gameObject, TEXT_TIMER);
        
    }

    private void OnDestroy()
    {
        UIManager.Instance.SecretText.text = "";
    }


}
