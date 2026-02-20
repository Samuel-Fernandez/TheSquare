using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SealDescriptionUI : MonoBehaviour
{
    [Header("Seal Description Properties")]
    // Ajouter le Seal ici

    [Header("TextMeshPro")]
    public TextMeshProUGUI sealTitleTxt;
    public TextMeshProUGUI neutralTxt;
    public TextMeshProUGUI fireTxt;
    public TextMeshProUGUI groundTxt;
    public TextMeshProUGUI lightTxt;
    public TextMeshProUGUI shadowTxt;
    public TextMeshProUGUI vegetalTxt;
    public TextMeshProUGUI waterTxt;
    public TextMeshProUGUI windTxt;

    [Header("Buff")]
    public TextMeshProUGUI buffTitle;

    public TextMeshProUGUI hpTxt;
    public TextMeshProUGUI strTxt;
    public TextMeshProUGUI defTxt;
    public TextMeshProUGUI speTxt;
    public TextMeshProUGUI critCTxt;
    public TextMeshProUGUI critDTxt;
    public TextMeshProUGUI bowSTxt;
    public TextMeshProUGUI pickSTxt;
    public TextMeshProUGUI dodcTxt;

    [Header("Resonnance")]
    public TextMeshProUGUI resonnanceTitle;

    public TextMeshProUGUI effectTxt;
    public TextMeshProUGUI resonnanceRadiusTxt;
    public TextMeshProUGUI actCTxt;
    public TextMeshProUGUI mageffectTxt;

    [Header("Aura")]
    public TextMeshProUGUI auraTitle;

    public TextMeshProUGUI auraRadiusTxt;
    public TextMeshProUGUI durationTxt;
    public TextMeshProUGUI tickRateTxt;
    public TextMeshProUGUI regenTxt;
    public TextMeshProUGUI defenseTxt;
    public TextMeshProUGUI strengthTxt;
    public TextMeshProUGUI enemiesSlowTxt;
    public TextMeshProUGUI enemiesDamageTxt;

    [Header("Momentum")]
    public TextMeshProUGUI momentumTitle;

    public TextMeshProUGUI triggerTxt;
    public TextMeshProUGUI maxStackTxt;
    public TextMeshProUGUI stackDecayTxt;
    public TextMeshProUGUI strStackTxt;
    public TextMeshProUGUI defStackTxt;
    public TextMeshProUGUI speStackTxt;
    public TextMeshProUGUI squareCoinsStackTxt;
    public TextMeshProUGUI bowsStackTxt;
    public TextMeshProUGUI pickaxesStackTxt;
    public TextMeshProUGUI dodgecStackTxt;

    [Header("Container")]
    public GameObject momentumContainer;

    public GameObject buffContainer;
    public GameObject resonnanceContainer;
    public GameObject auraContainer;

    public GameObject neutralContainer;
    public GameObject fireContainer;
    public GameObject groundContainer;
    public GameObject lightContainer;
    public GameObject shadowContainer;
    public GameObject vegetalContainer;
    public GameObject waterContainer;
    public GameObject windContainer;

    public void ResetUI()
    {
        // Containers d'archétypes
        momentumContainer?.SetActive(false);
        buffContainer?.SetActive(false);
        resonnanceContainer?.SetActive(false);
        auraContainer?.SetActive(false);

        // Containers d'essences
        neutralContainer?.SetActive(false);
        fireContainer?.SetActive(false);
        groundContainer?.SetActive(false);
        lightContainer?.SetActive(false);
        shadowContainer?.SetActive(false);
        vegetalContainer?.SetActive(false);
        waterContainer?.SetActive(false);
        windContainer?.SetActive(false);

        // Textes de BUFF
        hpTxt?.gameObject.SetActive(false);
        strTxt?.gameObject.SetActive(false);
        defTxt?.gameObject.SetActive(false);
        speTxt?.gameObject.SetActive(false);
        critCTxt?.gameObject.SetActive(false);
        critDTxt?.gameObject.SetActive(false);
        bowSTxt?.gameObject.SetActive(false);
        pickSTxt?.gameObject.SetActive(false);
        dodcTxt?.gameObject.SetActive(false);

        // Textes de RESONANCE
        effectTxt?.gameObject.SetActive(false);
        resonnanceRadiusTxt?.gameObject.SetActive(false);
        actCTxt?.gameObject.SetActive(false);
        mageffectTxt?.gameObject.SetActive(false);

        // Textes d'AURA
        auraRadiusTxt?.gameObject.SetActive(false);
        durationTxt?.gameObject.SetActive(false);
        tickRateTxt?.gameObject.SetActive(false);
        regenTxt?.gameObject.SetActive(false);
        defenseTxt?.gameObject.SetActive(false);
        strengthTxt?.gameObject.SetActive(false);
        enemiesSlowTxt?.gameObject.SetActive(false);
        enemiesDamageTxt?.gameObject.SetActive(false);

        // Textes de MOMENTUM
        triggerTxt?.gameObject.SetActive(false);
        maxStackTxt?.gameObject.SetActive(false);
        stackDecayTxt?.gameObject.SetActive(false);
        strStackTxt?.gameObject.SetActive(false);
        defStackTxt?.gameObject.SetActive(false);
        speStackTxt?.gameObject.SetActive(false);
        squareCoinsStackTxt?.gameObject.SetActive(false);
        bowsStackTxt?.gameObject.SetActive(false);
        pickaxesStackTxt?.gameObject.SetActive(false);
        dodgecStackTxt?.gameObject.SetActive(false);

        // Textes d'essences
        neutralTxt?.gameObject.SetActive(false);
        fireTxt?.gameObject.SetActive(false);
        groundTxt?.gameObject.SetActive(false);
        lightTxt?.gameObject.SetActive(false);
        shadowTxt?.gameObject.SetActive(false);
        vegetalTxt?.gameObject.SetActive(false);
        waterTxt?.gameObject.SetActive(false);
        windTxt?.gameObject.SetActive(false);
    }


    public void SetUI()
    {
        // Réinitialiser l'UI (tout désactiver)
        ResetUI();

        // Vérifier qu'un sceau est équipé
        if (SealManager.instance == null || !SealManager.instance.HasSealEquipped())
        {
            Debug.LogWarning("No seal equipped to display!");
            return;
        }

        Seal seal = SealManager.instance.equippedSeal;

        // === TITRE DU SCEAU ===
        if (sealTitleTxt != null)
            sealTitleTxt.text = seal.name;

        // === COMPOSITION DES ESSENCES ===
        DisplayEssenceComposition(seal);

        // === BUFF ===
        if (seal.isBuffActive)
        {
            buffContainer?.SetActive(true);
            if (buffTitle != null)
                buffTitle.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF");

            if (seal.hpPercent >= 0.001f)
            {
                hpTxt?.gameObject.SetActive(true);
                hpTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF_HP", $"{seal.hpPercent * 100f:F1}");
            }

            if (seal.forcePercent >= 0.001f)
            {
                strTxt?.gameObject.SetActive(true);
                strTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF_STR", $"{seal.forcePercent * 100f:F1}");
            }

            if (seal.defensePercent >= 0.001f)
            {
                defTxt?.gameObject.SetActive(true);
                defTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF_DEF", $"{seal.defensePercent * 100f:F1}");
            }

            if (seal.speedPercent >= 0.001f)
            {
                speTxt?.gameObject.SetActive(true);
                speTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF_SPE", $"{seal.speedPercent * 100f:F1}");
            }

            if (seal.critChance >= 0.001f)
            {
                critCTxt?.gameObject.SetActive(true);
                critCTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF_CRITC", $"{seal.critChance * 100f:F1}");
            }

            if (seal.critDmg >= 0.001f)
            {
                critDTxt?.gameObject.SetActive(true);
                critDTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF_CRITD", $"{seal.critDmg * 100f:F1}");
            }

            if (seal.bowSpeedPercent >= 0.001f)
            {
                bowSTxt?.gameObject.SetActive(true);
                bowSTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF_BOWSPE", $"{seal.bowSpeedPercent * 100f:F1}");
            }

            if (seal.pickaxeSpeedPercent >= 0.001f)
            {
                pickSTxt?.gameObject.SetActive(true);
                pickSTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF_PICKAXESPE", $"{seal.pickaxeSpeedPercent * 100f:F1}");
            }

            if (seal.dodgeChancePercent >= 0.001f)
            {
                dodcTxt?.gameObject.SetActive(true);
                dodcTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_BUFF_DODGEC", $"{seal.dodgeChancePercent * 100f:F1}");
            }
        }

        // === RESONANCE ===
        if (seal.isResonanceActive)
        {
            resonnanceContainer?.SetActive(true);
            if (resonnanceTitle != null)
                resonnanceTitle.text = LocalizationManager.instance.GetText("UI", "SEAL_RESONANCE");

            string effectKey = seal.resonanceEffect switch
            {
                ResonanceEffectType.Explosion => "SEAL_RESONANCE_EFFECT_EXPLOSION",
                ResonanceEffectType.StopEntity => "SEAL_RESONANCE_EFFECT_STOP_ENTITY",
                ResonanceEffectType.Electricity => "SEAL_RESONANCE_EFFECT_ELECTRICITY",
                _ => "SEAL_RESONANCE_EFFECT"
            };
            string effectName = LocalizationManager.instance.GetText("UI", effectKey);

            if (effectTxt != null)
            {
                effectTxt?.gameObject.SetActive(true);
                effectTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_RESONANCE_EFFECT", effectName);
            }

            if (seal.resonanceRadius >= 0.001f && resonnanceRadiusTxt != null)
            {
                resonnanceRadiusTxt?.gameObject.SetActive(true);
                resonnanceRadiusTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_RESONANCE_RADIUS", $"{seal.resonanceRadius:F1}");
            }

            if (seal.activationChancePercent >= 0.001f && actCTxt != null)
            {
                actCTxt?.gameObject.SetActive(true);
                actCTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_RESONANCE_ACTIVATION", $"{seal.activationChancePercent * 100f:F1}");
            }

            if (seal.effectMagnitudePercent >= 0.001f && mageffectTxt != null)
            {
                mageffectTxt?.gameObject.SetActive(true);
                mageffectTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_RESONANCE_MAGNITUDE", $"{seal.effectMagnitudePercent * 100f:F1}");
            }
        }

        // === AURA ===
        if (seal.isAuraActive)
        {
            auraContainer?.SetActive(true);
            if (auraTitle != null)
                auraTitle.text = LocalizationManager.instance.GetText("UI", "SEAL_AURA");

            if (seal.auraRadius >= 0.001f && auraRadiusTxt != null)
            {
                auraRadiusTxt?.gameObject.SetActive(true);
                auraRadiusTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_AURA_RADIUS", $"{seal.auraRadius:F1}");
            }

            if (seal.auraDuration >= 0.001f && durationTxt != null)
            {
                durationTxt?.gameObject.SetActive(true);
                durationTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_AURA_DURATION", $"{seal.auraDuration:F1}");
            }

            if (seal.auraTickRate >= 0.001f && tickRateTxt != null)
            {
                tickRateTxt?.gameObject.SetActive(true);
                tickRateTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_AURA_INTENSITY", $"{seal.auraTickRate:F2}");
            }

            if (seal.playerHpRegenPerTick >= 0.001f && regenTxt != null)
            {
                regenTxt?.gameObject.SetActive(true);
                regenTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_AURA_REGENERATION", $"{seal.playerHpRegenPerTick * 100f:F1}");
            }

            if (seal.playerDefensePerTick >= 0.001f && defenseTxt != null)
            {
                defenseTxt?.gameObject.SetActive(true);
                defenseTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_AURA_DEF", $"{seal.playerDefensePerTick * 100f:F1}");
            }

            if (seal.playerStrengthPerTick >= 0.001f && strengthTxt != null)
            {
                strengthTxt?.gameObject.SetActive(true);
                strengthTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_AURA_STR", $"{seal.playerStrengthPerTick * 100f:F1}");
            }

            if (seal.enemySlow >= 0.001f && enemiesSlowTxt != null)
            {
                enemiesSlowTxt?.gameObject.SetActive(true);
                enemiesSlowTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_AURA_ENEMY_SLOW", $"{seal.enemySlow * 100f:F1}");
            }

            if (seal.enemyDamagePerTick >= 0.001f && enemiesDamageTxt != null)
            {
                enemiesDamageTxt?.gameObject.SetActive(true);
                enemiesDamageTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_AURA_ENEMY_DAMAGE", $"{seal.enemyDamagePerTick * 100f:F1}");
            }
        }

        // === MOMENTUM ===
        if (seal.isMomentumActive)
        {
            momentumContainer?.SetActive(true);
            if (momentumTitle != null)
                momentumTitle.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM");

            if (triggerTxt != null)
            {
                triggerTxt?.gameObject.SetActive(true);
                string triggerKey = seal.momentumTrigger switch
                {
                    MomentumTriggerType.OnKill => "SEAL_MOMENTUM_TRIGGER_ONKILL",
                    MomentumTriggerType.OnDamageTaken => "SEAL_MOMENTUM_TRIGGER_ONDAMAGETAKEN",
                    MomentumTriggerType.OnCrit => "SEAL_MOMENTUM_TRIGGER_ONCRIT",
                    MomentumTriggerType.OnPerfectDodge => "SEAL_MOMENTUM_TRIGGER_ONPERFECTDODGE",
                    MomentumTriggerType.OnMineralMined => "SEAL_MOMENTUM_TRIGGER_ONMINERALMINED",
                    MomentumTriggerType.OnBowUsing => "SEAL_MOMENTUM_TRIGGER_ONBOWUSING",
                    _ => "SEAL_MOMENTUM_TRIGGER"
                };
                string triggerName = LocalizationManager.instance.GetText("UI", triggerKey);
                triggerTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_TRIGGER", triggerName);
            }

            if (seal.maxStacks >= 0.001f && maxStackTxt != null)
            {
                maxStackTxt?.gameObject.SetActive(true);
                maxStackTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_MAXSTACKS", $"{seal.maxStacks:F0}");
            }

            if (seal.stackDecay >= 0.001f && stackDecayTxt != null)
            {
                stackDecayTxt?.gameObject.SetActive(true);
                stackDecayTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_DECAY", $"{seal.stackDecay:F1}");
            }

            if (seal.forceStack >= 0.001f && strStackTxt != null)
            {
                strStackTxt?.gameObject.SetActive(true);
                strStackTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_STR", $"{seal.forceStack * 100f:F1}");
            }

            if (seal.defenseStack >= 0.001f && defStackTxt != null)
            {
                defStackTxt?.gameObject.SetActive(true);
                defStackTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_DEF", $"{seal.defenseStack * 100f:F1}");
            }

            if (seal.speedStack >= 0.001f && speStackTxt != null)
            {
                speStackTxt?.gameObject.SetActive(true);
                speStackTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_SPE", $"{seal.speedStack * 100f:F1}");
            }

            if (seal.squareCoinStack >= 0.001f && squareCoinsStackTxt != null)
            {
                squareCoinsStackTxt?.gameObject.SetActive(true);
                squareCoinsStackTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_SC", $"{seal.squareCoinStack * 100f:F1}");
            }

            if (seal.bowSpeedPercentStack >= 0.001f && bowsStackTxt != null)
            {
                bowsStackTxt?.gameObject.SetActive(true);
                bowsStackTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_BOWS", $"{seal.bowSpeedPercentStack * 100f:F1}");
            }

            if (seal.pickaxeSpeedPercentStack >= 0.001f && pickaxesStackTxt != null)
            {
                pickaxesStackTxt?.gameObject.SetActive(true);
                pickaxesStackTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_PICKAXES", $"{seal.pickaxeSpeedPercentStack * 100f:F1}");
            }

            if (seal.dodgeChancePercentStack >= 0.001f && dodgecStackTxt != null)
            {
                dodgecStackTxt?.gameObject.SetActive(true);
                dodgecStackTxt.text = LocalizationManager.instance.GetText("UI", "SEAL_MOMENTUM_DODGEC", $"{seal.dodgeChancePercentStack * 100f:F1}");
            }
        }
    }


    // Affiche la composition des essences
    private void DisplayEssenceComposition(Seal seal)
    {
        foreach (var essenceComp in seal.essenceComposition)
        {
            if (essenceComp.essence == null) continue;

            string essenceID = essenceComp.essence.essenceID.ToLower();
            float percentage = essenceComp.percentage;

            // Ne pas afficher les essences à 0%
            if (percentage <= 0) continue;

            switch (essenceID)
            {
                case "neutral":
                    neutralContainer?.SetActive(true);
                    if (neutralTxt != null)
                    {
                        neutralTxt.gameObject.SetActive(true);
                        neutralTxt.text = $"Neutral {percentage:F1}%";
                    }
                    break;

                case "fire":
                    fireContainer?.SetActive(true);
                    if (fireTxt != null)
                    {
                        fireTxt.gameObject.SetActive(true);
                        fireTxt.text = $"Fire {percentage:F1}%";
                    }
                    break;

                case "ground":
                    groundContainer?.SetActive(true);
                    if (groundTxt != null)
                    {
                        groundTxt.gameObject.SetActive(true);
                        groundTxt.text = $"Ground {percentage:F1}%";
                    }
                    break;

                case "light":
                    lightContainer?.SetActive(true);
                    if (lightTxt != null)
                    {
                        lightTxt.gameObject.SetActive(true);
                        lightTxt.text = $"Light {percentage:F1}%";
                    }
                    break;

                case "shadow":
                    shadowContainer?.SetActive(true);
                    if (shadowTxt != null)
                    {
                        shadowTxt.gameObject.SetActive(true);
                        shadowTxt.text = $"Shadow {percentage:F1}%";
                    }
                    break;

                case "vegetal":
                    vegetalContainer?.SetActive(true);
                    if (vegetalTxt != null)
                    {
                        vegetalTxt.gameObject.SetActive(true);
                        vegetalTxt.text = $"Vegetal {percentage:F1}%";
                    }
                    break;

                case "water":
                    waterContainer?.SetActive(true);
                    if (waterTxt != null)
                    {
                        waterTxt.gameObject.SetActive(true);
                        waterTxt.text = $"Water {percentage:F1}%";
                    }
                    break;

                case "wind":
                    windContainer?.SetActive(true);
                    if (windTxt != null)
                    {
                        windTxt.gameObject.SetActive(true);
                        windTxt.text = $"Wind {percentage:F1}%";
                    }
                    break;

                default:
                    Debug.LogWarning($"Unknown essence ID: {essenceID}");
                    break;
            }
        }
    }


}
