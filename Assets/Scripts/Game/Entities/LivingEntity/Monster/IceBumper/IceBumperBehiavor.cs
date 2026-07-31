using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Stats), typeof(Rigidbody2D), typeof(LifeManager))]
public class IceBumperBehiavor : MonoBehaviour
{
    [Header("Paramètres de Détection & Charge")]
    public float detectionRadius = 5f;
    public float chargeDuration = 1f;
    [Range(0f, 1f)]
    public float freezeChance = 0.2f;

    [Header("Animations")]
    public string idleAnimation = "Idle";
    public string turnAnimation = "Turn";

    [Header("Sons")]
    public string bounceSoundName = "Hurt";

    private Stats stats;
    private Rigidbody2D rb;
    private LifeManager lifeManager;
    private ObjectAnimation objectAnimation;
    private SoundContainer soundContainer;
    private SpriteRenderer spriteRenderer;

    private Transform playerTransform;
    private Vector2 moveDirection = Vector2.zero;

    private bool isActivated = false;
    private bool isRushing = false;
    private float currentSpeed = 0f;

    private void Awake()
    {
        stats = GetComponent<Stats>();
        rb = GetComponent<Rigidbody2D>();
        lifeManager = GetComponent<LifeManager>();
        objectAnimation = GetComponent<ObjectAnimation>();
        soundContainer = GetComponent<SoundContainer>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        if (objectAnimation != null)
        {
            objectAnimation.PlayAnimation(idleAnimation);
        }
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

        // 1. Détection initiale du joueur tant qu'il n'est pas activé
        if (!isActivated)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= detectionRadius)
            {
                StartCoroutine(ActivateAndRushRoutine());
            }
            return;
        }

        // 2. Calcul dynamique de la vitesse (moins il a de PV, plus il va vite jusqu'à 4x sa vitesse de base)
        UpdateSpeed();

        // 3. Orientation du sprite (FlipX selon la direction horizontale)
        UpdateSpriteDirection();
    }

    private void FixedUpdate()
    {
        if (stats != null && (!stats.canMove || stats.isDying))
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        // Application de la vitesse rectiligne si en phase de rush
        if (isRushing && rb != null)
        {
            rb.velocity = moveDirection * currentSpeed;
        }
    }

    /// <summary>
    /// Coroutine d'activation : passe 1s en animation "Turn" puis commence à foncer indéfiniment.
    /// </summary>
    private IEnumerator ActivateAndRushRoutine()
    {
        isActivated = true;

        if (rb != null) rb.velocity = Vector2.zero;

        // Lancer l'animation "Turn" pendant 1 seconde
        if (objectAnimation != null)
        {
            objectAnimation.PlayAnimation(turnAnimation);
        }

        yield return new WaitForSeconds(chargeDuration);

        // Calculer la direction initiale vers le joueur au moment de s'élancer
        if (playerTransform != null)
        {
            Vector2 toPlayer = (playerTransform.position - transform.position);
            moveDirection = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : Vector2.down;
        }
        else
        {
            moveDirection = Vector2.down;
        }

        isRushing = true;
        UpdateSpeed();
    }

    /// <summary>
    /// Calcule la vitesse en fonction du pourcentage de vie restante.
    /// Vitesse de base = stats.speed
    /// Vitesse maximale = stats.speed * 4f
    /// </summary>
    private void UpdateSpeed()
    {
        if (stats == null || lifeManager == null) return;

        float baseSpeed = stats.speed;
        float maxSpeed = baseSpeed * 4f;

        float healthPercentage = (float)lifeManager.life / (stats.health <= 0 ? 1 : stats.health);
        healthPercentage = Mathf.Clamp01(healthPercentage);

        // Moins il a de vie (healthPercentage proche de 0), plus la vitesse se rapproche de maxSpeed
        currentSpeed = Mathf.Lerp(maxSpeed, baseSpeed, healthPercentage);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isRushing || collision == null || collision.contacts.Length == 0) return;

        bool isPlayer = collision.gameObject.GetComponent<PlayerController>() != null ||
                        (collision.gameObject.GetComponent<Stats>() != null &&
                         collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player);

        if (isPlayer)
        {
            // 20% de chance de glacer le joueur au contact
            EntityEffects effects = collision.gameObject.GetComponent<EntityEffects>();
            if (effects != null && !effects.isFreeze)
            {
                if (Random.value <= freezeChance)
                {
                    effects.SetState(isFreeze: true);
                }
            }

            // S'il touche le joueur : rebond à l'opposé du joueur
            Vector2 awayFromPlayer = (transform.position - collision.transform.position).normalized;
            if (awayFromPlayer != Vector2.zero)
            {
                moveDirection = awayFromPlayer;
            }
            else
            {
                ContactPoint2D contact = collision.contacts[0];
                moveDirection = Vector2.Reflect(moveDirection, contact.normal).normalized;
            }
        }
        else
        {
            // S'il touche un obstacle / mur : calcul du vecteur réfléchi par la normale du contact
            ContactPoint2D contact = collision.contacts[0];
            moveDirection = Vector2.Reflect(moveDirection, contact.normal).normalized;
        }

        // Jouer le son "Hurt" à chaque impact/rebond
        if (soundContainer != null)
        {
            soundContainer.PlaySound(bounceSoundName, 1);
        }
    }

    /// <summary>
    /// Appelé lors de la prise de dégâts (notamment coup d'épée) pour repousser l'IceBumper à l'opposé du joueur.
    /// </summary>
    public void OnDamageTaken()
    {
        if (playerTransform == null && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        if (playerTransform != null && isRushing)
        {
            Vector2 awayFromPlayer = (transform.position - playerTransform.position).normalized;
            if (awayFromPlayer != Vector2.zero)
            {
                moveDirection = awayFromPlayer;
            }

            if (soundContainer != null)
            {
                soundContainer.PlaySound(bounceSoundName, 1);
            }
        }
    }

    private void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f)
        {
            spriteRenderer.flipX = moveDirection.x < 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Zone de détection du joueur (Rouge)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
