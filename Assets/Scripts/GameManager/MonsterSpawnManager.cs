using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SpawnerState
{
    public string id;
    public float spawnChanceEverySeconds;
    public int maxMonsters;
    public List<bool> hasMonsterAtIndex = new();
    public float timeSinceLastSpawn = 0f;
    public string sceneId;
}

public class MonsterSpawnManager : MonoBehaviour
{
    public static MonsterSpawnManager instance;

    private Dictionary<string, SpawnerState> spawnerStates = new();
    private Dictionary<string, MonsterSpawnerBehiavor> sceneSpawners = new();
    private string currentSceneName;
    private bool isLoadingScene = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        isLoadingScene = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneSpawners.Clear();
        currentSceneName = scene.name;
        isLoadingScene = false;

        // À l'entrée dans la scène : on calcule le repeuplement d’après le timer
        ProcessRespawnsForCurrentScene();
    }

    private void ProcessRespawnsForCurrentScene()
    {
        foreach (var state in spawnerStates.Values)
        {
            if (state.sceneId != currentSceneName)
                continue;

            int maxToSpawn = Mathf.Min(state.maxMonsters, state.hasMonsterAtIndex.Count);
            int currentCount = state.hasMonsterAtIndex.Count(b => b);
            float timePassed = state.timeSinceLastSpawn;

            // Calcul du nombre total de monstres devant être présents
            int monstersShouldBePresent = Mathf.Min(maxToSpawn,
                currentCount + Mathf.FloorToInt(timePassed / state.spawnChanceEverySeconds));

            int toSpawn = monstersShouldBePresent - currentCount;

            if (toSpawn > 0)
            {
                // Marquer toSpawn emplacements comme occupés
                for (int i = 0; i < state.hasMonsterAtIndex.Count && toSpawn > 0; i++)
                {
                    if (!state.hasMonsterAtIndex[i])
                    {
                        state.hasMonsterAtIndex[i] = true;
                        toSpawn--;
                    }
                }
                // Garder le reste de temps non utilisé
                state.timeSinceLastSpawn %= state.spawnChanceEverySeconds;
            }

            // Synchroniser visuels avec l'état modifié
            if (sceneSpawners.TryGetValue(state.id, out var spawner))
            {
                spawner.SyncVisualsWithState();
            }
        }
    }

    public void RegisterSpawner(MonsterSpawnerBehiavor spawner)
    {
        if (spawner == null || string.IsNullOrEmpty(spawner.id))
            return;

        if (!spawnerStates.ContainsKey(spawner.id))
        {
            var newState = new SpawnerState
            {
                id = spawner.id,
                maxMonsters = spawner.maxMonsters,
                spawnChanceEverySeconds = spawner.spawnChanceEverySeconds,
                hasMonsterAtIndex = new List<bool>(),
                sceneId = currentSceneName
            };

            int spawnCount = spawner.transform.childCount;
            List<int> indices = Enumerable.Range(0, spawnCount).OrderBy(_ => Random.value).ToList();

            // Au premier chargement, on marque maxMonsters emplacements comme true
            for (int i = 0; i < spawnCount; i++)
            {
                newState.hasMonsterAtIndex.Add(indices.IndexOf(i) < spawner.maxMonsters);
            }

            spawnerStates[spawner.id] = newState;
        }
        else
        {
            // Met à jour la scène
            spawnerStates[spawner.id].sceneId = currentSceneName;
        }

        if (!sceneSpawners.ContainsKey(spawner.id))
        {
            sceneSpawners.Add(spawner.id, spawner);
        }

        spawner.SyncVisualsWithState();
    }

    public SpawnerState GetSpawnerState(string id)
    {
        spawnerStates.TryGetValue(id, out var state);
        return state;
    }

    public void UpdateSpawnerDeadMonsters()
    {
        if (isLoadingScene) return;

        // Le timer s'incrémente même hors scène
        foreach (var state in spawnerStates.Values)
        {
            // Ici on incrémente toujours le timer, sans condition de scène active
            state.timeSinceLastSpawn += Time.deltaTime;
        }

        // Mais on ne fait le rapport que pour les spawners en scène active
        foreach (var spawner in sceneSpawners.Values)
        {
            spawner.ReportDeadMonsters();
        }
    }

    // Retourne la liste de tous les états des spawners pour sauvegarde
    public List<SpawnerState> GetAllSpawnerStates()
    {
        return new List<SpawnerState>(spawnerStates.Values);
    }

    // Charge la liste d'états depuis la sauvegarde
    public void LoadSpawnerStates(List<SpawnerState> loadedStates)
    {
        spawnerStates.Clear();

        foreach (var state in loadedStates)
        {
            spawnerStates[state.id] = state;
        }

        // Synchronise la scène avec ces états chargés
        foreach (var spawner in sceneSpawners.Values)
        {
            spawner.SyncVisualsWithState();
        }
    }

}
