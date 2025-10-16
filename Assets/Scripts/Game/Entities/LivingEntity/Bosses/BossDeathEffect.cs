using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDeathEffect : MonoBehaviour
{
    public GameObject explosionPrefab;

    public void SpawnExplosions(float duration, int frequencyPerSeconds)
    {
        StartCoroutine(ExplosionRoutine(duration, frequencyPerSeconds));
    }

    private IEnumerator ExplosionRoutine(float duration, int frequencyPerSeconds)
    {
        float interval = 1f / frequencyPerSeconds;
        float elapsed = 0f;

        // Récupère le SpriteRenderer dans les enfants
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found in children.");
            yield break;
        }

        Bounds bounds = spriteRenderer.bounds;

        while (elapsed < duration)
        {
            Vector3 randomPos = GetRandomPositionAroundBounds(bounds, 0.5f);
            Instantiate(explosionPrefab, randomPos, Quaternion.identity);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private Vector3 GetRandomPositionAroundBounds(Bounds bounds, float margin)
    {
        float xMin = bounds.min.x - margin;
        float xMax = bounds.max.x + margin;
        float yMin = bounds.min.y - margin;
        float yMax = bounds.max.y + margin;

        float x = Random.Range(xMin, xMax);
        float y = Random.Range(yMin, yMax);

        return new Vector3(x, y, 0);
    }
}
