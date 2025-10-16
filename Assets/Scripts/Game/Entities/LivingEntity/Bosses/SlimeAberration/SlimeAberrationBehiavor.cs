using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SlimeAberrationAttack { NONE, EYE_APPEAR, EYE_DISAPPEAR, EYE_LASER, BOUNCE, EXPLODE }

public class SlimeAberrationBehiavor : MonoBehaviour
{
    public GameObject eyePrefab;
    GameObject eyeInstance;

    public List<GameObject> slimesMonsterToSpawn = new List<GameObject>();
    public List<Transform> slimesSpawnPoint = new List<Transform>();

    public GameObject launchedSlimeballPrefab;

    public float timeMinNewAttack;
    public float timeMaxNewAttack;

    LifeManager lifeManager;
    NewMonsterMovement monsterMovement;
    SoundContainer soundContainer;
    Stats stats;
    Coroutine attackRoutine;
    GameObject bossBar;
    string id = "SLIME_ABERRATION";

    SlimeAberrationAttack actualAttack = SlimeAberrationAttack.NONE;

    List<GameObject> activeSlimes = new List<GameObject>();
    const int MAX_SLIMES = 4;


    private void Start()
    {
        lifeManager = GetComponent<LifeManager>();
        monsterMovement = GetComponent<NewMonsterMovement>();
        soundContainer = GetComponent<SoundContainer>();
        stats = GetComponent<Stats>();
    }

    public void InitBoss()
    {
        attackRoutine = StartCoroutine(GetSlimeAberrationAttackRoutine());
        StartCoroutine(SpawnSlimes());
        bossBar = NotificationManager.instance.ShowBossBar(id, stats.health);
    }

    void RemoveAllSlimes()
    {
        foreach (var item in activeSlimes)
        {
            if(item != null)
            {
                Destroy(item);
            }
        }
    }

    bool death = false;

    private void Update()
    {
        if(bossBar)
            bossBar.GetComponent<BossBarUI>().UpdateBossLife(GetComponent<LifeManager>().life);

        if(GetComponent<LifeManager>().life <= 0 && !death)
        {
            RemoveAllSlimes();
            GetComponent<LifeManager>().life = 1;
            GetComponent<CircleCollider2D>().enabled = false;
            StopAllCoroutines();
            attackRoutine = null;
            death = true;
            Destroy(bossBar);
            StartCoroutine(DeathRoutine());
            StartCoroutine(DeathSoundRoutine());
        }


    }

    IEnumerator DeathSoundRoutine()
    {
        while(true)
        {
            GetComponent<SoundContainer>().PlaySound("Hurt", 1);
            yield return new WaitForSeconds(.5f);
        }
    }

    IEnumerator DeathRoutine()
    {
        GetComponent<BossDeathEffect>().SpawnExplosions(4, 6);
        yield return new WaitForSeconds(3);
        if (eyeInstance)
        {
            eyeInstance.GetComponent<SlimeAberrationEyeBehiavor>().StopAllCoroutines();
            if(eyeInstance.GetComponent<SlimeAberrationEyeBehiavor>().laserInstance != null)
                Destroy(eyeInstance.GetComponent<SlimeAberrationEyeBehiavor>().laserInstance);
            Destroy(eyeInstance);
        }


        Destroy(gameObject);
        
    }

    // Sélectionne une attaque aléatoire pour le Slime Aberration
    // en fonction d'une liste de probabilités définies.
    private SlimeAberrationAttack ChooseRandomAttack()
    {
        List<(SlimeAberrationAttack, float)> attackChances = new List<(SlimeAberrationAttack, float)>();

        attackChances.Add((SlimeAberrationAttack.NONE, 5f));
        
        if(eyeInstance == null)
        {
            attackChances.Add((SlimeAberrationAttack.EYE_APPEAR, 30f));
            attackChances.Add((SlimeAberrationAttack.BOUNCE, 10f));
            attackChances.Add((SlimeAberrationAttack.EXPLODE, 10f));

        }

        if (eyeInstance != null)
        {
            attackChances.Add((SlimeAberrationAttack.EYE_LASER, 15f));
            attackChances.Add((SlimeAberrationAttack.EYE_DISAPPEAR, 20f));
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

        return SlimeAberrationAttack.NONE;
    }

    // Supprime le boss de la scène
    // Détruit l'UI associée (barre de vie) et l'objet du boss lui-même.
    public void RemoveBoss()
    {
        Destroy(bossBar);
        Destroy(gameObject);
    }

    #region Coroutines

    // Coroutine qui attend un délai aléatoire, puis choisit et lance une attaque
    // Répète indéfiniment tant que le boss est actif
    IEnumerator GetSlimeAberrationAttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(timeMinNewAttack, timeMaxNewAttack));

            if (actualAttack != SlimeAberrationAttack.NONE)
                continue;

            actualAttack = ChooseRandomAttack();

            switch (actualAttack)
            {
                case SlimeAberrationAttack.NONE:
                    break;
                case SlimeAberrationAttack.EYE_APPEAR:
                    yield return StartCoroutine(EyeAppearRoutine());
                    break;
                case SlimeAberrationAttack.EYE_DISAPPEAR:
                    yield return StartCoroutine(EyeDisappearRoutine());
                    break;
                case SlimeAberrationAttack.EYE_LASER:
                    yield return StartCoroutine(EyeLaserRoutine());
                    break;
                case SlimeAberrationAttack.BOUNCE:
                    yield return StartCoroutine(BounceRoutine());
                    break;
                case SlimeAberrationAttack.EXPLODE:
                    yield return StartCoroutine(ExplodeRoutine());
                    break;
            }

            actualAttack = SlimeAberrationAttack.NONE;
        }
    }




    #endregion

    IEnumerator ExplodeRoutine()
    {

        GetComponent<SoundContainer>().PlaySound("ReverseExplosion", 0);

        GetComponent<EntityLight>().TransitionLightIntensity(25, 25, 3);

        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForSeconds(0.25f);
        }

        CameraManager.instance.SetFilter(CameraFilter.SHYRON);
        yield return new WaitForSeconds(1);

        GetComponent<SoundContainer>().PlaySound("Explosion", 0);
        GetComponent<SoundContainer>().PlaySound("Explosion2", 0);

        CameraManager.instance.ShakeCamera(5, 5, 1);

        if (HasClearLineOfSight())
        {
            PlayerManager.instance.player.GetComponent<LifeManager>().TakeDamage(9999, gameObject, false);
        }

        CameraManager.instance.SetFilter(CameraFilter.NONE);
        GetComponent<EntityLight>().TransitionLightIntensity(.25f, .75f, 1);
        yield return new WaitForSeconds(1);
    }

    private bool HasClearLineOfSight()
    {
        Vector3 start = transform.position;
        Vector3 end = PlayerManager.instance.player.transform.position;
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance);

        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger)
                continue;

            // Ignorer les objets sur le layer "LowHeight"
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("LowHeight"))
                continue;

            // Ignorer les entités non-joueur
            Stats stats = hit.collider.GetComponent<Stats>();
            if (stats != null && stats.entityType != EntityType.Player)
                continue;

            // Ligne de vue claire si le joueur est touché
            if (hit.collider.gameObject == PlayerManager.instance.player)
                return true;

            // Obstruction détectée
            return false;
        }

        return true;
    }

    public void SpawnEye(int x = 666, int y = 666)
    {
        StartCoroutine(EyeAppearRoutine(x, y));
    }

    public void RemoveEye()
    {
        StartCoroutine(EyeDisappearRoutine());
    }

    public void ThrowLaser(int duration)
    {
        StartCoroutine(eyeInstance.GetComponent<SlimeAberrationEyeBehiavor>().SpawnAndSweepLaser(duration));
    }
    IEnumerator EyeAppearRoutine(int x = 666, int y = 666)
    {
        // Rayon max dans lequel l'œil peut apparaître
        float radius = 1f;

        // Position aléatoire dans un cercle autour du transform
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        
        Vector3 spawnPosition = (x == 666 && y == 666) ? transform.position + new Vector3(randomOffset.x, randomOffset.y, 0) : transform.position + new Vector3(x, y, 0);

        // Création de l'œil
        this.eyeInstance = Instantiate(eyePrefab, spawnPosition, Quaternion.identity);
        eyeInstance.GetComponent<SlimeAberrationEyeBehiavor>().Init(gameObject);

        CircleCollider2D circleCol = GetComponent<CircleCollider2D>();
        if (circleCol == null)
            circleCol = gameObject.AddComponent<CircleCollider2D>();

        circleCol.offset = transform.InverseTransformPoint(spawnPosition);

        SpriteRenderer eyeSprite = eyeInstance.GetComponent<SpriteRenderer>();
        if (eyeSprite != null)
        {
            float spriteRadius = Mathf.Max(eyeSprite.bounds.extents.x, eyeSprite.bounds.extents.y);
            circleCol.radius = spriteRadius;
        }
        else
        {
            circleCol.radius = .5f;
        }

        circleCol.enabled = true;

        yield return new WaitForSeconds(4);
    }



    IEnumerator EyeDisappearRoutine()
    {
        // Supprimer le collider si présent
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
            Destroy(col);

        // Supprimer l'œil
        eyeInstance.GetComponent<SlimeAberrationEyeBehiavor>().Remove();

        yield return new WaitForSeconds(.5f);
        eyeInstance = null;
    }


    IEnumerator EyeLaserRoutine()
    {
        yield return StartCoroutine(eyeInstance.GetComponent<SlimeAberrationEyeBehiavor>().SpawnAndSweepLaser());
    }

    IEnumerator BounceRoutine()
    {
        float jumpHeight = 0.5f;    // Hauteur du saut
        float duration = 1f;        // Durée totale du saut
        float elapsed = 0f;

        // On prend le transform du sprite uniquement
        Transform spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
        Vector3 startPos = spriteTransform.localPosition;

        GetComponent<SoundContainer>().PlaySound("Jump", 2);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Courbe sinusoïdale pour un mouvement fluide
            float yOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            spriteTransform.localPosition = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);

            yield return null;
        }

        // S'assure qu'on termine bien à la position de départ
        spriteTransform.localPosition = startPos;

        GetComponent<SoundContainer>().PlaySound("Land", 2);
        CameraManager.instance.ShakeCamera(3, 3, 1);

        for (int i = 0; i < Random.Range(5, 20); i++)
        {
            GameObject launchedSlimeballInstance = Instantiate(
                launchedSlimeballPrefab,
                new Vector2(transform.position.x, transform.position.y),
                Quaternion.identity
            );

            // Génération d'une target aléatoire : 
            // y = position actuelle - 10
            // x = position actuelle + random(-6, 6)
            Vector2 randomTarget = new Vector2(
                transform.position.x + Random.Range(-15f, 15f),
                transform.position.y - 10f
            );

            // Vitesse aléatoire entre 5 et 8
            float randomSpeed = Random.Range(5f, 8f);

            launchedSlimeballInstance
                .GetComponent<LaunchedSlimeball>()
                .Init(stats.strength, gameObject, randomTarget, randomSpeed);

            GetComponent<SoundContainer>().PlaySound("Throw", 3);

            yield return new WaitForSeconds(.25f);

        }

    }

    IEnumerator SpawnSlimes()
    {
        while (true)
        {
            activeSlimes.RemoveAll(slime => slime == null);

            if (activeSlimes.Count <= MAX_SLIMES)
            {
                Transform transformPoint = slimesSpawnPoint[Random.Range(0, slimesSpawnPoint.Count)];
                GameObject slimeInstance = Instantiate(slimesMonsterToSpawn[Random.Range(0, slimesMonsterToSpawn.Count)], new Vector2(transformPoint.position.x, transformPoint.position.y), Quaternion.identity);
                activeSlimes.Add(slimeInstance);
            }
            yield return new WaitForSeconds(Random.Range(1, 20));
        }
    }


}