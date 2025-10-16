using System.Collections;
using UnityEngine;

public class SkeletonBridgeBehiavor : MonoBehaviour
{
    public string id;
    private bool isRotating = false;
    private float initialAngle = 0f;
    private bool isActivated = false; // État d'activation du pont

    private void Start()
    {
        // Stocker l'angle initial (position de repos)
        this.initialAngle = transform.eulerAngles.z;

        // Récupérer l'état sauvegardé
        bool state;
        SaveManager.instance.twoStateContainer.TryGetState(id, out state);

        if (state)
        {
            // Si le pont était activé, le mettre directement dans la position activée
            isActivated = true;
            float targetAngle = (initialAngle + 90f) % 360f;
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
        else
        {
            // S'assurer que le pont est dans sa position initiale
            isActivated = false;
            transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
        }
    }

    public void Activate()
    {
        if (!isRotating)
        {
            // Basculer l'état d'activation
            isActivated = !isActivated;

            // Calculer l'angle cible
            float targetAngle = isActivated ?
                (initialAngle + 90f) % 360f :
                initialAngle;

            // Sauvegarder le nouvel état
            SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, isActivated);

            // Démarrer la rotation
            StartCoroutine(RotateToAngle(transform.eulerAngles.z, targetAngle));
        }
    }

    private IEnumerator RotateToAngle(float startAngle, float targetAngle)
    {
        yield return new WaitForSeconds(1f);
        GetComponent<SoundContainer>().PlaySound("Rotation", 1);
        isRotating = true;

        float duration = 1f;
        float elapsed = 0f;

        // Pour interpolation correcte si on passe par 0°
        float totalAngle = Mathf.DeltaAngle(startAngle, targetAngle);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t); // smoothstep easing
            float angle = startAngle + totalAngle * smoothT;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // S'assurer que la rotation finale est exacte
        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        isRotating = false;
    }
}