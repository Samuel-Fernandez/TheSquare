using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EntityLight))]
public class MyliaSealEffectBehiavor : MonoBehaviour
{
    private EntityLight entityLight;
    private SoundContainer soundContainer;

    [Header("Sprite")]
    [Tooltip("Laissez vide pour faire tourner l'objet lui-même")]
    public Transform spriteTransform;
    public float rotationSpeed = 360f; // 360 degrés par seconde

    [Header("Lumière")]
    public Color lightColor = Color.yellow;
    public float lightTransitionDuration = 3f; // Transition de 3 secondes
    
    [Header("Son")]
    public float timeBetweenSounds = 2f; // Un son toutes les 2 secondes
    public string soundName = "SealSound";

    private bool lightToggle = false;

    private void Start()
    {
        entityLight = GetComponent<EntityLight>();
        soundContainer = GetComponent<SoundContainer>();

        // Configuration initiale de la lumière
        if (entityLight != null)
        {
            entityLight.SetLightColor(lightColor);
            // On commence avec les valeurs basses
            entityLight.SetLightIntensity(0.5f, 1f);
            StartCoroutine(LightRoutine());
        }

        // Lancement de la coroutine du son
        StartCoroutine(SoundRoutine());
    }

    private void Update()
    {
        // Rotation de 360 degrés toutes les secondes (sur l'axe Z)
        if (spriteTransform != null)
        {
            spriteTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator LightRoutine()
    {
        while (true)
        {
            if (lightToggle)
            {
                // Retour à la petite lumière
                entityLight.TransitionLightIntensity(0.5f, 1f, lightTransitionDuration);
            }
            else
            {
                // Passage à la grande lumière
                entityLight.TransitionLightIntensity(2f, 10f, lightTransitionDuration);
            }

            lightToggle = !lightToggle;
            
            // On attend la fin de la transition pour lancer la suivante
            yield return new WaitForSeconds(lightTransitionDuration);
        }
    }

    private IEnumerator SoundRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenSounds);
            
            if (soundContainer != null)
            {
                // On passe 1 en second paramètre (basé sur l'utilisation du projet pour PlaySound)
                soundContainer.PlaySound(soundName, 1);
            }
            else
            {
                Debug.LogWarning("SoundContainer introuvable sur l'objet MyliaSealEffectBehiavor pour jouer " + soundName);
            }
        }
    }
}
