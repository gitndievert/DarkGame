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

public static class Tags
{
    public static LayerMask enemyLayer => LayerMask.NameToLayer("Enemy");
    public static LayerMask playerLayer => LayerMask.NameToLayer("Player");

    public const string DEFAULT_TAG = "Untagged";

    public const string AMMO_TAG = "Ammo";
    public const string ARMOR_TAG = "Armor";
    public const string POWERUP_TAG = "Powerup";
    public const string WEAPON_TAG = "Weapon";
    public const string ACTION_TAG = "Action";
    public const string HEALTH_TAG = "Health";
    public const string PROJECTILE_TAG = "Projectile";
    public const string ENEMY_TAG = "Enemy";
    public const string PLAYER_TAG = "Player";
}
