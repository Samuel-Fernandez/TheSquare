using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum KingAttacks { NONE, LITTLE_BONES, SPEARS, BONE_REGEN, TRIPLE_DASH }

public class SkeletonKingBehiavor : MonoBehaviour
{
    public GameObject littleBonesPrefab;
    public GameObject spearPrefab;
    public GameObject dashChargePrefab;
    public GameObject crackledBonePrefab;
    public List<GameObject> skeletonsPrefab;

    public float timeMinNewAttack;
    public float timeMaxNewAttack;

    private bool pauseEvent; // Arrête le comportement si besoin d'une pause pour l'événement
    private bool isPhaseTwo; // Si est passé en phase 2

    private LifeManager lifeManager;
    private NewMonsterMovement monsterMovement;
    private SoundContainer soundContainer;
    private Stats stats;

    private Coroutine attackRoutine;

    // Liste pour gérer les squelettes
    private List<GameObject> activeSkeletons = new List<GameObject>();
    private const int MAX_SKELETONS = 5;

    GameObject bossBar;

    string id = "SKELETON_KING";

    private KingAttacks actualKingAttack = KingAttacks.NONE;

    private void Start()
    {
        lifeManager = GetComponent<LifeManager>();
        monsterMovement = GetComponent<NewMonsterMovement>();
        soundContainer = GetComponent<SoundContainer>();
        stats = GetComponent<Stats>();
        attackRoutine = StartCoroutine(GetKingAttackRoutine());
        StartCoroutine(SpawnSkeletonsRoutine());

        bossBar = NotificationManager.instance.ShowBossBar(id, stats.health);
    }

    private void Update()
    {
        // Nettoie la liste des squelettes détruits
        CleanupSkeletonsList();

        if (!isPhaseTwo && actualKingAttack == KingAttacks.NONE)
        {
            isPhaseTwo = GetComponent<LifeManager>().life <= stats.health / 2;

            if (isPhaseTwo)
            {
                StopCoroutine(attackRoutine);
                StartCoroutine(PhaseTwoRoutine());
            }
        }

        bossBar.GetComponent<BossBarUI>().UpdateBossLife(GetComponent<LifeManager>().life);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Stats>() != null && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
        {
            // Ne lance PassiveAttackRoutine que si ce n'est pas en cours de TripleDash
            if (actualKingAttack == KingAttacks.NONE)
            {
                StartCoroutine(PassiveAttackRoutine());
            }
        }
    }

    // Nettoie la liste des squelettes en retirant ceux qui sont null (détruits)
    private void CleanupSkeletonsList()
    {
        activeSkeletons.RemoveAll(skeleton => skeleton == null);
    }

    // Ajoute un squelette à la liste de suivi
    private void AddSkeletonToList(GameObject skeleton)
    {
        if (skeleton != null && !activeSkeletons.Contains(skeleton))
        {
            activeSkeletons.Add(skeleton);
        }
    }

    // Détruit tous les squelettes restants
    private void DestroyAllSkeletons()
    {
        foreach (GameObject skeleton in activeSkeletons)
        {
            if (skeleton != null)
            {
                Destroy(skeleton);
            }
        }
        activeSkeletons.Clear();
    }

    // Vérifie si on peut faire apparaître des squelettes
    private bool CanSpawnSkeletons()
    {
        CleanupSkeletonsList();
        return activeSkeletons.Count < MAX_SKELETONS;
    }

    // Récupération des attaques avec probabilités
    private KingAttacks ChooseRandomAttack()
    {
        List<(KingAttacks, float)> attackChances = new List<(KingAttacks, float)>();

        if (!isPhaseTwo)
        {
            // Phase 1
            attackChances.Add((KingAttacks.LITTLE_BONES, 50f));
            attackChances.Add((KingAttacks.SPEARS, 50f));
        }
        else
        {
            // Phase 2
            attackChances.Add((KingAttacks.BONE_REGEN, 20f));
            attackChances.Add((KingAttacks.TRIPLE_DASH, 20f));
            attackChances.Add((KingAttacks.LITTLE_BONES, 15f));
            attackChances.Add((KingAttacks.SPEARS, 15f));
        }

        float total = 0f;
        foreach (var attack in attackChances)
            total += attack.Item2;

        float randomValue = Random.Range(0, total);
        float cumulative = 0f;

        foreach (var attack in attackChances)
        {
            cumulative += attack.Item2;
            if (randomValue <= cumulative)
                return attack.Item1;
        }

        return KingAttacks.LITTLE_BONES; // fallback
    }

    // Détruit tous les squelettes et supprime le boss
    public void RemoveBoss()
    {
        DestroyAllSkeletons();
        Destroy(bossBar);
        Destroy(gameObject);
    }

    #region Coroutines

    IEnumerator SpawnSkeletonsRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(10, 30));

            // Vérifie si on peut faire apparaître des squelettes
            if (!CanSpawnSkeletons())
            {
                continue;
            }

            // APPARITION DES OS
            int maxPossibleSkeletons = MAX_SKELETONS - activeSkeletons.Count;
            int skeletonsToSpawn = isPhaseTwo ?
                Mathf.Min(Random.Range(1, 3), maxPossibleSkeletons) :
                Mathf.Min(Random.Range(1, 2), maxPossibleSkeletons);

            if (skeletonsToSpawn <= 0)
                continue;

            List<Vector3> spawnPositions = new List<Vector3>();

            int attempts = 0;
            int maxAttempts = 200;

            while (spawnPositions.Count < skeletonsToSpawn && attempts < maxAttempts)
            {
                attempts++;

                Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(1f, 6f);
                Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                bool isValid = true;

                foreach (Vector3 pos in spawnPositions)
                {
                    if (Vector3.Distance(pos, spawnPos) < 1f)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (!isValid)
                    continue;

                // Vérifie la présence de colliders sauf le joueur
                Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPos, 0.1f);
                foreach (Collider2D col in colliders)
                {
                    Stats stats = col.GetComponent<Stats>();
                    if (stats == null || stats.entityType != EntityType.Player)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    spawnPositions.Add(spawnPos);
                }
            }

            foreach (Vector3 pos in spawnPositions)
            {
                GameObject skeletonInstance = skeletonsPrefab[Random.Range(0, skeletonsPrefab.Count)];
                GameObject spawnedSkeleton = Instantiate(skeletonInstance, pos, Quaternion.identity);
                AddSkeletonToList(spawnedSkeleton);
            }
        }
    }

    IEnumerator PhaseTwoRoutine()
    {
        monsterMovement.SetSpeedMultiplier(0f);
        monsterMovement.EnableAnimations = false;
        soundContainer.PlaySound("PhaseTwo", 1);
        GetComponent<ObjectAnimation>().PlayAnimation("PhaseTwo", true);
        PlayerManager.instance.player.GetComponent<LifeManager>().KnockBack(PlayerManager.instance.player, 15, gameObject);
        yield return new WaitForSeconds(2);
        stats.speed *= 1.5f;
        timeMinNewAttack /= 2f;
        timeMaxNewAttack /= 2f;
        monsterMovement.SetSpeedMultiplier(1f);
        monsterMovement.EnableAnimations = true;
        actualKingAttack = KingAttacks.NONE;
        attackRoutine = StartCoroutine(GetKingAttackRoutine());
    }

    private bool stopRegen = false;

    IEnumerator BoneRegenRoutine()
    {
        monsterMovement.SetSpeedMultiplier(0f);
        monsterMovement.EnableAnimations = false;
        GetComponent<ObjectAnimation>().PlayAnimation("GroundAttack", true);

        PlayerManager.instance.player.GetComponent<LifeManager>().KnockBack(PlayerManager.instance.player, 15, gameObject);
        soundContainer.PlaySound("BigPunch", 2);

        yield return new WaitForSeconds(1f);

        // Étape 1 : spawn des crackledBones
        float radius = 1.25f;
        float boneSpacing = .75f;
        List<Vector3> spawnPositions = new List<Vector3>();
        Vector3 center = transform.position;

        List<GameObject> crackledBones = new List<GameObject>();

        int maxBones = Mathf.FloorToInt((2 * Mathf.PI * radius) / boneSpacing);
        float angleStep = 360f / maxBones;

        for (int i = 0; i < maxBones; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            Vector3 spawnPos = center + offset;

            Collider2D[] overlaps = Physics2D.OverlapBoxAll(spawnPos, Vector2.one * 0.9f, 0f);
            bool blocked = false;

            foreach (var col in overlaps)
            {
                if (col.isTrigger) continue;

                Stats colStats = col.GetComponent<Stats>();
                if (colStats == null || colStats.entityType != EntityType.Player)
                {
                    blocked = true;
                    break;
                }
            }

            if (!blocked)
            {
                spawnPositions.Add(spawnPos);
            }
        }

        foreach (var pos in spawnPositions)
        {
            GameObject crackled = Instantiate(crackledBonePrefab, pos, Quaternion.identity);
            crackled.GetComponent<ObjectPerspective>().level = GetComponent<ObjectPerspective>().level - 1;
            crackledBones.Add(crackled);
        }

        // Étape 2 : régénération jusqu'à perte de vulnérabilité
        stopRegen = false;
        StartCoroutine(CheckVulnerability());

        LifeManager lifeManager = GetComponent<LifeManager>();

        while (!stopRegen)
        {
            lifeManager.Heal(1);
            soundContainer.PlaySound("LittleBone", 2);
            GetComponent<EntityLight>().TransitionLightIntensity(4, 8, .25f);
            GetComponent<EntityLight>().TransitionLightColor(Color.green, .25f);
            yield return new WaitForSeconds(0.25f);
            GetComponent<EntityLight>().TransitionLightIntensity(.5f, 1, .25f);
            GetComponent<EntityLight>().TransitionLightColor(Color.white, .25f);
            yield return new WaitForSeconds(0.25f);
        }

        foreach (var bone in crackledBones)
        {
            if (bone != null)
                bone.GetComponent<DestroyableBehiavor>().ForceDestroy();
        }

        // Étape 3 : retour à l'état normal
        yield return new WaitForSeconds(3f);

        monsterMovement.EnableAnimations = true;
        monsterMovement.SetSpeedMultiplier(1f);
        actualKingAttack = KingAttacks.NONE;
    }

    IEnumerator CheckVulnerability()
    {
        while (stats.isVulnerable)
            yield return null;

        stopRegen = true;
    }



    IEnumerator TripleDashRoutine()
    {
        monsterMovement.SetSpeedMultiplier(0f);
        monsterMovement.EnableAnimations = false;
        ObjectAnimation animation = GetComponent<ObjectAnimation>();
        animation.PlayAnimation("TripleDash");
        GameObject dashInstance = Instantiate(dashChargePrefab, new Vector2(transform.position.x, transform.position.y - .5f), Quaternion.identity);
        dashInstance.transform.localScale *= 2;
        soundContainer.PlaySound("Charge", 2);
        yield return new WaitForSeconds(1f); // Charge initiale

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float dashSpeed = 16f;
        int dashCount = 3;

        for (int i = 0; i < dashCount; i++)
        {
            GameObject player = PlayerManager.instance.player;

            stats.doingAttack = true;

            soundContainer.PlaySound("Dash", 2);
            if (player == null)
                break;

            Vector3 direction = (player.transform.position - transform.position).normalized;
            float dashDuration = 0.5f;
            float dashTimer = 0f;
            animation.PlayAnimation("PassiveAttack", true);
            bool interrupted = false;

            while (dashTimer < dashDuration)
            {
                Vector3 movement = direction * dashSpeed * Time.deltaTime;
                rb.MovePosition(rb.position + new Vector2(movement.x, movement.y));

                // Vérifie collision avec le joueur
                if (!interrupted)
                {
                    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
                    foreach (var hit in hits)
                    {
                        Stats hitStats = hit.GetComponent<Stats>();
                        if (hitStats != null && hitStats.entityType == EntityType.Player)
                        {
                            // Applique les dégâts comme dans PassiveAttackRoutine
                            soundContainer.PlaySound("BigPunch", 2);
                            CameraManager.instance.ShakeCamera(3, 3, .5f);
                            PlayerManager.instance.player.GetComponent<LifeManager>().TakeDamage(stats.strength, gameObject, false);

                            interrupted = true;
                            break; // Sort du foreach
                        }
                    }
                }

                dashTimer += Time.deltaTime;
                yield return null;
            }

            stats.doingAttack = false;

            // Pause entre les dashs même en cas d'interruption
            yield return new WaitForSeconds(1.5f);
        }

        monsterMovement.EnableAnimations = true;
        monsterMovement.SetSpeedMultiplier(1f);
        actualKingAttack = KingAttacks.NONE;
    }

    IEnumerator PassiveAttackRoutine()
    {
        monsterMovement.SetSpeedMultiplier(0);
        GetComponent<ObjectAnimation>().PlayAnimation("PassiveAttack");
        soundContainer.PlaySound("BigPunch", 2);
        CameraManager.instance.ShakeCamera(3, 3, .5f);
        PlayerManager.instance.player.GetComponent<LifeManager>().TakeDamage(stats.strength, gameObject, false);
        monsterMovement.EnableAnimations = false;

        yield return new WaitForSeconds(.5f);
        monsterMovement.EnableAnimations = true;
        monsterMovement.SetSpeedMultiplier(1);
    }

    IEnumerator LittleBonesRoutine()
    {
        // ANIMATION
        monsterMovement.SetSpeedMultiplier(0);
        monsterMovement.EnableAnimations = false;

        soundContainer.PlaySound("Levitation", 2);
        GetComponent<ObjectParticles>().SpawnParticle("DarkMagic", new Vector2(transform.position.x, transform.position.y + 2));

        GetComponent<ObjectAnimation>().PlayAnimation("AirAttack", true, false);
        yield return StartCoroutine(FloatUpRoutine(1f, 2f));
        GetComponent<ObjectPerspective>().level++;

        soundContainer.PlaySound("LittleBone", 2);

        // APPARITION DES OS
        int bonesToSpawn = isPhaseTwo ? Random.Range(8, 16) : Random.Range(4, 16);
        List<Vector3> spawnPositions = new List<Vector3>();

        int attempts = 0;
        int maxAttempts = 200;

        while (spawnPositions.Count < bonesToSpawn && attempts < maxAttempts)
        {
            attempts++;

            Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(0, 2f);
            Vector3 spawnPos = PlayerManager.instance.player.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            bool isValid = true;

            // Vérifie l'espacement avec les autres os
            foreach (Vector3 pos in spawnPositions)
            {
                if (Vector3.Distance(pos, spawnPos) < 1f)
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid)
                continue;

            // Vérifie la présence de colliders sauf le joueur
            Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPos, 0.1f);
            foreach (Collider2D col in colliders)
            {
                Stats stats = col.GetComponent<Stats>();
                if (stats == null || stats.entityType != EntityType.Player)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                spawnPositions.Add(spawnPos);
            }
        }


        foreach (Vector3 pos in spawnPositions)
        {
            GameObject littleBoneInstance = Instantiate(littleBonesPrefab, pos, Quaternion.identity);
            littleBoneInstance.transform.localScale *= 2;
            littleBoneInstance.GetComponent<ObjectPerspective>().level = GetComponent<ObjectPerspective>().level - 1;
            yield return new WaitForSeconds(0.05f);
        }

        // FIN DE L'ATTAQUE
        GetComponent<ObjectAnimation>().PlayAnimation("AirAttack", true, true);
        yield return StartCoroutine(FloatUpRoutine(1f, -2f));

        GetComponent<ObjectPerspective>().level--;
        monsterMovement.EnableAnimations = true;
        monsterMovement.SetSpeedMultiplier(1);
        actualKingAttack = KingAttacks.NONE;
    }

    IEnumerator SpearsRoutine()
    {
        // ANIMATION
        monsterMovement.SetSpeedMultiplier(0);
        monsterMovement.EnableAnimations = false;

        soundContainer.PlaySound("Levitation", 2);
        GetComponent<ObjectParticles>().SpawnParticle("DarkMagic", new Vector2(transform.position.x, transform.position.y + 2));

        GetComponent<ObjectAnimation>().PlayAnimation("AirAttack", true, false);
        yield return StartCoroutine(FloatUpRoutine(1f, 2f));
        GetComponent<ObjectPerspective>().level++;

        // ATTAQUE : Spawn de 3 spears à 4 unités autour du joueur
        GameObject player = PlayerManager.instance.player;
        if (player != null)
        {
            Vector3 playerPos = player.transform.position;
            List<Vector3> usedPositions = new List<Vector3>();
            int spearsToSpawn = isPhaseTwo ? 6 : 3;
            int attempts = 0;
            int maxAttempts = 100;

            while (usedPositions.Count < spearsToSpawn && attempts < maxAttempts)
            {
                attempts++;

                Vector2 randomDir = Random.insideUnitCircle.normalized;
                Vector3 spawnPos = playerPos + new Vector3(randomDir.x, randomDir.y, 0f) * 4f;

                bool isValid = true;

                foreach (var pos in usedPositions)
                {
                    if (Vector3.Distance(pos, spawnPos) < 1f)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid && Physics2D.OverlapPointAll(spawnPos).Length > 0)
                {
                    isValid = false;
                }

                if (isValid)
                {
                    usedPositions.Add(spawnPos);
                }
            }

            foreach (Vector3 pos in usedPositions)
            {
                soundContainer.PlaySound("SpawnSpear", 3);
                GameObject spearInstance = Instantiate(spearPrefab, pos, Quaternion.identity);
                spearInstance.GetComponent<ObjectPerspective>().level = GetComponent<ObjectPerspective>().level;
                spearInstance.GetComponent<SpearBehiavor>().InitSpear(stats.strength, 12, false, stats.knockbackPower, this.gameObject);

                yield return new WaitForSeconds(1f);
            }
        }

        // FIN DE L'ATTAQUE
        GetComponent<ObjectAnimation>().PlayAnimation("AirAttack", true, true);
        yield return StartCoroutine(FloatUpRoutine(1f, -2f));

        GetComponent<ObjectPerspective>().level--;
        monsterMovement.EnableAnimations = true;
        monsterMovement.SetSpeedMultiplier(1);
        actualKingAttack = KingAttacks.NONE;
    }

    IEnumerator FloatUpRoutine(float duration, float targetYOffset)
    {
        // Récupère le sprite et la hitbox
        var spriteTransform = GetComponentInChildren<SpriteRenderer>()?.transform;
        var collider = GetComponent<Collider2D>();

        if (spriteTransform == null)
        {
            yield break;
        }

        Vector3 startPos = spriteTransform.localPosition;
        Vector3 endPos = startPos + new Vector3(0f, targetYOffset, 0f);

        if (collider != null)
            collider.enabled = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            spriteTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        if (collider != null)
            collider.enabled = targetYOffset < 0;
    }

    // Choisi une attaque pour l'entité et lance l'attaque
    IEnumerator GetKingAttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(timeMinNewAttack, timeMaxNewAttack));

            if (pauseEvent || actualKingAttack != KingAttacks.NONE)
                continue;

            actualKingAttack = ChooseRandomAttack();

            switch (actualKingAttack)
            {
                case KingAttacks.LITTLE_BONES:
                    yield return StartCoroutine(LittleBonesRoutine());
                    break;
                case KingAttacks.SPEARS:
                    yield return StartCoroutine(SpearsRoutine());
                    break;
                case KingAttacks.BONE_REGEN:
                    yield return StartCoroutine(BoneRegenRoutine());
                    break;
                case KingAttacks.TRIPLE_DASH:
                    yield return StartCoroutine(TripleDashRoutine());
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        // Stop toutes les coroutines du boss
        StopAllCoroutines();

        // Détruit tous les squelettes actifs
        DestroyAllSkeletons();

        // Supprime la barre de vie
        if (bossBar != null)
            Destroy(bossBar);
    }

        #endregion
}