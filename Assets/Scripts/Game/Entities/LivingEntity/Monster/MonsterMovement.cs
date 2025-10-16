using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    Transform player;
    Stats stats;
    ObjectAnimation anim;
    SpriteRenderer spriteRenderer;
    public Vector3 direction;
    public float actualSpeed;
    public float detectionZoneRadius = 0f;
    private bool movingRandomly = false;
    private float originalSpeed;
    public bool stopMonsterMovement = false; // Si le monstre a un comportement spécifique de mouvement dans un autre script

    private void Start()
    {
        player = PlayerManager.instance.player.transform;
        stats = GetComponent<Stats>();
        anim = GetComponent<ObjectAnimation>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        originalSpeed = stats.speed;  // Garde une trace de la vitesse normale
        actualSpeed = originalSpeed;  // Initialise la vitesse à la vitesse normale

        StartCoroutine(RoutineSound());
        StartCoroutine(RandomMovement()); // Démarre le mouvement aléatoire
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0 || !stats.canMove || stopMonsterMovement) return;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionZoneRadius)
            {
                if(!stopMonsterMovement)
                    direction = (player.position - transform.position).normalized; // Calcule la direction vers le joueur
                movingRandomly = false; // Arrête le mouvement aléatoire
            }
            else
            {
                // Si le joueur est hors de la zone de détection, le monstre peut se déplacer aléatoirement
                if (!movingRandomly)
                {
                    StartCoroutine(RandomMovement());
                }
            }
        }

        // FAIRE LA MÊME POUR LE NEW MONSTERMOVEMENT
        float speed = actualSpeed * (GetComponent<EntityEffects>().isSlimed ? 0.5f : 1f);
        transform.position += direction * speed * Time.fixedDeltaTime;


        // Met à jour la direction du sprite
        UpdateSpriteDirection();
    }

    public Vector3 GetDirection() => direction;

    public void UpdateSpeed(float multiplier, bool reverse = false)
    {
        actualSpeed = stats.speed * multiplier;

        if (reverse)
        {
            // Inverser la direction pour reculer
            direction = (transform.position - player.position).normalized; // Reculer en s'éloignant du joueur
        }
        else
        {
            direction = (player.position - transform.position).normalized; // Avancer vers le joueur
        }
    }

    public void ResetSpeed()
    {
        actualSpeed = originalSpeed;
    }

    public void UpdateSpriteDirection()
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.flipX = direction.x > 0;
        }
    }

    public float GetDistanceToPlayer()
    {
        if (player != null)
        {
            return Vector3.Distance(transform.position, player.position);
        }
        return Mathf.Infinity; // Retourne une valeur très élevée si le joueur n'existe pas ou est null
    }

    IEnumerator RandomMovement()
    {
        movingRandomly = true;

        while (!player || Vector3.Distance(transform.position, player.position) > detectionZoneRadius)
        {
            // Choisir une direction aléatoire
            float randomX = Random.Range(-1f, 1f);
            float randomY = Random.Range(-1f, 1f);
            direction = new Vector3(randomX, randomY, 0).normalized; // Direction aléatoire

            // Attendre un certain temps avant de changer de direction
            yield return new WaitForSeconds(2f); // Change de direction toutes les 2 secondes
        }

        movingRandomly = false; // Arrête le mouvement aléatoire si le joueur entre dans la zone de détection
    }

    IEnumerator RoutineSound()
    {
        yield return new WaitForSeconds(Random.Range(1, 5));
        GetComponent<SoundContainer>().PlaySound("Move", 2);
    }
}
