using System.Collections;
using UnityEngine;

// Larme de givre : attaque de Mylia (voir Documentation/Conception_Boss_Mylia.md). Apparait, grossit,
// puis poursuit lentement le joueur pendant sa duree de vie. Se detruit au contact du joueur, expire
// avec un fondu si personne ne la touche, ou fond litteralement si le joueur l'enflamme (Lanterne).
[RequireComponent(typeof(SoundContainer))]
[RequireComponent(typeof(EntityEffects))]
public class IceTearBehiavor : MonoBehaviour
{
    public string spawnSoundName = "Spawn";

    [Header("Duree de vie")]
    public float lifetime = 30f;
    public float fadeOutDuration = 0.5f;

    [Header("Apparition")]
    public Vector2 spawnScale = new Vector2(0.1f, 0.1f);
    public Vector2 targetScale = new Vector2(0.5f, 0.5f);
    public float spawnGrowDuration = 2f;

    [Header("Fonte (Lanterne)")]
    public float meltShrinkDuration = 0.5f;

    [Header("Errance au spawn")]
    public float wanderDuration = 1.5f;
    public float wanderSpeed = 1f;
    public float wanderJitter = 3f;
    public float minDispatchRadius = 2f;
    public float maxDispatchRadius = 6f;

    // Mylia : sert de centre pour le rayon de dispatch (2 a 6 unites) au spawn.
    public Transform owner;

    SpriteRenderer spriteRenderer;
    SoundContainer soundContainer;
    EntityEffects entityEffects;
    Transform playerTransform;

    float speed;
    int force;

    bool hasDealtDamage = false;
    bool isWandering = true;
    Coroutine lifetimeCoroutine;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        soundContainer = GetComponent<SoundContainer>();
        entityEffects = GetComponent<EntityEffects>();

        // La larme peut toujours etre enflammee par la Lanterne, quelle que soit la config du prefab.
        entityEffects.canBeFire = true;

        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        soundContainer.PlaySound(spawnSoundName, 1);

        StartCoroutine(GrowRoutine());
        StartCoroutine(WanderRoutine());
        lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        StartCoroutine(WatchFireState());
    }

    public void Init(float speed, int force)
    {
        this.speed = speed;
        this.force = force;
    }

    void Update()
    {
        // Pendant l'errance initiale, WanderRoutine pilote seule la position : pas de poursuite.
        if (isWandering) return;

        if (playerTransform == null && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        if (playerTransform == null) return;

        // Poursuite continue : la direction est recalculee chaque frame vers la position actuelle du joueur.
        Vector2 direction = playerTransform.position - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
        }
    }

    // Petit effet "larme qui divague" au moment du spawn : part du transform d'origine (l'oeil de
    // Mylia) et derive de maniere erratique vers un point tire entre minDispatchRadius et
    // maxDispatchRadius de Mylia, avant de ceder la main a la poursuite du joueur.
    IEnumerator WanderRoutine()
    {
        Vector3 origin = owner != null ? owner.position : transform.position;
        float dispatchDistance = Random.Range(minDispatchRadius, maxDispatchRadius);
        Vector3 dispatchTarget = origin + (Vector3)(Random.insideUnitCircle.normalized * dispatchDistance);

        Vector2 wanderDirection = Random.insideUnitCircle.normalized;
        float elapsed = 0f;

        while (elapsed < wanderDuration)
        {
            elapsed += Time.deltaTime;

            Vector2 toTarget = dispatchTarget - transform.position;
            Vector2 desiredDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : wanderDirection;

            // Melange la direction vers le point de dispatch avec du bruit aleatoire, pour garder
            // l'aspect erratique ("divague") tout en tendant vers la zone visee.
            wanderDirection = (desiredDirection + Random.insideUnitCircle * wanderJitter * Time.deltaTime).normalized;
            transform.position += (Vector3)(wanderDirection * wanderSpeed * Time.deltaTime);

            yield return null;
        }

        isWandering = false;
    }

    IEnumerator GrowRoutine()
    {
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(spawnScale.x, spawnScale.y, currentScale.z);

        float elapsed = 0f;
        while (elapsed < spawnGrowDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnGrowDuration;
            transform.localScale = new Vector3(
                Mathf.Lerp(spawnScale.x, targetScale.x, t),
                Mathf.Lerp(spawnScale.y, targetScale.y, t),
                currentScale.z);
            yield return null;
        }

        transform.localScale = new Vector3(targetScale.x, targetScale.y, currentScale.z);
    }

    IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);

        yield return StartCoroutine(FadeOutRoutine());

        Destroy(gameObject);
    }

    IEnumerator FadeOutRoutine()
    {
        if (spriteRenderer == null) yield break;

        Color startColor = spriteRenderer.color;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, elapsed / fadeOutDuration);
            spriteRenderer.color = color;
            yield return null;
        }
    }

    // Suit le meme principe que IceBlockBehiavor.WatchFireState : la larme ne doit jamais "bruler"
    // normalement (degats/anim/particules de feu), seulement fondre.
    IEnumerator WatchFireState()
    {
        yield return new WaitUntil(() => entityEffects.isFire);

        entityEffects.StopFire();

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < meltShrinkDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / meltShrinkDuration);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasDealtDamage) return;

        Stats targetStats = collision.GetComponent<Stats>();
        if (targetStats == null || targetStats.entityType != EntityType.Player) return;

        LifeManager lifeManager = collision.GetComponent<LifeManager>();
        if (lifeManager == null) return;

        lifeManager.TakeDamage(force, gameObject, false);

        EntityEffects targetEffects = collision.GetComponent<EntityEffects>();
        if (targetEffects != null && targetEffects.canBeFreeze)
        {
            targetEffects.SetState(isFreeze: true);
        }

        hasDealtDamage = true;

        Destroy(gameObject);
    }
}
