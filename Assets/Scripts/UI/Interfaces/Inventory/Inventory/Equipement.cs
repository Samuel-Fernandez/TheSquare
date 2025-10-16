using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Equipement : MonoBehaviour
{
    public List<GameObject> equipementSlots;
    public List<GameObject> equippedSlots;
    public GameObject equipementStats;
    public GameObject actualSlot;

    bool wait = false;

    public static Equipement instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        equipementSlots[0].GetComponent<EquipementSlot>().ClickEquipementSlot();
    }

    public void Update()
    {
        if (PlayerManager.instance.playerInputActions.Menu.QuickEquip.triggered && InventoryManager.instance.equipementPanel.activeSelf)
        {
            EquipementSlot slot = actualSlot.GetComponent<EquipementSlot>();

            if (!slot.equippingSlot)
            {
                // Cas inventaire -> équiper
                EquipButton();
            }
            else if (slot.equippingSlot && slot.actualItem != null)
            {
                // Cas déjà équipé -> déséquiper
                if (!InventoryFull())
                {
                    AddItem(slot.actualItem);
                    slot.RemoveItem();
                    slot.ClickEquipementSlot();
                    GetComponent<SoundContainer>().PlayUISound("equip", 1);
                    EventSystem.current.SetSelectedGameObject(actualSlot.gameObject);
                }
                else
                {
                    // Inventaire plein -> son "denied"
                    GetComponent<SoundContainer>().PlayUISound("denied", 1);
                }
            }
        }
    }



    public void EquipButton()
    {
        if(actualSlot.GetComponent<EquipementSlot>().equippingSlot && !InventoryFull())
        {
            AddItem(actualSlot.GetComponent<EquipementSlot>().actualItem);
            actualSlot.GetComponent<EquipementSlot>().RemoveItem();
            actualSlot.GetComponent<EquipementSlot>().ClickEquipementSlot();
            GetComponent<SoundContainer>().PlayUISound("equip", 1);
            EventSystem.current.SetSelectedGameObject(actualSlot.gameObject);

        }
        else
        {
            EquipementSlot actualEquipSlot = actualSlot.GetComponent<EquipementSlot>();
            Item actualItem = actualEquipSlot.actualItem;

            int slotIndex = 0;

            if (actualItem is Helmet helmet)
            {
                EquipItemInSlot(actualEquipSlot, equippedSlots[slotIndex]);
                EventSystem.current.SetSelectedGameObject(equippedSlots[slotIndex].gameObject);
            }

            slotIndex++;
            if (actualItem is Chestplate chestplate)
            {
                EquipItemInSlot(actualEquipSlot, equippedSlots[slotIndex]);
                EventSystem.current.SetSelectedGameObject(equippedSlots[slotIndex].gameObject);

            }

            slotIndex++;
            if (actualItem is Leggings leggings)
            {
                EquipItemInSlot(actualEquipSlot, equippedSlots[slotIndex]);
                EventSystem.current.SetSelectedGameObject(equippedSlots[slotIndex].gameObject);

            }

            slotIndex++;
            if (actualItem is Boots boots)
            {
                EquipItemInSlot(actualEquipSlot, equippedSlots[slotIndex]);
                EventSystem.current.SetSelectedGameObject(equippedSlots[slotIndex].gameObject);

            }

            slotIndex++;
            if (actualItem is Weapon weapon)
            {
                EquipItemInSlot(actualEquipSlot, equippedSlots[slotIndex]);
                EventSystem.current.SetSelectedGameObject(equippedSlots[slotIndex].gameObject);

            }

            actualEquipSlot.ClickEquipementSlot();
        }
        
    }

    private void EquipItemInSlot(EquipementSlot actualEquipSlot, GameObject equippedSlot)
    {
        if(!wait)
        {
            EquipementSlot equippedEquipSlot = equippedSlot.GetComponent<EquipementSlot>();

            if (equippedEquipSlot.actualItem == null)
            {
                equippedEquipSlot.AddItem(actualEquipSlot.actualItem);
                actualEquipSlot.RemoveItem();
            }
            else
            {
                Item temp = equippedEquipSlot.actualItem;
                equippedEquipSlot.AddItem(actualEquipSlot.actualItem);
                actualEquipSlot.AddItem(temp);
            }
            ReorganizeSlots();
            GetComponent<SoundContainer>().PlayUISound("equip", 1);

            StartCoroutine(RoutineWait());

        }

    }

    public void TrashButton()
    {
        if(!wait)
        {
            actualSlot.GetComponent<EquipementSlot>().RemoveItem();
            ReorganizeSlots();
            actualSlot.GetComponent<EquipementSlot>().ClickEquipementSlot();
            GetComponent<SoundContainer>().PlayUISound("denied", 1);

            StartCoroutine(RoutineWait());
            EventSystem.current.SetSelectedGameObject(actualSlot.gameObject);

        }
    }

    public void ConsumeButton()
    {
        if(!wait)
        {
            ConsumableEffect((actualSlot.GetComponent<EquipementSlot>().actualItem as Consumables));
            actualSlot.GetComponent<EquipementSlot>().RemoveItem();
            ReorganizeSlots();
            actualSlot.GetComponent<EquipementSlot>().ClickEquipementSlot();
            GetComponent<SoundContainer>().PlayUISound("UseItem", 1);
            StartCoroutine(RoutineWait());
            EventSystem.current.SetSelectedGameObject(actualSlot.gameObject);

        }

    }

    void ConsumableEffect(Consumables consumable)
    {
        // AJOUTER SON DE SOIN

        switch (consumable.type)
        {
            case ConsumableType.NONE:
                break;
            case ConsumableType.HEALING:
                switch (consumable.typeHealing)
                {
                    case HealingType.NONE:
                        break;
                    case HealingType.PERCENTAGE:
                        // FAIRE HEAL DANS LIFEMANAGER
                        PlayerManager.instance.player.GetComponent<LifeManager>().Heal(Mathf.RoundToInt(PlayerManager.instance.player.GetComponent<Stats>().health * consumable.power));
                        break;
                    case HealingType.LIFE_POINT:
                        PlayerManager.instance.player.GetComponent<LifeManager>().Heal(Mathf.RoundToInt(consumable.power));
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    IEnumerator RoutineWait()
    {
        wait = true;
        yield return new WaitForSecondsRealtime(.5f);
        wait = false;
    }

    public void AddItem(Item item)
    {
        foreach (GameObject slot in equipementSlots)
        {
            if (slot.GetComponent<EquipementSlot>().actualItem == null)
            {
                slot.GetComponent<EquipementSlot>().AddItem(item);
                return;
            }
        }
    }

    public void RemoveItem(Item item)
    {
        foreach (GameObject slot in equipementSlots)
        {
            if (slot.GetComponent<EquipementSlot>().actualItem == item)
            {
                slot.GetComponent<EquipementSlot>().RemoveItem();
                ReorganizeSlots();
                return;
            }
        }

    }

    public bool InventoryFull()
    {
        foreach (GameObject slot in equipementSlots)
        {
            if (slot.GetComponent<EquipementSlot>().actualItem == null)
                return false;
        }
        return true;
    }

    private void ReorganizeSlots()
    {
        for (int i = 0; i < equipementSlots.Count - 1; i++)
        {
            EquipementSlot currentSlot = equipementSlots[i].GetComponent<EquipementSlot>();
            EquipementSlot nextSlot = equipementSlots[i + 1].GetComponent<EquipementSlot>();

            if (currentSlot.actualItem == null && nextSlot.actualItem != null)
            {
                currentSlot.AddItem(nextSlot.actualItem);
                nextSlot.RemoveItem();
            }
        }
    }

    public bool CheckInventory(int nbItems)
    {
        int emptySlots = 0;

        foreach (GameObject slot in equipementSlots)
        {
            if (slot.GetComponent<EquipementSlot>().actualItem == null)
            {
                emptySlots++;
                if (emptySlots >= nbItems)
                    return true;
            }
        }

        return false;
    }
}
