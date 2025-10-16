using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWeakness : MonoBehaviour
{
    LifeManager parentLifeManager;

    private void Start()
    {
        parentLifeManager = GetComponentInParent<LifeManager>();
    }

    public void TakeDamage(int damage, GameObject attackingEntity, bool isCritical, float knockbackMultiplier = 1)
    {
        parentLifeManager.TakeDamage(damage, attackingEntity, isCritical, knockbackMultiplier = 1);
    }
}
