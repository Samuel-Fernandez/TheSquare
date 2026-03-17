using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossesBehiavor : MonoBehaviour
{
    public bool fightMod;
    public List<GameObject> bossComposition;

    private void Start()
    {
        UpdateState();
    }

    public void SetFightMod(bool fightMod)
    {
        this.fightMod = fightMod;
        UpdateState();
    }

    void UpdateState()
    {
        if (!fightMod)
        {
            if (GetComponent<ObjectAnimation>() != null)
                GetComponent<ObjectAnimation>().PlayAnimation("Idle");

            CompositionAnimation();
        }
        else
        {
            if (GetComponent<MonsterMovement>())
                GetComponent<MonsterMovement>().enabled = fightMod;
            if (GetComponent<SkeletonKingBehiavor>())
                GetComponent<SkeletonKingBehiavor>().enabled = fightMod;
            if (GetComponent<SlimeAberrationBehiavor>())
                GetComponent<SlimeAberrationBehiavor>().InitBoss();
        }
    }

    public void CompositionAnimation()
    {
        foreach (GameObject comp in bossComposition)
        {
            //On suppose des animations simples ici.
            comp.GetComponent<ObjectAnimation>().PlayAnimation("Start");
        }
    }

    private void OnDestroy()
    {
        NotificationManager.instance.ShowSpecialPopUpSquareCoins(
                PlayerManager.instance.player.GetComponent<Stats>().money.ToString(),
                (PlayerManager.instance.player.GetComponent<Stats>().money + GetComponent<Stats>().money).ToString());
        PlayerManager.instance.player.GetComponent<Stats>().money += GetComponent<Stats>().money;
    }
}
