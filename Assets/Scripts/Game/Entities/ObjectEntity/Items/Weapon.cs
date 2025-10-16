using UnityEngine;


[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class Weapon : Item
{
    public int damage;
    public float knockbackPower;
    public float critChance;
    public float critDamage;
    public WEAPON_ENCHANT enchant;
    public int enchantLevel;

    public int baseDamage;
    public float baseCritChance;
    public float baseCritDamage;
    public float baseKnockbackPower;

    // A compléter
    public float vampire = 0;                               // FAIT
    public float fireAttackChance = 0;                      // FAIT
    public float iceAttackChance = 0;                       // FAIT
    public float poisonAttackChance = 0;                    // FAIT
    public float doubleSquareCoinsChances = 0;         // % de chance de doubler en tuant FAIT
    public float dropChance = 0;                       // Chance de tomber sur un meilleur item lors d'un drop FAIT


    // 02-XXXX-DD-KK-CC-CD-00-00-00-00 : Weapon-ID-Damage-KnockbackP-CritC-CritD
    public void GenerateStats()
    {
        if (itemId.Length < 16)
        {
            Debug.LogError($"Invalid itemId format: {itemId}");
            return;
        }
        GetEnchant();
        UpdateLevel();

        float levelMultiplier = 1 + ((level - 1) * .2f);

        // Parsing des bonus de l'itemId
        float damageBonus = int.Parse(this.itemId.Substring(6, 2)) / 100f;
        float knockbackBonus = int.Parse(this.itemId.Substring(8, 2)) / 100f;
        float critChanceBonus = int.Parse(this.itemId.Substring(10, 2)) / 100f;
        float critDamageBonus = int.Parse(this.itemId.Substring(12, 2)) / 100f;

        // Calcul de base avec bonus et multiplicateur de niveau
        damage = (int)(baseDamage * (1 + damageBonus) * levelMultiplier);
        knockbackPower = baseKnockbackPower * (1 + knockbackBonus) * levelMultiplier;
        critChance = baseCritChance * (1 + critChanceBonus) * levelMultiplier;
        critDamage = baseCritDamage * (1 + critDamageBonus) * levelMultiplier;

        // Bonus par niveau uniquement si la valeur de base est non nulle
        if (baseDamage != 0)
            damage += (level - 1); // +1 par niveau au-delà du premier

        if (baseKnockbackPower != 0)
            knockbackPower += (level - 1) * 0.01f;

        if (baseCritChance != 0)
            critChance += (level - 1) * 0.01f;

        if (baseCritDamage != 0)
            critDamage += (level - 1) * 0.01f;


        // ---- Nouveau calcul de value avec pow et poids ----
        float total = 0;

        total += Mathf.Pow(damage * 5, 1.3f);
        total += Mathf.Pow(baseKnockbackPower * 100, 1.3f); // KBP (si c’est une stat séparée, ajuste)
        total += Mathf.Pow(critChance * 200, 1.3f); // CC (si tu as une variable critChance)
        total += Mathf.Pow(critDamage * 200, 1.3f); // CD (si tu as une variable critDamage)
        total += Mathf.Pow(vampire * 500, 1.3f);
        total += Mathf.Pow(doubleSquareCoinsChances * 250, 1.3f);
        total += Mathf.Pow(fireAttackChance * 200, 1.3f);
        total += Mathf.Pow(poisonAttackChance * 200, 1.3f);
        total += Mathf.Pow(iceAttackChance * 200, 1.3f);

        value = (int)Mathf.Round(total);
    }


    public int GetBaseDamage()
    {
        return (int)(baseDamage * (1 + int.Parse(this.itemId.Substring(6, 2)) / 100f));
    }

    public float GetBaseKnockbackPower()
    {
        return baseKnockbackPower * (1 + int.Parse(this.itemId.Substring(8, 2)) / 100f);
    }

    public float GetBaseCritChance()
    {
        return baseCritChance * (1 + int.Parse(this.itemId.Substring(10, 2)) / 100f);
    }

    public float GetBaseCritDamage()
    {
        return baseCritDamage * (1 + int.Parse(this.itemId.Substring(12, 2)) / 100f);
    }


    void GetEnchant()
    {
        string enchantDigit = this.itemId.Substring(this.itemId.Length - 6, 2);

        switch (enchantDigit)
        {
            case "01":
                enchant = WEAPON_ENCHANT.POISON;
                this.colorEnchant1 = new Color(249f / 255f, 174f / 255f, 252f / 255f); // Normalized
                this.colorEnchant2 = new Color(101f / 255f, 9f / 255f, 105f / 255f);  // Normalized
                break;
            case "02":
                enchant = WEAPON_ENCHANT.ICE;
                this.colorEnchant1 = new Color(179f / 255f, 255f / 255f, 251f / 255f); // Normalized
                this.colorEnchant2 = new Color(35f / 255f, 217f / 255f, 207f / 255f);  // Normalized
                break;
            case "03":
                enchant = WEAPON_ENCHANT.FLAME;
                this.colorEnchant1 = new Color(255f / 255f, 75f / 255f, 69f / 255f); // Normalized
                this.colorEnchant2 = new Color(163f / 255f, 161f / 255f, 29f / 255f);  // Normalized
                break;
            case "04":
                enchant = WEAPON_ENCHANT.VAMPIRE;
                this.colorEnchant1 = new Color(86f / 255f, 56f / 255f, 105f / 255f); // Normalized
                this.colorEnchant2 = new Color(59f / 255f, 4f / 255f, 74f / 255f);  // Normalized
                break;
            default:
                enchant = WEAPON_ENCHANT.NULL;
                this.colorEnchant1 = Color.white; // Default color
                this.colorEnchant2 = Color.white; // Default color
                break;
        }

        this.enchantLevel = int.Parse(this.itemId.Substring(this.itemId.Length - 4, 1));

    }




}
