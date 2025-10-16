using UnityEngine;
using UnityEngine.Tilemaps;

public class GrassInteraction : MonoBehaviour
{
    public Tilemap tilemap;

    private void Start()
    {
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ajout de messages de débogage

        Stats stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {

            // Obtenir la position du joueur dans la grille de la tilemap
            Vector3Int gridPosition = tilemap.WorldToCell(collision.transform.position);

            // Vérifier si une tuile existe à cette position avant de la détruire
            if (tilemap.HasTile(gridPosition))
            {
                tilemap.SetTile(gridPosition, null);
            }
            else
            {
                Debug.Log("Aucune tuile trouvée à cette position");
            }
        }
        else
        {
            Debug.Log("Collision avec un objet non joueur");
        }
    }
}
