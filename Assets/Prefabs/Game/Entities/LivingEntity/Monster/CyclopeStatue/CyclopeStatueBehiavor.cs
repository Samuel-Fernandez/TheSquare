using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum CyclopeDirection { UP, DOWN, RIGHT, LEFT }

public class CyclopeStatueBehiavor : MonoBehaviour
{
    public CyclopeDirection direction;

    public BoxCollider2D UpCollider;
    public BoxCollider2D DownCollider;
    public BoxCollider2D LeftCollider;
    public BoxCollider2D RightCollider;

    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite; // utilisé aussi pour droite, mais inversé
    public float radius = 5f;

    private GameObject player;
    private SpriteRenderer sprite;
    private bool isJumping = false;

    private float jumpInterval = .5f;
    private float jumpDuration = 0.5f;

    Stats stats;

    void Start()
    {
        player = PlayerManager.instance.player;
        sprite = GetComponentInChildren<SpriteRenderer>();
        StartCoroutine(BehaviorLoop());
        stats = GetComponent<Stats>();
    }

    IEnumerator BehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(jumpInterval);

            if (!stats.canMove)
            {
                yield return new WaitUntil(() => stats.canMove);
            }

            if (!PlayerInRadius()) continue;

            CyclopeDirection newDir = GetNextDirection();

            bool axisChange = IsAxisChange(direction, newDir);
            bool opposite = IsOpposite(direction, newDir);
            Vector3 nextPos = GetNextPosition(direction, newDir);

            // Changer de direction avec saut sur place
            if (axisChange)
            {
                yield return JumpInPlace();
                direction = newDir;
                UpdateSpriteAndCollider();
            }
            else if (opposite)
            {
                yield return JumpInPlace();
                yield return JumpInPlace();
                direction = newDir;
                UpdateSpriteAndCollider();
            }

            // Déplacer uniquement si la case suivante est libre
            if (!IsBlocked(nextPos))
            {
                direction = newDir; // Mettre à jour la direction
                UpdateSpriteAndCollider();
                yield return MoveOneUnitInDirection();
            }
            else
            {
                // Saut sur place si direction a changé mais case bloquée
                if (axisChange || opposite)
                {
                    // Déjà fait plus haut, donc pas besoin de sauter de nouveau
                }
                else
                {
                    // Case bloquée mais pas changement de direction rester sur place
                    yield return JumpInPlace();
                }
            }
        }
    }


    CyclopeDirection GetNextDirection()
    {
        Vector2 diff = player.transform.position - transform.position;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            return diff.x > 0 ? CyclopeDirection.RIGHT : CyclopeDirection.LEFT;
        else
            return diff.y > 0 ? CyclopeDirection.UP : CyclopeDirection.DOWN;
    }

    Vector3 GetNextPosition(CyclopeDirection oldDir, CyclopeDirection newDir)
    {
        Vector3 pos = transform.position;

        // Si saut sur place, reste à la position actuelle
        if (IsAxisChange(oldDir, newDir) || IsOpposite(oldDir, newDir))
            return pos;

        switch (newDir)
        {
            case CyclopeDirection.UP: pos += Vector3.up; break;
            case CyclopeDirection.DOWN: pos += Vector3.down; break;
            case CyclopeDirection.LEFT: pos += Vector3.left; break;
            case CyclopeDirection.RIGHT: pos += Vector3.right; break;
        }

        return pos;
    }

    bool IsBlocked(Vector3 pos)
    {
        // Vérifie les collisions avec des colliders dans la zone
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, 0.1f);
        foreach (var hit in hits)
        {
            if (hit == null) continue;

            // Si c'est un collider non trigger et que ce n'est pas un Stats bloqué
            if (!hit.isTrigger && hit.GetComponent<Stats>() == null)
                return true;

            // Si c'est un trou ("Hole" + chiffre) bloqué
            if (hit.tag.StartsWith("Hole"))
                return true;
        }

        // Aucun blocage détecté
        return false;
    }


    bool PlayerInRadius()
    {
        return Vector2.Distance(player.transform.position, transform.position) <= radius;
    }

    bool IsAxisChange(CyclopeDirection oldDir, CyclopeDirection newDir)
    {
        return (IsHorizontal(oldDir) && IsVertical(newDir)) || (IsVertical(oldDir) && IsHorizontal(newDir));
    }

    bool IsOpposite(CyclopeDirection oldDir, CyclopeDirection newDir)
    {
        return (oldDir == CyclopeDirection.UP && newDir == CyclopeDirection.DOWN)
            || (oldDir == CyclopeDirection.DOWN && newDir == CyclopeDirection.UP)
            || (oldDir == CyclopeDirection.LEFT && newDir == CyclopeDirection.RIGHT)
            || (oldDir == CyclopeDirection.RIGHT && newDir == CyclopeDirection.LEFT);
    }

    bool IsHorizontal(CyclopeDirection dir)
    {
        return dir == CyclopeDirection.LEFT || dir == CyclopeDirection.RIGHT;
    }

    bool IsVertical(CyclopeDirection dir)
    {
        return dir == CyclopeDirection.UP || dir == CyclopeDirection.DOWN;
    }

    IEnumerator JumpInPlace()
    {
        yield return JumpToPosition(transform.position);
    }

    IEnumerator MoveOneUnitInDirection()
    {
        Vector3 target = transform.position;
        switch (direction)
        {
            case CyclopeDirection.UP: target += Vector3.up; break;
            case CyclopeDirection.DOWN: target += Vector3.down; break;
            case CyclopeDirection.LEFT: target += Vector3.left; break;
            case CyclopeDirection.RIGHT: target += Vector3.right; break;
        }

        yield return JumpToPosition(target);
    }

    IEnumerator JumpToPosition(Vector3 target)
    {
        if (isJumping) yield break;
        isJumping = true;
        GetComponent<Collider2D>().enabled = false;

        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            float height = Mathf.Sin(t * Mathf.PI) * 1f; // arc de saut
            sprite.transform.localPosition = new Vector3(0, height, 0);
            transform.position = Vector3.Lerp(start, target, t);

            yield return null;
        }

        sprite.transform.localPosition = Vector3.zero;
        transform.position = target;
        isJumping = false;
        GetComponent<Collider2D>().enabled = true;
        CameraManager.instance.ShakeCamera(4, 4, .5f);
        GetComponent<SoundContainer>().PlaySound("Jump", 1);
    }

    void UpdateSpriteAndCollider()
    {
        UpCollider.enabled = false;
        DownCollider.enabled = false;
        LeftCollider.enabled = false;
        RightCollider.enabled = false;

        switch (direction)
        {
            case CyclopeDirection.UP: sprite.sprite = upSprite; UpCollider.enabled = true; sprite.flipX = false; break;
            case CyclopeDirection.DOWN: sprite.sprite = downSprite; DownCollider.enabled = true; sprite.flipX = false; break;
            case CyclopeDirection.LEFT: sprite.sprite = leftSprite; LeftCollider.enabled = true; sprite.flipX = false; break;
            case CyclopeDirection.RIGHT: sprite.sprite = leftSprite; RightCollider.enabled = true; sprite.flipX = true; break;
        }
    }
}
