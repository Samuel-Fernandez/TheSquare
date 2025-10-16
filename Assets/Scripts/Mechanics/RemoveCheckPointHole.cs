using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCheckPointHole : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
        {
            collision.gameObject.GetComponent<PlayerController>().safeTeleportation = null;
        }
    }
}
