using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EntityLight : MonoBehaviour
{
    public GameObject entityLight;
    // Pour objet temporaires, comme particules, etc...
    public bool tempLight;
    public float durationLight;
    private Light2D light2D;

    void Awake()
    {
        // Récupérer le composant Light2D au démarrage
        if (entityLight != null)
        {
            light2D = entityLight.GetComponent<Light2D>();
        }
        if (tempLight)
        {
            TransitionLightIntensity(0, 0, durationLight);
        }
    }

    // Méthode pour régler l'intensité de la lumière
    public void SetLightIntensity(float intensity = .25f, float radius = 1.5f)
    {
        if (light2D != null)
        {
            light2D.intensity = intensity;
            light2D.pointLightOuterRadius = radius;
        }
        else
        {
            Debug.LogWarning("Light2D component not found on the provided GameObject.");
        }
    }

    // Méthode pour régler la couleur de la lumière
    public void SetLightColor(Color color)
    {
        if (light2D != null)
        {
            light2D.color = color;
        }
        else
        {
            Debug.LogWarning("Light2D component not found on the provided GameObject.");
        }
    }

    private Coroutine transitionCoroutine; // Stocker la référence à la coroutine en cours
    private Coroutine colorTransitionCoroutine; // Stocker la référence à la coroutine de couleur
    private Coroutine fadeOutCoroutine; // Coroutine spécifique pour le fade out final

    // Méthode pour faire une transition en douceur de l'intensité et du rayon de la lumière
    public void TransitionLightIntensity(float targetIntensity, float targetRadius, float duration)
    {
        if (light2D == null) return;

        // Si une transition est déjà en cours, arrêter la précédente
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        // Démarrer une nouvelle transition
        transitionCoroutine = StartCoroutine(TransitionIntensityAndRadiusCoroutine(targetIntensity, targetRadius, duration));
    }

    // Coroutine pour la transition de l'intensité et du rayon
    private IEnumerator TransitionIntensityAndRadiusCoroutine(float targetIntensity, float targetRadius, float duration)
    {
        float startIntensity = light2D.intensity;
        float startRadius = light2D.pointLightOuterRadius;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            light2D.intensity = Mathf.Lerp(startIntensity, targetIntensity, time / duration);
            light2D.pointLightOuterRadius = Mathf.Lerp(startRadius, targetRadius, time / duration);
            yield return null;
        }

        // Assurer que les valeurs finales sont exactes
        light2D.intensity = targetIntensity;
        light2D.pointLightOuterRadius = targetRadius;

        // Réinitialiser la référence de la coroutine
        transitionCoroutine = null;
    }

    // Méthode pour faire une transition en douceur de la couleur de la lumière
    public void TransitionLightColor(Color targetColor, float duration)
    {
        if (light2D != null)
        {
            // Si une transition de couleur est déjà en cours, arrêter la précédente
            if (colorTransitionCoroutine != null)
            {
                StopCoroutine(colorTransitionCoroutine);
            }

            colorTransitionCoroutine = StartCoroutine(TransitionColorCoroutine(targetColor, duration));
        }
    }

    // Coroutine pour la transition de la couleur
    private IEnumerator TransitionColorCoroutine(Color targetColor, float duration)
    {
        Color startColor = light2D.color;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            light2D.color = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
        }

        light2D.color = targetColor; // Assurer que la couleur est exactement égale à la cible à la fin

        // Réinitialiser la référence de la coroutine
        colorTransitionCoroutine = null;
    }

    // NOUVELLE MÉTHODE : Arrêter seulement les transitions de blink (pas les fade out)
    public void StopBlinkTransitions()
    {
        // Arrêter seulement les transitions d'intensité et de couleur en cours
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        if (colorTransitionCoroutine != null)
        {
            StopCoroutine(colorTransitionCoroutine);
            colorTransitionCoroutine = null;
        }
    }
}