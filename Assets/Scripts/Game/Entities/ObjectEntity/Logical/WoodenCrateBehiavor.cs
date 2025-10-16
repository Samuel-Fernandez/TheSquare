using System.Collections;
using UnityEngine;

public class WoodenCrateBehavior : MonoBehaviour
{
    [Header("Push Settings")]
    private float pushStep = 0.5f;
    private float pushDuration = 0.5f;
    private float pushDelay = 0.5f; // Temps d'attente avant de commencer à pousser

    [Header("Detection")]
    private float detectionDistance = 1f; // Distance pour détecter le joueur
    [SerializeField] private float obstacleDetectionRadius = 0.35f; // Rayon pour détecter les obstacles

    private bool isBeingPushed = false;
    private bool isMoving = false;
    private Vector2 lastPushDirection;
    private Coroutine pushDelayRoutine;
    private Coroutine moveRoutine;
    private Transform playerTransform;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Désactive la physique pour un contrôle manuel
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Récupère le joueur via PlayerManager
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        // **NOUVEAU : Corrige la position initiale**
        FixInitialPosition();

        // **NOUVEAU : Attend un frame pour que les colliders se stabilisent**
        StartCoroutine(DelayedStart());
    }

    // **NOUVELLE MÉTHODE : Initialisation retardée**
    private IEnumerator DelayedStart()
    {
        yield return new WaitForFixedUpdate();

        // Test de débogage pour la position initiale
        if (Application.isEditor)
        {
            Vector2[] testDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

            foreach (Vector2 dir in testDirections)
            {
                Vector2 testPos = (Vector2)transform.position + dir * pushStep;
                bool blocked = IsPositionBlocked(testPos);
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null || isMoving) return;

        CheckPlayerPush();
    }

    private void CheckPlayerPush()
    {
        Vector2 toPlayer = (Vector2)(playerTransform.position - transform.position);
        float distance = toPlayer.magnitude;

        // Le joueur est-il assez proche ?
        if (distance > detectionDistance || !PlayerManager.instance.player.GetComponent<Stats>().canMove)
        {
            StopPushing();
            return;
        }

        Vector2 pushDirection = GetPushDirection(toPlayer);

        if (pushDirection == Vector2.zero)
        {
            StopPushing();
            return;
        }

        // **CORRECTION : Vérification d'obstacle simplifiée**
        Vector2 targetPosition = (Vector2)transform.position + pushDirection * pushStep;
        if (IsPositionBlocked(targetPosition))
        {
            StopPushing();
            return;
        }

        // Vérifie si le joueur essaie de se déplacer dans la direction de la boîte
        Vector2 playerInput = GetPlayerInput();

        // Le joueur pousse-t-il dans la bonne direction ?
        if (Vector2.Dot(playerInput.normalized, pushDirection) < 0.8f)
        {
            StopPushing();
            return;
        }

        // Commence ou continue la poussée
        if (!isBeingPushed || pushDirection != lastPushDirection)
        {
            StartPushing(pushDirection);
        }
    }

    private Vector2 GetPushDirection(Vector2 toPlayer)
    {
        // Convertit la direction vers le joueur en direction cardinale
        Vector2 direction = Vector2.zero;

        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
        {
            direction = new Vector2(toPlayer.x > 0 ? -1 : 1, 0);
        }
        else
        {
            direction = new Vector2(0, toPlayer.y > 0 ? -1 : 1);
        }

        return direction;
    }

    private Vector2 GetPlayerInput()
    {
        // Récupère les inputs via PlayerManager
        if (PlayerManager.instance != null && PlayerManager.instance.playerInputActions != null)
        {
            return PlayerManager.instance.playerInputActions.Gameplay.Move.ReadValue<Vector2>();
        }

        return Vector2.zero;
    }

    private void StartPushing(Vector2 direction)
    {
        // Vérifie d'abord s'il y a un obstacle dans la direction de poussée
        Vector2 targetPosition = (Vector2)transform.position + direction * pushStep;
        if (IsPositionBlocked(targetPosition))
        {
            // Il y a un obstacle, on ne peut pas pousser
            return;
        }

        // Arrête les routines précédentes
        if (pushDelayRoutine != null)
            StopCoroutine(pushDelayRoutine);

        isBeingPushed = true;
        lastPushDirection = direction;

        // Lance le délai avant de commencer à pousser
        pushDelayRoutine = StartCoroutine(PushDelayCoroutine(direction));
    }

    private void StopPushing()
    {
        PlayerManager.instance.player.GetComponent<PlayerController>().isPushing = false;

        if (pushDelayRoutine != null)
        {
            StopCoroutine(pushDelayRoutine);
            pushDelayRoutine = null;
        }

        isBeingPushed = false;
    }

    private IEnumerator PushDelayCoroutine(Vector2 direction)
    {
        // Cycle continu : attendre 0.5s de poussée -> déplacer -> répéter
        while (isBeingPushed && lastPushDirection == direction)
        {
            // Phase 1: Attendre 0.5s en vérifiant que le joueur pousse toujours
            float elapsedPushTime = 0f;
            bool stillPushing = true;

            while (elapsedPushTime < pushDelay && stillPushing)
            {
                yield return new WaitForFixedUpdate();
                elapsedPushTime += Time.fixedDeltaTime;

                // Vérifie si le joueur pousse toujours dans la bonne direction
                stillPushing = IsPlayerStillPushing(direction);
            }

            // Si le joueur a arrêté de pousser pendant l'attente, on sort
            if (!stillPushing || !isBeingPushed || lastPushDirection != direction)
            {
                break;
            }

            PlayerManager.instance.player.GetComponent<PlayerController>().isPushing = true;

            // Phase 2: Effectuer le mouvement
            yield return StartCoroutine(MoveCrateAndPlayer(direction));

            // Petite pause pour la stabilité
            yield return new WaitForFixedUpdate();
        }
    }

    private bool IsPlayerStillPushing(Vector2 expectedDirection)
    {
        if (playerTransform == null) return false;

        Vector2 toPlayer = (Vector2)(playerTransform.position - transform.position);
        float distance = toPlayer.magnitude;

        // Le joueur est-il toujours proche ?
        if (distance > detectionDistance)
            return false;

        // Le joueur pousse-t-il toujours dans la bonne direction ?
        Vector2 currentPushDirection = GetPushDirection(toPlayer);
        if (currentPushDirection != expectedDirection)
            return false;

        // Le joueur a-t-il toujours l'input correspondant ?
        Vector2 playerInput = GetPlayerInput();
        if (Vector2.Dot(playerInput.normalized, expectedDirection) < 0.8f)
            return false;

        // Vérifie qu'il n'y a toujours pas d'obstacle
        Vector2 targetPosition = (Vector2)transform.position + expectedDirection * pushStep;
        if (IsPositionBlocked(targetPosition))
            return false;

        return true;
    }

    private IEnumerator MoveCrateAndPlayer(Vector2 direction)
    {
        if (isMoving) yield break;

        isMoving = true;
        GetComponent<SoundContainer>().PlaySound("MoveBox", 2);

        Vector2 crateStartPos = transform.position;
        Vector2 crateTargetPos = crateStartPos + direction * pushStep;

        Vector2 playerStartPos = playerTransform.position;
        Vector2 playerTargetPos = playerStartPos + direction * pushStep;

        // Vérifie s'il y a un obstacle pour la boîte
        if (IsPositionBlocked(crateTargetPos))
        {
            isMoving = false;
            PlayerManager.instance.player.GetComponent<PlayerController>().isPushing = false;
            StopPushing();
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < pushDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / pushDuration;

            // Mouvement linéaire lisse
            transform.position = Vector2.Lerp(crateStartPos, crateTargetPos, t);
            playerTransform.position = Vector2.Lerp(playerStartPos, playerTargetPos, t);

            yield return null;
        }

        // Assure la position finale exacte
        transform.position = crateTargetPos;
        playerTransform.position = playerTargetPos;

        isMoving = false;
        PlayerManager.instance.player.GetComponent<PlayerController>().isPushing = false;
    }

    // **MÉTHODE DE DÉBOGAGE AVANCÉE**
    private bool IsPositionBlocked(Vector2 position, bool debugMode = false)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, obstacleDetectionRadius);

        foreach (Collider2D hit in hits)
        {

            // Ignore la caisse elle-même, le joueur, et les triggers
            if (hit.gameObject == gameObject ||
                hit.gameObject == playerTransform?.gameObject ||
                hit.isTrigger)
            {
                if (debugMode) Debug.Log($"    -> IGNORÉ");
                continue;
            }

            if (debugMode) Debug.Log($"    -> BLOQUE LE MOUVEMENT!");
            return true;
        }

        if (debugMode) Debug.Log("=== Résultat: POSITION LIBRE ===");
        return false;
    }

    // **NOUVELLE MÉTHODE : Correction de position au démarrage**
    private void FixInitialPosition()
    {
        // Corrige la position pour qu'elle soit exactement sur la grille
        Vector2 currentPos = transform.position;
        Vector2 fixedPos = new Vector2(
            Mathf.Round(currentPos.x / pushStep) * pushStep,
            Mathf.Round(currentPos.y / pushStep) * pushStep
        );

        if (Vector2.Distance(currentPos, fixedPos) > 0.01f)
        {
            transform.position = fixedPos;
        }
    }

    private void OnDrawGizmos()
    {
        // Visualise la zone de détection du joueur
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        // Visualise la zone de détection d'obstacles
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, obstacleDetectionRadius);
    }
}