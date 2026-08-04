using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class BigBallRockBehiavor : MonoBehaviour
{
    [Header("Damage")]
    [Range(0f, 1f)] public float damagePercentOfPlayerMaxHealth = 0.1f;

    [Header("Roll Movement")]
    public float speed = 3f;
    public Vector2 direction = Vector2.right;

    [Header("Sky Fall")]
    public bool comesFromSky = false;
    public float skyHeight = 5f;
    public float fallDuration = 1f;
    public Vector2 shadowExpandedScale = new Vector2(10f, 2.5f);

    [Header("Sound")]
    public float rollSoundInterval = 3f;

    [Header("References")]
    public Transform shadowTransform;

    private Collider2D ballCollider;
    private SoundContainer soundContainer;
    private SpriteRenderer spriteRenderer;
    private Transform spriteTransform;

    private Vector3 baseSpriteLocalPos = Vector3.zero;

    private bool isRolling = false;
    private bool isFallingIntoHole = false;
    private Coroutine rollSoundCoroutine;

    void Start()
    {
        ballCollider = GetComponent<Collider2D>();
        soundContainer = GetComponent<SoundContainer>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteTransform = spriteRenderer != null ? spriteRenderer.transform : null;

        if (shadowTransform == null)
        {
            Transform shadow = transform.Find("Shadow");
            if (shadow == null) shadow = transform.Find("Ombre");
            if (shadow == null) shadow = transform.Find("ombre");
            shadowTransform = shadow;
        }

        if (spriteTransform != null)
            baseSpriteLocalPos = spriteTransform.localPosition;

        UpdateSpriteFlip();

        if (comesFromSky)
        {
            StartCoroutine(FallFromSkyRoutine());
        }
        else
        {
            ballCollider.enabled = true;
            BeginRolling();
        }
    }

    // Doit être appelée avant que Start() ne s'exécute (ex: juste après Instantiate)
    public void Init(float speed, Vector2 direction, bool comesFromSky)
    {
        this.speed = speed;
        this.direction = direction.normalized;
        this.comesFromSky = comesFromSky;
    }

    void FixedUpdate()
    {
        if (!isRolling) return;

        transform.position += (Vector3)(direction.normalized * speed * Time.fixedDeltaTime);
    }

    private IEnumerator FallFromSkyRoutine()
    {
        ballCollider.enabled = false;

        Vector3 startSpritePos = baseSpriteLocalPos + new Vector3(0f, skyHeight, 0f);
        Vector3 endSpritePos = baseSpriteLocalPos;
        if (spriteTransform != null)
            spriteTransform.localPosition = startSpritePos;

        float shadowBaseZ = shadowTransform != null ? shadowTransform.localScale.z : 1f;
        Vector3 shadowStartScale = new Vector3(0f, 0f, shadowBaseZ);
        Vector3 shadowEndScale = new Vector3(shadowExpandedScale.x, shadowExpandedScale.y, shadowBaseZ);
        if (shadowTransform != null)
            shadowTransform.localScale = shadowStartScale;

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            float t = elapsed / fallDuration;

            if (spriteTransform != null)
                spriteTransform.localPosition = Vector3.Lerp(startSpritePos, endSpritePos, t * t); // ease-in : effet de chute

            if (shadowTransform != null)
                shadowTransform.localScale = Vector3.Lerp(shadowStartScale, shadowEndScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spriteTransform != null)
            spriteTransform.localPosition = endSpritePos;
        if (shadowTransform != null)
            shadowTransform.localScale = shadowEndScale;

        if (soundContainer != null)
            soundContainer.PlaySound("Impact", 2);

        ballCollider.enabled = true;
        BeginRolling();
    }

    private void BeginRolling()
    {
        isRolling = true;
        if (rollSoundCoroutine == null)
            rollSoundCoroutine = StartCoroutine(RollSoundRoutine());
    }

    private IEnumerator RollSoundRoutine()
    {
        while (true)
        {
            if (soundContainer != null)
                soundContainer.PlaySound("Roll", 2);

            yield return new WaitForSeconds(rollSoundInterval);
        }
    }

    private void UpdateSpriteFlip()
    {
        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.01f)
            spriteRenderer.flipX = direction.x < 0f;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Stats targetStats = collision.gameObject.GetComponent<Stats>();
        if (targetStats != null && targetStats.entityType == EntityType.Player)
        {
            LifeManager targetLife = collision.gameObject.GetComponent<LifeManager>();
            int damage = Mathf.RoundToInt(targetStats.health * damagePercentOfPlayerMaxHealth);
            targetLife.TakeDamage(damage, gameObject, false);
            targetLife.KnockBack(collision.gameObject, targetStats.knockbackResistance + 10, gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isRolling || isFallingIntoHole) return;
        if (collision.tag == null || !collision.tag.StartsWith("Hole")) return;

        Tilemap tilemap = collision.GetComponent<Tilemap>();
        if (tilemap == null) return;

        Vector3Int cell = tilemap.WorldToCell(transform.position);
        if (!tilemap.HasTile(cell)) return;

        StartCoroutine(FallIntoHoleRoutine(tilemap.GetCellCenterWorld(cell)));
    }

    private IEnumerator FallIntoHoleRoutine(Vector3 holeCenter)
    {
        isFallingIntoHole = true;
        isRolling = false;
        ballCollider.enabled = false;

        if (rollSoundCoroutine != null)
        {
            StopCoroutine(rollSoundCoroutine);
            rollSoundCoroutine = null;
        }

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPos, holeCenter, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
