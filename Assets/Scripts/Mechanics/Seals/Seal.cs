using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Seal", menuName = "Alchemy/Seal")]
public class Seal : ScriptableObject
{
    [Header("Identity")]
    public List<EssenceComposition> essenceComposition = new List<EssenceComposition>();
    public Sprite sprite;

    [Header("Active Archetypes")]
    public bool isResonanceActive = false;
    public bool isBuffActive = false;
    public bool isAuraActive = false;
    public bool isMomentumActive = false;

    [Header("Archetype Scores (Debug)")]
    [SerializeField] private float resonanceScore = 0f;
    [SerializeField] private float buffScore = 0f;
    [SerializeField] private float auraScore = 0f;
    [SerializeField] private float momentumScore = 0f;

    [Header("Cursor Influences - BUFF")]
    public float hpPercent = 0f;
    public float forcePercent = 0f;
    public float defensePercent = 0f;
    public float speedPercent = 0f;
    public float critDmg = 0f;
    public float critChance = 0f;
    public float bowSpeedPercent = 0f;
    public float pickaxeSpeedPercent = 0f;
    public float dodgeChancePercent = 0f;

    [Header("Cursor Influences - RESONANCE")]
    public ResonanceEffectType resonanceEffect;
    public float resonanceRadius = 1f;
    public float activationChancePercent = .15f;
    public float effectMagnitudePercent = .5f;

    [Header("Cursor Influences - AURA")]
    public float auraRadius = 3f;
    public float auraDuration = 5f;
    public float auraTickRate = 0f;
    // Player effects
    public float playerHpRegenPerTick = 0f;
    public float playerDefensePerTick = 0f;
    public float playerStrengthPerTick = 0f;
    // Enemy effects
    public float enemySlow = 0f;
    public float enemyDamagePerTick = 0f;

    [Header("Cursor Influences - MOMENTUM")]
    public MomentumTriggerType momentumTrigger;
    public float maxStacks = 3f;
    public float stackDecay = 5f;
    // % de la statistique concernée
    public float forceStack = 0f;
    public float defenseStack = 0f;
    public float speedStack = 0f;
    public float squareCoinStack = 0f;
    public float bowSpeedPercentStack = 0f;
    public float pickaxeSpeedPercentStack = 0f;
    public float dodgeChancePercentStack = 0f;

    // Génère le sceau à partir de 4 items spéciaux
    public void GenerateSeal(SpecialItems[] specialItems)
    {
        if (specialItems == null || specialItems.Length != 4)
        {
            Debug.LogError("GenerateSeal requires exactly 4 special items!");
            return;
        }

        // Dictionnaire temporaire pour accumuler les pourcentages par essence
        Dictionary<Spirit, float> essenceAccumulator = new Dictionary<Spirit, float>();

        // Pour chaque item spécial
        foreach (var item in specialItems)
        {
            if (item == null || item.essenceComposition == null) continue;

            // Récupérer les essences normalisées de l'item
            Dictionary<Spirit, float> itemEssences = item.GetNormalizedEssences();

            // Additionner chaque essence (on divise par 4 car on fait la moyenne)
            foreach (var kvp in itemEssences)
            {
                if (essenceAccumulator.ContainsKey(kvp.Key))
                {
                    essenceAccumulator[kvp.Key] += kvp.Value / 4f;
                }
                else
                {
                    essenceAccumulator[kvp.Key] = kvp.Value / 4f;
                }
            }
        }

        // Convertir le dictionnaire en liste EssenceComposition
        essenceComposition.Clear();

        foreach (var kvp in essenceAccumulator)
        {
            essenceComposition.Add(new EssenceComposition
            {
                essence = kvp.Key,
                percentage = kvp.Value * 100f // Convertir en pourcentage
            });
        }

        // Optionnel : Trier par pourcentage décroissant pour plus de clarté
        essenceComposition.Sort((a, b) => b.percentage.CompareTo(a.percentage));

        // Générer les stats du sceau basées sur cette composition
        GenerateSealStats();
    }

    // Génère les statistiques du sceau basées sur la composition d'essences
    private void GenerateSealStats()
    {
        // Réinitialiser toutes les stats
        ResetStats();

        // Pour chaque essence dans la composition
        foreach (var essenceComp in essenceComposition)
        {
            if (essenceComp.essence == null) continue;

            Spirit spirit = essenceComp.essence;
            float percent = essenceComp.percentage / 100f; // Convertir en 0-1

            // BUFF
            hpPercent += spirit.hpPerPercent * percent;
            forcePercent += spirit.forcePerPercent * percent;
            defensePercent += spirit.defensePerPercent * percent;
            speedPercent += spirit.speedPercent * percent;
            critDmg += spirit.critDmg * percent;
            critChance += spirit.critChance * percent;
            bowSpeedPercent += spirit.bowSpeedPercent * percent;
            pickaxeSpeedPercent += spirit.pickaxeSpeedPercent * percent;
            dodgeChancePercent += spirit.dodgeChancePercent * percent;

            // RESONANCE
            resonanceRadius += spirit.radiusPerPercent * percent;
            activationChancePercent += spirit.activationChancePerPercent * percent;
            effectMagnitudePercent += spirit.effectMagnitudePerPercent * percent;

            // AURA
            auraRadius += spirit.auraRadiusPerPercent * percent;
            auraDuration += spirit.auraDurationPerPercent * percent;
            auraTickRate += spirit.auraTickRatePerPercent * percent;
            playerHpRegenPerTick += spirit.playerHpRegenPerPercent * percent;
            playerDefensePerTick += spirit.playerDefensePerPercent * percent;
            playerStrengthPerTick += spirit.playerStrengthPerPercent * percent;
            enemySlow += spirit.enemySlowPerPercent * percent;
            enemyDamagePerTick += spirit.enemyDamagePerPercent * percent;

            // MOMENTUM
            maxStacks += spirit.maxStacksPerPercent * percent;
            stackDecay += spirit.stackDecayPerPercent * percent;
            forceStack += spirit.forceStackPerPercent * percent;
            defenseStack += spirit.defenseStackPerPercent * percent;
            speedStack += spirit.speedStackPerPercent * percent;
            squareCoinStack += spirit.squareCoinStackPerPercent * percent;
            bowSpeedPercentStack += spirit.bowSpeedPercentStackPerPercent * percent;
            pickaxeSpeedPercentStack += spirit.pickaxeSpeedPercentStackPerPercent * percent;
            dodgeChancePercentStack += spirit.dodgeChancePercentStackPerPercent * percent;
        }

        // Déterminer l'archétype dominant et les effets associés
        DetermineArchetype();

        Debug.Log($"Seal generated with {essenceComposition.Count} essences");
    }

    private void ResetStats()
    {
        hpPercent = 0f;
        forcePercent = 0f;
        defensePercent = 0f;
        speedPercent = 0f;
        critDmg = 0f;
        critChance = 0f;
        bowSpeedPercent = 0f;
        pickaxeSpeedPercent = 0f;
        dodgeChancePercent = 0f;

        resonanceRadius = 1f;
        activationChancePercent = 0.15f;
        effectMagnitudePercent = 0.5f;

        auraRadius = 3f;
        auraDuration = 5f;
        auraTickRate = 0f;
        playerHpRegenPerTick = 0f;
        playerDefensePerTick = 0f;
        playerStrengthPerTick = 0f;
        enemySlow = 0f;
        enemyDamagePerTick = 0f;

        maxStacks = 3f;
        stackDecay = 5f;
        forceStack = 0f;
        defenseStack = 0f;
        speedStack = 0f;
        squareCoinStack = 0f;
        bowSpeedPercentStack = 0f;
        pickaxeSpeedPercentStack = 0f;
        dodgeChancePercentStack = 0f;
    }

    private void DetermineArchetype()
    {
        // Réinitialiser les scores
        resonanceScore = 0f;
        buffScore = 0f;
        auraScore = 0f;
        momentumScore = 0f;

        // Calculer les scores de chaque archétype (somme pondérée des votes)
        foreach (var essenceComp in essenceComposition)
        {
            if (essenceComp.essence == null) continue;

            Spirit spirit = essenceComp.essence;
            float weight = essenceComp.percentage / 100f;

            resonanceScore += spirit.votesResonance * weight;
            buffScore += spirit.votesBuff * weight;
            auraScore += spirit.votesAura * weight;
            momentumScore += spirit.votesMomentum * weight;
        }

        // Déterminer quels archétypes sont actifs
        isResonanceActive = resonanceScore >= 1.5f;
        isBuffActive = buffScore >= 1.5f;
        isAuraActive = auraScore >= 1.5f;
        isMomentumActive = momentumScore >= 1.5f;

        // Définir les effets pour les archétypes actifs
        if (isResonanceActive)
        {
            Spirit dominantSpirit = GetDominantSpiritForArchetype("Resonance");
            if (dominantSpirit != null)
                resonanceEffect = dominantSpirit.resonanceEffect;
        }

        if (isMomentumActive)
        {
            Spirit dominantSpirit = GetDominantSpiritForArchetype("Momentum");
            if (dominantSpirit != null)
                momentumTrigger = dominantSpirit.momentumTrigger;
        }

        // Log des archétypes actifs
        List<string> activeArchetypes = new List<string>();
        if (isResonanceActive) activeArchetypes.Add($"Resonance({resonanceScore:F2})");
        if (isBuffActive) activeArchetypes.Add($"Buff({buffScore:F2})");
        if (isAuraActive) activeArchetypes.Add($"Aura({auraScore:F2})");
        if (isMomentumActive) activeArchetypes.Add($"Momentum({momentumScore:F2})");

        if (activeArchetypes.Count > 0)
        {
            Debug.Log($"Active archetypes: {string.Join(", ", activeArchetypes)}");
        }
        else
        {
            Debug.LogWarning("No archetype reached the 0.75 threshold!");
        }
    }

    private Spirit GetDominantSpiritForArchetype(string archetype)
    {
        Spirit dominant = null;
        float maxPercentage = 0f;

        foreach (var comp in essenceComposition)
        {
            if (comp.essence == null) continue;

            // Vérifier si cette essence a des votes pour cet archétype
            int votes = 0;
            switch (archetype)
            {
                case "Resonance": votes = comp.essence.votesResonance; break;
                case "Buff": votes = comp.essence.votesBuff; break;
                case "Aura": votes = comp.essence.votesAura; break;
                case "Momentum": votes = comp.essence.votesMomentum; break;
            }

            if (votes > 0 && comp.percentage > maxPercentage)
            {
                maxPercentage = comp.percentage;
                dominant = comp.essence;
            }
        }

        return dominant;
    }
}