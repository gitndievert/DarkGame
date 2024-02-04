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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenEffects : MonoBehaviour
{
    public Image FlashImage;    
    public Color DamageFlashColor = new Color(1f, 0f, 0f, 0.7f);
    public Color FlashColor = new Color(1f, 1f, 1f, 0.7f);
    public Color HealFlashColor = new Color(100f, 255f, 67f, 0.7f);
    public float FlashSpeed = 5f;    
    public Image FadeImage;
    public float FadeSpeed = 3f;    

    public void Flash()
    {
        StartCoroutine(FlashCoroutine(FlashColor));
    }

    public void DamageFlash()
    {
        StartCoroutine(FlashCoroutine(DamageFlashColor));
    }

    public void HealFlash()
    {
        StartCoroutine(FlashCoroutine(HealFlashColor));
    }

    public void Fade()
    {
        if (FadeImage == null) return;
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string scenename, string levelname)
    {
        UIManager.Instance.LevelNameLabel.text = levelname;
        StartCoroutine(FadeIn());
        StartCoroutine(FadeOut(scenename));
    }

    public void FadeOutScene(string scenename)
    {
        if (FadeImage == null) return;
        StartCoroutine(FadeOut(scenename));
    }

    private IEnumerator FadeIn()
    {
        float alpha = 1f;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * FadeSpeed;
            FadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeOut(string sceneName)
    {
        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * FadeSpeed;
            FadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        UIManager.Instance.LevelNameLabel.text = "";
        SceneManager.LoadScene(sceneName);
    }


    private IEnumerator FlashCoroutine(Color flashColor)
    {
        float alpha = flashColor.a;

        while (alpha > 0f)
        {
            FlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            alpha -= FlashSpeed * Time.deltaTime;
            yield return null;
        }

        FlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }

}