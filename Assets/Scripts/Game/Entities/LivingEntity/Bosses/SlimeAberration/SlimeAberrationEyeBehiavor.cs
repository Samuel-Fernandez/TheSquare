using System.Collections;
using UnityEngine;

public class SlimeAberrationEyeBehiavor : MonoBehaviour
{
    public GameObject laserPrefab;
    public GameObject laserInstance;
    GameObject slimeAberration;

    public void Init(GameObject slimeAberration)
    {
        this.slimeAberration = slimeAberration;
        GetComponent<ObjectAnimation>().PlayAnimation("Appear", true);
        GetComponent<SoundContainer>().PlaySound("Open", 2);
    }

    public void Remove()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Disappear", true);
        GetComponent<SoundContainer>().PlaySound("Open", 2);
        Destroy(gameObject, .5f);
    }

    // Méthode pour faire apparaître le laser
    public IEnumerator SpawnAndSweepLaser(int laserDuration = 4)
    {
        Vector2 center = new Vector2(transform.position.x, transform.position.y);

        if (laserPrefab == null)
        {
            Debug.LogError("laserPrefab not assigned on " + name);
            yield break;
        }

        laserInstance = Instantiate(laserPrefab, center, Quaternion.identity);

        LaserBehavior laser = laserInstance.GetComponent<LaserBehavior>();

        // Optionnel : initialiser strength/points si tu veux
        laser.Init(center, center + Vector2.down * 14.142f, slimeAberration.GetComponent<Stats>().strength, slimeAberration);

        float duration = laserDuration; // un aller = 2s
        float radius = Mathf.Sqrt(2f) * 10f;

        yield return SweepArc(center, -180, 0, radius, duration, laser);
        yield return SweepArc(center, 0, -180, radius, duration, laser);

        Destroy(laserInstance);
        laserInstance = null;
    }

    private IEnumerator SweepArc(Vector2 center, float startAngleDeg, float endAngleDeg, float radius, float duration, LaserBehavior laser)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // easing (régule la vitesse et fait "ralentir progressivement")
            float eased = Mathf.SmoothStep(0f, 1f, t);

            float angleRad = Mathf.Lerp(startAngleDeg, endAngleDeg, eased) * Mathf.Deg2Rad;
            Vector2 a = center;
            Vector2 b = center + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;

            laser.UpdateLaser(a, b);

            yield return null;
        }
    }
}
