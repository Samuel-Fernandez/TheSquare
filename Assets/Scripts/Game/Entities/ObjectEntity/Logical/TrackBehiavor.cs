using UnityEngine;

public enum WagonMovement
{
    NORMAL,
    END,
    AXIS_CHANGE_PLUS,
    AXIS_CHANGE_MINUS,
}

public enum TrackType { LINEAR, DIRECTIONAL, END }

public class TrackBehavior : MonoBehaviour
{
    public TrackType type;
    public Vector2 initialDirection = new Vector2(0, 1); // Direction initiale pour les rails END

    public Vector2 GetWagonMovement(Vector2 currentDirection)
    {
        if (currentDirection == Vector2.zero)
        {
            Debug.LogWarning("GetWagonMovement: currentDirection zero");
            return Vector2.zero;
        }

        // Snap vers axes entiers (1,0),(0,1),(-1,0),(0,-1) pour éviter les petites imprécisions float
        int sx = Mathf.RoundToInt(currentDirection.x);
        int sy = Mathf.RoundToInt(currentDirection.y);
        Vector2 snapped = new Vector2(sx, sy);

        float angle = Mathf.Repeat(transform.eulerAngles.z, 360f); // [0,360)
        float tol = 1f; // tolérance en degrés

        Debug.Log($"GetWagonMovement - angle rail: {angle}, direction courante (snapped): {snapped}");

        // Pour 0° ou 180° -> swap axes : (x,y) -> (y,x)
        if (Mathf.Abs(angle - 0f) <= tol || Mathf.Abs(angle - 180f) <= tol)
        {
            Vector2 newDir = new Vector2(snapped.y, snapped.x);
            Debug.Log($"Remap 0/180 -> {snapped} -> {newDir.normalized}");
            return newDir.normalized;
        }
        // Pour 90° ou 270° -> swap + inversion : (x,y) -> (-y,-x)
        else if (Mathf.Abs(angle - 90f) <= tol || Mathf.Abs(angle - 270f) <= tol)
        {
            Vector2 newDir = new Vector2(-snapped.y, -snapped.x);
            Debug.Log($"Remap 90/270 -> {snapped} -> {newDir.normalized}");
            return newDir.normalized;
        }

        Debug.LogWarning($"Angle non standard ({angle}) — fallback: retourne la direction actuelle normalisée");
        return currentDirection.normalized;
    }

}