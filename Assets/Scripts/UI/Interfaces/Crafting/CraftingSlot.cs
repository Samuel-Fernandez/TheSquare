using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Interfaces.Crafting
{
    [System.Serializable]
    public struct SpecialItemRequirement
    {
        public SpecialItems specialItem;
        public int amount;
    }

    [System.Serializable]
    public struct EquipmentRequirement
    {
        public Item baseItem; // L'équipement requis de base
        public int amount;
    }

    /// <summary>
    /// Composant attaché à chaque bouton de slot de crafting.
    /// Contient les informations de recette et gère le clic pour afficher la description.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CraftingSlot : MonoBehaviour
    {
        [Header("Item produit")]
        public Item itemToCraft;

        [Header("Ingrédients requis")]
        public List<SpecialItemRequirement> specialItemsRequired = new List<SpecialItemRequirement>();
        public List<EquipmentRequirement> equipmentRequired = new List<EquipmentRequirement>();

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                button = GetComponentInChildren<Button>(true);
            }

            if (button != null)
            {
                button.onClick.AddListener(OnClickSlot);
                Debug.Log($"CraftingSlot '{gameObject.name}' : Composant Button détecté et lié !");
            }
            else
            {
                Debug.LogError($"CraftingSlot '{gameObject.name}' : Aucun composant Button trouvé sur l'objet ou ses enfants !");
            }
        }

        private int lastClickFrame = -1;

        private void OnClickSlot()
        {
            // Bloque les appels multiples lors de la même frame (ex: double liaison inspecteur/code)
            if (Time.frameCount == lastClickFrame) return;
            lastClickFrame = Time.frameCount;

            Debug.Log($"CraftingSlot '{gameObject.name}' cliqué !");
            if (CraftingGridFiller.instance != null)
            {
                CraftingGridFiller.instance.ShowItemDescription(this);
            }
            else
            {
                Debug.LogError("CraftingGridFiller.instance est NULL ! L'instance du gestionnaire n'a pas été initialisée.");
            }
        }

        /// <summary>
        /// Configure le slot de craft et assigne l'icône de l'équipement sur son image enfant.
        /// </summary>
        public void SetupSlot(Item item)
        {
            itemToCraft = item;

            // Cherche le composant Image de l'icône (un enfant du bouton)
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                // On évite de remplacer l'image de fond du bouton principal (le cadre)
                if (img.gameObject != gameObject)
                {
                    if (itemToCraft != null)
                    {
                        img.sprite = itemToCraft.sprite;
                        img.gameObject.SetActive(true);
                    }
                    else
                    {
                        img.gameObject.SetActive(false);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Vérifie si toutes les conditions de craft sont réunies.
        /// </summary>
        public bool CanCraft()
        {
            if (itemToCraft == null) return false;

            // 1. Vérification des SpecialItems
            foreach (var req in specialItemsRequired)
            {
                if (req.specialItem == null) continue;

                SpecialItems playerItem = PlayerManager.instance.GetSpecialItem(req.specialItem.itemId);
                if (playerItem == null || playerItem.nb < req.amount)
                {
                    return false;
                }
            }

            // 2. Vérification des équipements requis dans l'inventaire
            foreach (var req in equipmentRequired)
            {
                if (req.baseItem == null) continue;

                int ownedCount = 0;

                // Vérifier les slots d'équipement normaux
                foreach (GameObject slotGo in Equipement.instance.equipementSlots)
                {
                    EquipementSlot slot = slotGo.GetComponent<EquipementSlot>();
                    if (slot != null && slot.actualItem != null && slot.actualItem.GetID() == req.baseItem.GetID())
                    {
                        ownedCount++;
                    }
                }

                // Vérifier les slots d'équipement actuellement portés
                foreach (GameObject slotGo in Equipement.instance.equippedSlots)
                {
                    EquipementSlot slot = slotGo.GetComponent<EquipementSlot>();
                    if (slot != null && slot.actualItem != null && slot.actualItem.GetID() == req.baseItem.GetID())
                    {
                        ownedCount++;
                    }
                }

                if (ownedCount < req.amount)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
