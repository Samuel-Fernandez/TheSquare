using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager instance;

    private string currentLanguage = "en";
    private Dictionary<string, Dictionary<string, string>> localizationData = new Dictionary<string, Dictionary<string, string>>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        LoadLocalizationData();
    }

    private void LoadLocalizationData()
    {
        localizationData.Clear(); // Reset pour éviter les conflits

        string[] files = { "UI", "pnj_text", "items", "effects", "SKILLS", "QUEST", "LOCATION", "MONSTER", "SIGN", "CINEMATIC", "SPECIAL_OBJECTS", "BOSS"};

        foreach (string file in files)
        {
            TextAsset textAsset = Resources.Load<TextAsset>($"Localization/{currentLanguage}/{file}");
            if (textAsset != null)
            {
                Dictionary<string, string> data = JsonUtility.FromJson<LocalizationFile>(textAsset.text).ToDictionary();
                if (data != null)
                {
                    localizationData[file] = data;
                }
                else
                {
                    Debug.LogWarning($"Failed to parse localization file: {file}");
                }
            }
            else
            {
                Debug.LogWarning($"Localization file not found: {file}");
            }
        }
    }

    public string GetText(string category, string key, params object[] args)
    {
        if (localizationData.ContainsKey(category) && localizationData[category].ContainsKey(key))
        {
            string rawText = localizationData[category][key];
            return string.Format(rawText, args);
        }
        else
        {
            Debug.LogWarning($"Localization key not found: {category}/{key}");
            return null;
        }
    }


    public List<string> GetTexts(string category, string key)
    {
        if (localizationData.ContainsKey(category) && localizationData[category].ContainsKey(key))
        {
            string value = localizationData[category][key];
            return new List<string>(value.Split('|'));
        }
        else
        {
            Debug.LogWarning($"Localization key not found: {category}/{key}");
            return null;
        }
    }


    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }

    public void SetCurrentLanguage(string language)
    {
        currentLanguage = language;
        LoadLocalizationData();
    }
}

[System.Serializable]
public class LocalizationFile
{
    public List<LocalizationEntry> entries;

    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        foreach (var entry in entries)
        {
            dictionary[entry.key] = entry.value;
        }
        return dictionary;
    }
}

[System.Serializable]
public class LocalizationEntry
{
    public string key;
    public string value;
}
