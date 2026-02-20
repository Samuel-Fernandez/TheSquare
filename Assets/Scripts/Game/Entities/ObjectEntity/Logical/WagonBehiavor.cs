using System.Collections;
using UnityEngine;

public class WagonBehavior : MonoBehaviour
{
    private bool playerNear = false;
    private bool timerRunning = false;
    private bool isMoving = false;
    private bool playerOnWagon = false;
    private bool hasStarted = false;
    private bool wagonStopped = false;

    // Compteur de rails END rencontrés
    private int endRailCount = 0;

    // Direction actuelle du wagon (normalisée)
    private Vector2 currentDirection = Vector2.zero;

    // Référence au joueur monté sur le wagon
    private GameObject mountedPlayer = null;

    // Dernier rail traité pour éviter les doubles détections
    private TrackBehavior lastProcessedTrack = null;

    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!playerOnWagon || !hasStarted || isMoving || wagonStopped) return;

        // Avancer le wagon
        Vector2 targetPos = (Vector2)transform.position + currentDirection;
        float moveTime = 0.25f;
        StartCoroutine(MoveWagon(targetPos, moveTime));
    }

    private IEnumerator MoveWagon(Vector2 targetPos, float duration)
    {
        if (isMoving) yield break;
        isMoving = true;

        Vector2 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector2.Lerp(startPos, targetPos, t);

            if (mountedPlayer != null)
            {
                mountedPlayer.transform.position = (Vector2)transform.position + Vector2.up * 0.5f;
            }

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        transform.position = targetPos;
        if (mountedPlayer != null)
            mountedPlayer.transform.position = (Vector2)targetPos + Vector2.up * 0.5f;

        CheckTrack();

        GetComponent<SoundContainer>().PlaySound("Move", 2);

        isMoving = false;
    }

    private void CheckTrack()
    {
        if (wagonStopped) return;

        float checkRadius = 0.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, checkRadius);

        TrackBehavior closestTrack = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            TrackBehavior track = hit.GetComponent<TrackBehavior>();
            if (track == null || track == lastProcessedTrack) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTrack = track;
            }
        }

        if (closestTrack != null)
        {
            switch (closestTrack.type)
            {
                case TrackType.END:
                    wagonStopped = true;
                    isMoving = false;
                    hasStarted = false;
                    lastProcessedTrack = closestTrack;

                    StopAllCoroutines();
                    StartCoroutine(DismountPlayer());
                    return;
                case TrackType.DIRECTIONAL:
                    lastProcessedTrack = closestTrack;
                    StartCoroutine(HandleAxisChange(closestTrack));
                    break;

                case TrackType.LINEAR:
                    break;
            }
        }

    }

    private IEnumerator HandleAxisChange(TrackBehavior track)
    {
        Vector2 newDirection = track.GetWagonMovement(currentDirection).normalized;
        if (newDirection == Vector2.zero)
        {
            Debug.LogWarning("[HandleAxisChange] newDirection == Vector2.zero -> annulation");
            yield break;
        }

        Debug.Log($"[HandleAxisChange] Direction actuelle: {currentDirection}, Nouvelle direction: {newDirection}");

        // Durée totale identique à avant (0.175 + 0.175 = 0.35)
        float duration = 0.35f;
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + newDirection; // déplacement d'1 unité dans la nouvelle direction

        // Mettre à jour la direction immédiatement pour cohérence (dismount, jump, etc.)
        currentDirection = newDirection;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector2.Lerp(startPos, endPos, t);

            if (mountedPlayer != null)
                mountedPlayer.transform.position = (Vector2)transform.position + Vector2.up * 0.5f;

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        transform.position = endPos;
        if (mountedPlayer != null)
            mountedPlayer.transform.position = (Vector2)endPos + Vector2.up * 0.5f;

        StartCoroutine(ResetLastProcessedTrack(track));
    }




    private IEnumerator ResetLastProcessedTrack(TrackBehavior track)
    {
        yield return new WaitForSeconds(0.5f);

        float distance = Vector2.Distance(transform.position, track.transform.position);
        if (distance > 1f)
        {
            lastProcessedTrack = null;
            Debug.Log("[ResetLastProcessedTrack] Rail réinitialisé");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerOnWagon)
        {
            if(collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Monster)
            {
                collision.gameObject.GetComponent<LifeManager>().KnockBack(collision.gameObject, 20, gameObject);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (playerOnWagon) return;

        if (collision.gameObject.GetComponent<Stats>()?.entityType == EntityType.Player)
        {
            playerNear = true;
            if (!timerRunning)
                StartCoroutine(AdvanceTimer());
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Stats>()?.entityType == EntityType.Player)
        {
            playerNear = false;
        }
    }

    private IEnumerator AdvanceTimer()
    {
        timerRunning = true;
        float timer = 0f;

        while (timer < 1f)
        {
            if (!playerNear || !IsPlayerMovingToward())
            {
                timerRunning = false;
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        yield return MountPlayerRoutine();
        timerRunning = false;
    }

    private bool IsPlayerMovingToward()
    {
        var moveInput = PlayerManager.instance.playerInputActions.Gameplay.Move.ReadValue<Vector2>();
        Vector3 directionToWagon = (transform.position - PlayerManager.instance.player.transform.position).normalized;
        Vector3 playerMove = new Vector3(moveInput.x, moveInput.y, 0).normalized;
        return Vector3.Dot(playerMove, directionToWagon) > 0.5f;
    }

    private IEnumerator MountPlayerRoutine()
    {
        GameObject player = PlayerManager.instance.player;
        Collider2D wagonCollider = GetComponent<Collider2D>();

        player.GetComponent<Stats>().canMove = false;
        wagonCollider.isTrigger = true;
        player.GetComponent<SoundContainer>().PlaySound("jump", 1);

        Vector3 startPos = player.transform.position;
        Vector3 endPos = (Vector2)transform.position + Vector2.up * 0.5f;

        float duration = .5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float height = Mathf.Sin(Mathf.PI * t) * 1f;
            player.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.transform.position = endPos;
        mountedPlayer = player;
        playerOnWagon = true;

        float checkRadius = 0.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, checkRadius);

        foreach (var hit in hits)
        {
            TrackBehavior track = hit.GetComponent<TrackBehavior>();
            if (track != null && track.type == TrackType.END)
            {
                currentDirection = track.initialDirection.normalized;
                hasStarted = true;
                lastProcessedTrack = track;
                Debug.Log($"[MountPlayerRoutine] Démarrage - Direction: {currentDirection}");
                break;
            }
        }

        yield return null;
    }

    private IEnumerator DismountPlayer()
    {
        if (mountedPlayer == null) yield break;

        Debug.Log("[DismountPlayer] Début de la descente");

        GameObject player = mountedPlayer;
        Collider2D wagonCollider = GetComponent<Collider2D>();

        player.GetComponent<SoundContainer>().PlaySound("jump", 1);

        Vector2 jumpDirection = -currentDirection;
        if (jumpDirection == Vector2.zero) jumpDirection = Vector2.down;

        Vector3 startPos = player.transform.position;
        Vector3 endPos = startPos + (Vector3)(jumpDirection * .75f);

        float duration = .5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float height = Mathf.Sin(Mathf.PI * t) * 1f;
            player.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.transform.position = endPos;
        player.GetComponent<Stats>().canMove = true;
        wagonCollider.isTrigger = false;

        playerOnWagon = false;
        mountedPlayer = null;
        hasStarted = false;
        wagonStopped = false;
        endRailCount = 0;
        lastProcessedTrack = null;

        Debug.Log("[DismountPlayer] Descente terminée - Reset complet");
    }
}