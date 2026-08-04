using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RockBoulderBehiavor : MonoBehaviour
{
    [Header("Grille")]
    [Tooltip("Taille d'une case, utilisée pour aligner l'arrêt du rocher (ex: sur un GroundButton)")]
    public float gridSize = 1f;

    [Header("Glissade")]
    public float slideSpeed = 4f;
    [Tooltip("Rayon utilisé pour détecter un obstacle (collider non trigger) devant le rocher")]
    public float obstacleCheckRadius = 0.4f;

    private Rigidbody2D rb;
    private SoundContainer soundContainer;

    public bool IsSliding { get; private set; } = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.bodyType = RigidbodyType2D.Kinematic;

        soundContainer = GetComponent<SoundContainer>();

        // S'assure que le rocher démarre bien aligné sur la grille
        transform.position = SnapToGrid(transform.position);
    }

    // direction : 0 = haut, 1 = gauche, 2 = droite, 3 = bas (convention utilisée par UseSpecialObject)
    public void Hit(int direction)
    {
        if (IsSliding) return;

        Vector2 slideDirection = DirectionFromInt(direction);
        if (slideDirection == Vector2.zero) return;

        if (IsPositionBlocked((Vector2)transform.position + slideDirection * gridSize * 0.5f))
            return;

        StartCoroutine(SlideRoutine(slideDirection));
    }

    private Vector2 DirectionFromInt(int direction)
    {
        switch (direction)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.left;
            case 2: return Vector2.right;
            case 3: return Vector2.down;
            default: return Vector2.zero;
        }
    }

    private IEnumerator SlideRoutine(Vector2 direction)
    {
        IsSliding = true;

        if (soundContainer != null)
            soundContainer.PlaySound("Slide", 2);

        while (!IsPositionBlocked((Vector2)transform.position + direction * slideSpeed * Time.deltaTime))
        {
            transform.position += (Vector3)(direction * slideSpeed * Time.deltaTime);
            yield return null;
        }

        // Alignement propre sur la grille à l'arrêt
        Vector2 snapped = SnapToGrid(transform.position);
        if (!IsPositionBlocked(snapped))
            transform.position = snapped;

        if (soundContainer != null)
            soundContainer.PlaySound("Impact", 2);

        IsSliding = false;
    }

    private bool IsPositionBlocked(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, obstacleCheckRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.isTrigger) continue;

            return true;
        }

        return false;
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize
        );
    }
}
