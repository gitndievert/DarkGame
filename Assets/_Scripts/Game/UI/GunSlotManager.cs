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


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunSlotManager : MonoBehaviour
{

    private Image[] _gunSlotImages;
    private Color ActiveFuncColor => new(1f, 1f, 1f, 1f);
    private Color SelectedFuncColor => new(1f, 0.92f, 0.016f, 1f);

    private readonly HashSet<int> _activeIndex = new();

    private void Awake()
    {
        _gunSlotImages = GetComponentsInChildren<Image>();        
    }

    public void ActivateGunSlot(int slotNumber)
    {
        foreach (Image slot in _gunSlotImages)
        {
            bool isSelected = slot.name == slotNumber.ToString();
            if (isSelected)
            {
                slot.color = ActiveFuncColor;
                _activeIndex.Add(slotNumber);
                return;
            }
        }
    }

    public void SelectGunSlot(int slotNumber)
    {
        if (!_activeIndex.Contains(slotNumber)) return;        
        foreach (Image slot in _gunSlotImages)
        {
            int.TryParse(slot.name, out int slotValue);
            bool isSelected = slotValue == slotNumber;
            if (!_activeIndex.Contains(slotValue)) continue;
            if (isSelected)
            {
                slot.rectTransform.sizeDelta = new Vector2(40f, 40f);
                slot.color = SelectedFuncColor;
            }
            else
            {
                slot.rectTransform.sizeDelta = new Vector2(25f, 25f);
                slot.color = ActiveFuncColor;
            }
        }
        
    }       

}
