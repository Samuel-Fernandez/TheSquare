using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobMovement : MonoBehaviour
{
    public float minInterval = 1f; // Intervalle minimum en secondes
    public float maxInterval = 5f; // Intervalle maximum en secondes

    public float jumpLength;
    public float animationWait;

    private Transform player; // R�f�rence au joueur
    private Stats stats;
    private ObjectAnimation anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb; // R�f�rence au Rigidbody2D

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        player = PlayerManager.instance.player.transform;
        stats = GetComponent<Stats>();
        anim = GetComponent<ObjectAnimation>();

        anim.PlayAnimation("Afk");

        // D�marrer le premier saut
        StartCoroutine(JumpCoroutine());
    }

    private IEnumerator JumpCoroutine()
    {
        while (true)
        {
            // Attendre un intervalle alatoire avant de sauter
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return StartCoroutine(WaitWhileCantMove(waitTime));

            // Effectuer le saut
            yield return StartCoroutine(PerformJump());
        }
    }

    private IEnumerator PerformJump()
    {
        float jumpDuration = jumpLength;
        float elapsedTime = 0f;
        stats.doingAttack = true;

        Vector3 initialPosition = transform.position;
        Vector3 direction;
        float distanceToPlayer = Vector3.Distance(initialPosition, player.position);

        // Choisir la direction : vers le joueur ou al�atoire
        if (distanceToPlayer <= 5f)
        {
            direction = (player.position - initialPosition).normalized;
        }
        else
        {
            // Direction al�atoire dans un cercle unit�
            direction = Random.insideUnitCircle.normalized;
        }

        // AUGMENTATION de la vitesse : * 1.5f
        Vector3 targetPosition = initialPosition + direction * stats.speed * 1.5f * jumpDuration;

        if (anim != null)
        {
            anim.PlayAnimation("Movement");
            yield return StartCoroutine(WaitWhileCantMove(animationWait));
        }

        GetComponent<SoundContainer>().PlaySound("Move", 1);

        while (elapsedTime < jumpDuration)
        {
            while (!stats.canMove) yield return null;

            Vector3 newPosition = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / jumpDuration);

            if (!DetectObstacleCollision(newPosition) && !GetComponent<LifeManager>().isKnockbacking)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                break;
            }

            UpdateSpriteDirection(direction);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        GetComponent<SoundContainer>().PlaySound("Move", 1);

        if (anim != null)
        {
            anim.PlayAnimation("Afk");
        }

        stats.doingAttack = false;
    }


    private void UpdateSpriteDirection(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private bool DetectObstacleCollision(Vector3 newPosition)
    {
        // Utiliser Raycast pour v�rifier les collisions avec des obstacles
        RaycastHit2D hit = Physics2D.Raycast(transform.position, newPosition - transform.position, Vector3.Distance(transform.position, newPosition));
        if (hit.collider != null && hit.collider.gameObject != gameObject && hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            return true; // Retourne vrai si une collision avec un obstacle est d�tect�e
        }
        return false;
    }

    private IEnumerator WaitWhileCantMove(float time)
    {
        float timer = 0f;
        while (timer < time)
        {
            if (stats.canMove)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }
        while (!stats.canMove) yield return null;
    }
}
