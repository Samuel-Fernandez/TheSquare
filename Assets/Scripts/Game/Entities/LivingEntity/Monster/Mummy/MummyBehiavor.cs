using System.Collections;
using UnityEngine;

public class MummyBehavior : MonoBehaviour
{
    private NewMonsterMovement movement;
    private Stats stats;
    private ObjectAnimation anim;

    public MummyColliderTriggerBehavior colliderUp;
    public MummyColliderTriggerBehavior colliderRight;
    public MummyColliderTriggerBehavior colliderDown;
    public MummyColliderTriggerBehavior colliderLeft;

    void Start()
    {
        movement = GetComponent<NewMonsterMovement>();
        stats = GetComponent<Stats>();
        anim = GetComponent<ObjectAnimation>();

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 4));

            if (movement.IsInDetectionZone)
            {
                movement.EnableAnimations = false;
                movement.SetSpeedMultiplier(0);

                // Détermination de la direction d’attaque
                string attackDir;
                MummyColliderTriggerBehavior activeCollider = null;

                if (Mathf.Abs(movement.Direction.y) > Mathf.Abs(movement.Direction.x))
                {
                    if (movement.Direction.y > 0)
                    {
                        attackDir = "Up";
                        activeCollider = colliderUp;
                    }
                    else
                    {
                        attackDir = "Down";
                        activeCollider = colliderDown;
                    }
                }
                else
                {
                    attackDir = "Side";
                    activeCollider = movement.Direction.x > 0 ? colliderRight : colliderLeft;
                }

                // Désactiver tous les colliders
                colliderUp.gameObject.SetActive(false);
                colliderRight.gameObject.SetActive(false);
                colliderDown.gameObject.SetActive(false);
                colliderLeft.gameObject.SetActive(false);

                // Début d’attaque
                movement.LockFlip(true);
                GetComponent<SoundContainer>().PlaySound("Attack", 2);
                yield return StartCoroutine(anim.PlayAnimationCoroutine("StartAttack" + attackDir, true));

                yield return new WaitForSeconds(.3f);

                // Jouer l'animation de fin d'attaque
                GetComponent<SoundContainer>().PlaySound("Attack", 2);
                yield return StartCoroutine(anim.PlayAnimationCoroutine("EndAttack" + attackDir, true));

                stats.doingAttack = true;

                // Activer le collider correspondant
                activeCollider.gameObject.SetActive(true);

                GameObject grabbedPlayer = null;
                float attackDuration = 0.5f; // Durée pendant laquelle le bandeau persiste
                float elapsed = 0f;

                while (elapsed < attackDuration)
                {
                    if (activeCollider.collisionActive && activeCollider.LastCollision != null)
                    {
                        grabbedPlayer = activeCollider.LastCollision;
                        break;
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (grabbedPlayer != null)
                    yield return StartCoroutine(GrabSequence(grabbedPlayer, attackDir));
                else
                    GrabAnimation(attackDir);

                // Reset état après l’attaque
                activeCollider.gameObject.SetActive(false);
                movement.SetSpeedMultiplier(1);
                movement.EnableAnimations = true;
                movement.LockFlip(false);
                stats.doingAttack = false;
            }
        }
    }


    private IEnumerator GrabSequence(GameObject player, string attackDir)
    {
        // On lance les deux coroutines en parallèle
        Coroutine pull = StartCoroutine(PullPlayer(player));
        Coroutine animReverse = StartCoroutine(GrabAnimation(attackDir));

        // On attend que les deux soient terminées
        yield return pull;
        yield return animReverse;
    }

    private IEnumerator PullPlayer(GameObject player)
    {
        Vector3 start = player.transform.position;

        // Direction vers la momie, mais s'arrêter à 0.5 unités
        Vector3 dir = (transform.position - start).normalized;
        Vector3 target = transform.position - dir * 0.5f;

        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            player.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        player.transform.position = target;
    }

    private IEnumerator GrabAnimation(string attackDir)
    {
        // Jouer StartAttack normalement
        StartCoroutine(anim.PlayAnimationCoroutine("StartAttack" + attackDir));
        // Puis EndAttack en reverse et en yield
        yield return StartCoroutine(anim.PlayAnimationCoroutine("EndAttack" + attackDir, true, true));
    }
}