using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipementSlot : MonoBehaviour
{
    public Item actualItem;
    public Image buttonSprite;
    public GameObject cursor;
    public bool equippingSlot = false;

    private Color color1;
    private Color color2;
    private float colorTransitionTime = 2f; // Duration of the color transition in seconds
    private float transitionTimer = 0f; // Timer to keep track of transition progress
    private bool transitioningToColor2 = true; // Direction of transition
    private bool isTransitioning = false; // To track if a transition is ongoing

    private void Start()
    {
        if (actualItem != null)
        {
            buttonSprite.gameObject.SetActive(true);
            buttonSprite.sprite = actualItem.sprite;
        }
        else
        {
            buttonSprite.gameObject.SetActive(false);
            buttonSprite.sprite = null;
        }
    }

    private void Update()
    {
        // Handle color transition for buttonSprite
        if (isTransitioning)
        {
            HandleColorTransition();
        }
    }

    public void ClickEquipementSlot()
    {
        Equipement.instance.equipementStats.GetComponent<EquipementStats>().WriteStats(actualItem);

        foreach (GameObject slot in Equipement.instance.equipementSlots)
            slot.GetComponent<EquipementSlot>().HideCursor();

        foreach (GameObject slot in Equipement.instance.equippedSlots)
        {
            slot.GetComponent<EquipementSlot>().HideCursor();
        }

        ShowCursor();
        Equipement.instance.actualSlot = gameObject;
    }

    public void AddItem(Item item)
    {
        actualItem = item;
        buttonSprite.sprite = actualItem.sprite;
        buttonSprite.gameObject.SetActive(true);

        // Start color transition if item has an enchantment
        if (item is Weapon weapon && weapon.enchant != WEAPON_ENCHANT.NULL)
        {
            StartColorTransition(weapon.colorEnchant1, weapon.colorEnchant2);
        }
        else if (item is Boots boots && boots.armorEnchant != ARMOR_ENCHANT.NULL)
        {
            StartColorTransition(boots.colorEnchant1, boots.colorEnchant2);
        }
        else if (item is Chestplate chestplate && chestplate.armorEnchant != ARMOR_ENCHANT.NULL)
        {
            StartColorTransition(chestplate.colorEnchant1, chestplate.colorEnchant2);
        }
        else if (item is Helmet helmet && helmet.armorEnchant != ARMOR_ENCHANT.NULL)
        {
            StartColorTransition(helmet.colorEnchant1, helmet.colorEnchant2);
        }
        else if (item is Leggings leggings && leggings.armorEnchant != ARMOR_ENCHANT.NULL)
        {
            StartColorTransition(leggings.colorEnchant1, leggings.colorEnchant2);
        }
        else
        {
            isTransitioning = false;
            buttonSprite.color = Color.white;
        }
    }

    public void RemoveItem()
    {
        actualItem = null;
        buttonSprite.sprite = null;
        buttonSprite.gameObject.SetActive(false);
        isTransitioning = false; // Stop color transition if item is removed
    }

    public void ShowCursor()
    {
        cursor.SetActive(true);
    }

    public void HideCursor()
    {
        cursor.SetActive(false);
    }

    private void StartColorTransition(Color start, Color end)
    {
        color1 = start;
        color2 = end;
        transitionTimer = 0f; // Reset timer for new transition
        transitioningToColor2 = true; // Start transitioning to color2
        isTransitioning = true; // Begin transition
    }

    private void HandleColorTransition()
    {
        if (isTransitioning)
        {
            // Update the transition timer
            transitionTimer += Time.unscaledDeltaTime;
            float t = Mathf.PingPong(transitionTimer / colorTransitionTime, 1); // Calculate transition progress

            // Lerp color based on transition progress
            buttonSprite.color = Color.Lerp(transitioningToColor2 ? color1 : color2, transitioningToColor2 ? color2 : color1, t);

            // Check if the transition has completed
            if (transitionTimer >= colorTransitionTime)
            {
                transitionTimer = 0f; // Reset timer
                transitioningToColor2 = !transitioningToColor2; // Toggle transition direction
            }
        }
    }
}
