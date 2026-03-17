using UnityEngine;

/// <summary>
/// Composant de base pour les effets de Résonance (Explosion, Glace, Électricité).
/// Gère la réception des statistiques du sceau (rayon, magnitude).
/// Attachez une classe qui hérite de celle-ci à vos prefabs, ou utilisez la par défaut.
/// </summary>
public class ResonanceEffectBehavior : MonoBehaviour
{
    protected Seal resonanceSeal;
    protected float radius;
    protected float magnitudePercent;

    /// <summary>
    /// Initialise les données de résonance provenant du sceau.
    /// </summary>
    public virtual void Initialize(Seal seal, float resonanceRadius, float effectMagnitudePercent)
    {
        resonanceSeal = seal;
        radius = resonanceRadius;
        magnitudePercent = effectMagnitudePercent;
        
        // Ajuster l'échelle visuelle globale par défaut en fonction du rayon
        // (Peut être surchargé ou ignoré si l'effet gère sa propre taille, par exemple pour le scale local des particules)
        float diameter = radius * 2f;
        transform.localScale = new Vector3(diameter, diameter, 1f);

        // Si l'effet comporte une lumière (EntityLight) ou un SpriteRenderer,
        // on peut leur appliquer la couleur de l'essence dominante de la résonance
        ApplyDominantColors();
    }

    protected virtual void ApplyDominantColors()
    {
        if (resonanceSeal == null) return;

        Spirit dominantSpirit = resonanceSeal.GetDominantSpiritForArchetype("Resonance");
        if (dominantSpirit != null)
        {
            Color color = dominantSpirit.essenceColor;
            color.a = 1f; // Assure l'opacité à 100%

            EntityLight el = GetComponentInChildren<EntityLight>();
            if (el != null) el.SetLightColor(color);

            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }
    }
}
