using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MyliaPhase1Attack { NONE, ICE_BUMPER, TELEPORT, ICE_SPADE, ICE_TEAR }

public class MyliaPhase1Behiavor : MonoBehaviour
{
    [Header("Boss Settings")]
    public float timeMinNewAttack = 1f;
    public float timeMaxNewAttack = 3f;
    public string attackSoundName = "Attack";

    [Header("Attack Dialogue")]
    public GameObject bossAttackTextPrefab;
    public Transform bossAttackTextParent;

    [Header("Ice Bumper Attack")]
    public GameObject iceBlockPrefab;
    public string prayAnimation = "Pray";
    public float prayAnimationDuration = 0.5f;
    public float iceBlockAppearDuration = 1f;
    public float spinDuration = 8f;
    public float slowDownDuration = 3f;
    public float colliderRadiusMultiplier = 2f;
    public float spinSpeedMultiplier = 4f;
    public string bounceSoundName = "Hit";

    [Header("Fire Vulnerability")]
    public float fireVulnerabilityDuration = 5f;

    [Header("Vulnerability End")]
    public string vulnerabilityEndSoundName = "Defense";
    public string vulnerabilityEndAnimation = "RaiseArms";
    public float vulnerabilityEndAnimationDuration = 0.15f;
    public string afkDownAnimation = "AfkDown";
    public string downArmsAnimation = "DownArms";
    public float downArmsAnimationDuration = 0.8f;
    public float vulnerabilityEndShakeAmplitude = 4f;
    public float vulnerabilityEndShakeFrequency = 4f;
    public float vulnerabilityEndShakeDuration = 1f;
    public Color explosionLightColor = Color.white;
    public float explosionLightIntensityMultiplier = 3f;
    public float explosionLightRadiusMultiplier = 3f;
    public float explosionLightBurstDuration = 0.1f;
    public float explosionLightFadeDuration = 0.6f;

    [Header("Teleportation")]
    public string teleportSoundName = "Teleportation";
    public int teleportCount = 3;
    public float teleportRadius = 3f;
    public float teleportShakeDuration = 0.15f;
    public float teleportShakeMagnitude = 0.1f;
    public Color teleportFlashColor = new Color(0.2f, 0.6f, 1f, 1f);
    public float teleportFlashFadeInDuration = 0.1f;
    public float teleportFlashFadeOutDuration = 0.1f;
    public float teleportDownArmsDuration = 0.3f;

    [Header("Ice Spade Attack")]
    public GameObject iceSpadePrefab;
    public int iceSpadeCount = 6;
    public float iceSpadeSpawnRadius = 2f;
    public float iceSpadeSpawnInterval = 0.25f;
    public float iceSpadeOrbitSpeed = 45f;
    public float iceSpadeSelfRotationSpeed = 360f;
    public float iceSpadeDelayBeforeLaunches = 3f;
    public float iceSpadeLaunchInterval = 0.75f;
    public int iceSpadeDamage = 1;
    public float iceSpadeSpeed = 6f;

    [Header("Ice Tear Attack")]
    public GameObject iceTearPrefab;
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;
    public string crySoundName = "Cry";
    public int iceTearCount = 8;
    public float iceTearAttackDuration = 5f;
    public int iceTearDamage = 1;
    public float iceTearSpeed = 1.5f;

    [Header("Sun Darkness")]
    public float sunTransitionDuration = 1f;

    [Header("Ice Bumper Field")]
    public GameObject iceBlockObstaclePrefab;
    public int minIceBlockObstacles = 5;
    public int maxIceBlockObstacles = 15;
    public float iceBlockFieldRadius = 10f;
    public float iceBlockMinSpacing = 1f;
    public float iceBlockObstacleCheckRadius = 0.4f;

    [Header("Ice Ground Field")]
    public float iceGroundRadius = 30f;
    public float iceGroundFillDuration = 2f;
    public string iceGroundSoundName = "IceGround";

    [Header("Damage Reaction")]
    public float damageShakeAmplitude = 2f;
    public float damageShakeFrequency = 2f;
    public float damageShakeDuration = 0.3f;
    public Color angryLightColor = Color.red;
    public float angryLightIntensityMultiplier = 1.5f;
    public float angryLightFlashDuration = 0.3f;

    [Header("Ice Bumper Light")]
    public float lightStartIntensity = 0f;
    public float lightStartRadius = 0f;
    public float lightTargetIntensity = 1f;
    public float lightTargetRadius = 3f;
    public Color lightStartColor = Color.white;
    public Color lightTargetColor = Color.cyan;

    LifeManager lifeManager;
    SoundContainer soundContainer;
    Stats stats;
    Rigidbody2D rb;
    CircleCollider2D circleCollider;
    ObjectAnimation objectAnimation;
    SpriteRenderer spriteRenderer;
    SpriteRenderer shadowSpriteRenderer;
    EntityEffects entityEffects;
    EntityLight entityLight;
    Transform playerTransform;
    Coroutine attackRoutine;
    Coroutine sunDarknessCoroutine;
    Coroutine iceGroundFillCoroutine;
    Coroutine lightEffectCoroutine;
    GameObject bossBar;
    BossBarUI bossBarUI;

    Material originalSpriteMaterial;
    Material teleportFlashMaterial;
    float originalSpriteAlpha;
    float originalShadowAlpha;

    Color baseLightColor = Color.white;
    float baseLightIntensity;
    float baseLightRadius;
    int previousLife = -1;

    bool isSpinning = false;
    bool isStunned = false;
    Vector2 spinDirection = Vector2.zero;
    float currentSpinSpeed = 0f;
    float baseColliderRadius;
    float? speedBeforeAttackBuff = null;
    float savedSunIntensity;

    MyliaPhase1Attack actualAttack = MyliaPhase1Attack.NONE;
    bool death = false;
    string id = "MYLIA_PHASE1";

    // Fenetre de vulnerabilite legitime (ICE_SPADE, ICE_TEAR, fin de ICE_BUMPER en feu). Necessaire
    // car DamageEffect.FlashColorCoroutine remet stats.isVulnerable a true tout seul 0.25s apres
    // n'importe quel coup encaisse, meme si la fenetre de Mylia est deja terminee entre-temps : un
    // coup bien place en bordure de fenetre pouvait donc la rouvrir en boucle (spam). Update() force
    // isVulnerable a rester ferme tant qu'on n'est pas dans une de ces fenetres.
    bool inVulnerabilityWindow = false;

    List<Vector3Int> iceFieldCells = new List<Vector3Int>();

    // Objets spawnes non parentes au boss (donc pas detruits automatiquement avec lui) : suivis ici
    // pour pouvoir tous les nettoyer d'un coup des que la vie tombe a 0 (cf. CleanupSpawnedObjects).
    List<GameObject> activeIceSpades = new List<GameObject>();
    List<GameObject> activeIceTears = new List<GameObject>();
    List<GameObject> activeIceBlockObstacles = new List<GameObject>();

    private void Start()
    {
        lifeManager = GetComponent<LifeManager>();
        soundContainer = GetComponent<SoundContainer>();
        stats = GetComponent<Stats>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        objectAnimation = GetComponent<ObjectAnimation>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        shadowSpriteRenderer = GetComponent<ObjectPerspective>()?.shadowSpriteRenderer;
        entityEffects = GetComponent<EntityEffects>();
        entityLight = GetComponent<EntityLight>();

        if (entityLight != null)
        {
            baseLightColor = entityLight.CurrentColor;
            baseLightIntensity = entityLight.CurrentIntensity;
            baseLightRadius = entityLight.CurrentRadius;
        }

        baseColliderRadius = circleCollider.radius;

        originalSpriteMaterial = spriteRenderer.sharedMaterial;
        teleportFlashMaterial = new Material(Shader.Find("Custom/SpriteFlash"));
        teleportFlashMaterial.SetColor("_FlashColor", teleportFlashColor);
        originalSpriteAlpha = spriteRenderer.color.a;
        originalShadowAlpha = shadowSpriteRenderer != null ? shadowSpriteRenderer.color.a : 0f;

        // Seule l'attaque ICE_BUMPER bloque les coups d'epee du joueur (cf. IceBumperAttack) : partout
        // ailleurs, un coup porte hors vulnerabilite declenche plutot une esquive (TriggerDodgeTeleport).
        stats.blockPlayerAttack = false;

        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        InitBoss();
    }

    private void Update()
    {
        // Verrouille isVulnerable en dehors des fenetres legitimes : contre le hit-flash de
        // DamageEffect qui peut sinon la rouvrir tout seul apres la fin d'une attaque (cf. spam).
        if (!inVulnerabilityWindow && stats.isVulnerable)
        {
            stats.isVulnerable = false;
        }

        if (bossBarUI != null)
            bossBarUI.UpdateBossLife(lifeManager.life);

        if (playerTransform == null && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }

        if (previousLife < 0)
        {
            previousLife = lifeManager.life;
        }
        else if (!death && lifeManager.life < previousLife)
        {
            OnDamageTaken();
        }
        previousLife = lifeManager.life;

        if (lifeManager.life <= 0 && !death)
        {
            death = true;
            StopAllCoroutines();
            attackRoutine = null;
            sunDarknessCoroutine = null;
            iceGroundFillCoroutine = null;
            StopSunDarkness();
            CleanupSpawnedObjects();
            Destroy(bossBar);
            StartCoroutine(DeathRoutine());
            StartCoroutine(DeathSoundRoutine());
        }
    }

    void InitBoss()
    {
        attackRoutine = StartCoroutine(AttackRoutine());
        bossBar = NotificationManager.instance.ShowBossBar(id, stats.health);
        bossBarUI = bossBar.GetComponent<BossBarUI>();
    }

    // Instancie la repartie de combat (voir BossAttackTextUI) : non bloquant, elle s'affiche et se
    // detruit d'elle-meme pendant que l'attaque se deroule en parallele.
    void PlayAttackText()
    {
        if (bossAttackTextPrefab == null) return;

        GameObject instance = Instantiate(bossAttackTextPrefab, bossAttackTextParent);
        instance.GetComponent<BossAttackTextUI>().Play(id);
    }

    // Public : appelee aussi bien par la detection generique de perte de vie (Update) que
    // directement par MyliaIceSpadeBehiavor lorsqu'une lance deviee la touche, pour garantir le
    // meme tremblement d'ecran qu'un coup d'epee sans dependre du timing de la frame suivante.
    public void OnDamageTaken()
    {
        CameraManager.instance.ShakeCamera(damageShakeAmplitude, damageShakeFrequency, damageShakeDuration);

        if (entityLight == null) return;

        if (lightEffectCoroutine != null)
        {
            StopCoroutine(lightEffectCoroutine);
        }
        lightEffectCoroutine = StartCoroutine(AngryLightFlashRoutine());
    }

    IEnumerator AngryLightFlashRoutine()
    {
        float halfDuration = angryLightFlashDuration * 0.5f;

        entityLight.TransitionLightColor(angryLightColor, halfDuration);
        entityLight.TransitionLightIntensity(baseLightIntensity * angryLightIntensityMultiplier, baseLightRadius, halfDuration);

        yield return new WaitForSeconds(halfDuration);

        entityLight.TransitionLightColor(baseLightColor, halfDuration);
        entityLight.TransitionLightIntensity(baseLightIntensity, baseLightRadius, halfDuration);

        lightEffectCoroutine = null;
    }

    // IEnumerator (et non void) : le retour de vulnerabilite doit rester actif tant que la teleportation
    // n'est pas terminee, sinon AttackRoutine remettrait actualAttack a NONE trop tot et pourrait
    // declencher une nouvelle attaque pendant que Mylia est encore en train de se teleporter.
    IEnumerator OnVulnerabilityEndRoutine()
    {
        CameraManager.instance.ShakeCamera(vulnerabilityEndShakeAmplitude, vulnerabilityEndShakeFrequency, vulnerabilityEndShakeDuration);
        soundContainer.PlaySound(vulnerabilityEndSoundName, 1);

        if (entityLight != null)
        {
            if (lightEffectCoroutine != null)
            {
                StopCoroutine(lightEffectCoroutine);
            }
            lightEffectCoroutine = StartCoroutine(LightExplosionRoutine());
        }

        actualAttack = MyliaPhase1Attack.TELEPORT;
        yield return StartCoroutine(TeleportAttack());
        actualAttack = MyliaPhase1Attack.NONE;
    }

    // Appelee par PlayerController lorsque le joueur touche Mylia a l'epee alors qu'elle n'est ni
    // vulnerable ni deja en train d'attaquer : elle se teleporte pour esquiver ce coup. Ne fait rien
    // si elle est deja occupee (attaque en cours, y compris une esquive precedente).
    public void TriggerDodgeTeleport()
    {
        if (actualAttack != MyliaPhase1Attack.NONE || stats.isVulnerable || isStunned || death) return;

        actualAttack = MyliaPhase1Attack.TELEPORT;
        StartCoroutine(DodgeTeleportRoutine());
    }

    IEnumerator DodgeTeleportRoutine()
    {
        yield return StartCoroutine(TeleportAttack());
        actualAttack = MyliaPhase1Attack.NONE;
    }

    // Attaque TELEPORT : enchainee apres la fenetre de vulnerabilite (cf. OnVulnerabilityEndRoutine)
    // ou declenchee en esquive quand le joueur l'attaque a l'epee hors attaque/vulnerabilite (cf.
    // TriggerDodgeTeleport). RaiseArms reste figee sur sa derniere image (LastSpriteStay) pendant
    // toute la boucle des n teleportations ; DownArms puis AfkDown ne sont joues qu'une fois la
    // sequence de teleportation terminee.
    IEnumerator TeleportAttack()
    {
        stats.doingAttack = true;

        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(vulnerabilityEndAnimation, true);
        yield return new WaitForSeconds(vulnerabilityEndAnimationDuration);

        yield return StartCoroutine(TeleportSequenceRoutine());

        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(downArmsAnimation);
        yield return new WaitForSeconds(teleportDownArmsDuration);

        objectAnimation.PlayAnimation(afkDownAnimation);

        stats.doingAttack = false;
    }

    // Attaque ICE_SPADE : fait apparaitre iceSpadeCount lances (une par une, en cercle regulier
    // autour de Mylia) qui orbitent et tournent sur elles-memes en attendant d'etre lancees une par
    // une, aleatoirement, sur le joueur. L'attaque se termine des que la derniere a ete lancee.
    IEnumerator IceSpadeAttack()
    {
        PlayAttackText();

        stats.doingAttack = true;
        stats.isVulnerable = true;
        inVulnerabilityWindow = true;
        stats.blockPlayerAttack = false;

        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(vulnerabilityEndAnimation, true);

        List<MyliaIceSpadeBehiavor> spades = new List<MyliaIceSpadeBehiavor>();
        List<float> orbitAngles = new List<float>();
        List<bool> launched = new List<bool>();

        float angleStep = 360f / iceSpadeCount;

        for (int i = 0; i < iceSpadeCount; i++)
        {
            float angleDeg = angleStep * i;
            Vector3 spawnPosition = GetIceSpadeOrbitPosition(angleDeg);

            GameObject instance = Instantiate(iceSpadePrefab, spawnPosition, Quaternion.identity);
            MyliaIceSpadeBehiavor spade = instance.GetComponent<MyliaIceSpadeBehiavor>();
            spade.owner = transform;
            spade.Init(iceSpadeDamage, iceSpadeSpeed);
            spade.Rotate(iceSpadeSelfRotationSpeed);

            activeIceSpades.Add(instance);
            spades.Add(spade);
            orbitAngles.Add(angleDeg);
            launched.Add(false);

            yield return new WaitForSeconds(iceSpadeSpawnInterval);
        }

        Coroutine orbitCoroutine = StartCoroutine(OrbitIceSpadesRoutine(spades, orbitAngles, launched));

        yield return new WaitForSeconds(iceSpadeDelayBeforeLaunches);

        List<int> remainingIndices = new List<int>();
        for (int i = 0; i < spades.Count; i++)
        {
            remainingIndices.Add(i);
        }

        while (remainingIndices.Count > 0)
        {
            remainingIndices.RemoveAll(index => spades[index] == null);
            if (remainingIndices.Count == 0) break;

            int pick = Random.Range(0, remainingIndices.Count);
            int spadeIndex = remainingIndices[pick];
            remainingIndices.RemoveAt(pick);

            launched[spadeIndex] = true;

            MyliaIceSpadeBehiavor spade = spades[spadeIndex];
            if (spade != null)
            {
                soundContainer.PlaySound(attackSoundName, 1);
                // StopRotate coupe net la rotation et oriente/lance immediatement la pointe vers
                // la position actuelle du joueur (pas de poursuite continue une fois lancee).
                spade.StopRotate(playerTransform != null ? playerTransform : transform);
            }

            yield return new WaitForSeconds(iceSpadeLaunchInterval);
        }

        StopCoroutine(orbitCoroutine);

        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(downArmsAnimation);
        yield return new WaitForSeconds(downArmsAnimationDuration);

        objectAnimation.PlayAnimation(afkDownAnimation);

        stats.doingAttack = false;
        stats.isVulnerable = false;
        inVulnerabilityWindow = false;
    }

    Vector3 GetIceSpadeOrbitPosition(float angleDeg)
    {
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return transform.position + new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * iceSpadeSpawnRadius;
    }

    // Fait tourner autour de Mylia toutes les lances pas encore lancees, tant que l'attaque dure.
    IEnumerator OrbitIceSpadesRoutine(List<MyliaIceSpadeBehiavor> spades, List<float> orbitAngles, List<bool> launched)
    {
        while (true)
        {
            for (int i = 0; i < spades.Count; i++)
            {
                if (spades[i] == null || launched[i]) continue;

                orbitAngles[i] += iceSpadeOrbitSpeed * Time.deltaTime;
                spades[i].transform.position = GetIceSpadeOrbitPosition(orbitAngles[i]);
            }

            yield return null;
        }
    }

    // Attaque ICE_TEAR : Mylia pleure (Pray tenue, 2 sons "Cry" repartis sur iceTearAttackDuration)
    // et fait apparaitre iceTearCount larmes une par une, alternativement depuis chaque oeil.
    IEnumerator IceTearAttack()
    {
        PlayAttackText();

        stats.doingAttack = true;
        stats.isVulnerable = true;
        inVulnerabilityWindow = true;

        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(prayAnimation, true);

        float spawnInterval = iceTearAttackDuration / iceTearCount;

        soundContainer.PlaySound(crySoundName, 1);

        for (int i = 0; i < iceTearCount; i++)
        {
            // 2e "Cry" a mi-chemin des iceTearCount apparitions, reparti sur les 5 secondes.
            if (i == iceTearCount / 2)
            {
                soundContainer.PlaySound(crySoundName, 1);
            }

            SpawnIceTear(i);

            yield return new WaitForSeconds(spawnInterval);
        }

        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation("Idle");

        stats.doingAttack = false;
        stats.isVulnerable = false;
        inVulnerabilityWindow = false;
    }

    void SpawnIceTear(int index)
    {
        Transform eye = (index % 2 == 0) ? leftEyeTransform : rightEyeTransform;
        if (eye == null) eye = transform;

        GameObject instance = Instantiate(iceTearPrefab, eye.position, Quaternion.identity);
        IceTearBehiavor tear = instance.GetComponent<IceTearBehiavor>();
        tear.owner = transform;
        tear.Init(iceTearSpeed, iceTearDamage);

        activeIceTears.Add(instance);
    }

    IEnumerator TeleportSequenceRoutine()
    {
        for (int i = 0; i < teleportCount; i++)
        {
            soundContainer.PlaySound(teleportSoundName, 1);

            // Fondu vers un bleu plein avant meme le tremblement, qui precede le saut.
            spriteRenderer.material = teleportFlashMaterial;
            yield return StartCoroutine(TeleportFlashRoutine(0f, 1f, teleportFlashFadeInDuration));

            yield return StartCoroutine(SpriteShakeRoutine(teleportShakeDuration, teleportShakeMagnitude));

            transform.position = FindValidTeleportPosition(transform.position, teleportRadius);

            yield return StartCoroutine(TeleportFlashRoutine(1f, 0f, teleportFlashFadeOutDuration));
            spriteRenderer.material = originalSpriteMaterial;
        }
    }

    // from/to portent sur _FlashAmount (0 = normal, 1 = bleu plein) ; le sprite et son ombre
    // disparaissent en meme temps par l'alpha, en opposition directe avec ce fondu de couleur
    // (invisibles au moment ou la couleur est pleinement bleue, visibles quand elle est normale).
    IEnumerator TeleportFlashRoutine(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ApplyTeleportFlash(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        ApplyTeleportFlash(to);
    }

    void ApplyTeleportFlash(float flashAmount)
    {
        teleportFlashMaterial.SetFloat("_FlashAmount", flashAmount);

        float visibility = 1f - flashAmount;

        Color spriteColor = spriteRenderer.color;
        spriteColor.a = originalSpriteAlpha * visibility;
        spriteRenderer.color = spriteColor;

        if (shadowSpriteRenderer != null)
        {
            Color shadowColor = shadowSpriteRenderer.color;
            shadowColor.a = originalShadowAlpha * visibility;
            shadowSpriteRenderer.color = shadowColor;
        }
    }

    IEnumerator SpriteShakeRoutine(float duration, float magnitude)
    {
        if (spriteRenderer == null)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        Vector3 originalLocalPosition = spriteRenderer.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.transform.localPosition = originalLocalPosition + (Vector3)(Random.insideUnitCircle * magnitude);
            yield return null;
        }

        spriteRenderer.transform.localPosition = originalLocalPosition;
    }

    // Choisit un point dans un rayon donné qui n'empiete pas sur un collider solide (mur, obstacle...)
    // et dont le trajet depuis la position actuelle ne traverse pas non plus un tel collider,
    // pour eviter que Mylia ne se teleporte a travers ou de l'autre cote d'un mur.
    Vector3 FindValidTeleportPosition(Vector3 origin, float radius)
    {
        const int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 candidate = (Vector2)origin + Random.insideUnitCircle * radius;

            if (!IsTeleportPathBlocked(origin, candidate))
            {
                return candidate;
            }
        }

        return origin;
    }

    // Balaye un cercle du rayon de collision de Mylia le long du segment origine -> destination :
    // detecte a la fois un obstacle a l'arrivee et un mur traverse en chemin.
    bool IsTeleportPathBlocked(Vector2 origin, Vector2 destination)
    {
        Vector2 offset = destination - origin;
        float distance = offset.magnitude;
        Vector2 direction = distance > 0.001f ? offset / distance : Vector2.zero;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, baseColliderRadius, direction, distance);
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.gameObject == gameObject) continue;
            if (!hit.collider.isTrigger)
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator LightExplosionRoutine()
    {
        entityLight.TransitionLightColor(explosionLightColor, explosionLightBurstDuration);
        entityLight.TransitionLightIntensity(baseLightIntensity * explosionLightIntensityMultiplier, baseLightRadius * explosionLightRadiusMultiplier, explosionLightBurstDuration);

        yield return new WaitForSeconds(explosionLightBurstDuration);

        entityLight.TransitionLightColor(baseLightColor, explosionLightFadeDuration);
        entityLight.TransitionLightIntensity(baseLightIntensity, baseLightRadius, explosionLightFadeDuration);

        lightEffectCoroutine = null;
    }

    IEnumerator DeathSoundRoutine()
    {
        while (true)
        {
            soundContainer.PlaySound("Hurt", 1);
            yield return new WaitForSeconds(.5f);
        }
    }

    IEnumerator DeathRoutine()
    {
        GetComponent<BossDeathEffect>().SpawnExplosions(4, 6);
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    // Detruit tout ce que le boss a spawn en tant qu'objets independants (non parentes a son transform,
    // donc pas detruits automatiquement avec lui) et efface les tiles de glace posees au sol, pour
    // qu'aucune larme, lance ou bloc de glace ne survive a la mort de Mylia.
    void CleanupSpawnedObjects()
    {
        DestroyAllInList(activeIceSpades);
        DestroyAllInList(activeIceTears);
        DestroyAllInList(activeIceBlockObstacles);

        if (MyliaIceFieldTilemap.instance != null)
        {
            MyliaIceFieldTilemap.instance.RemoveTiles(iceFieldCells);
        }
        iceFieldCells.Clear();
    }

    void DestroyAllInList(List<GameObject> objects)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        objects.Clear();
    }

    #region Attack Logic

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(timeMinNewAttack, timeMaxNewAttack));

            if (isStunned)
                continue;

            if (actualAttack != MyliaPhase1Attack.NONE)
                continue;

            actualAttack = ChooseRandomAttack();

            if (actualAttack != MyliaPhase1Attack.NONE)
            {
                soundContainer.PlaySound(attackSoundName, 1);
            }

            switch (actualAttack)
            {
                case MyliaPhase1Attack.NONE:
                    break;
                case MyliaPhase1Attack.ICE_BUMPER:
                    yield return StartCoroutine(IceBumperAttack());
                    break;
                case MyliaPhase1Attack.ICE_SPADE:
                    yield return StartCoroutine(IceSpadeAttack());
                    break;
                case MyliaPhase1Attack.ICE_TEAR:
                    yield return StartCoroutine(IceTearAttack());
                    break;
            }

            actualAttack = MyliaPhase1Attack.NONE;
        }
    }

    MyliaPhase1Attack ChooseRandomAttack()
    {
        MyliaPhase1Attack[] attacks = { MyliaPhase1Attack.ICE_BUMPER, MyliaPhase1Attack.ICE_SPADE, MyliaPhase1Attack.ICE_TEAR };
        //MyliaPhase1Attack[] attacks = { MyliaPhase1Attack.ICE_SPADE };
        return attacks[Random.Range(0, attacks.Length)];
    }

    IEnumerator IceBumperAttack()
    {
        PlayAttackText();

        stats.doingAttack = true;
        stats.blockPlayerAttack = true;

        // 1. Prière : anim figée sur la dernière image pendant 0.5s, en même temps que le spawn du champ de blocs de glace dans l'arène
        // et l'extinction de la lumière du soleil, qui restera à 0 jusqu'à ce que le feu la touche ou que l'attaque se termine
        StartSunDarkness();
        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation(prayAnimation, true);
        iceFieldCells.Clear();
        soundContainer.PlaySound(iceGroundSoundName, 1);
        iceGroundFillCoroutine = StartCoroutine(FillIceGroundCircle());
        List<GameObject> iceBlockField = SpawnIceBlockField();
        yield return new WaitForSeconds(prayAnimationDuration);

        // 2. Apparition du bloc de glace au centre de Mylia, en enfant (suit tous ses mouvements), avec élévation de lumière sur 1s
        GameObject iceBlockInstance = Instantiate(iceBlockPrefab, transform);
        iceBlockInstance.transform.localPosition = Vector3.zero;
        iceBlockInstance.transform.localRotation = Quaternion.identity;

        // Un Rigidbody2D simulé est piloté par le moteur physique indépendamment de la hiérarchie :
        // sans ça, le bloc de glace resterait figé à sa position de spawn au lieu de suivre Mylia.
        Rigidbody2D iceBlockRb = iceBlockInstance.GetComponent<Rigidbody2D>();
        if (iceBlockRb != null)
        {
            iceBlockRb.simulated = false;
        }

        MyliaIceBlockVisual iceBlockVisual = iceBlockInstance.GetComponent<MyliaIceBlockVisual>();
        EntityLight iceBlockLight = iceBlockInstance.GetComponent<EntityLight>();

        iceBlockVisual.Init();
        if (iceBlockLight != null)
        {
            iceBlockLight.SetLightIntensity(lightStartIntensity, lightStartRadius);
            iceBlockLight.SetLightColor(lightStartColor);
            iceBlockLight.TransitionLightIntensity(lightTargetIntensity, lightTargetRadius, iceBlockAppearDuration);
            iceBlockLight.TransitionLightColor(lightTargetColor, iceBlockAppearDuration);
        }
        yield return new WaitForSeconds(iceBlockAppearDuration);

        // 3. Tournoiement façon BigIceBumper pendant 8s, collider doublé (le feu ne fait rien durant cette phase)
        circleCollider.radius = baseColliderRadius * colliderRadiusMultiplier;

        Vector2 toPlayer = playerTransform != null ? (Vector2)(playerTransform.position - transform.position) : Vector2.down;
        spinDirection = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : Vector2.down;
        currentSpinSpeed = stats.speed * spinSpeedMultiplier;
        isSpinning = true;

        float spinTimer = 0f;
        while (spinTimer < spinDuration)
        {
            rb.velocity = spinDirection * currentSpinSpeed;
            UpdateSpriteDirection(spinDirection);
            spinTimer += Time.deltaTime;
            yield return null;
        }

        isSpinning = false;

        // 4. Ralentissement -> arrêt total sur 3s : seule fenêtre où le feu rend Mylia vulnérable.
        // Vitesse de base quadruplée pendant tout ce mode (façon BigIceBumper).
        speedBeforeAttackBuff = stats.speed;
        stats.speed *= spinSpeedMultiplier;

        entityEffects.canBeFire = true;

        float startSpeed = currentSpinSpeed;
        float slowTimer = 0f;
        while (slowTimer < slowDownDuration && !entityEffects.isFire)
        {
            float ratio = 1f - (slowTimer / slowDownDuration);
            rb.velocity = spinDirection * startSpeed * ratio;
            slowTimer += Time.deltaTime;
            yield return null;
        }
        rb.velocity = Vector2.zero;
        circleCollider.radius = baseColliderRadius;

        if (entityEffects.isFire)
        {
            entityEffects.canBeFire = false;
            yield return StartCoroutine(FireInterruptSequence(iceBlockVisual, iceBlockField));
            yield break;
        }

        RestoreSpeedBuff();
        StopSunDarkness();

        // 5. Retrait du bloc de glace central
        iceBlockVisual.Remove();
        entityEffects.canBeFire = false;

        // 6. Le feu fait fondre (et disparaître) tous les blocs de glace éparpillés dans l'arène
        MeltIceBlockField(iceBlockField);

        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation("Idle");

        stats.doingAttack = false;
        stats.blockPlayerAttack = false;
    }

    // Si Mylia prend feu pendant le ralentissement (seule fenêtre autorisée), elle devient vulnérable
    // et incapable d'agir pendant fireVulnerabilityDuration secondes : capté "à la IceBlock" (StopFire
    // immédiat), puis on fait disparaître prématurément tous les blocs de glace en cours.
    IEnumerator FireInterruptSequence(MyliaIceBlockVisual centerIceBlockVisual, List<GameObject> field)
    {
        entityEffects.StopFire();

        circleCollider.radius = baseColliderRadius;
        RestoreSpeedBuff();
        StopSunDarkness();

        centerIceBlockVisual.Remove();
        MeltIceBlockField(field);

        objectAnimation.StopAnimation();
        objectAnimation.PlayAnimation("Idle");

        isStunned = true;
        stats.doingAttack = false;
        stats.isVulnerable = true;
        inVulnerabilityWindow = true;
        stats.blockPlayerAttack = false;

        yield return new WaitForSeconds(fireVulnerabilityDuration);

        stats.isVulnerable = false;
        inVulnerabilityWindow = false;
        isStunned = false;

        yield return StartCoroutine(OnVulnerabilityEndRoutine());
    }

    void RestoreSpeedBuff()
    {
        if (speedBeforeAttackBuff.HasValue)
        {
            stats.speed = speedBeforeAttackBuff.Value;
            speedBeforeAttackBuff = null;
        }
    }

    void StartSunDarkness()
    {
        savedSunIntensity = (LightManager.instance != null && LightManager.instance.sunLight != null)
            ? LightManager.instance.sunLight.intensity
            : 0f;

        sunDarknessCoroutine = StartCoroutine(DarkenSunRoutine());
    }

    IEnumerator DarkenSunRoutine()
    {
        // Transition douce vers 0, puis on continue de réécrire 0 chaque frame : évite qu'un autre
        // système (cycle jour/nuit de MeteoManager) ne réapplique une valeur différente pendant
        // que Mylia est en train de prier/tournoyer.
        float elapsed = 0f;
        while (elapsed < sunTransitionDuration)
        {
            elapsed += Time.deltaTime;
            if (LightManager.instance != null)
            {
                LightManager.instance.SetSunIntensity(Mathf.Lerp(savedSunIntensity, 0f, elapsed / sunTransitionDuration));
            }
            yield return null;
        }

        while (true)
        {
            if (LightManager.instance != null)
            {
                LightManager.instance.SetSunIntensity(0f);
            }
            yield return null;
        }
    }

    void StopSunDarkness()
    {
        if (sunDarknessCoroutine != null)
        {
            StopCoroutine(sunDarknessCoroutine);
            sunDarknessCoroutine = null;
        }

        StartCoroutine(RestoreSunRoutine());
    }

    IEnumerator RestoreSunRoutine()
    {
        float startIntensity = (LightManager.instance != null && LightManager.instance.sunLight != null)
            ? LightManager.instance.sunLight.intensity
            : savedSunIntensity;

        float elapsed = 0f;
        while (elapsed < sunTransitionDuration)
        {
            elapsed += Time.deltaTime;
            if (LightManager.instance != null)
            {
                LightManager.instance.SetSunIntensity(Mathf.Lerp(startIntensity, savedSunIntensity, elapsed / sunTransitionDuration));
            }
            yield return null;
        }

        if (LightManager.instance != null)
        {
            LightManager.instance.SetSunIntensity(savedSunIntensity);
        }
    }

    IEnumerator FillIceGroundCircle()
    {
        if (MyliaIceFieldTilemap.instance == null) yield break;

        yield return StartCoroutine(MyliaIceFieldTilemap.instance.FillCircle(
            transform.position, iceGroundRadius, iceGroundFillDuration, iceFieldCells));
    }

    void MeltIceBlockField(List<GameObject> field)
    {
        if (iceGroundFillCoroutine != null)
        {
            StopCoroutine(iceGroundFillCoroutine);
            iceGroundFillCoroutine = null;
        }

        foreach (GameObject block in field)
        {
            if (block == null) continue;

            EntityEffects blockEffects = block.GetComponent<EntityEffects>();
            if (blockEffects != null)
            {
                blockEffects.SetState(force: 1, isFire: true);
            }
        }

        if (MyliaIceFieldTilemap.instance != null)
        {
            MyliaIceFieldTilemap.instance.RemoveTiles(iceFieldCells);
        }
        iceFieldCells.Clear();
    }

    List<GameObject> SpawnIceBlockField()
    {
        List<GameObject> spawned = new List<GameObject>();
        if (iceBlockObstaclePrefab == null) return spawned;

        List<Vector2> placedPositions = new List<Vector2>();
        int targetCount = Random.Range(minIceBlockObstacles, maxIceBlockObstacles + 1);
        int maxAttempts = targetCount * 30;
        int attempts = 0;

        while (spawned.Count < targetCount && attempts < maxAttempts)
        {
            attempts++;

            Vector2 candidate = (Vector2)transform.position + Random.insideUnitCircle * iceBlockFieldRadius;

            bool tooClose = false;
            foreach (Vector2 placed in placedPositions)
            {
                if (Vector2.Distance(candidate, placed) < iceBlockMinSpacing)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // Vérifie qu'il n'y a pas de collider (mur, obstacle...) à cet endroit
            Collider2D[] colliders = Physics2D.OverlapCircleAll(candidate, iceBlockObstacleCheckRadius);
            bool hasObstacle = false;
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    hasObstacle = true;
                    break;
                }
            }
            if (hasObstacle) continue;

            GameObject instance = Instantiate(iceBlockObstaclePrefab, candidate, Quaternion.identity);
            spawned.Add(instance);
            activeIceBlockObstacles.Add(instance);
            placedPositions.Add(candidate);
        }

        return spawned;
    }

    void UpdateSpriteDirection(Vector2 direction)
    {
        if (spriteRenderer == null) return;

        if (Mathf.Abs(direction.x) > 0.1f)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isSpinning || collision == null || collision.contacts.Length == 0) return;

        bool isPlayer = IsPlayerCollision(collision.gameObject);

        if (isPlayer)
        {
            Vector2 awayFromPlayer = (transform.position - collision.transform.position).normalized;
            spinDirection = awayFromPlayer != Vector2.zero
                ? awayFromPlayer
                : Vector2.Reflect(spinDirection, collision.contacts[0].normal).normalized;

            if (lifeManager != null)
            {
                lifeManager.Attack(collision.gameObject);
            }
        }
        else
        {
            ContactPoint2D contact = collision.contacts[0];
            spinDirection = Vector2.Reflect(spinDirection, contact.normal).normalized;
        }

        if (soundContainer != null)
        {
            soundContainer.PlaySound(bounceSoundName, 1);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isSpinning || collision == null || collision.contacts.Length == 0) return;

        if (IsPlayerCollision(collision.gameObject)) return;

        ContactPoint2D contact = collision.contacts[0];
        if (Vector2.Dot(spinDirection, contact.normal) < 0f)
        {
            spinDirection = Vector2.Reflect(spinDirection, contact.normal).normalized;
        }
    }

    private bool IsPlayerCollision(GameObject other)
    {
        return other.GetComponent<PlayerController>() != null ||
               (other.GetComponent<Stats>() != null &&
                other.GetComponent<Stats>().entityType == EntityType.Player);
    }

    #endregion
}
