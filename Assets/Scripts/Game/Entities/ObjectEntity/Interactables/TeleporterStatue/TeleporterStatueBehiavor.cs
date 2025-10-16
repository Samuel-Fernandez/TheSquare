using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleporterStatueBehiavor : MonoBehaviour
{
    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;

    Vector2 teleportationPosition;

    void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        StartCoroutine(WaitForTeleportationManager());
    }

    private IEnumerator WaitForTeleportationManager()
    {
        while (
            TeleportationManager.instance == null ||
            MeteoManager.instance == null ||
            MeteoManager.instance.actualScene == null ||
            string.IsNullOrEmpty(MeteoManager.instance.actualScene.sceneID) ||
            TeleportationManager.instance.teleportationsAvailable == null
        )
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        teleportationPosition = new Vector2(
            RoundToHalf(transform.position.x + (transform.rotation * circleCollider.offset).x),
            RoundToHalf(transform.position.y + (transform.rotation * circleCollider.offset).y)
        );

        bool isTeleporterKnown = TeleportationManager.instance.CheckTeleporter(
            MeteoManager.instance.actualScene.sceneID,
            teleportationPosition.x,
            teleportationPosition.y
        );

        if (!isTeleporterKnown)
        {
            spriteRenderer.color = Color.gray;
        }
        else
        {
            FadeToWhite(.1f);
        }
    }

    public void Interaction()
    {
        bool isTeleporterKnown = TeleportationManager.instance.CheckTeleporter(
            MeteoManager.instance.actualScene.sceneID,
            teleportationPosition.x,
            teleportationPosition.y
        );

        if (!isTeleporterKnown)
        {
            TeleportationManager.instance.AddTeleporter(
                MeteoManager.instance.actualScene.sceneID,
                teleportationPosition.x,
                teleportationPosition.y
            );
            FadeToWhite(1);
        }
        else
        {
            TeleportationManager.instance.ToggleTeleportationUI();
        }
    }

    public void FadeToWhite(float duration)
    {
        StartCoroutine(FadeColor(spriteRenderer.color, Color.white, duration));
    }

    private IEnumerator FadeColor(Color startColor, Color endColor, float duration)
    {
        float elapsed = 0f;

        var entityLight = GetComponent<EntityLight>();
        if (entityLight != null)
        {
            entityLight.TransitionLightIntensity(2, 2, .5f);
        }

        while (elapsed < duration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = endColor;

        if (entityLight != null)
        {
            entityLight.TransitionLightIntensity(.25f, 1, .5f);
        }
    }

    float RoundToHalf(float value)
    {
        return Mathf.Round(value * 2f) / 2f;
    }
}
