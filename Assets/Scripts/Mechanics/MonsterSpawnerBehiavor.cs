using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MonsterToSpawn
{
    public GameObject monsterPrefab;
    public float chanceSpawn;
}

public class MonsterSpawnerBehiavor : MonoBehaviour
{
    public string id;
    public List<MonsterToSpawn> monstersToSpawn;
    public float spawnChanceEverySeconds;
    public int maxMonsters;
    public int altitudeLevelForMonsters = 0;

    private Dictionary<int, GameObject> activeMonsters = new Dictionary<int, GameObject>();
    private List<Transform> spawnPoints = new List<Transform>();

    private void Start()
    {
        // Enregistrer les points enfants comme emplacements de spawn
        foreach (Transform child in transform)
            spawnPoints.Add(child);

        MonsterSpawnManager.instance.RegisterSpawner(this);
        SyncVisualsWithState();
    }

    private void Update()
    {
        MonsterSpawnManager.instance.UpdateSpawnerDeadMonsters();
    }

    public GameObject GetMonster()
    {
        float totalChance = 0f;
        foreach (var m in monstersToSpawn)
            totalChance += m.chanceSpawn;

        float random = Random.Range(0, totalChance);
        float cumulative = 0f;

        foreach (var m in monstersToSpawn)
        {
            cumulative += m.chanceSpawn;
            if (random <= cumulative)
                return m.monsterPrefab;
        }

        return null;
    }

    public void SyncVisualsWithState()
    {
        var state = MonsterSpawnManager.instance.GetSpawnerState(id);
        if (state == null) return;

        // Détruire tous les anciens monstres
        foreach (var obj in activeMonsters.Values)
            if (obj != null) Destroy(obj);
        activeMonsters.Clear();

        // Recréer les monstres marqués comme présents dans l'état
        for (int i = 0; i < state.hasMonsterAtIndex.Count && i < spawnPoints.Count; i++)
        {
            if (state.hasMonsterAtIndex[i])
            {
                GameObject prefab = GetMonster();
                if (prefab != null && spawnPoints[i])
                {
                    GameObject instance = Instantiate(prefab, spawnPoints[i].position, Quaternion.identity);
                    if (instance.GetComponent<ObjectPerspective>())
                        instance.GetComponent<ObjectPerspective>().level = altitudeLevelForMonsters;

                    MonsterDeath controller = instance.GetComponent<MonsterDeath>();
                    if (controller != null)
                    {
                        controller.spawnerId = id;
                        controller.spawnIndex = i;
                    }

                    activeMonsters[i] = instance;
                }
            }
        }
    }

    public void ReportDeadMonsters()
    {
        var state = MonsterSpawnManager.instance.GetSpawnerState(id);
        if (state == null) return;

        List<int> keysToRemove = new List<int>();

        foreach (var kvp in activeMonsters)
        {
            if (kvp.Value == null)
            {
                // Marquer ce slot comme libre (monstre mort)
                if (kvp.Key >= 0 && kvp.Key < state.hasMonsterAtIndex.Count)
                {
                    state.hasMonsterAtIndex[kvp.Key] = false;
                    // Ne pas reset le timer ici : il continue à s'incrémenter
                }
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (int key in keysToRemove)
        {
            activeMonsters.Remove(key);
        }
    }
}
