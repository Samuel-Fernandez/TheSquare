using UnityEngine;
using System.Collections;

public enum SkeletonType
{
    NONE,
    THROWER,
    WIZARD,
}

public class SkeletonBehiavor : MonoBehaviour
{
    public SkeletonType type;
    ObjectAnimation anim;
    MonsterMovement monsterMovement;
    bool doAction;

    private Coroutine currentCoroutine;
    private bool isCoroutineRunning = false;

    public GameObject shadow;
    private Vector3 originalShadowScale;

    // Thrower
    public GameObject bonePrefab;
    bool startAnimation = false;

    private bool doJump = false;
    private bool isJumping = false;
    private float jumpCooldown = 0f; // Nouveau: cooldown pour éviter les tentatives de saut en boucle

    private Stats stats;

    [SerializeField] private float jumpRadius = 3f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private AnimationCurve jumpCurve;
    // Ajoute cette variable dans la classe
    [SerializeField] private float jumpDetectionRadius = 4f;


    void Start()
    {
        anim = GetComponent<ObjectAnimation>();
        monsterMovement = GetComponent<MonsterMovement>();
        stats = GetComponent<Stats>();

        StartCoroutine(CheckIfStuck());

        if (shadow != null)
            originalShadowScale = shadow.transform.localScale;

        if (jumpCurve.keys.Length == 0)
        {
            // Créer une courbe qui monte puis descend
            Keyframe[] keys = new Keyframe[3];
            keys[0] = new Keyframe(0, 0, 0, 2);
            keys[1] = new Keyframe(0.5f, 1, 0, 0);
            keys[2] = new Keyframe(1, 0, -2, 0);
            jumpCurve = new AnimationCurve(keys);
        }

        if (type == SkeletonType.NONE)
            stats.doingAttack = true;
    }

    private void Update()
    {
        // Décrémenter le cooldown de saut s'il est actif
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }

        Animations(monsterMovement.GetDirection());

        // Vérifier si le saut est nécessaire
        if (doJump && !isJumping && !isCoroutineRunning && jumpCooldown <= 0 && PlayerInJumpRange())
        {
            Jump();
        }
        else if (type == SkeletonType.THROWER)
        {
            if (!doJump)
            {
                ThrowBone();

                // Lancer la coroutine si doAction est vrai et qu'elle n'est pas déjà en cours
                if (doAction && !isCoroutineRunning)
                {
                    currentCoroutine = StartCoroutine(RoutineThrowBone());
                }
            }
        }
        else if (type == SkeletonType.WIZARD)
        {
            if (!doJump)
            {
                WizardAttack();

                // Lancer la coroutine si doAction est vrai et qu'elle n'est pas déjà en cours
                if (doAction && !isCoroutineRunning)
                {
                    currentCoroutine = StartCoroutine(RoutineWizardAttack());
                }
            }
        }
    }

    private void Jump()
    {
        Vector3? target = GetFreePositionAround(transform.position, jumpRadius);
        if (target.HasValue)
        {
            // Empêcher de sauter à nouveau pendant le saut
            isJumping = true;
            StartCoroutine(JumpRoutine(target.Value));
        }
        else
        {
            // Pas de position trouvée, on met un cooldown mais on garde doJump actif pour réessayer
            jumpCooldown = 1f; // 1 seconde avant de réessayer de sauter
        }
    }

    private Vector3? GetFreePositionAround(Vector3 origin, float radius)
    {
        // Essayer avec un rayon variable si la première tentative échoue
        float[] radii = { radius, radius * 0.75f, radius * 1.25f };

        foreach (float currentRadius in radii)
        {
            int checks = 30; // Augmentation du nombre de directions testées
            // Pour randomiser la recherche, on commence à un angle aléatoire
            float startAngle = Random.Range(0f, 360f);

            for (int i = 0; i < checks; i++)
            {
                float angle = startAngle + (360f / checks) * i;
                Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                Vector3 checkPos = origin + dir * currentRadius;

                // Réduire légèrement le rayon de vérification pour être moins strict
                Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.4f);

                // Debug pour voir les positions testées
                Debug.DrawLine(origin, checkPos, Color.red, 1f);

                if (hit == null)
                {
                    return checkPos;
                }
            }
        }

        return null; // Aucune position libre trouvée
    }

    IEnumerator JumpRoutine(Vector3 targetPosition)
    {
        // Réduire l'ombre au début du saut
        StartCoroutine(ScaleShadow(.25f, false));

        GetComponent<Collider2D>().enabled = false;

        // Animation de saut
        Vector3 start = transform.position;
        Vector3 startSpritePos = GetComponentInChildren<SpriteRenderer>().gameObject.transform.localPosition;
        float duration = jumpDuration;
        float elapsed = 0f;

        try
        {
            GetComponent<SoundContainer>().PlaySound("SkeletonJump", 2);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Erreur lors de la lecture du son: " + e.Message);
        }

        bool shadowGrowing = false;

        // Boucle principale du saut
        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;

            // Position horizontale (déplacement vers la cible)
            transform.position = Vector3.Lerp(start, targetPosition, normalizedTime);

            // Position verticale du sprite (saut)
            float heightFactor = jumpCurve.Evaluate(normalizedTime);
            Vector3 spriteJumpPos = startSpritePos + new Vector3(0, jumpHeight * heightFactor, 0);
            GetComponentInChildren<SpriteRenderer>().gameObject.transform.localPosition = spriteJumpPos;

            // Commencer à agrandir l'ombre pendant la descente (après avoir atteint le point le plus haut)
            if (normalizedTime > 0.5f && !shadowGrowing)
            {
                shadowGrowing = true;
                StartCoroutine(ScaleShadow(duration * 0.5f, true)); // Agrandit l'ombre pendant la moitié restante du saut
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assurez-vous que tout est en position finale
        transform.position = targetPosition;
        GetComponentInChildren<SpriteRenderer>().gameObject.transform.localPosition = startSpritePos;


        // Réinitialisation des états à la fin
        GetComponent<Collider2D>().enabled = true;
        doJump = false;
        isJumping = false;
        jumpCooldown = 0.5f; // Cooldown réduit avant de pouvoir à nouveau sauter

    }

    public void Animations(Vector3 direction)
    {
        if (isCoroutineRunning && type == SkeletonType.THROWER && startAnimation)
        {
            GetComponent<ObjectAnimation>().PlayAnimation("Throw");
        }
        else if (isCoroutineRunning && type == SkeletonType.WIZARD && startAnimation)
        {
            GetComponent<ObjectAnimation>().PlayAnimation("Attack");
        }
        else if (direction.y > 0)
        {
            GetComponent<ObjectAnimation>().PlayAnimation("Up");
        }
        else if (direction.y < 0)
        {
            GetComponent<ObjectAnimation>().PlayAnimation("Down");
        }
        else if (direction.x != 0)
        {
            GetComponent<ObjectAnimation>().PlayAnimation("Side");
        }
        else
        {
            // Animation par défaut quand il n'y a pas de mouvement
            GetComponent<ObjectAnimation>().PlayAnimation("Down");
        }
    }

    private bool PlayerInJumpRange()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
            return false;

        float distance = Vector3.Distance(transform.position, PlayerManager.instance.player.transform.position);
        return distance <= jumpDetectionRadius;
    }


    public void ThrowBone()
    {
        // Reculer
        if (monsterMovement.GetDistanceToPlayer() < 2.5f)
        {
            monsterMovement.UpdateSpeed(1, true);
            monsterMovement.stopMonsterMovement = true;
            doAction = false;
        }
        // S'arrêter
        else if (monsterMovement.GetDistanceToPlayer() >= 2.5f && monsterMovement.GetDistanceToPlayer() < 6)
        {
            monsterMovement.stopMonsterMovement = true;
            monsterMovement.UpdateSpeed(0);
            doAction = true;
        }
        // Avancer
        else
        {
            monsterMovement.stopMonsterMovement = false;
            monsterMovement.ResetSpeed();
            doAction = false;
        }

        if (isCoroutineRunning)
            monsterMovement.UpdateSpeed(0);
    }

    public void WizardAttack()
    {
        // Reculer
        if (monsterMovement.GetDistanceToPlayer() < 2.5f)
        {
            monsterMovement.UpdateSpeed(1, true);
            monsterMovement.stopMonsterMovement = true;
            doAction = false;
        }
        // S'arrêter
        else if (monsterMovement.GetDistanceToPlayer() >= 2.5f && monsterMovement.GetDistanceToPlayer() < 5)
        {
            monsterMovement.stopMonsterMovement = true;
            monsterMovement.UpdateSpeed(0);
            doAction = true;
        }
        // Avancer
        else
        {
            monsterMovement.stopMonsterMovement = false;
            monsterMovement.ResetSpeed();
            doAction = false;
        }

        if (isCoroutineRunning)
            monsterMovement.UpdateSpeed(0);
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

    IEnumerator CheckIfStuck()
    {
        Vector3 lastPosition = transform.position;
        float stuckThreshold = 0.5f; // Augmentation du seuil pour être plus sensible à l'immobilité

        while (true)
        {
            yield return new WaitForSeconds(1f);

            float distance = Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;

            // Si le squelette est presque immobile et n'est pas déjà en train de sauter ou d'effectuer une action
            if (distance < stuckThreshold && !isJumping && !isCoroutineRunning)
            {
                doJump = true;
            }
        }
    }

    public GameObject wizardBone;

    IEnumerator RoutineWizardAttack()
    {
        isCoroutineRunning = true; // Marque la coroutine comme active

        // Temps d'attente aléatoire avant l'attaque
        yield return new WaitForSeconds(Random.Range(.5f, 3));

        GetComponent<SoundContainer>().PlaySound("Attack", 1);

        // Transition de couleur (rose) et d'intensité
        GetComponent<EntityLight>().TransitionLightColor(new Color(255, 0f, 200), 0.8f); // Rose : valeurs entre 0 et 1
        GetComponent<EntityLight>().TransitionLightIntensity(1f, 2f, 0.8f); // Intensité correcte

        startAnimation = true;

        if (PlayerManager.instance.player)
        {
            // Définir les dégâts de l'os en fonction de la force du sorcier
            int damage = GetComponent<Stats>().strength;

            int bonesToSpawn = Random.Range(3, 6);
            float delay = 1f / bonesToSpawn; // Durée totale de 1 seconde répartie entre les spawns

            for (int i = 0; i < bonesToSpawn; i++)
            {
                Vector3 playerPosition = PlayerManager.instance.player.transform.position;

                // Calculer une position aléatoire autour du joueur (entre 0 et 2 unités)
                Vector3 randomOffset = new Vector3(
                    Random.Range(-1.5f, 1.5f),
                    Random.Range(-1.5f, 1.5f),
                    0f
                );
                Vector3 spawnPosition = playerPosition + randomOffset;

                // Créer l'instance de l'os (wizardBone)
                GameObject boneInstance = Instantiate(wizardBone, spawnPosition, Quaternion.identity);
                boneInstance.GetComponent<ObjectPerspective>().level = GetComponent<ObjectPerspective>().level;

                // Configurer l'os
                boneInstance.GetComponent<WizardBone>().damage = damage;

                // Délai entre chaque spawn
                yield return new WaitForSeconds(delay);
            }
        }

        // Retour à la couleur blanche et diminution progressive de l'intensité
        GetComponent<EntityLight>().TransitionLightColor(new Color(1f, 1f, 1f), .5f); // Blanc
        GetComponent<EntityLight>().TransitionLightIntensity(0.25f, 1f, .5f);

        // Marque la fin de la coroutine
        isCoroutineRunning = false;
        startAnimation = false;
    }

    IEnumerator RoutineThrowBone()
    {
        isCoroutineRunning = true; // Marque la coroutine comme active

        // Temps d'attente aléatoire
        yield return new WaitForSeconds(Random.Range(.2f, 1));

        startAnimation = true;

        // Temps de l'attaque
        yield return new WaitForSeconds(.25f);

        if (PlayerManager.instance.player)
        {
            // Calculer la direction en fonction de la position du joueur et de l'ennemi
            Vector2 directionToPlayer = (PlayerManager.instance.player.transform.position - transform.position).normalized;
            float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x); // Angle en radians

            // Temps de l'attaque
            yield return new WaitForSeconds(.5f);

            // Créer l'instance de l'os (bone)
            GameObject boneInstance = Instantiate(bonePrefab, transform.position, Quaternion.identity);
            boneInstance.GetComponent<ObjectPerspective>().level = GetComponent<ObjectPerspective>().level;

            boneInstance.GetComponent<ProjectileBehavior>().InitProjectile(
                GetComponent<Stats>().strength,
                4,
                angleToPlayer, // Passer l'angle calculé vers le joueur
                false,
                GetComponent<Stats>().knockbackPower,
                this.gameObject
            );
        }

        // Marque la fin de la coroutine
        isCoroutineRunning = false;
        startAnimation = false;
    }
}