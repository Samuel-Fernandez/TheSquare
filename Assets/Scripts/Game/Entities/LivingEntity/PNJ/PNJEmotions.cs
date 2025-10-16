using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PNJEmotions : MonoBehaviour
{
    public GameObject angryEmotion;
    public GameObject embarassedEmotion;
    public GameObject inLoveEmotion;
    public GameObject speakingEmotion;

    private GameObject currentEmotionInstance;

    public void Emotion(PnjEmotions emotion)
    {
        if (currentEmotionInstance != null)
        {
            Destroy(currentEmotionInstance); // Détruire l'émotion précédente si elle existe
        }

        GameObject prefab = null;
        switch (emotion)
        {
            case PnjEmotions.IN_LOVE:
                prefab = inLoveEmotion;
                break;
            case PnjEmotions.ANGRY:
                prefab = angryEmotion;
                break;
            case PnjEmotions.EMBARASSED:
                prefab = embarassedEmotion;
                break;
            case PnjEmotions.SPEAKING:
                prefab = speakingEmotion;
                break;
            case PnjEmotions.JUMP:
                Jump();
                return;
        }

        if (prefab != null)
        {
            Vector2 position = (Vector2)transform.position + Vector2.up; // Position relative en 2D
            currentEmotionInstance = Instantiate(prefab, position, Quaternion.identity, transform); // Instancier en tant qu'enfant du PNJ
        }
    }

    public void Jump()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(JumpCoroutine());
        }
    }

    private IEnumerator JumpCoroutine()
    {
        PNJMovementAnimation movementScript = GetComponent<PNJMovementAnimation>();

        if (movementScript != null)
        {
            movementScript.enabled = false; // Désactiver le script de mouvement
        }

        GetComponent<SoundContainer>().PlaySound("jump", 1);

        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + Vector2.up / 4; // Saut en 2D, vers le haut avec hauteur divisée par 4

        float duration = 0.75f; // Durée du saut de 0,75 secondes
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float height = Mathf.Sin(Mathf.PI * t) / 4; // Courbe de saut réaliste avec hauteur divisée par 4

            // Calculer la position Y en utilisant la courbe de hauteur et l'interpolation linéaire
            float yOffset = Mathf.Lerp(0, height, t);
            transform.position = new Vector2(startPosition.x, startPosition.y + yOffset);
            elapsedTime += Time.unscaledDeltaTime; // Utiliser le temps non échelonné

            yield return null;
        }

        transform.position = startPosition; // Retour à la position initiale

        if (movementScript != null)
        {
            movementScript.enabled = true; // Réactiver le script de mouvement
        }
    }
}
