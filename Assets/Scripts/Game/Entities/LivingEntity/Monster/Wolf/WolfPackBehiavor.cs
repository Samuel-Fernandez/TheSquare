using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfPackBehiavor : MonoBehaviour
{
    [Header("Pack Settings")]
    [SerializeField] private GameObject wolfPrefab;
    [SerializeField] private int minWolves = 3;
    [SerializeField] private int maxWolves = 6;
    [SerializeField] private float spawnRadius = 3f;

    [Header("Orbit Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float orbitRadius = 3f;
    [SerializeField] private float orbitSpeed = 50f; // degrés par seconde

    private List<Transform> wolves = new List<Transform>();

    private class OrbitPoint
    {
        public Transform transform;
        public float angle;
    }

    private List<OrbitPoint> orbitPoints = new List<OrbitPoint>();
    private Dictionary<Transform, OrbitPoint> wolfOrbitMap = new Dictionary<Transform, OrbitPoint>();

    private Transform player;

    private void Start()
    {
        player = PlayerManager.instance?.player?.transform;
        if (player == null)
        {
            Debug.LogWarning("Player not found.");
            return;
        }

        SpawnWolves();
        StartCoroutine(TemporaryRetargetRoutine());
    }

    private void Update()
    {
        if (player == null) return;

        // Nettoyage des loups morts
        wolves.RemoveAll(w => w == null);

        // Réassignation si nécessaire
        if (orbitPoints.Count > 0 && wolves.Count != wolfOrbitMap.Count)
        {
            ReassignOrbitPoints();
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionRange && orbitPoints.Count == 0)
        {
            CreateOrbitPoints();
            AssignTargets();
        }

        RotateOrbitPoints();
    }

    private void SpawnWolves()
    {
        int targetCount = Random.Range(minWolves, maxWolves + 1);
        int tries = 100;
        int successfulSpawns = 0;

        while (successfulSpawns < targetCount && tries > 0)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(0.5f, spawnRadius);
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0);

            Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPos, 1f);
            bool hasBlockingCollider = false;

            foreach (var hit in hits)
            {
                if (!hit.isTrigger)
                {
                    hasBlockingCollider = true;
                    break;
                }
            }

            if (!hasBlockingCollider)
            {
                GameObject wolf = Instantiate(wolfPrefab, spawnPos, Quaternion.identity);
                wolves.Add(wolf.transform);
                successfulSpawns++;
            }

            tries--;
        }
    }

    private void CreateOrbitPoints()
    {
        orbitPoints.Clear();
        int count = wolves.Count;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            GameObject orbitGO = new GameObject("OrbitPoint_" + i);

            OrbitPoint op = new OrbitPoint
            {
                transform = orbitGO.transform,
                angle = angle
            };

            UpdateOrbitPointPosition(op);
            orbitPoints.Add(op);
        }
    }

    private void AssignTargets()
    {
        wolfOrbitMap.Clear();

        for (int i = 0; i < wolves.Count && i < orbitPoints.Count; i++)
        {
            Transform wolf = wolves[i];
            OrbitPoint orbit = orbitPoints[i];

            wolfOrbitMap[wolf] = orbit;

            NewMonsterMovement movement = wolf.GetComponent<NewMonsterMovement>();
            if (movement != null)
                movement.SetTarget(orbit.transform);
        }
    }

    private void ReassignOrbitPoints()
    {
        orbitPoints.ForEach(op => Destroy(op.transform.gameObject));
        orbitPoints.Clear();
        wolfOrbitMap.Clear();

        CreateOrbitPoints();
        AssignTargets();
    }

    private void RotateOrbitPoints()
    {
        foreach (OrbitPoint op in orbitPoints)
        {
            op.angle -= orbitSpeed * Time.deltaTime;
            if (op.angle < 0f) op.angle += 360f;

            UpdateOrbitPointPosition(op);
        }
    }

    private void UpdateOrbitPointPosition(OrbitPoint op)
    {
        float angleRad = op.angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * orbitRadius;
        op.transform.position = player.position + offset;
    }

    private IEnumerator TemporaryRetargetRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 5f));

            if (wolves.Count == 0 || orbitPoints.Count == 0) continue;

            foreach (var wolf in wolves)
            {
                if (wolf == null || !wolfOrbitMap.ContainsKey(wolf)) continue;

                float dist = Vector3.Distance(wolf.position, wolfOrbitMap[wolf].transform.position);
                if (dist < 0.5f)
                {
                    var stats = wolf.GetComponent<Stats>();
                    if (stats != null) stats.doingAttack = true;

                    var movement = wolf.GetComponent<NewMonsterMovement>();
                    if (movement != null)
                    {
                        Transform originalTarget = wolfOrbitMap[wolf].transform;
                        movement.SetTarget(player);
                        StartCoroutine(ResetTargetAfterDelay(wolf.gameObject, originalTarget, 1f));
                    }

                    break;
                }
            }
        }
    }

    private IEnumerator ResetTargetAfterDelay(GameObject wolfObj, Transform originalTarget, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (wolfObj != null)
        {
            var stats = wolfObj.GetComponent<Stats>();
            if (stats != null) stats.doingAttack = false;

            var movement = wolfObj.GetComponent<NewMonsterMovement>();
            if (movement != null) movement.SetTarget(originalTarget);
        }
    }
}
