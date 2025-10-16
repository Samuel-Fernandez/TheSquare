using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemCategory> itemCategories = new List<ItemCategory>();

    [System.Serializable]
    public class ItemCategory
    {
        public string categoryName;
        public List<Item> items = new List<Item>();
    }

    public void InitializeDataBase()
    {
        foreach (ItemCategory category in itemCategories)
        {
            // Initialise un compteur pour l'incrémentation
            int counter = 0;

            for (int i = 0; i < category.items.Count; i++)
            {
                // Formate l'ID de l'item en concaténant le nom de la catégorie et un nombre incrémenté
                category.items[i].itemId = category.categoryName + counter.ToString("D4");

                // Incrémente le compteur pour le prochain item
                counter++;
            }
        }
    }


    public List<Item> GetAllItemsOfType(string itemID)
    {
        // Trouver la catégorie avec le nom correspondant à itemID
        ItemCategory category = itemCategories.Find(cat => cat.categoryName == itemID);

        // Si la catégorie existe, retourner une liste de clones des items, sinon retourner une liste vide
        if (category != null)
        {
            List<Item> clonedItems = new List<Item>();
            foreach (Item item in category.items)
            {
                clonedItems.Add(ScriptableObjectUtility.Clone(item));
            }
            return clonedItems;
        }
        else
        {
            return new List<Item>();
        }
    }

    public Item GetItemByID(string itemID)
    {
        // Parcourir toutes les catégories d'items
        foreach (ItemCategory category in itemCategories)
        {
            // Parcourir tous les items de la catégorie actuelle
            foreach (Item item in category.items)
            {
                // Vérifier si les 6 premiers caractères de l'ID de l'item correspondent
                if (item.itemId.Substring(0, 6) == itemID.Substring(0, 6))
                {
                    return ScriptableObjectUtility.Clone(item);
                }
            }
        }
        // Si aucun item correspondant n'est trouvé, retourner null
        return null;
    }
}
