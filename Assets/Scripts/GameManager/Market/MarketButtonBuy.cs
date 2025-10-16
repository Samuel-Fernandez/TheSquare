using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketButtonBuy : MonoBehaviour
{
    public Item item;
    public Image sprite;
    public GameObject nbTxt;
    public bool isSelling = false;
    public int nb;

    public void UpdateNumber()
    {
        nbTxt.GetComponent<TextMeshProUGUI>().text = nb.ToString();
    }

    public void SetButton(Item item)
    {
        this.item = item;
        sprite.sprite = item.sprite;

        if (this.item is SpecialItems)
        {
            nbTxt.SetActive(true);
            nbTxt.GetComponent<TextMeshProUGUI>().text = (item as SpecialItems).nb.ToString();
            nb = (item as SpecialItems).nb;
        }
        else
        {
            nbTxt.SetActive(false);
            nb = 0;
        }
    }

    public void ButtonClick()
    {
        MarketManager.instance.ClickButtonBuy(this.gameObject);
    }
}
