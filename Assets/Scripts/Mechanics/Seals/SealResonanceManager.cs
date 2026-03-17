using UnityEngine;

public class SealResonanceManager : MonoBehaviour
{
    private Seal currentSeal;
    private Stats playerStats;

    [Header("Resonance Prefabs")]
    [Tooltip("Prefab for Explosion effect")]
    public GameObject explosionPrefab;
    [Tooltip("Prefab for Freeze/Ice effect")]
    public GameObject freezePrefab;
    [Tooltip("Prefab for Electricity effect")]
    public GameObject electricityPrefab;

    private void Start()
    {
        playerStats = GetComponent<Stats>();
        UpdateSealReference();
    }

    public void UpdateSealReference()
    {
        currentSeal = SealManager.instance != null ? SealManager.instance.equippedSeal : null;
    }

    /// <summary>
    /// Appelé lors d'un impact (attaque, sort, etc.) pour tenter de déclencher la résonance.
    /// </summary>
    /// <param name="impactPosition">La position où l'effet doit apparaître.</param>
    public void TryTriggerResonance(Vector3 impactPosition)
    {
        UpdateSealReference();

        // On vérifie que le joueur a bien l'archétype Résonance d'activé sur ses stats
        if (currentSeal != null && playerStats != null && currentSeal.isResonanceActive)
        {
            // Tirage aléatoire en fonction de activationChancePercent
            // Ex: 0.15f -> 15% de chance
            float random = Random.Range(0f, 1f);
            if (random <= currentSeal.activationChancePercent)
            {
                ActivateResonance(impactPosition);
            }
        }
    }

    private void ActivateResonance(Vector3 position)
    {
        GameObject prefabToSpawn = null;

        switch (currentSeal.resonanceEffect)
        {
            case ResonanceEffectType.Explosion:
                prefabToSpawn = explosionPrefab;
                break;
            case ResonanceEffectType.FreezeEntity:
                prefabToSpawn = freezePrefab;
                break;
            case ResonanceEffectType.Electricity:
                prefabToSpawn = electricityPrefab;
                break;
        }

        if (prefabToSpawn != null)
        {
            GameObject effectInstance = Instantiate(prefabToSpawn, position, Quaternion.identity);

            // Tenter de récupérer un script ResonanceEffectBehavior pour lui passer les stats
            ResonanceEffectBehavior behavior = effectInstance.GetComponent<ResonanceEffectBehavior>();
            if (behavior == null)
            {
                // Ajout automatique du script de base s'il n'est pas présent
                behavior = effectInstance.AddComponent<ResonanceEffectBehavior>();
            }

            // On lui passe les paramètres clés du sceau
            behavior.Initialize(currentSeal, currentSeal.resonanceRadius, currentSeal.effectMagnitudePercent);
        }
        else
        {
            Debug.LogWarning($"SealResonanceManager: Prefab pour l'effet {currentSeal.resonanceEffect} non assigné.");
        }
        
        SoundContainer soundContainer = GetComponent<SoundContainer>();
        if (soundContainer != null)
        {
            // soundContainer.PlaySound("ResonanceActivate", 1); // Décommenter si un son global existe
        }
    }
}
