using UnityEngine;


[CreateAssetMenu(fileName = "New Chestplate", menuName = "Items/Chestplate")]
public class Chestplate : Item
{
    public int defense;
    public int life;
    public int damage;
    public float critChance;
    public float critDamage;
    public float knockbackResistance;
    public float knockbackPower;
    public ARMOR_ENCHANT armorEnchant;
    public int enchantLevel;

    public int baseDefense;
    public int baseLife;
    public int baseDamage;
    public float baseCritChance;
    public float baseCritDamage;
    public float baseKnockbackResistance;
    public float baseKnockbackPower;

    // A compléter
    public float dragonSkin = 0;                            // FAIT
    public float regenRate = 0;                        // nb de coeur regénéré toutes les 60 secondes FAIT
    public float negativeEffectReducer = 0;            // % de dégât / de temps en moins FAIT 
    public float mineralChance = 0;                    // Chance que de meilleurs minéraux apparaissent FAIT
    public float dodgeChance = 0;                      // Chance d'esquiver totalement un dégât FAIT
    public float doubleMineralDropChance = 0;          // Chance de doubler un minéral FAIT


    // 04-XXXX-AA-LL-DD-CC-CC-KK-KK : Chestplate-ID-Defense-Life-Damage-CritC-CritD-KnockbackP-KnockbackR
    public void GenerateStats()
    {
        GetEnchant();
        UpdateLevel();

        float levelMultiplier = 1 + ((level - 1) * 0.2f);

        float defenseBonus = int.Parse(this.itemId.Substring(6, 2)) / 100f;
        float lifeBonus = int.Parse(this.itemId.Substring(8, 2)) / 100f;
        float damageBonus = int.Parse(this.itemId.Substring(10, 2)) / 100f;
        float critDamageBonus = int.Parse(this.itemId.Substring(12, 2)) / 100f;

        defense = (int)(baseDefense * (1 + defenseBonus) * levelMultiplier);
        life = (int)(baseLife * (1 + lifeBonus) * levelMultiplier);
        damage = (int)(baseDamage * (1 + damageBonus) * levelMultiplier);
        critChance = baseCritChance * (1 + damageBonus) * levelMultiplier;
        critDamage = baseCritDamage * (1 + critDamageBonus) * levelMultiplier;
        knockbackPower = baseKnockbackPower * (1 + lifeBonus) * levelMultiplier;
        knockbackResistance = baseKnockbackResistance * (1 + lifeBonus) * levelMultiplier;

        if (baseDefense != 0)
            defense += (level - 1);

        if (baseLife != 0)
            life += (level - 1) * 4;

        if (baseDamage != 0)
            damage += (level - 1);

        if (baseCritChance != 0)
            critChance += (level - 1) * 0.01f;

        if (baseCritDamage != 0)
            critDamage += (level - 1) * 0.01f;

        if (baseKnockbackPower != 0)
            knockbackPower += (level - 1) * 0.01f;

        if (baseKnockbackResistance != 0)
            knockbackResistance += (level - 1) * 0.01f;

        // ---- Correction : toujours multiple de 4
        life = Mathf.RoundToInt(life / 4f) * 4;

        // ---- Nouveau calcul de value avec pow et poids ----
        float total = 0;

        total += Mathf.Pow(damage * 5, 1.3f);
        total += Mathf.Pow(baseKnockbackResistance * 100, 1.3f); // KBR
        total += Mathf.Pow(baseKnockbackPower * 100, 1.3f); // KBP (si c’est une stat séparée, ajuste)
        total += Mathf.Pow(life * 200, 1.3f);    // HP
        total += Mathf.Pow(critChance * 200, 1.3f); // CC (si tu as une variable critChance)
        total += Mathf.Pow(critDamage * 200, 1.3f); // CD (si tu as une variable critDamage)
        total += Mathf.Pow(regenRate * 15, 1.3f);
        total += Mathf.Pow(dragonSkin * 150, 1.3f);
        total += Mathf.Pow(dodgeChance * 150, 1.3f);
        total += Mathf.Pow(doubleMineralDropChance * 120, 1.3f);
        total += Mathf.Pow(mineralChance * 120, 1.3f);
        total += Mathf.Pow(negativeEffectReducer * 100, 1.3f);

        value = (int)Mathf.Round(total);
    }


    public int GetBaseDefense()
    {
        return (int)(baseDefense * (1 + int.Parse(this.itemId.Substring(6, 2)) / 100f));
    }

    public int GetBaseLife()
    {
        return (int)(baseLife * (1 + int.Parse(this.itemId.Substring(8, 2)) / 100f));
    }

    public int GetBaseDamage()
    {
        return (int)(baseDamage * (1 + int.Parse(this.itemId.Substring(10, 2)) / 100f));
    }

    public float GetBaseCritChance()
    {
        return baseCritChance * (1 + int.Parse(this.itemId.Substring(10, 2)) / 100f);
    }

    public float GetBaseCritDamage()
    {
        return baseCritDamage * (1 + int.Parse(this.itemId.Substring(12, 2)) / 100f);
    }

    public float GetBaseKnockbackPower()
    {
        return (baseKnockbackPower * (1 + int.Parse(this.itemId.Substring(8, 2)) / 100f));
    }

    public float GetBaseKnockbackResistance()
    {
        return (baseKnockbackResistance * (1 + int.Parse(this.itemId.Substring(8, 2)) / 100f));
    }


    void GetEnchant()
    {
        string enchantDigit = this.itemId.Substring(this.itemId.Length - 6, 2);

        switch (enchantDigit)
        {
            case "01":
                armorEnchant = ARMOR_ENCHANT.REGEN;
                colorEnchant1 = new Color(37f / 255f, 153f / 255f, 37f / 255f); // Normalized
                colorEnchant2 = new Color(3f / 255f, 252f / 255f, 3f / 255f);  // Normalized
                break;
            case "02":
                armorEnchant = ARMOR_ENCHANT.DRAGON_SKIN;
                colorEnchant1 = new Color(128f / 255f, 46f / 255f, 133f / 255f); // Normalized
                colorEnchant2 = new Color(206f / 255f, 30f / 255f, 217f / 255f);  // Normalized
                break;
            case "03":
                armorEnchant = ARMOR_ENCHANT.FANTOM_DODGE;
                colorEnchant1 = new Color(135f / 255f, 135f / 255f, 135f / 255f, 1f); // Normalized
                colorEnchant2 = new Color(135f / 255f, 135f / 255f, 135f / 255f, 0.5f); // Normalized
                break;
            case "04":
                // À définir selon le besoin
                break;
            default:
                armorEnchant = ARMOR_ENCHANT.NULL;
                colorEnchant1 = Color.white; // Default color
                colorEnchant2 = Color.white; // Default color
                break;
        }

        this.enchantLevel = int.Parse(this.itemId.Substring(this.itemId.Length - 4, 1));

    }




}
