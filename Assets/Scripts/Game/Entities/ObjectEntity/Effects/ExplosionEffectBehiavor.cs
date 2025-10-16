using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffectBehiavor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f)); // rotation aléatoire 2D
        GetComponent<EntityLight>().SetLightColor(Color.red);
        GetComponent<SoundContainer>().PlaySound("Explosion", 3);
    }


    IEnumerator EffectRoutine()
    {
        GetComponent<EntityLight>().TransitionLightIntensity(3, 2, .25f);
        yield return new WaitForSeconds(.25f);
        GetComponent<EntityLight>().TransitionLightIntensity(0, 0, .25f);
    }
}
