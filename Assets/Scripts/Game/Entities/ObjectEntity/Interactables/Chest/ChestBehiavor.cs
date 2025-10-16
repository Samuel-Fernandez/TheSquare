using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;

public enum CHEST_TYPE
{
    NORMAL,
    DUNGEON_KEY,
    DUNGEON_BOSS_KEY,
    SPECIAL_OBJECT,
}

public class ChestBehiavor : MonoBehaviour
{
    public Item item;
    public string ID;
    public bool state;
    public Sprite openChest;
    public Sprite closedChest;
    public int nbIfSpecialItems;
    public CHEST_TYPE chestType;
    public ToolType toolToUnlock;

    bool openPossible;

    private void Start()
    {
        SaveManager.instance.twoStateContainer.TryGetState(ID, out state);

        // Considéré comme ouvert dans la sauvegarde
        if(state)
        {
            GetComponentInChildren<SpriteRenderer>().sprite = openChest;
            GetComponent<InteractableBehiavor>().canInteract = false;
        }
        else
        {
            GetComponentInChildren<SpriteRenderer>().sprite = closedChest;
        }
    }

    public void Interaction()
    {

        if (chestType == CHEST_TYPE.NORMAL)
        {
            openPossible = (((item is Weapon) || (item is Weapon) || (item is Helmet) || (item is Chestplate) || (item is Leggings) || (item is Boots) || (item is Consumables)) && !Equipement.instance.InventoryFull()) || (item is SpecialItems) || (item is Money);

            if (openPossible)
            {
                SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(ID, true);
                GetComponent<InteractableBehiavor>().canInteract = false;
                GetComponent<InteractableBehiavor>().oneShot = true;
                StartCoroutine(RoutineOpenChest());
            }
            else
            {
                NotificationManager.instance.ShowPopup(LocalizationManager.instance.GetText("UI", "NOTIFICATION_INVENTORY_FULL"));
            }
        }
        else if (chestType == CHEST_TYPE.DUNGEON_KEY)
        {
            SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(ID, true);
            GetComponent<InteractableBehiavor>().canInteract = false;
            GetComponent<InteractableBehiavor>().oneShot = true;
            StartCoroutine(RoutineOpenChest());
        }
        else if (chestType == CHEST_TYPE.DUNGEON_BOSS_KEY)
        {
            SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(ID, true);
            GetComponent<InteractableBehiavor>().canInteract = false;
            GetComponent<InteractableBehiavor>().oneShot = true;
            StartCoroutine(RoutineOpenChest());
        }
        else if (chestType == CHEST_TYPE.SPECIAL_OBJECT)
        {
            SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(ID, true);
            GetComponent<InteractableBehiavor>().canInteract = false;
            GetComponent<InteractableBehiavor>().oneShot = true;
            StartCoroutine(RoutineOpenChest());
        }

    }

    IEnumerator RoutineOpenChest()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Open");
        GetComponent<SoundContainer>().PlaySound("Open", 1);
        yield return new WaitForSeconds(.5f);
        GetComponent<ObjectAnimation>().StopAnimation();
        yield return new WaitForSeconds(.5f);
        GetComponent<SoundContainer>().PlaySound("Reward", 0);

        if(chestType == CHEST_TYPE.NORMAL)
        {
            Item itemInstance = ScriptableObjectUtility.Clone(item);


            if (itemInstance is SpecialItems specialItem)
            {
                specialItem.nb = nbIfSpecialItems;
            }

            NotificationManager.instance.ShowItemResume(itemInstance);
        }
        else if (chestType == CHEST_TYPE.DUNGEON_KEY)
        {
            NotificationManager.instance.ShowItemResume("key");
        }
        else if (chestType == CHEST_TYPE.DUNGEON_BOSS_KEY)
        {
            NotificationManager.instance.ShowItemResume("boss_key");
        }
        else if (chestType == CHEST_TYPE.SPECIAL_OBJECT)
        {
            NotificationManager.instance.ShowItemResume(toolToUnlock);
        }

    }
}
