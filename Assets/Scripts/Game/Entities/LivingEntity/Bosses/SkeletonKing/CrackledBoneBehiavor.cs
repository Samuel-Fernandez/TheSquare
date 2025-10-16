using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrackledBoneBehiavor : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Apparition", false);
        yield return new WaitForSeconds(.25f);

        GetComponent<ObjectAnimation>().PlayAnimation("Spawn", true);
        GetComponent<SoundContainer>().PlaySound("Spawn", 2);
        yield return new WaitForSeconds(.5f);

        GetComponent<DestroyableBehiavor>().enabled = true;
    }
}
