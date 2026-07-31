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

    [Header("Information")]
    [Tooltip("Chances restantes sur 10000 (chances de ne rien obtenir). Calculé automatiquement.")]
    [SerializeField]
    private int placesRestantes = 10000;

    private void OnValidate()
    {
        int total = 0;
        if (loots != null)
        {
            foreach (var loot in loots)
            {
                total += loot.lootChance;
            }
        }
        placesRestantes = 10000 - total;
    }
}