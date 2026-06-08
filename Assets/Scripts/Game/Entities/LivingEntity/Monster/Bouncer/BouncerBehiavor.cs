using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Stats), typeof(Rigidbody2D))]
public class BouncerBehiavor : MonoBehaviour
{
    [Header("Jump Settings")]
    public float baseJumpCooldown = 3f;
    public float minJumpCooldown = 0.5f;
    public float jumpDuration = 1f;
    public float jumpSpeed = 5f;

    [Header("Animations")]
    public string idleAnimation = "Idle";
    public string jumpAnimation = "Jump";
    public string airAnimation = "Air";

    [Header("Visual Effects")]
    public Transform shadowTransform;
    public float jumpVisualHeight = 1f;
    public float jumpScaleMultiplier = 1.2f;

    private Stats stats;
    private Rigidbody2D rb;
    private ObjectAnimation objectAnimation;
    private LifeManager lifeManager;
    private SpriteRenderer spriteRenderer;
    private SoundContainer soundContainer;
    
    // Tailles et positions initiales pour les effets visuels
    private Vector3 baseSpriteScale = Vector3.one;
    private Vector3 baseSpriteLocalPos = Vector3.zero;
    private Vector3 baseShadowScale = Vector3.one;
    
    // We will disable trigger colliders to prevent damaging the player while jumping.
    private Collider2D[] allColliders;

    private float jumpTimer;
    private bool isJumping = false;
    private Vector2 moveDirection;

    private void Start()
    {
        stats = GetComponent<Stats>();
        rb = GetComponent<Rigidbody2D>();
        objectAnimation = GetComponent<ObjectAnimation>();
        lifeManager = GetComponent<LifeManager>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        soundContainer = GetComponent<SoundContainer>();
        allColliders = GetComponentsInChildren<Collider2D>();

        if (spriteRenderer != null)
        {
            baseSpriteScale = spriteRenderer.transform.localScale;
            baseSpriteLocalPos = spriteRenderer.transform.localPosition;
        }

        if (shadowTransform == null)
        {
            Transform shadow = transform.Find("Shadow");
            if (shadow == null) shadow = transform.Find("Ombre");
            if (shadow == null) shadow = transform.Find("ombre");
            shadowTransform = shadow;
        }

        if (shadowTransform != null)
        {
            baseShadowScale = shadowTransform.localScale;
        }

        // Set initial timer
        jumpTimer = GetCurrentJumpCooldown();
    }

    private void Update()
    {
        if (MeteoManager.instance != null && !MeteoManager.instance.time) return;
        if (stats != null && (!stats.canMove || stats.isDying)) return;

        if (!isJumping)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0f)
            {
                StartCoroutine(JumpRoutine());
                jumpTimer = GetCurrentJumpCooldown();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isJumping && rb != null)
        {
            rb.velocity = moveDirection * jumpSpeed;
        }
    }

    private IEnumerator JumpRoutine()
    {
        isJumping = true;
        moveDirection = Vector2.zero; // On s'arrête pour la préparation
        if (rb != null) rb.velocity = Vector2.zero;

        // Début de l'animation de préparation (Jump)
        if (objectAnimation != null) objectAnimation.PlayAnimation(jumpAnimation);

        // Délai de 1 seconde avant de décoller (le temps de jouer Jump)
        yield return new WaitForSeconds(1f);

        // Décollage (Air)
        if (stats != null) stats.SetVulnerability(false);
        SetDamageTriggersEnabled(false);

        if (soundContainer != null) soundContainer.PlaySound("Jump", 1); 
        if (objectAnimation != null) objectAnimation.PlayAnimation(airAnimation);

        // Direction purement aléatoire
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
        UpdateSpriteDirection(moveDirection);

        float timer = 0f;
        float actualJumpTime = jumpDuration;

        while (timer < actualJumpTime)
        {
            timer += Time.deltaTime;
            float progress = timer / actualJumpTime;
            
            // Courbe en cloche (parabole) de 0 à 1 puis à 0
            float heightCurve = Mathf.Sin(progress * Mathf.PI);

            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localPosition = baseSpriteLocalPos + new Vector3(0f, heightCurve * jumpVisualHeight, 0f);
                spriteRenderer.transform.localScale = baseSpriteScale * (1f + (jumpScaleMultiplier - 1f) * heightCurve);
            }

            if (shadowTransform != null)
            {
                // L'ombre rétrécit légèrement pendant le saut
                shadowTransform.localScale = baseShadowScale * (1f - 0.3f * heightCurve);
            }

            yield return null;
        }

        // Réinitialisation des visuels à la fin du saut
        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localPosition = baseSpriteLocalPos;
            spriteRenderer.transform.localScale = baseSpriteScale;
        }
        if (shadowTransform != null)
        {
            shadowTransform.localScale = baseShadowScale;
        }

        // Fin du saut
        isJumping = false;
        if (rb != null) rb.velocity = Vector2.zero;
        if (stats != null) stats.SetVulnerability(true);
        SetDamageTriggersEnabled(true);
        if (objectAnimation != null) objectAnimation.PlayAnimation(idleAnimation);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isJumping) return;

        // S'il touche un obstacle et pas le joueur, on rebondit
        if (collision.gameObject.GetComponent<PlayerController>() == null)
        {
            // Calcul du vecteur réfléchi par la normale du contact
            ContactPoint2D contact = collision.contacts[0];
            moveDirection = Vector2.Reflect(moveDirection, contact.normal).normalized;
            UpdateSpriteDirection(moveDirection);
            
            if (soundContainer != null) soundContainer.PlaySound("Bounce", 1);
        }
    }

    private float GetCurrentJumpCooldown()
    {
        if (lifeManager == null || stats == null) return baseJumpCooldown;

        float hpPercentage = (float)lifeManager.life / (stats.health <= 0 ? 1 : stats.health);
        hpPercentage = Mathf.Clamp01(hpPercentage);
        return Mathf.Lerp(minJumpCooldown, baseJumpCooldown, hpPercentage);
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (spriteRenderer != null)
        {
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                // Ajuster selon l'orientation de base de vos sprites de monstre
                spriteRenderer.flipX = direction.x < 0; 
            }
        }
    }

    private void SetDamageTriggersEnabled(bool isEnabled)
    {
        if (allColliders == null) return;
        
        foreach (var col in allColliders)
        {
            // Only disable triggers so the physics collider remains active for bouncing
            if (col != null && col.isTrigger)
            {
                col.enabled = isEnabled;
            }
        }
    }
}
