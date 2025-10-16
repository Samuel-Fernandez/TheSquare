using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerImpact : MonoBehaviour
{
    int power;
    public void SetHammerImpact(int power)
    {
        this.power = power;
    }

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 1);
        GetComponent<ObjectAnimation>().PlayAnimation("Impact");
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.GetComponent<Stats>() && (collision.GetComponent<Stats>().entityType == EntityType.Monster || collision.GetComponent<Stats>().entityType == EntityType.Boss))
        {
            collision.GetComponent<LifeManager>().TakeDamage(power, PlayerManager.instance.player, false);
        }
        else if(collision.GetComponent<DestroyableBehiavor>())
        {
            collision.GetComponent<DestroyableBehiavor>().DestroyObject(1);
        }
    }
}
