using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardBone : MonoBehaviour
{
    public int damage = 3;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
        {
            collision.gameObject.GetComponent<LifeManager>().TakeDamage(damage, this.gameObject, false);
            Destroy(gameObject);
        }
    }

    IEnumerator SpawnRoutine()
    {
        GetComponent<SoundContainer>().PlaySound("Spawn", 1);
        GetComponent<ObjectAnimation>().PlayAnimation("Spawn", true);

        GetComponent<EntityLight>().TransitionLightIntensity(1.4f, 3, 2f);

        // Activer immédiatement le collider après la transition de lumière
        yield return new WaitForSeconds(1); // Attendre 0.4 secondes après le spawn

        GetComponent<Collider2D>().isTrigger = false; // Le collider devient actif ici

        // Pause entre les transitions
        yield return new WaitForSeconds(.4f);

        // Deuxième transition d'intensité
        GetComponent<EntityLight>().TransitionLightIntensity(0.25f, 1, 0.3f);

        // Attente avant la destruction de l'objet
        yield return new WaitForSeconds(Random.Range(3, 6));

        Destroy(gameObject); // Détruire l'objet après un certain délai
    }

}
