using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    Stats stats;
    public float actualSpeed;
    Rigidbody2D rb;
    SoundContainer soundContainer;
    UseSpecialObject specialObjects;

    bool fallIntoHole = false;
    Vector3 lastPosition;
    CircleCollider2D playerCollider;

    private SpecialAttackController specialAttackController;

    public bool isPushing;


    private void Start()
    {
        stats = GetComponent<Stats>();
        actualSpeed = stats.speed;
        rb = GetComponent<Rigidbody2D>();
        soundContainer = GetComponent<SoundContainer>();
        specialObjects = GetComponent<UseSpecialObject>();

        StartCoroutine(getLastPosition());
        playerCollider = GetComponent<CircleCollider2D>();

        playerInputActions = PlayerManager.instance.playerInputActions;

        // Obtenir la référence au SpecialAttackController
        specialAttackController = GetComponent<SpecialAttackController>();
    }

    void Update()
    {
        if (Time.deltaTime > 0 && stats.canMove)
        {
            Dodge();
            Attack();
            UseSpecialObject();
        }

        if (GetComponent<EntityEffects>().isSlimed && actualSpeed != stats.speed / 2)
            actualSpeed = actualSpeed / 2;
        else if (!GetComponent<EntityEffects>().isSlimed && actualSpeed == stats.speed / 2)
            actualSpeed = stats.speed;
    }

    private void FixedUpdate()
    {
        if (Time.deltaTime > 0 && stats.canMove)
            Move();

        GetLastDirection();

    }

    public bool canDodge = true;
    public bool isDodging = false;
    const float dodgeCooldown = 1.5f;

    public GameObject dodgeInterface;
    GameObject dodgeInterfaceInstance;

    void Dodge()
    {
        if (canDodge && playerInputActions.Gameplay.Dodge.triggered && !stats.isBowShooting && !isAttacking && stats.canMove && !specialObjects.isHammering && !specialObjects.isPickaxing && !specialObjects.isShielding && !isPushing && !GetComponent<EntityEffects>().isSlimed)
        {
            isDodging = true;
            if (GetComponent<EntityEffects>().isFire)
                GetComponent<EntityEffects>().StopFire();

            soundContainer.PlaySound("Dodge", 1);
            StartCoroutine(PerformDodge());
        }
    }

    // Coroutine pour le dodge
    IEnumerator PerformDodge()
    {
        float originalSpeed = actualSpeed;
        actualSpeed *= 4f;
        stats.isVulnerable = false;
        canDodge = false;
        isDodging = true;

        Collider2D[] playerColliders = GetComponents<Collider2D>();
        List<Collider2D> ignoredColliders = new List<Collider2D>();
        List<Stats> entitiesBeingDodged = new List<Stats>();
        List<GameObject> projectilesBeingDodged = new List<GameObject>(); // Tracker les projectiles séparément

        // Trouver tous les objets à désactiver temporairement
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            Collider2D otherCol = obj.GetComponent<Collider2D>();
            if (otherCol == null) continue;

            Stats entityStats = obj.GetComponent<Stats>();
            if (entityStats != null && (entityStats.entityType == EntityType.Monster || entityStats.entityType == EntityType.Boss))
            {
                foreach (var playerCol in playerColliders)
                    Physics2D.IgnoreCollision(playerCol, otherCol, true);
                ignoredColliders.Add(otherCol);
                entitiesBeingDodged.Add(entityStats);
            }

            if (obj.CompareTag("Projectile"))
            {
                foreach (var playerCol in playerColliders)
                    Physics2D.IgnoreCollision(playerCol, otherCol, true);
                ignoredColliders.Add(otherCol);
                projectilesBeingDodged.Add(obj); // Ajouter à la liste des projectiles

                // Vérification immédiate pour les projectiles déjà proches
                if (IsPlayerOverlappingEntityByDistance(obj))
                {
                    PlayerManager.instance.DodgeTime();
                }
            }
        }

        float dodgeDuration = 0.2f;
        float elapsedTime = 0f;
        HashSet<Stats> alreadyDodgedEntities = new HashSet<Stats>();
        HashSet<GameObject> alreadyDodgedProjectiles = new HashSet<GameObject>();

        while (elapsedTime < dodgeDuration)
        {
            // Vérifier les entités
            foreach (Stats entityStats in entitiesBeingDodged)
            {
                if (entityStats != null && entityStats.doingAttack && !alreadyDodgedEntities.Contains(entityStats))
                {
                    if (IsPlayerOverlappingEntityByDistance(entityStats.gameObject))
                    {
                        PlayerManager.instance.DodgeTime();
                        alreadyDodgedEntities.Add(entityStats);
                    }
                }
            }

            // Vérifier les projectiles (utiliser la distance car les collisions sont désactivées)
            foreach (GameObject projectile in projectilesBeingDodged)
            {
                if (projectile != null && !alreadyDodgedProjectiles.Contains(projectile))
                {
                    if (IsPlayerOverlappingEntityByDistance(projectile))
                    {
                        PlayerManager.instance.DodgeTime();
                        alreadyDodgedProjectiles.Add(projectile);
                    }
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fin de l'esquive
        actualSpeed = stats.speed;
        stats.isVulnerable = true;
        isDodging = false;

        // Réactiver les collisions
        foreach (Collider2D playerCol in playerColliders)
        {
            foreach (Collider2D ignoredCol in ignoredColliders)
            {
                if (ignoredCol != null)
                    Physics2D.IgnoreCollision(playerCol, ignoredCol, false);
            }
        }

        dodgeInterfaceInstance = Instantiate(dodgeInterface, transform.position + Vector3.up * 0.75f, Quaternion.identity, transform);
        yield return StartCoroutine(UpdateScrollbar());
        canDodge = true;
        Destroy(dodgeInterfaceInstance);
    }

    // Version améliorée de la détection par distance
    private bool IsPlayerOverlappingEntityByDistance(GameObject entity)
    {
        if (entity == null) return false;

        float overlapDistance = 0.5f;

        // Essayer d'obtenir la taille réelle du collider pour une détection plus précise
        Collider2D entityCollider = entity.GetComponent<Collider2D>();
        if (entityCollider != null)
        {
            // Utiliser les bounds du collider pour une détection plus précise
            float entityRadius = Mathf.Max(entityCollider.bounds.size.x, entityCollider.bounds.size.y) / 2f;
            overlapDistance = entityRadius + 0.3f; // Ajouter une petite marge
        }

        float distance = Vector2.Distance(transform.position, entity.transform.position);

        return distance <= overlapDistance;
    }

    // Alternative : Détection par bounds (plus précise que la distance)
    private bool IsPlayerOverlappingEntityByBounds(GameObject entity)
    {
        if (entity == null) return false;

        Collider2D entityCollider = entity.GetComponent<Collider2D>();
        Collider2D playerCollider = GetComponent<Collider2D>();

        if (entityCollider == null || playerCollider == null) return false;

        // Vérifier si les bounds se chevauchent
        bool overlapping = playerCollider.bounds.Intersects(entityCollider.bounds);


        return overlapping;
    }



    // Méthode avec raycast (pour détecter même à travers les collisions désactivées)
    private bool IsPlayerOverlappingEntityByRaycast(GameObject entity)
    {
        Vector2 directionToEntity = (entity.transform.position - transform.position).normalized;
        float maxDistance = 2f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToEntity, maxDistance, ~0, -Mathf.Infinity, Mathf.Infinity);

        if (hit.collider != null && hit.collider.gameObject == entity)
        {
            return true;
        }

        return false;
    }



    // Coroutine pour mettre à jour la scrollbar
    IEnumerator UpdateScrollbar()
    {
        float elapsedTime = 0f;
        Scrollbar scrollbar = dodgeInterfaceInstance.GetComponentInChildren<Scrollbar>();

        while (elapsedTime < dodgeCooldown)
        {
            elapsedTime += Time.deltaTime;
            float value = 1 - (elapsedTime / dodgeCooldown);
            scrollbar.size = value;

            yield return null;
        }

        scrollbar.size = 0;
    }

    public int currentDirection = 0; // 0 = Up, 1 = Left, 2 = Right, 3 = Down


    void UseSpecialObject()
    {
        if (specialObjects.isHammering || specialObjects.isPickaxing)
        {
            actualSpeed = 0;
        }

        if (stats.isBowShooting || specialObjects.isShielding)
        {
            actualSpeed = .3f;
        }

        if (GetComponent<UseSpecialObject>().lanternIsOn && SpecialObjectsManager.instance.actualObject.toolType != ToolType.LIGHT)
        {
            GetComponent<UseSpecialObject>().lanternIsOn = false;
            GetComponent<UseSpecialObject>().lanternLight.SetActive(false);
        }

        if (GetComponent<UseSpecialObject>().isShadowing && SpecialObjectsManager.instance.actualObject.toolType != ToolType.DARK_MEDAL)
        {
            specialObjects.UsingShadowMedal();
        }

        if (PlayerManager.instance.playerInputActions.Gameplay.SpecialItem.IsPressed() && !isAttacking && !isDodging && !stats.isDying && !isPushing)
        {
            switch (SpecialObjectsManager.instance.actualObject.toolType)
            {
                case ToolType.BOW:
                    if (!stats.isBowShooting && PlayerManager.instance.GetSpecialItem(SpecialItemType.ARROW).nb > 0 && SpecialObjectsManager.instance.actualObject.equiped)
                        specialObjects.ShootBow(currentDirection);
                    break;
                case ToolType.PICKAXE:
                    if (!specialObjects.isPickaxing && SpecialObjectsManager.instance.actualObject.equiped)
                        specialObjects.UsingPickaxe(currentDirection);
                    break;
                case ToolType.LIGHT:
                    if (!specialObjects.isLightning && SpecialObjectsManager.instance.actualObject.equiped)
                        specialObjects.UsingLantern(currentDirection);
                    break;
                case ToolType.HAMMER:
                    if (!specialObjects.isHammering && SpecialObjectsManager.instance.actualObject.equiped)
                        specialObjects.HammerAttack(currentDirection);
                    break;
                case ToolType.DARK_MEDAL:
                    if (SpecialObjectsManager.instance.actualObject.equiped)
                        specialObjects.UsingShadowMedal();
                    break;
                case ToolType.SHIELD:
                    if (SpecialObjectsManager.instance.actualObject.equiped && !specialObjects.isShielding)
                        specialObjects.UsingShield(currentDirection);
                    break;
                default:
                    break;
            }
        }
    }

    public float distanceToHitbox = .5f;
    public float hitboxSize = .5f;
    public GameObject swordSlash;
    public bool isAttacking = false;
    public bool isHoldingAttack = false;
    private Vector2 lastAttackDirection;
    public Vector2 lastAttackPosition;
    bool lastAttackTouchEnemy = false;
    float holdAttackTime = 0f;
    private bool holdAttackRegistered = false; // Nouvelle variable pour s'assurer que RegisterInput(HoldAttack) n'est exécuté qu'une fois
    private bool releaseAttackRegistered = false; // Nouvelle variable pour s'assurer que RegisterInput(ReleaseAttack) n'est exécuté qu'une fois

    public bool CanAttack
    {
        get
        {
            return !isDodging &&
                   !stats.isDying &&
                   !stats.isBowShooting &&
                   !specialObjects.isHammering &&
                   !specialObjects.isPickaxing &&
                   !specialObjects.isShielding &&
                   !isPushing &&
                   HasWeapon();
        }
    }

    private bool attackAnimationCompleted = false;

    public GameObject bigSlashChargePrefab;
    GameObject bigSlashChargeInstance;

    void Attack()
    {
        // Si le bouton d'attaque est enfoncé et que le joueur peut attaquer
        if (playerInputActions.Gameplay.Attack.triggered && CanAttack)
        {
            SpecialAttack specialAttack = specialAttackController.RegisterInput(PlayerInputAction.Attack);
            if (specialAttack != null)
            {
                ExecuteSpecialAttack(specialAttack.attackName);
                return;
            }
            // Sinon, exécuter l'attaque normale uniquement si le joueur n'est pas déjà en train d'attaquer
            else if (!isAttacking)
            {
                lastAttackPosition = transform.position;
                lastAttackTouchEnemy = false;
                isAttacking = true;
                isHoldingAttack = true;
                attackAnimationCompleted = false;
                holdAttackTime = 0f;
                holdAttackRegistered = false; // Réinitialiser le flag de HoldAttack
                releaseAttackRegistered = false; // Réinitialiser le flag de ReleaseAttack
                actualSpeed = 0;
                soundContainer.PlaySound("Attack", 1);
                soundContainer.PlaySound("SwordSlash", 2);

                if (PlayerManager.instance.isDogingTime)
                    soundContainer.PlaySound("BigSlashImpact", 2);

                StartCoroutine(AttackCoroutine());
            }
        }

        // Gérer l'attaque maintenue
        if (PlayerManager.instance.GetSpecialAttack("BigSlash").isAvailable && playerInputActions.Gameplay.Attack.IsPressed() && !isAttacking)
        {
            if (isHoldingAttack && attackAnimationCompleted)
            {
                holdAttackTime += Time.deltaTime;
                actualSpeed = 0; // Maintenir la vitesse à 0 pendant l'attaque maintenue

                // Instancier le prefab uniquement si on a maintenu au moins 1 seconde et que ce n'est pas encore fait
                if (holdAttackTime >= 1f && !holdAttackRegistered)
                {
                    SpecialAttack specialAttack = specialAttackController.RegisterInput(PlayerInputAction.HoldAttack);

                    bigSlashChargeInstance = Instantiate(bigSlashChargePrefab, transform.position, Quaternion.identity);
                    soundContainer.PlaySound("BigSlashCharge", 2);

                    if (specialAttack != null)
                    {
                        ExecuteSpecialAttack(specialAttack.attackName);
                    }
                    holdAttackRegistered = true;
                }

                // Jouer l'animation appropriée en fonction de la direction
                UpdateHoldAttackAnimation();
            }

        }
        // Dans la méthode Attack(), partie 'else' lorsque le bouton est relâché
        else
        {
            // Si le bouton est relâché, arrêter l'attaque maintenue
            if (isHoldingAttack)
            {
                // Si l'attaque a été maintenue pendant au moins 1 seconde et que ReleaseAttack n'a pas encore été enregistré
                if (holdAttackTime >= 1.0f && !releaseAttackRegistered && attackAnimationCompleted)
                {
                    SpecialAttack specialAttack = specialAttackController.RegisterInput(PlayerInputAction.ReleaseAttack);

                    Destroy(bigSlashChargeInstance);

                    if (specialAttack != null)
                    {
                        ExecuteSpecialAttack(specialAttack.attackName);
                        releaseAttackRegistered = true;
                        // Ne pas restaurer la vitesse ici, car l'attaque spéciale va s'en charger
                    }
                    else
                    {
                        // Si aucune attaque spéciale n'est déclenchée, restaurer la vitesse
                        actualSpeed = stats.speed;
                    }
                }
                else
                {
                    // Si l'attaque a été maintenue moins d'une seconde ou que l'animation initiale n'est pas terminée
                    if (attackAnimationCompleted)
                    {
                        actualSpeed = stats.speed;
                        Destroy(bigSlashChargeInstance);
                    }
                }

                isHoldingAttack = false;
                holdAttackRegistered = false;
                releaseAttackRegistered = false; // Réinitialiser ce drapeau ici
            }
        }

        if (Mathf.Abs(horizontalInput) > 0 || Mathf.Abs(verticalInput) > 0 && !isAttacking)
        {
            lastAttackDirection = new Vector2(horizontalInput, verticalInput).normalized;
        }
    }

    // Nouvelle méthode pour mettre à jour l'animation d'attaque maintenue
    void UpdateHoldAttackAnimation()
    {
        ObjectAnimation animator = GetComponent<ObjectAnimation>();

        // Déterminer quelle animation jouer en fonction de la direction
        if (Mathf.Abs(lastAttackDirection.x) > Mathf.Abs(lastAttackDirection.y))
        {
            // Attaque horizontale
            animator.PlayAnimation("HoldAttackSide");
        }
        else
        {
            // Attaque verticale
            if (lastAttackDirection.y > 0)
            {
                // Vers le haut
                animator.PlayAnimation("HoldAttackUp");
            }
            else
            {
                // Vers le bas
                animator.PlayAnimation("HoldAttackDown");
            }
        }
    }

    // Méthode pour exécuter une attaque spéciale quand un combo est complété
    private void ExecuteSpecialAttack(string attackName)
    {
        // Faire ici les différentes attaques spéciales 
        switch (attackName)
        {
            case "Tornado":
                StartCoroutine(specialAttackController.TornadoRoutine());
                break;
            case "Lightning":
                if (lastAttackTouchEnemy)
                    StartCoroutine(specialAttackController.LightningRoutine());
                break;
            case "BigSlash":
                StartCoroutine(specialAttackController.BigSlashRoutine());
                break;
            default:
                break;
        }
    }

    IEnumerator AttackCoroutine()
    {
        float elapsedTime = 0f;
        Vector2 attackDirection = GetAttackDirection();
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * distanceToHitbox;
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

        GameObject slashInstance = Instantiate(swordSlash, attackPosition, Quaternion.Euler(0f, 0f, angle - 90));

        if (SpecialEquipementManager.instance.CheckPower(SpecialPower.LASER))
            SpecialEquipementManager.instance.LaunchLaser();

        bool attackBlocked = false; // drapeau pour stopper l’attaque

        while (elapsedTime < 0.15f)
        {
            attackPosition = (Vector2)transform.position + attackDirection * distanceToHitbox;
            Collider2D[] hitEntities = Physics2D.OverlapCircleAll(attackPosition, hitboxSize);

            foreach (Collider2D hitEntity in hitEntities)
            {
                if (!hitEntity.isTrigger)
                {
                    Stats entityStats = hitEntity.GetComponent<Stats>();

                    // Si une entité bloque les attaques du joueur
                    if (entityStats != null && entityStats.blockPlayerAttack)
                    {
                        attackBlocked = true;
                        break; // on sort du foreach
                    }

                    if (entityStats != null && (entityStats.entityType == EntityType.Monster || entityStats.entityType == EntityType.Boss))
                    {
                        if (entityStats.isVulnerable)
                        {
                            soundContainer.PlaySound("SwordHit", 2);
                            lastAttackTouchEnemy = true;
                        }

                        GetComponent<LifeManager>().Attack(hitEntity.gameObject);
                    }
                    else
                    {
                        DestroyableBehiavor destroyableBehavior = hitEntity.GetComponent<DestroyableBehiavor>();
                        if (destroyableBehavior != null)
                            destroyableBehavior.DestroyObject(0);

                        HourglassBehiavor hourglassBehiavor = hitEntity.GetComponent<HourglassBehiavor>();
                        if (hourglassBehiavor != null)
                            hourglassBehiavor.Activation();

                        SwitchBehiavor switchBehiavor = hitEntity.GetComponent<SwitchBehiavor>();
                        if (switchBehiavor != null)
                            switchBehiavor.Switch(true);
                    }
                }
                else
                {
                    DestroyableBehiavor destroyableBehavior = hitEntity.GetComponent<DestroyableBehiavor>();
                    if (destroyableBehavior != null)
                        destroyableBehavior.DestroyObject(0);

                    DamageZoneBehiavor damageZoneBehiavor = hitEntity.GetComponent<DamageZoneBehiavor>();
                    if (damageZoneBehiavor != null)
                        damageZoneBehiavor.SwordCollide();
                }
            }

            // Si une entité a bloqué l’attaque on arrête tout immédiatement
            if (attackBlocked)
            {
                isAttacking = false;
                attackAnimationCompleted = true;
                actualSpeed = stats.speed;
                isHoldingAttack = false;

                // facultatif : effet visuel ou sonore de blocage
                soundContainer.PlaySound("ShieldImpact", 1);
                GetComponent<ObjectParticles>().SpawnParticle("FireMetal", slashInstance.transform.position);
                GetComponent<LifeManager>().KnockBack(gameObject, stats.knockbackResistance + 5, slashInstance);

                yield break; // stop net la coroutine
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fin normale de l’attaque
        isAttacking = false;
        attackAnimationCompleted = true;
        actualSpeed = stats.speed;
        isHoldingAttack = playerInputActions.Gameplay.Attack.IsPressed();
    }


    public Vector2 GetAttackDirection()
    {
        // Si le joueur est en train de maintenir une attaque, retourner la direction fixée au début de l'attaque
        if (isHoldingAttack)
        {
            return lastAttackDirection;
        }

        // Obtenir les entrées de mouvement
        Vector2 moveInput = playerInputActions.Gameplay.Move.ReadValue<Vector2>();
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        // Si le joueur ne bouge pas, retourner la dernière direction d'attaque
        if (horizontalInput == 0 && verticalInput == 0)
        {
            return lastAttackDirection;
        }

        // Calculer la direction d'attaque en fonction des entrées
        Vector2 attackDirection = Vector2.right * horizontalInput + Vector2.up * verticalInput;

        // Normaliser la direction d'attaque
        attackDirection.Normalize();

        // Mettre à jour la dernière direction d'attaque
        lastAttackDirection = attackDirection;

        return attackDirection;
    }

    public float horizontalInput;
    public float verticalInput;
    public bool isMoving = false;

    void Move()
    {
        Vector2 moveInput = playerInputActions.Gameplay.Move.ReadValue<Vector2>();

        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        isMoving = (horizontalInput != 0 || verticalInput != 0);

        // Calculer le vecteur de déplacement en fonction des entrées
        Vector2 movement = new Vector2(horizontalInput, verticalInput).normalized * actualSpeed;

        // Déplacer le personnage en ajustant directement sa position
        transform.position += (Vector3)movement * Time.fixedDeltaTime * 2;
    }

    public void UpdateSpeed(float newSpeed)
    {
        if (GetComponent<Stats>() && specialObjects && !isDodging && !isAttacking && !GetComponent<Stats>().isBowShooting && !specialObjects.isHammering && !specialObjects.isPickaxing && !specialObjects.isShielding)
        {
            actualSpeed = newSpeed;
        }
    }

    void GetLastDirection()
    {
        if (!stats.isBowShooting && stats.canMove && !specialObjects.isHammering && !specialObjects.isPickaxing && !specialObjects.isShielding)
        {
            Vector2 moveInput = playerInputActions.Gameplay.Move.ReadValue<Vector2>();

            // Ajouter une zone morte pour éviter les micro-mouvements
            float deadZone = 0.2f;

            // Si l'input est trop faible, on ignore
            if (moveInput.magnitude < deadZone)
                return;

            // Déterminer la direction principale en comparant les valeurs absolues
            float absX = Mathf.Abs(moveInput.x);
            float absY = Mathf.Abs(moveInput.y);

            // Si le mouvement vertical est plus fort que l'horizontal
            if (absY > absX)
            {
                if (moveInput.y > 0)
                {
                    currentDirection = 0; // Up
                }
                else
                {
                    currentDirection = 3; // Down
                }
            }
            // Si le mouvement horizontal est plus fort que le vertical
            else if (absX > absY)
            {
                if (moveInput.x > 0)
                {
                    currentDirection = 2; // Right
                }
                else
                {
                    currentDirection = 1; // Left
                }
            }
            // Si les deux sont égaux, on privilégie la direction avec la plus grande valeur absolue
            else
            {
                if (absX >= absY)
                {
                    currentDirection = moveInput.x > 0 ? 2 : 1; // Right ou Left
                }
                else
                {
                    currentDirection = moveInput.y > 0 ? 0 : 3; // Up ou Down
                }
            }
        }
    }

    // Ajout dans PlayerController
    public bool cantFall = false;
    public Transform safeTeleportation;

    // Nouveau système pour éviter les téléportations simultanées
    private bool isTeleporting = false;

    // NOUVEAU : Backup de sécurité mis à jour régulièrement
    private Vector3 safeBackupPosition = Vector3.zero;
    private float lastSafeBackupTime = 0f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Éviter de traiter si on est en train de téléporter
        if (isTeleporting) return;

        ObjectPerspective perspective = GetComponent<ObjectPerspective>();

        // Vérifie si le tag commence par "Hole"
        if (collision.gameObject.tag.StartsWith("Hole") && !fallIntoHole && !cantFall)
        {
            string tag = collision.gameObject.tag;
            if (int.TryParse(tag.Substring(4), out int holeLevel))
            {
                // Compare le niveau du tag avec celui de l'objet
                if (holeLevel == perspective.level)
                {
                    Tilemap tilemap = collision.GetComponent<Tilemap>();
                    Vector3 holeWorldPosition;

                    if (ShouldFallInHole(tilemap, out holeWorldPosition))
                    {
                        FallIntoHole(holeWorldPosition);
                    }
                }
            }
        }
    }

    // NOUVEAU : Méthode pour mettre à jour la position de backup
    private void UpdateSafeBackupPosition()
    {
        // Mettre à jour toutes les 0.3 secondes quand le joueur n'est pas en train de tomber
        if (!fallIntoHole && !isTeleporting && Time.time - lastSafeBackupTime > 0.3f)
        {
            // Si on a un safeTeleportation valide, l'utiliser comme backup
            if (safeTeleportation != null)
            {
                safeBackupPosition = safeTeleportation.position;
            }
            // Sinon, utiliser la position actuelle si elle est sûre (pas dans un trou)
            else if (!IsPositionInHole(transform.position))
            {
                safeBackupPosition = transform.position;
            }

            lastSafeBackupTime = Time.time;
        }
    }

    // NOUVEAU : Vérifier si une position est dans un trou
    private bool IsPositionInHole(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f);
        foreach (var col in colliders)
        {
            if (col.gameObject.tag.StartsWith("Hole"))
            {
                return true;
            }
        }
        return false;
    }

    // MODIFIÉ : Appeler UpdateSafeBackupPosition dans Update ou LateUpdate
    private void LateUpdate()
    {
        UpdateSafeBackupPosition();
        // ... reste de votre code LateUpdate ...
    }

    private bool ShouldFallInHole(Tilemap tilemap, out Vector3 holeWorldPosition)
    {
        float radius = playerCollider.radius;
        Vector2 playerCenter = (Vector2)transform.position + playerCollider.offset;
        holeWorldPosition = Vector3.zero;

        // Chercher le trou dans les tuiles adjacentes pour gérer tous les cas d'approche
        Vector3Int playerTilePosition = tilemap.WorldToCell(playerCenter);
        Vector3Int holeTilePosition = Vector3Int.zero;
        bool holeFound = false;

        // Vérifier la tuile actuelle et les tuiles adjacentes (priorité aux directions cardinales)
        Vector3Int[] tilesToCheck = {
        playerTilePosition,                    // Centre
        playerTilePosition + Vector3Int.down,  // Bas (priorité pour approche par le haut)
        playerTilePosition + Vector3Int.up,    // Haut
        playerTilePosition + Vector3Int.left,  // Gauche
        playerTilePosition + Vector3Int.right  // Droite
    };

        foreach (Vector3Int tilePos in tilesToCheck)
        {
            if (tilemap.HasTile(tilePos))
            {
                holeTilePosition = tilePos;
                holeFound = true;
                break;
            }
        }

        if (!holeFound)
            return false;

        Vector3 holeTileCenter = tilemap.GetCellCenterWorld(holeTilePosition);
        holeWorldPosition = holeTileCenter;

        // Calculer la distance entre le centre du joueur et le centre de la tuile du trou
        float distanceToHoleCenter = Vector2.Distance(playerCenter, holeTileCenter);

        // Obtenir la taille d'une tuile pour déterminer si le joueur est suffisamment centré
        Vector3 cellSize = tilemap.cellSize;
        float maxDistanceFromCenter = Mathf.Min(cellSize.x, cellSize.y) * 0.4f; // 40% de la taille de tuile

        // Le joueur doit être suffisamment proche du centre du trou pour tomber
        if (distanceToHoleCenter <= maxDistanceFromCenter)
        {
            return true;
        }

        // Alternative : vérifier si suffisamment du collider du joueur est au-dessus du trou
        return CheckColliderOverlapWithHole(tilemap, holeTilePosition, playerCenter, radius);
    }

    private bool CheckColliderOverlapWithHole(Tilemap tilemap, Vector3Int holeTilePosition, Vector2 playerCenter, float radius)
    {
        // Points de test autour du collider du joueur
        int numPoints = 12;
        int pointsOverHole = 0;

        // Test du centre
        if (tilemap.WorldToCell(playerCenter) == holeTilePosition)
            pointsOverHole++;

        // Test des points sur le périmètre du collider
        for (int i = 0; i < numPoints; i++)
        {
            float angle = (i * 2 * Mathf.PI) / numPoints;
            Vector2 testPoint = playerCenter + new Vector2(
                Mathf.Cos(angle) * radius * 0.8f, // 80% du rayon pour éviter les bords
                Mathf.Sin(angle) * radius * 0.8f
            );

            Vector3Int testTilePosition = tilemap.WorldToCell(testPoint);
            if (testTilePosition == holeTilePosition && tilemap.HasTile(testTilePosition))
                pointsOverHole++;
        }

        // Le joueur tombe si plus de 60% de ses points de test sont sur le trou
        float overlapPercentage = (float)pointsOverHole / (numPoints + 1);
        return overlapPercentage >= 0.6f;
    }

    void FallIntoHole(Vector3 holeCenter)
    {
        if (!fallIntoHole && !isTeleporting)
        {
            fallIntoHole = true;
            isTeleporting = true;

            // CRITIQUE : Capturer IMMÉDIATEMENT toutes les destinations possibles
            // avec ordre de priorité clair
            Vector3 destinationPosition = Vector3.zero;
            bool hasValidDestination = false;

            // 1. Priorité : safeTeleportation (si valide et loin du trou)
            if (safeTeleportation != null)
            {
                float distanceFromHole = Vector3.Distance(
                    new Vector3(safeTeleportation.position.x, safeTeleportation.position.y, 0),
                    new Vector3(holeCenter.x, holeCenter.y, 0)
                );

                if (distanceFromHole > 1f && !IsPositionInHole(safeTeleportation.position))
                {
                    destinationPosition = safeTeleportation.position;
                    hasValidDestination = true;
                    Debug.Log($"FallIntoHole: Destination = safeTeleportation ({destinationPosition})");
                }
                else
                {
                    Debug.LogWarning($"FallIntoHole: safeTeleportation trop proche du trou ou dans un trou ({distanceFromHole}m)");
                }
            }

            // 2. Backup : safeBackupPosition
            if (!hasValidDestination && safeBackupPosition != Vector3.zero)
            {
                float distanceFromHole = Vector3.Distance(
                    new Vector3(safeBackupPosition.x, safeBackupPosition.y, 0),
                    new Vector3(holeCenter.x, holeCenter.y, 0)
                );

                if (distanceFromHole > 1f && !IsPositionInHole(safeBackupPosition))
                {
                    destinationPosition = safeBackupPosition;
                    hasValidDestination = true;
                    Debug.Log($"FallIntoHole: Destination = safeBackupPosition ({destinationPosition})");
                }
            }

            // 3. Fallback : lastPosition (ancien système)
            if (!hasValidDestination && lastPosition != Vector3.zero)
            {
                float distanceFromHole = Vector3.Distance(
                    new Vector3(lastPosition.x, lastPosition.y, 0),
                    new Vector3(holeCenter.x, holeCenter.y, 0)
                );

                if (distanceFromHole > 1f && !IsPositionInHole(lastPosition))
                {
                    destinationPosition = lastPosition;
                    hasValidDestination = true;
                    Debug.Log($"FallIntoHole: Destination = lastPosition ({destinationPosition})");
                }
            }

            // 4. Dernier recours : chercher une position sûre autour du trou
            if (!hasValidDestination)
            {
                Debug.LogError("FallIntoHole: Aucune destination valide! Recherche d'une position sûre...");
                destinationPosition = FindSafePositionAroundHole(holeCenter);
                hasValidDestination = destinationPosition != Vector3.zero;
            }

            if (!hasValidDestination)
            {
                Debug.LogError("FallIntoHole: IMPOSSIBLE de trouver une destination sûre! Annulation de la chute.");
                fallIntoHole = false;
                isTeleporting = false;
                return;
            }

            StartCoroutine(FallIntoHoleRoutine(holeCenter, destinationPosition));
        }
    }

    // NOUVEAU : Trouver une position sûre autour du trou
    private Vector3 FindSafePositionAroundHole(Vector3 holeCenter)
    {
        // Tester 8 directions autour du trou
        Vector3[] directions = {
        new Vector3(2, 0, 0),
        new Vector3(-2, 0, 0),
        new Vector3(0, 2, 0),
        new Vector3(0, -2, 0),
        new Vector3(1.5f, 1.5f, 0),
        new Vector3(-1.5f, 1.5f, 0),
        new Vector3(1.5f, -1.5f, 0),
        new Vector3(-1.5f, -1.5f, 0)
    };

        foreach (var dir in directions)
        {
            Vector3 testPos = new Vector3(holeCenter.x + dir.x, holeCenter.y + dir.y, transform.position.z);
            if (!IsPositionInHole(testPos))
            {
                Debug.Log($"Position sûre trouvée: {testPos}");
                return testPos;
            }
        }

        Debug.LogError("Impossible de trouver une position sûre!");
        return Vector3.zero;
    }

    IEnumerator FallIntoHoleRoutine(Vector3 holeCenter, Vector3 destinationPosition)
    {
        GetComponent<SoundContainer>().PlaySound("Fall", 1);
        GetComponent<LifeManager>().TakeDamage(1, Color.red, false, true);
        stats.canMove = false;

        Transform playerTransform = transform;
        Vector3 originalScale = playerTransform.localScale;
        float duration = 0.5f;
        float elapsedTime = 0f;

        Vector3 startPosition = playerTransform.position;
        Vector3 finalHolePosition = new Vector3(holeCenter.x, holeCenter.y, startPosition.z);

        // Animation de chute
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            playerTransform.position = Vector3.Lerp(startPosition, finalHolePosition, smoothT);
            playerTransform.localScale = Vector3.Lerp(originalScale, Vector3.zero, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = finalHolePosition;
        playerTransform.localScale = Vector3.zero;

        yield return new WaitForSecondsRealtime(0.5f);

        // Téléportation vers la destination pré-calculée
        Debug.Log($"Téléportation vers: {destinationPosition}");
        playerTransform.position = destinationPosition;

        // Animation de réapparition
        elapsedTime = 0f;
        duration = 0.25f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float scale = Mathf.SmoothStep(0f, 1f, t);
            playerTransform.localScale = originalScale * scale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerTransform.localScale = originalScale;
        stats.canMove = true;
        fallIntoHole = false;

        // Mettre à jour le backup avec la nouvelle position sûre
        safeBackupPosition = destinationPosition;
        lastSafeBackupTime = Time.time;

        // CRITIQUE : Petit délai avant de permettre une nouvelle chute
        // Cela évite que le joueur retombe immédiatement s'il réapparaît près d'un trou
        yield return new WaitForSeconds(0.3f);

        isTeleporting = false;
    }

    bool HasWeapon()
    {
        return Equipement.instance.equippedSlots[4].GetComponent<EquipementSlot>().actualItem != null || MeteoManager.instance.actualScene.name.StartsWith("beggining");
    }

    IEnumerator getLastPosition()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            if (!fallIntoHole && !isTeleporting)
                lastPosition = transform.position;
        }
    }
}
