using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrasierBehiavor : MonoBehaviour
{
    [Header("References")]
    public GameObject firestickPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 8f;
    public int maxFireSticks = 2;
    public float spawnRadius = 2f;
    public int spawnPositionAttempts = 8;
    public float obstacleCheckRadius = 0.3f;

    [Header("Spawn Animation")]
    public float jumpHeight = 0.75f;
    public float jumpDuration = 0.4f;

    [Header("Light Flicker Settings")]
    public Color lightColor = new Color(1f, 0.55f, 0.1f);
    public float lightIntensityMin = 0.5f;
    public float lightIntensityMax = 1.5f;
    public float lightRadiusMin = 1f;
    public float lightRadiusMax = 2f;
    public float flickerIntervalMin = 0.05f;
    public float flickerIntervalMax = 0.25f;
    public float flickerTransitionTime = 0.1f;

    private readonly List<GameObject> spawnedSticks = new List<GameObject>();
    private EntityLight entityLight;
    private SoundContainer soundContainer;

    private void Awake()
    {
        entityLight = GetComponent<EntityLight>();
        soundContainer = GetComponent<SoundContainer>();
    }

    private void Start()
    {
        if (entityLight != null)
        {
            entityLight.SetLightColor(lightColor);
        }

        StartCoroutine(FlickerRoutine());
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            spawnedSticks.RemoveAll(stick => stick == null);

            if (spawnedSticks.Count < maxFireSticks)
            {
                TrySpawnFireStick();
            }
        }
    }

    private void TrySpawnFireStick()
    {
        if (firestickPrefab == null) return;

        for (int i = 0; i < spawnPositionAttempts; i++)
        {
            Vector2 candidatePos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

            if (!IsPositionBlocked(candidatePos))
            {
                SpawnFireStickAt(candidatePos);
                return;
            }
        }
    }

    private bool IsPositionBlocked(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, obstacleCheckRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject || hit.isTrigger) continue;
            return true;
        }
        return false;
    }

    private void SpawnFireStickAt(Vector2 position)
    {
        GameObject instance = Instantiate(firestickPrefab, transform.position, Quaternion.identity);
        spawnedSticks.Add(instance);

        if (soundContainer != null) soundContainer.PlaySound("Fire", 1);

        FireStickBehiavor stick = instance.GetComponent<FireStickBehiavor>();
        if (stick != null)
        {
            Vector3 targetPosition = new Vector3(position.x, position.y, transform.position.z);
            stick.PlaySpawnAnimation(targetPosition, jumpHeight, jumpDuration);
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            float intensity = Random.Range(lightIntensityMin, lightIntensityMax);
            float radius = Random.Range(lightRadiusMin, lightRadiusMax);
            float interval = Random.Range(flickerIntervalMin, flickerIntervalMax);

            if (entityLight != null)
            {
                entityLight.TransitionLightIntensity(intensity, radius, flickerTransitionTime);
            }

            yield return new WaitForSeconds(interval);
        }
    }
}
