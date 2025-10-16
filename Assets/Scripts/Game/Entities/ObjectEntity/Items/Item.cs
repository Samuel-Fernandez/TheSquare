using Grpc.Core.Logging;
using System.Globalization;
using Unity.Collections;
using UnityEngine;

public enum Rarity
{
    COMMON, // MAX LEVEL 3
    UNCOMMON, // MAX LEVEL 5
    RARE, // MAX LEVEL 7
    EPIC, // MAX LEVEL 9
    LEGENDARY // MAX LEVEL 10
}

public enum WEAPON_ENCHANT
{
    NULL,
    POISON,
    ICE,
    FLAME,
    VAMPIRE,
}

public enum ARMOR_ENCHANT
{
    NULL,
    REGEN,
    DRAGON_SKIN,
    FANTOM_DODGE,
}

// ITEMS ID 20 digits
// 00-XXXX-00000000000000-EN-L-LLL            : Money-ID-X-ENCHANT-ENCHANT_LEVEL-LEVEL
// 01-XXXX-00000000000000-EN-L-LLL            : Consumable-ID-X-ENCHANT-ENCHANT_LEVEL-LEVEL
// 02-XXXX-DD-KK-CC-CD-00-00-00-00-EN-L-LLL   : Weapon-ID-Damage-KnockbackP-CritC-CritD-X-ENCHANT-ENCHANT_LEVEL-LEVEL
// 03-XXXX-AA-LL-DD-CC-CD-00-00-00-EN-L-LLL   : Helmet-ID-Defense-Life-Damage-CritC-CritD-X-ENCHANT-ENCHANT_LEVEL-LEVEL
// 04-XXXX-AA-LL-DD-CC-CC-KK-KK-00-EN-L-LLL   : Chestplate-ID-Defense-Life-Damage-CritC-CritD-KnockbackP-KnockbackR-X-ENCHANT-ENCHANT_LEVEL-LEVEL
// 05-XXXX-AA-LL-CC-CC-SS-00-00-00-EN-L-LLL   : Leggings-ID-Defense-Life-CritC-CritD-Speed-X-ENCHANT-ENCHANT_LEVEL-LEVEL
// 06-XXXX-AA-LL-CC-CC-SS-00-00-00-EN-L-LLL   : Boots-ID-Defense-Life-CritC-CritD-Speed-X-ENCHANT-ENCHANT_LEVEL-LEVEL
// 07-0000                                    : SpecialItems

[System.Serializable]
public class Item : ScriptableObject
{
    public string itemId;
    public string itemName;
    public string description;
    public Sprite sprite;
    public Color colorEnchant1;
    public Color colorEnchant2;
    public Rarity rarity;
    public int value;
    public int level;

    public void GenerateID()
    {
        string stats1 = Randomizer();
        string stats2 = Randomizer();
        string stats3 = Randomizer();
        string stats4 = Randomizer();
        string stats5 = Randomizer();
        string stats6 = Randomizer();
        string stats7 = Randomizer();
        string stats8 = EnchantRandomizer();

        itemId += stats1;
        itemId += stats2;
        itemId += stats3;
        itemId += stats4;
        itemId += stats5;
        itemId += stats6;
        itemId += stats7;
        itemId += stats8;
        itemId += "000"; // Niveau de l'arme

        this.level = 1;
    }

    // Weapon :
    // Poison           : 5% de chances poison
    // Glace            : 5% de chances glace
    // Flammes          : 5% de chances flammes
    // Vol vie          : 10 % de chances de voler 20 % des dégâts donnés
    // 
    // Armor            : 
    // Régénération     : Régénère 10 % de la vie toutes les 20 secondes
    // Peau de dragon   : 10 % de resistance aux coups
    // Esquive fantôme  : 10 % de chance de supprimer des dégâts
    // 


    // LEVELS
    // 1 : 0-5
    // 2 : 6-15
    // 3 : 15-30
    // 4 : 31-50
    // 5 : 51-100
    // 6 : 101-200
    // 7 : 201-350
    // 8 : 351-500
    // 9 : 501-999
    // 10: >1000
    public void UpdateLevel()
    {
        string levelID = itemId.Substring(itemId.Length - 3);
        int level = int.Parse(levelID);

        if (level <= 5)
        {
            this.level = 1;
        }
        else if (level > 5 && level <= 15)
        {
            this.level = 2;
        }
        else if (level > 15 && level <= 30)
        {
            this.level = 3;
        }
        else if (level > 30 && level <= 50)
        {
            this.level = 4;
        }
        else if (level > 50 && level <= 100)
        {
            this.level = 5;
        }
        else if (level > 100 && level <= 200)
        {
            this.level = 6;
        }
        else if (level > 200 && level <= 350)
        {
            this.level = 7;
        }
        else if (level > 350 && level <= 500)
        {
            this.level = 8;
        }
        else if (level > 501 && level <= 999)
        {
            this.level = 9;
        }
        else if (level == 1000)
        {
            this.level = 10;
        }

    }

    public string EnchantRandomizer()
    {
        // CHANGER LE 0 PAR 900
        if(Random.Range(0 + PlayerManager.instance.player.GetComponent<Stats>().luck * 2, 1000) >= 900)
        {
            int randomLevel = 1;
            int chance = Random.Range(1 + (PlayerManager.instance.player.GetComponent<Stats>().luck), 1000);

            if(chance > 500 && chance < 850)
            {
                randomLevel = 2;
            }
            else if (chance >= 850 && chance < 900)
            {
                randomLevel = 3;
            }
            else if (chance >= 900 && chance < 990)
            {
                randomLevel = 4;
            }
            else if (chance >= 990)
            {
                randomLevel = 5;
            }


            return Random.Range(1, 5).ToString("00") + randomLevel.ToString();
        }
        else
        {
            return "000";
        }
    }

    public string Randomizer()
    {
        int luck = PlayerManager.instance.player.GetComponent<Stats>().luck;

        // Ajustement de la chance pour une transformation logistique inverse
        float adjustedLuck = Mathf.Exp((luck + (PlayerManager.instance.itemsAddChance / 2)) / 10f);

        // Générer un nombre aléatoire entre 0 et 1
        float randomValue = Random.value;

        // Appliquer la transformation logistique inverse
        float inverseLogistic = Mathf.Log(adjustedLuck * (randomValue / (1 - randomValue)));

        // Convertir la valeur inversée en un nombre entre 0 et 99
        int biasedNumber = Mathf.FloorToInt(Mathf.Clamp01(inverseLogistic / 6f) * 99);

        // Si le nombre est 0, on le remplace par 1 pour éviter des valeurs trop basses
        if (biasedNumber == 0)
        {
            biasedNumber = 1;
        }

        return biasedNumber.ToString("00"); // Retourne le nombre avec deux chiffres
    }

    public string GetID()
    {
        return itemId.Substring(0, 6);
    }

    public int GetMaxLevel()
    {
        switch (rarity)
        {
            case Rarity.COMMON:
                return 3;
            case Rarity.UNCOMMON:
                return 5;
            case Rarity.RARE:
                return 7;
            case Rarity.EPIC:
                return 9;
            case Rarity.LEGENDARY:
                return 10;
            default:
                return 0;
        }
    }




}


