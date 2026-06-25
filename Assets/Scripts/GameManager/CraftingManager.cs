using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameManager
{
    /// <summary>
    /// Gère l'ouverture, la fermeture, la navigation par onglets et la localisation
    /// du menu de crafting. Empêche la collision d'affichage avec l'inventaire.
    /// </summary>
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager instance;

        [Header("Structure UI")]
        public GameObject craftingMenu;
        public TextMeshProUGUI txtTitle;
        public TextMeshProUGUI txtCraftButton;

        [Header("Onglets de Panneaux")]
        public GameObject weaponPanel;
        public GameObject helmetPanel;
        public GameObject chestplatePanel;
        public GameObject leggingsPanel;
        public GameObject bootsPanel;

        [Header("Boutons d'Onglets")]
        public GameObject weaponButton;
        public GameObject helmetButton;
        public GameObject chestplateButton;
        public GameObject leggingsButton;
        public GameObject bootsButton;

        public bool canOpenCrafting = true;

        private float defaultFixedDeltaTime;
        private CraftTab currentTab = CraftTab.WEAPON;

        private enum CraftTab
        {
            WEAPON,
            HELMET,
            CHESTPLATE,
            LEGGINGS,
            BOOTS
        }

        private List<CraftTab> tabOrder = new List<CraftTab>
        {
            CraftTab.WEAPON,
            CraftTab.HELMET,
            CraftTab.CHESTPLATE,
            CraftTab.LEGGINGS,
            CraftTab.BOOTS
        };

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            defaultFixedDeltaTime = Time.fixedDeltaTime;

            // Relier dynamiquement les clics souris sur les boutons d'onglets
            BindButton(weaponButton, OpenTabWeapon);
            BindButton(helmetButton, OpenTabHelmet);
            BindButton(chestplateButton, OpenTabChestplate);
            BindButton(leggingsButton, OpenTabLeggings);
            BindButton(bootsButton, OpenTabBoots);

            // Ouvrir l'onglet par défaut (Armes)
            OpenTab(CraftTab.WEAPON);
        }

        private void BindButton(GameObject buttonGo, UnityEngine.Events.UnityAction action)
        {
            if (buttonGo != null)
            {
                Button btn = buttonGo.GetComponent<Button>();
                if (btn == null)
                {
                    btn = buttonGo.GetComponentInChildren<Button>(true);
                }
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(action);
                }
            }
        }

        private void Update()
        {
            if (craftingMenu != null && craftingMenu.activeSelf)
            {
                // Navigation via la manette ou le clavier
                if (PlayerManager.instance != null && PlayerManager.instance.playerInputActions != null)
                {
                    if (PlayerManager.instance.playerInputActions.Menu.InventoryRight.triggered)
                    {
                        NavigateCrafting(true);
                    }
                    if (PlayerManager.instance.playerInputActions.Menu.InventoryLeft.triggered)
                    {
                        NavigateCrafting(false);
                    }
                }

                // Fermeture via la touche Escape ou la touche Pause (Start/Options de la manette)
                if (Input.GetKeyDown(KeyCode.Escape) ||
                    (PlayerManager.instance != null && PlayerManager.instance.playerInputActions != null && PlayerManager.instance.playerInputActions.Menu.Pause.triggered))
                {
                    CloseCrafting();
                }
            }
        }

        /// <summary>
        /// Ouvre le menu de crafting, suspend le temps et gère les conflits avec l'inventaire.
        /// </summary>
        public void OpenCrafting()
        {
            if (!canOpenCrafting) return;

            // Fermer l'inventaire s'il est actuellement ouvert pour éviter les superpositions d'écrans
            if (InventoryManager.instance != null && InventoryManager.instance.inventory != null && InventoryManager.instance.inventory.activeSelf)
            {
                InventoryManager.instance.ToggleInventory(true);
            }

            if (craftingMenu != null && !craftingMenu.activeSelf)
            {
                UIAnimator.instance.ActivateObjectWithTransition(craftingMenu, 0.2f);

                // Mettre le jeu en pause
                Time.timeScale = 0f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;

                if (QuestManager.instance != null)
                {
                    QuestManager.instance.canOpenQuests = false;
                }

                // Appliquer la localisation
                LocalizeUI();

                // Restaurer l'onglet actif actuel
                OpenTab(currentTab);
            }
        }

        /// <summary>
        /// Ferme le menu de crafting et restaure le temps normal du jeu.
        /// </summary>
        public void CloseCrafting()
        {
            if (craftingMenu != null && craftingMenu.activeSelf)
            {
                // Reprendre le temps normal du jeu
                Time.timeScale = 1f;
                Time.fixedDeltaTime = defaultFixedDeltaTime;

                UIAnimator.instance.DeactivateObjectWithTransition(craftingMenu, 0.2f);

                if (QuestManager.instance != null)
                {
                    QuestManager.instance.canOpenQuests = true;
                }
            }
        }

        private void LocalizeUI()
        {
            if (LocalizationManager.instance != null)
            {
                if (txtTitle != null)
                {
                    string titleText = LocalizationManager.instance.GetText("UI", "CRAFTING_MENU_TITLE");
                    txtTitle.text = !string.IsNullOrEmpty(titleText) ? titleText : "Crafting";
                }
                if (txtCraftButton != null)
                {
                    string craftText = LocalizationManager.instance.GetText("UI", "CRAFT_BUTTON");
                    txtCraftButton.text = !string.IsNullOrEmpty(craftText) ? craftText : "Craft";
                }
            }
        }

        private void NavigateCrafting(bool toRight)
        {
            int index = tabOrder.IndexOf(currentTab);
            if (index == -1) index = 0;

            index = (index + (toRight ? 1 : -1) + tabOrder.Count) % tabOrder.Count;
            OpenTab(tabOrder[index]);
        }

        // Méthodes publiques liées aux événements de clics sur les boutons d'onglets de l'UI
        public void OpenTabWeapon() => OpenTab(CraftTab.WEAPON);
        public void OpenTabHelmet() => OpenTab(CraftTab.HELMET);
        public void OpenTabChestplate() => OpenTab(CraftTab.CHESTPLATE);
        public void OpenTabLeggings() => OpenTab(CraftTab.LEGGINGS);
        public void OpenTabBoots() => OpenTab(CraftTab.BOOTS);

        private void OpenTab(CraftTab tab)
        {
            CloseAllTabs();
            currentTab = tab;

            switch (tab)
            {
                case CraftTab.WEAPON:
                    if (weaponPanel != null) UIAnimator.instance.ActivateObjectWithTransition(weaponPanel, 0.2f);
                    if (weaponButton != null)
                    {
                        ChangeButtonChildImageColor(weaponButton, Color.white);
                        SelectButton(weaponButton);
                    }
                    break;

                case CraftTab.HELMET:
                    if (helmetPanel != null) UIAnimator.instance.ActivateObjectWithTransition(helmetPanel, 0.2f);
                    if (helmetButton != null)
                    {
                        ChangeButtonChildImageColor(helmetButton, Color.white);
                        SelectButton(helmetButton);
                    }
                    break;

                case CraftTab.CHESTPLATE:
                    if (chestplatePanel != null) UIAnimator.instance.ActivateObjectWithTransition(chestplatePanel, 0.2f);
                    if (chestplateButton != null)
                    {
                        ChangeButtonChildImageColor(chestplateButton, Color.white);
                        SelectButton(chestplateButton);
                    }
                    break;

                case CraftTab.LEGGINGS:
                    if (leggingsPanel != null) UIAnimator.instance.ActivateObjectWithTransition(leggingsPanel, 0.2f);
                    if (leggingsButton != null)
                    {
                        ChangeButtonChildImageColor(leggingsButton, Color.white);
                        SelectButton(leggingsButton);
                    }
                    break;

                case CraftTab.BOOTS:
                    if (bootsPanel != null) UIAnimator.instance.ActivateObjectWithTransition(bootsPanel, 0.2f);
                    if (bootsButton != null)
                    {
                        ChangeButtonChildImageColor(bootsButton, Color.white);
                        SelectButton(bootsButton);
                    }
                    break;
            }
        }

        private void CloseAllTabs()
        {
            if (weaponPanel != null) UIAnimator.instance.DeactivateObjectWithTransition(weaponPanel, 0f);
            if (helmetPanel != null) UIAnimator.instance.DeactivateObjectWithTransition(helmetPanel, 0f);
            if (chestplatePanel != null) UIAnimator.instance.DeactivateObjectWithTransition(chestplatePanel, 0f);
            if (leggingsPanel != null) UIAnimator.instance.DeactivateObjectWithTransition(leggingsPanel, 0f);
            if (bootsPanel != null) UIAnimator.instance.DeactivateObjectWithTransition(bootsPanel, 0f);

            if (weaponButton != null) ChangeButtonChildImageColor(weaponButton, Color.gray);
            if (helmetButton != null) ChangeButtonChildImageColor(helmetButton, Color.gray);
            if (chestplateButton != null) ChangeButtonChildImageColor(chestplateButton, Color.gray);
            if (leggingsButton != null) ChangeButtonChildImageColor(leggingsButton, Color.gray);
            if (bootsButton != null) ChangeButtonChildImageColor(bootsButton, Color.gray);
        }

        private void SelectButton(GameObject button)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(button);
            }
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
}
