using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AraxieBehiavor : MonoBehaviour
{
    public List<GameObject> hairs = new List<GameObject>();
    private EntityLight entityLight;

    private void Start()
    {
        entityLight = GetComponent<EntityLight>();

        RainbowHair();
        if (entityLight != null)
        {
            StartCoroutine(RainbowLightEffect());
        }
    }

    // Méthode pour démarrer l'effet arc-en-ciel de la lumière
    private IEnumerator RainbowLightEffect()
    {
        float duration = 2f; // Temps pour parcourir tout l'arc-en-ciel
        float intensityVariance = 0.2f; // Amplitude du vacillement de la lumière
        float baseIntensity = 5;

        float baseRadius = 10f; // Nouveau rayon 10x plus grand
        float radiusVariance = 2f; // Variation du radius

        while (true)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float hue = Mathf.Repeat(elapsedTime / duration, 1f);
                Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
                entityLight.SetLightColor(rainbowColor);

                // Faire légèrement vaciller l'intensité
                float flicker = Mathf.Sin(Time.time * 10f) * intensityVariance;
                entityLight.SetLightIntensity(Mathf.Clamp(baseIntensity + flicker, 0.5f, 1.5f));

                // Modifier dynamiquement le radius avec un effet pulsant
                float newRadius = baseRadius + Mathf.Sin(Time.time * 3f) * radiusVariance;
                entityLight.TransitionLightIntensity(baseIntensity + flicker, newRadius, 0.1f);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    // Méthode pour démarrer l'effet arc-en-ciel des cheveux
    public void RainbowHair()
    {
        StartCoroutine(RainbowHairEffect());
    }

    private IEnumerator RainbowHairEffect()
    {
        while (true)
        {
            float duration = 2f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float hue = Mathf.Repeat(elapsedTime / duration, 1f);
                Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

                foreach (GameObject hair in hairs)
                {
                    SpriteRenderer sr = hair.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = rainbowColor;
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
