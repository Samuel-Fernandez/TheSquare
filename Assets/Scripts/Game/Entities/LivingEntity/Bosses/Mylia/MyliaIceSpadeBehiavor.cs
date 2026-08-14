using System.Collections;
using UnityEngine;

// Pic de glace lance par Mylia (attaque "Pics de glace renvoyables"). La pointe du sprite est
// orientee vers le bas par defaut : toute la logique d'orientation (Throw/StopRotate) applique
// donc une correction de +90 degres a l'angle Atan2 (au lieu du -90 habituel pour un sprite
// pointant vers le haut, voir ProjectileBehiavor/SpearBehiavor).
public class MyliaIceSpadeBehiavor : MonoBehaviour
{
    public string spawnSoundName = "Spawn";
    public string hitSoundName = "Hit";
    public string deviationSoundName = "Deviation";

    // Duree max de vol vers le joueur (post-Throw, hors deviation) avant autodestruction. Ne
    // s'applique ni pendant l'orbite en attente (aucun Throw n'est appele) ni apres une deviation.
    public float noHitLifetime = 5f;

    // Mylia (la lanceuse) : cible visee lorsque le joueur devie le pic avec un coup d'epee.
    public Transform owner;

    int damage;
    float speed;

    Vector2 moveDirection = Vector2.zero;
    bool isMoving = false;

    float rotationSpeed = 0f;
    bool isRotating = false;

    Coroutine lifetimeCoroutine;

    SoundContainer soundContainer;

    bool isDeviated = false;
    bool hasDealtDamage = false;

    void Start()
    {
        soundContainer = GetComponent<SoundContainer>();
        soundContainer.PlaySound(spawnSoundName, 1);
    }

    public void Init(int damage, float speed)
    {
        this.damage = damage;
        this.speed = speed;
    }

    // Lance le pic en ligne droite vers targetPosition, a la vitesse fixee par Init. Le pic continue
    // sa course au-dela de cette position tant que rien ne l'arrete (pas d'arret automatique dessus).
    public void Throw(Vector3 targetPosition)
    {
        Vector2 direction = targetPosition - transform.position;
        moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : moveDirection;
        isMoving = true;

        // Si le pic est en train de tournoyer (Rotate), on laisse la rotation gerer l'orientation
        // visuelle : pas de recalage brutal de l'angle pendant qu'il tourne sur lui-meme.
        if (!isRotating)
        {
            OrientTowards(moveDirection);
        }

        // Le timer d'autodestruction ne court que pour un vol non devie vers le joueur.
        if (!isDeviated)
        {
            if (lifetimeCoroutine != null)
            {
                StopCoroutine(lifetimeCoroutine);
            }
            lifetimeCoroutine = StartCoroutine(LifetimeTimeoutRoutine());
        }
    }

    IEnumerator LifetimeTimeoutRoutine()
    {
        yield return new WaitForSeconds(noHitLifetime);
        DestroySpade();
    }

    // Fait tournoyer le pic sur son axe Z a la vitesse donnee (degres/seconde), independamment
    // de son eventuel deplacement en cours.
    public void Rotate(float rotationSpeedPerSecond)
    {
        rotationSpeed = rotationSpeedPerSecond;
        isRotating = true;
    }

    // Arrete net le tournoiement (aucune latence : pas de fade a attendre), oriente immediatement
    // la pointe du pic vers target et le relance en ligne droite dans cette direction.
    public void StopRotate(Transform target)
    {
        isRotating = false;
        rotationSpeed = 0f;

        if (target == null) return;

        // isRotating est deja a false : Throw va reorienter la pointe vers la nouvelle direction.
        Throw(target.position);
    }

    // Appelee par le hitbox d'epee du joueur (voir PlayerController.AttackCoroutine) lorsque le
    // joueur devie le pic au bon moment : il fonce desormais vers Mylia et lui inflige des degats.
    // Tant que le pic orbite encore autour de Mylia (isMoving reste false : aucun Throw n'a encore
    // ete appele), sa position est geree en externe par MyliaPhase1Behiavor.OrbitIceSpadesRoutine ;
    // le laisser devier a ce stade ferait s'affronter les deux systemes de deplacement. On ignore
    // donc toute deviation avant que la lance ait ete effectivement lancee.
    public void Deviate()
    {
        if (isDeviated || !isMoving) return;

        isDeviated = true;

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        soundContainer.PlaySound(deviationSoundName, 1);

        StopRotate(owner);
    }

    void OrientTowards(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;

        float angleDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
    }

    void Update()
    {
        if (isRotating)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        if (isMoving)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasDealtDamage) return;

        Stats targetStats = collision.GetComponent<Stats>();
        if (targetStats == null) return;

        // Tant qu'il n'a pas ete devie, le pic ne blesse que le joueur. Une fois devie, il fonce
        // vers Mylia et ne blesse plus qu'elle (le Boss vise par owner).
        EntityType requiredTarget = isDeviated ? EntityType.Boss : EntityType.Player;
        if (targetStats.entityType != requiredTarget) return;

        LifeManager lifeManager = collision.GetComponent<LifeManager>();
        if (lifeManager == null) return;

        int lifeBeforeHit = lifeManager.life;
        lifeManager.TakeDamage(damage, gameObject, false);
        hasDealtDamage = true;

        // Meme tremblement d'ecran qu'un coup d'epee, uniquement si le coup a reellement porte
        // (Mylia peut ne plus etre vulnerable si la lance revient apres la fin de la fenetre).
        if (isDeviated && lifeManager.life < lifeBeforeHit)
        {
            MyliaPhase1Behiavor bossBehavior = collision.GetComponent<MyliaPhase1Behiavor>();
            if (bossBehavior != null)
            {
                bossBehavior.OnDamageTaken();
            }
        }

        DestroySpade();
    }

    void DestroySpade()
    {
        soundContainer.PlaySound(hitSoundName, 1);
        Destroy(gameObject);
    }
}
