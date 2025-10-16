using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpecialAttackController : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    private List<PlayerInputAction> currentInputSequence = new List<PlayerInputAction>();
    private float lastInputTime;
    private Coroutine resetSequenceCoroutine;

    public GameObject tornadoEffectPrefab;
    public GameObject slashEffectPrefab;

    private void Awake()
    {
        lastInputTime = Time.time;

    }

    private void Start()
    {
        playerInputActions = PlayerManager.instance.playerInputActions;
    }

    public IEnumerator BigSlashRoutine()
    {
        // Récupérer les références nécessaires
        PlayerController playerController = GetComponent<PlayerController>();
        Vector2 attackDirection = playerController.GetAttackDirection();
        Vector2 playerPosition = transform.position;

        // Calculer la position de l'attaque en fonction de la direction et doubler la distance
        Vector2 attackPosition = playerPosition + attackDirection * (playerController.distanceToHitbox * 1.5f);

        // Calculer l'angle de rotation en fonction de la direction d'attaque
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

        // Désactiver le script PlayerAnimation
        GetComponent<PlayerAnimation>().off = true;

        // Jouer l'animation d'attaque appropriée en fonction de la direction
        ObjectAnimation animationController = GetComponent<ObjectAnimation>();
        if (Mathf.Abs(attackDirection.x) > Mathf.Abs(attackDirection.y))
        {
            // Attaque horizontale
            animationController.PlayAnimation("AttackSide");
        }
        else
        {
            // Attaque verticale
            if (attackDirection.y > 0)
            {
                // Vers le haut
                animationController.PlayAnimation("AttackUp");
            }
            else
            {
                // Vers le bas
                animationController.PlayAnimation("AttackDown");
            }
        }

        // Instancier le gameObject avec une taille doublée
        GameObject slashInstance = Instantiate(slashEffectPrefab, attackPosition, Quaternion.Euler(0f, 0f, angle - 90));

        // Doubler la taille du slash
        slashInstance.transform.localScale = slashInstance.transform.localScale * 2f;

        // Variables pour les dégâts
        float attackDuration = 0.25f; // Un peu plus long que l'attaque normale
        float elapsedTime = 0f;

        // Son d'attaque puissante
        SoundContainer soundContainer = GetComponent<SoundContainer>();
        if (soundContainer != null)
        {
            soundContainer.PlaySound("Attack", 2); // Volume plus élevé pour une attaque puissante
            soundContainer.PlaySound("BigSlashImpact", 2);
        }

        CameraManager.instance.ShakeCamera(4, 8, .5f);
        // Tant que la durée de l'attaque n'est pas écoulée
        while (elapsedTime < attackDuration)
        {
            // Mettre à jour la position de l'attaque en fonction du mouvement du joueur
            attackPosition = (Vector2)playerPosition + attackDirection * (playerController.distanceToHitbox * 1.5f);
            //slashInstance.transform.position = attackPosition;

            // Vérifier les collisions avec les entités avec un rayon doublé
            Collider2D[] hitEntities = Physics2D.OverlapCircleAll(attackPosition, playerController.hitboxSize * 2f);

            foreach (Collider2D hitEntity in hitEntities)
            {
                if (!hitEntity.isTrigger)
                {
                    Stats entityStats = hitEntity.GetComponent<Stats>();
                    if (entityStats != null && entityStats.entityType == EntityType.Monster)
                    {
                        if (entityStats.isVulnerable)
                        {
                            if (soundContainer != null)
                                soundContainer.PlaySound("SwordHit", 2);

                            // Faire des dégâts accrus avec l'attaque puissante
                            LifeManager lifeManager = GetComponent<LifeManager>();
                            if (lifeManager != null)
                            {
                                // Appel spécial pour doubler les dégâts
                                lifeManager.Attack(hitEntity.gameObject, 2f); // Supposant une méthode surchargée pour multiplier les dégâts
                            }
                        }
                    }
                    else
                    {
                        DestroyableBehiavor destroyableBehavior = hitEntity.GetComponent<DestroyableBehiavor>();
                        if (destroyableBehavior != null)
                        {
                            destroyableBehavior.DestroyObject(0);
                        }

                        HourglassBehiavor hourglassBehiavor = hitEntity.GetComponent<HourglassBehiavor>();
                        if (hourglassBehiavor != null)
                            hourglassBehiavor.Activation();

                        SwitchBehiavor switchBehiavor = hitEntity.GetComponent<SwitchBehiavor>();
                        if (switchBehiavor != null)
                        {
                            switchBehiavor.Switch(false);
                        }
                    }
                }
                else
                {
                    DestroyableBehiavor destroyableBehavior = hitEntity.GetComponent<DestroyableBehiavor>();
                    if (destroyableBehavior != null)
                    {
                        destroyableBehavior.DestroyObject(0);
                    }
                }
            }

            // Mettre à jour le temps écoulé
            elapsedTime += Time.deltaTime;

            // Attendre la prochaine frame
            yield return null;
        }

        // Détruire l'instance du slash
        Destroy(slashInstance);

        // Réactiver le script PlayerAnimation
        GetComponent<PlayerAnimation>().off = false;

        // Restaurer la vitesse du joueur
        playerController.actualSpeed = GetComponent<Stats>().speed;
        playerController.isHoldingAttack = false;

        yield return null;
    }

    public IEnumerator LightningRoutine()
    {
        var controller = GetComponent<PlayerController>();
        var stats = GetComponent<Stats>();
        var soundContainer = GetComponent<SoundContainer>();
        var lifeManager = GetComponent<LifeManager>();

        // Désactiver le contrôle du joueur
        controller.isAttacking = true;
        controller.actualSpeed = 0;
        GetComponent<PlayerAnimation>().off = true;

        // Jouer les sons de l'attaque lightning
        soundContainer.PlaySound("Attack", 1);

        // Attendre 0.5s avant le dash de retour
        yield return new WaitForSecondsRealtime(0.25f);
        GetComponent<PlayerAnimation>().off = true;
        GetComponent<ObjectAnimation>().PlayAnimation("LightningAttack");

        // Position initiale d'attaque
        Vector2 startPosition = controller.lastAttackPosition;
        Vector2 currentPosition = transform.position;
        Vector2 direction = (startPosition - currentPosition).normalized;
        float dashDistance = Vector2.Distance(startPosition, currentPosition);
        float dashDuration = 0.15f;
        float dashSpeed = dashDistance / dashDuration;

        soundContainer.PlaySound("LightningCharge", 2);

        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector2.Lerp(currentPosition, startPosition, elapsedTime / dashDuration);

            Collider2D[] hitEntities = Physics2D.OverlapCircleAll(transform.position, 0.5f);

            foreach (Collider2D hitEntity in hitEntities)
            {
                if (!hitEntity.isTrigger)
                {
                    Stats entityStats = hitEntity.GetComponent<Stats>();
                    if (entityStats != null && entityStats.entityType == EntityType.Monster)
                    {
                        if (entityStats.isVulnerable)
                            soundContainer.PlaySound("SwordHit", 2);

                        lifeManager.Attack(hitEntity.gameObject, 2f, 3); // dégâts plus puissants
                    }
                    else
                    {
                        hitEntity.GetComponent<DestroyableBehiavor>()?.DestroyObject(0);
                        hitEntity.GetComponent<HourglassBehiavor>()?.Activation();
                        hitEntity.GetComponent<SwitchBehiavor>()?.Switch(false);
                    }
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fin du dash
        transform.position = startPosition;

        // Rétablir les états
        GetComponent<PlayerAnimation>().off = false;
        controller.isAttacking = false;
        controller.actualSpeed = stats.speed;
    }



    public IEnumerator TornadoRoutine()
    {
        Instantiate(tornadoEffectPrefab, transform.position, Quaternion.identity);

        var controller = GetComponent<PlayerController>();
        var stats = GetComponent<Stats>();
        var soundContainer = GetComponent<SoundContainer>();

        controller.isAttacking = true;
        controller.actualSpeed = 0;

        soundContainer.PlaySound("Attack", 1);
        soundContainer.PlaySound("TornadoSword", 2);
        soundContainer.PlaySound("TornadoWind", 2);

        GetComponent<PlayerAnimation>().off = true;
        GetComponent<ObjectAnimation>().PlayAnimation("TornadoAttack");

        float elapsedTime = 0f;
        float duration = 0.25f; // Durée totale de l'attaque tornade
        float radius = 1f;

        while (elapsedTime < duration)
        {
            Collider2D[] hitEntities = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (Collider2D hitEntity in hitEntities)
            {
                if (!hitEntity.isTrigger)
                {
                    Stats entityStats = hitEntity.GetComponent<Stats>();
                    if (entityStats != null && entityStats.entityType == EntityType.Monster)
                    {
                        if (entityStats.isVulnerable)
                            soundContainer.PlaySound("SwordHit", 2);

                        GetComponent<LifeManager>().Attack(hitEntity.gameObject, 1.5f, 2);
                    }
                    else
                    {
                        hitEntity.GetComponent<DestroyableBehiavor>()?.DestroyObject(0);
                        hitEntity.GetComponent<HourglassBehiavor>()?.Activation();
                        hitEntity.GetComponent<SwitchBehiavor>()?.Switch(false);
                    }
                }
                else
                {
                    hitEntity.GetComponent<DestroyableBehiavor>()?.DestroyObject(0);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        GetComponent<PlayerAnimation>().off = false;
        controller.isAttacking = false;
        controller.actualSpeed = stats.speed;
    }


    private Vector2 lastMoveDirection = Vector2.zero;
    private float directionThreshold = 0.5f; // Seuil pour considérer un mouvement significatif

    private void Update()
    {

        if (playerInputActions.Gameplay.Dodge.triggered && GetComponent<PlayerController>().canDodge)
        {
            RegisterInput(PlayerInputAction.Dodge);
        }

        // Pour le mouvement, on vérifie en continu
        Vector2 currentMoveInput = playerInputActions.Gameplay.Move.ReadValue<Vector2>();

        // Si le mouvement dépasse le seuil et qu'il s'agit d'une nouvelle direction
        if (currentMoveInput.magnitude > directionThreshold &&
            (lastMoveDirection.magnitude <= directionThreshold ||
             !IsSameDirection(currentMoveInput, lastMoveDirection)))
        {
            // Déterminer la direction dominante
            if (Mathf.Abs(currentMoveInput.x) > Mathf.Abs(currentMoveInput.y))
            {
                // Le mouvement horizontal est dominant
                if (currentMoveInput.x > 0)
                    RegisterInput(PlayerInputAction.Right);
                else
                    RegisterInput(PlayerInputAction.Left);
            }
            else
            {
                // Le mouvement vertical est dominant
                if (currentMoveInput.y > 0)
                    RegisterInput(PlayerInputAction.Up);
                else
                    RegisterInput(PlayerInputAction.Down);
            }
        }

        // Mémoriser la direction actuelle
        lastMoveDirection = currentMoveInput;
    }

    // Vérifie si deux vecteurs de mouvement indiquent la même direction dominante
    private bool IsSameDirection(Vector2 dir1, Vector2 dir2)
    {
        // Horizontal dominant dans les deux cas
        if (Mathf.Abs(dir1.x) > Mathf.Abs(dir1.y) && Mathf.Abs(dir2.x) > Mathf.Abs(dir2.y))
        {
            return (dir1.x > 0 && dir2.x > 0) || (dir1.x < 0 && dir2.x < 0);
        }
        // Vertical dominant dans les deux cas
        else if (Mathf.Abs(dir1.x) <= Mathf.Abs(dir1.y) && Mathf.Abs(dir2.x) <= Mathf.Abs(dir2.y))
        {
            return (dir1.y > 0 && dir2.y > 0) || (dir1.y < 0 && dir2.y < 0);
        }
        // Directions dominantes différentes
        return false;
    }

    public SpecialAttack RegisterInput(PlayerInputAction inputAction)
    {
        currentInputSequence.Add(inputAction);
        lastInputTime = Time.time;

        if (resetSequenceCoroutine != null)
            StopCoroutine(resetSequenceCoroutine);

        SpecialAttack attack = CheckForSpecialAttackAndGet();

        if (attack == null)
            resetSequenceCoroutine = StartCoroutine(ResetSequenceAfterDelay());

        return attack;
    }


    // Méthode à ajouter à SpecialAttackController
    public SpecialAttack CheckForSpecialAttackAndGet()
    {
        List<SpecialAttack> availableAttacks = PlayerManager.instance.runtimeSpecialAttacks;

        foreach (var attack in availableAttacks)
        {
            if (!attack.isAvailable)
                continue;

            foreach (var combo in attack.combos)
            {
                if (IsSequenceMatch(currentInputSequence, combo.inputSequence))
                {
                    ResetInputSequence();
                    return attack; // Renvoyer l'attaque spéciale trouvée
                }
            }
        }

        return null; // Aucune attaque spéciale trouvée
    }

    private bool IsSequenceMatch(List<PlayerInputAction> currentSequence, List<PlayerInputAction> comboSequence)
    {
        // Si la séquence actuelle est plus courte que le combo, ce n'est pas un match
        if (currentSequence.Count < comboSequence.Count)
            return false;

        // Vérifier les derniers inputs pour voir s'ils correspondent au combo
        for (int i = 0; i < comboSequence.Count; i++)
        {
            int currentIndex = currentSequence.Count - comboSequence.Count + i;
            if (currentSequence[currentIndex] != comboSequence[i])
                return false;
        }

        return true;
    }

    private IEnumerator ResetSequenceAfterDelay()
    {
        // Trouver le temps imparti le plus long parmi toutes les attaques spéciales
        float maxDuration = 0f;
        foreach (var attack in PlayerManager.instance.runtimeSpecialAttacks)
        {
            if (attack.duration > maxDuration)
                maxDuration = attack.duration;
        }

        // Attendre ce délai
        yield return new WaitForSeconds(maxDuration);

        // Réinitialiser la séquence si aucun nouveau input n'a été enregistré depuis
        if (Time.time - lastInputTime >= maxDuration)
        {
            ResetInputSequence();
        }
    }

    public void ResetInputSequence()
    {
        currentInputSequence.Clear();

        if (resetSequenceCoroutine != null)
        {
            StopCoroutine(resetSequenceCoroutine);
            resetSequenceCoroutine = null;
        }
    }
}