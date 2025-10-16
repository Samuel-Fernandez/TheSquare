using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignBehiavor : MonoBehaviour
{
    public string ID;

    public void ShowText()
    {
        NotificationManager.instance.ShowBubble(LocalizationManager.instance.GetTexts("SIGN", ID));
    }
}
