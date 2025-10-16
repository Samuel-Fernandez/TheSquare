using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipementUpgradeButton : MonoBehaviour
{
    Item actualItem;
    public Image buttonImage;

    public void SetButton(Item actualItem)
    {
        this.actualItem = actualItem;
        buttonImage.sprite = actualItem.sprite;
    }

    public void ClickButton()
    {
        AnvilUpgradeManager.instance.Select(gameObject);
    }

    public Item GetItem()
    {
        return actualItem;
    }
}
