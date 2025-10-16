using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PnjScheduleManager : MonoBehaviour
{
    public static PnjScheduleManager instance;
    public List<PNJSchedule> PNJschedules;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void UpdateSchedules()
    {
        DayHour currentHour = ConvertTimeWorldToDayHour(MeteoManager.instance.timeWorld, 30);

        foreach (PNJSchedule pnjSchedule in PNJschedules)
        {
            bool state = false;

            if(!pnjSchedule.requirement || SaveManager.instance.twoStateContainer.TryGetState(pnjSchedule.requirement.ID, out state))
            {
                GameObject pnj = pnjSchedule.pnj;

                foreach (ScheduleEntry schedule in pnjSchedule.schedule)
                {
                    if (schedule.scene == MeteoManager.instance.actualScene && currentHour >= schedule.beginTime && currentHour < schedule.endTime)
                    {
                        SpawnPNJ(schedule, pnj, schedule.position, schedule.movement);
                    }
                }
            }
            
        }
    }

    private void SpawnPNJ(ScheduleEntry schedule, GameObject pnj, Vector2 position, PNJMovement movement)
    {
        //GameObject existingPNJ = GameObject.Find(pnj.name);
        //if (existingPNJ != null) return;

        GameObject createdPNJ = Instantiate(pnj, position, Quaternion.identity);
        PNJBehiavor behiavor = createdPNJ.GetComponent<PNJBehiavor>();
        behiavor.id = schedule.pnjID;
        behiavor.type = schedule.pnjType;
        behiavor.traderType = schedule.traderType;

        if (behiavor.type == PNJType.QUESTER)
            behiavor.quest = schedule.quests;

        if (movement != null)
            createdPNJ.GetComponent<PNJBehiavor>().movement = movement;
    }

    private DayHour ConvertTimeWorldToDayHour(float timeWorld, float lengthOneDay)
    {
        float dayCycleDuration = lengthOneDay * 60f;
        float timeInCurrentDay = timeWorld % dayCycleDuration;
        float initialTimeInSeconds = 8 * 60 * 60;

        float totalSecondsInDay = initialTimeInSeconds + timeInCurrentDay * (24 * 60 * 60) / dayCycleDuration;
        int hours = Mathf.FloorToInt(totalSecondsInDay / 3600) % 24;

        return (DayHour)hours;
    }
}
