using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Trader Inventory", menuName = "Market/Trader Inventory")]
public class TraderInventory : ScriptableObject
{
    [System.Serializable]
    public class TraderItem
    {
        [Header("Item Configuration")]
        public Item item;

        [Header("Spawn Settings")]
        [Range(0f, 100f)]
        public float spawnChance = 50f; // Pourcentage de chance d'apparaître dans le marché

        [Header("Quantity Settings")]
        public int minQuantity = 1;
        public int maxQuantity = 5;

        [Header("Additional Settings")]
        public bool alwaysAvailable = false; // Si true, cet item apparaîtra toujours
        public int weight = 1; // Poids pour la sélection aléatoire (plus élevé = plus de chances)
    }

    [Header("Trader Configuration")]
    public TraderType traderType;
    public string traderName = ""; // Nom du trader pour identification
    public string description = ""; // Description du trader (zone, niveau, etc.)

    [Header("Market Settings")]
    public int minItemsInMarket = 2;
    public int maxItemsInMarket = 16;

    [Header("Available Items")]
    public List<TraderItem> availableItems = new List<TraderItem>();

    /// <summary>
    /// Génère une liste d'items pour le marché basée sur les configurations
    /// </summary>
    public List<Item> GenerateMarketItems()
    {
        List<Item> marketItems = new List<Item>();

        // Ajouter d'abord les items toujours disponibles
        foreach (var traderItem in availableItems)
        {
            if (traderItem.alwaysAvailable && traderItem.item != null)
            {
                Item clonedItem = ScriptableObjectUtility.Clone(traderItem.item);
                SetupClonedItem(clonedItem, traderItem);
                marketItems.Add(clonedItem);
            }
        }

        // Calculer combien d'items supplémentaires nous devons ajouter
        int targetItemCount = Random.Range(minItemsInMarket, maxItemsInMarket + 1);
        int itemsToAdd = Mathf.Max(0, targetItemCount - marketItems.Count);

        // Créer une liste pondérée des items disponibles (excluant ceux toujours disponibles)
        List<TraderItem> weightedItems = new List<TraderItem>();
        foreach (var traderItem in availableItems)
        {
            if (!traderItem.alwaysAvailable && traderItem.item != null)
            {
                // Vérifier la chance d'apparition
                if (Random.Range(0f, 100f) <= traderItem.spawnChance)
                {
                    // Ajouter l'item selon son poids
                    for (int i = 0; i < traderItem.weight; i++)
                    {
                        weightedItems.Add(traderItem);
                    }
                }
            }
        }

        // Sélectionner aléatoirement les items supplémentaires
        for (int i = 0; i < itemsToAdd && weightedItems.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, weightedItems.Count);
            TraderItem selectedTraderItem = weightedItems[randomIndex];

            Item clonedItem = ScriptableObjectUtility.Clone(selectedTraderItem.item);
            SetupClonedItem(clonedItem, selectedTraderItem);
            marketItems.Add(clonedItem);

            // Optionnel : retirer l'item de la liste pondérée pour éviter les doublons
            // Vous pouvez commenter ces lignes si vous voulez autoriser les doublons
            weightedItems.RemoveAll(x => x == selectedTraderItem);
        }

        return marketItems;
    }

    /// <summary>
    /// Configure un item cloné selon ses paramètres
    /// </summary>
    private void SetupClonedItem(Item clonedItem, TraderItem traderItem)
    {
        if (clonedItem != null)
        {
            clonedItem.GenerateID();

            // Générer les stats pour les équipements
            if (clonedItem is Weapon weapon)
                weapon.GenerateStats();
            else if (clonedItem is Helmet helmet)
                helmet.GenerateStats();
            else if (clonedItem is Chestplate chestplate)
                chestplate.GenerateStats();
            else if (clonedItem is Leggings leggings)
                leggings.GenerateStats();
            else if (clonedItem is Boots boots)
                boots.GenerateStats();
            else if (clonedItem is SpecialItems specialItems)
            {
                // Pour les items spéciaux, définir la quantité
                specialItems.nb = Random.Range(traderItem.minQuantity, traderItem.maxQuantity + 1);
            }
        }
    }

    /// <summary>
    /// Valide la configuration du trader
    /// </summary>
    public bool IsValid()
    {
        if (availableItems.Count == 0)
            return false;

        foreach (var traderItem in availableItems)
        {
            if (traderItem.item == null)
                return false;

            if (traderItem.minQuantity < 0 || traderItem.maxQuantity < traderItem.minQuantity)
                return false;
        }

        return minItemsInMarket >= 0 && maxItemsInMarket >= minItemsInMarket;
    }
}