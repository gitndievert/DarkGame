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
using UnityEngine.UI;

public class GunSlot : MonoBehaviour
{
    public int SlotIndex;
    public bool ActiveSlot = false;
    public Image GunSlotImage;

    private Color ActiveFuncColor => new (1f, 1f, 1f, 1f);
    private Color SelectedFuncColor => new (1f, 0.92f, 0.016f, 1f);
    private Vector2 GunSlotDim;
    private Vector2 GunSlotSelectedDim;

    private void Start()
    {
        GunSlotImage = GetComponent<Image>();
        int.TryParse(transform.name, out int numberName);
        SlotIndex = numberName;
        GunSlotDim = GunSlotImage.rectTransform.sizeDelta;
        GunSlotSelectedDim = new Vector2(GunSlotDim.x + 15f, GunSlotDim.y + 15f);        
    }

    public void ActivateGunSlot()
    {
        ActiveSlot = true;
        SetColor(ActiveFuncColor);
    }

    public void SelectSlotState(bool selected)
    {
        if (!ActiveSlot) return;
        if(selected)
        {            
            SetColor(SelectedFuncColor);
            SetImageSize(GunSlotSelectedDim);
        }
        else
        {   
            SetColor(ActiveFuncColor);
            SetImageSize(GunSlotDim);
        }        
    }    

    //COME BACK LATER, ADD REALTIME AMMO COUNTERS UNDER THE SLOT!

    public void SetColor(Color color)
    {
        if (GunSlotImage == null) return;
        GunSlotImage.color = color;
    }

    public void SetImageSize(Vector2 size)
    {
        if (GunSlotImage == null) return;
        GunSlotImage.rectTransform.sizeDelta = size;        
    }

    public void SetImageSize(int width, int height)
    {
        if (GunSlotImage == null) return;        
        GunSlotImage.rectTransform.sizeDelta = new Vector2(width, height);        
    }

}
