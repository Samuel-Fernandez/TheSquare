using UnityEngine;

[CreateAssetMenu(fileName = "New Helmet", menuName = "Items/Helmet")]
public class Helmet : Item
{
    public int defense;
    public int life;
    public int damage;
    public ARMOR_ENCHANT armorEnchant;
    public int enchantLevel;

    public int baseDefense;
    public int baseLife;
    public int baseDamage;

    // A compléter
    public float vampire = 0;                               // FAIT
    public float dragonSkin = 0;                            // FAIT
    public float fireAttackChance = 0;                      // FAIT
    public float iceAttackChance = 0;                       // FAIT
    public float poisonAttackChance = 0;                    // FAIT
    public float regenRate = 0;                        // nb de coeur regénéré toutes les 60 secondes FAIT
    public float doubleSquareCoinsChances = 0;         // % de chance de doubler en tuant FAIT
    public float negativeEffectReducer = 0;            // % de dégât / de temps en moins FAIT 
    public float mineralChance = 0;                    // Chance que de meilleurs minéraux apparaissent FAIT
    public float dodgeChance = 0;                      // Chance d'esquiver totalement un dégât FAIT
    public float doubleMineralDropChance = 0;          // Chance de doubler un minéral FAIT
    public float dropChance = 0;                       // Chance de tomber sur un meilleur item lors d'un drop FAIT


    // 03-XXXX-AA-LL-DD-CC-CD-00-00 : Helmet-ID-Defense-Life-Damage-CritC-CritD
    public void GenerateStats()
    {
        GetEnchant();
        UpdateLevel();

        float levelMultiplier = 1 + ((level - 1) * 0.2f);

        float defenseBonus = int.Parse(this.itemId.Substring(6, 2)) / 100f;
        float lifeBonus = int.Parse(this.itemId.Substring(8, 2)) / 100f;
        float damageBonus = int.Parse(this.itemId.Substring(10, 2)) / 100f;

        defense = (int)(baseDefense * (1 + defenseBonus) * levelMultiplier);
        life = (int)(baseLife * (1 + lifeBonus) * levelMultiplier);
        damage = (int)(baseDamage * (1 + damageBonus) * levelMultiplier);

        Mathf.Exp(2);

        if (baseDefense != 0)
            defense += (level - 1);

        if (baseLife != 0)
            life += (level - 1) * 4;

        if (baseDamage != 0)
            damage += (level - 1);

        // ---- Correction : toujours multiple de 4
        life = Mathf.RoundToInt(life / 4f) * 4;

        // ---- Nouveau calcul de value avec pow et poids ----
        float total = 0;

        total += Mathf.Pow(damage * 5, 1.3f);
        total += Mathf.Pow(life * 200, 1.3f);    // HP
        total += Mathf.Pow(vampire * 500, 1.3f);
        total += Mathf.Pow(regenRate * 15, 1.3f);
        total += Mathf.Pow(doubleSquareCoinsChances * 250, 1.3f);
        total += Mathf.Pow(fireAttackChance * 200, 1.3f);
        total += Mathf.Pow(poisonAttackChance * 200, 1.3f);
        total += Mathf.Pow(iceAttackChance * 200, 1.3f);
        total += Mathf.Pow(dragonSkin * 150, 1.3f);
        total += Mathf.Pow(dodgeChance * 150, 1.3f);
        total += Mathf.Pow(dropChance * 120, 1.3f);
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
