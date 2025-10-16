using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelWall : MonoBehaviour
{
    public int level;
    private HashSet<Collider2D> ignoredColliders = new HashSet<Collider2D>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ObjectPerspective objPerspective = collision.gameObject.GetComponent<ObjectPerspective>();
        if (objPerspective != null)
        {
            if (objPerspective.level != this.level)
            {
                Ignore(collision.collider);
            }
        }
    }

    private void Update()
    {
        // Vérifie les collisions déjà ignorées si leur level a changé
        List<Collider2D> toRemove = new List<Collider2D>();
        foreach (Collider2D col in ignoredColliders)
        {
            if (col == null)
            {
                toRemove.Add(col);
                continue;
            }

            ObjectPerspective obj = col.GetComponent<ObjectPerspective>();
            if (obj != null && obj.level == this.level)
            {
                Physics2D.IgnoreCollision(col, GetComponent<Collider2D>(), false); // Réactiver la collision
                toRemove.Add(col);
            }
        }

        foreach (var col in toRemove)
        {
            ignoredColliders.Remove(col);
        }
    }

    private void Ignore(Collider2D collider)
    {
        Physics2D.IgnoreCollision(collider, GetComponent<Collider2D>());
        ignoredColliders.Add(collider);
    }
}
