using UnityEngine;
using System.Collections;

public class DamageEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Stats stats;
    Color colorSprite;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        colorSprite = spriteRenderer.color;
        stats = GetComponent<Stats>();
    }

    // M�thode pour d�clencher l'effet de clignotement en rouge pendant 0.5 secondes
    public void DamageEffects(bool setVulnerability = true, Color? color = null)
    {
        StartCoroutine(FlashColorCoroutine(setVulnerability, color ?? Color.red));
    }

    private IEnumerator FlashColorCoroutine(bool setVulnerability, Color color)
    {
        // Changer la couleur en la couleur choisie
        spriteRenderer.color = color;

        if (setVulnerability)
            stats.isVulnerable = false;

        float invulnerabilityDuration = stats.entityType == EntityType.Player ? 0.5f : 0.25f;
        yield return new WaitForSecondsRealtime(invulnerabilityDuration);

        // Revenir � la couleur normale
        spriteRenderer.color = colorSprite;

        if (setVulnerability)
            stats.isVulnerable = true;

    }
}
