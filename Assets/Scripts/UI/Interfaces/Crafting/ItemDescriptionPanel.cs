using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Interfaces.Crafting
{
    /// <summary>
    /// Gère l'affichage des détails de l'équipement sélectionné et de ses ingrédients de craft.
    /// Mappe automatiquement ses composants enfants par nom pour éviter les assignations manuelles pénibles.
    /// </summary>
    public class ItemDescriptionPanel : MonoBehaviour
    {
        [Header("Général")]
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private Button craftButton;

        [Header("Conteneurs (Auto-Détection)")]
        [Tooltip("Glissez ici l'objet 'EquipementStats' parent des statistiques")]
        [SerializeField] private Transform statsContainer;

        [Tooltip("Glissez ici l'objet 'Requirements' parent des ingrédients")]
        [SerializeField] private Transform requirementsContainer;

        // Références de statistiques mappées automatiquement
        private GameObject txtDamage;
        private GameObject txtHealPoint;
        private GameObject txtDefense;
        private GameObject txtCritDamage;
        private GameObject txtCritChance;
        private GameObject txtKnockbackPower;
        private GameObject txtKnockbackResistance;
        private GameObject txtSpeed;
        private GameObject txtDragonSkin;
        private GameObject txtRegenRate;
        private GameObject txtNegativeEffectReducer;
        private GameObject txtMineralChance;
        private GameObject txtDodgeChance;
        private GameObject txtDoubleMineralDropChance;
        private GameObject txtVampire;
        private GameObject txtFireAttackChance;
        private GameObject txtIceAttackChance;
        private GameObject txtPoisonAttackChance;
        private GameObject txtDoubleSquareCoinsChances;
        private GameObject txtDropChance;

        [System.Serializable]
        public struct RequirementUI
        {
            public GameObject root;          // Conteneur du slot d'ingrédient
            public Image iconImage;          // Image de l'ingrédient (SpecialItem ou équipement)
            public TextMeshProUGUI txtQuantity; // Texte quantité possédée / requise
        }

        // Liste des slots d'ingrédients mappés automatiquement
        private List<RequirementUI> requirementSlots = new List<RequirementUI>();
        private CraftingSlot currentSlot;

        private void Awake()
        {
            AutoMapFields();
            if (craftButton != null)
            {
                craftButton.onClick.AddListener(OnCraftButtonClick);
            }
        }

        /// <summary>
        /// Mappe automatiquement toutes les statistiques et les conteneurs d'ingrédients.
        /// </summary>
        public void AutoMapFields()
        {
            if (statsContainer != null)
            {
                txtDamage = FindStatObject("DMG");
                txtHealPoint = FindStatObject("HP");
                txtDefense = FindStatObject("DEF");
                txtCritDamage = FindStatObject("CRITD");
                txtCritChance = FindStatObject("CRITC");
                txtKnockbackPower = FindStatObject("KBP");
                txtKnockbackResistance = FindStatObject("KBR");
                txtSpeed = FindStatObject("SPE");
                
                txtDragonSkin = FindStatObject("DRAGON_SKIN");
                txtRegenRate = FindStatObject("REGEN_RATE");
                txtNegativeEffectReducer = FindStatObject("EFFECT_REDUCER");
                txtMineralChance = FindStatObject("MINERAL_CHANCE");
                txtDodgeChance = FindStatObject("DODGE_CHANCE");
                txtDoubleMineralDropChance = FindStatObject("DOUBLE_MINERAL");
                
                txtVampire = FindStatObject("VAMPIRE");
                txtFireAttackChance = FindStatObject("FIRE");
                txtIceAttackChance = FindStatObject("ICE");
                txtPoisonAttackChance = FindStatObject("POISON");
                txtDoubleSquareCoinsChances = FindStatObject("DOUBLE_SQUARE_COIN");
                txtDropChance = FindStatObject("DROP_CHANCE");
            }

            if (requirementsContainer != null)
            {
                requirementSlots.Clear();
                foreach (Transform child in requirementsContainer)
                {
                    RequirementUI ui = new RequirementUI();
                    ui.root = child.gameObject;
                    ui.iconImage = child.GetComponentInChildren<Image>(true);
                    ui.txtQuantity = child.GetComponentInChildren<TextMeshProUGUI>(true);
                    requirementSlots.Add(ui);
                }
            }
        }

        private GameObject FindStatObject(string name)
        {
            Transform child = FindChildRecursive(statsContainer, name);
            if (child == null && name == "DOUBLE_SQUARE_COIN")
            {
                // Sécurité pour la déclinaison au pluriel
                child = FindChildRecursive(statsContainer, "DOUBLE_SQUARE_COINS");
            }
            return child != null ? child.gameObject : null;
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent == null) return null;
            Transform result = parent.Find(name);
            if (result != null) return result;

            for (int i = 0; i < parent.childCount; i++)
            {
                result = FindChildRecursive(parent.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

        public void DisplayItem(CraftingSlot slot)
        {
            this.currentSlot = slot;
            Debug.Log($"[ItemDescriptionPanel] DisplayItem appelé. Slot: {(slot != null ? slot.gameObject.name : "Null")}");
            
            // S'assurer que les champs sont mappés (utile aussi si le panneau commence désactivé)
            if (txtDamage == null && statsContainer != null)
            {
                Debug.Log("[ItemDescriptionPanel] AutoMapFields car les références de statistiques sont nulles.");
                AutoMapFields();
            }

            if (slot == null)
            {
                Debug.LogWarning("[ItemDescriptionPanel] Le slot fourni est null !");
                gameObject.SetActive(false);
                return;
            }

            if (slot.itemToCraft == null)
            {
                Debug.LogWarning($"[ItemDescriptionPanel] Le slot '{slot.gameObject.name}' n'a pas d'item de craft assigné (itemToCraft est null) !");
                gameObject.SetActive(false);
                return;
            }

            Debug.Log($"[ItemDescriptionPanel] Activation de {gameObject.name} et affichage de l'item '{slot.itemToCraft.itemName}' ({slot.itemToCraft.GetID()}).");
            gameObject.SetActive(true);

            // 1. Titre (Nom localisé)
            if (txtTitle != null)
            {
                txtTitle.text = LocalizationManager.instance.GetText("items", slot.itemToCraft.GetID() + "_NAME");
            }

            // 2. Afficher les statistiques de l'équipement
            ResetStats();
            DisplayStats(slot.itemToCraft);

            // 3. Afficher les ingrédients requis
            DisplayRequirements(slot);

            // 4. Activer ou désactiver le bouton craft selon les conditions réunies
            if (craftButton != null)
            {
                craftButton.interactable = slot.CanCraft();
            }
        }

        private void ResetStats()
        {
            if (txtDamage != null) txtDamage.SetActive(false);
            if (txtHealPoint != null) txtHealPoint.SetActive(false);
            if (txtDefense != null) txtDefense.SetActive(false);
            if (txtCritDamage != null) txtCritDamage.SetActive(false);
            if (txtCritChance != null) txtCritChance.SetActive(false);
            if (txtKnockbackPower != null) txtKnockbackPower.SetActive(false);
            if (txtKnockbackResistance != null) txtKnockbackResistance.SetActive(false);
            if (txtSpeed != null) txtSpeed.SetActive(false);
            if (txtDragonSkin != null) txtDragonSkin.SetActive(false);
            if (txtRegenRate != null) txtRegenRate.SetActive(false);
            if (txtNegativeEffectReducer != null) txtNegativeEffectReducer.SetActive(false);
            if (txtMineralChance != null) txtMineralChance.SetActive(false);
            if (txtDodgeChance != null) txtDodgeChance.SetActive(false);
            if (txtDoubleMineralDropChance != null) txtDoubleMineralDropChance.SetActive(false);
            if (txtVampire != null) txtVampire.SetActive(false);
            if (txtFireAttackChance != null) txtFireAttackChance.SetActive(false);
            if (txtIceAttackChance != null) txtIceAttackChance.SetActive(false);
            if (txtPoisonAttackChance != null) txtPoisonAttackChance.SetActive(false);
            if (txtDoubleSquareCoinsChances != null) txtDoubleSquareCoinsChances.SetActive(false);
            if (txtDropChance != null) txtDropChance.SetActive(false);
        }

        private void DisplayStats(Item item)
        {
            if (item is Weapon weapon)
            {
                SetTextVisibility(txtDamage, weapon.baseDamage);
                SetTextVisibility(txtKnockbackPower, weapon.baseKnockbackPower);
                SetTextVisibility(txtCritChance, weapon.baseCritChance * 100, true);
                SetTextVisibility(txtCritDamage, weapon.baseCritDamage * 100, true);
                SetTextVisibility(txtVampire, weapon.vampire * 100, true);
                SetTextVisibility(txtFireAttackChance, weapon.fireAttackChance * 100, true);
                SetTextVisibility(txtIceAttackChance, weapon.iceAttackChance * 100, true);
                SetTextVisibility(txtPoisonAttackChance, weapon.poisonAttackChance * 100, true);
                SetTextVisibility(txtDoubleSquareCoinsChances, weapon.doubleSquareCoinsChances * 100, true);
                SetTextVisibility(txtDropChance, weapon.dropChance * 100, true);
            }
            else if (item is Boots boots)
            {
                SetTextVisibility(txtDefense, boots.baseDefense);
                SetTextVisibility(txtHealPoint, boots.baseLife);
                SetTextVisibility(txtSpeed, boots.baseSpeed * 100, true);
                SetTextVisibility(txtDragonSkin, boots.dragonSkin * 100, true);
                SetTextVisibility(txtRegenRate, boots.regenRate);
                SetTextVisibility(txtNegativeEffectReducer, boots.negativeEffectReducer * 100, true);
                SetTextVisibility(txtMineralChance, boots.mineralChance * 100, true);
                SetTextVisibility(txtDodgeChance, boots.dodgeChance * 100, true);
                SetTextVisibility(txtDoubleMineralDropChance, boots.doubleMineralDropChance * 100, true);
            }
            else if (item is Chestplate chestplate)
            {
                SetTextVisibility(txtDefense, chestplate.baseDefense);
                SetTextVisibility(txtHealPoint, chestplate.baseLife);
                SetTextVisibility(txtDamage, chestplate.baseDamage);
                SetTextVisibility(txtCritChance, chestplate.baseCritChance * 100, true);
                SetTextVisibility(txtCritDamage, chestplate.baseCritDamage * 100, true);
                SetTextVisibility(txtKnockbackResistance, chestplate.baseKnockbackResistance);
                SetTextVisibility(txtKnockbackPower, chestplate.baseKnockbackPower);
                SetTextVisibility(txtDragonSkin, chestplate.dragonSkin * 100, true);
                SetTextVisibility(txtRegenRate, chestplate.regenRate);
                SetTextVisibility(txtNegativeEffectReducer, chestplate.negativeEffectReducer * 100, true);
                SetTextVisibility(txtMineralChance, chestplate.mineralChance * 100, true);
                SetTextVisibility(txtDodgeChance, chestplate.dodgeChance * 100, true);
                SetTextVisibility(txtDoubleMineralDropChance, chestplate.doubleMineralDropChance * 100, true);
            }
            else if (item is Helmet helmet)
            {
                SetTextVisibility(txtDefense, helmet.baseDefense);
                SetTextVisibility(txtHealPoint, helmet.baseLife);
                SetTextVisibility(txtDamage, helmet.baseDamage);
                SetTextVisibility(txtDragonSkin, helmet.dragonSkin * 100, true);
                SetTextVisibility(txtRegenRate, helmet.regenRate);
                SetTextVisibility(txtNegativeEffectReducer, helmet.negativeEffectReducer * 100, true);
                SetTextVisibility(txtMineralChance, helmet.mineralChance * 100, true);
                SetTextVisibility(txtDodgeChance, helmet.dodgeChance * 100, true);
                SetTextVisibility(txtDoubleMineralDropChance, helmet.doubleMineralDropChance * 100, true);
            }
            else if (item is Leggings leggings)
            {
                SetTextVisibility(txtDefense, leggings.baseDefense);
                SetTextVisibility(txtHealPoint, leggings.baseLife);
                SetTextVisibility(txtSpeed, leggings.baseSpeed * 100, true);
                SetTextVisibility(txtKnockbackResistance, leggings.baseKnockbackResistance);
                SetTextVisibility(txtKnockbackPower, leggings.baseKnockbackPower);
                SetTextVisibility(txtDragonSkin, leggings.dragonSkin * 100, true);
                SetTextVisibility(txtRegenRate, leggings.regenRate);
                SetTextVisibility(txtNegativeEffectReducer, leggings.negativeEffectReducer * 100, true);
                SetTextVisibility(txtMineralChance, leggings.mineralChance * 100, true);
                SetTextVisibility(txtDodgeChance, leggings.dodgeChance * 100, true);
                SetTextVisibility(txtDoubleMineralDropChance, leggings.doubleMineralDropChance * 100, true);
            }
        }

        private void SetTextVisibility(GameObject go, float value, bool percentage = false)
        {
            if (go == null) return;
            if (value != 0)
            {
                go.SetActive(true);
                var textComponent = go.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = percentage ? $"{value:F0}%" : (value % 1 == 0 ? $"{value:F0}" : $"{value:F2}");
                }
            }
            else
            {
                go.SetActive(false);
            }
        }

        private void SetTextVisibility(GameObject go, int value)
        {
            if (go == null) return;
            if (value != 0)
            {
                go.SetActive(true);
                var textComponent = go.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = $"{value}";
                }
            }
            else
            {
                go.SetActive(false);
            }
        }

        private void DisplayRequirements(CraftingSlot slot)
        {
            // 1. Calculer le nombre total d'ingrédients requis
            int totalReqs = slot.specialItemsRequired.Count + slot.equipmentRequired.Count;

            // 2. Si on a besoin de plus de slots que ceux existants, on les instancie dynamiquement
            if (totalReqs > requirementSlots.Count && requirementSlots.Count > 0)
            {
                int slotsNeeded = totalReqs - requirementSlots.Count;
                GameObject template = requirementSlots[0].root;
                for (int i = 0; i < slotsNeeded; i++)
                {
                    GameObject newSlot = Instantiate(template, requirementsContainer);
                    RequirementUI ui = new RequirementUI();
                    ui.root = newSlot;
                    ui.iconImage = newSlot.GetComponentInChildren<Image>(true);
                    ui.txtQuantity = newSlot.GetComponentInChildren<TextMeshProUGUI>(true);
                    requirementSlots.Add(ui);
                }
            }

            // Masquer tous les slots par défaut
            foreach (var ui in requirementSlots)
            {
                if (ui.root != null) ui.root.SetActive(false);
            }

            int uiIndex = 0;

            // 3. Charger les SpecialItems requis
            foreach (var req in slot.specialItemsRequired)
            {
                if (uiIndex >= requirementSlots.Count) break;

                var ui = requirementSlots[uiIndex];
                if (ui.root != null)
                {
                    ui.root.SetActive(true);

                    // Image de l'item spécial
                    if (ui.iconImage != null && req.specialItem != null)
                    {
                        ui.iconImage.sprite = req.specialItem.sprite;
                    }

                    // Quantité possédée / requise
                    SpecialItems specialItemData = req.specialItem != null 
                         ? PlayerManager.instance.GetSpecialItem(req.specialItem.itemId) 
                         : null;
                    
                    int owned = specialItemData != null ? specialItemData.nb : 0;
                    if (ui.txtQuantity != null)
                    {
                        ui.txtQuantity.text = $"{owned}/{req.amount}";
                        ui.txtQuantity.color = (owned >= req.amount) ? Color.white : Color.red;
                    }
                }
                uiIndex++;
            }

            // 4. Charger les équipements requis
            foreach (var req in slot.equipmentRequired)
            {
                if (uiIndex >= requirementSlots.Count) break;

                var ui = requirementSlots[uiIndex];
                if (ui.root != null)
                {
                    ui.root.SetActive(true);

                    // Image de l'équipement requis
                    if (ui.iconImage != null && req.baseItem != null)
                    {
                        ui.iconImage.sprite = req.baseItem.sprite;
                    }

                    // Compter combien le joueur en possède
                    int owned = 0;
                    foreach (GameObject slotGo in Equipement.instance.equipementSlots)
                    {
                        EquipementSlot eqSlot = slotGo.GetComponent<EquipementSlot>();
                        if (eqSlot != null && eqSlot.actualItem != null && eqSlot.actualItem.GetID() == req.baseItem.GetID())
                        {
                            owned++;
                        }
                    }
                    foreach (GameObject slotGo in Equipement.instance.equippedSlots)
                    {
                        EquipementSlot eqSlot = slotGo.GetComponent<EquipementSlot>();
                        if (eqSlot != null && eqSlot.actualItem != null && eqSlot.actualItem.GetID() == req.baseItem.GetID())
                        {
                            owned++;
                        }
                    }

                    if (ui.txtQuantity != null)
                    {
                        ui.txtQuantity.text = $"{owned}/{req.amount}";
                        ui.txtQuantity.color = (owned >= req.amount) ? Color.white : Color.red;
                    }
                }
                uiIndex++;
            }
        }

        private void OnCraftButtonClick()
        {
            if (currentSlot == null || currentSlot.itemToCraft == null) return;

            // 1. Vérifier si on possède toutes les ressources et équipements requis
            if (!currentSlot.CanCraft())
            {
                Debug.LogWarning("[ItemDescriptionPanel] Ressources ou équipements requis manquants.");
                return;
            }

            // 2. Vérifier si l'inventaire a encore de la place
            if (Equipement.instance != null && Equipement.instance.InventoryFull())
            {
                if (NotificationManager.instance != null && LocalizationManager.instance != null)
                {
                    NotificationManager.instance.ShowPopup(LocalizationManager.instance.GetText("UI", "NOTIFICATION_INVENTORY_FULL"));
                }
                return;
            }

            // 3. Vider l'inventaire des ressources requises (SpecialItems)
            foreach (var req in currentSlot.specialItemsRequired)
            {
                if (req.specialItem == null) continue;
                SpecialItems playerItem = PlayerManager.instance.GetSpecialItem(req.specialItem.itemId);
                if (playerItem != null)
                {
                    playerItem.nb -= req.amount;
                }
            }

            // 4. Vider l'inventaire des équipements requis
            foreach (var req in currentSlot.equipmentRequired)
            {
                if (req.baseItem == null) continue;
                for (int i = 0; i < req.amount; i++)
                {
                    Item itemToRemove = null;
                    foreach (GameObject slotGo in Equipement.instance.equipementSlots)
                    {
                        EquipementSlot slot = slotGo.GetComponent<EquipementSlot>();
                        if (slot != null && slot.actualItem != null && slot.actualItem.GetID() == req.baseItem.GetID())
                        {
                            itemToRemove = slot.actualItem;
                            break;
                        }
                    }
                    if (itemToRemove == null)
                    {
                        foreach (GameObject slotGo in Equipement.instance.equippedSlots)
                        {
                            EquipementSlot slot = slotGo.GetComponent<EquipementSlot>();
                            if (slot != null && slot.actualItem != null && slot.actualItem.GetID() == req.baseItem.GetID())
                            {
                                itemToRemove = slot.actualItem;
                                break;
                            }
                        }
                    }

                    if (itemToRemove != null)
                    {
                        Equipement.instance.RemoveItem(itemToRemove);
                    }
                }
            }

            // 5. Instancier l'équipement produit, générer ses statistiques de base et l'ajouter à l'inventaire
            Item craftedInstance = ScriptableObjectUtility.Clone(currentSlot.itemToCraft);
            craftedInstance.GenerateID();
            craftedInstance.level = 1;

            if (craftedInstance is Weapon weapon) weapon.GenerateStats();
            else if (craftedInstance is Boots boots) boots.GenerateStats();
            else if (craftedInstance is Chestplate chestplate) chestplate.GenerateStats();
            else if (craftedInstance is Helmet helmet) helmet.GenerateStats();
            else if (craftedInstance is Leggings leggings) leggings.GenerateStats();

            if (Equipement.instance != null)
            {
                Equipement.instance.AddItem(craftedInstance);
            }

            // Jouer le son de réussite via le SoundContainer principal du GameManager (sur PlayerManager)
            if (PlayerManager.instance != null)
            {
                SoundContainer soundContainer = PlayerManager.instance.GetComponent<SoundContainer>();
                if (soundContainer != null)
                {
                    soundContainer.PlayUISound("Craft", 1);
                }
            }

            // 6. Rafraîchir l'affichage du menu et notifier le joueur
            DisplayItem(currentSlot);

            // Refocaliser sur le slot de recette actuel pour ne pas perdre la sélection clavier/manette
            if (UnityEngine.EventSystems.EventSystem.current != null && currentSlot != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(currentSlot.gameObject);
            }

            if (NotificationManager.instance != null && LocalizationManager.instance != null)
            {
                string localizedObtained = LocalizationManager.instance.GetText("UI", "ITEM_RESUME", 
                    LocalizationManager.instance.GetText("items", craftedInstance.GetID() + "_NAME"));
                NotificationManager.instance.ShowPopup(localizedObtained ?? $"Crafted: {craftedInstance.itemName}");
            }
        }
    }
}
