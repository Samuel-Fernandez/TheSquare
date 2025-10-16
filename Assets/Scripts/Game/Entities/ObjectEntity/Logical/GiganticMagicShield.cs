using System.Collections;
using UnityEngine;

public class GiganticMagicShield : MonoBehaviour
{
    public string id;

    [Tooltip("Tours par seconde")]
    public float rotationsPerSecond = 1f;

    [Tooltip("Référence vers le script EntityLight (sera cherché dans les enfants si vide)")]
    public EntityLight lightEntity;

    // Blink settings (modifiables dans l'inspector)
    public float highIntensity = 30f;
    public float highRadius = 10f;
    public float lowIntensity = 15f;
    public float lowRadius = 15f;
    public float fadeDuration = 3; // durée de la transition (s)
    public float holdHigh = 3;     // maintien après montée
    public float holdLow = 3;      // maintien après descente

    private float rotationSpeed; // degrés par seconde

    bool isDisappearing = false;

    void Awake()
    {
        // si rien d'assigné, chercher dans les enfants
        if (lightEntity == null)
        {
            lightEntity = GetComponentInChildren<EntityLight>();
        }
    }

    void Start()
    {
        bool state;

        SaveManager.instance.twoStateContainer.TryGetState(id, out state);

        if (state)
        {
            Destroy(gameObject);
        }
        // initialiser la vitesse de rotation
        rotationSpeed = rotationsPerSecond * 360f;

        if (lightEntity != null)
        {
            // s'assurer que la couleur est violette au départ (transition instantanée si duration = 0)
            lightEntity.TransitionLightColor(Color.magenta, 0f);

            // démarrer le clignotement qui utilise les méthodes Transition*
            StartCoroutine(BlinkLightCoroutine());

            StartCoroutine(SoundRoutine());
        }
        else
        {
            Debug.LogWarning("EntityLight not found in children of GiganticMagicShield.");
        }
    }

    void Update()
    {
        // rotation autour de l'axe Z
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    IEnumerator SoundRoutine()
    {
        while (!isDisappearing)
        {
            yield return new WaitForSeconds(2f);
            GetComponent<SoundContainer>().PlaySound("Ambiant", 2);
        }
    }

    private IEnumerator BlinkLightCoroutine()
    {
        while (!isDisappearing)
        {
            // montée (transition douce vers forte intensite / rayon)
            if (!isDisappearing) // Vérifier avant chaque action
            {
                lightEntity.TransitionLightIntensity(highIntensity, highRadius, fadeDuration);
                lightEntity.TransitionLightColor(Color.magenta, fadeDuration);
            }

            yield return new WaitForSeconds(fadeDuration + holdHigh);

            if (!isDisappearing) // Vérifier avant chaque action
            {
                // descente (transition douce vers faible intensite / rayon)
                lightEntity.TransitionLightIntensity(lowIntensity, lowRadius, fadeDuration);
            }

            yield return new WaitForSeconds(fadeDuration + holdLow);
        }
    }

    public void FadeAndDestroy()
    {
        isDisappearing = true; // Mettre ça EN PREMIER pour arrêter les blinks

        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, true);

        // Arrêter explicitement les transitions de blink dans EntityLight
        if (lightEntity != null)
        {
            lightEntity.StopBlinkTransitions();
        }

        StopAllCoroutines(); // Arrêter toutes les coroutines après avoir stoppé les transitions
        StartCoroutine(FadeOutCoroutine(5f));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        // Commencer la transition de fade out pour la lumière
        if (lightEntity != null)
        {
            lightEntity.TransitionLightIntensity(0, 0, duration);
        }

        // Récupérer tous les SpriteRenderer enfants
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        Color[] initialColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            initialColors[i] = sprites[i].color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Fade SpriteRenderer
            for (int i = 0; i < sprites.Length; i++)
            {
                Color c = initialColors[i];
                sprites[i].color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 0f, t));
            }

            yield return null;
        }

        // S'assurer que les sprites sont bien à 0
        foreach (var sr in sprites)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        Destroy(gameObject);
    }
}