using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MainTitle : MonoBehaviour
{
    public GameObject UI;
    public GameObject cosmetic;
    public GameObject whiteTransition;

    public TextMeshProUGUI startText;
    public TextMeshProUGUI leaveText;

    const string mainTitle = "MainTitle";

    private void Start()
    {
        if (startText != null)
            startText.text = LocalizationManager.instance.GetText("UI", "MAIN_MENU_START");

        if (leaveText != null)
            leaveText.text = LocalizationManager.instance.GetText("UI", "MAIN_MENU_LEAVE");

        StartCoroutine(DelayAnimation());
    }

    IEnumerator DelayAnimation()
    {
        yield return new WaitForSecondsRealtime(6f);
        cosmetic.SetActive(false);
        UI.SetActive(true);
        SoundManager.instance.PlayMusic(mainTitle);
        yield return new WaitForSecondsRealtime(7f);
        whiteTransition.SetActive(false);
    }
}
