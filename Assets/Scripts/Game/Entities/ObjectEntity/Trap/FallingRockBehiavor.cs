using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FallingRockBehiavor : MonoBehaviour
{
    public int damage;
    BoxCollider2D rockCollider;

    void Start()
    {
        rockCollider = GetComponent<BoxCollider2D>();
        rockCollider.enabled = false; // désactivé au début

        // Récupère le sprite enfant
        Transform sprite = GetComponentInChildren<SpriteRenderer>().transform;

        // Position de départ (en haut)
        sprite.localPosition = new Vector3(0, 6, 0);

        // Lance la coroutine de chute
        StartCoroutine(FallRoutine(sprite));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Stats>() &&
            collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
        {
            collision.GetComponent<LifeManager>().TakeDamage(damage, gameObject, false);
        }
    }

    IEnumerator FallRoutine(Transform sprite)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = new Vector3(0, 6, 0);
        Vector3 endPos = new Vector3(0, 0, 0);

        GetComponent<SoundContainer>().PlaySound("Falling", 3);

        bool colliderActivated = false;

        while (elapsed < duration)
        {
            // Ratio de progression (0 -> 1)
            float t = elapsed / duration;

            // Courbe d'accélération (ease-in)
            t = t * t;

            // Interpolation de la position
            sprite.localPosition = Vector3.Lerp(startPos, endPos, t);

            // Active le collider quand il reste 0.15s
            if (!colliderActivated && (duration - elapsed) <= 0.15f)
            {
                rockCollider.enabled = true;
                colliderActivated = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fin de la chute
        sprite.localPosition = endPos;

        yield return GetComponent<ObjectAnimation>().PlayAnimationCoroutine("Destroy");
        GetComponent<LootChance>().Drop();
        GetComponent<SoundContainer>().PlaySound("Destroy", 3);
        Destroy(gameObject);
    }
}
