using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiSquareBehaviour : MonoBehaviour
{
    [Header("References")]
    public GameObject laserPrefab;
    public Transform laserSpawnPoint;

    [Header("Phase 2 Settings")]
    public float bigLaserThickness = 3f;
    public float shakeAmplitude = 0.15f;
    public float shakeSpeed = 0.05f;

    [Header("Attack Settings")]
    public int minAttacksPhase1 = 4;
    public int maxAttacksPhase1 = 10;
    public float phase1WarningDuration = 0.6f;
    public float phase1LaserDuration = 0.3f;
    public float phase1TimeBetweenLasers = 0.6f;

    public int minAttacksPhase2 = 3;
    public int maxAttacksPhase2 = 3;
    public float phase2WarningDuration = 0.6f;
    public float phase2LaserDuration = 0.8f;
    public float phase2TimeBetweenLasers = 1f;

    public float randomTargetOffset = 1f;

    [Header("Cooldown Settings")]
    public float minTimeBetweenAttacksPhase1 = 2f;
    public float maxTimeBetweenAttacksPhase1 = 3f;
    public float minTimeBetweenAttacksPhase2 = 1f;
    public float maxTimeBetweenAttacksPhase2 = 3f;

    private Stats stats;
    private ObjectAnimation objectAnimation;
    private SoundContainer soundContainer;
    private NewMonsterMovement movement;
    private LifeManager lifeManager;
    private Transform spriteTransform;
    private Vector3 originalSpritePos;
    private EntityLight entityLight;

    private Color defaultLightColor = Color.white;
    private float defaultLightIntensity = 0.5f;
    private float defaultLightRadius = 1.5f;

    private bool isAttacking = false;
    private bool isInPhase2 = false;
    private bool isTransitioning = false;

    private Coroutine behaviorCoroutine;

    void Awake()
    {
        stats = GetComponent<Stats>();
        objectAnimation = GetComponent<ObjectAnimation>();
        soundContainer = GetComponent<SoundContainer>();
        movement = GetComponent<NewMonsterMovement>();
        lifeManager = GetComponent<LifeManager>();
        entityLight = GetComponent<EntityLight>();

        if (transform.childCount > 0)
        {
            spriteTransform = transform.GetChild(0);
        }
        else
        {
            spriteTransform = transform;
        }

        if (spriteTransform != null)
        {
            originalSpritePos = spriteTransform.localPosition;
        }

        if (entityLight != null && entityLight.entityLight != null)
        {
            var light2D = entityLight.entityLight.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
            if (light2D != null)
            {
                defaultLightColor = light2D.color;
                defaultLightIntensity = light2D.intensity;
                defaultLightRadius = light2D.pointLightOuterRadius;
            }
        }
    }

    void OnEnable()
    {
        if (behaviorCoroutine != null) StopCoroutine(behaviorCoroutine);
        behaviorCoroutine = StartCoroutine(BehaviorRoutine());
    }

    void OnDisable()
    {
        if (behaviorCoroutine != null) StopCoroutine(behaviorCoroutine);
        isAttacking = false;
        isTransitioning = false;
        SetMovement(true);
        SetVulnerable(true);
    }

    private void SetVulnerable(bool vulnerable)
    {
        if (stats != null)
        {
            stats.isVulnerable = vulnerable;
            stats.blockPlayerAttack = !vulnerable;
        }
    }

    private void SetMovement(bool canMove)
    {
        if (movement != null)
        {
            movement.enabled = canMove;
            movement.EnableAnimations = canMove;
            movement.CanMove = canMove;
        }
        else
        {
            var oldMovement = GetComponent<MonsterMovement>();
            if (oldMovement != null) oldMovement.enabled = canMove;
        }
        
        if (stats != null) stats.canMove = canMove;
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (!canMove)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    void Update()
    {
        if (PlayerManager.instance?.player == null || isTransitioning) return;

        if (!isInPhase2 && lifeManager != null && lifeManager.life <= stats.health / 2)
        {
            isInPhase2 = true;
            if (behaviorCoroutine != null) StopCoroutine(behaviorCoroutine);
            behaviorCoroutine = StartCoroutine(TransitionRoutine());
        }
    }

    private IEnumerator BehaviorRoutine()
    {
        while (true)
        {
            if (PlayerManager.instance?.player == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            if (!isAttacking && !isTransitioning)
            {
                if (isInPhase2)
                {
                    yield return new WaitForSeconds(Random.Range(minTimeBetweenAttacksPhase2, maxTimeBetweenAttacksPhase2));
                    if (!isTransitioning) yield return StartCoroutine(Phase2AttackRoutine());
                }
                else
                {
                    yield return new WaitForSeconds(Random.Range(minTimeBetweenAttacksPhase1, maxTimeBetweenAttacksPhase1));
                    if (!isTransitioning) yield return StartCoroutine(Phase1AttackRoutine());
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator Phase1AttackRoutine()
    {
        isAttacking = true;
        SetVulnerable(false);
        SetMovement(false);

        if (entityLight != null)
        {
            entityLight.SetLightColor(new Color(0.6f, 0f, 1f)); // Violet
            entityLight.TransitionLightIntensity(2f, 3f, 0.5f);
        }

        int laserCount = Random.Range(minAttacksPhase1, maxAttacksPhase1 + 1);
        for (int i = 0; i < laserCount; i++)
        {
            if (PlayerManager.instance?.player == null) break;

            objectAnimation.PlayAnimation("LittleAttack");
            soundContainer.PlaySound("LittleAttack", 1);
            
            StartCoroutine(FireLaserWithWarning(1f, phase1WarningDuration, phase1LaserDuration));
            
            yield return new WaitForSeconds(phase1TimeBetweenLasers);
        }
        
        yield return new WaitForSeconds(0.3f);

        objectAnimation.PlayAnimation("LittleIdle");
        soundContainer.PlaySound("LittleIdle", 1);
        
        if (entityLight != null)
        {
            entityLight.SetLightColor(defaultLightColor);
            entityLight.TransitionLightIntensity(defaultLightIntensity, defaultLightRadius, 0.5f);
        }

        SetVulnerable(true);
        SetMovement(true);
        isAttacking = false;
    }

    private IEnumerator TransitionRoutine()
    {
        isAttacking = true;
        isTransitioning = true;
        SetVulnerable(false);
        SetMovement(false);

        objectAnimation.PlayAnimation("Transition");
        soundContainer.PlaySound("Transition", 1);
        
        if (entityLight != null)
        {
            entityLight.SetLightIntensity(3f, 5f);
            entityLight.TransitionLightIntensity(defaultLightIntensity, defaultLightRadius, 1f); 
        }

        yield return new WaitForSeconds(1f);

        isTransitioning = false;
        isAttacking = false;
        SetVulnerable(true);
        
        objectAnimation.PlayAnimation("BigIdle");

        SetMovement(true);

        behaviorCoroutine = StartCoroutine(BehaviorRoutine());
    }

    private IEnumerator Phase2AttackRoutine()
    {
        isAttacking = true;
        SetVulnerable(false);
        SetMovement(false);

        if (entityLight != null)
        {
            entityLight.SetLightColor(new Color(0.6f, 0f, 1f)); // Violet
            entityLight.TransitionLightIntensity(2f, 3f, 0.5f);
        }

        objectAnimation.PlayAnimation("BigAttack", true);

        yield return new WaitForSeconds(1f);

        soundContainer.PlaySound("BigAttack", 1);
        
        Coroutine shakeCoroutine = null;
        if (spriteTransform != null) 
        {
            shakeCoroutine = StartCoroutine(ShakeSpriteRoutine(originalSpritePos));
        }
        
        yield return new WaitForSeconds(1f);

        int laserCount = Random.Range(minAttacksPhase2, maxAttacksPhase2 + 1);
        for (int i = 0; i < laserCount; i++)
        {
            if (PlayerManager.instance?.player == null) break;

            StartCoroutine(FireLaserWithWarning(bigLaserThickness, phase2WarningDuration, phase2LaserDuration));

            yield return new WaitForSeconds(phase2TimeBetweenLasers);
        }
        
        yield return new WaitForSeconds(0.8f);

        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        if (spriteTransform != null) spriteTransform.localPosition = originalSpritePos;

        objectAnimation.PlayAnimation("BigIdle");
        soundContainer.PlaySound("BigIdle", 1);

        if (entityLight != null)
        {
            entityLight.SetLightColor(defaultLightColor);
            entityLight.TransitionLightIntensity(defaultLightIntensity, defaultLightRadius, 0.5f);
        }

        SetVulnerable(true);
        SetMovement(true);
        isAttacking = false;
    }

    private IEnumerator FireLaserWithWarning(float thickness, float warningDuration, float laserDuration)
    {
        if (laserSpawnPoint == null || PlayerManager.instance?.player == null) yield break;

        Vector2 startPos = laserSpawnPoint.position;
        Vector2 targetPos = (Vector2)PlayerManager.instance.player.transform.position;
        targetPos.x += Random.Range(-randomTargetOffset, randomTargetOffset);
        targetPos.y += Random.Range(-randomTargetOffset, randomTargetOffset);

        Vector2 direction = (targetPos - startPos).normalized;
        Vector2 endPos = targetPos + direction * 10f; // Continue 10 units past the target

        LineRenderer warningLine = DrawWarningLine(startPos, endPos, thickness);

        // Fade in effect
        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            if (warningLine != null)
            {
                float alpha = Mathf.Lerp(0f, 0.5f, elapsed / warningDuration);
                warningLine.startColor = new Color(0.6f, 0f, 1f, alpha);
                warningLine.endColor = new Color(0.6f, 0f, 1f, alpha);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (warningLine != null) Destroy(warningLine.gameObject);

        FireLaser(thickness, endPos, laserDuration);
    }

    private LineRenderer DrawWarningLine(Vector2 start, Vector2 end, float thickness)
    {
        GameObject lineObj = new GameObject("LaserWarningLine");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        
        float visualThickness = thickness * 0.2f;
        if (visualThickness < 0.1f) visualThickness = 0.1f;

        lr.startWidth = visualThickness;
        lr.endWidth = visualThickness;
        
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0.6f, 0f, 1f, 0f); // Starts at 0 alpha for fade in
        lr.endColor = new Color(0.6f, 0f, 1f, 0f);
        
        lr.sortingOrder = 10;
        
        return lr;
    }

    private void FireLaser(float thickness, Vector2? forcedTarget = null, float duration = 0.5f)
    {
        if (laserPrefab == null || laserSpawnPoint == null) return;

        Vector2 targetPos = forcedTarget ?? (Vector2)PlayerManager.instance.player.transform.position;

        GameObject laser = Instantiate(laserPrefab, laserSpawnPoint.position, Quaternion.identity);
        LaserBehavior laserBehavior = laser.GetComponent<LaserBehavior>();

        if (laserBehavior != null)
        {
            int damage = isInPhase2 ? stats.strength * 2 : stats.strength;
            laserBehavior.Init(laserSpawnPoint.position, targetPos, damage, gameObject);

            Transform laserVisual = laser.transform.GetChild(0);
            if (laserVisual != null)
            {
                Vector3 scale = laserVisual.localScale;
                scale.x = thickness;
                laserVisual.localScale = scale;
            }
            
            Destroy(laser, duration);
        }
    }

    private IEnumerator ShakeSpriteRoutine(Vector3 originalPos)
    {
        if (spriteTransform == null) yield break;

        while (true)
        {
            float offsetX = Random.Range(-shakeAmplitude, shakeAmplitude);
            float offsetY = Random.Range(-shakeAmplitude, shakeAmplitude);
            spriteTransform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            yield return new WaitForSeconds(shakeSpeed);
        }
    }
}
