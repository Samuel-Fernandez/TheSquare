using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WallJumper : MonoBehaviour
{
    [Header("Cible du saut")]
    public Transform landingPoint;

    [Header("Saut")]
    public float jumpDuration = 1f;
    public AnimationCurve jumpCurve;

    [Header("Lumière du saut")]
    public float jumpLightIntensity = 1f;
    public float jumpLightRadius = 2f;
    public float lightFallDuration = 0.5f;

    private bool isJumping = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isJumping) return;

        Stats stats = other.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            StartCoroutine(HandleJump(other.gameObject));
        }
    }

    private IEnumerator HandleJump(GameObject player)
    {
        isJumping = true;

        Stats stats = player.GetComponent<Stats>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        SpriteRenderer spriteRenderer = player.GetComponentInChildren<SpriteRenderer>();
        Transform spriteTransform = spriteRenderer != null ? spriteRenderer.transform : null;

        // Plus aucune action possible pendant le temps du saut
        stats.canMove = false;

        // Désactivation de la hitbox pour passer au-dessus du mur
        if (playerCollider != null)
            playerCollider.enabled = false;

        GetComponent<SoundContainer>().PlaySound("Jump", 2);

        // Éclat de lumière instantané sur l'objet de saut, puis retour progressif à l'état de base
        EntityLight entityLight = GetComponent<EntityLight>();
        if (entityLight != null && entityLight.entityLight != null)
        {
            Light2D light2D = entityLight.entityLight.GetComponent<Light2D>();
            if (light2D != null)
            {
                float baseIntensity = light2D.intensity;
                float baseRadius = light2D.pointLightOuterRadius;

                entityLight.SetLightIntensity(jumpLightIntensity, jumpLightRadius);
                entityLight.TransitionLightIntensity(baseIntensity, baseRadius, lightFallDuration);
            }
        }

        Vector3 startPos = player.transform.position;
        Vector3 endPos = landingPoint.position;

        float timer = 0f;
        while (timer < jumpDuration)
        {
            float t = timer / jumpDuration;
            player.transform.position = Vector3.Lerp(startPos, endPos, t);

            if (spriteTransform != null)
                spriteTransform.localPosition = new Vector3(0, jumpCurve.Evaluate(t), 0);

            timer += Time.deltaTime;
            yield return null;
        }

        player.transform.position = endPos;

        if (spriteTransform != null)
            spriteTransform.localPosition = Vector3.zero;

        if (playerCollider != null)
            playerCollider.enabled = true;

        stats.canMove = true;

        isJumping = false;
    }
}
