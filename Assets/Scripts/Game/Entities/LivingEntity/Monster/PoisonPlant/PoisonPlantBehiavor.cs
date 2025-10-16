using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonPlantBehiavor : MonoBehaviour
{
    [Header("Detection")]
    public float radiusDetection = 5f;

    [Header("Combat")]
    public GameObject damageZone;
    public GameObject spitSpawn;
    public GameObject spitProjectilePrefab;

    [Header("Timing")]
    public float spitCooldownMin = 1f;
    public float spitCooldownMax = 8f;
    public float nearAttackRadius = 1.5f;

    bool isAppeared = false;
    bool isSpitting = false;
    bool isPerformingNearAttack = false;

    Coroutine currentMainRoutine;
    Coroutine currentNearAttackRoutine; // Nouvelle référence pour tracking
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        if (damageZone != null)
            damageZone.GetComponent<DamageZoneBehiavor>().Init(this.gameObject);

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // S'assurer que la damage zone est désactivée au début
        if (damageZone != null)
            damageZone.SetActive(false);
    }

    private void Update()
    {
        // Vérifier si le joueur existe
        if (PlayerManager.instance?.player == null) return;

        // Flip du sprite selon la position du joueur
        bool shouldFlipLeft = PlayerManager.instance.player.transform.position.x < transform.position.x;
        FlipSprite(shouldFlipLeft);

        // Gestion de l'apparition/disparition
        HandleAppearance();

        // Gestion de l'attaque proche (seulement si la plante est apparue et pas en train de cracher)
        HandleNearAttack();
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
            currentMainRoutine = StartCoroutine(PoisonPlantMainRoutine());

            GetComponent<SoundContainer>().PlaySound("Appear", 2);
        }
        // Disparition
        else if (!playerInRadius && isAppeared)
        {
            StartCoroutine(DisappearSequence());
        }
    }

    // Nouvelle méthode pour gérer proprement la disparition
    IEnumerator DisappearSequence()
    {
        isAppeared = false;

        // Arrêter la routine principale proprement
        if (currentMainRoutine != null)
        {
            StopCoroutine(currentMainRoutine);
            currentMainRoutine = null;
        }

        // Si une attaque proche est en cours, l'arrêter proprement
        if (currentNearAttackRoutine != null)
        {
            StopCoroutine(currentNearAttackRoutine);
            currentNearAttackRoutine = null;
            yield return StartCoroutine(CleanupNearAttack());
        }

        // Nettoyer les états
        isSpitting = false;
        isPerformingNearAttack = false;

        // Jouer la disparition
        yield return StartCoroutine(DisappearRoutine());

        GetComponent<SoundContainer>().PlaySound("Appear", 2);
    }

    // Méthode pour nettoyer l'état après interruption d'attaque proche
    IEnumerator CleanupNearAttack()
    {
        var stats = GetComponent<Stats>();
        if (stats != null)
            stats.doingAttack = false;

        // Désactiver la zone de dégâts si elle est active
        if (damageZone != null && damageZone.activeInHierarchy)
        {
            damageZone.SetActive(false);
            var damageZoneBehavior = damageZone.GetComponent<DamageZoneBehiavor>();
            if (damageZoneBehavior != null)
                damageZoneBehavior.playerTouched = false;
        }

        // Remettre l'animation à l'idle
        var objectAnimation = GetComponent<ObjectAnimation>();
        if (objectAnimation != null)
            objectAnimation.PlayAnimation("Idle");

        isPerformingNearAttack = false;
        yield return null; // Attendre une frame pour s'assurer que tout est nettoyé
    }

    void HandleNearAttack()
    {
        // Correction de la condition problématique
        var stats = GetComponent<Stats>();
        bool statsDoingAttack = stats != null && stats.doingAttack;

        if (isAppeared && !isSpitting && !isPerformingNearAttack &&
            !statsDoingAttack &&
            PlayerIsInRadius(nearAttackRadius) &&
            currentNearAttackRoutine == null) // Vérification supplémentaire
        {
            currentNearAttackRoutine = StartCoroutine(NearAttackRoutine());
        }
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

    IEnumerator DisappearRoutine()
    {
        // Désactiver la damage zone si elle est active
        if (damageZone != null && damageZone.activeInHierarchy)
        {
            damageZone.SetActive(false);
            if (damageZone.GetComponent<DamageZoneBehiavor>() != null)
                damageZone.GetComponent<DamageZoneBehiavor>().playerTouched = false;
        }

        // Jouer l'animation de disparition
        var objectAnimation = GetComponent<ObjectAnimation>();
        if (objectAnimation != null)
        {
            yield return objectAnimation.PlayAnimationCoroutine("Disappear", true);
        }
    }

    IEnumerator NearAttackRoutine()
    {
        isPerformingNearAttack = true;
        var stats = GetComponent<Stats>();

        try // Protection contre les interruptions
        {
            if (stats != null)
                stats.doingAttack = true;

            var objectAnimation = GetComponent<ObjectAnimation>();

            // Animation de début d'attaque
            if (objectAnimation != null)
                yield return objectAnimation.PlayAnimationCoroutine("StartAttack", true);

            GetComponent<SoundContainer>().PlaySound("Whip", 2);

            // Activer la zone de dégâts
            if (damageZone != null)
                damageZone.SetActive(true);

            // Rotation 360°
            yield return StartCoroutine(Rotate360Routine(0.5f));

            // Animation de fin d'attaque
            if (objectAnimation != null)
                yield return objectAnimation.PlayAnimationCoroutine("FinishAttack", true);

            // Désactiver la zone de dégâts
            if (damageZone != null)
            {
                damageZone.SetActive(false);
                var damageZoneBehavior = damageZone.GetComponent<DamageZoneBehiavor>();
                if (damageZoneBehavior != null)
                    damageZoneBehavior.playerTouched = false;
            }

            // Retour à l'idle
            if (objectAnimation != null)
                objectAnimation.PlayAnimation("Idle");
        }
        finally // S'assurer que l'état est toujours nettoyé
        {
            if (stats != null)
                stats.doingAttack = false;

            isPerformingNearAttack = false;
            currentNearAttackRoutine = null;

            // Sécurité supplémentaire : s'assurer que la damage zone est désactivée
            if (damageZone != null && damageZone.activeInHierarchy)
            {
                damageZone.SetActive(false);
                var damageZoneBehavior = damageZone.GetComponent<DamageZoneBehiavor>();
                if (damageZoneBehavior != null)
                    damageZoneBehavior.playerTouched = false;
            }
        }
    }

    IEnumerator PoisonPlantMainRoutine()
    {
        var objectAnimation = GetComponent<ObjectAnimation>();

        // Animation d'apparition
        if (objectAnimation != null)
            yield return objectAnimation.PlayAnimationCoroutine("Appear", true);

        // Boucle principale tant que la plante est apparue
        while (isAppeared)
        {
            // Animation idle
            if (objectAnimation != null)
                objectAnimation.PlayAnimation("Idle");

            // Attendre un délai aléatoire avant de cracher
            yield return new WaitForSeconds(Random.Range(spitCooldownMin, spitCooldownMax));

            // Vérifier si on peut encore cracher (plante toujours apparue et pas d'attaque proche)
            if (isAppeared && !isPerformingNearAttack)
            {
                yield return StartCoroutine(SpitAttackRoutine());
            }
        }
    }

    IEnumerator SpitAttackRoutine()
    {
        isSpitting = true;
        var objectAnimation = GetComponent<ObjectAnimation>();

        try
        {
            // Animation de début de crachat
            if (objectAnimation != null)
                yield return objectAnimation.PlayAnimationCoroutine("StartSpit", true);

            GetComponent<SoundContainer>().PlaySound("Spit", 2);

            // Créer le projectile
            if (spitProjectilePrefab != null && spitSpawn != null && PlayerManager.instance?.player != null)
            {
                Vector2 spawnPosition = spitSpawn.transform.position;
                Vector2 targetPosition = PlayerManager.instance.player.transform.position;

                GameObject poisonSpitInstance = Instantiate(spitProjectilePrefab, spawnPosition, Quaternion.identity);

                // Initialiser le projectile avec la direction correcte
                var poisonSpitBehavior = poisonSpitInstance.GetComponent<PoisonSpitBehiavor>();
                if (poisonSpitBehavior != null)
                {
                    poisonSpitBehavior.Init(gameObject, targetPosition);
                }
            }

            // Animation de fin de crachat
            if (objectAnimation != null)
                yield return objectAnimation.PlayAnimationCoroutine("EndSpit", true);
        }
        finally
        {
            isSpitting = false;
        }
    }

    public IEnumerator Rotate360Routine(float duration)
    {
        float elapsed = 0f;
        float startRotation = transform.eulerAngles.z;
        float endRotation = startRotation + 360f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Lerp(startRotation, endRotation, elapsed / duration);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // S'assurer que la rotation finale est exacte
        transform.rotation = Quaternion.Euler(0f, 0f, startRotation);
    }

    // Méthode utilitaire pour debug
    private void OnDrawGizmosSelected()
    {
        // Visualiser le rayon de détection
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radiusDetection);

        // Visualiser le rayon d'attaque proche
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, nearAttackRadius);
    }

    // Méthode de debug pour forcer le reset de l'état (utile pendant le développement)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void ForceResetState()
    {
        StopAllCoroutines();
        isSpitting = false;
        isPerformingNearAttack = false;
        currentMainRoutine = null;
        currentNearAttackRoutine = null;

        var stats = GetComponent<Stats>();
        if (stats != null)
            stats.doingAttack = false;

        if (damageZone != null)
            damageZone.SetActive(false);
    }
}