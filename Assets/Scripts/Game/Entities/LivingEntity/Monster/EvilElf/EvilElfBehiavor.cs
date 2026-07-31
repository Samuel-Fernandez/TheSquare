using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Stats))]
[RequireComponent(typeof(NewMonsterMovement))]
[RequireComponent(typeof(ObjectAnimation))]
public class EvilElfBehiavor : MonoBehaviour
{
    [Header("Préfab du Cercle de Soin")]
    public GameObject healCirclePrefab;

    [Header("Paramètres de Détection")]
    public float playerActivationRadius = 6f; // Dist max pour activer l'elfe par rapport au joueur
    public float monsterHealRadius = 4f;     // Dist max pour repérer un monstre à soigner

    [Header("Paramètres d'Attaque")]
    public float minAttackInterval = 2f;
    public float maxAttackInterval = 5f;

    [Header("Sons")]
    public string attackSoundName = "Attack";

    private Stats stats;
    private NewMonsterMovement monsterMovement;
    private ObjectAnimation objectAnimation;
    private SoundContainer soundContainer;
    private Transform playerTransform;

    private bool isAttacking = false;
    private float nextAttackTimer = 0f;

    private void Awake()
    {
        stats = GetComponent<Stats>();
        monsterMovement = GetComponent<NewMonsterMovement>();
        objectAnimation = GetComponent<ObjectAnimation>();
        soundContainer = GetComponent<SoundContainer>();
    }

    private void Start()
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        ResetAttackTimer();
    }

    private void Update()
    {
        if (stats != null && stats.isDying) return;

        // Mise à jour de la référence du joueur si nécessaire
        if (playerTransform == null && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool isPlayerInRange = distanceToPlayer <= playerActivationRadius;

        // Si l'elfe est en train d'attaquer, il ne gère pas ses déplacements
        if (isAttacking) return;

        if (isPlayerInRange)
        {
            // Recherche du monstre le plus proche (excluant soi-même) dans le rayon de 4 unités
            GameObject nearestMonster = GetNearestMonsterInRadius(monsterHealRadius);

            if (nearestMonster != null)
            {
                // S'il y a un monstre à max 4 unités : on essaie de se rapprocher de lui
                monsterMovement.SetReversed(false);
                monsterMovement.SetTarget(nearestMonster.transform);
                monsterMovement.CanMove = true;
            }
            else
            {
                // S'il n'y a pas d'autres monstres : s'éloigner du joueur
                monsterMovement.SetReversed(true);
                monsterMovement.SetTarget(playerTransform);
                monsterMovement.CanMove = true;
            }

            // Gestion du déclenchement de l'attaque
            nextAttackTimer -= Time.deltaTime;
            if (nextAttackTimer <= 0f)
            {
                // On vérifie s'il y a un monstre à portée pour lancer l'attaque
                GameObject attackTargetMonster = GetNearestMonsterInRadius(monsterHealRadius);
                if (attackTargetMonster != null)
                {
                    StartCoroutine(PerformHealAttackRoutine(attackTargetMonster));
                }
                else
                {
                    // Si aucun monstre n'est là au moment d'attaquer, retester rapidement
                    nextAttackTimer = 0.5f;
                }
            }
        }
        else
        {
            // En dehors du rayon d'activation du joueur : arrêt du déplacement
            monsterMovement.CanMove = false;
        }
    }

    /// <summary>
    /// Recherche le monstre allié le plus proche dans le rayon donné.
    /// </summary>
    private GameObject GetNearestMonsterInRadius(float maxRadius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, maxRadius);
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D col in colliders)
        {
            if (col == null || col.gameObject == gameObject) continue;

            Stats targetStats = col.GetComponent<Stats>();

            // On s'assure qu'il s'agit bien d'un monstre ou boss vivant
            if (targetStats != null && !targetStats.isDying &&
                (targetStats.entityType == EntityType.Monster || targetStats.entityType == EntityType.Boss))
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = col.gameObject;
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// Coroutine d'attaque de soin sur 5 secondes.
    /// </summary>
    private IEnumerator PerformHealAttackRoutine(GameObject targetMonster)
    {
        isAttacking = true;

        // Arrêt des mouvements et des animations automatiques de marche
        monsterMovement.CanMove = false;
        monsterMovement.EnableAnimations = false;
        if (stats != null) stats.canMove = false;

        // 1. Préparation (1 seconde) : animation StartHeal
        objectAnimation.PlayAnimation("StartHeal");
        yield return new WaitForSeconds(1f);

        // Jouer le son Attack
        if (soundContainer != null)
        {
            soundContainer.PlaySound(attackSoundName, 1);
        }

        // Position de soin sous le monstre cible (si toujours présent), sinon sous l'Elf
        Vector3 spawnPosition = (targetMonster != null) ? targetMonster.transform.position : transform.position;

        // 2. Instanciation du HealCircleBehiavor s'il est configuré
        if (healCirclePrefab != null)
        {
            GameObject circleObj = Instantiate(healCirclePrefab, spawnPosition, Quaternion.identity);
            HealCircleBehiavor healCircle = circleObj.GetComponent<HealCircleBehiavor>();

            if (healCircle != null)
            {
                // valueHeal = 5, percentageHeal = true, frequency = 0.75, duration = 5, isAlly = false
                healCircle.Init(isAlly: false, percentageHeal: true, valueHeal: 5f, duration: 5f, frequency: 0.75f);
            }
        }
        else
        {
            Debug.LogWarning("HealCirclePrefab non assigné sur " + gameObject.name);
        }

        // 3. Attaque continue (4 secondes) : animation ContinuousHeal
        objectAnimation.PlayAnimation("ContinuousHeal");
        yield return new WaitForSeconds(4f);

        // 4. Fin de l'attaque : réactivation des mouvements et des animations automatiques
        monsterMovement.EnableAnimations = true;
        monsterMovement.CanMove = true;
        if (stats != null) stats.canMove = true;

        isAttacking = false;
        ResetAttackTimer();
    }

    private void ResetAttackTimer()
    {
        nextAttackTimer = Random.Range(minAttackInterval, maxAttackInterval);
    }

    private void OnDrawGizmosSelected()
    {
        // Gizmos d'activation par rapport au joueur (Jaune)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerActivationRadius);

        // Gizmos de soin par rapport aux monstres (Cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, monsterHealRadius);
    }
}
