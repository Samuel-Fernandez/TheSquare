using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum ThrowerType
{
    NONE,
    PROJECTILE,
    EFFECT,
}

public enum Orientation
{
    UP,
    LEFT,
    RIGHT,
    DOWN,
}

public enum EffectType
{
    NONE,
    FIRE,
    ICE,
    POISON
}


public class Thrower : MonoBehaviour
{
    public ThrowerType type;
    public Orientation orientation;
    public EffectType effectType;
    public GameObject throwablePrefab;
    public float interval;
    public float duration;
    bool isThrowing;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RoutineActivation());
    }

    // Update is called once per frame
    void Update()
    {
        if (type == ThrowerType.EFFECT)
            ThrowEffect();
    }

    public void ThrowProjectile()
    {
        if (isThrowing)
        {
            GameObject projectile = Instantiate(throwablePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<ProjectileBehavior>().InitProjectile(Mathf.Max(PlayerManager.instance.player.GetComponent<Stats>().health / 6, 1), 6, (int)orientation, false, 5, gameObject);
        }
    }

    public GameObject colliderObject;
    public float colliderSize; // Utilise float pour des dimensions plus flexibles

    public void ThrowEffect()
    {
        if (isThrowing)
        {
            if(Random.Range(0, 100) >= 80)
                GetComponent<SoundContainer>().PlaySound("Throw", 2);

            if (colliderObject) return; // Si un collider existe déjà, ne rien faire

            // Crée un BoxCollider2D
            colliderObject = new GameObject("EffectCollider");
            BoxCollider2D boxCollider = colliderObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(0.5f, 0.5f); // Largeur fixe de 0.5 unités
            boxCollider.transform.parent = transform; // Enfant de l'objet du script

            // Positionne le collider en fonction de l'orientation
            Vector3 offset = Vector3.zero;
            Vector3 direction = Vector3.zero;

            switch (orientation)
            {
                case Orientation.UP:
                    offset = new Vector3(0, 0.5f + 0.25f, 0);
                    boxCollider.size = new Vector2(0.5f, colliderSize);
                    direction = Vector3.up;
                    break;
                case Orientation.DOWN:
                    offset = new Vector3(0, -(0.5f + 0.25f), 0);
                    boxCollider.size = new Vector2(0.5f, colliderSize);
                    direction = Vector3.down;
                    break;
                case Orientation.LEFT:
                    offset = new Vector3(-(0.5f + 0.25f), 0, 0);
                    boxCollider.size = new Vector2(colliderSize, 0.5f);
                    direction = Vector3.left;
                    break;
                case Orientation.RIGHT:
                    offset = new Vector3(0.5f + 0.25f, 0, 0);
                    boxCollider.size = new Vector2(colliderSize, 0.5f);
                    direction = Vector3.right;
                    break;
            }

            // Applique l'offset en tenant compte de la rotation
            boxCollider.transform.localPosition = transform.InverseTransformPoint(transform.position + offset);

            GetComponent<ObjectParticles>().ChangeDuration(duration);

            // Génère des particules tous les 0.5 unités le long de la longueur du collider
            int steps = Mathf.CeilToInt(colliderSize / 0.5f); // Nombre d'étapes
            for (int i = 0; i < steps; i++)
            {
                Vector3 particlePosition = transform.position + offset + direction * (i * 0.5f);
                switch (orientation)
                {
                    case Orientation.UP:
                        GetComponent<ObjectParticles>().SpawnParticle("Effect", particlePosition, Quaternion.Euler(0, 0, 0));
                        break;
                    case Orientation.LEFT:
                        GetComponent<ObjectParticles>().SpawnParticle("Effect", particlePosition, Quaternion.Euler(0, 0, 90));
                        break;
                    case Orientation.RIGHT:
                        GetComponent<ObjectParticles>().SpawnParticle("Effect", particlePosition, Quaternion.Euler(0, 0, -90));
                        break;
                    case Orientation.DOWN:
                        GetComponent<ObjectParticles>().SpawnParticle("Effect", particlePosition, Quaternion.Euler(0, 0, 180));
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            if (colliderObject)
            {
                Destroy(colliderObject); // Détruit le collider
                colliderObject = null;
            }
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Vérifie si l'objet en collision possède le script Stats
        if (collision.GetComponent<Stats>() != null)
        {
            switch (effectType)
            {
                case EffectType.NONE:
                    break;
                case EffectType.FIRE:
                    collision.GetComponent<EntityEffects>().SetState(Mathf.Max(collision.gameObject.GetComponent<Stats>().health / 15, 1), true, false, false);
                    break;
                case EffectType.ICE:
                    collision.GetComponent<EntityEffects>().SetState(0, false, true, false);
                    break;
                case EffectType.POISON:
                    collision.GetComponent<EntityEffects>().SetState(0, false, false, true);
                    break;
                default:
                    break;
            }
            // Applique l'effet sur l'objet en collision
        }
    }

    IEnumerator RoutineActivation()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        while (true)
        {
            if (spriteRenderer != null)
            {
                Color startColor = Color.white;
                Color endColor = Color.red;
                float elapsedTime = 0f;

                spriteRenderer.color = startColor; // White


                while (elapsedTime < interval)
                {
                    spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / interval);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                spriteRenderer.color = startColor; // White
                GetComponent<SoundContainer>().PlaySound("Throw", 2);


                if (type == ThrowerType.PROJECTILE)
                {
                    isThrowing = true;
                    ThrowProjectile();
                    isThrowing = false;
                }

                if (type == ThrowerType.EFFECT)
                {
                    isThrowing = true;
                    yield return new WaitForSeconds(duration);
                    isThrowing = false;
                }
            }
        }
    }

}
