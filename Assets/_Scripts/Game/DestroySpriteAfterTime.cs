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

[RequireComponent(typeof(SpriteRenderer))]
public class DestroySpriteAfterTime : DestroyAfterTime
{
    public bool TimeIsFade = true;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;    
    
    // Start is called before the first frame update
    protected override void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (TimeIsFade)
        {
            StartCoroutine(FadeSprite());
        }
        else
        {
            base.Start();
        }
    }    

    protected IEnumerator FadeSprite()
    {        
        float elapsedTime = 0f;
        Color startColor = _spriteRenderer.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        while (elapsedTime < TimeToDestroy)
        {
            _spriteRenderer.color = Color.Lerp(startColor, targetColor, elapsedTime / TimeToDestroy);
            elapsedTime += Time.deltaTime;
            yield return null;
        }        
        _spriteRenderer.color = targetColor;        
        Destroy(gameObject);
    }

}
