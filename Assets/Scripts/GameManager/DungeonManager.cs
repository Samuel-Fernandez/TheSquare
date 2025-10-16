using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class Dungeon
{
    public string title;
    public int nbKeys;
    public bool isFinished;
    public bool bossDoorUnlocked;
}

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;

    public bool isInDungeon;
    [NonSerialized] public Dungeon actualDungeon = null;
    
    public List<Dungeon> dungeonDB = new List<Dungeon>();

    public GameObject uiDungeon;

    public TextMeshProUGUI keyText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        // Si dans un donjon et pas de donjon ajouté
        if (MeteoManager.instance.actualScene.sceneType == SceneType.DUNGEON && actualDungeon == null)
        {
            actualDungeon = GetDungeon(MeteoManager.instance.actualScene.dungeonName);
            isInDungeon = true;
            uiDungeon.SetActive(true);
        }
        // Si pas dans un donjon 
        else if(MeteoManager.instance.actualScene.sceneType != SceneType.DUNGEON && actualDungeon != null)
        {
            actualDungeon = null;
            isInDungeon = false;
            uiDungeon.SetActive(false);
        }
    }

    public Dungeon GetDungeon(string title)
    {
        // Recherche du donjon dans la base de données en fonction du titre
        Dungeon foundDungeon = dungeonDB.Find(dungeon => dungeon.title == title);

        if (foundDungeon != null)
        {
            return foundDungeon;
        }
        else
        {
            Debug.LogWarning($"Dungeon with title '{title}' not found. Probably forgot to add a title in SceneData");
            return null;
        }
    }


    public void AddKey()
    {
        if (actualDungeon != null)
        {
            // Augmente le nombre de clés du donjon
            actualDungeon.nbKeys++;

            // Met à jour l'interface utilisateur
            UpdateUI();

            SaveManager.instance.SaveDungeonsOnly();
        }
        else
        {
            Debug.LogWarning("No dungeon to add a key to.");
        }
    }

    public void RemoveKey()
    {
        if (actualDungeon != null)
        {
            if (actualDungeon.nbKeys > 0)
            {
                // Diminue le nombre de clés du donjon
                actualDungeon.nbKeys--;

                // Met à jour l'interface utilisateur
                UpdateUI();

                SaveManager.instance.SaveDungeonsOnly();
            }
            else
            {
                Debug.LogWarning("No keys left to remove.");
            }
        }
        else
        {
            Debug.LogWarning("No dungeon to remove a key from.");
        }
    }

    public void UpdateUI()
    {
        if (keyText != null && actualDungeon != null)
        {
            keyText.text = $"{actualDungeon.nbKeys}";
        }
    }



}
