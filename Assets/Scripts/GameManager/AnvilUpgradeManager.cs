using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AnvilUpgradeManager : MonoBehaviour
{
    public GameObject anvilUpgradeUI;
    public GameObject equipementViewer;
    public GameObject buttonPrefab;
    public GameObject selectedEquipement;
    public GameObject statsPanel;
    public Item selectedItem;
    public List<GameObject> mineralAddButtons;
    public GameObject upgradeButton;
    public List<GameObject> buttonList = new List<GameObject>();

    int cost = 0;
    public TextMeshProUGUI actualSquareCoins;
    public TextMeshProUGUI costTxt;

    // Level Gestion
    public GameObject levelScrollbar;
    public TextMeshProUGUI potentialLevelTxt;
    public int itemLevel;
    public bool itemUpgraded;
    int itemProgression;
    int maxLevel = 0;

    // Localization
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI costLabelText;
    public TextMeshProUGUI equipementTitleText;
    public TextMeshProUGUI noMineralText;
    public TextMeshProUGUI noEquipementText;


    public int potentialLevel = 0;


    bool wait;

    public static AnvilUpgradeManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        equipementTitleText.text = LocalizationManager.instance.GetText("UI", "INVENTORY_EQUIPEMENTS");
        noMineralText.text = LocalizationManager.instance.GetText("UI", "NO_MINERAL");
        noEquipementText.text = LocalizationManager.instance.GetText("UI", "NO_EQUIPEMENT");
    }

    bool NoMineral()
    {
        if(selectedItem)
        {
            foreach (GameObject button in mineralAddButtons)
            {
                if (button.activeSelf)
                    return false;
            }

            return true;
        }

        return false;
    }

    private void Update()
    {
        if(CalculatePotentialProgession() <= 0)
            upgradeButton.GetComponent<Button>().interactable = false;
        else
            upgradeButton.GetComponent<Button>().interactable = true;

        CalculateCost();
        UpdateTexts();

        if (selectedItem)
            UpdateProgressLevel();

        noMineralText.gameObject.SetActive(NoMineral());
    }

    void UpdateTexts()
    {
        actualSquareCoins.text = PlayerManager.instance.player.GetComponent<Stats>().money.ToString();
        costTxt.text = cost.ToString();

        if (selectedItem)
        {
            int damage = 0;
            int hp = 0;
            int defense = 0;
            float critD = 0;
            float critC = 0;
            float kbp = 0;
            float kbr = 0;
            float speed = 0;

            int extraLevel = potentialLevel;
            float futureLevelMultiplier = 1 + ((selectedItem.level + extraLevel - 1) * 0.2f);

            if (selectedItem is Helmet helmet)
            {
                int futureDefense = Mathf.RoundToInt(helmet.GetBaseDefense() * futureLevelMultiplier);
                int futureHp = Mathf.RoundToInt(helmet.GetBaseLife() * futureLevelMultiplier);
                int futureDamage = Mathf.RoundToInt(helmet.GetBaseDamage() * futureLevelMultiplier);

                if (helmet.GetBaseDefense() != 0) futureDefense += (selectedItem.level + extraLevel - 1);
                if (helmet.GetBaseLife() != 0) futureHp += (selectedItem.level + extraLevel - 1) * 4;
                if (helmet.GetBaseDamage() != 0) futureDamage += (selectedItem.level + extraLevel - 1);

                defense = futureDefense - helmet.defense;
                hp = futureHp - helmet.life;
                damage = futureDamage - helmet.damage;
            }
            else if (selectedItem is Chestplate chestplate)
            {
                int futureDefense = Mathf.RoundToInt(chestplate.GetBaseDefense() * futureLevelMultiplier);
                int futureHp = Mathf.RoundToInt(chestplate.GetBaseLife() * futureLevelMultiplier);
                int futureDamage = Mathf.RoundToInt(chestplate.GetBaseDamage() * futureLevelMultiplier);
                float futureCritD = chestplate.GetBaseCritDamage() * futureLevelMultiplier;
                float futureCritC = chestplate.GetBaseCritChance() * futureLevelMultiplier;
                float futureKbp = chestplate.GetBaseKnockbackPower() * futureLevelMultiplier;
                float futureKbr = chestplate.GetBaseKnockbackResistance() * futureLevelMultiplier;

                if (chestplate.GetBaseDefense() != 0) futureDefense += (selectedItem.level + extraLevel - 1);
                if (chestplate.GetBaseLife() != 0) futureHp += (selectedItem.level + extraLevel - 1) * 4;
                if (chestplate.GetBaseDamage() != 0) futureDamage += (selectedItem.level + extraLevel - 1);
                if (chestplate.GetBaseCritChance() != 0) futureCritC += (selectedItem.level + extraLevel - 1) * 0.01f;
                if (chestplate.GetBaseCritDamage() != 0) futureCritD += (selectedItem.level + extraLevel - 1) * 0.01f;
                if (chestplate.GetBaseKnockbackPower() != 0) futureKbp += (selectedItem.level + extraLevel - 1) * 0.01f;
                if (chestplate.GetBaseKnockbackResistance() != 0) futureKbr += (selectedItem.level + extraLevel - 1) * 0.01f;

                defense = futureDefense - chestplate.defense;
                hp = futureHp - chestplate.life;
                damage = futureDamage - chestplate.damage;
                critD = futureCritD - chestplate.critDamage;
                critC = futureCritC - chestplate.critChance;
                kbp = futureKbp - chestplate.knockbackPower;
                kbr = futureKbr - chestplate.knockbackResistance;
            }
            else if (selectedItem is Leggings leggings)
            {
                int futureDefense = Mathf.RoundToInt(leggings.GetBaseDefense() * futureLevelMultiplier);
                int futureHp = Mathf.RoundToInt(leggings.GetBaseLife() * futureLevelMultiplier);
                float futureKbp = leggings.GetBaseKnockbackPower() * futureLevelMultiplier;
                float futureKbr = leggings.GetBaseKnockbackResistance() * futureLevelMultiplier;
                float futureSpeed = leggings.GetBaseSpeed() * futureLevelMultiplier;

                if (leggings.GetBaseDefense() != 0) futureDefense += (selectedItem.level + extraLevel - 1);
                if (leggings.GetBaseLife() != 0) futureHp += (selectedItem.level + extraLevel - 1) * 4;
                if (leggings.GetBaseKnockbackPower() != 0) futureKbp += (selectedItem.level + extraLevel - 1) * 0.01f;
                if (leggings.GetBaseKnockbackResistance() != 0) futureKbr += (selectedItem.level + extraLevel - 1) * 0.01f;
                if (leggings.GetBaseSpeed() != 0) futureSpeed += (selectedItem.level + extraLevel - 1) * 0.01f;

                defense = futureDefense - leggings.defense;
                hp = futureHp - leggings.life;
                kbp = futureKbp - leggings.knockbackPower;
                kbr = futureKbr - leggings.knockbackResistance;
                speed = futureSpeed - leggings.speed;
            }
            else if (selectedItem is Boots boots)
            {
                int futureDefense = Mathf.RoundToInt(boots.GetBaseDefense() * futureLevelMultiplier);
                int futureHp = Mathf.RoundToInt(boots.GetBaseLife() * futureLevelMultiplier);
                float futureSpeed = boots.GetBaseSpeed() * futureLevelMultiplier;

                if (boots.GetBaseDefense() != 0) futureDefense += (selectedItem.level + extraLevel - 1);
                if (boots.GetBaseLife() != 0) futureHp += (selectedItem.level + extraLevel - 1) * 4;
                if (boots.GetBaseSpeed() != 0) futureSpeed += (selectedItem.level + extraLevel - 1) * 0.01f;

                defense = futureDefense - boots.defense;
                hp = futureHp - boots.life;
                speed = futureSpeed - boots.speed;
            }
            else if (selectedItem is Weapon weapon)
            {
                int futureDamage = Mathf.RoundToInt(weapon.GetBaseDamage() * futureLevelMultiplier);
                float futureCritD = weapon.GetBaseCritDamage() * futureLevelMultiplier;
                float futureCritC = weapon.GetBaseCritChance() * futureLevelMultiplier;
                float futureKbp = weapon.GetBaseKnockbackPower() * futureLevelMultiplier;

                if (weapon.GetBaseDamage() != 0) futureDamage += (selectedItem.level + extraLevel - 1);
                if (weapon.GetBaseCritDamage() != 0) futureCritD += (selectedItem.level + extraLevel - 1) * 0.01f;
                if (weapon.GetBaseCritChance() != 0) futureCritC += (selectedItem.level + extraLevel - 1) * 0.01f;
                if (weapon.GetBaseKnockbackPower() != 0) futureKbp += (selectedItem.level + extraLevel - 1) * 0.01f;

                damage = futureDamage - weapon.damage;
                critD = futureCritD - weapon.critDamage;
                critC = futureCritC - weapon.critChance;
                kbp = futureKbp - weapon.knockbackPower;
            }

            if (potentialLevel > 0)
            {
                statsPanel.GetComponent<EquipementStats>().WriteBonusStats(
                    damage,
                    speed,
                    hp,
                    defense,
                    Mathf.Round(critC * 100),
                    Mathf.Round(critD * 100),
                    kbp,
                    kbr,
                    0
                );
            }
            else
            {
                statsPanel.GetComponent<EquipementStats>().WriteBonusStats();
            }
        }
    }



    public void UpgradeEquipement()
    {
        if (PlayerManager.instance.player.GetComponent<Stats>().money >= cost)
        {
            itemUpgraded = true;
            GetComponent<SoundContainer>().PlayUISound("Upgrade", 1);
            GetComponent<SoundContainer>().PlayUISound("Success", 1);
            foreach (GameObject mineralButton in mineralAddButtons)
            {
                mineralButton.GetComponent<MineralAdd>().RemoveMinerals();
            }

            PlayerManager.instance.player.GetComponent<Stats>().money -= cost;

            int newProgression = CalculatePotentialProgession() + itemProgression;

            if (itemLevel + potentialLevel >= maxLevel)
            {
                itemLevel = maxLevel;
                newProgression = GetUpperLimit(0) - 1;
            }

            string newItemId = selectedItem.itemId.Substring(0, selectedItem.itemId.Length - 3) + newProgression.ToString("D3");
            selectedItem.itemId = newItemId;

            if (selectedItem is Helmet)
                (selectedItem as Helmet).GenerateStats();
            else if (selectedItem is Chestplate)
                (selectedItem as Chestplate).GenerateStats();
            else if (selectedItem is Leggings)
                (selectedItem as Leggings).GenerateStats();
            else if (selectedItem is Boots)
                (selectedItem as Boots).GenerateStats();
            else if (selectedItem is Weapon)
                (selectedItem as Weapon).GenerateStats();

            if (selectedItem.level > maxLevel)
            {
                upgradeButton.SetActive(false);
                foreach (GameObject button in mineralAddButtons)
                {
                    button.SetActive(false);
                }
            }

            // Reset du niveau potentiel pour afficher les bonnes stats ensuite
            potentialLevel = 0;

            InitializeAnvilUpgrade();
        }
    }



    void UpdateProgressLevel()
    {
        int potentialProgression = CalculatePotentialProgession();

        // Gérer l'incrémentation du potentialLevel
        while (itemLevel + potentialLevel < maxLevel &&
               itemProgression + potentialProgression >= GetUpperLimit(potentialLevel))
        {
            potentialLevel++;
        }

        // Gérer la décrémentation du potentialLevel
        while (potentialLevel > 0 &&
               itemProgression + potentialProgression < GetUpperLimit(potentialLevel - 1))
        {
            potentialLevel--;
        }

        // Affichage de la barre de progression et du texte
        if (itemLevel + potentialLevel < maxLevel)
        {
            levelScrollbar.GetComponent<Scrollbar>().size = (float)(itemProgression + potentialProgression - GetUpperLimit(potentialLevel - 1)) / (float)(GetUpperLimit(potentialLevel) - GetUpperLimit(potentialLevel - 1));
            levelScrollbar.GetComponent<Scrollbar>().handleRect.GetComponent<Image>().color = ChangeColorPotential(potentialLevel);
            potentialLevelTxt.text = LocalizationManager.instance.GetText("UI", "LEVEL_ABREVIATION_TEXT") + (itemLevel + potentialLevel).ToString();
            potentialLevelTxt.color = ChangeColorPotential(potentialLevel);
        }
        else
        {
            levelScrollbar.GetComponent<Scrollbar>().size = 1;
            levelScrollbar.GetComponent<Scrollbar>().handleRect.GetComponent<Image>().color = Color.yellow;
            potentialLevelTxt.text = LocalizationManager.instance.GetText("UI", "LEVEL_ABREVIATION_TEXT") + (itemLevel + potentialLevel).ToString();
            potentialLevelTxt.color = Color.yellow;
        }
    }

    Color ChangeColorPotential(int potentialLevel)
    {
        switch (potentialLevel)
        {
            case 1:
                return new Color(0.0f, 1.0f, 0.0f); // Green
            case 2:
                return new Color(0.5f, 1.0f, 0.0f); // Light Green
            case 3:
                return new Color(1.0f, 1.0f, 0.0f); // Yellow
            case 4:
                return new Color(1.0f, 0.8f, 0.0f); // Light Orange
            case 5:
                return new Color(1.0f, 0.5f, 0.0f); // Orange
            case 6:
                return new Color(1.0f, 0.3f, 0.0f); // Dark Orange
            case 7:
                return new Color(1.0f, 0.0f, 0.0f); // Red
            case 8:
                return new Color(0.8f, 0.0f, 0.0f); // Dark Red
            case 9:
                return new Color(0.5f, 0.0f, 0.0f); // Darkest Red
            default:
                return new Color(0.0f, 0.8f, 0.0f);
        }
    }


    int GetUpperLimit(int potentialLevel)
    {
        switch (itemLevel + potentialLevel)
        {
            case 1:
                return 6;
            case 2: 
                return 16;
            case 3: 
                return 31;
            case 4:
                return 51;
            case 5:
                return 101;
            case 6:
                return 201;
            case 7:
                return 351;
            case 8:
                return 501;
            case 9:
                return 1000;
            default:
                return 0;
        }
    }


    // Calcule le nombre d'Exp potentiel
    int CalculatePotentialProgession()
    {
        int exp = 0;

        foreach (GameObject mineralButton in mineralAddButtons)
        {
            exp += mineralButton.GetComponent<MineralAdd>().GetPotentialExp();
        }

        return exp;
    }

    // Récupère et converti la progression / niveau de l'item
    void GetItemLevel()
    {
        if (selectedItem)
        {
            itemProgression = int.Parse(selectedItem.itemId.Substring(selectedItem.itemId.Length - 3));

            if (itemProgression <= 5)
            {
                itemLevel = 1;
            }
            else if (itemProgression > 5 && itemProgression <= 15)
            {
                itemLevel = 2;
            }
            else if (itemProgression > 15 && itemProgression <= 30)
            {
                itemLevel = 3;
            }
            else if (itemProgression > 30 && itemProgression <= 50)
            {
                itemLevel = 4;
            }
            else if (itemProgression > 50 && itemProgression <= 100)
            {
                itemLevel = 5;
            }
            else if (itemProgression > 100 && itemProgression <= 200)
            {
                itemLevel = 6;
            }
            else if (itemProgression > 200 && itemProgression <= 350)
            {
                itemLevel = 7;
            }
            else if (itemProgression > 350 && itemProgression <= 500)
            {
                itemLevel = 8;
            }
            else if (itemProgression > 501 && itemProgression <= 999)
            {
                itemLevel = 9;
            }
            else if (itemProgression == 1000)
            {
                itemLevel = 10;
            }

            maxLevel = selectedItem.GetMaxLevel();
        }
    }

    // Lorsqu'un item est sélectionné
    public void Select(GameObject clickedButton)
    {
        selectedEquipement.SetActive(true);
        selectedItem = clickedButton.GetComponent<EquipementUpgradeButton>().GetItem();
        selectedEquipement.GetComponent<Image>().sprite = selectedItem.sprite;

        statsPanel.SetActive(true);
        statsPanel.GetComponent<EquipementStats>().WriteStats(selectedItem);
        levelScrollbar.SetActive(true);
        potentialLevel = 0;

        GetItemLevel();

        if(selectedItem.level < maxLevel)
        {
            upgradeButton.SetActive(true);

            foreach (GameObject button in mineralAddButtons)
            {
                button.SetActive(true);
                button.GetComponent<MineralAdd>().Initialize();
            }
        }
            
    }

    // Calcul le coût
    void CalculateCost()
    {
        int tempCost = 0;
        if (selectedItem)
            switch (selectedItem.rarity)
            {
                case Rarity.COMMON:
                    tempCost = tempCost = (int)Mathf.Round(CalculatePotentialProgession() * 2.25f); ;
                    break;
                case Rarity.UNCOMMON:
                    tempCost = (int)Mathf.Round(CalculatePotentialProgession() * 6f);
                    break;
                case Rarity.RARE:
                    tempCost = (int)Mathf.Round(CalculatePotentialProgession() * 12f);
                    break;
                case Rarity.EPIC:
                    tempCost = (int)Mathf.Round(CalculatePotentialProgession() * 20f);
                    break;
                case Rarity.LEGENDARY:
                    tempCost = (int)Mathf.Round(CalculatePotentialProgession() * 40f);
                    break;
                default:
                    break;
            }

        this.cost = tempCost;
    }

    // Active / Désactive l'UI
    public void ToggleUI()
    {

        InventoryManager.instance.canOpenInventory = anvilUpgradeUI.activeSelf;
        QuestManager.instance.canOpenQuests = anvilUpgradeUI.activeSelf;

        if (anvilUpgradeUI.activeSelf)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;

        if (!anvilUpgradeUI.activeSelf)
            UIAnimator.instance.ActivateObjectWithTransition(anvilUpgradeUI, .5f);
        else
            UIAnimator.instance.DeactivateObjectWithTransition(anvilUpgradeUI, .5f);

        InitializeAnvilUpgrade();

    }

    // Initialisation de l'UI
    public void InitializeAnvilUpgrade()
    {
        if (selectedItem)
            selectedItem = null;
        
        upgradeText.text = LocalizationManager.instance.GetText("UI", "ANVIL_BUTTON_UPGRADE");
        costLabelText.text = LocalizationManager.instance.GetText("UI", "LEVEL_COST_TEXT");

    selectedEquipement.SetActive(false);
        statsPanel.SetActive(false);
        levelScrollbar.SetActive(false);
        upgradeButton.SetActive(false);

        foreach (GameObject button in buttonList)
        {
            Destroy(button);
        }

        foreach (GameObject button in mineralAddButtons)
        {
            button.SetActive(false);
        }

        buttonList.Clear();

        foreach (GameObject item in Equipement.instance.equippedSlots)
        {
            if(item.GetComponent<EquipementSlot>().actualItem != null)
            {
                noEquipementText.gameObject.SetActive(false);
                // Créer un nouveau bouton
                GameObject newButton = Instantiate(buttonPrefab, equipementViewer.transform);
                // Initialiser le bouton
                newButton.GetComponent<EquipementUpgradeButton>().SetButton(item.GetComponent<EquipementSlot>().actualItem);
                // Ajouter le bouton à la liste de boutons
                buttonList.Add(newButton);
            }
           
        }

        foreach (GameObject item in Equipement.instance.equipementSlots)
        {
            if (item.GetComponent<EquipementSlot>().actualItem != null && item.GetComponent<EquipementSlot>().actualItem is not Consumables)
            {
                noEquipementText.gameObject.SetActive(false);
                // Créer un nouveau bouton
                GameObject newButton = Instantiate(buttonPrefab, equipementViewer.transform);
                // Initialiser le bouton
                newButton.GetComponent<EquipementUpgradeButton>().SetButton(item.GetComponent<EquipementSlot>().actualItem);
                // Ajouter le bouton à la liste de boutons
                buttonList.Add(newButton);
            }
        }

    }
}
