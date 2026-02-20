using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SealButtonSpecialItems : MonoBehaviour
{
    [Header("Button Properties")]
    public Image itemSprite;
    public TextMeshProUGUI quantity;
    public bool bigButton = false;

    [Header("Button Metadatas")]
    public SpecialItems item;
    public GameObject concernedButton; // Petit bouton qui avait le specialitem concerné


    public void InitButton(SpecialItems item)
    {
        this.item = item;
        itemSprite.gameObject.SetActive(true);
        quantity.gameObject.SetActive(true);
        itemSprite.sprite = item.sprite;
        quantity.text = item.nb.ToString();

        if (bigButton)
            quantity.text = item.numberRequiredForSealing.ToString();
    }

    public void OnClick()
    {
        SealUI.instance.SetSelectedButton(gameObject);
    }

    // A CONTINUER. LORSQUE REMOVE, REPASSE EN ACTIVESELF = TRUE CONCERNANT LE BOUTON SPECIALITEM DE SEALUI
    // Concerne les 4 Gros boutons uniquement.
    public void RemoveItem()
    {
        item = null;
        itemSprite.gameObject.SetActive(false);
        quantity.gameObject.SetActive(false);
        if(concernedButton)
            concernedButton.SetActive(true);
        GetComponent<Button>().enabled = false;
        ResetColor();
    }

    public void ChangeColor()
    {
        var maxEssence = item.essenceComposition
            .OrderByDescending(e => e.percentage)
            .First();

        // Applique la couleur en gardant l'alpha complet
        Color essenceColor = maxEssence.essence.essenceColor;
        essenceColor.a = 1f; // Force l'opacité à 100%
        GetComponent<Image>().color = essenceColor;
    }


    public void ResetColor()
    {
        GetComponent<Image>().color = new Color(64f / 255f, 64f / 255f, 64f / 255f, 1);
    }
}
