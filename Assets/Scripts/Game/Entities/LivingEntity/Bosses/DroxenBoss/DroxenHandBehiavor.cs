using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroxenHandBehiavor : MonoBehaviour
{
    public int life;
    public GameObject eyeWeakness;
    public GameObject explosionEffect;
    public Vector2 basePosition;
    public bool isLeftHand = true;
    Stats stats;

    private Vector2 currentTargetOffset;
    private float changeTargetTimer = 0f;
    private float changeTargetInterval = 2f;
    private SpriteRenderer spriteRenderer;
    private bool isPunching = false;
    private bool hasCollided = false;
    bool closedHand;

    private void Awake()
    {
        // Récupérer le SpriteRenderer (sur l'objet ou son enfant)
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Mettre le sprite complètement blanc IMMÉDIATEMENT
        spriteRenderer.color = new Color(10f, 10f, 10f, 1f);
    }

    private void Start()
    {
        stats = GetComponent<Stats>();
        basePosition = transform.position;
        currentTargetOffset = Vector2.zero;

        // Démarrer la coroutine de fade
        StartCoroutine(WhiteFadeRoutine());
    }

    private IEnumerator WhiteFadeRoutine()
    {
        // Désactiver le mouvement pendant le fade
        stats.canMove = false;

        // Créer une couleur complètement blanche (surexposée)
        Color whiteColor = new Color(10f, 10f, 10f, 1f);
        Color normalColor = Color.white;

        // Attendre 1 seconde (le sprite est déjà blanc depuis Awake)
        yield return new WaitForSeconds(1f);

        // Transition du blanc au normal en 1 seconde
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Interpolation douce
            float smoothT = t * t * (3f - 2f * t);

            // Passer du blanc surexposé à la couleur normale
            spriteRenderer.color = Color.Lerp(whiteColor, normalColor, smoothT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // S'assurer que la couleur est bien normale à la fin
        spriteRenderer.color = normalColor;

        // Réactiver le mouvement
        stats.canMove = true;
    }

    private void Update()
    {
        eyeWeakness.SetActive(!closedHand);
        if (stats.canMove)
        {
            changeTargetTimer += Time.deltaTime;

            if (changeTargetTimer >= changeTargetInterval)
            {
                GetComponent<SoundContainer>().PlaySound("Moving", 2);
                float randomX = Random.Range(-2f, 2f);
                currentTargetOffset = new Vector2(randomX, 0);
                changeTargetTimer = 0f;
            }

            Vector3 targetPos = basePosition + currentTargetOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 3f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si on est en train de puncher et qu'on touche quelque chose qui n'est pas un trigger
        if (isPunching && !collision.collider.isTrigger)
        {
            hasCollided = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Au cas où OnCollisionEnter2D serait manqué
        if (isPunching && !collision.collider.isTrigger)
        {
            hasCollided = true;
        }
    }

    public IEnumerator SmashGround()
    {
        stats.canMove = false;

        yield return GetComponent<ObjectAnimation>().PlayAnimationCoroutine("Close", true);
        closedHand = true;

        // Récupère le sprite (premier enfant)
        Transform sprite = GetComponentInChildren<SpriteRenderer>().transform;
        Vector3 startPos = sprite.localPosition;
        Vector3 downPos = startPos + new Vector3(0, -0.35f, 0);

        float downDuration = 0.1f;
        float upDuration = 0.4f;

        int smashCount = 5;

        for (int i = 0; i < smashCount; i++)
        {
            float elapsed = 0f;

            // --- Descente rapide ---
            while (elapsed < downDuration)
            {
                float t = elapsed / downDuration;
                t = t * t; // acceleration
                sprite.localPosition = Vector3.Lerp(startPos, downPos, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            sprite.localPosition = downPos;

            // Effet d'impact
            CameraManager.instance.ShakeCamera(3, 3, 1);
            GetComponent<SoundContainer>().PlaySound("Impact", 2);

            // --- Remontée lente ---
            elapsed = 0f;
            while (elapsed < upDuration)
            {
                float t = elapsed / upDuration;
                t = 1 - Mathf.Pow(1 - t, 2); // décélération
                sprite.localPosition = Vector3.Lerp(downPos, startPos, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            sprite.localPosition = startPos;

            // Pause entre les frappes sauf après la dernière
            if (i < smashCount - 1)
                yield return new WaitForSeconds(1f);
        }

        // Ouvre la main après les 5 frappes
        yield return GetComponent<ObjectAnimation>().PlayAnimationCoroutine("Open", true);
        stats.canMove = true;
        closedHand = false;
    }


    public IEnumerator PunchRoutine()
    {
        stats.canMove = false;
        Vector2 punchStartPosition = transform.position;
        GetComponent<ObjectAnimation>().PlayAnimation("Close", true);
        closedHand = true;
        Transform spriteTransform = transform.GetChild(0);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // === PHASE DE PRÉPARATION ===
        float prepDuration = 1f;
        float elapsed = 0f;
        float targetAngle = 90f;
        Quaternion startRot = spriteTransform.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 0, targetAngle);

        while (elapsed < prepDuration)
        {
            float t = elapsed / prepDuration;
            float smoothT = t * t * (3f - 2f * t);
            spriteTransform.localRotation = Quaternion.Lerp(startRot, endRot, smoothT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteTransform.localRotation = endRot;

        // === PHASE 1: ATTAQUE ===
        // Réinitialiser les flags
        isPunching = true;
        hasCollided = false;

        // S'assurer que le Rigidbody est bien configuré
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        float speed = 10f;
        Vector2 moveDir = Vector2.down;
        float maxDistance = 10f; // Distance maximale au lieu de durée
        Vector2 startPos = rb.position;

        while (!hasCollided)
        {
            // Vérifier la distance parcourue
            float distanceTraveled = Vector2.Distance(startPos, rb.position);
            if (distanceTraveled >= maxDistance)
                break;

            // Bouger le Rigidbody
            Vector2 movement = moveDir * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);

            yield return new WaitForFixedUpdate();

            // Vérification supplémentaire après le WaitForFixedUpdate
            // pour laisser le temps à la physique de détecter les collisions
            if (hasCollided)
                break;
        }

        // Arrêter immédiatement tout mouvement
        isPunching = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        CameraManager.instance.ShakeCamera(3, 3, 1);
        GetComponent<SoundContainer>().PlaySound("Impact", 2);
        yield return new WaitForSeconds(0.5f);

        // === PHASE 2: RETOUR ===
        elapsed = 0f;
        float returnDuration = 1f;
        Vector3 returnStartPos = transform.position;
        Quaternion returnStartRot = spriteTransform.localRotation;

        while (elapsed < returnDuration)
        {
            float t = elapsed / returnDuration;
            float smoothT = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(returnStartPos, punchStartPosition, smoothT);
            spriteTransform.localRotation = Quaternion.Lerp(returnStartRot, Quaternion.identity, smoothT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = punchStartPosition;
        spriteTransform.localRotation = Quaternion.identity;
        GetComponent<ObjectAnimation>().PlayAnimation("Open", true);
        stats.canMove = true;
        closedHand = false;
    }
}