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

using Dark.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : DSingle<UIManager>
{   
    public Canvas Canvas;

    public TMP_Text HealthLabel;
    public TMP_Text ArmorLabel;
    public TMP_Text AmmoLabel;    
    public TMP_Text PowerupLabel;
    public TMP_Text LevelNameLabel;
    public Transform GunSlotLabels;

    public ScreenEffects ScreenEffects { get; private set; }

    private GunSlot[] GunSlotImages;

    protected override void DAwake()
    {
        if (Canvas == null)
            throw new System.Exception("Missing canvas in this scene");

        Canvas.gameObject.SetActive(true);
        GunSlotImages = GunSlotLabels.GetComponentsInChildren<GunSlot>();
        DontDestroyOnLoad(GunSlotLabels.gameObject);
    }

    private void Start()
    {   
        ScreenEffects = GetComponent<ScreenEffects>();        
        var labels = Canvas.GetComponentsInChildren<TMP_Text>();
        if (labels.Length > 0)
        {
            foreach (TMP_Text label in labels)
            {
                switch (label.name)
                {
                    case "Health":
                        HealthLabel = label;
                        break;
                    case "Armor":
                        ArmorLabel = label;
                        break;
                    case "Ammo":
                        AmmoLabel = label;
                        break;
                    case "Powerup":
                        PowerupLabel = label;
                        break;
                    case "LevelName":
                        LevelNameLabel = label;
                        break;
                }
            }
        }
        //Get active image slots
        //GunSlotImages = GunSlotLabels.GetComponentsInChildren<GunSlot>();       
        
    }       

    public void ActivateGunSlot(int slotNumber)
    {
        foreach (GunSlot slot in GunSlotImages)
        {
            if (slotNumber == slot.SlotIndex)
            {
                slot.ActivateGunSlot();
                return;
            }
        }
    }

    public void SelectGunSlot(int slotNumber)
    {
        foreach (GunSlot slot in GunSlotImages)
        {
            bool isSelected = slot.name == slotNumber.ToString();
            slot.SelectSlotState(isSelected);
        }
    }

}
