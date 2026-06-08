using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceWizardBehiavor : MonoBehaviour
{
    [Header("Detection")]
    public float radiusDetection = 6f;

    [Header("Combat")]
    public GameObject attackPrefab;
    public GameObject invulnerabilityPrefab;

    [Header("Timings / Settings")]
    public float waitDuration = 3f;
    public float fadeDuration = 0.5f;
    public int attackSpawnCount = 6;
    public float attackSpawnInterval = 0.5f;

    [Header("Lights")]
    public Color attackLightColor = Color.cyan;

    bool isAppeared = false;
    Coroutine currentMainRoutine;
    SpriteRenderer spriteRenderer;
    EntityLight entityLight;
    GameObject currentShield;

    float baseLightIntensity = 1f;
    float baseLightRadius = 5f;
    Color baseLightColor = Color.white;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        entityLight = GetComponentInChildren<EntityLight>();

        var light2D = GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
        if (light2D != null)
        {
            baseLightIntensity = light2D.intensity;
            baseLightRadius = light2D.pointLightOuterRadius;
            baseLightColor = light2D.color;
        }
    }

    private void Update()
    {
        if (PlayerManager.instance?.player == null) return;

        bool shouldFlipLeft = PlayerManager.instance.player.transform.position.x < transform.position.x;
        FlipSprite(shouldFlipLeft);

        HandleAppearance();
    }

    void HandleAppearance()
    {
        bool playerInRadius = PlayerIsInRadius(radiusDetection);

        if (playerInRadius && !isAppeared)
        {
            isAppeared = true;
            if (currentMainRoutine != null)
                StopCoroutine(currentMainRoutine);

            currentMainRoutine = StartCoroutine(WizardMainRoutine());

            if (GetComponent<SoundContainer>() != null)
                GetComponent<SoundContainer>().PlaySound("Appear", 2);
        }
        else if (!playerInRadius && isAppeared)
        {
            isAppeared = false;
            if (currentMainRoutine != null)
            {
                StopCoroutine(currentMainRoutine);
                currentMainRoutine = null;
            }

            var stats = GetComponent<Stats>();
            if (stats != null) stats.SetVulnerability(true);

            if (currentShield != null) Destroy(currentShield);

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                spriteRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }
        }
    }

    void FlipSprite(bool flipLeft)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.flipX = flipLeft;
    }

    bool PlayerIsInRadius(float maxRadiusDistance)
    {
        if (PlayerManager.instance?.player == null) return false;
        return Vector2.Distance(transform.position, PlayerManager.instance.player.transform.position) <= maxRadiusDistance;
    }

    IEnumerator WizardMainRoutine()
    {
        while (isAppeared)
        {
            float actionRoll = Random.value;

            if (actionRoll <= 0.3f)
            {
                yield return StartCoroutine(AttackRoutine());
            }
            else if (actionRoll <= 0.8f)
            {
                yield return StartCoroutine(TeleportRoutine());
            }
            else
            {
                yield return new WaitForSeconds(waitDuration);
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        var objectAnimation = GetComponent<ObjectAnimation>();

        if (objectAnimation != null)
        {
            yield return objectAnimation.PlayAnimationCoroutine("StartAttack", true);
        }

        if (GetComponent<SoundContainer>() != null)
            GetComponent<SoundContainer>().PlaySound("Attack", 2);

        var stats = GetComponent<Stats>();
        if (stats != null) stats.SetVulnerability(false);

        if (invulnerabilityPrefab != null)
        {
            Vector3 shieldPos = transform.position + new Vector3(0, 0.5f, 0);
            currentShield = Instantiate(invulnerabilityPrefab, shieldPos, Quaternion.identity, transform);
        }

        if (entityLight != null)
        {
            entityLight.TransitionLightColor(attackLightColor, 0.2f);
            entityLight.TransitionLightIntensity(baseLightIntensity * 3f, baseLightRadius * 3f, 0.2f);
        }

        for (int i = 0; i < attackSpawnCount; i++)
        {
            if (PlayerManager.instance?.player != null && attackPrefab != null)
            {
                GameObject spike = Instantiate(attackPrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
                IceWizardSpikeBehavior behavior = spike.GetComponent<IceWizardSpikeBehavior>();
                if (behavior != null && stats != null)
                {
                    behavior.Init(stats.strength);
                }
            }
            yield return new WaitForSeconds(attackSpawnInterval);
        }

        if (entityLight != null)
        {
            entityLight.TransitionLightColor(baseLightColor, 0.2f);
            entityLight.TransitionLightIntensity(baseLightIntensity, baseLightRadius, 0.2f);
        }

        if (currentShield != null) Destroy(currentShield);
        if (stats != null) stats.SetVulnerability(true);

        if (objectAnimation != null)
        {
            yield return objectAnimation.PlayAnimationCoroutine("EndAttack", true);
        }
    }

    IEnumerator TeleportRoutine()
    {
        if (GetComponent<SoundContainer>() != null)
            GetComponent<SoundContainer>().PlaySound("Teleport", 2);

        if (entityLight != null)
            entityLight.TransitionLightIntensity(0f, 0f, fadeDuration);

        yield return StartCoroutine(FadeSprite(0f));

        Vector2 validPos = transform.position;
        bool found = false;
        int maxTries = 30;

        while (!found && maxTries > 0)
        {
            maxTries--;
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDist = Random.Range(2f, 3f);
            Vector2 dir = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

            Vector2 targetPos = (Vector2)PlayerManager.instance.player.transform.position + dir * randomDist;

            RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, targetPos);
            bool isBlocked = false;
            foreach (var hit in hits)
            {
                if (hit.collider != null && !hit.collider.isTrigger)
                {
                    if (hit.collider.gameObject != PlayerManager.instance.player.gameObject && hit.collider.gameObject != this.gameObject)
                    {
                        isBlocked = true;
                        break;
                    }
                }
            }

            if (!isBlocked)
            {
                found = true;
                validPos = targetPos;
            }
        }

        transform.position = validPos;

        if (entityLight != null)
            entityLight.TransitionLightIntensity(baseLightIntensity, baseLightRadius, fadeDuration);

        yield return StartCoroutine(FadeSprite(1f));
    }

    IEnumerator FadeSprite(float targetAlpha)
    {
        if (spriteRenderer == null) yield break;

        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float a = Mathf.Lerp(startColor.a, targetAlpha, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, a);
            yield return null;
        }

        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radiusDetection);
    }
}
