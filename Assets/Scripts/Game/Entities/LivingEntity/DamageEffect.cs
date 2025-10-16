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

    // Méthode pour déclencher l'effet de clignotement en rouge pendant 0.5 secondes
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

        // Attendre 0.5 secondes
        yield return new WaitForSecondsRealtime(0.15f);

        // Revenir à la couleur normale
        spriteRenderer.color = colorSprite;

        if (setVulnerability)
            stats.isVulnerable = true;

    }
}
