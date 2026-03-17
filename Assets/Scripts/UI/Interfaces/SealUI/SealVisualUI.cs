using UnityEngine;
using UnityEngine.UI;

public class SealVisualUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image for the primary essence (Z-Index 0)")]
    public Image baseLayer;

    [Tooltip("Image for the archetype effect (Z-Index 1)")]
    public Image archetypeLayer;

    /// <summary>
    /// Updates all visual layers based on the provided Seal.
    /// Call this when the Seal slot is assigned or updated.
    /// </summary>
    /// <param name="seal">The Seal data object. Can be null to clear the slot.</param>
    public void UpdateVisuals(Seal seal)
    {
        if (seal == null)
        {
            // Clear or hide the seal visually
            if (baseLayer) baseLayer.gameObject.SetActive(false);
            if (archetypeLayer) archetypeLayer.gameObject.SetActive(false);
            return;
        }

        // --- 1. Base Layer (Primary Essence) ---
        if (baseLayer)
        {
            if (seal.baseSprite != null)
            {
                baseLayer.gameObject.SetActive(true);
                baseLayer.sprite = seal.baseSprite;

                // Appliquer la couleur secondaire si elle existe, sinon blanc pur.
                Color finalColor = Color.white;
                if (seal.secondaryColor != Color.clear)
                {
                    finalColor = seal.secondaryColor;
                    finalColor.a = 1f; // Forcer l'alpha à 1 pour être visible
                }
                baseLayer.color = finalColor;
            }
            else
            {
                // Fallback in case the seal has no primary image assigned
                baseLayer.gameObject.SetActive(false);
            }
        }

        // --- 2. Archetype Layer ---
        if (archetypeLayer)
        {
            if (seal.archetypeSprite != null)
            {
                archetypeLayer.gameObject.SetActive(true);
                archetypeLayer.sprite = seal.archetypeSprite;

                // L'archétype prend lui aussi la couleur secondaire pour se fondre avec le sceau
                Color finalColor = Color.white;
                if (seal.secondaryColor != Color.clear)
                {
                    finalColor = seal.secondaryColor;
                    finalColor.a = 1f; // Forcer l'alpha à 1 pour être visible
                }
                archetypeLayer.color = finalColor;
            }
            else
            {
                // Seal might be very weak/incomplete and not reach any archetype threshold
                archetypeLayer.gameObject.SetActive(false);
            }
        }
    }
}
