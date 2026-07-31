using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TheSquare.Mechanics.UniverseHeart;

public enum SquareKillerState
{
    Sleep,
    Awaking,
    Active
}

[RequireComponent(typeof(Stats))]
public class SquareKillerBehiavor : MonoBehaviour
{
    [Header("Settings")]
    public Sprite sleepSprite;
    public float nodeSize = 0.8f;
    public int maxPathSearchDepth = 5000;

    [Header("Distance Thresholds")]
    public float distanceClose = 5f;
    public float distanceVeryClose = 2f;

    [Header("Normal Step")]
    public float normalWaitTime = 0.25f;
    public float normalMoveDuration = 0.25f;
    public float normalAnimSpeed = 2f;

    [Header("Close Step")]
    public float closeWaitTime = 0.125f;
    public float closeMoveDuration = 0.125f;
    public float closeAnimSpeed = 4f;

    [Header("Very Close Step")]
    public float veryCloseWaitTime = 0.0625f;
    public float veryCloseMoveDuration = 0.0625f;
    public float veryCloseAnimSpeed = 8f;

    [Header("Camera Shake")]
    public float shakeAmplitude = 2f;
    public float shakeFrequency = 3f;
    public float shakeDuration = 0.2f;

    private SquareKillerState currentState = SquareKillerState.Sleep;
    private Stats stats;
    private ObjectAnimation objAnim;
    private SpriteRenderer spriteRenderer;
    private SoundContainer soundContainer;

    private bool walkAlternate = false;
    private Vector2 startPos;
    private bool hasKilledPlayer = false;

    private void Awake()
    {
        stats = GetComponent<Stats>();
        objAnim = GetComponent<ObjectAnimation>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        soundContainer = GetComponent<SoundContainer>();
    }

    private void Start()
    {
        startPos = transform.position;
        if (InsideTheSquareManager.instance != null)
        {
            InsideTheSquareManager.instance.squareKillers.Add(this);
        }

        if (spriteRenderer != null && sleepSprite != null)
        {
            spriteRenderer.sprite = sleepSprite;
        }

        if (objAnim != null)
        {
            objAnim.StopAllAnimations();
        }
    }

    private void Update()
    {
        if (currentState == SquareKillerState.Sleep)
        {
            if (InsideTheSquareManager.player_is_revealed)
            {
                StartCoroutine(AwakeRoutine());
            }
        }
    }

    public void ResetToSleep()
    {
        StopAllCoroutines();
        currentState = SquareKillerState.Sleep;
        hasKilledPlayer = false;
        transform.position = startPos;

        if (spriteRenderer != null && sleepSprite != null)
        {
            spriteRenderer.sprite = sleepSprite;
        }
        if (objAnim != null)
        {
            objAnim.StopAllAnimations();
        }
    }

    private IEnumerator AwakeRoutine()
    {
        currentState = SquareKillerState.Awaking;

        if (objAnim != null)
        {
            objAnim.PlayAnimation("Awake", lastImageStay: true);
        }

        if (soundContainer != null)
        {
            soundContainer.PlaySound("Scream", 1); // 1 is pitch
        }

        yield return new WaitForSeconds(1f);

        currentState = SquareKillerState.Active;
        StartCoroutine(MovementRoutine());
        StartCoroutine(ScreamRoutine());
    }

    private IEnumerator MovementRoutine()
    {
        while (currentState == SquareKillerState.Active)
        {
            if (PlayerManager.instance == null || PlayerManager.instance.player == null)
            {
                yield return null;
                continue;
            }

            Transform playerTransform = PlayerManager.instance.player.transform;
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            float waitTime;
            float moveDuration;
            float animSpeed;

            if (distanceToPlayer < distanceVeryClose)
            {
                waitTime = veryCloseWaitTime;
                moveDuration = veryCloseMoveDuration;
                animSpeed = veryCloseAnimSpeed;
            }
            else if (distanceToPlayer < distanceClose)
            {
                waitTime = closeWaitTime;
                moveDuration = closeMoveDuration;
                animSpeed = closeAnimSpeed;
            }
            else
            {
                waitTime = normalWaitTime;
                moveDuration = normalMoveDuration;
                animSpeed = normalAnimSpeed;
            }

            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }

            // Calculate next step
            Vector2 nextPos = GetNextStepPosition(playerTransform.position);

            // Flip Sprite based on direction
            Vector2 dir = (nextPos - (Vector2)transform.position).normalized;
            if (Mathf.Abs(dir.x) > 0.01f && spriteRenderer != null)
            {
                spriteRenderer.flipX = dir.x > 0;
            }

            // Play Walk Animation
            string animName = walkAlternate ? "Walk2" : "Walk1";
            walkAlternate = !walkAlternate;

            if (objAnim != null)
            {
                objAnim.PlayAnimation(animName, lastImageStay: true, playInReverse: false, animationSpeed: animSpeed);
            }

            // Move smoothly over moveDuration
            Vector2 startPos = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                transform.position = Vector2.Lerp(startPos, nextPos, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = nextPos;

            // Step effects
            if (soundContainer != null)
            {
                soundContainer.PlaySound("Rumbling", 1);
            }
            if (CameraManager.instance != null)
            {
                CameraManager.instance.ShakeCamera(shakeAmplitude, shakeFrequency, shakeDuration);
            }
        }
    }

    private IEnumerator ScreamRoutine()
    {
        while (currentState == SquareKillerState.Active)
        {
            float wait = Random.Range(2f, 5f);
            yield return new WaitForSeconds(wait);

            if (soundContainer != null)
            {
                soundContainer.PlaySound("Scream", 1);
            }
        }
    }

    private Vector2 GetNextStepPosition(Vector2 playerPos)
    {
        Vector2 currentPos = transform.position;
        float gridStep = nodeSize > 0.1f ? nodeSize : 0.5f;

        List<Vector2> path = FindPath(currentPos, playerPos, gridStep);
        float maxDistance = stats != null ? Mathf.Max(stats.speed, 0.1f) : 1f;

        if (path != null && path.Count > 1)
        {
            Vector2 targetPos = path[1];

            // Lissage du chemin (String-pulling)
            for (int i = 1; i < path.Count; i++)
            {
                float directDist = Vector2.Distance(currentPos, path[i]);

                if (directDist <= maxDistance)
                {
                    if (IsPathClear(currentPos, path[i]))
                    {
                        targetPos = path[i];
                    }
                    else
                    {
                        // Si bloqué, on reste sur le dernier point valide
                        break;
                    }
                }
                else
                {
                    // Si le noeud dépasse la distance max, on interpole
                    Vector2 dir = (path[i] - path[i - 1]).normalized;
                    float distRemaining = maxDistance - Vector2.Distance(currentPos, targetPos);
                    if (distRemaining > 0)
                    {
                        Vector2 partialPos = targetPos + dir * distRemaining;
                        if (IsPathClear(currentPos, partialPos))
                        {
                            targetPos = partialPos;
                        }
                    }
                    break;
                }
            }
            return targetPos;
        }

        // Fallback: ligne droite si pas de chemin, mais seulement s'il n'y a pas de mur
        Vector2 directDir = (playerPos - currentPos).normalized;
        Vector2 fallbackPos = currentPos + directDir * maxDistance;
        if (IsPathClear(currentPos, fallbackPos))
        {
            return fallbackPos;
        }
        return currentPos; // Ne bouge pas si un mur bloque
    }

    private class Node
    {
        public Vector2Int Grid;
        public Vector2 Position;
        public Node Parent;
        public float G;
        public float H;
        public float F => G + H;
    }

    private List<Vector2> FindPath(Vector2 start, Vector2 target, float stepSize)
    {
        Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();
        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        Vector2Int startGrid = Vector2Int.zero;
        Vector2 GridToPos(Vector2Int grid) => start + new Vector2(grid.x, grid.y) * stepSize;
        Vector2Int PosToGrid(Vector2 pos) => new Vector2Int(Mathf.RoundToInt((pos.x - start.x) / stepSize), Mathf.RoundToInt((pos.y - start.y) / stepSize));

        Node startNode = new Node { Grid = startGrid, Position = start, G = 0, H = Vector2.Distance(start, target) };
        openList.Add(startNode);
        allNodes[startGrid] = startNode;

        Vector2Int targetGrid = PosToGrid(target);
        int iterations = 0;
        Node bestNode = startNode;

        Vector2Int[] dirs = {
            new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        while (openList.Count > 0 && iterations < maxPathSearchDepth)
        {
            iterations++;

            // Get node with lowest F score
            Node current = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].F < current.F || (openList[i].F == current.F && openList[i].H < current.H))
                {
                    current = openList[i];
                }
            }

            openList.Remove(current);
            closedList.Add(current.Grid);

            // Check if we reached the target or got close enough
            if (current.Grid == targetGrid || Vector2.Distance(current.Position, target) <= stepSize)
            {
                bestNode = current;
                break;
            }

            if (current.H < bestNode.H)
            {
                bestNode = current;
            }

            // Explore neighbors
            foreach (var dir in dirs)
            {
                Vector2Int neighborGrid = current.Grid + dir;

                if (closedList.Contains(neighborGrid)) continue;

                Vector2 neighborPos = GridToPos(neighborGrid);

                if (!IsWalkable(neighborPos)) continue;
                if (!IsPathClear(current.Position, neighborPos)) continue;

                float moveCost = (dir.x != 0 && dir.y != 0) ? stepSize * 1.414f : stepSize;
                float newG = current.G + moveCost;

                if (!allNodes.TryGetValue(neighborGrid, out Node neighbor))
                {
                    neighbor = new Node { Grid = neighborGrid, Position = neighborPos };
                    allNodes[neighborGrid] = neighbor;
                }

                if (newG < neighbor.G || !openList.Contains(neighbor))
                {
                    neighbor.G = newG;
                    neighbor.H = Vector2.Distance(neighborPos, target);
                    neighbor.Parent = current;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        // Reconstruct path
        List<Vector2> path = new List<Vector2>();
        Node curr = bestNode;
        while (curr != null)
        {
            path.Add(curr.Position);
            curr = curr.Parent;
        }
        path.Reverse();
        return path;
    }

    private bool IsWalkable(Vector2 pos)
    {
        Collider2D[] cols = Physics2D.OverlapBoxAll(pos, new Vector2(nodeSize * 0.4f, nodeSize * 0.4f), 0f);
        foreach (var col in cols)
        {
            if (!col.isTrigger && col.transform != this.transform)
            {
                Stats s = col.GetComponent<Stats>();
                if (s != null && s.entityType == EntityType.Player)
                {
                    continue; // Player is not an obstacle
                }
                return false;
            }
        }
        return true;
    }

    private bool IsPathClear(Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0.01f) return true;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(from, new Vector2(nodeSize * 0.2f, nodeSize * 0.2f), 0f, dir.normalized, dist);
        foreach (var hit in hits)
        {
            if (hit.collider != null && !hit.collider.isTrigger && hit.transform != this.transform)
            {
                if (hit.fraction == 0) continue; // Ignore colliders already touched at start point

                Stats s = hit.collider.GetComponent<Stats>();
                if (s != null && s.entityType == EntityType.Player) continue;
                return false;
            }
        }
        return true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckHitPlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckHitPlayer(collision);
    }

    private void CheckHitPlayer(Collider2D col)
    {
        if (currentState != SquareKillerState.Active) return;

        Stats stats = col.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            // Si hasKilledPlayer est vrai OU que le joueur ne peut déjà plus bouger (déjà mort par un autre), on ignore.
            if (hasKilledPlayer || !stats.canMove) return;
            hasKilledPlayer = true;

            IEnumerator routine = KillPlayerRoutine(stats);

            // Empêcher de bouger le tueur
            currentState = SquareKillerState.Sleep;
            StopAllCoroutines();

            if (ScenesManager.instance != null)
            {
                ScenesManager.instance.StartCoroutine(routine);
            }
        }
    }

    private IEnumerator KillPlayerRoutine(Stats playerStats)
    {
        playerStats.canMove = false;
        
        PlayerAnimation pAnim = playerStats.GetComponent<PlayerAnimation>();
        if (pAnim != null) pAnim.off = true;

        // Animer et jouer le son
        ObjectAnimation playerAnim = playerStats.GetComponent<ObjectAnimation>();
        if (playerAnim != null) playerAnim.PlayAnimation("Die", lastImageStay: true);

        SoundContainer playerSound = playerStats.GetComponent<SoundContainer>();
        if (playerSound != null) playerSound.PlaySound("Death", 1);

        yield return new WaitForSeconds(1f);

        if (ScenesManager.instance != null)
        {
            Vector2 respawnPos = Vector2.zero;
            if (InsideTheSquareManager.instance != null)
            {
                respawnPos = InsideTheSquareManager.instance.transform.position;
            }
            ScenesManager.instance.ChangeSceneObject(SceneManager.GetActiveScene().name, respawnPos);
        }
    }
}
