using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class SpawnPoint
{
    public GameObject monsterPrefab;
    public Vector2 spawnPoint; // Position relative au spawner
}

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    public List<SpawnPoint> spawnPoints;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Stats>().entityType == EntityType.Player)
        {
            SpawnMonster();
        }
    }

    public void SpawnMonster()
    {
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            Vector2 spawnPosition = (Vector2)transform.position + spawnPoint.spawnPoint;

            Instantiate(spawnPoint.monsterPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
