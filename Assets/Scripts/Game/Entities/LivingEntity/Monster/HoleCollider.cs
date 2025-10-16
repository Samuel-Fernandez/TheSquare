using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class HoleCollider : MonoBehaviour
{
    [Header("Comportement")]
    public bool canFallInHoles = false;
    public float checkRadius = 0.25f;

    private Vector3 lastValidPosition;
    private bool isFalling = false;

    void Start()
    {
        lastValidPosition = transform.position;
    }

    private void Update()
    {
        if (isFalling || canFallInHoles)
            return;

        canFallInHoles = !GetComponent<Stats>().isVulnerable;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, checkRadius);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Untagged")) continue;
            if (!col.tag.StartsWith("Hole")) continue;

            Tilemap tilemap = col.GetComponent<Tilemap>();
            if (tilemap == null) continue;

            Vector3Int cell = tilemap.WorldToCell(transform.position);
            if (tilemap.HasTile(cell))
            {
                // On remet l'entité à sa dernière position valide
                transform.position = lastValidPosition;
                return;
            }
        }

        // Sinon, la position actuelle est valide
        lastValidPosition = transform.position;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!canFallInHoles || isFalling) return;
        if (!collision.tag.StartsWith("Hole")) return;

        Tilemap tilemap = collision.GetComponent<Tilemap>();
        if (tilemap == null) return;

        Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        if (!tilemap.HasTile(cellPos)) return;

        Vector3 holeCenter = tilemap.GetCellCenterWorld(cellPos);
        if (ShouldFallInHole(tilemap))
        {
            StartCoroutine(FallIntoHoleRoutine(holeCenter));
        }
    }

    private bool ShouldFallInHole(Tilemap tilemap)
    {
        Vector2 center = (Vector2)transform.position;
        int totalPoints = 0, pointsOverHole = 0;
        int rings = 3, pointsPerRing = 8;

        for (int r = 0; r < rings; r++)
        {
            float currentRadius = checkRadius * (r / (float)(rings - 1));
            if (r == 0)
            {
                Vector3Int cell = tilemap.WorldToCell(center);
                if (tilemap.HasTile(cell)) pointsOverHole++;
                totalPoints++;
                continue;
            }

            for (int i = 0; i < pointsPerRing; i++)
            {
                float angle = (i * 2 * Mathf.PI) / pointsPerRing;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * currentRadius;
                Vector3Int cell = tilemap.WorldToCell(point);
                if (tilemap.HasTile(cell)) pointsOverHole++;
                totalPoints++;
            }
        }

        return (float)pointsOverHole / totalPoints >= 0.7f;
    }

    private IEnumerator FallIntoHoleRoutine(Vector3 holeCenter)
    {
        isFalling = true;

        var sound = GetComponent<SoundContainer>();
        if (sound) sound.PlaySound("Fall", 2);

        var life = GetComponent<LifeManager>();
        if (life) life.TakeDamage(1, Color.black, false);

        var stats = GetComponent<Stats>();
        if (stats) stats.canMove = false;

        Transform tf = transform;
        Vector3 start = tf.position;
        Vector3 scaleStart = tf.localScale;
        float time = 0f;
        float duration = 0.5f;

        while (time < duration)
        {
            float t = time / duration;
            float smooth = Mathf.SmoothStep(0, 1, t);
            tf.position = Vector3.Lerp(start, holeCenter, smooth);
            tf.localScale = Vector3.Lerp(scaleStart, Vector3.zero, smooth);
            time += Time.deltaTime;
            yield return null;
        }

        // Écris ici ton code personnalisé pour gérer la mort définitive :
        // Ex: GetComponent<Monster>().Die();
        GetComponent<LifeManager>().Die();

    }
}
