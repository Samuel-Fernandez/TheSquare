using System.Collections;
using UnityEngine;

public class LaunchedSlimeball : MonoBehaviour
{
    int strength;
    public float speed;
    Vector2 direction;
    float lifetime = 1f;
    float timer = 0f;
    bool active = true;
    GameObject gameobjectToIgnore;
    bool isOnGround = false;
    SpriteRenderer spriteRenderer;

    public void Init(int strength, GameObject gameobjectToIgnore, Vector2? target = null, float speed = 3f)
    {
        this.strength = strength;
        this.speed = speed; // On utilise le speed passé en paramètre
        this.gameobjectToIgnore = gameobjectToIgnore;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Si target est null, on prend la position du joueur
        Vector2 finalTarget = target ?? (Vector2)PlayerManager.instance.player.transform.position;
        Vector2 origin = transform.position;
        direction = (finalTarget - origin).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        GetComponent<ObjectPerspective>().level = gameobjectToIgnore.GetComponent<ObjectPerspective>().level + 1;
    }


    void FixedUpdate()
    {
        if (!active) return;

        transform.position += (Vector3)(direction * speed * Time.fixedDeltaTime);

        timer += Time.fixedDeltaTime;
        if (timer >= lifetime)
        {
            active = false;
            StartCoroutine(GoToGroundRoutine());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("LowHeight"))
            return; // Ignore collisions avec ce layer

        // Ton code actuel ici
        if (!isOnGround)
        {
            if (!active) return;

            if (collision.gameObject != gameobjectToIgnore)
            {
                active = false;

                if (collision.gameObject.GetComponent<Stats>() != null &&
                    collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
                {
                    collision.gameObject.GetComponent<EntityEffects>().SetState(isSlimed: true);
                    collision.gameObject.GetComponent<LifeManager>().TakeDamage(strength, this.gameObject, false);
                }

                StartCoroutine(GoToGroundRoutine());
            }
        }
        else
        {
            if (collision.gameObject.GetComponent<Stats>() != null &&
                collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
            {
                collision.gameObject.GetComponent<EntityEffects>().SetState(isSlimed: true);
            }
        }
    }


    public IEnumerator GoToGroundRoutine()
    {
        isOnGround = true;
        GetComponent<ObjectPerspective>().level--;
        speed = 0;

        GetComponent<ObjectAnimation>()?.PlayAnimation("Ground", true);

        yield return new WaitForSeconds(3f);

        // Fondu sur 1 seconde
        float fadeDuration = 1f;
        float elapsed = 0f;

        Color startColor = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
