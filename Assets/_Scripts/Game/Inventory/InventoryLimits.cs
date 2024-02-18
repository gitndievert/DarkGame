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

public static class InventoryLimits
{
    public const int STARTING_MAX_HEALTH = 100;
    public const int STARTING_MAX_ARMOR = 100;

    public static int MaxHealth = STARTING_MAX_HEALTH;
    public static int MaxArmor = STARTING_MAX_ARMOR;

    public static int GetAmmoLimit(this AmmoType weapon)
    {
        switch(weapon)
        {
            case AmmoType.None:
                return 0;
            case AmmoType.Bullet:
                return 300;
            case AmmoType.Shell:
                return 48;
            case AmmoType.Rocket:
                return 50;
            case AmmoType.IonizedOrbs:
                return 200;
            case AmmoType.Acid:
                return 100;
            default: 
                return 50;
        }
    }

}
