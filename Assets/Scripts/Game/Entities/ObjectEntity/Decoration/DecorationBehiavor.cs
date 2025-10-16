using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DecorationType
{
    NONE,
    TORCH,
    CAMPFIRE,
}
public class DecorationBehiavor : MonoBehaviour
{
    public DecorationType type;

    public float minRandomSound;
    public float maxRandomSound;
    public float animationSpeed;
    public int pitch;

    private void Start()
    {

        switch (type)
        {
            case DecorationType.NONE:
                break;
            case DecorationType.TORCH:
                StartCoroutine(TorchRoutine());
                animationSpeed = Random.Range(.9f, 1.1f);
                GetComponent<ObjectParticles>().SpawnParticle("Smoke", this.gameObject, 0.5f, default, 0, 1);
                GetComponent<ObjectParticles>().SpawnParticle("Flame", this.gameObject, 0.25f, default, 0, 1.5f);
                break;
            case DecorationType.CAMPFIRE:
                StartCoroutine(TorchRoutine());
                animationSpeed = Random.Range(.5f, 1f);
                GetComponent<ObjectParticles>().SpawnParticle("Smoke", this.gameObject, 0.25f, default, 0, 1f);
                GetComponent<ObjectParticles>().SpawnParticle("Flame", this.gameObject, 0.15f, default, 0, 0f);
                break;
            default:
                break;
        }


        GetComponent<ObjectAnimation>().PlayAnimation("Animation", false, false, animationSpeed);
        StartCoroutine(SoundRoutine());


    }

    public IEnumerator TorchRoutine()
    {
        while (true)
        {
            float increase = Random.Range(.01f, .5f);
            float decrease = Random.Range(.01f, .5f);

            GetComponent<EntityLight>().TransitionLightIntensity(Random.Range(.5f, 2), Random.Range(10, 13), increase);
            yield return new WaitForSeconds(increase);
            GetComponent<EntityLight>().TransitionLightIntensity(Random.Range(.1f, .5f), Random.Range(5, 10), decrease);
            yield return new WaitForSeconds(decrease);
        }

    }

    public IEnumerator SoundRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(minRandomSound, maxRandomSound));
            GetComponent<SoundContainer>().PlaySound("Sound", pitch);
        }
        
    }
}
