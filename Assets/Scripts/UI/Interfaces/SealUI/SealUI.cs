using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SealUI : MonoBehaviour
{
    public static SealUI instance;

    [Header("TextMeshPro")]
    public TextMeshProUGUI uiTitle;
    public TextMeshProUGUI specialItemTitle;
    public TextMeshProUGUI addButonText;
    public TextMeshProUGUI neutralText;
    public TextMeshProUGUI fireText;
    public TextMeshProUGUI groundText;
    public TextMeshProUGUI lightText;
    public TextMeshProUGUI shadowText;
    public TextMeshProUGUI vegetalText;
    public TextMeshProUGUI waterText;
    public TextMeshProUGUI windText;

    [Header("Buttons")]
    public GameObject addButton;

    public List<GameObject> sealButtons; // les 4 gros bouttons

    public GameObject craftSealButton;

    [Header("Containers")]
    public GameObject sealUI;
    public GameObject buttonContainers;
    public GameObject neutralContainer;
    public GameObject fireContainer;
    public GameObject groundContainer;
    public GameObject lightContainer;
    public GameObject shadowContainer;
    public GameObject vegetalContainer;
    public GameObject waterContainer;
    public GameObject windContainer;
    public GameObject sealResume;
    public GameObject sealDescription; // La description du sceau

    [Header("Prefabs")]
    public GameObject buttonSpecialItems;

    List<GameObject> buttonList = new List<GameObject>();
    
    GameObject selectedButton;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleUI();
        }

        craftSealButton.SetActive(ConditionShowCraftButton());
        addButton.SetActive(!ConditionShowCraftButton());
    }

    public void ToggleSealDescription()
    {
        // Afficher l'UI de description du sceau cr��
        sealResume.SetActive(!sealResume.activeSelf);
        sealDescription.GetComponent<SealDescriptionUI>().SetUI();
    }

    public void CraftSeal()
    {
        // V�rifier qu'on a bien 4 items s�lectionn�s
        if (!ConditionShowCraftButton())
        {
            Debug.LogWarning("Cannot craft seal: not enough items selected!");
            return;
        }

        // R�cup�rer les 4 SpecialItems depuis les boutons
        SpecialItems[] specialItems = new SpecialItems[4];

        for (int i = 0; i < sealButtons.Count && i < 4; i++)
        {
            specialItems[i] = sealButtons[i].GetComponent<SealButtonSpecialItems>().item;

            if (specialItems[i] == null)
            {
                Debug.LogError($"Seal button {i} has no item!");
                return;
            }
        }

        // Retirer les items de l'inventaire
        foreach (var item in specialItems)
        {
            // Trouver l'item dans l'inventaire du joueur par ID
            SpecialItems runtimeItem = PlayerManager.instance.runtimeSpecialItems.Find(x => x.itemId == item.itemId);

            if (runtimeItem != null)
            {
                runtimeItem.nb -= item.numberRequiredForSealing;

                // V�rifier que le nombre ne devient pas n�gatif
                if (runtimeItem.nb < 0)
                {
                    Debug.LogWarning($"Not enough {runtimeItem.itemId} in inventory!");
                    runtimeItem.nb = 0;
                }
            }
            else
            {
                Debug.LogWarning($"Item {item.itemId} not found in player inventory!");
            }
        }

        // Cr�er et �quiper le sceau
        SealManager.instance.CreateSeal(specialItems);

        // Fermer l'UI de craft
        ToggleUI();

        // Ouvrir l'UI de description
        ToggleSealDescription();

        Debug.Log("Seal crafted successfully!");
    }

    // AJOUTE DANS L'UN DES 4 GROS BOUTONS LE SPECIALITEM, PUIS DESACTIVE LE BOUTON DU SPECIALITEM CONCERNE.
    public void AddButton()
    {
        foreach (var button in sealButtons)
        {
            if(!button.GetComponent<SealButtonSpecialItems>().item)
            {
                button.GetComponent<SealButtonSpecialItems>().InitButton(selectedButton.GetComponent<SealButtonSpecialItems>().item);
                button.GetComponent<SealButtonSpecialItems>().ChangeColor();
                button.GetComponent<Button>().enabled = true;
                button.GetComponent<SealButtonSpecialItems>().concernedButton = selectedButton;
                selectedButton.SetActive(false);
                selectedButton = null;
                break;

            }
        }

        addButton.GetComponent<Button>().enabled = false;
    }

    public void ToggleUI()
    {
        sealUI.SetActive(!sealUI.activeSelf);

        if (sealUI.activeSelf)
        {
            InitUI();
        }
    }

    public void SetSelectedButton(GameObject button)
    {
        selectedButton = button;
        addButton.GetComponent<Button>().enabled = true;
        ShowItemDescription();
    }

    public void InitUI()
    {
        addButonText.text = LocalizationManager.instance.GetText("UI", "ADD");
        uiTitle.text = LocalizationManager.instance.GetText("UI", "SEAL_UI_TITLE");
        selectedButton = null;
        addButton.GetComponent<Button>().enabled = false;

        foreach (var button in sealButtons)
        {
            button.GetComponent<Button>().enabled = false;
            button.GetComponent<SealButtonSpecialItems>().RemoveItem();
        }

        ClearSpecialItems();
        ResetItemDescription();
        InitSpecialItems();
    }

    public bool ConditionShowCraftButton()
    {
        foreach (var button in sealButtons)
        {
            if (button.GetComponent<SealButtonSpecialItems>().item == null)
                return false;
        }

        return true;
    }

    public void ResetItemDescription()
    {
        // Texte
        specialItemTitle.gameObject.SetActive(false);
        neutralText.gameObject.SetActive(false);
        fireText.gameObject.SetActive(false);
        groundText.gameObject.SetActive(false);
        lightText.gameObject.SetActive(false);
        shadowText.gameObject.SetActive(false);
        vegetalText.gameObject.SetActive(false);
        waterText.gameObject.SetActive(false);
        windText.gameObject.SetActive(false);

        // Containers
        neutralContainer.SetActive(false);
        fireContainer.SetActive(false);
        groundContainer.SetActive(false);
        lightContainer.SetActive(false);
        shadowContainer.SetActive(false);
        vegetalContainer.SetActive(false);
        waterContainer.SetActive(false);
        windContainer.SetActive(false);
    }

    public void ShowItemDescription()
    {
        ResetItemDescription();
        
        if (selectedButton)
        {
            SpecialItems item = selectedButton.GetComponent<SealButtonSpecialItems>().item;

            specialItemTitle.gameObject.SetActive(true);
            specialItemTitle.text = LocalizationManager.instance.GetText("items", item.itemId + "_NAME");

            foreach (var spirit in item.essenceComposition)
            {
                switch (spirit.essence.essenceID)
                {
                    case "neutral":
                        neutralText.gameObject.SetActive(true);
                        neutralContainer.SetActive(true);
                        neutralText.text = LocalizationManager.instance.GetText("SPIRIT", "neutral_NAME") + " " + spirit.percentage + "%";
                        break;
                    case "fire":
                        fireText.gameObject.SetActive(true);
                        fireContainer.SetActive(true);
                        fireText.text = LocalizationManager.instance.GetText("SPIRIT", "fire_NAME") + " " + spirit.percentage + "%";
                        break;
                    case "ground":
                        groundText.gameObject.SetActive(true);
                        groundContainer.SetActive(true);
                        groundText.text = LocalizationManager.instance.GetText("SPIRIT", "ground_NAME") + " " + spirit.percentage + "%";
                        break;
                    case "light":
                        lightText.gameObject.SetActive(true);
                        lightContainer.SetActive(true);
                        lightText.text = LocalizationManager.instance.GetText("SPIRIT", "light_NAME") + " " + spirit.percentage + "%";
                        break;
                    case "shadow":
                        shadowText.gameObject.SetActive(true);
                        shadowContainer.SetActive(true);
                        shadowText.text = LocalizationManager.instance.GetText("SPIRIT", "shadow_NAME") + " " + spirit.percentage + "%";
                        break;
                    case "vegetal":
                        vegetalText.gameObject.SetActive(true);
                        vegetalContainer.SetActive(true);
                        vegetalText.text = LocalizationManager.instance.GetText("SPIRIT", "vegetal_NAME") + " " + spirit.percentage + "%";
                        break;
                    case "water":
                        waterText.gameObject.SetActive(true);
                        waterContainer.SetActive(true);
                        waterText.text = LocalizationManager.instance.GetText("SPIRIT", "water_NAME") + " " + spirit.percentage + "%";
                        break;
                    case "wind":
                        windText.gameObject.SetActive(true);
                        windContainer.SetActive(true);
                        windText.text = LocalizationManager.instance.GetText("SPIRIT", "wind_NAME") + " " + spirit.percentage + "%";
                        break;
                    default:
                        break;
                }

            }
        }
    }


    public void ClearSpecialItems()
    {
        foreach (var item in buttonList)
        {
            Destroy(item);
        }

        buttonList.Clear();
    }

    public void InitSpecialItems()
    {
        foreach (var item in PlayerManager.instance.runtimeSpecialItems)
        {
            // Conditions pour afficher l'item
            if(item.nb >= item.numberRequiredForSealing && item.numberRequiredForSealing != 0)
            {
                GameObject buttonInstance = Instantiate(buttonSpecialItems, buttonContainers.transform);
                buttonInstance.GetComponent<SealButtonSpecialItems>().InitButton(item);
                buttonList.Add(buttonInstance);
            }
            
        }
    }

}
