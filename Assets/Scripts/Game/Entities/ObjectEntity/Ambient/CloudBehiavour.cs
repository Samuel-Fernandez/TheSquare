using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudBehiavour : MonoBehaviour
{
    public List<Sprite> sprites;
    private SpriteRenderer sr;
    private Coroutine fadeOutCoroutine;
    private Coroutine moveCoroutine;

    public void CreateCloud()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        sr.sprite = sprites[Random.Range(0, sprites.Count)];

        float targetAlpha = Random.Range(0.1f, 0.4f);
        float moveX = Random.Range(0.01f, 0.1f);
        float moveY = Random.Range(0.01f, 0.1f);
        float lifeTime = Random.Range(10f, 30f);

        StartCoroutine(FadeIn(sr, targetAlpha, 5f));
        moveCoroutine = StartCoroutine(MoveCloud(moveX, moveY));
        fadeOutCoroutine = StartCoroutine(FadeOutAndDestroy(sr, targetAlpha, lifeTime));
    }

    public void DestroyCloud()
    {
        if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        if (sr != null && gameObject != null)
            StartCoroutine(FadeOutAndDestroy(sr, sr.color.a, 0f));
    }

    IEnumerator FadeIn(SpriteRenderer sr, float targetAlpha, float duration)
    {
        float timer = 0f;
        if (sr == null) yield break;

        Color color = sr.color;
        color.a = 0f;
        sr.color = color;

        while (timer < duration)
        {
            if (sr == null) yield break;
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, targetAlpha, timer / duration);
            sr.color = color;
            yield return null;
        }

        if (sr != null)
        {
            color.a = targetAlpha;
            sr.color = color;
        }
    }

    IEnumerator MoveCloud(float xSpeed, float ySpeed)
    {
        while (true)
        {
            if (this == null || transform == null) yield break;
            transform.position += new Vector3(xSpeed, ySpeed, 0f) * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeOutAndDestroy(SpriteRenderer sr, float startAlpha, float delay)
    {
        if (sr == null) yield break;

        yield return new WaitForSeconds(delay);

        float duration = 5f;
        float timer = 0f;
        Color color = sr.color;

        while (timer < duration)
        {
            if (sr == null) yield break;
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, timer / duration);
            sr.color = color;
            yield return null;
        }

        if (this != null)
            Destroy(gameObject);
    }
}
