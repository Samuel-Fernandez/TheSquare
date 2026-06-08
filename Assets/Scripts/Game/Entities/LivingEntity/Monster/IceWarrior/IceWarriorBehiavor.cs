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
    public float dashSpeedMultiplier = 3f;
    public float dashDuration = 1f;
    public float prepareDuration = 1f;
    
    [Header("Sprite Orientations")]
    public bool walkSpriteReversed = true;  // Coche pour régler le moonwalk pendant la marche
    public bool reverseAttackSpriteSide = true; // Coche pour le sprite d'attaque
    
    [Header("Effects")]
    [Range(0f, 1f)] public float baseFreezeChance = 0.10f;
    [Range(0f, 1f)] public float dashFreezeChance = 0.50f;

    private NewMonsterMovement _monsterMovement;
    private ObjectAnimation _objectAnim;
    private Stats _stats;
    private SoundContainer _soundContainer;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;

    private bool _isAttacking = false;

    void Start()
    {
        _monsterMovement = GetComponent<NewMonsterMovement>();
        _objectAnim = GetComponent<ObjectAnimation>();
        _stats = GetComponent<Stats>();
        _soundContainer = GetComponent<SoundContainer>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();

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

        if (_soundContainer != null)
            _soundContainer.PlaySound("PrepareAttack", 2);

        Vector3 playerPos = PlayerManager.instance.player.transform.position;
        Vector3 dashDirection = (playerPos - transform.position).normalized;
        Vector2 diff = playerPos - transform.position;

        // Choix de l'animation par rapport au joueur + lastSprite à true
        if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
        {
            if (diff.y > 0) _objectAnim.PlayAnimation("AttackUp", true);
            else _objectAnim.PlayAnimation("AttackDown", true);
        }
        else
        {
            _objectAnim.PlayAnimation("AttackSide", true);
            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = reverseAttackSpriteSide ? diff.x > 0 : diff.x < 0;
            }
        }

        // Pendant la préparation et le début du dash, les dégâts sur lui ne repoussent pas le joueur
        _stats.blockPlayerAttack = false;

        yield return new WaitForSeconds(prepareDuration);

        // Début du dash
        if (_soundContainer != null)
            _soundContainer.PlaySound("Attack", 2);

        float dashSpeed = _stats.speed * dashSpeedMultiplier;
        float elapsed = 0f;

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

        // Sécurité de fin
        _stats.blockPlayerAttack = true;
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
}
