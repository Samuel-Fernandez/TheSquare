using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TheSquare.Mechanics.UniverseHeart;

[RequireComponent(typeof(EntityLight))]
public class SquareFollowersBehavior : MonoBehaviour
{
    [Header("Movement")]
    public float jumpInterval = 0.5f;
    public float jumpDistance = 1f;
    public float jumpDuration = 0.15f;
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float jumpHeight = 0.5f;

    [Header("Detection")]
    public float detectionRadius = 1f;
    public float followRadius = 5f;

    private Vector2 startPos;
    private bool isHidden = false;

    private EntityLight entityLight;
    private SpriteRenderer spriteRenderer;
    private SoundContainer soundContainer;
    private ObjectAnimation objectAnimation;
    private Vector3 initialSpritePos;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        soundContainer = GetComponent<SoundContainer>();
        entityLight = GetComponent<EntityLight>();
    }

    private void Start()
    {
        objectAnimation = GetComponent<ObjectAnimation>();
        startPos = transform.position;
        if (spriteRenderer != null) initialSpritePos = spriteRenderer.transform.localPosition;
        if (InsideTheSquareManager.instance != null)
        {
            InsideTheSquareManager.instance.squareFollowers.Add(this);
        }

        StartCoroutine(JumpRoutine());
    }

    private void Update()
    {
        if (isHidden) return;
        if (InsideTheSquareManager.player_is_revealed) return;

        CheckPlayerDetection();
    }

    private IEnumerator JumpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(jumpInterval);

            if (isHidden || InsideTheSquareManager.player_is_revealed || InsideTheSquareManager.instance == null) continue;

            // Choix d'une direction aléatoire sur la grille
            Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            Vector2 chosenDir = dirs[Random.Range(0, dirs.Length)];

            if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                float dist = Vector2.Distance(transform.position, PlayerManager.instance.player.transform.position);
                if (dist <= followRadius)
                {
                    Vector2 toPlayer = (PlayerManager.instance.player.transform.position - transform.position).normalized;
                    float maxDot = -Mathf.Infinity;
                    foreach (Vector2 d in dirs)
                    {
                        float dot = Vector2.Dot(d, toPlayer);
                        if (dot > maxDot)
                        {
                            maxDot = dot;
                            chosenDir = d;
                        }
                    }
                }
            }

            // Vérification des murs
            // On cast depuis le centre. On ignore les Triggers.
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;

            RaycastHit2D[] hits = new RaycastHit2D[5];
            int hitCount = Physics2D.Raycast(transform.position, chosenDir, filter, hits, jumpDistance);

            bool canJump = true;
            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider != null && hits[i].collider.gameObject != gameObject)
                {
                    canJump = false; // Il y a un mur
                    break;
                }
            }

            if (canJump)
            {
                // Aucun mur, on peut sauter
                Vector2 targetPos = (Vector2)transform.position + chosenDir * jumpDistance;

                // Vérifier si la position cible est dans une safe zone ou la zone centrale
                Collider2D[] overlaps = Physics2D.OverlapPointAll(targetPos);
                bool inSafeZone = false;
                foreach (var col in overlaps)
                {
                    if (col.GetComponent<SquareSafeZoneBehiavor>() != null || col.GetComponent<InsideTheSquareManager>() != null)
                    {
                        inSafeZone = true;
                        break;
                    }
                }

                if (!inSafeZone)
                {
                    StartCoroutine(PerformJump(targetPos));
                }
            }
        }
    }

    private IEnumerator PerformJump(Vector2 targetPos)
    {
        if (soundContainer != null) soundContainer.PlaySound("Jump", 1);
        if (objectAnimation != null) objectAnimation.PlayAnimation("Jump");

        Vector2 initialPos = transform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            float curveT = jumpCurve.Evaluate(t);
            transform.position = Vector2.Lerp(initialPos, targetPos, curveT);

            if (spriteRenderer != null)
            {
                float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;
                spriteRenderer.transform.localPosition = initialSpritePos + new Vector3(0, height, 0);
            }

            yield return null;
        }

        transform.position = targetPos;
        if (spriteRenderer != null) spriteRenderer.transform.localPosition = initialSpritePos;
        if (objectAnimation != null) objectAnimation.PlayAnimation("Idle");
    }

    private void CheckPlayerDetection()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null) return;

        float dist = Vector2.Distance(transform.position, PlayerManager.instance.player.transform.position);
        if (dist <= detectionRadius)
        {
            // Ignorer si le joueur est dans une safe zone et que le timer n'est pas à 100%
            if (InsideTheSquareManager.is_in_safezone && InsideTheSquareManager.instance != null && InsideTheSquareManager.instance.currentTimer < InsideTheSquareManager.instance.timeToFill)
            {
                return;
            }

            if (soundContainer != null) soundContainer.PlaySound("Laugh", 1);
            InsideTheSquareManager.TriggerReveal();
        }
    }

    public void HideFollower()
    {
        isHidden = true;
        gameObject.SetActive(false);
    }

    public void ResetFollower()
    {
        gameObject.SetActive(true);
        isHidden = false;
        transform.position = startPos;

        StopAllCoroutines();
        StartCoroutine(JumpRoutine());
    }
}
