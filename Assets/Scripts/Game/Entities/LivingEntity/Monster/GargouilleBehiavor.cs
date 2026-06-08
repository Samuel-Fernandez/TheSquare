using System.Collections;
using UnityEngine;

public class GargouilleBehiavor : MonoBehaviour
{
    private NewMonsterMovement movement;
    private SpriteRenderer spriteRenderer;
    private Stats stats;
    private ObjectAnimation objectAnim;
    private SoundContainer soundContainer;
    private Collider2D col2d;

    private Vector3 originPosition;
    private Vector3 initialSpritePos;

    public enum State { Asleep, Activating, Flying, Dropping, Dropped, Returning }
    [Header("Current State (Debug)")]
    [SerializeField] private State currentState = State.Asleep;

    [Header("Gargouille Settings")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float returnDistance = 6f; // Distance du JOUEUR par rapport à l'origine
    [SerializeField] private float dropTriggerDistance = 1.2f; // Distance pour que la gargouille écrase le joueur
    
    [Header("Timers & Durations")]
    [SerializeField] private float activationDuration = 1f;
    [SerializeField] private float flyTimeMin = 2.0f;
    [SerializeField] private float prepareToDropShakeDuration = 0.5f;
    [SerializeField] private float takeOffDuration = 0.4f;
    [SerializeField] private float dropDuration = 0.2f;
    [SerializeField] private float droppedWaitTime = 1f;

    [Header("Visual Settings")]
    [SerializeField] private float flyHeightOffset = 1f;
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private GameObject shadowObject;

    [Header("Audio Settings")]
    [SerializeField] private float flySoundInterval = 0.8f;
    
    private float flyTimer = 0f;
    private float flySoundTimer = 0f;
    private Coroutine actionRoutine;

    private void Start()
    {
        movement = GetComponent<NewMonsterMovement>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        stats = GetComponent<Stats>();
        objectAnim = GetComponent<ObjectAnimation>();
        soundContainer = GetComponent<SoundContainer>();
        col2d = GetComponent<Collider2D>();

        originPosition = transform.position;
        if (spriteRenderer != null)
        {
            initialSpritePos = spriteRenderer.transform.localPosition;
        }

        if (activationDuration <= 0f) activationDuration = 1f;
        if (flyTimeMin <= 0f) flyTimeMin = 2.0f;
        if (prepareToDropShakeDuration <= 0f) prepareToDropShakeDuration = 0.5f;
        if (takeOffDuration <= 0f) takeOffDuration = 0.4f;
        if (dropDuration <= 0f) dropDuration = 0.2f;
        if (droppedWaitTime <= 0f) droppedWaitTime = 1f;
        if (returnSpeed <= 0f) returnSpeed = 2f;
        if (dropTriggerDistance <= 0f) dropTriggerDistance = 1.2f;

        if (movement != null)
        {
            movement.EnableAnimations = false; 
            movement.enabled = false; 
        }
        
        SetHitboxActive(false);
        if (shadowObject != null) shadowObject.SetActive(false);
        
        currentState = State.Asleep;
    }

    private void Update()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null) return;
        if (stats != null && stats.isDying) return;

        // La condition de retour est fixée sur la distance du JOUEUR vis à vis du Point de Départ
        float playerDistToOrigin = Vector3.Distance(PlayerManager.instance.player.transform.position, originPosition);

        // Déclencher le retour si le joueur dépasse la zone
        if (playerDistToOrigin > returnDistance && currentState != State.Asleep && currentState != State.Returning)
        {
            SetState(State.Returning, ReturnRoutine());
            return;
        }

        switch (currentState)
        {
            case State.Asleep:
                float distToPlayerToWake = Vector3.Distance(transform.position, PlayerManager.instance.player.transform.position);
                if (distToPlayerToWake <= detectionRadius)
                {
                    SetState(State.Activating, ActivateRoutine());
                }
                break;

            case State.Flying:
                flyTimer += Time.deltaTime;
                flySoundTimer -= Time.deltaTime;
                
                if (flySoundTimer <= 0f)
                {
                    if (soundContainer != null) soundContainer.PlaySound("Fly", 1);
                    flySoundTimer = flySoundInterval; // Repète le cycle sonore
                }

                UpdateSpriteFlip(); 
                
                // Vérifier si la créature vole depuis au moins 2 secondes ET que le joueur se situe juste en dessous
                float currentDistToPlayer = Vector3.Distance(transform.position, PlayerManager.instance.player.transform.position);
                if (flyTimer >= flyTimeMin && currentDistToPlayer <= dropTriggerDistance)
                {
                    SetState(State.Dropping, PrepareAndDropRoutine());
                }
                break;

            case State.Dropped:
                UpdateSpriteFlip();
                break;
        }
    }

    private void SetState(State newState, IEnumerator routine)
    {
        currentState = newState;
        if (actionRoutine != null) StopCoroutine(actionRoutine);
        if (routine != null) actionRoutine = StartCoroutine(routine);
    }
    
    private void SetHitboxActive(bool active)
    {
        if (col2d != null) col2d.enabled = active;
        if (stats != null) stats.isVulnerable = active;
    }

    private void UpdateSpriteFlip()
    {
        if (movement != null && movement.enabled && movement.Direction != Vector3.zero && spriteRenderer != null)
        {
            if (Mathf.Abs(movement.Direction.x) > 0.1f)
            {
                spriteRenderer.flipX = movement.Direction.x < 0;
            }
        }
    }

    private IEnumerator ActivateRoutine()
    {
        if (movement != null) movement.enabled = false;
        SetHitboxActive(true); 
        if (shadowObject != null) shadowObject.SetActive(true);

        if (objectAnim != null) objectAnim.PlayAnimation("Activate");
        if (soundContainer != null) soundContainer.PlaySound("Activate", 1);

        float t = 0;
        while (t < activationDuration)
        {
            t += Time.deltaTime;
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localPosition = initialSpritePos + (Vector3)Random.insideUnitCircle * shakeIntensity;
            }
            yield return null;
        }

        if (spriteRenderer != null) spriteRenderer.transform.localPosition = initialSpritePos;

        if (objectAnim != null) objectAnim.PlayAnimation("Fly");
        SetHitboxActive(false);

        t = 0;
        Vector3 targetPos = initialSpritePos + new Vector3(0, flyHeightOffset, 0);
        while (t < takeOffDuration)
        {
            t += Time.deltaTime;
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localPosition = Vector3.Lerp(initialSpritePos, targetPos, t / takeOffDuration);
            }
            yield return null;
        }
        if (spriteRenderer != null) spriteRenderer.transform.localPosition = targetPos;

        StartFlying();
    }

    private void StartFlying()
    {
        currentState = State.Flying;
        flyTimer = 0f;
        flySoundTimer = 0f;
        
        SetHitboxActive(false); 
        if (movement != null) movement.enabled = true; 
    }

    private IEnumerator PrepareAndDropRoutine()
    {
        if (movement != null) movement.enabled = false; 
        SetHitboxActive(false);

        Vector3 floatingPos = initialSpritePos + new Vector3(0, flyHeightOffset, 0);
        
        float t = 0;
        while (t < prepareToDropShakeDuration)
        {
            t += Time.deltaTime;
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localPosition = floatingPos + (Vector3)Random.insideUnitCircle * shakeIntensity;
            }
            yield return null;
        }

        // Fige le sprite de chute sur sa dernière frame (lastSpriteStay à true)
        if (objectAnim != null) objectAnim.PlayAnimation("Falling", true);

        t = 0;
        while (t < dropDuration)
        {
            t += Time.deltaTime;
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localPosition = Vector3.Lerp(floatingPos, initialSpritePos, t / dropDuration);
            }
            yield return null;
        }
        
        if (spriteRenderer != null) spriteRenderer.transform.localPosition = initialSpritePos;
        SetHitboxActive(true); 
        if (soundContainer != null) soundContainer.PlaySound("Drop", 1);

        currentState = State.Dropped;
        
        yield return new WaitForSeconds(droppedWaitTime);

        if (currentState == State.Dropped)
        {
            SetState(State.Activating, TakeOffAndFlyRoutine());
        }
    }

    private IEnumerator TakeOffAndFlyRoutine()
    {
        if (movement != null) movement.enabled = false;
        SetHitboxActive(false); 
        if (objectAnim != null) objectAnim.PlayAnimation("Fly");
        
        float t = 0;
        Vector3 targetPos = initialSpritePos + new Vector3(0, flyHeightOffset, 0);
        while (t < takeOffDuration)
        {
            t += Time.deltaTime;
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localPosition = Vector3.Lerp(initialSpritePos, targetPos, t / takeOffDuration);
            }
            yield return null;
        }
        if (spriteRenderer != null) spriteRenderer.transform.localPosition = targetPos;

        StartFlying();
    }

    private IEnumerator ReturnRoutine()
    {
        if (movement != null) movement.enabled = false; 
        SetHitboxActive(false); 

        if (objectAnim != null) objectAnim.PlayAnimation("Fly");
        
        if (spriteRenderer != null && spriteRenderer.transform.localPosition.y < initialSpritePos.y + flyHeightOffset - 0.1f)
        {
            float t = 0;
            Vector3 startP = spriteRenderer.transform.localPosition;
            Vector3 targetP = initialSpritePos + new Vector3(0, flyHeightOffset, 0);
            while (t < takeOffDuration)
            {
                t += Time.deltaTime;
                spriteRenderer.transform.localPosition = Vector3.Lerp(startP, targetP, t / takeOffDuration);
                yield return null;
            }
        }

        Vector3 spriteReturnPos = initialSpritePos + new Vector3(0, flyHeightOffset, 0);
        if (spriteRenderer != null) spriteRenderer.transform.localPosition = spriteReturnPos;

        while (Vector3.Distance(transform.position, originPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originPosition, returnSpeed * Time.deltaTime);
            UpdateReturnSpriteFlip();
            
            flySoundTimer -= Time.deltaTime;
            if (flySoundTimer <= 0f)
            {
                 if (soundContainer != null) soundContainer.PlaySound("Fly", 1);
                 flySoundTimer = flySoundInterval;
            }
            yield return null;
        }

        transform.position = originPosition;

        if (spriteRenderer != null)
        {
            float t = 0;
            while(t < dropDuration)
            {
                t += Time.deltaTime;
                spriteRenderer.transform.localPosition = Vector3.Lerp(spriteReturnPos, initialSpritePos, t / dropDuration);
                yield return null;
            }
            spriteRenderer.transform.localPosition = initialSpritePos;
        }

        if (objectAnim != null) objectAnim.PlayAnimation("Deactivate", true);
        if (soundContainer != null) soundContainer.PlaySound("Deactivate", 1);
        
        SetHitboxActive(false); 
        if (shadowObject != null) shadowObject.SetActive(false);

        yield return new WaitForSeconds(1f); 
        
        SetState(State.Asleep, null);
    }

    private void UpdateReturnSpriteFlip()
    {
        if (spriteRenderer != null)
        {
            Vector3 dirToOrigin = originPosition - transform.position;
            if (Mathf.Abs(dirToOrigin.x) > 0.1f)
            {
                spriteRenderer.flipX = dirToOrigin.x < 0;
            }
        }
    }
}
