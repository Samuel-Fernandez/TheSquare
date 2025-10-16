using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketManager : MonoBehaviour
{
    // UI
    public GameObject UIMarket;

    // SELL
    public GameObject UISell;
    public GameObject sellButtonContainer; // Contenant avant vente
    public GameObject selledButtonContainer; // Contenant après vente
    public GameObject sellPanel;
    public List<GameObject> sellButtonList;
    GameObject selectedButton; // Dernier bouton cliqué
    public GameObject counterPanel; // Panel permettant le compteur pour items spéciaux
    public TextMeshProUGUI itemNameTxt;
    public TextMeshProUGUI itemValueTxt;
    public List<GameObject> buttonPlacedToSell;
    public GameObject buttonSellItem;
    public GameObject costContainer;

    // BUY
    public GameObject UIBuy;
    public List<Item> itemsToBuy; // Une seule liste pour tous les types
    List<GameObject> buyButtonList;
    public GameObject equipementStats;
    public GameObject buyContainer;
    public GameObject panelBuy;
    public GameObject counterBuy;
    public GameObject purchaseButton; // Bouton permettant l'achat

    // Prefabs
    public GameObject sellButton;
    public GameObject buyButton; // Bouton permettant d'afficher les items à acheter

    // Logic
    public TraderType type;
    public bool isSelling = true;

    // Counter
    public GameObject plusButton;
    public GameObject minusButton;
    public GameObject allButton;
    public GameObject removeButton;
    public GameObject plusButtonBuy;
    public GameObject minusButtonBuy;
    public GameObject allButtonBuy;
    public GameObject removeButtonBuy;
    public TextMeshProUGUI txtActualNb;
    public TextMeshProUGUI txtActualNbBuy;
    int actualNb;
    int maxNb;

    // Localization
    public TextMeshProUGUI title;
    public TextMeshProUGUI cost;
    public TextMeshProUGUI buyTxt;
    public TextMeshProUGUI SellTxt;
    public TextMeshProUGUI actualSquareCoins;
    public TextMeshProUGUI costSquareCoins;
    public TextMeshProUGUI sellButtonTxt;

    // To Save
    public TextMeshProUGUI reputationTxt;

    // New Trader Inventory System
    [Header("Current Trader")]
    public TraderInventory currentTraderInventory; // L'inventaire du trader actuel

    // New Market
    const int timeToRefresh = 1600;
    public int timerNewMarket = 0; // Un seul timer


    public static MarketManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(RoutineNewMarket());
    }

    private void Update()
    {

        if (UIMarket.activeSelf)
        {
            actualSquareCoins.text = PlayerManager.instance.player.GetComponent<Stats>().money.ToString();

            costSquareCoins.gameObject.SetActive(CalculatePrice() > 0);
            costSquareCoins.text = CalculatePrice().ToString();
            costContainer.SetActive(CalculatePrice() > 0);

            switch (type)
            {
                case TraderType.NONE:
                    break;
                case TraderType.ARMORER:
                    reputationTxt.text = Mathf.RoundToInt(PlayerLevels.instance.armorerReputation).ToString();
                    break;
                case TraderType.EXPLORER:
                    reputationTxt.text = Mathf.RoundToInt(PlayerLevels.instance.explorerReputation).ToString();
                    break;
                default:
                    break;
            }



            if (isSelling)
            {
                UpdateCounter();
                buttonSellItem.SetActive(CalculatePrice() > 0);
            }
            else
            {
                UpdateCounterBuy();
                purchaseButton.GetComponent<Button>().interactable = PlayerManager.instance.player.GetComponent<Stats>().money >= CalculatePrice();
            }
        }

    }

    public void PlusButton()
    {
        actualNb++;
        GetComponent<SoundContainer>().PlaySound("AddMineral", 1);
    }

    public void MinusButton()
    {
        actualNb--;
        GetComponent<SoundContainer>().PlaySound("AddMineral", 1);
    }

    public void RemoveButton()
    {
        actualNb = 0;
        GetComponent<SoundContainer>().PlaySound("AddMineral", 1);
    }

    public void AllButton()
    {
        actualNb = maxNb;
        GetComponent<SoundContainer>().PlaySound("AddMineral", 1);
    }

    public void UpdateCounter()
    {
        txtActualNb.text = actualNb.ToString();

        minusButton.GetComponent<Button>().interactable = actualNb > 0;
        removeButton.GetComponent<Button>().interactable = actualNb > 0;

        plusButton.GetComponent<Button>().interactable = actualNb < maxNb;
        allButton.GetComponent<Button>().interactable = actualNb < maxNb;
    }

    public void SellItems()
    {
        PlayerManager.instance.player.GetComponent<Stats>().money += CalculatePrice();
        GetComponent<SoundContainer>().PlaySound("Purchase", 1);

        if (type == TraderType.EXPLORER)
            PlayerLevels.instance.explorerReputation += CalculatePrice() / 500;
        else if (type == TraderType.ARMORER)
            PlayerLevels.instance.armorerReputation += CalculatePrice() / 500;



        if (type != TraderType.EXPLORER)
        {
            foreach (var button in buttonPlacedToSell)
            {
                Equipement.instance.RemoveItem(button.GetComponent<MarketButtonSell>().item);
                Destroy(button);
            }
        }
        else
        {
            foreach (var button in buttonPlacedToSell)
            {
                PlayerManager.instance.GetSpecialItem((button.GetComponent<MarketButtonSell>().item as SpecialItems).GetID()).nb -= (button.GetComponent<MarketButtonSell>().item as SpecialItems).nb;
                Destroy(button);
            }
        }



        buttonPlacedToSell.Clear();

        InitializeToSell();
    }

    int CalculatePrice()
    {
        int tempPrice = 0;

        // --- Fonction interne pour le calcul exponentiel unitaire ---
        int ExponentialPrice(int baseValue)
        {
            // Exemple : petit = proche du prix normal, gros = bien plus cher
            // Tu peux ajuster la formule selon le feeling
            return Mathf.RoundToInt(Mathf.Pow(baseValue, 1.2f));
        }

        // --- Cas : le joueur VEND ses items au PNJ ---
        if (isSelling)
        {
            if (type != TraderType.EXPLORER)
            {
                foreach (GameObject button in buttonPlacedToSell)
                {
                    tempPrice += button.GetComponent<MarketButtonSell>().item.value;
                }

                if (type == TraderType.ARMORER)
                    return Mathf.RoundToInt(tempPrice * (1 - PlayerLevels.instance.armorerReputation / 100f));
                else
                    return tempPrice;
            }
            else
            {
                foreach (GameObject button in buttonPlacedToSell)
                {
                    tempPrice += button.GetComponent<MarketButtonSell>().item.value
                               * button.GetComponent<MarketButtonSell>().nb;
                }

                return Mathf.RoundToInt(tempPrice * (1 - PlayerLevels.instance.explorerReputation / 100f));
            }
        }
        // --- Cas : le joueur ACHÈTE chez le PNJ ---
        else
        {
            if (selectedButton != null && selectedButton.GetComponent<MarketButtonBuy>())
            {
                int unitValue = 0;
                int quantity = 1;

                if (type != TraderType.EXPLORER)
                {
                    unitValue = selectedButton.GetComponent<MarketButtonBuy>().item.value;
                    quantity = 1;
                }
                else
                {
                    unitValue = (selectedButton.GetComponent<MarketButtonBuy>().item as SpecialItems).value;
                    quantity = actualNb;
                }

                // Prix = prix exponentiel unitaire × quantité
                tempPrice = ExponentialPrice(unitValue) * quantity;

                // Réputation appliquée
                if (type == TraderType.ARMORER)
                    return Mathf.RoundToInt(tempPrice * (1 + PlayerLevels.instance.armorerReputation / 100f));
                else if (type == TraderType.HEALER)
                    return Mathf.RoundToInt(tempPrice * (1 + PlayerLevels.instance.healerReputation / 100f));
                else
                    return Mathf.RoundToInt(tempPrice * (1 + PlayerLevels.instance.explorerReputation / 100f));
            }
            else
            {
                return 0;
            }
        }
    }



    public void PlaceButtonToSell()
    {
        // SI TOUT, DEPLACER
        // SI PAS TOUT, DUPLIQUER
        // SI PAS TOUT PUIS RAJOUT, AJOUTER
        // SI PAS TOUT PUIS TOUT, AJOUTER PUIS SUPPRIMER
        GetComponent<SoundContainer>().PlaySound("AddMineral", 1);

        if (type == TraderType.EXPLORER)
        {
            if (actualNb > 0)
            {
                // Vérifier si le type de l'item de selectedButton est déjà présent dans buttonPlacedToSell
                var existingButton = buttonPlacedToSell.FirstOrDefault(button =>
                    button.GetComponent<MarketButtonSell>().item is SpecialItems item &&
                    item.type == (selectedButton.GetComponent<MarketButtonSell>().item as SpecialItems).type);

                if (existingButton == null)
                {
                    // Type de l'item de selectedButton n'est pas encore dans buttonPlacedToSell
                    if (actualNb == maxNb)
                    {
                        selectedButton.transform.SetParent(selledButtonContainer.transform, false);
                        selectedButton.GetComponent<MarketButtonSell>().isSelling = true;
                        buttonPlacedToSell.Add(selectedButton);
                        sellButtonList.Remove(selectedButton);
                        selectedButton = null;
                        UpdateSellPanel(null);
                    }
                    else if (actualNb < maxNb)
                    {
                        GameObject newButton = Instantiate(sellButton, selledButtonContainer.transform);
                        SpecialItems temp = selectedButton.GetComponent<MarketButtonSell>().item as SpecialItems;
                        temp.nb = actualNb;
                        newButton.GetComponent<MarketButtonSell>().SetButton(temp);
                        newButton.GetComponent<MarketButtonSell>().isSelling = true;
                        selectedButton.GetComponent<MarketButtonSell>().nb = maxNb - actualNb;
                        selectedButton.GetComponent<MarketButtonSell>().UpdateNumber();
                        buttonPlacedToSell.Add(newButton);
                        selectedButton = null;
                        UpdateSellPanel(null);
                    }
                }
                else
                {

                    if (actualNb == maxNb)
                    {
                        // Fusionner et supprimer le bouton actuel
                        existingButton.GetComponent<MarketButtonSell>().nb += actualNb;
                        existingButton.GetComponent<MarketButtonSell>().UpdateNumber();
                        Destroy(selectedButton);
                        selectedButton = null;
                        UpdateSellPanel(null);
                    }
                    else if (actualNb < maxNb)
                    {
                        // Mettre à jour la quantité de l'élément existant
                        existingButton.GetComponent<MarketButtonSell>().nb += actualNb;
                        existingButton.GetComponent<MarketButtonSell>().UpdateNumber();

                        // Mettre à jour la quantité de l'élément sélectionné
                        selectedButton.GetComponent<MarketButtonSell>().nb = maxNb - actualNb;
                        selectedButton.GetComponent<MarketButtonSell>().UpdateNumber();

                        // Ne pas supprimer le bouton actuel
                        UpdateSellPanel(selectedButton);
                    }
                }
            }
        }
        else
        {
            selectedButton.transform.SetParent(selledButtonContainer.transform, false);
            selectedButton.GetComponent<MarketButtonSell>().isSelling = true;
            buttonPlacedToSell.Add(selectedButton);
            selectedButton = null;
            UpdateSellPanel(null);
        }

    }

    public void PlaceButtonToUnsell(GameObject clickedButton)
    {
        GetComponent<SoundContainer>().PlaySound("AddMineral", 1);

        if (type == TraderType.EXPLORER)
        {
            // Vérifier si le type de l'item de selectedButton est déjà présent dans buttonPlacedToSell
            var existingButton = sellButtonList.FirstOrDefault(button =>
                button.GetComponent<MarketButtonSell>().item is SpecialItems item &&
                item.type == (clickedButton.GetComponent<MarketButtonSell>().item as SpecialItems).type);


            if (existingButton != null)
            {
                existingButton.GetComponent<MarketButtonSell>().nb += clickedButton.GetComponent<MarketButtonSell>().nb;
                existingButton.GetComponent<MarketButtonSell>().UpdateNumber();
                Destroy(clickedButton);
                buttonPlacedToSell.Remove(clickedButton);
            }
            else
            {
                clickedButton.transform.SetParent(sellButtonContainer.transform, false);
                clickedButton.GetComponent<MarketButtonSell>().isSelling = false;
                buttonPlacedToSell.Remove(clickedButton);
                sellButtonList.Add(clickedButton);
            }
        }
        else
        {
            clickedButton.transform.SetParent(sellButtonContainer.transform, false);
            clickedButton.GetComponent<MarketButtonSell>().isSelling = false;
            buttonPlacedToSell.Remove(clickedButton);
        }

    }


    public void UpdateSellPanel(GameObject clickedButton)
    {
        if (clickedButton != null)
        {
            selectedButton = clickedButton;
            sellPanel.SetActive(true);

            if (type == TraderType.EXPLORER)
            {
                counterPanel.SetActive(true);
                actualNb = 0;
                maxNb = clickedButton.GetComponent<MarketButtonSell>().nb;
            }
            else
            {
                counterPanel.SetActive(false);
            }

            itemNameTxt.text = LocalizationManager.instance.GetText("items", selectedButton.GetComponent<MarketButtonSell>().item.GetID() + "_NAME");
            itemValueTxt.text = selectedButton.GetComponent<MarketButtonSell>().item.value.ToString();
        }
        else
        {
            sellPanel.SetActive(false);
        }

    }

    public void RemoveSellButton()
    {
        foreach (var button in sellButtonList)
        {
            Destroy(button);
        }

        sellButtonList.Clear();
    }

    public void CreateSellButton()
    {
        switch (type)
        {
            case TraderType.NONE:
                break;
            case TraderType.ARMORER:
                foreach (GameObject item in Equipement.instance.equipementSlots)
                {
                    if (item.GetComponent<EquipementSlot>().actualItem != null && item.GetComponent<EquipementSlot>().actualItem is not Consumables)
                    {
                        GameObject newButton = Instantiate(sellButton, sellButtonContainer.transform);
                        newButton.GetComponent<MarketButtonSell>().SetButton(item.GetComponent<EquipementSlot>().actualItem);
                        sellButtonList.Add(newButton);
                    }
                }
                break;
            case TraderType.EXPLORER:
                foreach (SpecialItems item in PlayerManager.instance.runtimeSpecialItems)
                {
                    if (item.nb > 0)
                    {
                        // Cloner l'objet SpecialItems
                        SpecialItems clonedItem = ScriptableObjectUtility.Clone(item);

                        // Créer un nouveau bouton et définir le bouton cloné
                        GameObject newButton = Instantiate(sellButton, sellButtonContainer.transform);
                        newButton.GetComponent<MarketButtonSell>().SetButton(clonedItem);

                        // Ajouter le nouveau bouton à la liste
                        sellButtonList.Add(newButton);
                    }
                }

                break;
            default:
                break;
        }

    }

    public void ToggleMarket(TraderInventory traderInventory)
    {
        this.currentTraderInventory = traderInventory;
        this.type = traderInventory != null ? traderInventory.traderType : TraderType.NONE;

        if (UIMarket.activeSelf)
        {
            UIAnimator.instance.DeactivateObjectWithTransition(UIMarket, .5f);
            InventoryManager.instance.canOpenInventory = true;
            QuestManager.instance.canOpenQuests = true;
            Time.timeScale = 1f;

        }
        else
        {
            UIAnimator.instance.ActivateObjectWithTransition(UIMarket, .5f);
            InventoryManager.instance.canOpenInventory = false;
            QuestManager.instance.canOpenQuests = false;
            Time.timeScale = 0f;
            InitializeMarket();
        }
    }

    public void ToggleMarket()
    {
        if (UIMarket.activeSelf)
        {
            UIAnimator.instance.DeactivateObjectWithTransition(UIMarket, .5f);
            InventoryManager.instance.canOpenInventory = true;
            QuestManager.instance.canOpenQuests = true;
            Time.timeScale = 1f;

        }
        else
        {
            UIAnimator.instance.ActivateObjectWithTransition(UIMarket, .5f);
            InventoryManager.instance.canOpenInventory = false;
            QuestManager.instance.canOpenQuests = false;
            Time.timeScale = 0f;
            InitializeMarket();
        }
    }

    public void InitializeMarket()
    {
        Localization();
        isSelling = true;
        InitializeToSell();
    }

    public void InitializeToSell()
    {
        foreach (var button in buttonPlacedToSell)
        {
            Destroy(button);
        }

        buttonPlacedToSell.Clear();

        UIBuy.SetActive(false);
        UISell.SetActive(true);
        isSelling = true;
        RemoveSellButton();
        CreateSellButton();
    }

    public void ClickButtonBuy(GameObject clickedButton)
    {
        UpdateBuyPanel(clickedButton);
        equipementStats.GetComponent<EquipementStats>().WriteStats(clickedButton.GetComponent<MarketButtonBuy>().item);
    }

    public void UpdateCounterBuy()
    {
        txtActualNbBuy.text = actualNb.ToString();

        minusButtonBuy.GetComponent<Button>().interactable = actualNb > 0;
        removeButtonBuy.GetComponent<Button>().interactable = actualNb > 0;

        plusButtonBuy.GetComponent<Button>().interactable = actualNb < maxNb;
        allButtonBuy.GetComponent<Button>().interactable = actualNb < maxNb;
    }

    public void UpdateBuyPanel(GameObject clickedButton)
    {
        if (clickedButton != null)
        {
            selectedButton = clickedButton;
            panelBuy.SetActive(true);

            if (type == TraderType.EXPLORER)
            {
                counterBuy.SetActive(true);
                actualNb = 0;
                maxNb = clickedButton.GetComponent<MarketButtonBuy>().nb;
            }
            else
            {
                counterBuy.SetActive(false);
            }
        }
        else
        {
            panelBuy.SetActive(false);
        }

    }

    public void RemoveBuyButton()
    {
        if (buyButtonList != null)
        {
            foreach (GameObject button in buyButtonList)
            {
                Destroy(button);
            }

            buyButtonList.Clear();
        }

    }

    public void CreateBuyButton()
    {
        if (buyButtonList == null)
        {
            buyButtonList = new List<GameObject>();
        }

        if (buyButton == null || buyContainer == null)
        {
            return;
        }

        foreach (var item in itemsToBuy)
        {
            if (item != null)
            {
                GameObject newButton = Instantiate(buyButton, buyContainer.transform);
                newButton.GetComponent<MarketButtonBuy>().SetButton(item);
                buyButtonList.Add(newButton);
            }
        }
    }

    public void PurchaseButton()
    {
        GetComponent<SoundContainer>().PlaySound("Purchase", 1);

        if (type == TraderType.EXPLORER)
            PlayerLevels.instance.explorerReputation += CalculatePrice() / 500;
        else if (type == TraderType.ARMORER)
            PlayerLevels.instance.armorerReputation += CalculatePrice() / 500;

        if (type == TraderType.EXPLORER)
        {
            PlayerManager.instance.player.GetComponent<Stats>().money -= CalculatePrice();

            PlayerManager.instance.GetSpecialItem((selectedButton.GetComponent<MarketButtonBuy>().item as SpecialItems).GetID()).nb += actualNb;
            (selectedButton.GetComponent<MarketButtonBuy>().item as SpecialItems).nb -= actualNb;
            selectedButton.GetComponent<MarketButtonBuy>().UpdateNumber();

            if ((selectedButton.GetComponent<MarketButtonBuy>().item as SpecialItems).nb <= 0)
            {
                Destroy(selectedButton);
                buyButtonList.Remove(selectedButton);
                itemsToBuy.Remove(selectedButton.GetComponent<MarketButtonBuy>().item);
                selectedButton = null;
            }

        }
        else
        {
            if (!Equipement.instance.InventoryFull())
            {
                PlayerManager.instance.player.GetComponent<Stats>().money -= CalculatePrice();

                Equipement.instance.AddItem(selectedButton.GetComponent<MarketButtonBuy>().item);
                Destroy(selectedButton);
                buyButtonList.Remove(selectedButton);
                itemsToBuy.Remove(selectedButton.GetComponent<MarketButtonBuy>().item);
                selectedButton = null;
            }

        }

        InitializeToBuy();
    }


    public void InitializeToBuy()
    {
        UISell.SetActive(false);
        panelBuy.SetActive(false);
        isSelling = false;
        RemoveBuyButton();

        UIBuy.SetActive(true);

        // Doit avoir un cooldown
        if (timerNewMarket == 0)
        {
            InitializeItemsToBuy();
        }

        CreateBuyButton();

    }

    public void InitializeItemsToBuy()
    {
        timerNewMarket = timeToRefresh;
        itemsToBuy.Clear();

        if (currentTraderInventory != null && currentTraderInventory.IsValid())
        {
            itemsToBuy.AddRange(currentTraderInventory.GenerateMarketItems());
        }
    }



    public void Localization()
    {
        title.text = LocalizationManager.instance.GetText("UI", "MARKET");
        switch (type)
        {
            case TraderType.NONE:
                break;
            case TraderType.ARMORER:
                title.text += " - " + LocalizationManager.instance.GetText("UI", "MARKET_ARMORER");
                break;
            case TraderType.EXPLORER:
                title.text += " - " + LocalizationManager.instance.GetText("UI", "MARKET_EXPLORER");
                break;
            default:
                break;
        }

        // Ajouter le nom du trader si disponible
        if (currentTraderInventory != null && !string.IsNullOrEmpty(currentTraderInventory.traderName))
        {
            title.text += " - " + currentTraderInventory.traderName;
        }

        cost.text = LocalizationManager.instance.GetText("UI", "LEVEL_COST_TEXT");
        buyTxt.text = LocalizationManager.instance.GetText("UI", "MARKET_BUY");
        SellTxt.text = LocalizationManager.instance.GetText("UI", "MARKET_SELL");
        sellButtonTxt.text = LocalizationManager.instance.GetText("UI", "MARKET_SELL_ITEMS");
    }

    public IEnumerator RoutineNewMarket()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (timerNewMarket > 0)
                timerNewMarket--;
        }

    }

}