using System.Collections;
using UnityEngine;

public class MyliaVisual : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer targetSpriteRenderer;

    [Header("Trail (Afterimage)")]
    public bool enableTrail = true;
    public Color trailColor = new Color(0.3f, 0.6f, 1f, 0.5f);
    public float trailSpawnInterval = 0.05f;
    public float trailMinMoveDistance = 0.05f;
    public float trailLifetime = 0.4f;
    public int trailSortingOrderOffset = -1;

    private Vector3 lastTrailPosition;
    private float lastTrailSpawnTime;

    private void Awake()
    {
        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        lastTrailPosition = transform.position;
    }

    private void Update()
    {
        if (enableTrail)
        {
            HandleTrail();
        }
    }

    private void HandleTrail()
    {
        if (targetSpriteRenderer == null || targetSpriteRenderer.sprite == null) return;

        float distance = Vector3.Distance(transform.position, lastTrailPosition);

        if (Time.time - lastTrailSpawnTime >= trailSpawnInterval && distance >= trailMinMoveDistance)
        {
            SpawnTrailGhost();
            lastTrailSpawnTime = Time.time;
            lastTrailPosition = transform.position;
        }
    }

    private void SpawnTrailGhost()
    {
        GameObject ghost = new GameObject("MyliaTrailGhost");
        ghost.transform.SetPositionAndRotation(targetSpriteRenderer.transform.position, targetSpriteRenderer.transform.rotation);
        ghost.transform.localScale = targetSpriteRenderer.transform.lossyScale;

        SpriteRenderer ghostRenderer = ghost.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = targetSpriteRenderer.sprite;
        ghostRenderer.flipX = targetSpriteRenderer.flipX;
        ghostRenderer.flipY = targetSpriteRenderer.flipY;
        ghostRenderer.color = trailColor;
        ghostRenderer.sortingLayerID = targetSpriteRenderer.sortingLayerID;
        ghostRenderer.sortingOrder = targetSpriteRenderer.sortingOrder + trailSortingOrderOffset;

        StartCoroutine(FadeAndDestroyGhost(ghostRenderer, trailLifetime));
    }

    private IEnumerator FadeAndDestroyGhost(SpriteRenderer ghostRenderer, float lifetime)
    {
        Color startColor = ghostRenderer.color;
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            if (ghostRenderer == null) yield break;

            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / lifetime);
            ghostRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        if (ghostRenderer != null)
        {
            Destroy(ghostRenderer.gameObject);
        }
    }
}
