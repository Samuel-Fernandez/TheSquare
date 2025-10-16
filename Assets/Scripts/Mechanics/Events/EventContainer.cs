using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PNJContainer
{
    public GameObject pnj;
    public string id;
}

[System.Serializable]
public struct MonsterContainer
{
    public GameObject monster;
    public string id;
    public bool isBattleObject;
}

public enum EventRequirementType
{
    NONE,
    DISCOVERY,
    COMMUNICATION,
    EVENT,
    SPECIAL_OBJECT,
    MONSTER_KILLED
}

[System.Serializable]
public class EventRequirementData
{
    public EventRequirementType requirementType;

    public SceneData sceneData; // Pour DISCOVERY
    public string pnjIDSpoken; // Pour COMMUNICATION
    public EventContainer eventRequired; // Pour EVENT
    public ToolType specialObject; // Pour SPECIAL_OBJECT
    public GameObject monsterRequired; // Pour MONSTER_KILLED
    public int nbMonsterRequired;
}



[CreateAssetMenu(fileName = "new EventContainer", menuName = "Events/EventContainer", order = 1)]
public class EventContainer : ScriptableObject
{
    public List<PNJContainer> pnjContainer;
    public List<MonsterContainer> monsterContainer;
    public GameObject camera;
    public string ID;

    public List<Event> eventsList;
    [Header("Requirements")]
    public List<EventRequirementData> requirements;



    public bool RequirementsGood()
    {
        foreach (var req in requirements)
        {
            switch (req.requirementType)
            {
                case EventRequirementType.NONE:
                    continue;

                case EventRequirementType.DISCOVERY:
                    if (!StatsManager.instance.locationFound.Contains(req.sceneData.sceneID))
                        return false;
                    break;

                case EventRequirementType.COMMUNICATION:
                    if (!StatsManager.instance.pnjSpoken.Contains(req.pnjIDSpoken))
                        return false;
                    break;

                case EventRequirementType.EVENT:
                    if (!SaveManager.instance.twoStateContainer.TryGetState(req.eventRequired.ID, out bool eventGood) || !eventGood)
                        return false;
                    break;

                case EventRequirementType.SPECIAL_OBJECT:
                    bool hasObject = false;
                    foreach (var specialObject in SpecialObjectsManager.instance.availableObjects)
                    {
                        if (specialObject.toolType == req.specialObject)
                        {
                            hasObject = true;
                            break;
                        }
                    }
                    if (!hasObject)
                        return false;
                    break;

                case EventRequirementType.MONSTER_KILLED:
                    bool monsterMatched = false;
                    foreach (var monsterKilled in StatsManager.instance.monsterKilled)
                    {
                        if (monsterKilled.idMonster == req.monsterRequired.name &&
                            monsterKilled.nb >= req.nbMonsterRequired)
                        {
                            monsterMatched = true;
                            break;
                        }
                    }
                    if (!monsterMatched)
                        return false;
                    break;

                default:
                    return false;
            }
        }

        return true;
    }


}
