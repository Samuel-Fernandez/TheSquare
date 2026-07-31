using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TheSquare.Mechanics.UniverseHeart;

public class SquareEyeBehavior : MonoBehaviour
{
    public enum EyeOrientation { Up, Down, Left, Right }

    [Header("Settings")]
    public EyeOrientation orientation = EyeOrientation.Down;
    public float viewRadius = 5f;
    public float viewAngle = 45f;
    public float sweepAngle = 90f;
    public float sweepSpeed = 1f;

    [Header("References")]
    public Transform eyeTransform;

    private EntityLight entityLight;
    private Transform lightTransform;
    private SpriteRenderer spriteRenderer;

    [Header("Light Settings")]
    public Color lightColor = new Color(0.6f, 0f, 1f, 1f); // Violet par défaut
    public float lightIntensity = 1f;
    public float lightFalloff = 0.5f;

    [Header("Sprites")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteSide; // Regarde à droite par défaut
    public Sprite spritePhase2;

    [Header("Shake Settings")]
    public float shakeIntensity = 0.05f;

    private float baseRotationZ;
    private bool isInPhase2 = false;
    private Vector3 originalSpritePos;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        // Récupération automatique de l'EntityLight sur ce GameObject
        entityLight = GetComponent<EntityLight>();
        if (entityLight != null && entityLight.entityLight != null)
        {
            lightTransform = entityLight.entityLight.transform;
            
            // Configuration de la lumière pour qu'elle corresponde au script
            Light2D l2d = entityLight.entityLight.GetComponent<Light2D>();
            if (l2d != null)
            {
                l2d.lightType = Light2D.LightType.Point;
                l2d.color = lightColor;
                l2d.intensity = lightIntensity;
                
                // Radius
                l2d.pointLightInnerRadius = 0f;
                l2d.pointLightOuterRadius = viewRadius;
                
                // Angle
                l2d.pointLightInnerAngle = 0f;
                l2d.pointLightOuterAngle = viewAngle;
                
                l2d.falloffIntensity = lightFalloff;
            }
        }

        if (spriteRenderer != null)
        {
            originalSpritePos = spriteRenderer.transform.localPosition;
        }

        if (InsideTheSquareManager.instance != null)
        {
            InsideTheSquareManager.instance.squareEyes.Add(this);
        }

        SetOrientation();
    }

    private void SetOrientation()
    {
        switch (orientation)
        {
            case EyeOrientation.Up:
                baseRotationZ = 0f;
                if (spriteRenderer != null && spriteUp != null) spriteRenderer.sprite = spriteUp;
                if (spriteRenderer != null) spriteRenderer.flipX = false;
                break;
            case EyeOrientation.Left:
                baseRotationZ = 90f;
                if (spriteRenderer != null && spriteSide != null) spriteRenderer.sprite = spriteSide;
                if (spriteRenderer != null) spriteRenderer.flipX = true;
                break;
            case EyeOrientation.Down:
                baseRotationZ = 180f;
                if (spriteRenderer != null && spriteDown != null) spriteRenderer.sprite = spriteDown;
                if (spriteRenderer != null) spriteRenderer.flipX = false;
                break;
            case EyeOrientation.Right:
                baseRotationZ = -90f;
                if (spriteRenderer != null && spriteSide != null) spriteRenderer.sprite = spriteSide;
                if (spriteRenderer != null) spriteRenderer.flipX = false;
                break;
        }

        if (lightTransform != null)
        {
            lightTransform.localRotation = Quaternion.Euler(0, 0, baseRotationZ);
        }
    }

    private void Update()
    {
        // Si l'alerte a été déclenchée (par lui-même ou un autre)
        if (InsideTheSquareManager.player_is_revealed)
        {
            if (!isInPhase2)
            {
                EnterPhase2();
            }
            ShakeSprite();
            return;
        }

        // Phase 1 (Balayage)
        if (lightTransform != null)
        {
            SweepLight();
            CheckPlayerDetection();
        }
    }

    private void SweepLight()
    {
        if (eyeTransform != null && lightTransform != null)
        {
            lightTransform.position = eyeTransform.position;
        }

        // Mouvement fluide de va-et-vient avec un Sinus
        float offset = Mathf.Sin(Time.time * sweepSpeed) * (sweepAngle / 2f);
        lightTransform.localRotation = Quaternion.Euler(0, 0, baseRotationZ + offset);
    }

    private void CheckPlayerDetection()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null) return;
        if (eyeTransform == null) return; // Sécurité

        Transform playerTransform = PlayerManager.instance.player.transform;
        Vector2 toPlayer = playerTransform.position - eyeTransform.position;
        float distanceToPlayer = toPlayer.magnitude;

        // 1. Le joueur est-il assez proche ?
        if (distanceToPlayer > viewRadius) return;

        // 2. Le joueur est-il dans l'angle du faisceau ?
        // On utilise lightTransform.up comme direction du faisceau, car rotationZ = 0 pointe vers le haut
        float angleToPlayer = Vector2.Angle(lightTransform.up, toPlayer);
        if (angleToPlayer > viewAngle / 2f) return;

        // 3. Y a-t-il un mur entre l'œil et le joueur ?
        RaycastHit2D[] hits = Physics2D.LinecastAll(eyeTransform.position, playerTransform.position);
        foreach (var hit in hits)
        {
            if (hit.collider != null && !hit.collider.isTrigger && hit.transform != playerTransform && hit.transform != this.transform)
            {
                Stats s = hit.collider.GetComponent<Stats>();
                if (s != null && s.entityType == EntityType.Player) 
                {
                    continue; // On ignore si c'est un collider du joueur
                }
                
                // C'est un mur ou un obstacle, la vision est bloquée
                return;
            }
        }

        // Si on arrive ici, le joueur est vu !
        InsideTheSquareManager.TriggerReveal();
    }

    public void ResetToPhase1()
    {
        isInPhase2 = false;

        if (entityLight != null)
        {
            entityLight.TransitionLightIntensity(lightIntensity, viewRadius, 0.2f);
        }

        SetOrientation(); // Remet le bon sprite selon l'orientation

        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localPosition = originalSpritePos;
        }
    }

    private void EnterPhase2()
    {
        isInPhase2 = true;

        if (spriteRenderer != null && spritePhase2 != null)
        {
            spriteRenderer.sprite = spritePhase2;
        }

        if (entityLight != null)
        {
            // Transition pour éteindre la lumière (intensité et rayon à 0 en 0.2s)
            entityLight.TransitionLightIntensity(0f, 0f, 0.2f);
        }
    }

    private void ShakeSprite()
    {
        if (spriteRenderer != null)
        {
            // Tremblement très rapide mais léger
            Vector2 randomOffset = Random.insideUnitCircle * shakeIntensity;
            spriteRenderer.transform.localPosition = originalSpritePos + new Vector3(randomOffset.x, randomOffset.y, 0);
        }
    }
}
