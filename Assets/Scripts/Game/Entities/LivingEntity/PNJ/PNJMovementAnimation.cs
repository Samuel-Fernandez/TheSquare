using UnityEngine;

public class PNJMovementAnimation : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private ObjectAnimation objectAnimation;

    private Vector2 lastVelocity;

    // Variable pour garder en mémoire la dernière direction de mouvement
    private Vector2 lastDirection;

    public bool reversedSprite = false;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        objectAnimation = GetComponent<ObjectAnimation>();

        lastVelocity = Vector2.zero;
        lastDirection = Vector2.zero;
    }

    private void Update()
    {
        Vector2 velocity = rb2d.velocity;

        if (velocity.magnitude > 0.1f)
        {
            Vector2 currentDirection = velocity.normalized;

            if (Mathf.Abs(currentDirection.x) > Mathf.Abs(currentDirection.y))
            {
                // Flip horizontal avec inversion si reversedSprite = true
                bool flip = currentDirection.x > 0;
                if (reversedSprite) flip = !flip;
                FlipSprite(flip);
            }
            else
            {
                FlipSprite(false); // Vertical : pas de flip
            }

            lastDirection = currentDirection;
        }
        else if (lastDirection.magnitude > 0.1f)
        {
            if (Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
            {
                bool flip = lastDirection.x < 0;
                if (reversedSprite) flip = !flip;
                FlipSprite(flip);
            }
            else
            {
                FlipSprite(false);
            }
        }
        else
        {
            FlipSprite(false);
        }

        lastVelocity = velocity;
    }


    // Méthode pour retourner le sprite horizontalement
    private void FlipSprite(bool flip)
    {
        Vector3 scale = transform.localScale;
        scale.x = flip ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x); // Inverser seulement la valeur de l'échelle x
        transform.localScale = scale;
    }
}
