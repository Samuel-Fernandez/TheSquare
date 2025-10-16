using UnityEngine;

[CreateAssetMenu(fileName = "New Boots", menuName = "Items/Boots")]
public class Boots : Item
{
    public int defense;
    public int life;
    public float speed;
    public ARMOR_ENCHANT armorEnchant;
    public int enchantLevel;

    public int baseDefense;
    public int baseLife;
    public float baseSpeed;

    // A compléter
    public float dragonSkin = 0;                            // FAIT
    public float regenRate = 0;                        // nb de coeur regénéré toutes les 60 secondes FAIT
    public float negativeEffectReducer = 0;            // % de dégât / de temps en moins FAIT 
    public float mineralChance = 0;                    // Chance que de meilleurs minéraux apparaissent FAIT
    public float dodgeChance = 0;                      // Chance d'esquiver totalement un dégât FAIT
    public float doubleMineralDropChance = 0;          // Chance de doubler un minéral FAIT


    // 06-XXXX-AA-LL-XX-XX-SS-00-00 : Boots-ID-Defense-Life-CritC-CritD-Speed
    public void GenerateStats()
    {
        if (itemId.Length < 16)
        {
            Debug.LogError($"Invalid itemId format: {itemId}");
            return;
        }

        UpdateLevel();
        GetEnchant();

        float levelMultiplier = 1 + ((level - 1) * 0.2f);

        float defenseBonus = int.Parse(this.itemId.Substring(6, 2)) / 100f;
        float lifeBonus = int.Parse(this.itemId.Substring(8, 2)) / 100f;
        float speedBonus = int.Parse(this.itemId.Substring(14, 2)) / 100f;

        defense = (int)(baseDefense * (1 + defenseBonus) * levelMultiplier);
        life = (int)(baseLife * (1 + lifeBonus) * levelMultiplier);
        speed = baseSpeed * speedBonus * levelMultiplier;

        if (baseDefense != 0)
            defense += (level - 1); // +1 par niveau au-delà du premier

        if (baseLife != 0)
            life += (level - 1) * 4; // +4 par niveau au-delà du premier

        if (baseSpeed != 0)
            speed += (level - 1) * 0.01f;

        // ---- Correction : toujours multiple de 4
        life = Mathf.RoundToInt(life / 4f) * 4;

        // ---- Nouveau calcul de value avec pow et poids ----
        float total = 0;

        total += Mathf.Pow(life * 200, 1.3f);
        total += Mathf.Pow(speed * 300, 1.3f);
        total += Mathf.Pow(regenRate * 15, 1.3f);
        total += Mathf.Pow(dragonSkin * 150, 1.3f);
        total += Mathf.Pow(dodgeChance * 150, 1.3f);
        total += Mathf.Pow(doubleMineralDropChance * 120, 1.3f);
        total += Mathf.Pow(mineralChance * 120, 1.3f);
        total += Mathf.Pow(negativeEffectReducer * 100, 1.3f);

        value = (int)Mathf.Round(total);
    }


    public float GetBaseSpeed()
    {
        return baseSpeed * int.Parse(this.itemId.Substring(14, 2)) / 100f;
    }

    public int GetBaseDefense()
    {
        return (int)(baseDefense * (1 + int.Parse(this.itemId.Substring(6, 2)) / 100f));
    }

    public int GetBaseLife()
    {
        return (int)(baseLife * (1 + int.Parse(this.itemId.Substring(8, 2)) / 100f));
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
