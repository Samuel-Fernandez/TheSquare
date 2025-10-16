using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectBehiavor : MonoBehaviour
{
    public float duration;
    ObjectAnimation objectAnimation;
    public bool destroyWithStopTime;

    // Start is called before the first frame update
    void Start()
    {
        objectAnimation = GetComponent<ObjectAnimation>();

        if (destroyWithStopTime)
            StartCoroutine(RoutineDestroyWithStopTime());

        if(duration > 0)
            Destroy(gameObject, duration);

    }

    // Update is called once per frame
    void Update()
    {
        objectAnimation.PlayAnimation("Effect");
    }

    IEnumerator RoutineDestroyWithStopTime()
    {
        yield return new WaitForSecondsRealtime(duration);
        Destroy(gameObject);
    }
}
