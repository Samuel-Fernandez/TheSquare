using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealMomentumManager : MonoBehaviour
{
    private Seal currentSeal;
    private Stats playerStats;

    [Header("Momentum State")]
    public int currentStacks = 0;
    private float decayTimer = 0f;

    [Header("Visuals")]
    public GameObject momentumStackPrefab;
    private List<GameObject> activeStackVisuals = new List<GameObject>();
    private float orbitAngle = 0f;
    public float orbitSpeed = 120f; // Vitesse de rotation (degrés par seconde)

    private void Start()
    {
        playerStats = GetComponent<Stats>();
    }

    private void Update()
    {
        if (currentStacks > 0)
        {
            decayTimer -= Time.deltaTime;
            if (decayTimer <= 0)
            {
                RemoveAllStacks();
            }
            else
            {
                // Force la position orbitale des stacks à chaque frame (empêche les bugs de collision/décalage)
                orbitAngle += orbitSpeed * Time.deltaTime;
                UpdateVisualsPositions();
            }
        }
    }

    private void UpdateSealReference()
    {
        currentSeal = SealManager.instance != null ? SealManager.instance.equippedSeal : null;
    }

    public void OnTrigger(MomentumTriggerType trigger)
    {
        UpdateSealReference();

        if (currentSeal != null && playerStats != null && currentSeal.isMomentumActive)
        {
            if (currentSeal.momentumTrigger == trigger)
            {
                AddStack();
            }
        }
    }

    private void AddStack()
    {
        GetComponent<SoundContainer>().PlaySound("MomentumStackAdd", 2);
        // Toujours réinitialiser le chrono lors d'un déclenchement
        decayTimer = currentSeal.stackDecay;

        if (currentStacks < Mathf.FloorToInt(currentSeal.maxStacks))
        {
            currentStacks++;

            // Spawn visual stack
            if (momentumStackPrefab != null)
            {
                GameObject stackVisual = Instantiate(momentumStackPrefab, transform.position, Quaternion.identity, transform);

                // Color settings (based on dominant Momentum spirit)
                Spirit dominantSpirit = currentSeal.GetDominantSpiritForArchetype("Momentum");
                if (dominantSpirit != null)
                {
                    Color spiritColor = dominantSpirit.essenceColor;
                    spiritColor.a = 1f; // Force l'alpha au maximum pour le SpriteRenderer

                    SpriteRenderer sr = stackVisual.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) sr.color = spiritColor;

                    EntityLight el = stackVisual.GetComponentInChildren<EntityLight>();
                    if (el != null)
                    {
                        el.SetLightColor(spiritColor);
                        el.SetLightIntensity(3f, 2f); // Force l'apparition de la lumière
                    }
                }

                activeStackVisuals.Add(stackVisual);
            }

            UpdateVisualsPositions();
            RefreshStats();
        }
    }

    private void UpdateVisualsPositions()
    {
        if (currentSeal == null || activeStackVisuals.Count == 0) return;

        int max = Mathf.FloorToInt(currentSeal.maxStacks);
        if (max <= 0) return;

        for (int i = 0; i < activeStackVisuals.Count; i++)
        {
            if (activeStackVisuals[i] != null)
            {
                float angleOffset = i * (360f / max);
                float currentAngle = (orbitAngle + angleOffset) * Mathf.Deg2Rad;

                float x = Mathf.Cos(currentAngle) * 0.5f;
                float y = Mathf.Sin(currentAngle) * 0.5f;

                activeStackVisuals[i].transform.localPosition = new Vector3(x, y, 0);
            }
        }
    }

    private void RemoveAllStacks()
    {
        GetComponent<SoundContainer>().PlaySound("MomentumEnd", 2);
        currentStacks = 0;
        decayTimer = 0f;
        ClearAllVisuals();
        RefreshStats();
    }

    public void CheckAndResetStacksIfSealChanged()
    {
        UpdateSealReference();
        if (currentSeal == null || !currentSeal.isMomentumActive)
        {
            if (currentStacks > 0)
            {
                currentStacks = 0;
                ClearAllVisuals();
                RefreshStats();
            }
        }
    }

    private void ClearAllVisuals()
    {
        foreach (var visual in activeStackVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        activeStackVisuals.Clear();
    }

    private void RefreshStats()
    {
        playerStats.UpdateStats();
        PlayerManager.instance.UpdateBonuses();
    }
}
