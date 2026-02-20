using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealManager : MonoBehaviour
{
    public static SealManager instance;

    [Header("Current Seal")]
    public Seal equippedSeal;

    [Header("Seal Database")]
    [Tooltip("Pour sauvegarder les sceaux créés")]
    public List<Seal> createdSeals = new List<Seal>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Génère le seal et l'équipe directement
    public void CreateSeal(SpecialItems[] specialItems)
    {
        if (specialItems == null || specialItems.Length != 4)
        {
            Debug.LogError("CreateSeal requires exactly 4 SpecialItems!");
            return;
        }

        // Vérifier que tous les items sont valides
        for (int i = 0; i < specialItems.Length; i++)
        {
            if (specialItems[i] == null)
            {
                Debug.LogError($"SpecialItem at index {i} is null!");
                return;
            }
        }

        // Créer une nouvelle instance de Seal
        Seal newSeal = ScriptableObject.CreateInstance<Seal>();
        newSeal.name = GenerateSealName();

        // Générer le sceau avec les 4 items
        newSeal.GenerateSeal(specialItems);

        // Équiper le nouveau sceau
        equippedSeal = newSeal;

        // Optionnel : Sauvegarder dans la database
        createdSeals.Add(newSeal);
    }

    // Génère un nom unique pour le sceau basé sur les items utilisés
    private string GenerateSealName()
    {
        return "no name";
    }

    // Récupère les stats du sceau équipé (utilitaire)
    public bool HasSealEquipped()
    {
        return equippedSeal != null;
    }
}