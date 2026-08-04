using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NewMonsterMovement))]
[RequireComponent(typeof(Stats))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LifeManager))]
[RequireComponent(typeof(ObjectAnimation))]
[RequireComponent(typeof(SoundContainer))]
[RequireComponent(typeof(EntityEffects))]
public class BigIceBumperBehiavor : MonoBehaviour
{
    [Header("Attack Timing")]
    public float attackIntervalMin = 3f;
    public float attackIntervalMax = 6f;
    public float attackAnimDuration = 1f;
    public float spinDuration = 5f;
    public float slowDownDuration = 1f;

    [Header("Spin Settings")]
    public float spinSpeedMultiplier = 4f;
    public string bounceSoundName = "Hit";

    [Header("Melting")]
    public float meltAnimDuration = 0.5f;
    public float fleeDuration = 4f;
    public float fleeSpeedMultiplier = 0.5f;

    [Header("Animations")]
    public string attackAnimation = "Attack";
    public string meltingAnimation = "Melting";
    public string sadAnimation = "Sad";

    [Header("Sounds & Particles")]
    public string meltingEffectName = "Melting";
    public string iceEffectName = "Ice";
    public int meltingParticleCount = 15;
    public int iceParticleCount = 15;
    public float particleSpreadRadius = 0.6f;

    private NewMonsterMovement monsterMovement;
    private Stats stats;
    private Rigidbody2D rb;
    private LifeManager lifeManager;
    private ObjectAnimation objectAnimation;
    private SoundContainer soundContainer;
    private EntityEffects entityEffects;
    private ObjectParticles objectParticles;
    private SpriteRenderer spriteRenderer;

    private Transform playerTransform;

    private bool hasIceBlock = true;
    private bool isSpinning = false;
    private Vector2 spinDirection = Vector2.zero;
    private float currentSpinSpeed = 0f;
    private float? speedBeforeAttackBuff = null;
    private Coroutine currentAttackSequence;

    private void Awake()
    {
        monsterMovement = GetComponent<NewMonsterMovement>();
        stats = GetComponent<Stats>();
        rb = GetComponent<Rigidbody2D>();
        lifeManager = GetComponent<LifeManager>();
        objectAnimation = GetComponent<ObjectAnimation>();
        soundContainer = GetComponent<SoundContainer>();
        entityEffects = GetComponent<EntityEffects>();
        objectParticles = GetComponent<ObjectParticles>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        StartCoroutine(MainLoop());
    }

    private void Update()
    {
        stats.blockPlayerAttack = hasIceBlock;

        if (playerTransform == null && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }
    }

    private IEnumerator MainLoop()
    {
        while (true)
        {
            hasIceBlock = true;
            monsterMovement.enabled = true;
            monsterMovement.EnableAnimations = true;

            Coroutine attackCycle = StartCoroutine(AttackCycleLoop());

            yield return new WaitUntil(() => stats.isDying || entityEffects.isFire);

            StopCoroutine(attackCycle);

            // AttackSequence est lancée via un StartCoroutine imbriqué dans AttackCycleLoop : l'arrêt de
            // attackCycle ci-dessus ne l'interrompt pas automatiquement, il faut l'arrêter explicitement
            // sous peine de la voir continuer (préparation ou tournoiement) pendant la fonte.
            if (currentAttackSequence != null)
            {
                StopCoroutine(currentAttackSequence);
                currentAttackSequence = null;
            }

            if (stats.isDying) yield break;

            yield return StartCoroutine(MeltAndRecoverSequence());
        }
    }

    private IEnumerator AttackCycleLoop()
    {
        while (true)
        {
            float wait = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(wait);

            if (monsterMovement != null && monsterMovement.IsInDetectionZone && playerTransform != null)
            {
                currentAttackSequence = StartCoroutine(AttackSequence());
                yield return currentAttackSequence;
                currentAttackSequence = null;
            }
        }
    }

    private IEnumerator AttackSequence()
    {
        stats.doingAttack = true;

        monsterMovement.enabled = false;
        monsterMovement.EnableAnimations = false;
        rb.velocity = Vector2.zero;

        // Vitesse de base quadruplée pendant toute la séquence d'attaque
        speedBeforeAttackBuff = stats.speed;
        stats.speed *= 4f;

        // 1. Anim d'attaque, immobile pendant 1s (StopAnimation force le redémarrage même si "Attack" était déjà l'animation figée précédente)
        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(attackAnimation, true);
        yield return new WaitForSeconds(attackAnimDuration);

        // 2. Tournoiement pendant 5s, façon IceBumper
        Vector2 toPlayer = playerTransform.position - transform.position;
        spinDirection = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : Vector2.down;
        currentSpinSpeed = stats.speed * spinSpeedMultiplier;
        isSpinning = true;

        float spinTimer = 0f;
        while (spinTimer < spinDuration)
        {
            rb.velocity = spinDirection * currentSpinSpeed;
            UpdateSpriteDirection(spinDirection);
            spinTimer += Time.deltaTime;
            yield return null;
        }

        isSpinning = false;

        // 3. Ralentissement progressif jusqu'à l'arrêt
        float startSpeed = currentSpinSpeed;
        float slowTimer = 0f;
        while (slowTimer < slowDownDuration)
        {
            float ratio = 1f - (slowTimer / slowDownDuration);
            rb.velocity = spinDirection * startSpeed * ratio;
            slowTimer += Time.deltaTime;
            yield return null;
        }
        rb.velocity = Vector2.zero;

        // 4. Anim d'attaque inversée (StopAnimation force le redémarrage malgré le même nom d'animation)
        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(attackAnimation, true, true);
        yield return new WaitForSeconds(attackAnimDuration);

        stats.speed = speedBeforeAttackBuff.Value;
        speedBeforeAttackBuff = null;

        stats.doingAttack = false;

        monsterMovement.enabled = true;
        monsterMovement.EnableAnimations = true;
    }

    private IEnumerator MeltAndRecoverSequence()
    {
        isSpinning = false;
        hasIceBlock = false;
        stats.doingAttack = false;

        // Au cas où le feu ait interrompu la séquence d'attaque avant la restauration de la vitesse
        if (speedBeforeAttackBuff.HasValue)
        {
            stats.speed = speedBeforeAttackBuff.Value;
            speedBeforeAttackBuff = null;
        }

        monsterMovement.enabled = false;
        monsterMovement.EnableAnimations = false;
        if (rb != null) rb.velocity = Vector2.zero;

        entityEffects.StopFire();

        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(meltingAnimation, true);
        soundContainer.PlaySound(meltingEffectName, 2);
        SpawnParticleBurst(meltingEffectName, meltingParticleCount);

        yield return new WaitForSeconds(meltAnimDuration);

        // Fuite pendant 4s, vitesse divisée par 2, anim "Sad"
        objectAnimation.PlayAnimation(sadAnimation);
        float fleeTimer = 0f;
        float fleeSpeed = stats.speed * fleeSpeedMultiplier;
        while (fleeTimer < fleeDuration)
        {
            if (playerTransform != null)
            {
                Vector2 awayFromPlayer = (transform.position - playerTransform.position);
                if (awayFromPlayer.sqrMagnitude > 0.001f)
                {
                    awayFromPlayer.Normalize();
                    transform.position += (Vector3)(awayFromPlayer * fleeSpeed * Time.deltaTime);
                    UpdateSpriteDirection(awayFromPlayer);
                }
            }

            fleeTimer += Time.deltaTime;
            yield return null;
        }

        // Reformation du bloc de glace : anim Melting inversée (StopAnimation force le redémarrage malgré le même nom d'animation)
        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(meltingAnimation, true, true);
        soundContainer.PlaySound(iceEffectName, 2);
        SpawnParticleBurst(iceEffectName, iceParticleCount);

        yield return new WaitForSeconds(meltAnimDuration);

        hasIceBlock = true;
    }

    private void SpawnParticleBurst(string id, int count)
    {
        if (objectParticles == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * particleSpreadRadius;
            objectParticles.SpawnParticle(id, transform.position + (Vector3)offset);
        }
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (spriteRenderer == null) return;

        if (Mathf.Abs(direction.x) > 0.1f)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isSpinning || collision == null || collision.contacts.Length == 0) return;

        bool isPlayer = IsPlayerCollision(collision.gameObject);

        if (isPlayer)
        {
            Vector2 awayFromPlayer = (transform.position - collision.transform.position).normalized;
            spinDirection = awayFromPlayer != Vector2.zero
                ? awayFromPlayer
                : Vector2.Reflect(spinDirection, collision.contacts[0].normal).normalized;

            // Garantit les dégâts dès le premier contact : si le joueur va dans la même direction que
            // l'entité, le rebond immédiat ci-dessus peut séparer les colliders en un seul FixedUpdate,
            // ce qui empêche LifeManager.OnCollisionStay2D de jamais se déclencher.
            if (lifeManager != null)
            {
                lifeManager.Attack(collision.gameObject);
            }
        }
        else
        {
            ContactPoint2D contact = collision.contacts[0];
            spinDirection = Vector2.Reflect(spinDirection, contact.normal).normalized;
        }

        if (soundContainer != null)
        {
            soundContainer.PlaySound(bounceSoundName, 1);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Si le tournoiement glisse le long d'un mur (ou d'un coin), le contact reste actif sans
        // jamais redéclencher OnCollisionEnter2D : sans correction ici, l'entité peut rester collée
        // en continuant de pousser dans le mur au lieu de rebondir.
        if (!isSpinning || collision == null || collision.contacts.Length == 0) return;

        if (IsPlayerCollision(collision.gameObject)) return;

        ContactPoint2D contact = collision.contacts[0];
        if (Vector2.Dot(spinDirection, contact.normal) < 0f)
        {
            spinDirection = Vector2.Reflect(spinDirection, contact.normal).normalized;
        }
    }

    private bool IsPlayerCollision(GameObject other)
    {
        return other.GetComponent<PlayerController>() != null ||
               (other.GetComponent<Stats>() != null &&
                other.GetComponent<Stats>().entityType == EntityType.Player);
    }
}
