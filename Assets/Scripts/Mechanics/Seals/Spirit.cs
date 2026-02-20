using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Spirit", menuName = "Alchemy/Spirit")]
public class Spirit : ScriptableObject
{
    [Header("Identity")]
    public string essenceID;
    public Color essenceColor;
    public Sprite icon;

    [Header("Archetype Votes")]
    public int votesResonance = 0;
    public int votesBuff = 0;
    public int votesAura = 0;
    public int votesMomentum = 0;

    [Header("Cursor Influences - BUFF")]
    public float hpPerPercent = 0f;
    public float forcePerPercent = 0f;
    public float defensePerPercent = 0f;
    public float speedPercent = 0f;
    public float critDmg = 0f;
    public float critChance = 0f;
    public float bowSpeedPercent = 0f;
    public float pickaxeSpeedPercent = 0f;
    public float dodgeChancePercent = 0f;

    [Header("Cursor Influences - RESONANCE")]
    public ResonanceEffectType resonanceEffect;
    public float radiusPerPercent = 0f;
    public float activationChancePerPercent = 0f;
    public float effectMagnitudePerPercent = 0f;

    [Header("Cursor Influences - AURA")]
    public float auraRadiusPerPercent = 0f;
    public float auraDurationPerPercent = 0f;
    public float auraTickRatePerPercent = 0f;

    // Player effects
    public float playerHpRegenPerPercent = 0f;
    public float playerDefensePerPercent = 0f;
    public float playerStrengthPerPercent = 0f;

    // Enemy effects
    public float enemySlowPerPercent = 0f;
    public float enemyDamagePerPercent = 0f;

    [Header("Cursor Influences - MOMENTUM")]
    public MomentumTriggerType momentumTrigger;
    public float maxStacksPerPercent = 0f;
    public float stackDecayPerPercent = 0f;

    // % de la statistique concernée
    public float forceStackPerPercent = 0f;
    public float defenseStackPerPercent = 0f;
    public float speedStackPerPercent = 0f;
    public float squareCoinStackPerPercent = 0f;
    public float bowSpeedPercentStackPerPercent = 0f;
    public float pickaxeSpeedPercentStackPerPercent = 0f;
    public float dodgeChancePercentStackPerPercent = 0f;
}

// Enums pour typer les effets
public enum ResonanceEffectType
{
    Explosion,
    StopEntity,
    Electricity,
}

public enum MomentumTriggerType
{
    OnKill,
    OnDamageTaken,
    OnCrit,
    OnPerfectDodge,
    OnMineralMined,
    OnBowUsing,
}