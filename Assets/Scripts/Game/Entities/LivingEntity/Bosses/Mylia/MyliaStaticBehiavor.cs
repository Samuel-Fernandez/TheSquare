using System.Collections;
using UnityEngine;

public class MyliaStaticBehiavor : MonoBehaviour
{
    [Header("Teleportation")]
    public string teleportSoundName = "Teleportation";
    public float teleportShakeDuration = 0.15f;
    public float teleportShakeMagnitude = 0.1f;
    public Color teleportFlashColor = new Color(0.2f, 0.6f, 1f, 1f);
    public float teleportFlashFadeInDuration = 0.1f;
    public float teleportFlashFadeOutDuration = 0.1f;

    SoundContainer soundContainer;
    SpriteRenderer spriteRenderer;
    SpriteRenderer shadowSpriteRenderer;

    Material originalSpriteMaterial;
    Material teleportFlashMaterial;
    float originalSpriteAlpha;
    float originalShadowAlpha;

    private void Start()
    {
        soundContainer = GetComponent<SoundContainer>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        shadowSpriteRenderer = GetComponent<ObjectPerspective>()?.shadowSpriteRenderer;

        originalSpriteMaterial = spriteRenderer.sharedMaterial;
        teleportFlashMaterial = new Material(Shader.Find("Custom/SpriteFlash"));
        teleportFlashMaterial.SetColor("_FlashColor", teleportFlashColor);
        originalSpriteAlpha = spriteRenderer.color.a;
        originalShadowAlpha = shadowSpriteRenderer != null ? shadowSpriteRenderer.color.a : 0f;
    }

    // Appelee depuis un evenement : teleporte l'objet vers (x, y), ajoute a sa position actuelle.
    public void Teleport(float x, float y)
    {
        StartCoroutine(TeleportRoutine(new Vector3(x, y, 0f)));
    }

    IEnumerator TeleportRoutine(Vector3 relativeOffset)
    {
        soundContainer.PlaySound(teleportSoundName, 1);

        // Fondu vers un bleu plein avant meme le tremblement, qui precede le saut.
        spriteRenderer.material = teleportFlashMaterial;
        yield return StartCoroutine(TeleportFlashRoutine(0f, 1f, teleportFlashFadeInDuration));

        yield return StartCoroutine(SpriteShakeRoutine(teleportShakeDuration, teleportShakeMagnitude));

        transform.position += relativeOffset;

        yield return StartCoroutine(TeleportFlashRoutine(1f, 0f, teleportFlashFadeOutDuration));
        spriteRenderer.material = originalSpriteMaterial;
    }

    // from/to portent sur _FlashAmount (0 = normal, 1 = bleu plein) ; le sprite et son ombre
    // disparaissent en meme temps par l'alpha, en opposition directe avec ce fondu de couleur
    // (invisibles au moment ou la couleur est pleinement bleue, visibles quand elle est normale).
    IEnumerator TeleportFlashRoutine(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ApplyTeleportFlash(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        ApplyTeleportFlash(to);
    }

    void ApplyTeleportFlash(float flashAmount)
    {
        teleportFlashMaterial.SetFloat("_FlashAmount", flashAmount);

        float visibility = 1f - flashAmount;

        Color spriteColor = spriteRenderer.color;
        spriteColor.a = originalSpriteAlpha * visibility;
        spriteRenderer.color = spriteColor;

        if (shadowSpriteRenderer != null)
        {
            Color shadowColor = shadowSpriteRenderer.color;
            shadowColor.a = originalShadowAlpha * visibility;
            shadowSpriteRenderer.color = shadowColor;
        }
    }

    IEnumerator SpriteShakeRoutine(float duration, float magnitude)
    {
        if (spriteRenderer == null)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        Vector3 originalLocalPosition = spriteRenderer.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.transform.localPosition = originalLocalPosition + (Vector3)(Random.insideUnitCircle * magnitude);
            yield return null;
        }

        spriteRenderer.transform.localPosition = originalLocalPosition;
    }
}
