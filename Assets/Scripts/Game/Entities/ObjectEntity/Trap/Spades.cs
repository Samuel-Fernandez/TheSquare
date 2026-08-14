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
        // Applique visuellement l��tat initial d�fini dans l'inspecteur
        UpdateSpadesVisual();

        // D�marre la routine si n�cessaire, apr�s avoir affich� le bon �tat initial
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

        // Met � jour aussi le collider
        GetComponent<BoxCollider2D>().isTrigger = !canHurt;
    }



    private void Update()
    {
        // Mise � jour du collider pour emp�cher ou permettre le passage
        GetComponent<BoxCollider2D>().isTrigger = !canHurt;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (canHurt && collision.gameObject.GetComponent<LifeManager>())
        {
            collision.gameObject.GetComponent<LifeManager>().TakeDamage(
                Mathf.RoundToInt(collision.gameObject.GetComponent<Stats>().health / 5),
                gameObject,
                false,
                1,
                true
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
            // Joue l�animation appropri�e
            if (canHurt)
                GetComponent<ObjectAnimation>().PlayAnimation("Spades", false, false);
            else
                GetComponent<ObjectAnimation>().PlayAnimation("Spades", false, true);

            yield return new WaitForSeconds(0.3f);
            GetComponent<ObjectAnimation>().StopAnimation();

            // Bascule l'�tat
            canHurt = !canHurt;
            UpdateSpadesVisual(); // utilise la m�thode commune
        }
    }
}
