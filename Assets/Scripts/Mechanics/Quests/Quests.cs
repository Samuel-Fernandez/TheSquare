using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestRequirement
{
    NONE,
    DISCOVERY,
    COMMUNICATION,
    EVENT,
    SPECIAL_OBJECT,
    MONSTER_KILLED,
    QUEST
}

public enum QuestCompletionCondition
{
    KILL_MONSTER,
    RESOURCES,
    DISCOVERY,
    COMMUNICATION,
    EVENT
}

public enum QuestReward
{
    NONE,
    SQUARE_COINS,
    ITEMS,
    EQUIPEMENT,
}

[System.Serializable]
public class ItemReward
{
    public SpecialItems rewardItem;
    public int nb;
}

[System.Serializable]
public class MonsterObjective
{
    public GameObject monster;
    public int nb;

    // Permet de savoir le nombre de monstres tués depuis stats quand la quête a été acceptée
    public int nbMonsterKilledSinceAccepted = 0;
}

[System.Serializable]
public class ResourcesObjective
{
    public SpecialItems item;
    public int nb;
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/New Quest", order = 0)]
public class Quests : ScriptableObject
{
    [Header("Quest Information")]
    public string id;                       // A Sauver
    public SceneData location;              // A Sauver
    // Se remplit tout seul
    public string pnjID;

    [Header("Requirements")]
    public QuestRequirement requirement;
    public SceneData sceneData; // Assurez-vous que SceneData est sérialisable
    public string pnjIDSpoken;
    public EventContainer eventRequired; // Assurez-vous que EventContainer est sérialisable
    public ToolType specialObject; // Assurez-vous que ToolType est sérialisable
    public GameObject monsterRequired;
    public int nbMonsterRequired;
    public Quests questsRequired;

    [Header("Completion Conditions")]
    public QuestCompletionCondition completionCondition;
    public List<MonsterObjective> monsterObjectiveList;
    public SceneData sceneDataObjective;
    public string pnjIDObjective;
    public EventContainer eventObjective;
    public List<ResourcesObjective> resourcesObjective;

    [Header("Rewards")]
    public QuestReward reward;
    public int nbSquareCoins;
    public List<ItemReward> rewardSpecialItem;
    public List<Item> rewardEquipement;    // A sauver

    // A utiliser uniquement si de type monstre à tuer
    public void UpdateMonsterProgression()
    {
        foreach (MonsterKilled monsterKilled in StatsManager.instance.monsterKilled)
        {
            foreach (var objective in monsterObjectiveList)
            {
                if (monsterKilled.idMonster == objective.monster.name)
                {
                    objective.nbMonsterKilledSinceAccepted = monsterKilled.nb;
                }
            }
        }
    }


}
