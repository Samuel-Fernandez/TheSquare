using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChanger : MonoBehaviour
{
    public int levelChange;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<ObjectPerspective>() != null)
        {
            // Changer le level de l'objet principal
            collision.gameObject.GetComponent<ObjectPerspective>().level = levelChange;

            // Changer aussi le level de tous ses enfants qui ont ObjectPerspective
            foreach (ObjectPerspective childPerspective in collision.gameObject.GetComponentsInChildren<ObjectPerspective>(true))
            {
                childPerspective.level = levelChange;
            }
        }
    }
}
