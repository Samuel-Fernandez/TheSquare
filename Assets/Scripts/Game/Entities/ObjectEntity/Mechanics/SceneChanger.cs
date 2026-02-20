using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public string scene;
    public Vector2 newPosition;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Stats>() && collision.GetComponent<Stats>().entityType == EntityType.Player)
        {
            if(ScenesManager.instance.canTeleportPlayer)
                ScenesManager.instance.ChangeSceneObject(scene, newPosition);
        }
    }
}
