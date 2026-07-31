using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NewMonsterMovement))]
[RequireComponent(typeof(Stats))]
[RequireComponent(typeof(ObjectAnimation))]
[RequireComponent(typeof(SoundContainer))]
public class IceWarriorBehiavor : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldownMin = 2f;
    public float attackCooldownMax = 5f;
    public float dashSpeedMultiplier = 6f;
    public float dashDuration = 1f;
    public float initialPrepareDuration = 1f;
    public float consecutivePrepareDuration = 0.5f;
    public float timeBetweenDashes = 0.5f;

    [Header("Sprite Orientations")]
    public bool walkSpriteReversed = true;  // Coche pour régler le moonwalk pendant la marche
    public bool reverseAttackSpriteSide = true; // Coche pour le sprite d'attaque

    [Header("Effects")]
    [Range(0f, 1f)] public float baseFreezeChance = 0.10f;
    [Range(0f, 1f)] public float dashFreezeChance = 0.50f;

    [Header("Spike Settings")]
    public GameObject spikePrefab;
    public int spikeCount = 8;
    public float spikeRadius = 1f;
    
    [Header("End Attack Effects")]
    public GameObject smokePrefab;
    public float shakeAmplitude = 3f;
    public float shakeFrequency = 3f;
    public float shakeDuration = 0.5f;

    private NewMonsterMovement _monsterMovement;
    private ObjectAnimation _objectAnim;
    private Stats _stats;
    private SoundContainer _soundContainer;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private EntityLight _entityLight;
    private TrailRenderer _trailRenderer;

    private bool _isAttacking = false;

    void Start()
    {
        _monsterMovement = GetComponent<NewMonsterMovement>();
        _objectAnim = GetComponent<ObjectAnimation>();
        _stats = GetComponent<Stats>();
        _soundContainer = GetComponent<SoundContainer>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _entityLight = GetComponent<EntityLight>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();

        if (_trailRenderer != null)
        {
            _trailRenderer.emitting = false;
        }

        if (_monsterMovement != null)
        {
            _monsterMovement.LockFlip(true); // On empêche NewMonsterMovement de gérer le flipX (qui bug).
        }

        StartCoroutine(BehaviorLoop());
        StartCoroutine(WalkSoundRoutine());
    }

    void Update()
    {
        // On gère manuellement le retournement du sprite pour corriger définitivement le moonwalk
        if (!_isAttacking && _monsterMovement != null && _monsterMovement.enabled)
        {
            Vector3 mDir = _monsterMovement.Direction;
            if (mDir.magnitude > 0.01f && Mathf.Abs(mDir.x) > Mathf.Abs(mDir.y))
            {
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.flipX = walkSpriteReversed ? mDir.x > 0 : mDir.x < 0;
                }
            }
        }
    }

    IEnumerator BehaviorLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(attackCooldownMin, attackCooldownMax);
            yield return new WaitForSeconds(waitTime);

            if (_monsterMovement != null && _monsterMovement.IsInDetectionZone && PlayerManager.instance?.player != null && _stats.canMove)
            {
                yield return StartCoroutine(AttackRoutine());
            }
        }
    }

    IEnumerator WalkSoundRoutine()
    {
        while (true)
        {
            if (!_isAttacking && _monsterMovement != null && _monsterMovement.enabled && _monsterMovement.Direction.magnitude > 0.1f)
            {
                if (_soundContainer != null)
                {
                    _soundContainer.PlaySound("Walks", 2);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator AttackRoutine()
    {
        _isAttacking = true;

        // POINT CLÉ : Désactiver complètement NewMonsterMovement pendant l'attaque
        if (_monsterMovement != null)
        {
            _monsterMovement.StopMovement();
            _monsterMovement.EnableAnimations = false;
            _monsterMovement.enabled = false; // Désactive le script pour empêcher tout conflit
        }

        // Arrêt immédiat de la physique (pour éviter qu'il glisse en commençant l'attaque)
        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
        }

        for (int i = 0; i < 3; i++)
        {
            if (PlayerManager.instance?.player == null) break;

            if (_soundContainer != null)
                _soundContainer.PlaySound("PrepareAttack", 2);

            // Pendant la préparation et le début du dash, les dégâts sur lui ne repoussent pas le joueur
            _stats.blockPlayerAttack = false;

            float currentPrepare = (i == 0) ? initialPrepareDuration : consecutivePrepareDuration;

            if (_entityLight != null)
            {
                _entityLight.SetLightColor(new Color(0.5f, 0.8f, 1f)); // Bleu glacier
                _entityLight.TransitionLightIntensity(1.5f, 2.5f, currentPrepare);
            }

            // --- Animation de préparation initiale ---
            Vector3 initialPlayerPos = PlayerManager.instance.player.transform.position;
            Vector2 initialDiff = initialPlayerPos - transform.position;

            if (Mathf.Abs(initialDiff.y) > Mathf.Abs(initialDiff.x))
            {
                if (initialDiff.y > 0) _objectAnim.PlayAnimation("AttackUp", true);
                else _objectAnim.PlayAnimation("AttackDown", true);
            }
            else
            {
                _objectAnim.PlayAnimation("AttackSide", true);
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.flipX = reverseAttackSpriteSide ? initialDiff.x > 0 : initialDiff.x < 0;
                }
            }
            // ------------------------------------------

            yield return new WaitForSeconds(currentPrepare);

            // Re-calcul juste avant de dasher
            Vector3 playerPos = PlayerManager.instance.player.transform.position;
            Vector3 dashDirection = (playerPos - transform.position).normalized;
            Vector2 diff = playerPos - transform.position;

            // Mise à jour de l'animation (sans la redémarrer si c'est la même)
            if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
            {
                if (diff.y > 0) _objectAnim.PlayAnimation("AttackUp", false);
                else _objectAnim.PlayAnimation("AttackDown", false);
            }
            else
            {
                _objectAnim.PlayAnimation("AttackSide", false);
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.flipX = reverseAttackSpriteSide ? diff.x > 0 : diff.x < 0;
                }
            }

            // Début du dash
            if (_soundContainer != null)
                _soundContainer.PlaySound("Attack", 2);

            if (_trailRenderer != null) _trailRenderer.emitting = true;

            if (_entityLight != null)
            {
                _entityLight.SetLightColor(Color.cyan);
                _entityLight.TransitionLightIntensity(3f, 4f, dashDuration);
            }

            float dashSpeed = _stats.speed * dashSpeedMultiplier;
            float elapsed = 0f;
            
            _stats.doingAttack = true;

            while (elapsed < dashDuration)
            {
                if (elapsed >= 0.5f && !_stats.blockPlayerAttack)
                {
                    _stats.blockPlayerAttack = true;
                }

                transform.position += dashDirection * dashSpeed * Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            _stats.doingAttack = false;

            if (_trailRenderer != null) _trailRenderer.emitting = false;

            if (_soundContainer != null)
            {
                _soundContainer.PlaySound("Impact", 2);
            }
            
            if (smokePrefab != null)
            {
                Instantiate(smokePrefab, transform.position, Quaternion.identity);
            }
            
            if (CameraManager.instance != null)
            {
                CameraManager.instance.ShakeCamera(shakeAmplitude, shakeFrequency, shakeDuration);
            }

            if (i < 2) 
            {
                // Pause entre chaque dash
                if (_entityLight != null)
                {
                    _entityLight.SetLightColor(Color.white);
                    _entityLight.TransitionLightIntensity(0.5f, 1.5f, 0.5f);
                }
                yield return new WaitForSeconds(timeBetweenDashes);
            }
        }

        // Fin des 3 dashs : apparition des spikes uniquement
        SpawnSpikesAround();
        
        if (_soundContainer != null)
        {
            _soundContainer.PlaySound("IceSpike", 2);
        }

        if (_entityLight != null)
        {
            _entityLight.SetLightColor(Color.white);
            _entityLight.TransitionLightIntensity(0.5f, 1.5f, 1f);
        }

        // Sécurité de fin
        _stats.blockPlayerAttack = true;
        _stats.doingAttack = false;
        _isAttacking = false;

        // Réactiver NewMonsterMovement 
        if (_monsterMovement != null)
        {
            _monsterMovement.enabled = true;
            _monsterMovement.EnableAnimations = true;
            _monsterMovement.ResumeMovement();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Stats targetStats = collision.gameObject.GetComponent<Stats>();
        if (targetStats != null && targetStats.entityType == EntityType.Player && targetStats.isVulnerable)
        {
            float freezeChance = _isAttacking ? dashFreezeChance : baseFreezeChance;

            EntityEffects effects = collision.gameObject.GetComponent<EntityEffects>();
            if (effects != null && !effects.isFreeze)
            {
                if (Random.value <= freezeChance)
                {
                    effects.SetState(0, false, true, false, false);
                }
            }
        }
    }

    private void SpawnSpikesAround()
    {
        if (spikePrefab == null) return;

        float angleStep = 360f / spikeCount;
        for (int i = 0; i < spikeCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 spawnOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spikeRadius;
            Vector3 spawnPos = transform.position + spawnOffset;

            Instantiate(spikePrefab, spawnPos, Quaternion.identity);
        }
    }
}
