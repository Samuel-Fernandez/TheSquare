using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mine
{
    public string id;
    public List<string> idSlots;
    public int timer;
    public int nbSlots;

    public Mine(string id, int nbSlots)
    {
        this.id = id;
        this.timer = 1800;
        this.nbSlots = nbSlots;
        idSlots = new List<string>(new string[nbSlots]);
    }

    public void SetMineralList()
    {
        for (int i = 0; i < idSlots.Count; i++)
        {
            if (string.IsNullOrEmpty(idSlots[i]))
            {
                string mineralType = ChooseMineral();
                idSlots[i] = mineralType;
            }
        }
    }

    string ChooseMineral()
    {
        int random = Random.Range((0 + (int)(PlayerLevels.instance.lvlLuck * (1 + PlayerManager.instance.mineralChance / 2) * 100)), 10001);
        bool ironChance = (random >= 8000 && random < 9000);
        bool silverChance = (random >= 9000 && random < 9500);
        bool diamondChance = (random >= 9500 && random < 9950);
        bool antimatterChance = (random >= 9950 && random < 9995);
        bool squareBlockChance = (random >= 9995);

        if (ironChance)
        {
            return Random.Range(0, 10) > 7 ? "BigIron" : "LittleIron";
        }
        else if (silverChance)
        {
            return Random.Range(0, 10) > 7 ? "BigSilver" : "LittleSilver";
        }
        else if (diamondChance)
        {
            return Random.Range(0, 10) > 7 ? "BigDiamond" : "LittleDiamond";
        }
        else if (antimatterChance)
        {
            return Random.Range(0, 10) > 7 ? "BigAntimatter" : "LittleAntimatter";
        }
        else if (squareBlockChance)
        {
            return Random.Range(0, 10) > 7 ? "BigSquareBlock" : "LittleSquareBlock";
        }

        return "";
    }
}


public class MineManager : MonoBehaviour
{
    public List<Mine> mineList = new List<Mine>();

    public static MineManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(TimerMineRoutine());
    }

    public Mine FindMineById(string id)
    {
        return mineList.Find(mine => mine.id == id);
    }

    IEnumerator TimerMineRoutine()
    {
        while (true)
        {
            foreach (var mine in mineList)
            {
                if (mine.timer > 0)
                {
                    mine.timer--;
                }
                else
                {
                    mine.SetMineralList();
                    mine.timer = 1800;
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }
}
