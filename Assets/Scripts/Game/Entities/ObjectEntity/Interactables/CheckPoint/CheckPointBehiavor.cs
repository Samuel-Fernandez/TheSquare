using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointBehiavor : MonoBehaviour
{
    bool activated = false;

    private void Update()
    {
        if (activated)
            GetComponent<ObjectAnimation>().PlayAnimation("CheckPoint");
    }
    public void ActiveCheckPoint()
    {
        activated = true;
        GetComponent<SoundContainer>().PlaySound("CheckPoint", 1);
        StartCoroutine(RoutineInactive());

        SaveManager.instance.Save();
        NotificationManager.instance.ShowPopup(LocalizationManager.instance.GetText("UI", "NOTIFICATION_SAVE"));
    }

    IEnumerator RoutineInactive()
    {
        GetComponent<EntityLight>().TransitionLightIntensity(3f, 2, 1f);
        yield return new WaitForSeconds(2f);
        GetComponent<EntityLight>().TransitionLightIntensity(.25f, 1, 1f);
        activated = false;
        GetComponent<ObjectAnimation>().StopAllAnimations();

    }
}
