using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonGuardBehavior : MonoBehaviour
{
    public float detectionRadius = 5f;
    public List<Sprite> downDash;
    public List<Sprite> upDash;
    public List<Sprite> sideDash;
    public GameObject chargeDashPrefab;

    private Transform player;
    private Stats stats;
    private Vector2 randomDirection;
    private Vector2 currentDirection;
    private SpriteRenderer spriteRenderer;
    private ObjectAnimation anim;

    private bool isDashing = false;
    private bool canAnimate = true;


    private void Start()
    {
        player = PlayerManager.instance.player.transform;
        stats = GetComponent<Stats>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<ObjectAnimation>();
        StartCoroutine(RandomMovementRoutine());
        StartCoroutine(RandomSoundRoutine());
        StartCoroutine(RandomDashRoutine());
    }

    private void FixedUpdate()
    {
        Move();
        Animate();
    }

    private void Move()
    {
        if (isDashing) return;


        if (DistanceToPlayer() <= detectionRadius)
        {
            currentDirection = (player.position - transform.position).normalized;
            transform.position += (Vector3)(currentDirection * stats.speed * Time.fixedDeltaTime);
        }
        else
        {
            currentDirection = randomDirection;
            transform.position += (Vector3)(randomDirection * stats.speed * 0.75f * Time.fixedDeltaTime);
        }
    }

    float DistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }

    private void Animate()
    {
        if (!canAnimate)
        {
            anim.StopAnimation();
            return;
        }

        if (Mathf.Abs(currentDirection.y) > Mathf.Abs(currentDirection.x))
        {
            anim.PlayAnimation(currentDirection.y > 0 ? "Up" : "Down");
        }
        else
        {
            anim.PlayAnimation("Side");

            // Flip uniquement si déplacement significatif sur X
            if (spriteRenderer != null && Mathf.Abs(currentDirection.x) > 0.01f)
            {
                spriteRenderer.flipX = currentDirection.x < 0;
            }
        }
    }

    private List<Sprite> GetDashSpritesFromDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
            return dir.y > 0 ? upDash : downDash;
        else
            return sideDash;
    }


    IEnumerator DashRoutine()
    {
        isDashing = true;
        stats.doingAttack = true;
        canAnimate = false;
        anim.StopAllAnimations();

        Instantiate(chargeDashPrefab, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);

        GetComponent<SoundContainer>().PlaySound("Charge", 1);

        // ÉTAPE 1 : Prémices du dash
        Vector2 dashDirection = (player.position - transform.position).normalized;

        // ÉTAPE 2 : Chargement du dash (0.9s -> 3 sprites * 0.3s)
        List<Sprite> dashSprites = GetDashSpritesFromDirection(dashDirection);
        for (int i = 0; i < 3 && i < dashSprites.Count; i++)
        {
            spriteRenderer.sprite = dashSprites[i];
            yield return new WaitForSeconds(0.4f);
        }

        // ÉTAPE 3 : Dash initial (1s à 4x vitesse)
        if (dashSprites.Count > 3)
            spriteRenderer.sprite = dashSprites[3];

        GetComponent<SoundContainer>().PlaySound("Dash", 2);

        float dashSpeed = stats.speed * 8f;
        float dashTime = .35f;
        float elapsed = 0f;

        while (elapsed < dashTime)
        {
            transform.position += (Vector3)(dashDirection * dashSpeed * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Décélération linéaire sur 1s (de 4x vitesse à 0)
        float decelTime = .1f;
        elapsed = 0f;
        while (elapsed < decelTime)
        {
            float t = 1f - (elapsed / decelTime); // interpolation inversée
            float currentSpeed = dashSpeed * t;
            transform.position += (Vector3)(dashDirection * currentSpeed * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // ÉTAPE 4 : Pause post-dash (1s)
        yield return new WaitForSeconds(1f);

        canAnimate = true;
        isDashing = false;
        stats.doingAttack = false;

    }


    IEnumerator RandomDashRoutine()
    {
        while (true)
        {
            if (!isDashing && DistanceToPlayer() <= detectionRadius)
            {
                yield return new WaitForSeconds(Random.Range(1f, 8f));
                StartCoroutine(DashRoutine());
            }
            else
            {
                yield return new WaitForSeconds(1);
            }
        }
    }

    IEnumerator RandomSoundRoutine()
    {
        while(true)
        {
            GetComponent<SoundContainer>().PlaySound("Move", 2);
            yield return new WaitForSeconds(Random.Range(2f, 8f));
        }
    }

    private IEnumerator RandomMovementRoutine()
    {
        while (true)
        {
            randomDirection = Random.insideUnitCircle.normalized;
            yield return new WaitForSeconds(Random.Range(2f, 4f));
        }
    }
}
