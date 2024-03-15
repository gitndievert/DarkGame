using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ArmorType
{
    None = 0,
    Head,
    Chest
}
public class Armor : BaseEntity
{
    public ArmorType ArmorType;
    public int ProjectionAmount;
}

