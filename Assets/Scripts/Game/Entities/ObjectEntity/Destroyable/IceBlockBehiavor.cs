using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EntityEffects))]
[RequireComponent(typeof(SoundContainer))]
[RequireComponent(typeof(ObjectParticles))]
public class IceBlockBehiavor : MonoBehaviour
{
    [Header("Melt Settings")]
    public string smokeParticleName = "Smoke";
    public string meltingSoundName = "Melting";
    public float meltDuration = 1f;

    [Header("Spawn Settings")]
    public float growDuration = 0.3f;

    private EntityEffects entityEffects;
    private SoundContainer soundContainer;
    private ObjectParticles objectParticles;
    private SpriteRenderer spriteRenderer;
    private Transform shadowTransform;

    private void Awake()
    {
        entityEffects = GetComponent<EntityEffects>();
        soundContainer = GetComponent<SoundContainer>();
        objectParticles = GetComponent<ObjectParticles>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        Transform shadow = transform.Find("Shadow");
        if (shadow == null) shadow = transform.Find("Ombre");
        if (shadow == null) shadow = transform.Find("ombre");
        shadowTransform = shadow;
    }

    private void Start()
    {
        StartCoroutine(GrowRoutine());
        StartCoroutine(WatchFireState());
    }

    private IEnumerator GrowRoutine()
    {
        Vector3 spriteTargetScale = spriteRenderer != null ? spriteRenderer.transform.localScale : Vector3.one;
        Vector3 shadowTargetScale = shadowTransform != null ? shadowTransform.localScale : Vector3.one;

        if (spriteRenderer != null)
        {
            Vector3 scale = spriteTargetScale;
            scale.y = 0f;
            spriteRenderer.transform.localScale = scale;
        }

        if (shadowTransform != null)
        {
            Vector3 scale = shadowTargetScale;
            scale.y = 0f;
            shadowTransform.localScale = scale;
        }

        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;

            if (spriteRenderer != null)
            {
                Vector3 scale = spriteTargetScale;
                scale.y = Mathf.Lerp(0f, spriteTargetScale.y, t);
                spriteRenderer.transform.localScale = scale;
            }

            if (shadowTransform != null)
            {
                Vector3 scale = shadowTargetScale;
                scale.y = Mathf.Lerp(0f, shadowTargetScale.y, t);
                shadowTransform.localScale = scale;
            }

            yield return null;
        }

        if (spriteRenderer != null) spriteRenderer.transform.localScale = spriteTargetScale;
        if (shadowTransform != null) shadowTransform.localScale = shadowTargetScale;
    }

    private IEnumerator WatchFireState()
    {
        yield return new WaitUntil(() => entityEffects.isFire);

        // On coupe l'effet de feu classique (dégâts, animation, particules de flammes) immédiatement :
        // le bloc de glace ne doit jamais brûler normalement, seulement fondre.
        entityEffects.StopFire();

        yield return StartCoroutine(MeltRoutine());
    }

    private IEnumerator MeltRoutine()
    {
        soundContainer.PlaySound(meltingSoundName, 1);
        objectParticles.SpawnParticle(smokeParticleName, transform.position);

        Vector3 spriteStartScale = spriteRenderer != null ? spriteRenderer.transform.localScale : Vector3.one;
        Vector3 shadowStartScale = shadowTransform != null ? shadowTransform.localScale : Vector3.one;

        float elapsed = 0f;
        while (elapsed < meltDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / meltDuration;

            if (spriteRenderer != null)
            {
                Vector3 scale = spriteStartScale;
                scale.y = Mathf.Lerp(spriteStartScale.y, 0f, t);
                spriteRenderer.transform.localScale = scale;
            }

            if (shadowTransform != null)
            {
                Vector3 scale = shadowStartScale;
                scale.y = Mathf.Lerp(shadowStartScale.y, 0f, t);
                shadowTransform.localScale = scale;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
