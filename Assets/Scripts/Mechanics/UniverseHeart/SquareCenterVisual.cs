using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityLight))]
public class SquareCenterVisual : MonoBehaviour
{
    [Header("Light Color Settings")]
    public Color lightColor = new Color(0.6f, 0f, 0.8f);
    
    [Header("Intensity & Radius Settings")]
    public float lightIntensityMax = 3.0f;
    public float lightIntensityMin = 0.5f;
    public float lightRadiusMax = 5.0f;
    public float lightRadiusMin = 2.0f;
    
    [Header("Pulse Timing Settings")]
    [Tooltip("Durée pour atteindre l'intensité maximale")]
    public float transitionTimeUp = 1.0f;
    [Tooltip("Durée pour revenir à l'intensité minimale")]
    public float transitionTimeDown = 1.0f;
    [Tooltip("Délai d'attente entre chaque phase de battement (Optionnel)")]
    public float pulseDelay = 0.1f;

    private EntityLight entityLight;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        entityLight = GetComponent<EntityLight>();
    }

    private void Start()
    {
        if (entityLight != null)
        {
            // Appliquer la couleur au démarrage (après l'initialisation de EntityLight)
            entityLight.SetLightColor(lightColor);
            
            // Démarrer la boucle visuelle
            pulseCoroutine = StartCoroutine(PulseRoutine());
        }
    }

    private void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
    }

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            // Phase d'expansion (grossissement)
            if (entityLight != null)
            {
                entityLight.TransitionLightIntensity(lightIntensityMax, lightRadiusMax, transitionTimeUp);
            }
            yield return new WaitForSeconds(transitionTimeUp);
            
            if (pulseDelay > 0)
                yield return new WaitForSeconds(pulseDelay);

            // Phase de contraction (réduction)
            if (entityLight != null)
            {
                entityLight.TransitionLightIntensity(lightIntensityMin, lightRadiusMin, transitionTimeDown);
            }
            yield return new WaitForSeconds(transitionTimeDown);

            if (pulseDelay > 0)
                yield return new WaitForSeconds(pulseDelay);
        }
    }
}
