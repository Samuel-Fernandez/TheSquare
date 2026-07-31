using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareEntranceBehiavor : MonoBehaviour
{
    [Header("Light Settings")]
    [Tooltip("Couleur de la lumière (violette par défaut)")]
    [SerializeField] private Color lightColor = new Color(0.6f, 0f, 1f, 1f); 
    
    [Tooltip("Intensité maximale de la lumière (doit être intense)")]
    [SerializeField] private float lightIntensity = 5f; 
    
    [Tooltip("Rayon maximal de la lumière")]
    [SerializeField] private float lightRadius = 8f; 

    [Header("Beat Settings")]
    [Tooltip("Durée d'un battement en secondes")]
    [SerializeField] private float beatDuration = 1f;
    [Tooltip("Intensité minimale de la lumière")]
    [SerializeField] private float minIntensity = 2f;
    [Tooltip("Multiplicateur du rayon au minimum du battement")]
    [SerializeField] private float minRadiusMultiplier = 0.8f;
    [Tooltip("Scale maximal lors du battement")]
    [SerializeField] private float maxScale = 1.2f;

    private EntityLight entityLight;
    private SoundContainer soundContainer;

    private void Start()
    {
        // 1. Instancier la particule "SquareParticle"
        ObjectParticles objectParticles = GetComponent<ObjectParticles>();
        if (objectParticles != null)
        {
            objectParticles.SpawnParticle("SquareParticle", transform.position);
        }
        else
        {
            Debug.LogWarning("ObjectParticles est introuvable sur l'objet " + gameObject.name);
        }

        soundContainer = GetComponent<SoundContainer>();
        if (soundContainer == null)
        {
            Debug.LogWarning("SoundContainer est introuvable sur l'objet " + gameObject.name);
        }

        // 3. Générer une lumière intense et violette qui bat
        entityLight = GetComponent<EntityLight>();
        if (entityLight != null)
        {
            entityLight.SetLightColor(lightColor);
            StartCoroutine(LightBeatRoutine());
        }
        else
        {
            Debug.LogWarning("EntityLight est introuvable sur l'objet " + gameObject.name);
        }
    }

    private IEnumerator LightBeatRoutine()
    {
        Vector3 baseScale = transform.localScale;
        float timer = 0f;
        
        // Joue le premier son au tout début
        if (soundContainer != null) soundContainer.PlaySound("Beat", 0);

        while (true)
        {
            timer += Time.deltaTime;
            
            // Si on dépasse la durée, on reboucle et on rejoue le son
            if (timer >= beatDuration)
            {
                timer -= beatDuration;
                if (soundContainer != null) soundContainer.PlaySound("Beat", 0);
            }

            // t de 0 à 1 sur la durée du battement
            float t = timer / beatDuration;

            // Onde en forme de cloche (0 -> 1 -> 0) sur la durée (sin(pi * t))
            float pulse = Mathf.Sin(t * Mathf.PI);
            
            // Scale
            transform.localScale = baseScale * Mathf.Lerp(1f, maxScale, pulse);

            // Lumière
            float currentIntensity = Mathf.Lerp(minIntensity, lightIntensity, pulse);
            float currentRadius = Mathf.Lerp(lightRadius * minRadiusMultiplier, lightRadius, pulse);
            
            entityLight.SetLightIntensity(currentIntensity, currentRadius);
            
            yield return null;
        }
    }
}
