using System.Collections.Generic;
using UnityEngine;

public class LootChance : MonoBehaviour
{
    public Loot loots;

    public void Drop()
    {
        if (loots != null && loots.loots.Count > 0)
        {
            GameObject lootItem = GetRandomLootItem();
            if (lootItem != null)
            {
                Instantiate(lootItem, transform.position, Quaternion.identity);
            }
        }
    }

    private GameObject GetRandomLootItem()
    {
        List<GameObject> lootPool = new List<GameObject>();

        // Add each loot item to the pool according to its chance
        foreach (LootContainer lootContainer in loots.loots)
        {
            for (int i = 0; i < lootContainer.lootChance; i++)
            {
                lootPool.Add(lootContainer.item);
            }
        }

        // If the loot pool is empty, return null
        if (lootPool.Count == 0)
        {
            return null;
        }

        // Pick a random item from the pool
        int randomIndex = Random.Range(0 + (int)(PlayerManager.instance.dropChance * 100 / lootPool.Count), lootPool.Count);
        return lootPool[randomIndex];
    }
}