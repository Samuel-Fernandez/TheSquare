using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryStats : MonoBehaviour
{
    public GameObject txtHP;
    public GameObject txtStrength;
    public GameObject txtDefense;
    public GameObject txtSpeed;
    public GameObject txtKnockbackPower;
    public GameObject txtKnockbackResistance;
    public GameObject txtCritD;
    public GameObject txtCritC;

    public GameObject txtQuiver;
    public GameObject txtSquareCoins;
    
    public GameObject txtIron;
    public GameObject txtSilver;
    public GameObject txtDiamond;
    public GameObject txtAntiMatter;
    public GameObject txtSquareBlock;

    public void Update()
    {
        txtHP.GetComponentInChildren<TextMeshProUGUI>().text = PlayerManager.instance.player.GetComponent<Stats>().health.ToString();
        txtStrength.GetComponentInChildren<TextMeshProUGUI>().text = PlayerManager.instance.player.GetComponent<Stats>().strength.ToString();
        txtDefense.GetComponentInChildren<TextMeshProUGUI>().text = PlayerManager.instance.player.GetComponent<Stats>().defense.ToString();
        txtSpeed.GetComponentInChildren<TextMeshProUGUI>().text = PlayerManager.instance.player.GetComponent<Stats>().speed.ToString("F2");
        txtKnockbackPower.GetComponentInChildren<TextMeshProUGUI>().text = PlayerManager.instance.player.GetComponent<Stats>().knockbackPower.ToString("F2");
        txtKnockbackResistance.GetComponentInChildren<TextMeshProUGUI>().text = PlayerManager.instance.player.GetComponent<Stats>().knockbackResistance.ToString("F2");
        txtCritD.GetComponentInChildren<TextMeshProUGUI>().text = (PlayerManager.instance.player.GetComponent<Stats>().critDamage * 100).ToString("F2") + " %";
        txtCritC.GetComponentInChildren<TextMeshProUGUI>().text = (PlayerManager.instance.player.GetComponent<Stats>().critChance * 100).ToString("F2") + " %";

        if(PlayerManager.instance.GetSpecialItem(SpecialItemType.ARROW) != null)
            txtQuiver.GetComponentInChildren<TextMeshProUGUI>().text = PlayerManager.instance.GetSpecialItem(SpecialItemType.ARROW).nb.ToString();
        txtSquareCoins.GetComponentInChildren<TextMeshProUGUI>().text = PlayerManager.instance.player.GetComponent<Stats>().money.ToString();

        MineralsUpdate();
    }

    void MineralsUpdate()
    {
        if (PlayerManager.instance.GetSpecialItem(SpecialItemType.IRON) != null)
            UpdateText(txtIron, PlayerManager.instance.GetSpecialItem(SpecialItemType.IRON).nb);

        if (PlayerManager.instance.GetSpecialItem(SpecialItemType.SILVER) != null)
            UpdateText(txtSilver, PlayerManager.instance.GetSpecialItem(SpecialItemType.SILVER).nb);

        if (PlayerManager.instance.GetSpecialItem(SpecialItemType.DIAMOND) != null)
            UpdateText(txtDiamond, PlayerManager.instance.GetSpecialItem(SpecialItemType.DIAMOND).nb);

        if (PlayerManager.instance.GetSpecialItem(SpecialItemType.ANTIMATTER) != null)
            UpdateText(txtAntiMatter, PlayerManager.instance.GetSpecialItem(SpecialItemType.ANTIMATTER).nb);

        if (PlayerManager.instance.GetSpecialItem(SpecialItemType.SQUAREBLOCK) != null)
            UpdateText(txtSquareBlock, PlayerManager.instance.GetSpecialItem(SpecialItemType.SQUAREBLOCK).nb);
    }

    void UpdateText(GameObject textObject, int value)
    {
        var textComponent = textObject.GetComponentInChildren<TextMeshProUGUI>();
        if (value <= 0)
        {
            textObject.gameObject.SetActive(false);
        }
        else
        {
            textObject.gameObject.SetActive(true);
            textComponent.text = value.ToString();
        }
    }

}
