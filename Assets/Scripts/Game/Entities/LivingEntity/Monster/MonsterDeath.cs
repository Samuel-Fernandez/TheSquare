using UnityEngine;

// Ajoutez ce script à vos prefabs de monstres
public class MonsterDeath : MonoBehaviour
{
    [HideInInspector] public string spawnerId; // ID du spawner qui a créé ce monstre
    [HideInInspector] public int spawnIndex; // Index dans le spawner

    // Si vous avez déjà un script pour gérer la mort du monstre,
    // ajoutez ce code quand le monstre meurt:
    public void OnMonsterDeath()
    {
        if (!string.IsNullOrEmpty(spawnerId) && MonsterSpawnManager.instance != null)
        {
            // Enregistrer la mort du monstre dans le spawner manager
            // MonsterSpawnManager.instance.RegisterKilledMonster(spawnerId, spawnIndex);
        }
    }
}