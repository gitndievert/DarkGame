using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    void TakeDamage(int amount);
    Transform AttackTarget { get; }
    string GetTag { get; }
}
