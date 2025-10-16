using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTeleporter : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<EntityLight>().SetLightColor(Color.magenta);
        StartCoroutine(LightCoroutine());

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;

        StartCoroutine(FadeIn(3f));
    }

    private IEnumerator FadeIn(float duration)
    {
        float timer = 0f;
        Color color = spriteRenderer.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / duration);
            spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }

    IEnumerator LightCoroutine()
    {
        while(true)
        {
            GetComponent<EntityLight>().TransitionLightIntensity(2, 2, 1);
            yield return new WaitForSeconds(1);
            GetComponent<EntityLight>().TransitionLightIntensity(.25f, 1, 1);
            yield return new WaitForSeconds(1);
        }
    }
}
