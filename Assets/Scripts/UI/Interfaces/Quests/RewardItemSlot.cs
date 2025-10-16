using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public Item item;
    public Image containerSprite;
    public EquipementStats equipementStat;
    public TextMeshProUGUI nbItems;

    private bool isPointerOver = false;
    private bool isSelected = false;

    public void CreateRewardSlot(Item item, EquipementStats equipementStat, int nb = 0)
    {
        this.item = item;
        this.equipementStat = equipementStat;
        this.containerSprite.sprite = item.sprite;

        if (nb > 0)
        {
            this.nbItems.text = nb.ToString();
            nbItems.gameObject.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        UpdateStatsDisplay();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UpdateStatsDisplay();
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        UpdateStatsDisplay();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        UpdateStatsDisplay();
    }

    private void UpdateStatsDisplay()
    {
        bool show = isPointerOver || isSelected;
        equipementStat.gameObject.SetActive(show);

        if (show)
            equipementStat.WriteStats(item);
    }
}
