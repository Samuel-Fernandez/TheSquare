using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class LaserBehavior : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public GameObject launcher;

    private int strength = 1;

    private BoxCollider2D col;
    private Transform laserVisual;
    private SpriteRenderer sr;
    private Light2D laserLight;

    public void Init(Vector2 A, Vector2 B, int strength, GameObject launcher)
    {
        this.pointA = A;
        this.pointB = B;
        this.strength = strength;
        this.launcher = launcher;
    }

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        laserVisual = transform.GetChild(0);
        sr = laserVisual.GetComponent<SpriteRenderer>();

        CreateLaserLight();
        StartCoroutine(FadeLightIntensity());
        StartCoroutine(BlinkAlpha());
    }

    void Update()
    {
        UpdateLaser(pointA, pointB);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.GetComponent<Stats>() && collision.GetComponent<Stats>().entityType == EntityType.Player)
        {
            collision.GetComponent<LifeManager>().TakeDamage(this.strength, false);
        }
    }

    public void UpdateLaser(Vector2 a, Vector2 b)
    {
        Vector2 direction = (b - a).normalized;
        float maxDistance = Vector2.Distance(a, b);
        float finalDistance = maxDistance;

        RaycastHit2D[] hits = Physics2D.RaycastAll(a, direction, maxDistance);

        foreach (var hit in hits)
        {
            if (hit.collider != null && !hit.collider.isTrigger && hit.collider.gameObject.layer != LayerMask.NameToLayer("LowHeight") && hit.collider.gameObject != launcher)
            {
                finalDistance = hit.distance;
                break;
            }
        }

        // Position et rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.position = a;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        // Échelle du sprite
        Vector3 scale = laserVisual.localScale;
        scale.y = finalDistance;
        laserVisual.localScale = scale;

        // Collider
        col.isTrigger = true;
        col.size = new Vector2(col.size.x, finalDistance);
        col.offset = new Vector2(0, finalDistance / 2f);

        // Mise à jour rayon lumière
        if (laserLight != null)
        {
            laserLight.pointLightOuterRadius = finalDistance;
        }
    }

    private IEnumerator BlinkAlpha()
    {
        bool toggle = false;
        while (true)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = toggle ? 1f : 0.5f;
                sr.color = c;
            }
            toggle = !toggle;

            GetComponent<SoundContainer>().PlaySound("Laser", 2);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator FadeLightIntensity()
    {
        float minIntensity = 0.7f;
        float maxIntensity = 3f;
        float halfDuration = 0.25f; // total 0.5s cycle

        while (true)
        {
            // Fade montée
            float timer = 0f;
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = timer / halfDuration;
                if (laserLight != null)
                    laserLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
                yield return null;
            }
            // Fade descente
            timer = 0f;
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = timer / halfDuration;
                if (laserLight != null)
                    laserLight.intensity = Mathf.Lerp(maxIntensity, minIntensity, t);
                yield return null;
            }
        }
    }

    private void CreateLaserLight()
    {
        GameObject lightObj = new GameObject("LaserLight");
        lightObj.transform.parent = laserVisual;
        lightObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        lightObj.transform.localRotation = Quaternion.identity;

        laserLight = lightObj.AddComponent<Light2D>();
        laserLight.lightType = Light2D.LightType.Point;
        laserLight.color = Color.red;
        laserLight.intensity = 1f;
        laserLight.pointLightOuterRadius = 1f;
        laserLight.pointLightInnerRadius = 0f;
    }
}
