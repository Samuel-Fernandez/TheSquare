using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JumperBehiavor : MonoBehaviour
{
    public enum JumpDirection { UP, DOWN, LEFT, RIGHT }
    public JumpDirection jumpDirection;

    public float jumpDistance = 2.5f;
    public float jumpDuration = 0.5f;
    public AnimationCurve jumpCurve;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Stats stats = other.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            StartCoroutine(HandleJump(other.gameObject));
        }
    }

    private IEnumerator HandleJump(GameObject player)
    {
        // Récupération des composants essentiels
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        SpriteRenderer spriteRenderer = player.GetComponentInChildren<SpriteRenderer>();
        Transform spriteTransform = spriteRenderer != null ? spriteRenderer.transform : null;

        Vector3 startPos = player.transform.position;
        Vector3 endPos = startPos + GetDirectionVector() * jumpDistance;

        // Désactivation de la hitbox
        if (playerCollider != null)
            playerCollider.enabled = false;

        float timer = 0f;

        GetComponent<SoundContainer>().PlaySound("Jump", 2);
        // Animation du saut
        while (timer < jumpDuration)
        {
            float t = timer / jumpDuration;
            Vector3 interpolatedPos = Vector3.Lerp(startPos, endPos, t);
            float verticalOffset = jumpCurve.Evaluate(t);

            player.transform.position = interpolatedPos;

            if (spriteTransform != null)
            {
                // Applique l’élévation simulée (ex. : vertical "saut")
                spriteTransform.localPosition = new Vector3(0, verticalOffset, 0);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Fin du saut : position finale propre
        player.transform.position = endPos;

        player.GetComponent<ObjectPerspective>().level--;

        if (spriteTransform != null)
        {
            spriteTransform.localPosition = Vector3.zero;
        }

        // Réactivation de la hitbox
        if (playerCollider != null)
            playerCollider.enabled = true;
    }

    private Vector3 GetDirectionVector()
    {
        return jumpDirection switch
        {
            JumpDirection.UP => Vector3.up,
            JumpDirection.DOWN => Vector3.down,
            JumpDirection.LEFT => Vector3.left,
            JumpDirection.RIGHT => Vector3.right,
            _ => Vector3.zero,
        };
    }
}

