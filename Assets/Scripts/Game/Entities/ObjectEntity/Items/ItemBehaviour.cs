using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    public Item itemData;
    private SpriteRenderer spriteRenderer;
    private Item itemInstance;

    private Color color1;
    private Color color2;
    private float colorTransitionTime = 2f; // Duration of the color transition in seconds

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = itemData.sprite;

        if (itemData is Money money)
            spriteRenderer.color = money.color;
    }

    private void Start()
    {
        itemInstance = ScriptableObjectUtility.Clone(itemData);

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

        // Start color transition if item has an enchantment
        if (itemInstance is Weapon wpn && wpn.enchant != WEAPON_ENCHANT.NULL)
        {
            StartColorTransition(wpn.colorEnchant1, wpn.colorEnchant2);
        }
        else if (itemInstance is Boots bt && bt.armorEnchant != ARMOR_ENCHANT.NULL)
        {
            StartColorTransition(bt.colorEnchant1, bt.colorEnchant2);
        }
        else if (itemInstance is Chestplate chp && chp.armorEnchant != ARMOR_ENCHANT.NULL)
        {
            StartColorTransition(chp.colorEnchant1, chp.colorEnchant2);
        }
        else if (itemInstance is Helmet hlm && hlm.armorEnchant != ARMOR_ENCHANT.NULL)
        {
            StartColorTransition(hlm.colorEnchant1, hlm.colorEnchant2);
        }
        else if (itemInstance is Leggings lgg && lgg.armorEnchant != ARMOR_ENCHANT.NULL)
        {
            StartColorTransition(lgg.colorEnchant1, lgg.colorEnchant2);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Stats>() && collision.GetComponent<Stats>().entityType == EntityType.Player)
        {
            if (itemData is Money money)
            {
                collision.GetComponent<Stats>().money += money.value;
                Destroy(gameObject);
                GetComponent<SoundContainer>().PlaySound("getMoney", 1);
            }
            else if (itemData is SpecialItems special)
            {
                GetComponent<SoundContainer>().PlaySound("getItem", 1);
                HandleSpecialItem(special);
                Destroy(gameObject);
            }
            // CHANGEMENT DES CONSUMABLES, SE CONSOMMENT DIRECTEMENT
            else if (itemData is Consumables consumables)
            {
                if (consumables.typeHealing == HealingType.LIFE_POINT)
                    collision.GetComponent<LifeManager>().Heal((int)consumables.power);
                else
                    collision.GetComponent<LifeManager>().Heal((int)(collision.GetComponent<Stats>().health * consumables.power));
                Destroy(gameObject);
            }
            else
            {
                if (!Equipement.instance.InventoryFull())
                {
                    Equipement.instance.AddItem(itemInstance);
                    NotificationManager.instance.ShowPopup(LocalizationManager.instance.GetText("UI", "NOTIFICATION_GET_TEXT") + " " + LocalizationManager.instance.GetText("items", itemData.GetID() + "_NAME"));
                    Destroy(gameObject);
                    GetComponent<SoundContainer>().PlaySound("getItem", 1);
                }
                else
                {
                    NotificationManager.instance.ShowPopup(LocalizationManager.instance.GetText("UI", "NOTIFICATION_INVENTORY_FULL"));
                }
            }
        }
    }

    private void HandleSpecialItem(SpecialItems special)
    {
        var playerItem = PlayerManager.instance.GetSpecialItem(special.GetID());
        playerItem.nb += special.nb;

        switch (special.type)
        {
            case SpecialItemType.ARROW:
                NotificationManager.instance.ShowSpecialPopUpArrow(
                    (playerItem.nb - special.nb).ToString(),
                    playerItem.nb.ToString());
                break;

            default:
                PlayerManager.instance.GetSpecialItem(special.GetID()).nb += 1;
                string localized = LocalizationManager.instance.GetText("items", special.itemId + "_NAME");
                NotificationManager.instance.ShowPopup(localized);
                break;
        }
    }


    private void StartColorTransition(Color start, Color end)
    {
        color1 = start;
        color2 = end;
        StartCoroutine(TransitionColors());
    }

    private IEnumerator TransitionColors()
    {
        while (true)
        {
            yield return StartCoroutine(LerpColor(color1, color2, colorTransitionTime));
            yield return StartCoroutine(LerpColor(color2, color1, colorTransitionTime));
        }
    }

    private IEnumerator LerpColor(Color start, Color end, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            spriteRenderer.color = Color.Lerp(start, end, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        spriteRenderer.color = end;
    }
}
