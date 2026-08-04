using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public int strength;
    public int speed;
    public int direction; // 0 = Up, 1 = Left, 2 = Right, 3 = Down
    float accurateDirection;
    public bool ally;
    public bool ignoreAll;
    public float knockbackPower;
    bool targeted; // Si true, emp�che de nouvelles interactions
    public bool destroyOnCollision = false; // Si se d�truit directement apr�s collision
    public string fireParticleName = "Flames";
    public GameObject fireEffect;
    private bool isOnFire = false;
    GameObject launcher;

    ObjectAnimation anim;
    ObjectParticles particles;
    EntityEffects entityEffects;

    private Vector2 movementDirection;

    private Vector3 localPositionOffset; // Pour stocker la position locale par rapport � l'objet parent

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<ObjectAnimation>();
        particles = GetComponent<ObjectParticles>();
        entityEffects = GetComponent<EntityEffects>();
        //InitMovementAndRotation();
    }

    public void InitProjectile(int strength, int speed, int direction, bool ally, float knockBackPower, GameObject launcher)
    {
        this.strength = strength;
        this.speed = speed;
        this.direction = direction;
        this.ally = ally;
        this.knockbackPower = knockBackPower;
        this.launcher = launcher;

        InitMovementAndRotation();
    }

    public void InitProjectile(int strength, int speed, float accurateDirection, bool ally, float knockBackPower, GameObject launcher, bool shouldRotate = true)
    {
        this.strength = strength;
        this.speed = speed;
        this.accurateDirection = accurateDirection;
        this.ally = ally;
        this.knockbackPower = knockBackPower;
        this.launcher = launcher;

        // Calculer la direction du mouvement en fonction de l'angle en radians
        movementDirection = new Vector2(Mathf.Cos(accurateDirection), Mathf.Sin(accurateDirection));

        // Appliquer la rotation correcte en fonction de l'angle
        if (shouldRotate)
            GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0, 0, accurateDirection * Mathf.Rad2Deg);
    }




    private void InitMovementAndRotation()
    {
        switch (direction)
        {
            case 0: // Up
                movementDirection = Vector2.up;
                GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case 1: // Left
                movementDirection = Vector2.left;
                GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case 2: // Right
                movementDirection = Vector2.right;
                GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case 3: // Down
                movementDirection = Vector2.down;
                GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // D�placer le projectile en fonction de sa direction et de sa vitesse
        if (MeteoManager.instance.time)
            transform.Translate(movementDirection * speed * Time.deltaTime, Space.World);
    }

    private void FixedUpdate()
    {
        if (transform.parent != null)
        {
            // Mettre � jour la position de la fl�che pour qu'elle suive le parent
            transform.position = transform.parent.TransformPoint(localPositionOffset);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Le projectile traverse le LittleBrasier sans s'y arr�ter, mais s'enflamme automatiquement
        // au passage : il transmettra ensuite le feu � sa v�ritable cible d'impact, si elle le permet.
        if (collision.GetComponent<LittleBrasierBehiavor>() != null)
        {
            if (!isOnFire)
            {
                isOnFire = true;

                if (entityEffects != null)
                {
                    // Le fireEffect est piloté à chaque frame par EntityEffects.Update() (DetermineState) :
                    // le SetActive direct se fait immédiatement écraser. Il faut passer par son isFire.
                    entityEffects.fireForce = Mathf.Max(strength, 1);
                    entityEffects.isFire = true;
                }
                else if (fireEffect != null)
                    fireEffect.SetActive(true);
                else if (particles != null)
                    particles.SpawnParticle(fireParticleName, gameObject, 0.15f);
            }

            return;
        }

        // Active les interrupteurs (SwitchBehiavor) en premier
        var switcher = collision.GetComponent<SwitchBehiavor>();
        if (switcher != null)
            switcher.Switch(true);

        if (ignoreAll)
        {
            var stats = collision.GetComponent<Stats>();
            if (stats == null || stats.entityType != EntityType.Player)
                return;
        }

        if (!targeted &&
            launcher != collision.gameObject &&
            !collision.GetComponent<ItemBehaviour>() &&
            !collision.GetComponent<ProjectileBehavior>() &&
            collision.gameObject.layer != LayerMask.NameToLayer("LowHeight") &&
            !collision.gameObject.tag.StartsWith("Hole"))
        {
            if (particles != null)
                particles.StopSpawningParticles();

            TryIgniteTarget(collision);

            var targetStats = collision.GetComponent<Stats>();
            var destroyableObjects = collision.GetComponent<DestroyableBehiavor>();
            var weakness = collision.GetComponent<MonsterWeakness>();

            if (weakness != null)
            {
                weakness.TakeDamage(strength, gameObject, true);
            }

            if (targetStats == null && destroyableObjects == null)
            {
                if (!destroyOnCollision)
                    AttachAndDestroy(collision.transform);
                else
                    Destroy(gameObject);

                targeted = true;
                return;
            }

            if (destroyableObjects)
            {
                if (!destroyableObjects.isLower)
                {
                    if (!destroyOnCollision)
                        AttachAndDestroy(collision.transform);
                    else
                        Destroy(gameObject);
                }

                return;
            }

            if (ally && (targetStats.entityType == EntityType.Monster || targetStats.entityType == EntityType.Boss))
            {
                if (collision.GetComponent<LifeManager>() != null)
                    collision.GetComponent<LifeManager>().TakeDamage(strength, gameObject, false);

                if (!destroyOnCollision)
                    AttachAndDestroy(collision.transform);
                else
                    Destroy(gameObject);

                targeted = true;
            }
            else if (!ally && targetStats.entityType == EntityType.Player)
            {
                collision.GetComponent<LifeManager>().TakeDamage(strength, gameObject, false);

                if (!destroyOnCollision)
                    AttachAndDestroy(collision.transform);
                else
                    Destroy(gameObject);

                targeted = true;
            }
        }
    }


    private void TryIgniteTarget(Collider2D collision)
    {
        if (!isOnFire) return;

        EntityEffects targetEffects = collision.GetComponent<EntityEffects>();
        if (targetEffects != null && targetEffects.canBeFire)
        {
            targetEffects.SetState(strength, true);
        }
    }

    public void AttachAndDestroy(Transform parent)
    {
        if (anim != null)
            anim.PlayAnimation("Shake");

        localPositionOffset = parent.InverseTransformPoint(transform.position);

        transform.SetParent(parent);

        transform.localPosition = localPositionOffset;

        movementDirection = Vector2.zero;

        Destroy(gameObject, 1f);
    }
}
