using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealAuraManager : MonoBehaviour
{
    private float cooldownTimer = 0f;
    private Stats playerStats;

    [Header("Aura Settings")]
    public GameObject auraPrefab;

    private void Start()
    {
        playerStats = GetComponent<Stats>();

        // Charger le prefab par défaut si non assigné
        if (auraPrefab == null)
        {
            auraPrefab = Resources.Load<GameObject>("Prefabs/Particles/AuraVisual");
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public void TryActivateAura()
    {
        if (cooldownTimer > 0) return;

        Seal currentSeal = SealManager.instance != null ? SealManager.instance.equippedSeal : null;

        // On vérifie que le joueur a bien l'archétype Aura d'activé sur ses stats
        if (currentSeal != null && playerStats != null && currentSeal.isAuraActive)
        {
            ActivateAura(currentSeal);
        }
    }

    private void ActivateAura(Seal seal)
    {
        cooldownTimer = 120f; // Cooldown de 120 secondes

        if (auraPrefab != null)
        {
            // Instancier l'Aura au sol (ne suit pas le joueur puisqu'elle n'est pas enfant du joueur)
            GameObject auraObj = Instantiate(auraPrefab, transform.position, Quaternion.identity);

            AuraBehavior auraBehavior = auraObj.GetComponent<AuraBehavior>();
            if (auraBehavior == null)
            {
                auraBehavior = auraObj.AddComponent<AuraBehavior>();
            }

            auraBehavior.Initialize(seal);
        }
        else
        {
            Debug.LogWarning("SealAuraManager: Le Prefab AuraVisual est manquant.");
        }

        SoundContainer soundContainer = GetComponent<SoundContainer>();
        if (soundContainer != null)
        {
            // soundContainer.PlaySound("AuraActivate", 1); // Décommenter/Ajuster si le son est disponible
        }
    }
}
