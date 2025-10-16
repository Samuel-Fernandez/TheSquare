using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootContainer
{
    public GameObject item;
    public int lootChance;
}

[CreateAssetMenu(fileName = "Loot", menuName = "Loot/Loot")]
public class Loot : ScriptableObject
{
    [SerializeField]
    public List<LootContainer> loots = new List<LootContainer>();

}