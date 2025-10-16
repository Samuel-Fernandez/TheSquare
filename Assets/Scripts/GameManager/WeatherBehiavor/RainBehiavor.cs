using System.Collections;
using UnityEngine;

public class RainBehavior : MonoBehaviour
{
    public GameObject lightningBolt; // Préfabriqué pour l'éclair
    public float lightningRadius = 15f; // Rayon autour du joueur où l'éclair peut apparaître

    private void Start()
    {
        StartCoroutine(RoutineLightningBolt()); // Démarrer la routine des éclairs
    }

    IEnumerator RoutineLightningBolt()
    {
        while (true)
        {
            // Attendre un temps aléatoire entre 1 et 5 secondes avant de faire apparaître un éclair
            yield return new WaitForSeconds(Random.Range(1f, 20f));

            // Calculer une position aléatoire dans le rayon autour du joueur
            Vector2 playerPosition = PlayerManager.instance.player.transform.position;
            Vector2 randomDirection = Random.insideUnitCircle * lightningRadius; // Position aléatoire dans le cercle
            Vector2 lightningPosition = playerPosition + randomDirection;

            // Instancier l'éclair à la position calculée
            Instantiate(lightningBolt, lightningPosition, Quaternion.identity);
        }
    }
}
