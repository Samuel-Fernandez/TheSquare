using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ScheduleEntry
{
    public SceneData scene;
    public Vector2 position;
    public DayHour beginTime;
    public DayHour endTime;
    public PNJMovement movement;

    public string pnjID;
    public TraderType traderType;
    public PNJType pnjType;

    [Tooltip("Uniquement utilisé si le PNJ est de type QUESTER")]
    public Quests quests;
}

[CreateAssetMenu(fileName = "NewPNJSchedule", menuName = "PNJ/Schedule")]
public class PNJSchedule : ScriptableObject
{
    public GameObject pnj;
    public EventContainer requirement;
    public List<ScheduleEntry> schedule = new List<ScheduleEntry>();
}

