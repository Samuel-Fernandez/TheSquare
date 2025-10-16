using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialItemUIManager : MonoBehaviour
{

    public static SpecialItemUIManager instance;

    public GameObject specialItemPrefab;
    public GameObject itemResume;
    public GameObject specialItemContainer;

    List<GameObject> buttons = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void DestroyButtons()
    {
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }

        buttons.Clear();
    }


    // Reset de l'UI
    public void InitUI()
    {
        DestroyButtons();

        if(PlayerManager.instance.runtimeSpecialItems.Count > 0)
        {
            itemResume.gameObject.SetActive(false);

            foreach (var item in PlayerManager.instance.runtimeSpecialItems)
            {
                if(item.nb > 0)
                {
                    GameObject buttonInstance = Instantiate(specialItemPrefab, specialItemContainer.transform);
                    // Appliquer l'item correspondant au bouton (Avec son nombre)
                    buttons.Add(buttonInstance);
                    buttonInstance.GetComponent<ButtonSpecialItem>().Init(item);
                }
            }
        }
        else
        {
            // AFFICHER ICI QUE POUR L'INSTANT, AUCUN ITEM
        }
        
        
    }

    // A appeler lorsqu'un bouton a été sélectionné (et pas cliqué)
    public void ButtonSelected(SpecialItems item)
    {
        itemResume.SetActive(true);
        itemResume.GetComponent<EquipementStats>().WriteStats(item);
    }
}

// FAIRE LE PREFAB DU BOUTON
