using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SporkBehiavor : MonoBehaviour
{
    [Header("Detection")]
    public float radiusDetection = 5f;

    [Header("Combat")]
    public GameObject spitSpawn;
    public GameObject spitProjectilePrefab;
    public int projectileSpeed = 5;

    [Header("Timing")]
    public float spitCooldownMin = 1f;
    public float spitCooldownMax = 4f;

    bool isAppeared = false;

    Coroutine currentMainRoutine;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // Vérifier si le joueur existe
        if (PlayerManager.instance?.player == null) return;

        // Flip du sprite selon la position du joueur (orientation)
        bool shouldFlipLeft = PlayerManager.instance.player.transform.position.x < transform.position.x;
        FlipSprite(shouldFlipLeft);

        // Gestion de l'apparition/disparition
        HandleAppearance();
    }

    void HandleAppearance()
    {
        bool playerInRadius = PlayerIsInRadius(radiusDetection);

        // Apparition
        if (playerInRadius && !isAppeared)
        {
            isAppeared = true;
            if (currentMainRoutine != null)
                StopCoroutine(currentMainRoutine);
            currentMainRoutine = StartCoroutine(SporkMainRoutine());

            if (GetComponent<SoundContainer>() != null)
                GetComponent<SoundContainer>().PlaySound("Appear", 2);
        }
        // Disparition
        else if (!playerInRadius && isAppeared)
        {
            StartCoroutine(DisappearSequence());
        }
    }

    IEnumerator DisappearSequence()
    {
        isAppeared = false;

        // Arrêter la routine principale proprement
        if (currentMainRoutine != null)
        {
            StopCoroutine(currentMainRoutine);
            currentMainRoutine = null;
        }

        // Jouer la disparition
        var objectAnimation = GetComponent<ObjectAnimation>();
        if (objectAnimation != null)
        {
            yield return objectAnimation.PlayAnimationCoroutine("Disappear", true);
        }

        if (GetComponent<SoundContainer>() != null)
            GetComponent<SoundContainer>().PlaySound("Appear", 2);
    }

    void FlipSprite(bool flipLeft)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.flipX = flipLeft;

        // Ajuster la position du spawn de projectile selon l'orientation
        if (spitSpawn != null)
        {
            Vector3 localPos = spitSpawn.transform.localPosition;
            localPos.x = flipLeft ? -Mathf.Abs(localPos.x) : Mathf.Abs(localPos.x);
            spitSpawn.transform.localPosition = localPos;
        }
    }

    bool PlayerIsInRadius(float maxRadiusDistance)
    {
        if (PlayerManager.instance?.player == null) return false;
        return Vector2.Distance(transform.position, PlayerManager.instance.player.transform.position) <= maxRadiusDistance;
    }

    IEnumerator SporkMainRoutine()
    {
        var objectAnimation = GetComponent<ObjectAnimation>();

        // Animation d'apparition
        if (objectAnimation != null)
            yield return objectAnimation.PlayAnimationCoroutine("Appear", true);

        // Boucle principale tant que le Spork est apparu
        while (isAppeared)
        {
            // Animation idle
            if (objectAnimation != null)
                objectAnimation.PlayAnimation("Idle");

            // Attendre un délai aléatoire entre 1 et 4 secondes avant de cracher
            yield return new WaitForSeconds(Random.Range(spitCooldownMin, spitCooldownMax));

            // Vérifier si on peut encore cracher (Spork toujours apparu)
            if (isAppeared)
            {
                yield return StartCoroutine(SpitAttackRoutine());
            }
        }
    }

    IEnumerator SpitAttackRoutine()
    {
        var objectAnimation = GetComponent<ObjectAnimation>();

        // Animation de début de crachat
        if (objectAnimation != null)
            yield return objectAnimation.PlayAnimationCoroutine("StartSpit", true);

        if (GetComponent<SoundContainer>() != null)
            GetComponent<SoundContainer>().PlaySound("Spit", 2);

        // Créer le projectile
        if (spitProjectilePrefab != null && spitSpawn != null && PlayerManager.instance?.player != null)
        {
            Vector2 spawnPosition = spitSpawn.transform.position;
            Vector3 targetPosition = PlayerManager.instance.player.transform.position;
            Vector2 direction = (targetPosition - (Vector3)spawnPosition).normalized;

            // Calcul de l'angle pour ProjectileBehavior
            float angle = Mathf.Atan2(direction.y, direction.x);

            GameObject projectileInstance = Instantiate(spitProjectilePrefab, spawnPosition, Quaternion.identity);

            // Initialiser le projectile
            var projectileBehavior = projectileInstance.GetComponent<ProjectileBehavior>();
            if (projectileBehavior != null)
            {
                // strength, speed, accurateDirection, ally, knockBackPower, launcher
                int strength = GetComponent<Stats>() != null ? GetComponent<Stats>().strength : 10;
                projectileBehavior.InitProjectile(strength, projectileSpeed, angle, false, 0f, gameObject, true);
            }
        }

        // Animation de fin de crachat
        if (objectAnimation != null)
            yield return objectAnimation.PlayAnimationCoroutine("EndSpit", true);
    }

    // Méthode utilitaire pour debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radiusDetection);
    }
}
