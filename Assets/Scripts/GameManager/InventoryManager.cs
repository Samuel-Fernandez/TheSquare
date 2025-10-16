using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class InventoryManager : MonoBehaviour
{
    public GameObject inventory;
    public static InventoryManager instance;

    public GameObject statsPanel;
    public GameObject equipementPanel;
    public GameObject itemsPanel;
    public GameObject specialItemsPanel;
    public GameObject mapPanel;
    public GameObject systemPanel;

    public GameObject statsButton;
    public GameObject equipementButton;
    public GameObject itemsButton;
    public GameObject lvlUpButton;
    public GameObject mapButton;
    public GameObject systemButton;

    public TextMeshProUGUI txtTitle;

    public bool canOpenInventory = true;

    LastPanel lastPanel = LastPanel.NULL;

    private List<LastPanel> panelOrder = new List<LastPanel>
    {
        LastPanel.STATS,
        LastPanel.EQUIPEMENT,
        LastPanel.SPECIAL_OBJECTS,
        LastPanel.SPECIAL_ITEMS,
        LastPanel.MAP,
        LastPanel.SYSTEM
    };


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        OpenStats();
        defaultFixedDeltaTime = Time.fixedDeltaTime; // Sauvegarde la valeur par défaut

    }

    private void Update()
    {
        ToggleInventory();

        if (inventory.activeSelf) // uniquement si inventaire ouvert
        {
            if (PlayerManager.instance.playerInputActions.Menu.InventoryRight.triggered)
                NavigateInventory(true);

            if (PlayerManager.instance.playerInputActions.Menu.InventoryLeft.triggered)
                NavigateInventory(false);
        }
    }


    enum LastPanel
    {
        STATS,
        EQUIPEMENT,
        SPECIAL_OBJECTS,
        SPECIAL_ITEMS,
        MAP,
        SYSTEM,
        NULL,
    }

    private float defaultFixedDeltaTime;

    private void NavigateInventory(bool toRight)
    {
        int index = panelOrder.IndexOf(lastPanel);
        if (index == -1) index = 0; // sécurité

        index = (index + (toRight ? 1 : -1) + panelOrder.Count) % panelOrder.Count;
        OpenPanel(panelOrder[index]);
    }

    private void OpenPanel(LastPanel panel)
    {
        switch (panel)
        {
            case LastPanel.STATS:
                OpenStats();
                SelectButton(statsButton);
                break;

            case LastPanel.EQUIPEMENT:
                OpenEquipement();
                SelectButton(equipementButton);
                break;

            case LastPanel.SPECIAL_OBJECTS:
                OpenItems();
                SelectButton(itemsButton);
                break;

            case LastPanel.SPECIAL_ITEMS:
                OpenSpecialItems();
                SelectButton(lvlUpButton);
                break;

            case LastPanel.MAP:
                OpenMap();
                SelectButton(mapButton);
                break;

            case LastPanel.SYSTEM:
                OpenSystem();
                SelectButton(systemButton);
                break;
        }
    }



    private void SelectButton(GameObject button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }


    public void ToggleInventory(bool var = false)
    {
        if (canOpenInventory)
        {
            if (PlayerManager.instance.playerInputActions.Menu.Pause.triggered || var)
            {
                if(!PlayerLevels.instance.UIPlayerLevels.activeSelf)
                {
                    if (!inventory.activeSelf)
                    {
                        UIAnimator.instance.ActivateObjectWithTransition(inventory, .2f);

                        Time.timeScale = 0f;
                        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Assure que fixedDeltaTime est en pause

                        QuestManager.instance.canOpenQuests = false;

                        if (lastPanel == LastPanel.SPECIAL_ITEMS)
                        {
                            SpecialItemUIManager.instance.InitUI();
                        }

                        if (lastPanel == LastPanel.MAP)
                        {
                            MapManager.instance.OpenMap();
                        }
                    }
                    else
                    {
                        Time.timeScale = 1f;
                        Time.fixedDeltaTime = defaultFixedDeltaTime; // Réinitialise fixedDeltaTime
                        UIAnimator.instance.DeactivateObjectWithTransition(inventory, .2f);
                        QuestManager.instance.canOpenQuests = true;
                    }
                }
            }
        }
    }

    public void OpenMap()
    {
        if (lastPanel != LastPanel.MAP)
        {
            CloseAll();
            ChangeButtonChildImageColor(mapButton, Color.white);
            UIAnimator.instance.ActivateObjectWithTransition(mapPanel, .2f);
            lastPanel = LastPanel.MAP;
            txtTitle.text = LocalizationManager.instance.GetText("UI", "MAP_PANEL");
            MapManager.instance.OpenMap();
        }
    }

    public void OpenStats()
    {
        if (lastPanel != LastPanel.STATS)
        {
            CloseAll();
            ChangeButtonChildImageColor(statsButton, Color.white);
            UIAnimator.instance.ActivateObjectWithTransition(statsPanel, .2f);
            lastPanel = LastPanel.STATS;
            txtTitle.text = LocalizationManager.instance.GetText("UI", "INVENTORY_STATS");
        }
    }

    public void OpenEquipement()
    {
        if (lastPanel != LastPanel.EQUIPEMENT)
        {
            CloseAll();
            ChangeButtonChildImageColor(equipementButton, Color.white);
            UIAnimator.instance.ActivateObjectWithTransition(equipementPanel, .2f);
            lastPanel = LastPanel.EQUIPEMENT;
            txtTitle.text = LocalizationManager.instance.GetText("UI", "INVENTORY_EQUIPEMENTS");
            Equipement.instance.equipementSlots[0].GetComponent<EquipementSlot>().ClickEquipementSlot();

        }
    }

    public void OpenItems()
    {
        if (lastPanel != LastPanel.SPECIAL_OBJECTS)
        {
            CloseAll();
            ChangeButtonChildImageColor(itemsButton, Color.white);
            UIAnimator.instance.ActivateObjectWithTransition(itemsPanel, .2f);
            lastPanel = LastPanel.SPECIAL_OBJECTS;
            SpecialObjectsManager.instance.UpdateAllButtons();
            txtTitle.text = LocalizationManager.instance.GetText("UI", "INVENTORY_ITEMS");

        }
    }

    public void OpenSpecialItems()
    {
        if (lastPanel != LastPanel.SPECIAL_ITEMS)
        {
            CloseAll();
            ChangeButtonChildImageColor(lvlUpButton, Color.white);
            UIAnimator.instance.ActivateObjectWithTransition(specialItemsPanel, .2f);
            lastPanel = LastPanel.SPECIAL_ITEMS;
            txtTitle.text = LocalizationManager.instance.GetText("UI", "INVENTORY_SPECIAL_ITEMS");
            SpecialItemUIManager.instance.InitUI();
        }
    }

    public void OpenSystem()
    {
        if (lastPanel != LastPanel.SYSTEM)
        {
            CloseAll();
            ChangeButtonChildImageColor(systemButton, Color.white);
            UIAnimator.instance.ActivateObjectWithTransition(systemPanel, .2f);
            lastPanel = LastPanel.SYSTEM;
            txtTitle.text = LocalizationManager.instance.GetText("UI", "INVENTORY_OPTIONS");
        }
    }

    void CloseAll()
    {
        UIAnimator.instance.DeactivateObjectWithTransition(statsPanel, 0f);
        UIAnimator.instance.DeactivateObjectWithTransition(equipementPanel, 0f);
        UIAnimator.instance.DeactivateObjectWithTransition(itemsPanel, 0f);
        UIAnimator.instance.DeactivateObjectWithTransition(specialItemsPanel, 0f);
        UIAnimator.instance.DeactivateObjectWithTransition(mapPanel, 0f);
        UIAnimator.instance.DeactivateObjectWithTransition(systemPanel, 0f);

        ChangeButtonChildImageColor(statsButton, Color.gray);
        ChangeButtonChildImageColor(equipementButton, Color.gray);
        ChangeButtonChildImageColor(itemsButton, Color.gray);
        ChangeButtonChildImageColor(lvlUpButton, Color.gray);
        ChangeButtonChildImageColor(mapButton, Color.gray);
        ChangeButtonChildImageColor(systemButton, Color.gray);
    }

    private void ChangeButtonChildImageColor(GameObject button, Color color)
    {
        Image[] images = button.GetComponentsInChildren<Image>(true);
        foreach (var image in images)
        {
            if (image.gameObject != button)
            {
                image.color = color;
                break;
            }
        }
    }
}
