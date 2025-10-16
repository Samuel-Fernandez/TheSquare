using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager instance;

    public Light2D sunLight;

    public float intensity;
    public float radius;
    public bool isOn;
    public Color color;

    private void Awake()
    {
        // Assurez-vous qu'il n'y a qu'une seule instance de LightManager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Méthode publique pour régler l'intensité de la lumière Sun
    public void SetSunIntensity(float intensity)
    {
        if (sunLight != null)
        {
            sunLight.intensity = intensity;
        }
    }

    // Méthode publique pour régler le rayon (radius) de la lumière Sun
    public void SetSunRadius(float radius)
    {
        if (sunLight != null)
        {
            sunLight.pointLightOuterRadius = radius;
        }
    }

    // Exemple de méthode pour allumer/éteindre la lumière Sun
    public void ToggleSunLight(bool isOn)
    {
        if (sunLight != null)
        {
            sunLight.enabled = isOn;
        }
    }

    // Exemple de méthode pour changer la couleur de la lumière Sun
    public void SetSunColor(Color color)
    {
        if (sunLight != null)
        {
            sunLight.color = color;
        }
    }
}
