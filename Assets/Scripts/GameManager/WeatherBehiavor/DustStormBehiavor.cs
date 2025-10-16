using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DustStormBehavior : MonoBehaviour
{
    [Header("Sprites de poussiere (1920x1080 recommandes)")]
    public List<Sprite> sprites;

    [Header("Reglages du spawn")]
    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 2f;

    [Header("Reglages duree de vie")]
    public float minLifetime = 1f;
    public float maxLifetime = 3f;

    [Header("Mouvement aleatoire (en pixels ecran)")]
    public float moveDistance = 200f;
    public float moveSpeed = 30f;

    [Header("Masque circulaire")]
    public float maskRadius = 200f;
    public float maskSoftness = 100f;
    public Vector2 maskCenter = new Vector2(960f, 540f);

    private Canvas canvas;
    private Image mask;
    private List<DustParticle> activeParticles = new List<DustParticle>();

    // Image independante du masque
    private Image independentDust;

    private class DustParticle
    {
        public Image image;
        public RectTransform rectTransform;
        public float baseAlpha;

        public DustParticle(Image img, RectTransform rt)
        {
            image = img;
            rectTransform = rt;
            baseAlpha = 1f;
        }
    }

    void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Aucun Canvas trouve en enfant de DustStormBehavior !");
        }

        if (canvas != null)
        {
            RectTransform canvasRT = canvas.GetComponent<RectTransform>();
            if (canvasRT != null)
            {
                maskCenter = new Vector2(canvasRT.sizeDelta.x * 0.5f, canvasRT.sizeDelta.y * 0.5f);
            }
        }
    }

    void Start()
    {
        if (canvas == null)
        {
            Debug.LogError("Canvas non present, arret du script DustStormBehavior.");
            enabled = false;
            return;
        }

        // Recuperer le masque
        mask = canvas.GetComponentInChildren<Image>();
        if (mask == null)
        {
            Debug.LogError("Aucun masque trouve dans le Canvas !");
        }

        // Lancer routines
        StartCoroutine(SpawnRoutine());
        StartCoroutine(UpdateMaskRoutine());
        StartCoroutine(IndependentDustRoutine());
    }

    // Routine pour les particules masquées
    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
            SpawnDust();
        }
    }

    private IEnumerator UpdateMaskRoutine()
    {
        while (true)
        {
            UpdateAllParticlesMask();
            yield return new WaitForSeconds(0.02f);
        }
    }

    private void UpdateAllParticlesMask()
    {
        activeParticles.RemoveAll(p => p.image == null);

        foreach (var particle in activeParticles)
        {
            if (particle.image != null)
            {
                UpdateParticleMask(particle);
            }
        }
    }

    private void UpdateParticleMask(DustParticle particle)
    {
        Vector2 particlePos = particle.rectTransform.anchoredPosition;
        float distanceToCenter = Vector2.Distance(particlePos, maskCenter);

        float maskAlpha = 1f;
        if (distanceToCenter < maskRadius + maskSoftness)
        {
            if (distanceToCenter <= maskRadius)
                maskAlpha = 0f;
            else
                maskAlpha = Mathf.Clamp01((distanceToCenter - maskRadius) / maskSoftness);
        }

        Color c = particle.image.color;
        c.a = particle.baseAlpha * maskAlpha;
        particle.image.color = c;
    }

    private void SpawnDust()
    {
        if (sprites.Count == 0 || mask == null) return;

        GameObject go = new GameObject("DustParticle");
        go.transform.SetParent(mask.transform, false);
        Image img = go.AddComponent<Image>();
        img.sprite = sprites[Random.Range(0, sprites.Count)];
        img.type = Image.Type.Tiled;
        img.color = new Color(1f, 1f, 1f, 0f);
        img.pixelsPerUnitMultiplier = 16f;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1920f, 1080f);
        rt.localScale = Vector3.one * 10f;
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        float lifetime = Random.Range(minLifetime, maxLifetime);
        Vector2 randomOffset = Random.insideUnitCircle.normalized * (moveDistance * 2f);

        DustParticle dustParticle = new DustParticle(img, rt);
        activeParticles.Add(dustParticle);

        StartCoroutine(DustLifeRoutine(dustParticle, lifetime, randomOffset));
    }

    private IEnumerator DustLifeRoutine(DustParticle particle, float lifetime, Vector2 moveOffset)
    {
        float fadeTime = 0.5f;
        Vector2 startPos = particle.rectTransform.anchoredPosition;
        Vector2 endPos = startPos + moveOffset;

        float t = 0f;
        while (t < lifetime && particle.image != null)
        {
            float progress = t / lifetime;
            particle.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);

            if (t < fadeTime)
                particle.baseAlpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            else if (t > lifetime - fadeTime)
                particle.baseAlpha = Mathf.Lerp(1f, 0f, (t - (lifetime - fadeTime)) / fadeTime);
            else
                particle.baseAlpha = 1f;

            t += Time.deltaTime;
            yield return null;
        }

        if (particle.image != null)
            Destroy(particle.rectTransform.gameObject);
    }

    // ----- Gestion de l'image independante du masque -----
    private void CreateIndependentDust()
    {
        if (independentDust != null) return;

        GameObject go = new GameObject("IndependentDust");
        go.transform.SetParent(canvas.transform, false); // pas enfant du masque
        independentDust = go.AddComponent<Image>();
        independentDust.sprite = sprites[Random.Range(0, sprites.Count)];
        independentDust.type = Image.Type.Tiled;
        independentDust.color = new Color(1f, 1f, 1f, 0.05f); // opacite 0.2
        independentDust.pixelsPerUnitMultiplier = 16f; // comme les autres

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1920f, 1080f);
        rt.localScale = Vector3.one * 10f;
        rt.anchoredPosition = Vector2.zero;

        // Rotation fixe comme les autres particules
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        // Mouvement identique aux particules, mais rotation constante
        Vector2 randomOffset = Random.insideUnitCircle.normalized * (moveDistance * 2f);
        StartCoroutine(IndependentDustMovement(rt, randomOffset));
    }

    private IEnumerator IndependentDustMovement(RectTransform rt, Vector2 moveOffset)
    {
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + moveOffset;
        float moveDuration = 5f; // ajustable pour correspondre à moveSpeed des particules

        while (independentDust != null)
        {
            float progress = Mathf.PingPong(Time.time / moveDuration, 1f);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);

            // Rotation fixe, ne pas changer
            yield return null;
        }
    }



    private IEnumerator IndependentDustRoutine()
    {
        while (true)
        {
            if (independentDust == null)
            {
                CreateIndependentDust();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
