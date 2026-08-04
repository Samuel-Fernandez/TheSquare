using System.Collections;
using UnityEngine;

public class FireStickBehiavor : MonoBehaviour
{
    [Header("Carry Settings")]
    public float speedDivider = 1.5f;
    public Vector3 carriedOffset = new Vector3(0.2f, 0.3f, 0f);

    [Header("Throw Settings")]
    public float throwDistance = 5f;
    public float throwSpeed = 8f;
    public float rotationsPerSecond = 4f;

    [Header("Light Flicker Settings")]
    public Color lightColor = new Color(1f, 0.55f, 0.1f);
    public float lightIntensityMin = 0.5f;
    public float lightIntensityMax = 1.5f;
    public float lightRadiusMin = 1f;
    public float lightRadiusMax = 2f;
    public float flickerIntervalMin = 0.05f;
    public float flickerIntervalMax = 0.25f;
    public float flickerTransitionTime = 0.1f;

    public bool isCarried = false;
    private bool isFlying = false;
    private int pendingFireForce = 1;
    private GameObject thrower;

    private EntityLight entityLight;
    private SoundContainer soundContainer;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int baseSortingOrder = 0;

    private void Awake()
    {
        entityLight = GetComponent<EntityLight>();
        soundContainer = GetComponent<SoundContainer>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (entityLight != null)
        {
            entityLight.SetLightColor(lightColor);
        }

        StartCoroutine(FlickerRoutine());

        if (spriteRenderer != null) baseSortingOrder = spriteRenderer.sortingOrder;
    }

    private void Update()
    {
        if (isCarried && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            transform.position = PlayerManager.instance.player.transform.position + carriedOffset;

            SpriteRenderer playerSr = PlayerManager.instance.player.GetComponentInChildren<SpriteRenderer>();
            if (playerSr != null && spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = playerSr.sortingOrder + 10;
                spriteRenderer.sortingLayerID = playerSr.sortingLayerID;
            }
        }
    }

    public void Interaction()
    {
        if (isFlying) return;

        if (!isCarried)
        {
            PickUp();
        }
        else
        {
            Throw();
        }
    }

    private void PickUp()
    {
        isCarried = true;
        Transform playerTransform = PlayerManager.instance.player.transform;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        ObjectPerspective op = GetComponent<ObjectPerspective>();
        if (op != null) op.enabled = false;

        InteractableBehiavor interactable = GetComponent<InteractableBehiavor>();
        if (interactable != null) interactable.forceHideUI = true;

        transform.SetParent(playerTransform);
        transform.localPosition = carriedOffset;
        transform.position = playerTransform.position + carriedOffset;

        if (soundContainer != null) soundContainer.PlaySound("Hold", 1);

        Stats playerStats = PlayerManager.instance.player.GetComponent<Stats>();
        if (playerStats != null)
        {
            playerStats.speed /= speedDivider;
            PlayerController pc = PlayerManager.instance.player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.isHoldingObject = true;
                pc.UpdateSpeed(playerStats.speed);
            }
        }
    }

    private void ForceDrop()
    {
        isCarried = false;
        transform.SetParent(null);

        if (rb != null) rb.isKinematic = false;

        ObjectPerspective op = GetComponent<ObjectPerspective>();
        if (op != null) op.enabled = true;

        InteractableBehiavor interactable = GetComponent<InteractableBehiavor>();
        if (interactable != null)
        {
            interactable.forceHideUI = false;
            interactable.canInteract = true;
        }

        if (spriteRenderer != null) spriteRenderer.sortingOrder = baseSortingOrder;

        Stats playerStats = PlayerManager.instance.player.GetComponent<Stats>();
        if (playerStats != null)
        {
            playerStats.UpdateStats();
        }

        PlayerController pc = PlayerManager.instance.player.GetComponent<PlayerController>();
        if (pc != null) pc.isHoldingObject = false;
    }

    private void Throw()
    {
        thrower = PlayerManager.instance.player;

        Stats throwerStats = thrower.GetComponent<Stats>();
        pendingFireForce = throwerStats != null ? Mathf.Max(throwerStats.strength / 2, 1) : 1;

        ForceDrop();

        if (soundContainer != null) soundContainer.PlaySound("Threw", 1);

        PlayerController pc = thrower.GetComponent<PlayerController>();
        Vector2 throwDirection = pc != null ? pc.GetAttackDirection() : Vector2.right;

        StartCoroutine(FlightRoutine(throwDirection.normalized));
    }

    private IEnumerator FlightRoutine(Vector2 direction)
    {
        isFlying = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + (Vector3)direction * throwDistance;
        float duration = throwDistance / Mathf.Max(throwSpeed, 0.01f);
        float rotationPerSecond = rotationsPerSecond * 360f;

        Transform spinTransform = spriteRenderer != null ? spriteRenderer.transform : transform;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            spinTransform.Rotate(0, 0, rotationPerSecond * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        // Vol termin� sans avoir rien touch� : atterrissage, arr�t simple de la rotation.
        isFlying = false;
    }

    public void PlaySpawnAnimation(Vector3 targetPosition, float jumpHeight, float jumpDuration)
    {
        StartCoroutine(SpawnAppearRoutine(targetPosition, jumpHeight, jumpDuration));
    }

    private IEnumerator SpawnAppearRoutine(Vector3 targetPosition, float jumpHeight, float jumpDuration)
    {
        Vector3 startPos = transform.position;
        Transform spinTransform = spriteRenderer != null ? spriteRenderer.transform : transform;
        Vector3 baseLocalPosition = spinTransform.localPosition;
        float rotationPerSecond = rotationsPerSecond * 360f;

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            spinTransform.localPosition = baseLocalPosition + Vector3.up * height;
            spinTransform.Rotate(0, 0, rotationPerSecond * Time.deltaTime);

            yield return null;
        }

        transform.position = targetPosition;
        spinTransform.localPosition = baseLocalPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFlying) return;
        if (other.isTrigger) return;
        if (other.gameObject == thrower) return;

        EntityEffects targetEffects = other.GetComponent<EntityEffects>();
        if (targetEffects != null && targetEffects.canBeFire)
        {
            targetEffects.SetState(pendingFireForce, true);
        }

        isFlying = false;
        Destroy(gameObject);
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            float intensity = Random.Range(lightIntensityMin, lightIntensityMax);
            float radius = Random.Range(lightRadiusMin, lightRadiusMax);
            float interval = Random.Range(flickerIntervalMin, flickerIntervalMax);

            if (entityLight != null)
            {
                entityLight.TransitionLightIntensity(intensity, radius, flickerTransitionTime);
            }

            yield return new WaitForSeconds(interval);
        }
    }
}
