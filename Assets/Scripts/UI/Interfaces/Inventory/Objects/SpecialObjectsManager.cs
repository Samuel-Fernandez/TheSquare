using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvailableObjects
{
    public ToolType toolType;
    public bool available;
    public bool equiped;
    public Sprite sprite;
    public string id;
}


public class SpecialObjectsManager : MonoBehaviour
{
    public static SpecialObjectsManager instance;

    public List<GameObject> buttonsObject;

    public AvailableObjects actualObject;

    public List<AvailableObjects> availableObjects;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void RemoveAllCursors()
    {
        foreach (var button in buttonsObject)
            button.GetComponent<ObjectButton>().RemoveCursor();
    }

    public void UpdateAllButtons()
    {
        foreach (var button in buttonsObject)
            button.GetComponent<ObjectButton>().UpdateButton();
    }

    public AvailableObjects GetSpecialObject(ToolType toolType)
    {
        foreach (var obj in availableObjects)
            if (obj.toolType == toolType)
                return obj;

        return availableObjects[0];
    }
}

