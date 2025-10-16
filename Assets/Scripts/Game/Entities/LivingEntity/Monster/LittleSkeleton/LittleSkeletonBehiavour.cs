using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleSkeletonBehiavour : MonoBehaviour
{
    MonsterMovement monsterMovement;
    bool isUnderground = false;
    bool doAction;


    void Start()
    {
        monsterMovement = GetComponent<MonsterMovement>();
        StartCoroutine(ActionsRoutine());
        StartCoroutine(SpawnParticlesRoutine());
    }

    IEnumerator SpawnParticlesRoutine()
    {   
        while (true)
        {
            if (isUnderground)
            {
                GetComponent<ObjectParticles>().SpawnParticle("Underground", gameObject.transform.position, Quaternion.identity);
            }

            yield return new WaitForSecondsRealtime(Random.Range(.05f, .2f));
        }
        
    }


    void Update()
    {
        Animations(monsterMovement.GetDirection());

        // Hitbox activée quand pas sous terre
        GetComponent<CircleCollider2D>().enabled = !isUnderground;
    }

    public void Animations(Vector3 direction)
    {
        if(!doAction)
        {
            if (isUnderground)
            {
                GetComponent<ObjectAnimation>().PlayAnimation("Underground");

            }
            else if (direction.y > 0)
            {
                GetComponent<ObjectAnimation>().PlayAnimation("Up");

            }
            else if (direction.y < 0)
            {
                GetComponent<ObjectAnimation>().PlayAnimation("Down");
            }
            else if (direction.x != 0)
            {
                GetComponent<ObjectAnimation>().PlayAnimation("Side");
            }
        }
       
    }

    IEnumerator ActionsRoutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        doAction = true;
        GetComponent<ObjectAnimation>().PlayAnimation("Outside");
        monsterMovement.UpdateSpeed(0);

        yield return new WaitForSecondsRealtime(1);

        monsterMovement.UpdateSpeed(1);
        doAction = false;

        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(1, 10));

            // Se met sous terre, prend 1 seconde
            doAction = true;
            GetComponent<ObjectAnimation>().PlayAnimation("Outside", false, true);
            monsterMovement.UpdateSpeed(0);

            yield return new WaitForSecondsRealtime(1);

            // Est sous terre
            doAction = false;
            isUnderground = true;
            monsterMovement.UpdateSpeed(2);

            yield return new WaitForSecondsRealtime(Random.Range(1, 20));

            // Tente de sortir de sous terre
            doAction = true;
            GetComponent<ObjectAnimation>().PlayAnimation("Outside");
            monsterMovement.UpdateSpeed(0);

            // Vérifie les collisions avant de sortir
            yield return new WaitForSecondsRealtime(1);

            if (CanExitUnderground())
            {
                monsterMovement.UpdateSpeed(1);
                doAction = false;
                isUnderground = false;
            }
            else
            {
                // Si collision, reste sous terre
                doAction = false;
                isUnderground = true;
            }
        }
    }

    private bool CanExitUnderground()
    {
        // Taille du cercle pour détecter les collisions
        float detectionRadius = 0.15f;

        // Position du monstre
        Vector2 position = transform.position;

        // Tous les objets en collision dans un rayon donné
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, detectionRadius);

        foreach (Collider2D collider in colliders)
        {
            // Ignore les collisions avec le joueur
            if (!collider.GetComponent<Stats>())
                return false;
        }

        // Aucun obstacle détecté, le monstre peut sortir
        return true;
    }


}
