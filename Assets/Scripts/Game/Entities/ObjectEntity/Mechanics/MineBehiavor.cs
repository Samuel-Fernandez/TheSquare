using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBehiavor : MonoBehaviour
{
    public string id;
    public List<GameObject> slots = new List<GameObject>();

    public GameObject littleIron;
    public GameObject bigIron;
    public GameObject littleSilver;
    public GameObject bigSilver;
    public GameObject littleDiamond;
    public GameObject bigDiamond;
    public GameObject littleAntimatter;
    public GameObject bigAntimatter;
    public GameObject littleSquareBlock;
    public GameObject bigSquareBlock;

    private void Start()
    {
        foreach (Transform child in transform)
        {
            slots.Add(child.gameObject);
        }

        Mine tempMine = MineManager.instance.FindMineById(id);

        if (tempMine == null)
        {
            tempMine = new Mine(id, slots.Count);
            MineManager.instance.mineList.Add(tempMine);
            tempMine.SetMineralList();
        }

        if (tempMine.idSlots.Count > 0)
            PlaceMineral();

    }

    void Update()
    {
        CheckMineralPresence();
    }

    void PlaceMineral()
    {
        Mine mine = MineManager.instance.FindMineById(id);
        if (mine == null) return;

        for (int i = 0; i < slots.Count; i++)
        {
            if (i >= mine.idSlots.Count) continue; // Éviter les erreurs d'index

            GameObject mineralToInstantiate = null;

            switch (mine.idSlots[i])
            {
                case "BigIron":
                    mineralToInstantiate = bigIron;
                    break;
                case "LittleIron":
                    mineralToInstantiate = littleIron;
                    break;
                case "BigSilver":
                    mineralToInstantiate = bigSilver;
                    break;
                case "LittleSilver":
                    mineralToInstantiate = littleSilver;
                    break;
                case "BigDiamond":
                    mineralToInstantiate = bigDiamond;
                    break;
                case "LittleDiamond":
                    mineralToInstantiate = littleDiamond;
                    break;
                case "BigAntimatter":
                    mineralToInstantiate = bigAntimatter;
                    break;
                case "LittleAntimatter":
                    mineralToInstantiate = littleAntimatter;
                    break;
                case "BigSquareBlock":
                    mineralToInstantiate = bigSquareBlock;
                    break;
                case "LittleSquareBlock":
                    mineralToInstantiate = littleSquareBlock;
                    break;
                default:
                    break;
            }

            if (mineralToInstantiate != null)
            {
                Instantiate(mineralToInstantiate, slots[i].transform.position, Quaternion.identity, slots[i].transform);
            }
        }
    }

    void CheckMineralPresence()
    {
        Mine mine = MineManager.instance.FindMineById(id);
        if (mine == null)
        {
            Debug.LogError($"Mine with id {id} not found in MineManager.");
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i >= mine.idSlots.Count) continue; // Éviter les erreurs d'index

            if (slots[i].transform.childCount == 0)
            {
                mine.idSlots[i] = null; // Ou "" selon votre logique
            }
        }
    }
}
