using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuraBehavior : MonoBehaviour
{
    private EntityLight auraLight;
    private SpriteRenderer auraSprite;

    private Seal auraSeal;
    private float auraTimer;
    private float nextTickTime;

    private bool isPlayerInside = false;
    private Stats playerStatsInAura;

    // Variables pour stocker les buffs appliqués
    private int appliedDefenseBuff = 0;
    private int appliedStrengthBuff = 0;

    public void Initialize(Seal seal)
    {
        auraLight = GetComponent<EntityLight>();
        auraSprite = GetComponentInChildren<SpriteRenderer>();

        Spirit dominantSpirit = seal.GetDominantSpiritForArchetype("Aura");
        Color auraColor = dominantSpirit != null ? dominantSpirit.essenceColor : Color.white;
        auraColor.a = 1f; // On force l'alpha à 1 pour la couleur de base, au cas où l'alpha sur l'éditeur est à 0.

        if (auraLight != null)
            auraLight.SetLightColor(auraColor);

        if (auraSprite != null)
            auraSprite.color = auraColor;

        auraSeal = seal;
        auraTimer = seal.auraDuration;
        nextTickTime = Time.time; // Premier tick immédiat

        // Ajuster l'échelle globale pour le rayon visuel
        float diameter = seal.auraRadius * 2f;
        transform.localScale = new Vector3(diameter, diameter, 1f);

        // Assurer la présence d'un Collider pour détecter l'entrée/sortie du joueur
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;

        // Le scale du transform étant appliqué (diameter), le radius du collider doit être 0.5f 
        // pour que la taille globale soit de radius (car 0.5 * diameter = radius)
        col.radius = 0.5f;

        // Force check if player is already inside upon creation
        Collider2D[] initialColliders = Physics2D.OverlapCircleAll(transform.position, seal.auraRadius);
        foreach (Collider2D collider in initialColliders)
        {
            Stats stats = collider.GetComponent<Stats>();
            if (stats != null && stats.entityType == EntityType.Player)
            {
                ApplyPlayerBuffs(stats);
                break;
            }
        }
    }

    private void Update()
    {
        if (auraSeal == null) return;

        auraTimer -= Time.deltaTime;

        if (auraSeal.auraTickRate > 0)
        {
            float tickDuration = 1f / auraSeal.auraTickRate;
            float timeRemaining = nextTickTime - Time.time;
            float progress = 1f - Mathf.Clamp01(timeRemaining / tickDuration);

            if (auraLight != null)
            {
                // L'objet entier est scale en diametre (rayon * 2).
                // Donc pour atteindre le rayon de l'aura en coordonnée locale, le rayon doit être 0.5f.
                float maxLocalRadius = auraSeal.auraRadius + 1;
                float minLocalRadius = maxLocalRadius / 4f;
                float currentRadius = Mathf.Lerp(maxLocalRadius, minLocalRadius, progress);

                // L'intensité de la lumière vaut la taille de l'aura
                auraLight.SetLightIntensity(auraSeal.auraRadius, currentRadius);
            }

            if (auraSprite != null)
            {
                Color c = auraSprite.color;
                c.a = Mathf.Lerp(1f, 0f, progress);
                auraSprite.color = c;
            }

            if (Time.time >= nextTickTime)
            {
                ApplyAuraTickEffects();
                nextTickTime = Time.time + tickDuration;
            }
        }

        if (auraTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyAuraTickEffects()
    {
        GetComponent<SoundContainer>().PlaySound("Pulse", 2);
        float radius = auraSeal.auraRadius;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D col in colliders)
        {
            Stats entityStats = col.GetComponent<Stats>();
            if (entityStats == null) continue;

            // Effets sur les monstres
            if (entityStats.entityType == EntityType.Monster || entityStats.entityType == EntityType.Boss)
            {
                if (auraSeal.enemyDamagePerTick > 0)
                {
                    LifeManager enemyLifeManager = col.GetComponent<LifeManager>();
                    if (enemyLifeManager != null)
                    {
                        int damageToDeal = Mathf.RoundToInt(auraSeal.enemyDamagePerTick * 100);
                        if (damageToDeal < 1 && auraSeal.enemyDamagePerTick > 0f) damageToDeal = 1;

                        enemyLifeManager.TakeDamage(damageToDeal, Color.red, true, true);
                    }
                }

                if (auraSeal.enemySlow > 0)
                {
                    EntityEffects enemyEffects = col.GetComponent<EntityEffects>();
                    if (enemyEffects != null)
                    {
                        // Le ralentissement dure jusqu'au prochain tick + petite marge
                        float slowDuration = (1f / auraSeal.auraTickRate) + 0.1f;
                        enemyEffects.ApplyAuraSlow(auraSeal.enemySlow, slowDuration);
                    }
                }
            }
            // Soin du joueur à chaque tick
            else if (entityStats.entityType == EntityType.Player)
            {
                if (auraSeal.playerHpRegenPerTick > 0)
                {
                    LifeManager playerLifeManager = col.GetComponent<LifeManager>();
                    if (playerLifeManager != null)
                    {
                        int healAmount = Mathf.RoundToInt(auraSeal.playerHpRegenPerTick * 100);
                        if (healAmount < 1 && auraSeal.playerHpRegenPerTick > 0f) healAmount = 1;

                        playerLifeManager.Heal(healAmount);
                    }
                }
            }
        }
    }

    private void ApplyPlayerBuffs(Stats stats)
    {
        if (stats != null && stats.entityType == EntityType.Player && !isPlayerInside)
        {
            isPlayerInside = true;
            playerStatsInAura = stats;

            // Arrondi mathématique standard (calculé en pourcentage des statistiques actuelles)
            appliedDefenseBuff = Mathf.RoundToInt(stats.defense * auraSeal.playerDefensePerTick);
            appliedStrengthBuff = Mathf.RoundToInt(stats.strength * auraSeal.playerStrengthPerTick);

            stats.defense += appliedDefenseBuff;
            stats.strength += appliedStrengthBuff;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (auraSeal == null) return;

        Stats stats = collision.GetComponent<Stats>();
        ApplyPlayerBuffs(stats);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (auraSeal == null) return;

        Stats stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player && isPlayerInside)
        {
            RemovePlayerBuffs();
        }
    }

    private void OnDestroy()
    {
        if (isPlayerInside)
        {
            RemovePlayerBuffs();
        }
    }

    private void RemovePlayerBuffs()
    {
        if (playerStatsInAura != null && auraSeal != null)
        {
            playerStatsInAura.defense -= appliedDefenseBuff;
            playerStatsInAura.strength -= appliedStrengthBuff;
        }
        isPlayerInside = false;
        playerStatsInAura = null;
        appliedDefenseBuff = 0;
        appliedStrengthBuff = 0;
    }
}
