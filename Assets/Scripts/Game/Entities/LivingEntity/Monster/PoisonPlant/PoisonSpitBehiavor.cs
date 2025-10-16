using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PoisonSpitBehiavor : MonoBehaviour
{
    public float speed;
    Vector2 direction;
    float lifetime = 1f;
    float timer = 0f;
    bool active = true;
    GameObject gameobjectToIgnore;
    bool isOnGround = false;
    SpriteRenderer spriteRenderer;

    public void Init(GameObject gameobjectToIgnore, Vector2? target = null, float speed = 3f)
    {
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
                    collision.gameObject.GetComponent<EntityEffects>().SetState(isPoison:true);
                }

                StartCoroutine(GoToGroundRoutine());
            }
        }
        else
        {
            if (collision.gameObject.GetComponent<Stats>() != null &&
                collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
            {
                collision.gameObject.GetComponent<EntityEffects>().SetState(isPoison: true);
            }
        }
    }


    public IEnumerator GoToGroundRoutine()
    {
        isOnGround = true;
        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<ObjectPerspective>().level--;
        speed = 0;
        GetComponent<ObjectParticles>().SpawnParticle("Poison", gameObject.transform.position, .5f, 6);
        GetComponent<SoundContainer>().PlaySound("Gas", 2);

        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }
}
