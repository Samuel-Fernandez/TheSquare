using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeleportationSelector : MonoBehaviour
{
    public TextMeshProUGUI textTitle;
    TeleportationAvailable actualTeleportation;

    public void Init(TeleportationAvailable actualTeleportation)
    {
        this.actualTeleportation = actualTeleportation;
        textTitle.text = LocalizationManager.instance.GetText("LOCATION", actualTeleportation.sceneName + "_SCENE");
    }

    public void Teleport()
    {
        TeleportationManager.instance.Teleport(actualTeleportation);
    }
}
