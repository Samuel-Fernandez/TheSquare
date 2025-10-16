using System.Collections;
using UnityEngine;

public class DesertTombBehiavor : MonoBehaviour
{
    public string id;

    private void Start()
    {
        bool state;

        if(SaveManager.instance.twoStateContainer.TryGetState(id, out state))
        {
            MoveLeftSmooth();
        }
    }
    // Méthode publique à appeler pour déplacer l'entité
    public void MoveLeftSmooth()
    {
        GetComponent<SoundContainer>().PlaySound("Move", 1);
        StartCoroutine(MoveLeftCoroutine(1.5f, 2f));
    }

    private IEnumerator MoveLeftCoroutine(float distance, float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(-distance, 0, 0);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // S'assurer d'arriver exactement à la position finale
    }
}
