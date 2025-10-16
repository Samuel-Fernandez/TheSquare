using System.Collections;
using UnityEngine;

public class AraxieFairy : MonoBehaviour
{
    private Vector3 humanScale;
    private Vector3 fairyScale;
    private Coroutine scaleCoroutine;
    private EntityLight entityLight;

    private void Awake()
    {
        humanScale = new Vector2(1, 1);
        fairyScale = humanScale * 0.5f;
        entityLight = GetComponent<EntityLight>();
    }

    public void TransformToFairy()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Transformation", false, false);
        GetComponent<SoundContainer>().PlaySound("AraxieToFairy", 0);
        GetComponent<Collider2D>().enabled = false;

        entityLight.TransitionLightIntensity(2f, 2, 0.5f);
        StartScaleTransition(fairyScale, 0.5f);

        StartCoroutine(PlayAfkAnimationDelayed("FairyAfk", 0.5f));
    }

    public void TransformToHuman()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Transformation", false, true);
        GetComponent<SoundContainer>().PlaySound("FairyToAraxie", 0);
        GetComponent<Collider2D>().enabled = true;

        entityLight.TransitionLightIntensity(0.25f, 1, 0.5f);
        StartScaleTransition(humanScale, 0.5f);

        StartCoroutine(PlayAfkAnimationDelayed("AfkDown", 0.5f));
    }

    public void InstantTransformToFairy()
    {
        // Arrêt des transitions en cours
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        transform.localScale = fairyScale;
        entityLight.SetLightIntensity(2f, 2); // Méthode supposée exister dans EntityLight
        GetComponent<Collider2D>().enabled = false;

        GetComponent<SoundContainer>().PlaySound("AraxieToFairy", 0);


        GetComponent<ObjectAnimation>().PlayAnimation("FairyAfk", false, false);
    }

    public void InstantTransformToHuman()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        transform.localScale = humanScale;
        entityLight.SetLightIntensity(0.25f, 1); // Méthode supposée exister dans EntityLight
        GetComponent<Collider2D>().enabled = true;

        GetComponent<ObjectAnimation>().PlayAnimation("AfkDown", false, false);
        GetComponent<SoundContainer>().PlaySound("FairyToAraxie", 0);

    }

    private void StartScaleTransition(Vector3 targetScale, float duration)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleOverTime(targetScale, duration));
    }

    private IEnumerator ScaleOverTime(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private IEnumerator PlayAfkAnimationDelayed(string animationName, float delay)
    {
        yield return new WaitForSeconds(delay);
        GetComponent<ObjectAnimation>().PlayAnimation(animationName, false, false);
    }
}
