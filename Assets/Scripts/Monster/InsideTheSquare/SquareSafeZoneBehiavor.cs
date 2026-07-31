using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSquare.Mechanics.UniverseHeart;

public class SquareSafeZoneBehiavor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (InsideTheSquareManager.instance == null) return;

        // La safe zone est visible en permanence (même avant le démarrage du timer),
        // et reste active même en alerte
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (col != null) col.enabled = true; // Toujours actif pour détecter le joueur
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (InsideTheSquareManager.instance == null) return;

        Stats stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            InsideTheSquareManager.is_in_safezone = true;
            if (InsideTheSquareManager.player_is_revealed && InsideTheSquareManager.instance.currentTimer < InsideTheSquareManager.instance.timeToFill)
            {
                InsideTheSquareManager.instance.ResetZone();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (InsideTheSquareManager.instance == null) return;

        Stats stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            InsideTheSquareManager.is_in_safezone = false;
        }
    }
}
