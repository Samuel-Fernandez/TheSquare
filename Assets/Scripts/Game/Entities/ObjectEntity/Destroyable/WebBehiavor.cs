using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebBehiavor : MonoBehaviour
{
    // On garde en mémoire les entités récemment repoussées pour éviter 
    // d'accumuler les forces et de bugger le moteur physique
    private HashSet<GameObject> knockedEntities = new HashSet<GameObject>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleKnockback(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleKnockback(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleKnockback(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        HandleKnockback(collision.gameObject);
    }

    private void HandleKnockback(GameObject target)
    {
        LifeManager lifeManager = target.GetComponent<LifeManager>();
        
        // Si la cible a de la vie et n'est pas déjà dans notre cooldown
        if (lifeManager != null && !knockedEntities.Contains(target))
        {
            lifeManager.KnockBack(target, 10, this.gameObject);
            
            SoundContainer sound = GetComponent<SoundContainer>();
            if (sound != null) sound.PlaySound("WebSound", 1);
            
            StartCoroutine(KnockbackCooldown(target));
        }
    }

    private IEnumerator KnockbackCooldown(GameObject target)
    {
        knockedEntities.Add(target);
        
        // Temps de recharge avant qu'UNE MÊME toile puisse refaire un knockback
        yield return new WaitForSeconds(0.5f); 
        
        if (knockedEntities.Contains(target))
        {
            knockedEntities.Remove(target);
        }
    }
}
