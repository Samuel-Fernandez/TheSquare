using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShyronHandBehiavor : MonoBehaviour
{
    int strength;

    public void InitHand(int strength)
    {
        this.strength = strength;
    }

    private void Start()
    {
        StartCoroutine(HandLifeRoutine());
        GetComponent<SoundContainer>().PlaySound("Opening", 2);
        GetComponent<EntityLight>().TransitionLightIntensity(3, 3, .5f);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Stats stat = collision.gameObject.GetComponent<Stats>();

        if (stat != null && stat.entityType == EntityType.Player)
        {
            collision.gameObject.GetComponent<LifeManager>().TakeDamage(strength, this.gameObject, false);
        }
    }

    IEnumerator HandLifeRoutine()
    {
        yield return new WaitForSecondsRealtime(1);

        GetComponent<ObjectAnimation>().PlayAnimation("Attack");
        GetComponent<Collider2D>().isTrigger = false;
        GetComponent<SoundContainer>().PlaySound("HandOut", 2);

        yield return new WaitForSecondsRealtime(1);

        Destroy(gameObject);
    }
}
