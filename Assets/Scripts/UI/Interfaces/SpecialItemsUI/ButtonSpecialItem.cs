using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSpecialItem : MonoBehaviour, ISelectHandler
{
    public SpecialItems item;
    public Image image;
    public TextMeshProUGUI nbText;

    public void Init(SpecialItems item)
    {
        this.item = item;
        this.image.sprite = item.sprite;
        this.nbText.text = item.nb.ToString();
    }

    public void OnSelect(BaseEventData eventData)
    {
        SpecialItemUIManager.instance.ButtonSelected(item);
    }
}
