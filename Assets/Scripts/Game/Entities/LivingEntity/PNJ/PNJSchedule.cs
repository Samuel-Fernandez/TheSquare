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

    [Tooltip("DÉPRÉCIÉ - Utiliser questsList à la place. Gardé pour rétrocompatibilité")]
    public Quests quests;

    [Tooltip("Liste des quêtes pour ce PNJ (dans l'ordre de déblocage). Uniquement utilisé si le PNJ est de type QUESTER")]
    public List<Quests> questsList;
}

[CreateAssetMenu(fileName = "NewPNJSchedule", menuName = "PNJ/Schedule")]
public class PNJSchedule : ScriptableObject
{
    public GameObject pnj;
    public EventContainer requirement;
    public List<ScheduleEntry> schedule = new List<ScheduleEntry>();
}