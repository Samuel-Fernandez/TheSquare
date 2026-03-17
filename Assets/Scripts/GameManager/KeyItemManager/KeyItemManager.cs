using System.Collections.Generic;
using UnityEngine;

public class KeyItemManager : MonoBehaviour
{
    public static KeyItemManager instance;

    [Tooltip("Liste de tous les objets clés du jeu")]
    public List<KeyItemObject> keyItemsList = new List<KeyItemObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Obtenir un KeyItemObject par son ID
    public KeyItemObject GetKeyItemByID(string id)
    {
        foreach (var item in keyItemsList)
        {
            if (item != null && item.id == id)
                return item;
        }
        return null;
    }

    // Savoir si un KeyItemObject est obtenu
    public bool IsKeyItemAcquired(string id)
    {
        KeyItemObject item = GetKeyItemByID(id);
        if (item != null)
        {
            return item.isAcquired;
        }
        return false;
    }

    // Définir l'état d'acquisition d'un KeyItemObject
    public void SetKeyItemAcquired(string id, bool state)
    {
        KeyItemObject item = GetKeyItemByID(id);
        if (item != null)
        {
            item.isAcquired = state;
        }
    }

    // --- Méthodes pour la sauvegarde ---
    public List<string> GetAcquiredKeyItems()
    {
        List<string> acquiredIds = new List<string>();
        foreach (var item in keyItemsList)
        {
            if (item != null && item.isAcquired)
            {
                acquiredIds.Add(item.id);
            }
        }
        return acquiredIds;
    }

    public void LoadAcquiredKeyItems(List<string> acquiredIds)
    {
        if (acquiredIds == null)
            acquiredIds = new List<string>();

        foreach (var item in keyItemsList)
        {
            if (item != null)
            {
                item.isAcquired = acquiredIds.Contains(item.id);
            }
        }
    }
}
