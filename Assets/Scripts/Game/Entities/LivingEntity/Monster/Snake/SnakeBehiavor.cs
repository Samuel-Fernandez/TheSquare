using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class SnakeBehiavor : MonoBehaviour
{
    [SerializeField] private float visibleAlpha = 1f;
    [SerializeField] private float hiddenAlpha = 0.1f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private int poisonChance = 25;

    private SpriteRenderer spriteRenderer;
    private NewMonsterMovement monsterMovement;

    private Coroutine currentFadeCoroutine;

    Stats stats;

    private void Start()
    {
        monsterMovement = GetComponent<NewMonsterMovement>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        stats = GetComponent<Stats>();

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = hiddenAlpha;
            spriteRenderer.color = c;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().isVulnerable && stats.entityType == EntityType.Monster && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
        {
            if(Random.Range(0, 101) <= poisonChance)
                collision.gameObject.GetComponent<EntityEffects>().SetState(1, false, false, true);
        }
    }

    private void Update()
    {
        if (monsterMovement == null || spriteRenderer == null)
            return;

        if (monsterMovement.IsInDetectionZone)
        {
            if (spriteRenderer.color.a < visibleAlpha)
                StartFadeTo(visibleAlpha);
        }
        else
        {
            if (spriteRenderer.color.a > hiddenAlpha)
                StartFadeTo(hiddenAlpha);
        }
    }

    private void StartFadeTo(float targetAlpha)
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeToAlpha(targetAlpha));
    }

    private IEnumerator FadeToAlpha(float targetAlpha)
    {
        float startAlpha = spriteRenderer.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;

            yield return null;
        }

        // Assure que l'alpha final soit bien exactement le targetAlpha
        Color finalColor = spriteRenderer.color;
        finalColor.a = targetAlpha;
        spriteRenderer.color = finalColor;

        currentFadeCoroutine = null;
    }
}
