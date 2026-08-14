using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Point d'ancrage scene pour le sol de glace temporaire de l'attaque Ice Bumper de Mylia.
// Mylia est instanciee dynamiquement (spawn via event) et ne peut donc pas referencer cet
// objet depuis l'Inspector : on passe par un singleton, comme les autres XManager du projet.
[RequireComponent(typeof(Tilemap))]
public class MyliaIceFieldTilemap : MonoBehaviour
{
    public static MyliaIceFieldTilemap instance;

    public TileBase iceTile;

    Tilemap tilemap;

    void Awake()
    {
        instance = this;
        tilemap = GetComponent<Tilemap>();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public Vector3Int PlaceTile(Vector3 worldPosition)
    {
        Vector3Int cell = tilemap.WorldToCell(worldPosition);
        tilemap.SetTile(cell, iceTile);
        return cell;
    }

    // Remplit un disque de tuiles centre sur "center", du centre vers l'exterieur, etale sur "duration"
    // secondes (effet de glace qui se repand). Les cellules posees sont ajoutees a "outCells" au fur et
    // a mesure, pour que l'appelant puisse les retirer plus tard (fonte du sol de glace).
    public IEnumerator FillCircle(Vector3 center, float radius, float duration, List<Vector3Int> outCells)
    {
        Vector3Int centerCell = tilemap.WorldToCell(center);
        float cellSize = tilemap.cellSize.x > 0 ? tilemap.cellSize.x : 1f;
        int cellRadius = Mathf.CeilToInt(radius / cellSize);

        List<Vector3Int> cells = new List<Vector3Int>();
        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                if (x * x + y * y > cellRadius * cellRadius) continue;
                cells.Add(new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z));
            }
        }

        cells.Sort((a, b) => (a - centerCell).sqrMagnitude.CompareTo((b - centerCell).sqrMagnitude));

        int placed = 0;
        float elapsed = 0f;
        while (placed < cells.Count)
        {
            elapsed += Time.deltaTime;
            int target = Mathf.Min(cells.Count, Mathf.FloorToInt((elapsed / duration) * cells.Count));
            while (placed < target)
            {
                tilemap.SetTile(cells[placed], iceTile);
                outCells.Add(cells[placed]);
                placed++;
            }
            yield return null;
        }
    }

    public void RemoveTiles(IEnumerable<Vector3Int> cells)
    {
        foreach (Vector3Int cell in cells)
        {
            tilemap.SetTile(cell, null);
        }
    }
}
