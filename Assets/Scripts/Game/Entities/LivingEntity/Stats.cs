using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public enum EntityType
{
    Player,
    Monster,
    Boss,
    PNJ
}

public class Stats : MonoBehaviour
{
    public EntityType entityType;
    public int health;
    public int strength;
    public int defense;
    public int luck;
    public float speed;
    public float critDamage;
    public float critChance;
    public float knockbackResistance;
    public float knockbackPower;
    public int money;

    public bool isVulnerable;
    public bool canMove;
    public bool isDying;
    public bool isBowShooting;

    private List<Item> previousEquippedItems;
    private PlayerController playerController;

    public bool doingAttack = false;

    // Interromp l'attaque à l'épée du joueur
    public bool blockPlayerAttack;

    private void Start()
    {
        if (entityType == EntityType.Player)
        {
            previousEquippedItems = new List<Item>(new Item[Equipement.instance.equippedSlots.Count]);
            playerController = GetComponent<PlayerController>();
            UpdateStats();
        }
    }

    bool tempTime = true;

    public void SetVulnerability(bool isVulnerable)
    {
        this.isVulnerable = isVulnerable;
    }

    private void Update()
    {
        if (entityType == EntityType.Player && (HasEquipmentChanged() || PlayerLevels.instance.lvlChanged || AnvilUpgradeManager.instance.itemUpgraded))
        {
            UpdateStats();
        }

        if(tempTime != MeteoManager.instance.time)
        {
            tempTime = MeteoManager.instance.time;
            canMove = MeteoManager.instance.time;
        }

        if (isDying)
            canMove = false;
    }

    private bool HasEquipmentChanged()
    {
        bool changed = false;
        for (int i = 0; i < Equipement.instance.equippedSlots.Count; i++)
        {
            Item currentItem = Equipement.instance.equippedSlots[i].GetComponent<EquipementSlot>().actualItem;
            if (currentItem != previousEquippedItems[i])
            {
                changed = true;
                previousEquippedItems[i] = currentItem;
            }
        }
        return changed;
    }

    private void UpdateStats()
    {
        if (PlayerLevels.instance.lvlChanged)
            PlayerLevels.instance.lvlChanged = false;

        if(AnvilUpgradeManager.instance.itemUpgraded)
            AnvilUpgradeManager.instance.itemUpgraded = false;

        // Get the current life manager
        LifeManager lifeManager = GetComponent<LifeManager>();

        // Calculate the current health percentage
        float currentHealthPercentage = (float)lifeManager.life / health;

        // Reset player stats to base values here if needed
        ResetStats();

        foreach (var equippedSlot in Equipement.instance.equippedSlots)
        {
            EquipementSlot slot = equippedSlot.GetComponent<EquipementSlot>();
            Item item = slot.actualItem;

            if (item != null)
            {
                if (item is Helmet helmet)
                {
                    AddStats(helmet);
                }
                else if (item is Chestplate chestplate)
                {
                    AddStats(chestplate);
                }
                else if (item is Leggings leggings)
                {
                    AddStats(leggings);
                }
                else if (item is Boots boots)
                {
                    AddStats(boots);
                }
                else if (item is Weapon weapon)
                {
                    AddStats(weapon);
                }
            }
        }

        // Update the life value based on the new health
        lifeManager.life = Mathf.RoundToInt(currentHealthPercentage * health);

        // Notify player controller of speed change
        if(playerController)
            playerController.UpdateSpeed(speed);
    }

    private void ResetStats()
    {
        // Reset all player stats to their base values
        health = PlayerLevels.instance.lvlHP * 4 + 8 + PlayerManager.instance.bonusHP;
        strength = PlayerLevels.instance.lvlSTR + PlayerManager.instance.bonusSTR;
        defense = 0;
        luck = PlayerLevels.instance.lvlLuck + PlayerManager.instance.bonusLUCK;
        speed = 1.35f + PlayerManager.instance.bonusSPE;
        critDamage = PlayerLevels.instance.lvlSTR / 20 + PlayerManager.instance.bonusCRITD;
        critChance = PlayerLevels.instance.lvlLuck / 100 + PlayerManager.instance.bonusCRITC;
        knockbackResistance = 0 + PlayerManager.instance.bonusKBR;
        knockbackPower = 5f + PlayerManager.instance.bonusKBP;
        // Reset other stats as needed
    }

    private void AddStats(Helmet helmet)
    {
        defense += helmet.defense;
        health += helmet.life;
        strength += helmet.damage;

    }

    private void AddStats(Chestplate chestplate)
    {
        defense += chestplate.defense;
        health += chestplate.life;
        strength += chestplate.damage;
        critChance += chestplate.critChance;
        critDamage += chestplate.critDamage;
        knockbackResistance += chestplate.knockbackResistance;
        knockbackPower += chestplate.knockbackPower;
    }

    private void AddStats(Leggings leggings)
    {
        defense += leggings.defense;
        health += leggings.life;
        speed += leggings.speed;
        critChance += leggings.knockbackResistance;
        critDamage += leggings.knockbackPower;
    }

    private void AddStats(Boots boots)
    {
        defense += boots.defense;
        health += boots.life;
        speed += boots.speed;

    }

    private void AddStats(Weapon weapon)
    {
        strength += weapon.damage;
        knockbackPower += weapon.knockbackPower;
        critChance += weapon.critChance;
        critDamage += weapon.critDamage;
    }
}
