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
        if (loots == null || loots.loots.Count == 0) return null;

        // On tire un nombre entre 0 et 9999 (soit 10000 possibilités)
        int roll = Random.Range(0, 10000);
        int currentWeight = 0;

        foreach (LootContainer lootContainer in loots.loots)
        {
            float finalChance = lootContainer.lootChance;
            
            // J'applique le dropChance du joueur comme un multiplicateur.
            // (Si c'est 1.5, tu auras 50% de chance en plus d'avoir cet objet)
            if (PlayerManager.instance != null && PlayerManager.instance.dropChance > 0)
            {
                // Ajouter une valeur fixe favorise mathématiquement beaucoup plus les objets rares !
                // Ex: si dropChance ajoute +50 chances...
                // Un objet rare (5 chances) passe à 55 (probabilité x11) !
                // Un objet très commun (3000 chances) passe à 3050 (presque inchangé).
                finalChance += (PlayerManager.instance.dropChance * 10f); // Le *10f est à ajuster selon ton équilibrage
            }

            currentWeight += (int)finalChance;

            // Si notre jet tombe dans la tranche de cet objet, on le drop
            if (roll < currentWeight)
            {
                return lootContainer.item;
            }
        }

        // Si on dépasse le poids total de tous les objets, le tirage tombe dans les "places restantes"
        return null;
    }
}