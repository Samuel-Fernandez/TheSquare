using System.Collections;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    public int damage;
    public float knockbackPower;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Récupération du SpriteRenderer de l’enfant
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        StartCoroutine(ShockwaveEffect());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Stats>() != null &&
            collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
        {
            collision.gameObject.GetComponent<LifeManager>().TakeDamage(damage, gameObject, false);
        }
    }

    public void InitShockwave(int damage, float knockbackPower)
    {
        this.damage = damage;
        this.knockbackPower = knockbackPower;
    }

    private IEnumerator ShockwaveEffect()
    {
        float interval = 0.5f;
        int steps = 8; // 0.5 * 8 = 4s
        Vector3 baseScale = transform.localScale;
        Color baseColor = spriteRenderer.color;

        for (int i = 0; i < steps; i++)
        {
            float stepElapsed = 0f;

            // Croissance linéaire : chaque étape ajoute +2 à x et y
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = startScale + new Vector3(10f, 10f, 0f);

            // Commence le fading à partir de 3s (i = 6 et i = 7)
            bool fading = i >= 6;

            while (stepElapsed < interval)
            {
                stepElapsed += Time.deltaTime;
                float t = stepElapsed / interval;

                // Échelle linéaire
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);

                // Alpha linéaire pendant la dernière seconde (étapes 6 et 7)
                if (fading)
                {
                    // Calcule de 0 1 sur la durée 3s-4s
                    float fadeTime = ((i - 6) + t) / 2f; // 0 1 entre i=6..7
                    Color newColor = baseColor;
                    newColor.a = Mathf.Lerp(1f, 0f, fadeTime);
                    spriteRenderer.color = newColor;
                }

                yield return null;
            }

            // Fin d'étape : verrouille la taille finale
            transform.localScale = targetScale;
        }

        Destroy(gameObject);
    }


}
