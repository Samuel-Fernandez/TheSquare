using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerLevels : MonoBehaviour
{

    public int lvlSTR = 1;
    public int lvlHP = 1;
    public int lvlLuck = 1;
    public int price;
    public bool lvlChanged; // a mettre à true quand achat

    public GameObject UIPlayerLevels;

    public TextMeshProUGUI titleText;

    public List<MyButton> buyButtons;

    public List<TextMeshProUGUI> lifeText;
    public List<TextMeshProUGUI> strengthText;
    public List<TextMeshProUGUI> luckText;
    
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI actualSquareCoins;

    // Skills
    public GameObject skillPanel;
    public List<string> acquiredSkillsID;
    public List<GameObject> skillButtonContainer;
    List<GameObject> skillButtons = new List<GameObject>();
    public TextMeshProUGUI skillDescription;
    public TextMeshProUGUI skillTitle;
    public List<GameObject> requirements;
    public GameObject buyButton;
    GameObject selectedButton;

    // Reputation A SAUVEGARDER
    public float explorerReputation;
    public float armorerReputation;
    public float healerReputation;

    public GameObject attributesMenu;
    public GameObject skillMenu;

    public GameObject attributesButton;
    public GameObject skillsButton;

    public static PlayerLevels instance;

    bool waitBuy = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateStats();
        skillPanel.SetActive(false);
        FindAllSkillButton();
        OpenAttributesMenu();
    }

    private void Update()
    {
        Color enabledColor = new Color(0.294f, 0.690f, 0.275f);  // Couleur 4BB046
        Color disabledColor = new Color(0.278f, 0.278f, 0.278f);  // Couleur 474747

        if (!EnoughMoney())
        {
            foreach (MyButton button in buyButtons)
            {
                button.enabled = false;
                button.GetComponent<Image>().color = disabledColor;
            }
        }
        else
        {
            foreach (MyButton button in buyButtons)
            {
                button.enabled = true;
                button.GetComponent<Image>().color = enabledColor;
            }
        }

        if (PlayerManager.instance.playerInputActions.Menu.InventoryRight.triggered || PlayerManager.instance.playerInputActions.Menu.InventoryLeft.triggered)
        {
            if (attributesMenu.activeSelf)
            {
                OpenSkillsMenu();
                if(EventSystem.current)
                    EventSystem.current.SetSelectedGameObject(attributesButton);
            }
            else
            {
                OpenAttributesMenu();

                if (EventSystem.current)
                    EventSystem.current.SetSelectedGameObject(skillsButton);
            }
        }

        if(PlayerManager.instance.playerInputActions.Menu.Pause.triggered && UIPlayerLevels.activeSelf)
        {
            ToggleUI();
        }
    }

    public void OpenSkillsMenu()
    {
        attributesMenu.SetActive(false);
        skillMenu.SetActive(true);
        titleText.text = LocalizationManager.instance.GetText("UI", "PLAYER_LEVELS_SKILLS");
    }

    public void OpenAttributesMenu()
    {
        attributesMenu.SetActive(true);
        skillMenu.SetActive(false);
        titleText.text = LocalizationManager.instance.GetText("UI", "PLAYER_LEVELS_ATTRIBUTES");
    }
    void FindAllSkillButton()
    {
        UIPlayerLevels.SetActive(true);
        foreach (GameObject container in skillButtonContainer)
        {
            foreach (Button item in container.GetComponentsInChildren<Button>())
            {
                skillButtons.Add(item.gameObject);
            }
        }
        UIPlayerLevels.SetActive(false);
    }

    public void BuySkill()
    {
        GetComponent<SoundContainer>().PlayUISound("Success", 1);
        acquiredSkillsID.Add(selectedButton.GetComponent<SkillButton>().skill.id);
        buyButton.SetActive(!acquiredSkillsID.Contains(selectedButton.GetComponent<SkillButton>().skill.id));
        PlayerManager.instance.player.GetComponent<Stats>().money -= selectedButton.GetComponent<SkillButton>().skill.cost;
        lvlChanged = true;
        UpdateStats();

        // Garder une référence du bouton acheté pour le re-sélectionner
        GameObject purchasedButton = selectedButton;

        foreach (GameObject button in skillButtons)
        {
            button.GetComponent<SkillButton>().UpdateButton();
        }

        SkillButtonClick(purchasedButton);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(purchasedButton);
    }

    public void SkillButtonClick(GameObject clickedButton)
    {
        foreach (GameObject button in skillButtons)
        {
            button.GetComponent<SkillButton>().Unselect();
        }

        UpdateSkillDescription(clickedButton);
        UpdateSkillBuyButton(CheckRequirements(clickedButton));

    }

    void UpdateSkillDescription(GameObject clickedButton)
    {
        selectedButton = clickedButton;
        skillPanel.SetActive(true);

        switch (clickedButton.GetComponent<SkillButton>().skill.type)
        {
            case BONUS_TYPE.REGEN:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "REGEN_DESC", clickedButton.GetComponent<SkillButton>().skill.intValue);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "REGEN_NAME");
                break;
            case BONUS_TYPE.ADD_STATS:
                switch (clickedButton.GetComponent<SkillButton>().skill.statAdd)
                {
                    case STATS_ADD.HP:
                        skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_HP_DESC", clickedButton.GetComponent<SkillButton>().skill.intValue);
                        skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_HP_NAME");
                        break;
                    case STATS_ADD.STR:
                        skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_STR_DESC", clickedButton.GetComponent<SkillButton>().skill.intValue);
                        skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_STR_NAME");
                        break;
                    case STATS_ADD.SPE:
                        skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_SPE_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue);
                        skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_SPE_NAME");
                        break;
                    case STATS_ADD.KBP:
                        skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_KBP_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                        skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_KBP_NAME");
                        break;
                    case STATS_ADD.KBR:
                        skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_KBR_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                        skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_KBR_NAME");
                        break;
                    case STATS_ADD.CRITD:
                        skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_CRITD_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                        skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_CRITD_NAME");
                        break;
                    case STATS_ADD.CRITC:
                        skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_CRITC_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                        skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_CRITC_NAME");
                        break;
                    case STATS_ADD.LUCK:
                        skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_LUCK_DESC", clickedButton.GetComponent<SkillButton>().skill.intValue);
                        skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "BONUS_LUCK_NAME");
                        break;
                    default:
                        break;
                }
                break;
            case BONUS_TYPE.DOUBLE_SQUARE:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "DOUBLE_SQUARE_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "DOUBLE_SQUARE_NAME");
                break;
            case BONUS_TYPE.EFFECT_REDUCER:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "EFFECT_REDUCER_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "EFFECT_REDUCER_NAME");
                break;
            case BONUS_TYPE.ITEM_CHANCE:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "ITEM_CHANCE_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "ITEM_CHANCE_NAME");
                break;
            case BONUS_TYPE.MINERAL_CHANCE:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "MINERAL_CHANCE_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "MINERAL_CHANCE_NAME");
                break;
            case BONUS_TYPE.DODGE_CHANCE:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "DODGE_CHANCE_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "DODGE_CHANCE_NAME");
                break;
            case BONUS_TYPE.DOUBLE_MINERAL:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "DOUBLE_MINERAL_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "DOUBLE_MINERAL_NAME");
                break;
            case BONUS_TYPE.DROP_CHANCE:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "DROP_CHANCE_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "DROP_CHANCE_NAME");
                break;
            case BONUS_TYPE.PICKAXE_SPEED:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "PICKAXE_SPEED_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "PICKAXE_SPEED_NAME");
                break;
            case BONUS_TYPE.BOW_SPEED:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "BOW_SPEED_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "BOW_SPEED_NAME");
                break;
            case BONUS_TYPE.SHIELD_KNOCKBACK:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "SHIELD_KNOCKBACK_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "SHIELD_KNOCKBACK_NAME");
                break;
            case BONUS_TYPE.VAMPIRE:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "VAMPIRE_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "VAMPIRE_NAME");
                break;
            case BONUS_TYPE.DRAGON_SKIN:
                skillDescription.text = LocalizationManager.instance.GetText("SKILLS", "DRAGON_SKIN_DESC", clickedButton.GetComponent<SkillButton>().skill.floatValue * 100);
                skillTitle.text = LocalizationManager.instance.GetText("SKILLS", "DRAGON_SKIN_NAME");
                break;
            default:
                break;
        }



        Skill skill = clickedButton.GetComponent<SkillButton>().skill;

        if(skill.lvlLifeMin > 1)
        {
            requirements[0].GetComponentInChildren<TextMeshProUGUI>().text = "> " + (skill.lvlLifeMin - 1);
            requirements[0].SetActive(true);
        }
        else
        {
            requirements[0].SetActive(false);
        }

        if (skill.lvlStrengthMin > 0)
        {
            requirements[1].GetComponentInChildren<TextMeshProUGUI>().text = "> " + (skill.lvlStrengthMin - 1);
            requirements[1].SetActive(true);
        }
        else
        {
            requirements[1].SetActive(false);
        }

        if (skill.lvlLuckMin > 0)
        {
            requirements[2].GetComponentInChildren<TextMeshProUGUI>().text = "> " + (skill.lvlLuckMin - 1);
            requirements[2].SetActive(true);
        }
        else
        {
            requirements[2].SetActive(false);
        }

        requirements[3].GetComponentInChildren<TextMeshProUGUI>().text = skill.cost.ToString();

    }

    void UpdateSkillBuyButton(bool canBuy)
    {
        Color canBuyColor = new Color(59 / 255f, 217 / 255f, 66 / 255f);
        Color cantBuyColor = new Color(71 / 255f, 71 / 255f, 71 / 255f);

        buyButton.GetComponent<Button>().enabled = canBuy;
        buyButton.SetActive(!acquiredSkillsID.Contains(selectedButton.GetComponent<SkillButton>().skill.id));

        if (canBuy)
        {
            buyButton.GetComponent<Image>().color = canBuyColor;
            buyButton.GetComponentInChildren<Image>().color = canBuyColor;
        }
        else
        {
            buyButton.GetComponent<Image>().color = cantBuyColor;
            buyButton.GetComponentInChildren<Image>().color = cantBuyColor;
        }
    }


    bool CheckRequirements(GameObject button)
    {
        Skill skill = button.GetComponent<SkillButton>().skill;
        bool allRequirementsGood = true;

        Color requirementGoodColor = new Color(59 / 255f, 217 / 255f, 66 / 255f);
        Color missingRequirementColor = new Color(232 / 255f, 28 / 255f, 28 / 255f);

        if (skill.lvlLifeMin <= lvlHP)
        {
            requirements[0].GetComponentInChildren<TextMeshProUGUI>().color = requirementGoodColor;
        }
        else
        {
            allRequirementsGood = false;
            requirements[0].GetComponentInChildren<TextMeshProUGUI>().color = missingRequirementColor;
        }

        if(skill.lvlStrengthMin <= lvlSTR)
        {
            requirements[1].GetComponentInChildren<TextMeshProUGUI>().color = requirementGoodColor;

        }
        else
        {
            allRequirementsGood = false;
            requirements[1].GetComponentInChildren<TextMeshProUGUI>().color = missingRequirementColor;
        }

        if (skill.lvlLuckMin <= lvlLuck)
        {
            requirements[2].GetComponentInChildren<TextMeshProUGUI>().color = requirementGoodColor;

        }
        else
        {
            allRequirementsGood = false;
            requirements[2].GetComponentInChildren<TextMeshProUGUI>().color = missingRequirementColor;
        }

        if(skill.cost <= PlayerManager.instance.player.GetComponent<Stats>().money)
        {
            requirements[3].GetComponentInChildren<TextMeshProUGUI>().color = requirementGoodColor;
        }
        else
        {
            requirements[3].GetComponentInChildren<TextMeshProUGUI>().color = missingRequirementColor;
            allRequirementsGood = false;
        }

        return allRequirementsGood;
    }

    public void ToggleUI()
    {
        InventoryManager.instance.canOpenInventory = UIPlayerLevels.activeSelf;
        QuestManager.instance.canOpenQuests = UIPlayerLevels.activeSelf;

        PlayerLevels.instance.UpdateStats();


        if (UIPlayerLevels.activeSelf)
        {
            Time.timeScale = 1f;
            SaveManager.instance.Save();
            NotificationManager.instance.ShowPopup(LocalizationManager.instance.GetText("UI", "NOTIFICATION_SAVE"));
        }
        else
            Time.timeScale = 0f;

        if (!UIPlayerLevels.activeSelf)
            UIAnimator.instance.ActivateObjectWithTransition(UIPlayerLevels, .5f);
        else
            UIAnimator.instance.DeactivateObjectWithTransition(UIPlayerLevels, .5f);
    }

    bool EnoughMoney()
    {
        return PlayerManager.instance.player.GetComponent<Stats>().money >= price;
    }

    public void BuyLife()
    {
        if(!waitBuy)
        {
            StartCoroutine(WaitBeforeBuy());

            if (EnoughMoney())
            {
                PlayerManager.instance.player.GetComponent<Stats>().money -= price;
                lvlHP++;
                UpdateStats();
                GetComponent<SoundContainer>().PlayUISound("lvlUp", 1);
                lvlChanged = true;
            }
            else
            {
                GetComponent<SoundContainer>().PlayUISound("denied", 1);
            }
        }
    }

    public void BuyStrength()
    {
        if(!waitBuy)
        {
            StartCoroutine(WaitBeforeBuy());

            if (EnoughMoney())
            {
                PlayerManager.instance.player.GetComponent<Stats>().money -= price;
                lvlSTR++;
                UpdateStats();
                GetComponent<SoundContainer>().PlayUISound("lvlUp", 1);
                lvlChanged = true;

            }
            else
            {
                GetComponent<SoundContainer>().PlayUISound("denied", 1);
            }
        }
        
    }

    public void BuyLuck()
    {
        if (!waitBuy)
        {
            StartCoroutine(WaitBeforeBuy());

            if (EnoughMoney())
            {
                PlayerManager.instance.player.GetComponent<Stats>().money -= price;
                lvlLuck++;
                UpdateStats();
                GetComponent<SoundContainer>().PlayUISound("lvlUp", 1);
                lvlChanged = true;
            }
            else
            {
                GetComponent<SoundContainer>().PlayUISound("denied", 1);
            }
        }
           
    }

    IEnumerator WaitBeforeBuy()
    {
        waitBuy = true;
        yield return new WaitForSecondsRealtime(.5f);
        waitBuy = false;
    }

    public void UpdateStats()
    {
        price = (int)(100 * Mathf.Pow(1.145f, lvlHP + lvlLuck + lvlSTR));

        priceText.text = LocalizationManager.instance.GetText("UI", "LEVEL_COST_TEXT") + " : " + price.ToString();

        lifeText[0].text = LocalizationManager.instance.GetText("UI", "LEVEL_ABREVIATION_TEXT") + ". " + lvlHP.ToString();
        strengthText[0].text = LocalizationManager.instance.GetText("UI", "LEVEL_ABREVIATION_TEXT") + ". " + lvlSTR.ToString();
        luckText[0].text = LocalizationManager.instance.GetText("UI", "LEVEL_ABREVIATION_TEXT") + ". " + lvlLuck.ToString();

        lifeText[1].text = "+" + (lvlHP * 4).ToString();

        strengthText[1].text = "+" + lvlSTR.ToString();
        strengthText[2].text = "+" + (lvlSTR / 20f).ToString();

        luckText[1].text = "+" + lvlLuck.ToString();
        luckText[2].text = "+" + (lvlLuck / 100f).ToString();

        actualSquareCoins.text = PlayerManager.instance.player.GetComponent<Stats>().money.ToString();
    }

}
