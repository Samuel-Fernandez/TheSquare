using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBehiavor : MonoBehaviour
{
    [Header("Detection & Movement")]
    public float detectionRadius = 5f;
    public float dashSpeed = 10f;
    
    [Header("Thread Attack")]
    public float threadCooldownMin = 3f;
    public float threadCooldownMax = 6f;
    public GameObject threadPrefab;
    public Transform threadSpawnPoint;

    private float threadCooldownTimer;
    
    private enum SpiderState { Idle, Chasing, PreparingThread, ShootingThread, Dashing }
    private SpiderState currentState = SpiderState.Idle;

    private Stats stats;
    private SpriteRenderer spriteRenderer;
    private ObjectAnimation objectAnimation;
    private Collider2D spiderCollider;
    private GameObject currentThread;
    private Transform dashTarget;

    private void Start()
    {
        stats = GetComponent<Stats>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        objectAnimation = GetComponent<ObjectAnimation>();
        spiderCollider = GetComponent<Collider2D>();
        
        // Initialiser le premier timer
        threadCooldownTimer = Random.Range(threadCooldownMin, threadCooldownMax);
    }

    private void Update()
    {
        if (PlayerManager.instance?.player == null) return;
        if (MeteoManager.instance != null && !MeteoManager.instance.time) return; // Arrêt du temps éventuel

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerManager.instance.player.transform.position);

        switch (currentState)
        {
            case SpiderState.Idle:
                // Si le joueur est à moins de 5 unités, on s'active
                if (distanceToPlayer <= detectionRadius)
                {
                    currentState = SpiderState.Chasing;
                    threadCooldownTimer = Random.Range(threadCooldownMin, threadCooldownMax);
                }
                else
                {
                    // Optionnel : figer l'animation si idle
                    if (objectAnimation != null) objectAnimation.PlayAnimation("Down");
                }
                break;

            case SpiderState.Chasing:
                ChasePlayer();

                threadCooldownTimer -= Time.deltaTime;
                if (threadCooldownTimer <= 0f)
                {
                    // L'araignée s'arrête (pas d'animation de marche) et se prépare pendant 1 sec
                    currentState = SpiderState.PreparingThread;
                    threadCooldownTimer = 1f; // On réutilise le timer pour l'attente
                    if (objectAnimation != null) objectAnimation.PlayAnimation("Down");
                }
                break;

            case SpiderState.PreparingThread:
                // L'araignée ne bouge plus et prépare son tir
                threadCooldownTimer -= Time.deltaTime;
                if (threadCooldownTimer <= 0f)
                {
                    ShootThread();
                }
                break;

            case SpiderState.ShootingThread:
                // L'araignée ne bouge pas. On garde juste son orientation actuelle.
                // On attend que les callbacks OnThreadHitPlayer ou OnThreadMissed 
                // soient appelés par le fil (SpiderThreadBehiavor).
                break;

            case SpiderState.Dashing:
                DashTowardsTarget();
                break;
        }
    }

    private void ChasePlayer()
    {
        Vector3 playerPos = PlayerManager.instance.player.transform.position;
        Vector3 direction = (playerPos - transform.position).normalized;
        
        float currentSpeed = stats != null ? stats.speed : 2f;
        transform.position = Vector3.MoveTowards(transform.position, playerPos, currentSpeed * Time.deltaTime);
        
        UpdateAnimations(direction);
    }

    private void ShootThread()
    {
        currentState = SpiderState.ShootingThread;

        if (threadPrefab != null)
        {
            Vector3 spawnPos = threadSpawnPoint != null ? threadSpawnPoint.position : transform.position;
            currentThread = Instantiate(threadPrefab, spawnPos, Quaternion.identity);
            
            SpiderThreadBehiavor threadLogic = currentThread.GetComponent<SpiderThreadBehiavor>();
            if (threadLogic != null)
            {
                Vector3 playerPos = PlayerManager.instance.player.transform.position;
                threadLogic.InitThread(this, playerPos);
                
                // On peut jouer un petit son ici
                var sound = GetComponent<SoundContainer>();
                if (sound != null) sound.PlaySound("Spit", 1);
            }
        }
        else
        {
            // S'il n'y a pas de fil, on annule l'attaque (sécurité)
            OnThreadMissed();
        }
    }

    private void DashTowardsTarget()
    {
        if (dashTarget == null)
        {
            currentState = SpiderState.Chasing;
            return;
        }

        Vector3 targetPos = dashTarget.position;
        Vector3 direction = (targetPos - transform.position).normalized;
        
        transform.position = Vector3.MoveTowards(transform.position, targetPos, dashSpeed * Time.deltaTime);
        UpdateAnimations(direction);

        // On arrête le dash dès qu'on est suffisamment proche (compense le recul du joueur)
        if (Vector2.Distance(transform.position, targetPos) <= 1.2f)
        {
            // Dégâts ou effet bonus lors du contact après le dash
            var targetLife = dashTarget.GetComponent<LifeManager>();
            var myStats = GetComponent<Stats>();
            
            if (targetLife != null && myStats != null)
            {
                // On inflige les dégâts
                targetLife.TakeDamage(myStats.strength, gameObject, false);
            }

            // On finit le dash, on réactive le collider et on supprime le fil
            if (spiderCollider != null) spiderCollider.enabled = true;
            if (currentThread != null)
            {
                Destroy(currentThread);
            }
            
            currentState = SpiderState.Chasing;
            // Optionnel : s'assurer qu'elle ne bouge plus dans ce frame après impact
            threadCooldownTimer = Random.Range(threadCooldownMin, threadCooldownMax);

            if (myStats != null) StartCoroutine(BiteWindowRoutine(myStats));
        }
    }

    // Un court instant après avoir atteint le joueur (fin du dash), la morsure de l'araignée reste dangereuse au contact
    private IEnumerator BiteWindowRoutine(Stats spiderStats)
    {
        yield return new WaitForSeconds(0.15f);
        spiderStats.doingAttack = true;
        yield return new WaitForSeconds(0.3f);
        spiderStats.doingAttack = false;
    }

    public void OnThreadHitPlayer(Transform playerTransform)
    {
        if (currentState == SpiderState.ShootingThread)
        {
            dashTarget = playerTransform;
            currentState = SpiderState.Dashing;
            
            // On désactive le corps de l'araignée pendant qu'elle a le fil
            if (spiderCollider != null) spiderCollider.enabled = false;
            
            // Jouer le son de l'impact ou du dash
            var sound = GetComponent<SoundContainer>();
            if (sound != null) sound.PlaySound("Whip", 1);
        }
    }

    public void OnThreadMissed()
    {
        if (currentState == SpiderState.ShootingThread)
        {
            currentState = SpiderState.Chasing;
            if (spiderCollider != null) spiderCollider.enabled = true;
            threadCooldownTimer = Random.Range(threadCooldownMin, threadCooldownMax);
        }
    }

    private void UpdateAnimations(Vector3 direction)
    {
        if (objectAnimation == null) return;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            objectAnimation.PlayAnimation("Side");
            if (spriteRenderer != null)
            {
                // Si on se dirige vers la gauche, on flip le sprite (ajustez selon votre modèle de sprite)
                spriteRenderer.flipX = direction.x < 0; 
            }
        }
        else
        {
            if (direction.y > 0)
            {
                objectAnimation.PlayAnimation("Up");
            }
            else
            {
                objectAnimation.PlayAnimation("Down");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        // La portée maximale du fil est gérée dans SpiderThreadBehiavor mais on peut l'afficher
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
