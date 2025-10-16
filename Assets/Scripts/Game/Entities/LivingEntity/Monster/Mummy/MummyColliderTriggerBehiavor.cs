using UnityEngine;

public class MummyColliderTriggerBehavior : MonoBehaviour
{
    public bool collisionActive = false;
    public GameObject LastCollision;

    private void OnTriggerStay2D(Collider2D collision)
    {
        Stats stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player && stats.isVulnerable)
        {
            collisionActive = true;
            LastCollision = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Stats stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            collisionActive = false;
            LastCollision = null;
        }
    }
}
