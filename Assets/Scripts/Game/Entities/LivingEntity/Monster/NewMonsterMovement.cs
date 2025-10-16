using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Stats))]
public class NewMonsterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private float baseSpeed = 2f;
    private float speedMultiplier = 1f;
    private bool canMove = true;
    [SerializeField] private bool isReversed = false;
    [SerializeField] private bool spriteSideReversed = false;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;

    [Header("Random Movement")]
    [SerializeField] private bool enableRandomMovement = true;
    [SerializeField] private float randomMovementInterval = 2f;
    [SerializeField] private float randomMovementSpeed = 0.5f;

    [Header("Animation")]
    [SerializeField] private bool enableAnimations = true;
    [SerializeField] private string upAnimation = "Up";
    [SerializeField] private string downAnimation = "Down";
    [SerializeField] private string sideAnimation = "Side";

    [Header("Audio")]
    [SerializeField] private bool enableMovementSound = true;
    [SerializeField] private string movementSoundName = "Move";
    [SerializeField] private float soundIntervalMin = 1f;
    [SerializeField] private float soundIntervalMax = 5f;

    [Header("Advanced Settings")]
    [SerializeField] private bool useFixedUpdate = true;
    [SerializeField] private bool smoothRotation = false;
    [SerializeField] private float rotationSmoothness = 5f;

    // Private variables
    private Transform target;
    private Stats stats;
    private ObjectAnimation objectAnim;
    private SpriteRenderer spriteRenderer;
    private SoundContainer soundContainer;
    private Animator animator;

    private Vector3 direction = Vector3.zero;
    private Vector3 targetDirection = Vector3.zero;
    private float currentSpeed;
    private bool isInDetectionZone = false;
    private Coroutine randomMovementCoroutine;
    private Coroutine soundCoroutine;

    // Public properties
    public Vector3 Direction => direction;
    public float CurrentSpeed => currentSpeed;
    public bool IsInDetectionZone => isInDetectionZone;
    public bool CanMove { get => canMove; set => canMove = value; }
    public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = value; }
    public bool IsReversed { get => isReversed; set => isReversed = value; }
    public float DetectionRadius { get => detectionRadius; set => detectionRadius = value; }
    public bool EnableAnimations { get => enableAnimations; set => enableAnimations = value; }

    // Events
    public System.Action<bool> OnDetectionZoneChanged;
    public System.Action<Vector3> OnDirectionChanged;

    private void Start()
    {
        InitializeComponents();
        InitializeMovement();
    }

    private void InitializeComponents()
    {
        // Récupération des composants requis
        stats = GetComponent<Stats>();
        objectAnim = GetComponent<ObjectAnimation>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        soundContainer = GetComponent<SoundContainer>();
        animator = GetComponent<Animator>();

        // Récupération du joueur
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            target = PlayerManager.instance.player.transform;
        }

        // Configuration de la vitesse de base
        if (stats != null)
        {
            baseSpeed = stats.speed;
        }
        currentSpeed = baseSpeed;
    }

    private void InitializeMovement()
    {
        // Démarrage des coroutines
        if (enableRandomMovement)
        {
            StartRandomMovement();
        }

        if (enableMovementSound && soundContainer != null)
        {
            StartSoundRoutine();
        }
    }

    private void Update()
    {
        if (!useFixedUpdate)
        {
            UpdateDetection();
            UpdateMovement(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            UpdateDetection();
            UpdateMovement(Time.fixedDeltaTime);
        }
    }

    private void UpdateMovement(float deltaTime)
    {
        if (Time.timeScale == 0 || stats != null && !stats.canMove)
            return;

        UpdateDetection();
        UpdateDirection();
        ApplyMovement(deltaTime);
        UpdateVisuals();
    }

    private void UpdateDetection()
    {
        if (target == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        bool wasInZone = isInDetectionZone;
        isInDetectionZone = distanceToPlayer <= detectionRadius;

        if (wasInZone != isInDetectionZone)
        {
            OnDetectionZoneChanged?.Invoke(isInDetectionZone);

            if (isInDetectionZone)
            {
                StopRandomMovement();
            }
            else if (enableRandomMovement)
            {
                StartRandomMovement();
            }
        }
    }

    private void UpdateDirection()
    {
        if (target == null) return;

        Vector3 newDirection = Vector3.zero;

        if (isInDetectionZone)
        {
            // Direction vers ou loin du joueur
            Vector3 toPlayer = (target.position - transform.position).normalized;
            newDirection = isReversed ? -toPlayer : toPlayer;
        }

        // Application de la direction avec lissage optionnel
        if (smoothRotation && newDirection != Vector3.zero)
        {
            targetDirection = newDirection;
            direction = Vector3.Slerp(direction, targetDirection, rotationSmoothness * Time.deltaTime);
        }
        else
        {
            direction = newDirection;
        }

        // Notification du changement de direction
        if (direction != Vector3.zero)
        {
            OnDirectionChanged?.Invoke(direction);
        }
    }

    private void ApplyMovement(float deltaTime)
    {
        if (direction == Vector3.zero) return;

        // Vérifie si on suit un target précis
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // Ne rien faire si on est très proche de la cible
            if (distance < 0.05f)
            {
                direction = Vector3.zero;
                return;
            }
        }

        // Calcul de la vitesse actuelle
        float speedToUse = isInDetectionZone ? baseSpeed : (baseSpeed * randomMovementSpeed);
        currentSpeed = speedToUse * speedMultiplier * (GetComponent<EntityEffects>().isSlimed ? 0.5f : 1f);

        // Application du mouvement
        transform.position += direction * currentSpeed * deltaTime;
    }


    // Nouvelle méthode publique
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void UpdateVisuals()
    {
        if (direction == Vector3.zero) return;

        UpdateSpriteDirection();
        UpdateAnimations();
    }

    private void UpdateSpriteDirection()
    {
        if (spriteRenderer == null || lockFlip) return;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.flipX = (direction.x < 0) ^ spriteSideReversed;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }



    private void UpdateAnimations()
    {
        if (!enableAnimations) return;

        // Animation avec ObjectAnimation
        if (objectAnim != null)
        {
            string animationName = GetAnimationName();
            if (!string.IsNullOrEmpty(animationName))
            {
                objectAnim.PlayAnimation(animationName);
            }
        }

        // Animation avec Animator
        if (animator != null)
        {
            animator.SetFloat("DirectionX", direction.x);
            animator.SetFloat("DirectionY", direction.y);
            animator.SetBool("IsMoving", direction.magnitude > 0.1f);
        }
    }

    private string GetAnimationName()
    {
        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            return direction.y > 0 ? upAnimation : downAnimation;
        }
        else
        {
            return sideAnimation;
        }
    }

    [SerializeField] private bool lockFlip = false;

    public void LockFlip(bool value)
    {
        lockFlip = value;
    }


    #region Random Movement
    private void StartRandomMovement()
    {
        if (randomMovementCoroutine != null)
        {
            StopCoroutine(randomMovementCoroutine);
        }
        randomMovementCoroutine = StartCoroutine(RandomMovementRoutine());
    }

    private void StopRandomMovement()
    {
        if (randomMovementCoroutine != null)
        {
            StopCoroutine(randomMovementCoroutine);
            randomMovementCoroutine = null;
        }
    }

    private IEnumerator RandomMovementRoutine()
    {

        while (!isInDetectionZone && enableRandomMovement)
        {
            // Génération d'une direction aléatoire
            float randomX = Random.Range(-1f, 1f);
            float randomY = Random.Range(-1f, 1f);
            direction = new Vector3(randomX, randomY, 0).normalized;

            yield return new WaitForSeconds(randomMovementInterval);
        }

    }
    #endregion

    #region Sound
    private void StartSoundRoutine()
    {
        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
        }
        soundCoroutine = StartCoroutine(SoundRoutine());
    }

    private IEnumerator SoundRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(soundIntervalMin, soundIntervalMax));

            if (soundContainer != null && direction.magnitude > 0.1f)
            {
                soundContainer.PlaySound(movementSoundName, 2);
            }
        }
    }
    #endregion

    #region Public Methods
    public void SetSpeed(float newSpeed)
    {
        baseSpeed = newSpeed;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void SetReversed(bool reversed)
    {
        isReversed = reversed;
    }

    public void SetDetectionRadius(float radius)
    {
        detectionRadius = radius;
    }

    public void ForceDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;
        StopRandomMovement();
    }

    public void ResetToRandomMovement()
    {
        if (enableRandomMovement && !isInDetectionZone)
        {
            StartRandomMovement();
        }
    }

    public float GetDistanceToTarget()
    {
        if (target == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, target.position);
    }

    public void StopMovement()
    {
        canMove = false;
        direction = Vector3.zero;
        StopRandomMovement();
    }

    public void ResumeMovement()
    {
        canMove = true;
        if (enableRandomMovement && !isInDetectionZone)
        {
            StartRandomMovement();
        }
    }
    #endregion
}