using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MonsterKilled
{
    public string idMonster;
    public int nb;
}

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance;

    public List<MonsterKilled> monsterKilled = new List<MonsterKilled>();
    public List<string> locationFound = new List<string>();
    public List<string> pnjSpoken = new List<string>();


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void MonsterKilled(string monsterID)
    {
        // Nettoyer le nom du monstre pour enlever les suffixes ajoutés par Unity
        int index = monsterID.IndexOf(" (");
        if (index > 0)
        {
            monsterID = monsterID.Substring(0, index); // Supprime tout après " ("
        }

        for (int i = 0; i < monsterKilled.Count; i++)
        {
            if (monsterKilled[i].idMonster == monsterID)
            {
                // Incrémente le compteur
                MonsterKilled updatedMonster = monsterKilled[i];
                updatedMonster.nb++;
                monsterKilled[i] = updatedMonster;

                return;
            }
        }

        // Si le monstre n'est pas trouvé, on l'ajoute
        MonsterKilled newMonster = new MonsterKilled { idMonster = monsterID, nb = 1 };
        monsterKilled.Add(newMonster);

    }


    public void LocationFound(string locationName)
    {
        if (!locationFound.Contains(locationName))
        {
            locationFound.Add(locationName);
        }
    }

    public void PNJSpoken(string pnjName)
    {
        if (!pnjSpoken.Contains(pnjName))
        {
            pnjSpoken.Add(pnjName);
        }
    }




}
