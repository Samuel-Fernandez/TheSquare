using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballBehiavor : MonoBehaviour
{
    public GameObject explosionEffect;
    GameObject launcher;
    private Vector2 direction;
    private float speed;
    private int strength;

    private void Update()
    {
        // Déplacement constant dans la direction fixée
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    public void Init(GameObject launcher, GameObject target, float speed, int strength)
    {
        this.speed = speed;
        this.strength = strength;
        this.launcher = launcher;

        // Calcul de la direction vers la cible
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = target.transform.position;
        direction = (targetPosition - startPosition).normalized;

        // Rotation de la fireball
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        // On soustrait 90° si le sprite regarde vers le haut par défaut
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Exemple de logique de collision
        if (collision.gameObject.tag != "LowHeight" && collision.gameObject != launcher)
        {
            if (collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
            {
                collision.gameObject.GetComponent<LifeManager>().TakeDamage(strength, false);

                if(!collision.gameObject.GetComponent<PlayerController>().isDodging)
                    collision.gameObject.GetComponent<EntityEffects>().SetState(Mathf.Min(strength / 4, 1), true);
            }

            Instantiate(explosionEffect, new Vector2(this.transform.position.x, this.transform.position.y), Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
