using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBehiavor : MonoBehaviour
{
    bool isAlly;
    GameObject entityHolding;

    public void SetShield(bool isAlly, GameObject entityHolding)
    {
        this.isAlly = isAlly;
        this.entityHolding = entityHolding;
    }

    public bool GetShieldAlly()
    {
        return isAlly; 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Stats>() || collision.GetComponent<ProjectileBehavior>())
        {
            if (isAlly)
                AllyShield(collision);
            else
                MonsterShield(collision);
        }
    }

    void AllyShield(Collider2D collision)
    {
        if((collision.GetComponent<Stats>() && collision.GetComponent<Stats>().entityType == EntityType.Monster) || collision.GetComponent<ProjectileBehavior>())
        {

            entityHolding.GetComponent<SoundContainer>().PlaySound("ShieldImpact", 2);
            GetComponent<ObjectParticles>().SpawnParticle("HitShield", transform.position);

            if(collision.GetComponent<Stats>())
            {
                collision.GetComponent<LifeManager>().KnockBack(collision.gameObject, collision.GetComponent<Stats>().knockbackPower * 2 * (1 + PlayerManager.instance.shieldKnockback), entityHolding);
                entityHolding.GetComponent<LifeManager>().KnockBack(entityHolding, collision.GetComponent<Stats>().knockbackPower * 2, collision.gameObject);
            }
            else if (collision.GetComponent<ProjectileBehavior>())
            {
                entityHolding.GetComponent<LifeManager>().KnockBack(entityHolding, entityHolding.GetComponent<Stats>().knockbackResistance + 5, collision.gameObject);
                collision.GetComponent<CapsuleCollider2D>().enabled = false;

            }


        }
    }

    public void MonsterShield(Collider2D collision)
    {

        if ((collision.GetComponent<Stats>() && collision.GetComponent<Stats>().entityType == EntityType.Monster) || collision.GetComponent<ProjectileBehavior>())
        {

            entityHolding.GetComponent<SoundContainer>().PlaySound("ShieldImpact", 2);
            GetComponent<ObjectParticles>().SpawnParticle("HitShield", transform.position);

            if (collision.GetComponent<Stats>())
            {
                collision.GetComponent<LifeManager>().KnockBack(collision.gameObject, collision.GetComponent<Stats>().knockbackPower * 2, entityHolding);
                entityHolding.GetComponent<LifeManager>().KnockBack(entityHolding, collision.GetComponent<Stats>().knockbackPower * 2, collision.gameObject);
            }
            else if (collision.GetComponent<ProjectileBehavior>())
            {
                entityHolding.GetComponent<LifeManager>().KnockBack(entityHolding, entityHolding.GetComponent<Stats>().knockbackResistance + 5, collision.gameObject);
                collision.GetComponent<CapsuleCollider2D>().enabled = false;

            }


        }

        /*if (collision.GetComponent<Stats>().entityType == EntityType.Monster)
        {
            entityHolding.GetComponent<SoundContainer>().PlaySound("ShieldImpact", 2);
            GetComponent<ObjectParticles>().SpawnParticle("HitShield", transform.position);

            GetComponent<ObjectParticles>().particles[0].particleSystem.GetComponent<EntityLight>().TransitionLightIntensity(0, 0, .5f);
            collision.GetComponent<LifeManager>().KnockBack(collision.gameObject, collision.GetComponent<Stats>().knockbackPower * 2, entityHolding);
            entityHolding.GetComponent<LifeManager>().KnockBack(entityHolding, collision.GetComponent<Stats>().knockbackPower * 2, collision.gameObject);
        }*/
    }
}
