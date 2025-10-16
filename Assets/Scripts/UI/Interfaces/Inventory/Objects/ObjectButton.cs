using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ToolType
{
    NONE,
    BOW,
    PICKAXE,
    LIGHT,
    HAMMER,
    DARK_MEDAL,
    SHIELD
}

public class ObjectButton : MonoBehaviour
{
    public ToolType toolType;
    public GameObject image;
    public GameObject cursor;
    public MyButton button;

    private Color lightGreen = new Color(0.58f, 0.72f, 0.59f); // Hex: 94B896
    private Color darkGreen = new Color(0.48f, 0.56f, 0.49f); // Hex: 7B907C
    private Color darkGray = new Color(0.30f, 0.30f, 0.30f); // Hex: 4D4D4D

    public void UpdateButton()
    {
        bool isAvailable = ObjectAvailable();
        image.SetActive(isAvailable);

        if (isAvailable)
        {
            GetComponent<Image>().color = lightGreen; // Image du bouton en vert clair
        }
        else
        {
            GetComponent<Image>().color = darkGray; // Image du bouton en gris foncé
        }

        cursor.SetActive(SpecialObjectsManager.instance.GetSpecialObject(toolType).equiped);
        image.GetComponent<Image>().sprite = SpecialObjectsManager.instance.GetSpecialObject(toolType).sprite;
    }

    public bool ObjectAvailable()
    {
        return SpecialObjectsManager.instance.GetSpecialObject(toolType).available;
    }

    public void SelectObject()
    {
        if (ObjectAvailable())
        {
            SpecialObjectsManager.instance.RemoveAllCursors();
            cursor.SetActive(true);
            SpecialObjectsManager.instance.GetSpecialObject(toolType).equiped = true;
            SpecialObjectsManager.instance.actualObject = SpecialObjectsManager.instance.GetSpecialObject(toolType);
            GetComponent<Image>().color = darkGreen; // Image du bouton en vert foncé
        }
    }

    public void RemoveCursor()
    {
        cursor.SetActive(false);

        SpecialObjectsManager.instance.GetSpecialObject(toolType).equiped = false;
        
        bool isAvailable = ObjectAvailable();
        if (isAvailable)
        {
            GetComponent<Image>().color = lightGreen; // Image du bouton en vert clair
        }
        else
        {
            GetComponent<Image>().color = darkGray; // Image du bouton en gris foncé
        }
    }
}
