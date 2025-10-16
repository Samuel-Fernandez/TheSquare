using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spades : MonoBehaviour
{
    public bool canHurt;
    public bool toggleSpades; // Pour les activateurs
    public float interval;
    public Sprite spadesUp;
    public Sprite spadesDown;

    private void Start()
    {
        // Applique visuellement l’état initial défini dans l'inspecteur
        UpdateSpadesVisual();

        // Démarre la routine si nécessaire, après avoir affiché le bon état initial
        if (!toggleSpades)
            StartCoroutine(RoutineSpades());
    }

    private void UpdateSpadesVisual()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

        if (canHurt)
            sr.sprite = spadesUp;
        else
            sr.sprite = spadesDown;

        // Met à jour aussi le collider
        GetComponent<BoxCollider2D>().isTrigger = !canHurt;
    }



    private void Update()
    {
        // Mise à jour du collider pour empêcher ou permettre le passage
        GetComponent<BoxCollider2D>().isTrigger = !canHurt;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (canHurt && collision.gameObject.GetComponent<LifeManager>())
        {
            collision.gameObject.GetComponent<LifeManager>().TakeDamage(
                Mathf.RoundToInt(collision.gameObject.GetComponent<Stats>().health / 5),
                gameObject,
                false
            );
            collision.gameObject.GetComponent<LifeManager>().KnockBack(collision.gameObject, 5f, gameObject);
        }
    }

    public IEnumerator RoutineSpades()
    {
        bool oneTime = true;
        while (!toggleSpades || oneTime)
        {
            oneTime = false;

            yield return new WaitForSeconds(interval);

            if (!canHurt)
                GetComponent<SoundContainer>().PlaySound("Spike", 3);
            // Joue l’animation appropriée
            if (canHurt)
                GetComponent<ObjectAnimation>().PlayAnimation("Spades", false, false);
            else
                GetComponent<ObjectAnimation>().PlayAnimation("Spades", false, true);

            yield return new WaitForSeconds(0.3f);
            GetComponent<ObjectAnimation>().StopAnimation();

            // Bascule l'état
            canHurt = !canHurt;
            UpdateSpadesVisual(); // utilise la méthode commune
        }
    }
}
