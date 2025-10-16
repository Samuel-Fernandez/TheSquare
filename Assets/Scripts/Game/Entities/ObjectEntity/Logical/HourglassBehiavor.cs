using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HourglassBehiavor : MonoBehaviour
{
    public int seconds = 0;
    bool active;
    public List<GameObject> logicalEntites;

    void ToggleLogicalEntities()
    {
        foreach (GameObject entity in logicalEntites)
        {
            if (active)
            {
                if (entity.GetComponent<DoorBehiavor>())
                    entity.GetComponent<DoorBehiavor>().OpenDoor();

            }
            else
            {
                if (entity.GetComponent<DoorBehiavor>())
                    entity.GetComponent<DoorBehiavor>().CloseDoor();

            }

            if (entity.GetComponent<Spades>())
                StartCoroutine(entity.GetComponent<Spades>().RoutineSpades());
        }
    }

    public void Activation()
    {
        if (!active)
        {
            GetComponent<EventPlayer>().eventContainer = EventGeneratorManager.instance.MoveCamera(new Vector2(0, 0), logicalEntites[0].transform.position - gameObject.transform.position, 1.5f, 1f);
            GetComponent<EventPlayer>().PlayAnimation();
            StartCoroutine(ActivationRoutine());
        }
    }

    IEnumerator ActivationRoutine()
    {
        GetComponent<EntityLight>().TransitionLightIntensity(2, 2, .5f);
        GetComponent<ObjectAnimation>().PlayAnimation("Activation", true);
        GetComponent<SoundContainer>().PlaySound("Activation", 1);
        active = true;

        // Activation
        yield return new WaitForSecondsRealtime(.5f);

        GetComponent<EntityLight>().TransitionLightIntensity(.25f, 1, .5f);


        int elapsedTime = seconds;
        GetComponent<ObjectAnimation>().PlayAnimation("Elapsing", true, false, 1f / seconds);

        ToggleLogicalEntities();
        while (elapsedTime > 0)
        {

            yield return new WaitForSecondsRealtime(1);
            GetComponent<SoundContainer>().PlaySound("Tick", 2);

            elapsedTime--;

        }

        active = false;
        ToggleLogicalEntities();
    }
}
