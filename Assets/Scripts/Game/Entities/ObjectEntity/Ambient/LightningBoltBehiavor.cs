using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBoltBehiavor : MonoBehaviour
{
    bool isImpacting;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RoutineLightningBoltImpact());
    }

    IEnumerator RoutineLightningBoltImpact()
    {
        GetComponent<ObjectParticles>().SpawnParticle("LightningBolt", gameObject.transform.position, .3f, 10);

        yield return new WaitForSeconds(3f);
        GetComponent<EntityLight>().TransitionLightIntensity(2, 3, .15f);
        GetComponent<ObjectAnimation>().PlayAnimation("LightningBolt", true);

        yield return new WaitForSeconds(.15f);
        GetComponent<SoundContainer>().PlaySound("LightningBolt", 1);
        isImpacting = true;

        GetComponent<EntityLight>().TransitionLightIntensity(0, 3, .05f);
        yield return new WaitForSeconds(.1f);
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isImpacting)
        {
            if(collision.GetComponent<EntityEffects>() && !collision.GetComponent<LifeManager>())
            {
                collision.GetComponent<EntityEffects>().SetState(10, true);
            }

            if (collision.GetComponent<LifeManager>())
            {
                collision.GetComponent<LifeManager>().TakeDamage(50, gameObject, false);
                
                if(collision.GetComponent<LifeManager>().life > 50)
                    collision.GetComponent<EntityEffects>().SetState(10, true);

            }
        }
    }
}
