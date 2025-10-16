using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipementStats : MonoBehaviour
{
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtDescription;
    public TextMeshProUGUI txtEnchant;
    public TextMeshProUGUI txtRarity;
    public TextMeshProUGUI txtLevel;
    public GameObject txtDamage;
    public GameObject txtSpeed;
    public GameObject txtHealPoint;
    public GameObject txtDefense;
    public GameObject txtCritChance;
    public GameObject txtCritDamage;
    public GameObject txtKnockbackPower;
    public GameObject txtKnockbackResistance;
    public GameObject txtValue;

    public GameObject txtDragonSkin;
    public GameObject txtRegenRate;
    public GameObject txtNegativeEffectReducer;
    public GameObject txtMineralChance;
    public GameObject txtDodgeChance;
    public GameObject txtDoubleMineralDropChance;

    public GameObject txtVampire;
    public GameObject txtFireAttackChance;
    public GameObject txtIceAttackChance;
    public GameObject txtPoisonAttackChance;
    public GameObject txtDoubleSquareCoinsChances;
    public GameObject txtDropChance;

    public GameObject txtDamageBonus;
    public GameObject txtSpeedBonus;
    public GameObject txtHealPointBonus;
    public GameObject txtDefenseBonus;
    public GameObject txtCritChanceBonus;
    public GameObject txtCritDamageBonus;
    public GameObject txtKnockbackPowerBonus;
    public GameObject txtKnockbackResistanceBonus;
    public GameObject txtValueBonus;

    public GameObject equipButton;
    public GameObject consumeButton;
    public GameObject trashButton;

    public bool hideButton;

    private Color color1;
    private Color color2;
    private float colorTransitionTime = 2f; // Duration of the color transition in seconds
    private float transitionTimer = 0f; // Timer to keep track of transition progress
    private bool transitioningToColor2 = true; // Direction of transition
    private bool isTransitioning = false; // To track if a transition is ongoing

    void Update()
    {
        // Handle color transition for txtEnchant
        if (isTransitioning)
        {
            HandleColorTransition();
        }
    }

    public void WriteBonusStats(int damage = 0, float speed = 0, int healPoint = 0, int defense = 0,
                            float critChance = 0, float critDamage = 0, float knockbackPower = 0,
                            float knockbackResistance = 0, int value = 0)
    {
        if (damage > 0)
        {
            txtDamageBonus.SetActive(true);
            txtDamageBonus.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            txtDamageBonus.GetComponent<TextMeshProUGUI>().text = "+" + damage;
        }
        else
        {
            txtDamageBonus.SetActive(false);
        }

        if (speed > 0)
        {
            txtSpeedBonus.SetActive(true);
            txtSpeedBonus.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            txtSpeedBonus.GetComponent<TextMeshProUGUI>().text = "+" + speed;
        }
        else
        {
            txtSpeedBonus.SetActive(false);
        }

        if (healPoint > 0)
        {
            txtHealPointBonus.SetActive(true);
            txtHealPointBonus.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            txtHealPointBonus.GetComponent<TextMeshProUGUI>().text = "+" + healPoint;
        }
        else
        {
            txtHealPointBonus.SetActive(false);
        }

        if (defense > 0)
        {
            txtDefenseBonus.SetActive(true);
            txtDefenseBonus.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            txtDefenseBonus.GetComponent<TextMeshProUGUI>().text = "+" + defense;
        }
        else
        {
            txtDefenseBonus.SetActive(false);
        }

        if (critChance > 0)
        {
            txtCritChanceBonus.SetActive(true);
            txtCritChanceBonus.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            txtCritChanceBonus.GetComponent<TextMeshProUGUI>().text = "+" + critChance.ToString("F2") + "%";
        }
        else
        {
            txtCritChanceBonus.SetActive(false);
        }

        if (critDamage > 0)
        {
            txtCritDamageBonus.SetActive(true);
            txtCritDamageBonus.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            txtCritDamageBonus.GetComponent<TextMeshProUGUI>().text = "+" + critDamage.ToString("F2") + "%";
        }
        else
        {
            txtCritDamageBonus.SetActive(false);
        }

        if (knockbackPower > 0)
        {
            txtKnockbackPowerBonus.SetActive(true);
            txtKnockbackPowerBonus.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            txtKnockbackPowerBonus.GetComponent<TextMeshProUGUI>().text = "+" + knockbackPower.ToString("F2");
        }
        else
        {
            txtKnockbackPowerBonus.SetActive(false);
        }

        if (knockbackResistance > 0)
        {
            txtKnockbackResistanceBonus.SetActive(true);
            txtKnockbackResistanceBonus.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            txtKnockbackResistanceBonus.GetComponent<TextMeshProUGUI>().text = "+" + knockbackResistance.ToString("F2");
        }
        else
        {
            txtKnockbackPowerBonus.SetActive(false);
        }

        if (value > 0)
        {
            txtValueBonus.SetActive(true);
            txtValueBonus.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            txtValueBonus.GetComponent<TextMeshProUGUI>().text = "+" + value;
        }
        else
        {
            txtValueBonus.SetActive(false);
        }
    }

    public void WriteStats(Item item)
    {
        if (item)
        {
            ResetTextFields();

            if(!hideButton)
            {
                equipButton.SetActive(item is not Consumables);
                consumeButton.SetActive(item is Consumables);
                trashButton.SetActive(true);
            }
            else
            {
                if(equipButton)
                    equipButton.SetActive(false);
                if(consumeButton)
                    consumeButton.SetActive(false);
                if(trashButton)
                    trashButton.SetActive(false);
            }
            

            txtTitle.gameObject.SetActive(true);
            txtDescription.gameObject.SetActive(true);

            if(txtValue.activeSelf)
                txtValue.gameObject.SetActive(true);
            txtRarity.gameObject.SetActive(true);

            if(item is not SpecialItems)
            {
                txtLevel.gameObject.SetActive(true);

                txtLevel.text = LocalizationManager.instance.GetText("UI", "LEVEL_ABREVIATION_TEXT") + " " + item.level;
                if (item.level == item.GetMaxLevel())
                    txtLevel.color = Color.yellow;
                else
                    txtLevel.color = Color.white;
            }
            else
            {
                txtLevel.gameObject.SetActive(false);

            }

            txtTitle.text = txtRarity.text = LocalizationManager.instance.GetText("items", item.GetID() + "_NAME");
            txtDescription.text = LocalizationManager.instance.GetText("items", item.GetID() + "_DESCRIPTION");
            txtValue.GetComponentInChildren<TextMeshProUGUI>().text = "Value " + item.value;

            switch (item.rarity)
            {
                case Rarity.COMMON:
                    txtRarity.text = LocalizationManager.instance.GetText("UI", "INVENTORY_ITEMS_COMMON");
                    txtRarity.color = Color.white;
                    break;
                case Rarity.UNCOMMON:
                    txtRarity.text = LocalizationManager.instance.GetText("UI", "INVENTORY_ITEMS_UNCOMMON");
                    txtRarity.color = Color.green;
                    break;
                case Rarity.RARE:
                    txtRarity.text = LocalizationManager.instance.GetText("UI", "INVENTORY_ITEMS_RARE");
                    txtRarity.color = Color.blue;
                    break;
                case Rarity.EPIC:
                    txtRarity.text = LocalizationManager.instance.GetText("UI", "INVENTORY_ITEMS_EPIC");
                    txtRarity.color = Color.magenta;
                    break;
                case Rarity.LEGENDARY:
                    txtRarity.text = LocalizationManager.instance.GetText("UI", "INVENTORY_ITEMS_LEGENDARY");
                    txtRarity.color = Color.yellow;
                    break;
                default:
                    txtRarity.color = Color.white;
                    break;
            }

            if (item is Weapon weapon)
            {
                SetTextVisibility(txtDamage, weapon.damage);
                SetTextVisibility(txtKnockbackPower, weapon.knockbackPower);
                SetTextVisibility(txtCritChance, weapon.critChance * 100, true);
                SetTextVisibility(txtCritDamage, weapon.critDamage * 100, true);

                SetTextVisibility(txtVampire, weapon.vampire * 100, true);
                SetTextVisibility(txtFireAttackChance, weapon.fireAttackChance * 100, true);
                SetTextVisibility(txtIceAttackChance, weapon.iceAttackChance * 100, true);
                SetTextVisibility(txtPoisonAttackChance, weapon.poisonAttackChance * 100, true);
                SetTextVisibility(txtDoubleSquareCoinsChances, weapon.doubleSquareCoinsChances * 100, true);
                SetTextVisibility(txtDropChance, weapon.dropChance * 100, true);

                if (weapon.enchant != WEAPON_ENCHANT.NULL)
                {
                    txtEnchant.gameObject.SetActive(true);
                    StartColorTransition(weapon.colorEnchant1, weapon.colorEnchant2);
                    txtEnchant.text = LocalizationManager.instance.GetText("effects", weapon.enchant.ToString(), weapon.enchantLevel * 5);
                }
            }
            else if (item is Boots boots)
            {
                SetTextVisibility(txtDefense, boots.defense);
                SetTextVisibility(txtHealPoint, boots.life);
                SetTextVisibility(txtSpeed, boots.speed);

                SetTextVisibility(txtDragonSkin, boots.dragonSkin * 100, true);
                SetTextVisibility(txtRegenRate, boots.regenRate);
                SetTextVisibility(txtNegativeEffectReducer, boots.negativeEffectReducer * 100, true);
                SetTextVisibility(txtMineralChance, boots.mineralChance * 100, true);
                SetTextVisibility(txtDodgeChance, boots.dodgeChance * 100, true);
                SetTextVisibility(txtDoubleMineralDropChance, boots.doubleMineralDropChance * 100, true);


                if (boots.armorEnchant != ARMOR_ENCHANT.NULL)
                {
                    txtEnchant.gameObject.SetActive(true);
                    StartColorTransition(boots.colorEnchant1, boots.colorEnchant2);

                    switch (boots.armorEnchant)
                    {
                        case ARMOR_ENCHANT.DRAGON_SKIN:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "DRAGON_SKIN", boots.enchantLevel * 3);
                            break;
                        case ARMOR_ENCHANT.REGEN:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "REGEN", boots.enchantLevel);
                            break;
                        case ARMOR_ENCHANT.FANTOM_DODGE:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "FANTOM_DODGE", boots.enchantLevel * 2);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (item is Chestplate chestplate)
            {
                SetTextVisibility(txtDefense, chestplate.defense);
                SetTextVisibility(txtHealPoint, chestplate.life);
                SetTextVisibility(txtDamage, chestplate.damage);
                SetTextVisibility(txtCritChance, chestplate.critChance * 100, true);
                SetTextVisibility(txtCritDamage, chestplate.critDamage * 100, true);
                SetTextVisibility(txtKnockbackResistance, chestplate.knockbackResistance);
                SetTextVisibility(txtKnockbackPower, chestplate.knockbackPower);

                SetTextVisibility(txtDragonSkin, chestplate.dragonSkin * 100, true);
                SetTextVisibility(txtRegenRate, chestplate.regenRate);
                SetTextVisibility(txtNegativeEffectReducer, chestplate.negativeEffectReducer * 100, true);
                SetTextVisibility(txtMineralChance, chestplate.mineralChance * 100, true);
                SetTextVisibility(txtDodgeChance, chestplate.dodgeChance * 100, true);
                SetTextVisibility(txtDoubleMineralDropChance, chestplate.doubleMineralDropChance * 100, true);

                if (chestplate.armorEnchant != ARMOR_ENCHANT.NULL)
                {
                    txtEnchant.gameObject.SetActive(true);
                    StartColorTransition(chestplate.colorEnchant1, chestplate.colorEnchant2);

                    switch (chestplate.armorEnchant)
                    {
                        case ARMOR_ENCHANT.DRAGON_SKIN:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "DRAGON_SKIN", chestplate.enchantLevel * 3);
                            break;
                        case ARMOR_ENCHANT.REGEN:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "REGEN", chestplate.enchantLevel);
                            break;
                        case ARMOR_ENCHANT.FANTOM_DODGE:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "FANTOM_DODGE", chestplate.enchantLevel * 2);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (item is Helmet helmet)
            {
                SetTextVisibility(txtDefense, helmet.defense);
                SetTextVisibility(txtHealPoint, helmet.life);
                SetTextVisibility(txtDamage, helmet.damage);

                SetTextVisibility(txtDragonSkin, helmet.dragonSkin * 100, true);
                SetTextVisibility(txtRegenRate, helmet.regenRate);
                SetTextVisibility(txtNegativeEffectReducer, helmet.negativeEffectReducer * 100, true);
                SetTextVisibility(txtMineralChance, helmet.mineralChance * 100, true);
                SetTextVisibility(txtDodgeChance, helmet.dodgeChance * 100, true);
                SetTextVisibility(txtDoubleMineralDropChance, helmet.doubleMineralDropChance * 100, true);

                if (helmet.armorEnchant != ARMOR_ENCHANT.NULL)
                {
                    txtEnchant.gameObject.SetActive(true);
                    StartColorTransition(helmet.colorEnchant1, helmet.colorEnchant2);

                    switch (helmet.armorEnchant)
                    {
                        case ARMOR_ENCHANT.DRAGON_SKIN:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "DRAGON_SKIN", helmet.enchantLevel * 3);
                            break;
                        case ARMOR_ENCHANT.REGEN:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "REGEN", helmet.enchantLevel);
                            break;
                        case ARMOR_ENCHANT.FANTOM_DODGE:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "FANTOM_DODGE", helmet.enchantLevel * 2);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (item is Leggings leggings)
            {
                SetTextVisibility(txtDefense, leggings.defense);
                SetTextVisibility(txtHealPoint, leggings.life);
                SetTextVisibility(txtSpeed, leggings.speed);
                SetTextVisibility(txtKnockbackResistance, leggings.knockbackResistance);
                SetTextVisibility(txtKnockbackPower, leggings.knockbackPower);

                SetTextVisibility(txtDragonSkin, leggings.dragonSkin * 100, true);
                SetTextVisibility(txtRegenRate, leggings.regenRate);
                SetTextVisibility(txtNegativeEffectReducer, leggings.negativeEffectReducer * 100, true);
                SetTextVisibility(txtMineralChance, leggings.mineralChance * 100, true);
                SetTextVisibility(txtDodgeChance, leggings.dodgeChance * 100, true);
                SetTextVisibility(txtDoubleMineralDropChance, leggings.doubleMineralDropChance * 100, true);

                if (leggings.armorEnchant != ARMOR_ENCHANT.NULL)
                {
                    txtEnchant.gameObject.SetActive(true);
                    StartColorTransition(leggings.colorEnchant1, leggings.colorEnchant2);

                    switch (leggings.armorEnchant)
                    {
                        case ARMOR_ENCHANT.DRAGON_SKIN:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "DRAGON_SKIN", leggings.enchantLevel * 3);
                            break;
                        case ARMOR_ENCHANT.REGEN:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "REGEN", leggings.enchantLevel);
                            break;
                        case ARMOR_ENCHANT.FANTOM_DODGE:
                            txtEnchant.text = LocalizationManager.instance.GetText("effects", "FANTOM_DODGE", leggings.enchantLevel * 2);
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (item is Consumables consumable)
            {
                switch (consumable.type)
                {
                    case ConsumableType.NONE:
                        break;
                    case ConsumableType.HEALING:
                        switch (consumable.typeHealing)
                        {
                            case HealingType.NONE:
                                break;
                            case HealingType.PERCENTAGE:
                                SetTextVisibility(txtHealPoint, consumable.power);
                                break;
                            case HealingType.LIFE_POINT:
                                SetTextVisibility(txtHealPoint, (int)consumable.power);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            ResetTextFields();
        }
    }

    private void ResetTextFields()
    {
        if (txtDamage != null) txtDamage.SetActive(false);
        if (txtSpeed != null) txtSpeed.SetActive(false);
        if (txtHealPoint != null) txtHealPoint.SetActive(false);
        if (txtDefense != null) txtDefense.SetActive(false);
        if (txtCritChance != null) txtCritChance.SetActive(false);
        if (txtCritDamage != null) txtCritDamage.SetActive(false);
        if (txtKnockbackPower != null) txtKnockbackPower.SetActive(false);
        if (txtKnockbackResistance != null) txtKnockbackResistance.SetActive(false);
        if (txtDragonSkin != null) txtDragonSkin.SetActive(false);
        if (txtRegenRate != null) txtRegenRate.SetActive(false);
        if (txtNegativeEffectReducer != null) txtNegativeEffectReducer.SetActive(false);
        if (txtMineralChance != null) txtMineralChance.SetActive(false);
        if (txtDodgeChance != null) txtDodgeChance.SetActive(false);
        if (txtDoubleMineralDropChance != null) txtDoubleMineralDropChance.SetActive(false);
        if (txtVampire != null) txtVampire.SetActive(false);
        if (txtFireAttackChance != null) txtFireAttackChance.SetActive(false);
        if (txtIceAttackChance != null) txtIceAttackChance.SetActive(false);
        if (txtPoisonAttackChance != null) txtPoisonAttackChance.SetActive(false);
        if (txtDoubleSquareCoinsChances != null) txtDoubleSquareCoinsChances.SetActive(false);
        if (txtDropChance != null) txtDropChance.SetActive(false);


        if (equipButton)
            equipButton.SetActive(false);

        if(consumeButton)
            consumeButton.SetActive(false);

        if(trashButton)
            trashButton.SetActive(false);

        txtTitle.gameObject.SetActive(false);
        txtDescription.gameObject.SetActive(false);
        txtValue.SetActive(false);
        txtRarity.gameObject.SetActive(false);

        txtEnchant.gameObject.SetActive(false);
        txtLevel.gameObject.SetActive(false);
    }

    private void SetIfExists(Item item, string fieldName, GameObject txtField, bool percentage = false)
    {
        var prop = item.GetType().GetProperty(fieldName);
        if (prop != null && prop.PropertyType == typeof(float))
        {
            float value = (float)prop.GetValue(item);
            SetTextVisibility(txtField, percentage ? value * 100 : value, percentage);
        }
    }


    private void SetTextVisibility(GameObject gameObject, float value, bool percentage = false)
    {
        if (value != 0)
        {
            gameObject.SetActive(true);
            var textComponent = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = percentage ? $"{value:F0} %" : $"{value:F2}";
        }
    }

    private void SetTextVisibility(GameObject gameObject, int value)
    {
        if (value != 0)
        {
            gameObject.SetActive(true);
            var textComponent = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = $"{value}";
        }
    }




private void StartColorTransition(Color start, Color end)
    {
        color1 = start;
        color2 = end;
        transitionTimer = 0f; // Reset timer for new transition
        transitioningToColor2 = true; // Start transitioning to color2
        isTransitioning = true; // Begin transition
    }

    private void HandleColorTransition()
    {
        if (isTransitioning)
        {
            // Update the transition timer
            transitionTimer += Time.unscaledDeltaTime;
            float t = Mathf.PingPong(transitionTimer / colorTransitionTime, 1); // Calculate transition progress

            // Lerp color based on transition progress
            txtEnchant.color = Color.Lerp(transitioningToColor2 ? color1 : color2, transitioningToColor2 ? color2 : color1, t);

            // Check if the transition has completed
            if (transitionTimer >= colorTransitionTime)
            {
                transitionTimer = 0f; // Reset timer
                transitioningToColor2 = !transitioningToColor2; // Toggle transition direction
            }
        }
    }
}
