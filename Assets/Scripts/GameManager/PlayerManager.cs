using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;
using static UnityEditor.Progress;

public class PlayerManager : MonoBehaviour
{

    public PlayerInputActions playerInputActions;

    public GameObject player;
    public static PlayerManager instance;
    public ItemDatabase database;
    public List<SpecialItems> runtimeSpecialItems = new List<SpecialItems>();
    public bool cantDie;
    public SpecialAttackDataBase attackDatabase;
    public List<SpecialAttack> runtimeSpecialAttacks;

    public GameObject lifeUI;

    public bool isDogingTime = false;

    

    void Start()
    {
        database.InitializeDataBase();
        InitializeSpecialItems();
        InitializeSpecialAttacks(); // Initialisation des attaques spéciales runtime
    }

    public void TeleportPlayer(float x, float y)
    {
        player.transform.position = new Vector3(x, y, 0);
    }

    public void DodgeTime()
    {
        StartCoroutine(DodgeTimeRoutine());
    }

    IEnumerator DodgeTimeRoutine()
    {
        if(!player.GetComponent<Stats>().isDying)
        {
            isDogingTime = true;
            Time.timeScale = 0.25f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Pour éviter les bugs physiques

            GetComponent<SoundContainer>().PlaySound("SlowMotion", 1);

            float zoomDuration = 0.25f;
            CameraManager.instance.ZoomCamera(2f, zoomDuration); // Assure-toi que cette méthode utilise Time.unscaledDeltaTime
            CameraManager.instance.SetChromaticAberrationEffect(2f, zoomDuration); // Assure-toi que cette méthode utilise Time.unscaledDeltaTime
            CameraManager.instance.SetVignetteEffect(2f, 2f, zoomDuration); // Assure-toi que cette méthode utilise Time.unscaledDeltaTime

            yield return new WaitForSecondsRealtime(1f); // Pas ralenti

            CameraManager.instance.DezoomCamera(4f, zoomDuration);
            CameraManager.instance.SetChromaticAberrationEffect(0f, zoomDuration); // Assure-toi que cette méthode utilise Time.unscaledDeltaTime
            CameraManager.instance.SetVignetteEffect(0f, 2f, zoomDuration); // Assure-toi que cette méthode utilise Time.unscaledDeltaTime


            yield return new WaitForSecondsRealtime(zoomDuration); // attendre la fin du dézoom si nécessaire

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f; // Rétablir le fixedDeltaTime par défaut
            isDogingTime = false;
        }
        else
        {
            CameraManager.instance.ResetCameraZoom();
        }
        
    }


    public void UnlockSpecialAttack(string name)
    {
        foreach (SpecialAttack attack in runtimeSpecialAttacks)
        {
            if(attack.attackName == name)
            {
                attack.isAvailable = true;
            }
        }
    }

    // Initialisation des attaques spéciales
    // Créer des copies des ScriptableObjects pour l'exécution
    void InitializeSpecialAttacks()
    {
        runtimeSpecialAttacks = new List<SpecialAttack>();

        foreach (SpecialAttack attack in attackDatabase.GetAllSpecialAttacks()) // Récupère toutes les attaques spéciales depuis la base
        {
            SpecialAttack clonedAttack = ScriptableObjectUtility.Clone(attack); // Crée une copie de l'attaque spéciale
            runtimeSpecialAttacks.Add(clonedAttack);
        }
    }

    // Récupère l'attaque spéciale en fonction du nom
    public SpecialAttack GetSpecialAttack(string attackName)
    {
        foreach (var attack in runtimeSpecialAttacks)
        {
            if (attack.attackName == attackName)
                return attack;
        }
        return null;
    }

    public void SetPlayerVulnerability(bool isVulnerable)
    {
        player.GetComponent<Stats>().isVulnerable = isVulnerable;
    }

    public void CantDie(bool cantDie)
    {
        this.cantDie = cantDie;
    }

    // Créer des copies des ScriptableObjects pour l'exécution
    void InitializeSpecialItems()
    {
        runtimeSpecialItems = new List<SpecialItems>();

        foreach (SpecialItems item in database.GetAllItemsOfType("07"))
        {
            SpecialItems clonedItem = ScriptableObjectUtility.Clone(item);
            clonedItem.sprite = item.sprite;
            runtimeSpecialItems.Add(clonedItem);
        }
    }


    public SpecialItems GetSpecialItem(SpecialItemType itemType)
    {
        foreach (SpecialItems item in runtimeSpecialItems)
        {
            if (item.type == itemType)
            {
                return item;
            }
        }

        // Si aucun item ne correspond, retourne null
        return null;
    }

    public SpecialItems GetSpecialItem(string itemId)
    {
        foreach (SpecialItems item in runtimeSpecialItems)
        {
            if (item.itemId == itemId)
            {
                return item;
            }
        }
        return null;
    }


    /*// SpecialItems
    public int nbArrow;
    public int nbIron;
    public int nbSilver;
    public int nbDiamond;
    public int nbAntiMatter;
    public int nbSquareBlock;
    // A sauvegarder
    public int nbWood;
    public int nbStone;*/

    // BONUSES
    public float regenRate = 0;                        // nb de coeur regénéré toutes les 60 secondes FAIT
    public float doubleSquareCoinsChances = 0;         // % de chance de doubler en tuant FAIT
    public float negativeEffectReducer = 0;            // % de dégât / de temps en moins FAIT 
    public float itemsAddChance = 0;                   // Amélioration statistique des items FAIT
    public float mineralChance = 0;                    // Chance que de meilleurs minéraux apparaissent FAIT
    public float dodgeChance = 0;                      // Chance d'esquiver totalement un dégât FAIT
    public float doubleMineralDropChance = 0;          // Chance de doubler un minéral FAIT
    public float dropChance = 0;                       // Chance de tomber sur un meilleur item lors d'un drop FAIT
    public float pickaxeSpeed = 0;                     // Réduction du temps de la pioche FAIT
    public float bowSpeed = 0;                         // Réduction du temps de l'arc FAIT
    public float shieldKnockback = 0;                  // Knockback ajouté au bouclier FAIT
    public int bonusHP = 0;                             // FAIT
    public int bonusSTR = 0;                             // FAIT
    public float bonusSPE = 0;                             // FAIT
    public float bonusKBP = 0;                             // FAIT
    public float bonusKBR = 0;                             // FAIT
    public float bonusCRITD = 0;                             // FAIT
    public float bonusCRITC = 0;                             // FAIT
    public int bonusLUCK = 0;                             // FAIT
    public float vampire = 0;                               // FAIT
    public float dragonSkin = 0;                            // FAIT
    public float fireAttackChance = 0;                      // FAIT
    public float iceAttackChance = 0;                       // FAIT
    public float poisonAttackChance = 0;                    // FAIT

    public List<Skill> allSkills;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }

    public void TogglePlayer(float delay)
    {
        StartCoroutine(TogglePlayerCoroutine(delay));
    }

    private IEnumerator TogglePlayerCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        player.SetActive(!player.activeSelf);
        lifeUI.SetActive(player.activeSelf);

    }

    private void Update()
    {
        UpdateBonuses();
    }

    public void ReassignItemsSprites()
    {
        foreach (var item in runtimeSpecialItems)
        {
            Item originalItem = database.GetItemByID(item.itemId);
            if (originalItem != null)
            {
                item.sprite = originalItem.sprite;
            }
            else
            {
                Debug.LogWarning("Item ID not found in database: " + item.itemId);
            }
        }
    }




    // TROUVER UN MOYEN D'UPDATE LES BONUS AVEC LES EQUIPEMENTS ET LES SKILLS ACHETES
    public void UpdateBonuses()
    {
        // BONUSES
        float regenRate = 0;                        // nb de coeur regénéré toutes les 60 secondes FAIT
        float doubleSquareCoinsChances = 0;         // % de chance de doubler en tuant FAIT
        float negativeEffectReducer = 0;            // % de dégât / de temps en moins FAIT 
        float itemsAddChance = 0;                   // Amélioration statistique des items FAIT
        float mineralChance = 0;                    // Chance que de meilleurs minéraux apparaissent FAIT
        float dodgeChance = 0;                      // Chance d'esquiver totalement un dégât FAIT
        float doubleMineralDropChance = 0;          // Chance de doubler un minéral FAIT
        float dropChance = 0;                       // Chance de tomber sur un meilleur item lors d'un drop FAIT
        float pickaxeSpeed = 0;                     // Réduction du temps de la pioche FAIT
        float bowSpeed = 0;                         // Réduction du temps de l'arc FAIT
        float shieldKnockback = 0;                  // Knockback ajouté au bouclier FAIT
        float bonusHP = 0;
        float bonusSTR = 0;
        float bonusSPE = 0;

        float bonusKBP = 0;
        float bonusKBR = 0;
        float bonusCRITD = 0;
        float bonusCRITC = 0;
        float bonusLUCK = 0;
        float vampire = 0;
        float dragonSkin = 0;
        float fireAttackChance = 0;
        float iceAttackChance = 0;
        float poisonAttackChance = 0;

        foreach (Skill skill in allSkills)
        {
            foreach (string id in PlayerLevels.instance.acquiredSkillsID)
            {
                if (skill.id == id)
                {
                    switch (skill.type)
                    {
                        case BONUS_TYPE.REGEN:
                            regenRate += skill.intValue;
                            break;
                        case BONUS_TYPE.ADD_STATS:
                            switch (skill.statAdd)
                            {
                                case STATS_ADD.HP:
                                    bonusHP += skill.intValue;
                                    break;
                                case STATS_ADD.STR:
                                    bonusSTR += skill.intValue;
                                    break;
                                case STATS_ADD.SPE:
                                    bonusSPE += skill.floatValue;
                                    break;
                                case STATS_ADD.KBP:
                                    bonusKBP += skill.floatValue;
                                    break;
                                case STATS_ADD.KBR:
                                    bonusKBR += skill.floatValue;
                                    break;
                                case STATS_ADD.CRITD:
                                    bonusCRITD += skill.floatValue;
                                    break;
                                case STATS_ADD.CRITC:
                                    bonusCRITC += skill.floatValue;
                                    break;
                                case STATS_ADD.LUCK:
                                    bonusLUCK += skill.intValue;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case BONUS_TYPE.DOUBLE_SQUARE:
                            doubleSquareCoinsChances += skill.floatValue;
                            break;
                        case BONUS_TYPE.EFFECT_REDUCER:
                            negativeEffectReducer += skill.floatValue;
                            break;
                        case BONUS_TYPE.ITEM_CHANCE:
                            itemsAddChance += skill.floatValue;
                            break;
                        case BONUS_TYPE.MINERAL_CHANCE:
                            mineralChance += skill.floatValue;
                            break;
                        case BONUS_TYPE.DODGE_CHANCE:
                            dodgeChance += skill.floatValue;
                            break;
                        case BONUS_TYPE.DOUBLE_MINERAL:
                            doubleMineralDropChance += skill.floatValue;
                            break;
                        case BONUS_TYPE.DROP_CHANCE:
                            dropChance += skill.floatValue;
                            break;
                        case BONUS_TYPE.PICKAXE_SPEED:
                            pickaxeSpeed += skill.floatValue;
                            break;
                        case BONUS_TYPE.BOW_SPEED:
                            bowSpeed += skill.floatValue;
                            break;
                        case BONUS_TYPE.SHIELD_KNOCKBACK:
                            shieldKnockback += skill.floatValue;
                            break;
                        case BONUS_TYPE.VAMPIRE:
                            vampire += skill.floatValue;
                            break;
                        case BONUS_TYPE.DRAGON_SKIN:
                            dragonSkin += skill.floatValue;
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        foreach (var equippedSlot in Equipement.instance.equippedSlots)
        {
            EquipementSlot slot = equippedSlot.GetComponent<EquipementSlot>();
            Item item = slot.actualItem;

            if (item != null)
            {
                if (item is Helmet helmet)
                {
                    vampire += helmet.vampire;

                    dragonSkin += helmet.dragonSkin;
                    regenRate += helmet.regenRate;
                    negativeEffectReducer += helmet.negativeEffectReducer;
                    mineralChance += helmet.mineralChance;
                    dodgeChance += helmet.dodgeChance;
                    doubleMineralDropChance += helmet.doubleMineralDropChance;

                    switch (helmet.armorEnchant)
                    {
                        case ARMOR_ENCHANT.REGEN:
                            regenRate += helmet.enchantLevel;
                            break;
                        case ARMOR_ENCHANT.DRAGON_SKIN:
                            dragonSkin += helmet.enchantLevel * 3;
                            break;
                        case ARMOR_ENCHANT.FANTOM_DODGE:
                            dodgeChance += helmet.enchantLevel * 2;
                            break;
                    }
                }
                else if (item is Chestplate chestplate)
                {
                    dragonSkin += chestplate.dragonSkin;
                    regenRate += chestplate.regenRate;
                    negativeEffectReducer += chestplate.negativeEffectReducer;
                    mineralChance += chestplate.mineralChance;
                    dodgeChance += chestplate.dodgeChance;
                    doubleMineralDropChance += chestplate.doubleMineralDropChance;

                    switch (chestplate.armorEnchant)
                    {
                        case ARMOR_ENCHANT.REGEN:
                            regenRate += chestplate.enchantLevel;
                            break;
                        case ARMOR_ENCHANT.DRAGON_SKIN:
                            dragonSkin += chestplate.enchantLevel * 3;
                            break;
                        case ARMOR_ENCHANT.FANTOM_DODGE:
                            dodgeChance += chestplate.enchantLevel * 2;
                            break;
                    }
                }
                else if (item is Leggings leggings)
                {
                    dragonSkin += leggings.dragonSkin;
                    regenRate += leggings.regenRate;
                    negativeEffectReducer += leggings.negativeEffectReducer;
                    mineralChance += leggings.mineralChance;
                    dodgeChance += leggings.dodgeChance;
                    doubleMineralDropChance += leggings.doubleMineralDropChance;

                    switch (leggings.armorEnchant)
                    {
                        case ARMOR_ENCHANT.REGEN:
                            regenRate += leggings.enchantLevel;
                            break;
                        case ARMOR_ENCHANT.DRAGON_SKIN:
                            dragonSkin += leggings.enchantLevel * 3;
                            break;
                        case ARMOR_ENCHANT.FANTOM_DODGE:
                            dodgeChance += leggings.enchantLevel * 2;
                            break;
                    }
                }
                else if (item is Boots boots)
                {
                    dragonSkin += boots.dragonSkin;
                    regenRate += boots.regenRate;
                    negativeEffectReducer += boots.negativeEffectReducer;
                    mineralChance += boots.mineralChance;
                    dodgeChance += boots.dodgeChance;
                    doubleMineralDropChance += boots.doubleMineralDropChance;

                    switch (boots.armorEnchant)
                    {
                        case ARMOR_ENCHANT.REGEN:
                            regenRate += boots.enchantLevel;
                            break;
                        case ARMOR_ENCHANT.DRAGON_SKIN:
                            dragonSkin += boots.enchantLevel * 3;
                            break;
                        case ARMOR_ENCHANT.FANTOM_DODGE:
                            dodgeChance += boots.enchantLevel * 2;
                            break;
                    }
                }
                else if (item is Weapon weapon)
                {
                    vampire += weapon.vampire;
                    fireAttackChance += weapon.fireAttackChance;
                    iceAttackChance += weapon.iceAttackChance;
                    poisonAttackChance += weapon.poisonAttackChance;
                    doubleSquareCoinsChances += weapon.doubleSquareCoinsChances;
                    dropChance += weapon.dropChance;

                    switch (weapon.enchant)
                    {
                        case WEAPON_ENCHANT.POISON:
                            poisonAttackChance += weapon.enchantLevel * 5;
                            break;
                        case WEAPON_ENCHANT.ICE:
                            iceAttackChance += weapon.enchantLevel * 5;
                            break;
                        case WEAPON_ENCHANT.FLAME:
                            fireAttackChance += weapon.enchantLevel * 5;
                            break;
                        case WEAPON_ENCHANT.VAMPIRE:
                            vampire += weapon.enchantLevel * 5;
                            break;
                    }
                }
            }
        }

        this.regenRate = regenRate;
        this.doubleSquareCoinsChances = doubleSquareCoinsChances;
        this.negativeEffectReducer = negativeEffectReducer;
        this.itemsAddChance = itemsAddChance;
        this.mineralChance = mineralChance;
        this.dodgeChance = dodgeChance;
        this.doubleMineralDropChance = doubleMineralDropChance;
        this.dropChance = dropChance;
        this.pickaxeSpeed = pickaxeSpeed;
        this.bowSpeed = bowSpeed;
        this.shieldKnockback = shieldKnockback;
        this.bonusHP = (int)bonusHP;
        this.bonusSTR = (int)bonusSTR;
        this.bonusLUCK = (int)bonusLUCK;
        this.bonusSPE = bonusSPE;
        this.bonusKBP = bonusKBP;
        this.bonusKBR = bonusKBR;
        this.bonusCRITD = bonusCRITD;
        this.bonusCRITC = bonusCRITC;
        this.vampire = vampire;
        this.dragonSkin = dragonSkin;
        this.fireAttackChance = fireAttackChance;
        this.iceAttackChance = iceAttackChance;
        this.poisonAttackChance = poisonAttackChance;

    }



}

