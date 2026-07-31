using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityLight))]
[RequireComponent(typeof(CircleCollider2D))]
public class HealCircleBehiavor : MonoBehaviour
{
    [Header("Paramètres de Soin")]
    public bool isAlly = true;
    public bool percentageHeal = false;
    public float valueHeal = 5f;
    public float duration = 5f;
    public float frequency = 1f;

    [Header("Paramètres Visuels & Rotation")]
    public float rotationSpeed = 180f; // Vitesse de rotation de base (degrés/seconde)
    public Transform spriteTransform; // Optionnel : Transform du sprite enfant si séparé du parent

    [Header("Couleurs de Lumière")]
    public Color allyColor = new Color(0.2f, 1f, 0.4f, 1f); // Vert émeraude
    public Color monsterColor = new Color(0.8f, 0.2f, 0.9f, 1f); // Violet / Magenta

    [Header("Sons")]
    public string soundName = "Pulse";

    private EntityLight entityLight;
    private SoundContainer soundContainer;
    private CircleCollider2D circleCollider;
    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

    // Liste des entités présentes dans le trigger du cercle
    private HashSet<Collider2D> entitiesInZone = new HashSet<Collider2D>();

    private Vector3 originalScale;
    private float effectRadius = 3f;
    private bool isInitialized = false;
    private bool isFadingOut = false;

    private Coroutine healRoutine;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        entityLight = GetComponent<EntityLight>();
        soundContainer = GetComponent<SoundContainer>();
        circleCollider = GetComponent<CircleCollider2D>();

        // Récupération de tous les SpriteRenderers sur l'objet et ses enfants
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        spriteRenderers.AddRange(renderers);

        if (spriteTransform != null)
            originalScale = spriteTransform.localScale;
        else
            originalScale = transform.localScale;

        if (circleCollider != null)
        {
            effectRadius = circleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        }
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Init(isAlly, percentageHeal, valueHeal, duration, frequency);
        }
    }

    /// <summary>
    /// Initialise les variables du cercle de soin et lance les coroutines.
    /// </summary>
    public void Init(bool isAlly, bool percentageHeal, float valueHeal, float duration, float frequency)
    {
        this.isAlly = isAlly;
        this.percentageHeal = percentageHeal;
        this.valueHeal = valueHeal;
        this.duration = Mathf.Max(0.5f, duration);
        this.frequency = Mathf.Max(0.1f, frequency);
        this.isInitialized = true;

        if (circleCollider != null)
        {
            effectRadius = circleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        }

        // Configuration initiale de la lumière (vert émeraude pour allié, violet/magenta pour monstre)
        if (entityLight != null)
        {
            Color lightColor = isAlly ? allyColor : monsterColor;
            entityLight.SetLightColor(lightColor);
            entityLight.SetLightIntensity(0.5f, effectRadius * 0.8f);
        }

        // Réinitialiser les coroutines si l'objet est réutilisé ou réinitialisé
        if (healRoutine != null) StopCoroutine(healRoutine);
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        healRoutine = StartCoroutine(HealRoutine());
        fadeRoutine = StartCoroutine(FadeAndDestroyRoutine());
    }

    private void Update()
    {
        // Rotation constante en rapport avec la fréquence
        float currentRotationSpeed = rotationSpeed / (frequency > 0 ? frequency : 1f);

        Transform targetTransform = spriteTransform != null ? spriteTransform : transform;
        targetTransform.Rotate(0, 0, currentRotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && !entitiesInZone.Contains(other))
        {
            entitiesInZone.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other != null && entitiesInZone.Contains(other))
        {
            entitiesInZone.Remove(other);
        }
    }

    /// <summary>
    /// Coroutine gérant les vagues de soins périodiques et la pulsation.
    /// </summary>
    private IEnumerator HealRoutine()
    {
        while (!isFadingOut)
        {
            // Effectuer le soin sur les entités présentes dans le trigger
            PerformHeal();

            // Lancer la pulsation visuelle (Lumière + Échelle)
            StartCoroutine(PulseVisualsRoutine());

            // Jouer le son "Pulse"
            if (soundContainer != null)
            {
                soundContainer.PlaySound(soundName, 1);
            }

            yield return new WaitForSeconds(frequency);
        }
    }

    /// <summary>
    /// Applique le soin aux entités valides présentes dans la zone.
    /// </summary>
    private void PerformHeal()
    {
        List<Collider2D> toRemove = new List<Collider2D>();

        foreach (Collider2D colliderHit in entitiesInZone)
        {
            if (colliderHit == null)
            {
                toRemove.Add(colliderHit);
                continue;
            }

            Stats targetStats = colliderHit.GetComponent<Stats>();
            LifeManager targetLife = colliderHit.GetComponent<LifeManager>();

            if (targetStats != null && targetLife != null && !targetStats.isDying)
            {
                bool isTargetAlly = (targetStats.entityType == EntityType.Player);
                bool isTargetMonster = (targetStats.entityType == EntityType.Monster || targetStats.entityType == EntityType.Boss);

                // Si isAlly est à false -> soin des monstres. Sinon -> soin du joueur.
                if ((isAlly && isTargetAlly) || (!isAlly && isTargetMonster))
                {
                    int healAmount = 0;

                    if (percentageHeal)
                    {
                        // Soin par pourcentage de la vie maximale
                        float pct = valueHeal > 1f ? valueHeal / 100f : valueHeal;
                        healAmount = Mathf.Max(1, Mathf.RoundToInt(targetStats.health * pct));
                    }
                    else
                    {
                        // Soin avec une valeur fixe
                        healAmount = Mathf.Max(1, Mathf.RoundToInt(valueHeal));
                    }

                    targetLife.Heal(healAmount);
                }
            }
        }

        // Nettoyage des colliders détruits
        foreach (Collider2D deadCollider in toRemove)
        {
            entitiesInZone.Remove(deadCollider);
        }
    }

    /// <summary>
    /// Coroutine gérant la pulsation d'intensité de la lumière et la légère dilatation du sprite.
    /// </summary>
    private IEnumerator PulseVisualsRoutine()
    {
        float pulseDuration = Mathf.Min(0.3f, frequency * 0.4f);

        // 1. Amplification de la lumière
        if (entityLight != null && !isFadingOut)
        {
            entityLight.TransitionLightIntensity(1.5f, effectRadius * 1.3f, pulseDuration * 0.5f);
        }

        // 2. Échelle pulsée
        Transform targetTransform = spriteTransform != null ? spriteTransform : transform;
        Vector3 pulsedScale = originalScale * 1.12f;
        float elapsed = 0f;

        while (elapsed < pulseDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(originalScale, pulsedScale, elapsed / (pulseDuration * 0.5f));
            yield return null;
        }

        // 3. Retour à la normale
        if (entityLight != null && !isFadingOut)
        {
            entityLight.TransitionLightIntensity(0.5f, effectRadius * 0.8f, pulseDuration * 0.5f);
        }

        elapsed = 0f;
        while (elapsed < pulseDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(pulsedScale, originalScale, elapsed / (pulseDuration * 0.5f));
            yield return null;
        }

        targetTransform.localScale = originalScale;
    }

    /// <summary>
    /// Coroutine gérant le cycle de vie et le fondu de 1 seconde avant destruction.
    /// </summary>
    private IEnumerator FadeAndDestroyRoutine()
    {
        float fadeDuration = 1f;

        if (duration <= fadeDuration)
        {
            fadeDuration = duration * 0.8f;
        }

        float timeBeforeFade = duration - fadeDuration;
        if (timeBeforeFade > 0f)
        {
            yield return new WaitForSeconds(timeBeforeFade);
        }

        isFadingOut = true;

        // Disparition progressive de la lumière
        if (entityLight != null)
        {
            entityLight.TransitionLightIntensity(0f, 0f, fadeDuration);
        }

        // Disparition progressive des Sprites
        float elapsed = 0f;

        List<Color> initialColors = new List<Color>();
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
                initialColors.Add(sr.color);
            else
                initialColors.Add(Color.white);
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alphaProgress = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color c = initialColors[i];
                    c.a = initialColors[i].a * alphaProgress;
                    spriteRenderers[i].color = c;
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
