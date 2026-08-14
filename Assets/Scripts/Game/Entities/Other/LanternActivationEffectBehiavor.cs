using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EntityLight))]
public class LanternActivationEffectBehiavor : MonoBehaviour
{
    [Header("Light")]
    public Color lightColor = Color.white;
    public float radiusMin = 0.5f;
    public float radiusMax = 2f;
    public float intensityMin = 0.5f;
    public float intensityMax = 1.5f;

    [Header("Taille / Fade")]
    public float startSize = 0f;
    public float endSize = 1f;
    public float transitionDuration = 0.5f;

    [Header("Temps de vie")]
    public float lifeDuration = 1f;

    private EntityLight entityLight;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        entityLight = GetComponent<EntityLight>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        entityLight.SetLightColor(lightColor);
        entityLight.SetLightIntensity(intensityMin, radiusMin);

        transform.localScale = Vector3.one * startSize;
        SetAlpha(0f);

        StartCoroutine(RoutinePlayEffect());
    }

    private IEnumerator RoutinePlayEffect()
    {
        entityLight.TransitionLightIntensity(intensityMax, radiusMax, transitionDuration);
        yield return StartCoroutine(RoutineFadeAndScale(0f, 1f, startSize, endSize, transitionDuration));

        yield return new WaitForSeconds(lifeDuration);

        entityLight.TransitionLightIntensity(intensityMin, radiusMin, transitionDuration);
        yield return StartCoroutine(RoutineFadeAndScale(1f, 0f, endSize, startSize, transitionDuration));

        Destroy(gameObject);
    }

    private IEnumerator RoutineFadeAndScale(float startAlpha, float targetAlpha, float startScale, float targetScale, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, t);

            yield return null;
        }

        SetAlpha(targetAlpha);
        transform.localScale = Vector3.one * targetScale;
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null) return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
