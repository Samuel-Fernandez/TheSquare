using UnityEngine;
using System.Collections.Generic;

public enum SpecialItemType
{
    ARROW,
    IRON,
    SILVER,
    DIAMOND,
    ANTIMATTER,
    SQUAREBLOCK,
    WOOD,
    STONE,
    AMETHYST,
    COAL,
    HAY,
    SLIME_BALL,
    SLIME_HEART,
    SQUARE_ANT,
}

[System.Serializable]
public class EssenceComposition
{
    public Spirit essence;
    [Range(0f, 100f)]
    public float percentage;
}

[CreateAssetMenu(fileName = "New Special Item", menuName = "Items/Special Item")]
public class SpecialItems : Item
{
    [Header("Special Item Properties")]
    public SpecialItemType type;
    public int nb;
    public int numberRequiredForSealing;

    [Header("Essence Composition")]
    [Tooltip("La somme des pourcentages devrait faire 100%")]
    public List<EssenceComposition> essenceComposition = new List<EssenceComposition>();

    [Header("Debug")]
    [SerializeField] private float totalPercentage;

    public int CalculateValue()
    {
        return value * nb;
    }

    // Retourne un dictionnaire normalis� (Essence -> pourcentage entre 0 et 1)
    public Dictionary<Spirit, float> GetNormalizedEssences()
    {
        Dictionary<Spirit, float> normalized = new Dictionary<Spirit, float>();

        float total = GetTotalPercentage();

        if (total <= 0f)
        {
            Debug.LogWarning($"Item {name} has no essence composition!");
            return normalized;
        }

        foreach (var comp in essenceComposition)
        {
            if (comp.essence != null && comp.percentage > 0f)
            {
                // Normalise � 1.0 au cas o� le total ne fait pas exactement 100%
                normalized[comp.essence] = comp.percentage / total;
            }
        }

        return normalized;
    }

    // Calcule le total des pourcentages
    public float GetTotalPercentage()
    {
        float total = 0f;
        foreach (var comp in essenceComposition)
        {
            total += comp.percentage;
        }
        return total;
    }

    // Valide que la composition fait bien 100%
    public bool IsCompositionValid()
    {
        float total = GetTotalPercentage();
        return Mathf.Approximately(total, 100f);
    }

    // Pour afficher un warning dans l'Inspector
    private void OnValidate()
    {
        totalPercentage = GetTotalPercentage();

        if (totalPercentage > 0f && !Mathf.Approximately(totalPercentage, 100f))
        {
            Debug.LogWarning($"[{name}] Essence composition totals {totalPercentage}% instead of 100%");
        }
    }
}