using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CinematicSpriteBehiavor : MonoBehaviour
{
    public Image sprite;
    public Image blackOverlay; // Une Image noire par-dessus, full-screen
    private Material material;    // Instance locale


    private void Start()
    {
        material = Instantiate(sprite.material);
        sprite.material = material;

        if (sprite.sprite != null)
        {
            material.SetTexture("_MainTex", sprite.sprite.texture);
        }

        material.SetFloat("_Saturation", 1f);  // couleur normale
        material.SetFloat("_Negative", 0f);    // pas de négatif
        material.SetColor("_Color", Color.black);
        material.SetFloat("_Fade", 0f);        // image complètement noire
    }

    public IEnumerator NewImageRoutine(Sprite newSprite, float duration)
    {
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.color = new Color(0, 0, 0, 0);

        // Fondu au noir
        float timer = 0f;
        while (timer < duration / 2f)
        {
            blackOverlay.color = new Color(0, 0, 0, timer / (duration / 2f));
            timer += Time.deltaTime;
            yield return null;
        }

        blackOverlay.color = Color.black;
        sprite.sprite = newSprite;
        material.SetTexture("_MainTex", newSprite.texture);
        material.SetColor("_Color", Color.white);



        // Retour au visible
        timer = 0f;
        while (timer < duration / 2f)
        {
            blackOverlay.color = new Color(0, 0, 0, 1 - timer / (duration / 2f));
            timer += Time.deltaTime;
            yield return null;
        }

        blackOverlay.color = new Color(0, 0, 0, 0);
        blackOverlay.gameObject.SetActive(false);
    }

    public void Shaking(float shakingDuration)
    {
        StartCoroutine(ShakeRoutine(shakingDuration));
    }

    private IEnumerator ShakeRoutine(float duration)
    {
        Vector3 originalPos = sprite.rectTransform.localPosition;
        float timer = 0f;

        while (timer < duration)
        {
            float x = Random.Range(-5f, 5f);
            float y = Random.Range(-5f, 5f);
            sprite.rectTransform.localPosition = originalPos + new Vector3(x, y, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        sprite.rectTransform.localPosition = originalPos;
    }

    public void Saturation(float saturationDuration, float saturationPower)
    {
        StartCoroutine(SaturationRoutine(saturationDuration, saturationPower));
    }

    private IEnumerator SaturationRoutine(float duration, float targetSaturation)
    {
        float start = material.GetFloat("_Saturation");
        float timer = 0f;

        while (timer < duration)
        {
            float value = Mathf.Lerp(start, targetSaturation, timer / duration);
            material.SetFloat("_Saturation", value);
            timer += Time.deltaTime;
            yield return null;
        }

        material.SetFloat("_Saturation", targetSaturation);
    }

    public void Negative(float negationDuration)
    {
        StartCoroutine(NegativeRoutine(negationDuration));
    }

    private IEnumerator NegativeRoutine(float duration)
    {
        float start = material.GetFloat("_Negative");
        float end = 1 - start;
        float timer = 0f;

        while (timer < duration)
        {
            float value = Mathf.Lerp(start, end, timer / duration);
            material.SetFloat("_Negative", value);
            timer += Time.deltaTime;
            yield return null;
        }

        material.SetFloat("_Negative", end);
    }
}
