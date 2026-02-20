using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SealCraftingButton : MonoBehaviour
{
    [Header("Button Properties")]
    public Image itemSprite;
    public TextMeshProUGUI quantity;

    [Header("Button Metadatas")]
    public SpecialItems item;
}
