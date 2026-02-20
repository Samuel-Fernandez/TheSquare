using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ItemResumeType
{
    ITEM,
    KEY_ITEM,
    SPECIAL_OBJECT
}

[Serializable]
public class KeyItem
{
    public string id;
    public Sprite sprite;
}

public class ItemResume : MonoBehaviour
{
    Item item;
    KeyItem keyItem;

    public List<KeyItem> keyItemDB = new List<KeyItem>();
    public GameObject bubble;
    public TextMeshProUGUI bubbleTxt;
    public TextMeshProUGUI nbTxt;
    public Image sprite;
    public GameObject equipementDescription;
    public GameObject okButton;
    string currentText;
    float typingSpeed = .05f;

    // Permettra de donner des clés, carte du donjon, autres
    public ItemResumeType resumeType;

    AvailableObjects selectedSpecialObject;

    private void Start()
    {
        Time.timeScale = 0f;
        InventoryManager.instance.canOpenInventory = false;
        QuestManager.instance.canOpenQuests = false;

        if(resumeType != ItemResumeType.ITEM)
            equipementDescription.SetActive(false);

    }

    public void OkButton()
    {
        Debug.Log("OK BUTTON");
        Debug.Log(resumeType);
        GetComponent<SoundContainer>().PlaySound("Ok", 1);
        Time.timeScale = 1f;
        InventoryManager.instance.canOpenInventory = true;
        QuestManager.instance.canOpenQuests = true;

        if (resumeType == ItemResumeType.ITEM)
        {
            bool equipement = (item is Weapon) || (item is Weapon) || (item is Helmet) || (item is Chestplate) || (item is Leggings) || (item is Boots) || (item is Consumables);

            if (equipement)
                Equipement.instance.AddItem(item);

            if (item is Money money)
            {
                PlayerManager.instance.player.GetComponent<Stats>().money += money.value;
            }

            if (item is SpecialItems specialItems)
            {
                PlayerManager.instance.GetSpecialItem(specialItems.GetID()).nb += specialItems.nb;
            }
        }
        else if (resumeType == ItemResumeType.KEY_ITEM)
        {
            switch (keyItem.id)
            {
                case "key":
                    Debug.Log("CLE ICI MEC");
                    DungeonManager.instance.AddKey();
                    break;
                case "boss_key":
                    break;
                default:
                    break;
            }
        }
        else if (resumeType == ItemResumeType.SPECIAL_OBJECT)
        {
            selectedSpecialObject.available = true;
            SaveManager.instance.SaveSpecialObjectsOnly();
        }

        UIAnimator.instance.DeactivateObjectWithTransition(this.gameObject, .5f);
        okButton.GetComponent<Button>().enabled = false;
        Destroy(this.gameObject, .6f);

    }

    public KeyItem GetKeyItem(string id)
    {
        return keyItemDB.Find(keyItem => keyItem.id == id);

    }

    public void Initialize(string id)
    {
        nbTxt.gameObject.SetActive(false);
        keyItem = GetKeyItem(id);
        sprite.sprite = keyItem.sprite;
        currentText = LocalizationManager.instance.GetText("UI", id);
        StartCoroutine(TypeText());
    }

    public void Initialize(ToolType toolType)
    {
        nbTxt.gameObject.SetActive(false);
        selectedSpecialObject = SpecialObjectsManager.instance.GetSpecialObject(toolType);
        sprite.sprite = SpecialObjectsManager.instance.GetSpecialObject(toolType).sprite;
        currentText = LocalizationManager.instance.GetText("SPECIAL_OBJECTS", selectedSpecialObject.id);
        StartCoroutine(TypeText());
    }

    public void Initialize(Item item)
    {
        if (resumeType == ItemResumeType.ITEM)
        {
            bool equipement = (item is Weapon) || (item is Weapon) || (item is Helmet) || (item is Chestplate) || (item is Leggings) || (item is Boots) || (item is Consumables) || (item is SpecialItems);

            Item itemInstance = ScriptableObjectUtility.Clone(item);

            if (equipement)
            {
                itemInstance.GenerateID();
                // Appel de GenerateStats directement sur l'objet itemData
                if (itemInstance is Helmet helmet)
                    helmet.GenerateStats();
                if (itemInstance is Chestplate chestplate)
                    chestplate.GenerateStats();
                if (itemInstance is Leggings leggings)
                    leggings.GenerateStats();
                if (itemInstance is Boots boots)
                    boots.GenerateStats();
                if (itemInstance is Weapon weapon)
                    weapon.GenerateStats();
                if (itemInstance is SpecialItems specialItem)
                {
                    nbTxt.gameObject.SetActive(true);
                    nbTxt.text = specialItem.nb.ToString();
                }
                else
                {
                    nbTxt.gameObject.SetActive(false);
                }

                equipementDescription.SetActive(true);
                equipementDescription.GetComponent<EquipementStats>().WriteStats(itemInstance);

                string itemName = LocalizationManager.instance.GetText("items", itemInstance.GetID() + "_NAME");


                // Problème ne trouve pas le nom
                if (itemInstance is SpecialItems specialItem_2)
                    itemName = specialItem_2.nb.ToString() + " " + itemName;

                currentText = LocalizationManager.instance.GetText("UI", "ITEM_RESUME", itemName);
            }
            else
            {
                equipementDescription.SetActive(false);
            }

            if (item is Money money)
            {
                currentText = LocalizationManager.instance.GetText("UI", "ITEM_RESUME_MONEY", money.value);

                sprite.color = money.color;
            }

            StartCoroutine(TypeText());

            this.item = itemInstance;
            sprite.sprite = this.item.sprite;
        }
            
    }

    private IEnumerator TypeText()
    {
        bubbleTxt.text = "";
        foreach (char letter in currentText.ToCharArray())
        {
            bubbleTxt.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }
}
