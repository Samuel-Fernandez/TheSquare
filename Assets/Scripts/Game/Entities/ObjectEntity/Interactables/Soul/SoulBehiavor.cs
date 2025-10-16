using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulBehiavor : MonoBehaviour
{
    public int money;

    void Start()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Normal");
        StartCoroutine(DetectPlayerInRangeRoutine());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Stats>() && collision.GetComponent<Stats>().entityType == EntityType.Player)
        {
            GameoverManager.instance.isSoul = false;
            collision.GetComponent<Stats>().money += money;
            NotificationManager.instance.ShowPopup("+" + money + " Square coins");
        }
    }

    private IEnumerator DetectPlayerInRangeRoutine()
    {
        while (true)
        {
            DetectPlayerInRange();
            yield return new WaitForSeconds(5f);
        }
    }

    private void DetectPlayerInRange()
    {
        // Définir le rayon de recherche
        float detectionRadius = 10f;

        // Trouver tous les objets dans un cercle autour de la position actuelle
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        // Parcourir tous les objets trouvés
        foreach (Collider2D collider in colliders)
        {
            Stats stats = collider.GetComponent<Stats>();
            if (stats != null && stats.entityType == EntityType.Player)
            {
                GetComponent<SoundContainer>().PlaySound("Ambient", 1);
                return; // Quitter la boucle dès qu'un joueur est trouvé
            }
        }
    }
}
