using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public Slider sliderSound;
    public Slider sliderMusic;

    public TextMeshProUGUI textSound;
    public TextMeshProUGUI textMusic;

    public TextMeshProUGUI labelSound;
    public TextMeshProUGUI labelMusic;
    public TextMeshProUGUI labelLeaveMsg;

    public void ChangeLanguage()
    {
        if (labelSound != null)
            labelSound.text = LocalizationManager.instance.GetText("UI", "SETTING_LABEL_SOUND");

        if (labelMusic != null)
            labelMusic.text = LocalizationManager.instance.GetText("UI", "SETTING_LABEL_MUSIC");
        
        if(labelLeaveMsg != null)
            labelLeaveMsg.text = LocalizationManager.instance.GetText("UI", "SETTING_LABEL_LEAVE");
    }

    private void Start()
    {
        if (sliderMusic && sliderSound)
        {
            sliderSound.value = PlayerPrefs.GetFloat("SoundVolume");
            sliderMusic.value = PlayerPrefs.GetFloat("MusicVolume");
        }

        ChangeLanguage();
    }

    private void Update()
    {
        if (sliderMusic && sliderSound)
        {
            SoundManager.instance.SetMusicVolume(sliderMusic.value);
            SoundManager.instance.SetSoundVolume(sliderSound.value);

            textSound.text = Mathf.Round(sliderSound.value * 100).ToString();
            textMusic.text = Mathf.Round(sliderMusic.value * 100).ToString();
        }
    }
}
