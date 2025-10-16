using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BigBlobBehiavor : MonoBehaviour
{
    public GameObject shockWavePrefab;
    public GameObject shadow;
    public float detectionRadius = 6;
    public float baseJumpHeight = .5f;     // Hauteur de base du saut
    public float heightMultiplier = 1.2f;   // Multiplicateur de hauteur en fonction de la distance
    public bool staticBlob; // Si blob pour event
    public float smallJumpHeight = 0.7f;   // Hauteur des petits sauts de déplacement


    Transform player;
    Stats stats;
    SpriteRenderer spriteRenderer;
    ObjectAnimation anim;
    Vector3 originalShadowScale;
    Vector3 shadowOffset;  // Offset initial entre le blob et son ombre
    bool isJumping = false;
    bool isMoving = false; // Pour éviter que déplacement et attaque se chevauchent

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (staticBlob)
            StartCoroutine(SpawnFromSkyRoutine());
        else
        {
            player = PlayerManager.instance.player.transform;
            stats = GetComponent<Stats>();
            anim = GetComponent<ObjectAnimation>();

            originalShadowScale = shadow.transform.localScale;
            shadowOffset = shadow.transform.position - transform.position;  // Calculer l'offset initial entre le blob et son ombre
            StartCoroutine(RandomSoundRoutine());
            StartCoroutine(BehaviorRoutine()); // Routine principale qui gère attaque et déplacement

            anim.PlayAnimation("Idle");
        }
    }

    float DistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }

    // Routine principale qui gère le comportement du blob
    IEnumerator BehaviorRoutine()
    {
        while (true)
        {
            if (!isJumping && !isMoving && DistanceToPlayer() <= detectionRadius)
            {
                // Décision entre attaque et déplacement
                float attackChance = 0.3f; // 30% de chance d'attaquer, 70% de se déplacer

                if (Random.value <= attackChance)
                {
                    // Attaque
                    yield return new WaitForSeconds(Random.Range(0, 1f));
                    yield return StartCoroutine(AttackJumpRoutine());
                }
                else
                {
                    // Petit saut de déplacement vers le joueur
                    yield return new WaitForSeconds(Random.Range(0, 1.5f));
                    yield return StartCoroutine(MovementJumpRoutine());
                }
            }
            else
            {
                yield return new WaitForSeconds(.5f);
            }
        }
    }

    // Vérifie si une position est libre d'obstacles (seul le joueur est accepté)
    bool IsPositionClear(Vector3 position, float checkRadius = 1f)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, checkRadius);

        foreach (Collider2D col in colliders)
        {
            // Ignorer le propre collider du blob
            if (col.transform == transform)
                continue;

            // Vérifier si c'est le joueur
            Stats entityStats = col.GetComponent<Stats>();
            if (entityStats != null && entityStats.entityType == EntityType.Player)
                continue; // Le joueur est accepté

            // Tout autre collider rend la position invalide
            return false;
        }
        return true;
    }

    // Trouve une position valide autour du joueur
    Vector3 FindValidPositionAroundPlayer(int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(0f, 3f);
            Vector3 targetPosition = player.position + new Vector3(offset.x, offset.y, 0);

            if (IsPositionClear(targetPosition))
            {
                return targetPosition;
            }
        }

        // Si aucune position valide n'est trouvée, retourner la position du joueur
        return player.position;
    }

    // Petit saut de déplacement vers le joueur
    IEnumerator MovementJumpRoutine()
    {
        isMoving = true;

        // Direction vers le joueur avec une petite variation
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
        Vector3 targetDirection = directionToPlayer + new Vector3(randomOffset.x, randomOffset.y, 0);

        // Distance de déplacement plus courte pour le mouvement
        float moveDistance = Random.Range(1f, 2f);
        Vector3 targetPosition = transform.position + targetDirection.normalized * moveDistance;

        // Vérifier si la position est libre
        if (!IsPositionClear(targetPosition))
        {
            // Si pas libre, essayer une position plus proche
            targetPosition = transform.position + targetDirection.normalized * (moveDistance * 0.5f);
            if (!IsPositionClear(targetPosition))
            {
                // Si toujours pas libre, annuler le déplacement
                isMoving = false;
                yield break;
            }
        }

        yield return StartCoroutine(PerformJump(targetPosition, smallJumpHeight, 0.8f, false));

        isMoving = false;
    }

    // Saut d'attaque (ancien JumpRoutine)
    IEnumerator AttackJumpRoutine()
    {
        isJumping = true;
        anim.StopAllAnimations();
        anim.PlayAnimation("Jump");

        // Étape 1 : Écrasement initial
        yield return StartCoroutine(LerpShadowScale(originalShadowScale, originalShadowScale * 1.25f, 0.2f));

        // Petite pause avant le saut
        yield return new WaitForSecondsRealtime(.8f);

        // Étape 2 : Trouver une position valide autour du joueur
        Vector3 targetPosition = FindValidPositionAroundPlayer();

        // Calculer la distance actuelle pour déterminer la hauteur du saut
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        float jumpHeight = baseJumpHeight + (distanceToTarget * heightMultiplier);
        jumpHeight = Mathf.Min(jumpHeight, 2);

        yield return StartCoroutine(PerformJump(targetPosition, jumpHeight, 1f, true));

        isJumping = false;
    }

    // Fonction générique pour effectuer un saut
    IEnumerator PerformJump(Vector3 targetPosition, float jumpHeight, float jumpDuration, bool isAttack)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        // Désactiver le collider pendant le saut
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SoundContainer>().PlaySound("Jump", 2);

        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;

            // Position en arc de saut pour le blob
            Vector3 horizontalPosition = Vector3.Lerp(startPos, targetPosition, t);
            float heightOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            transform.position = horizontalPosition + new Vector3(0, heightOffset, 0);

            // Déplacer l'ombre en conservant l'offset d'origine mais sans le déplacement vertical du saut
            shadow.transform.position = transform.position - new Vector3(0, heightOffset, 0) + shadowOffset;

            // Shadow scale dynamique pour les attaques seulement
            if (isAttack)
            {
                if (t < 0.5f)
                {
                    float scaleT = t / 0.5f;
                    shadow.transform.localScale = Vector3.Lerp(originalShadowScale * 1.25f, originalShadowScale * 0.5f, scaleT);
                }
                else
                {
                    float scaleT = (t - 0.5f) / 0.5f;
                    shadow.transform.localScale = Vector3.Lerp(originalShadowScale * 0.5f, originalShadowScale, scaleT);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Forcer la position finale
        transform.position = targetPosition;
        shadow.transform.position = targetPosition + shadowOffset;
        shadow.transform.localScale = originalShadowScale;

        // Effets spéciaux seulement pour les attaques
        if (isAttack)
        {
            GameObject shockwaveInstance = Instantiate(shockWavePrefab, transform.position, Quaternion.identity);
            shockwaveInstance.GetComponent<Shockwave>().InitShockwave(stats.strength, stats.knockbackPower);
            CameraManager.instance.ShakeCamera(2, 2, .5f);
        }

        // Animation idle à la fin
        GetComponent<SoundContainer>().PlaySound("Fall", isAttack ? 2 : 1);
        anim.PlayAnimation("Idle");
        GetComponent<Collider2D>().enabled = true;
    }

    IEnumerator RandomSoundRoutine()
    {
        while (true)
        {
            GetComponent<SoundContainer>().PlaySound("Move", 2);
            yield return new WaitForSeconds(Random.Range(1f, 5f));
        }
    }

    IEnumerator ScaleShadow(float duration, bool reverse)
    {
        if (shadow == null)
            yield break;

        Vector3 startScale = reverse ? Vector3.zero : originalShadowScale;
        Vector3 endScale = reverse ? originalShadowScale : Vector3.zero;

        float elapsed = 0;
        while (elapsed < duration)
        {
            shadow.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        shadow.transform.localScale = endScale;
    }

    IEnumerator LerpShadowScale(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            shadow.transform.localScale = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        shadow.transform.localScale = end;
    }

    public IEnumerator SpawnFromSkyRoutine()
    {
        float duration = 2f;
        float elapsed = 0f;

        Transform spriteTransform = spriteRenderer.gameObject.transform;

        Vector3 startBlobScale = Vector3.one * 3f;
        Vector3 endBlobScale = Vector3.one;

        Vector3 startShadowScale = Vector3.zero;
        Vector3 endShadowScale = new Vector3(6.9f, 2.7f, 1f);

        Vector3 baseSpritePos = spriteTransform.localPosition;
        Vector3 startSpritePos = baseSpritePos + new Vector3(0, 10f, 0);
        Vector3 endSpritePos = baseSpritePos;

        spriteTransform.localPosition = startSpritePos;
        spriteTransform.localScale = startBlobScale;
        shadow.transform.localScale = startShadowScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // interpolation exponentielle
            float smoothT = Mathf.Pow(t, 3f);

            spriteTransform.localPosition = Vector3.Lerp(startSpritePos, endSpritePos, smoothT);
            spriteTransform.localScale = Vector3.Lerp(startBlobScale, endBlobScale, smoothT);
            shadow.transform.localScale = Vector3.Lerp(startShadowScale, endShadowScale, smoothT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        GetComponent<SoundContainer>().PlaySound("Fall", 1);
        CameraManager.instance.ShakeCamera(2, 2, .5f);

        // Forcer les valeurs finales
        spriteTransform.localPosition = endSpritePos;
        spriteTransform.localScale = endBlobScale;
        shadow.transform.localScale = endShadowScale;
    }
}