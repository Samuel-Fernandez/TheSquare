using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DroxenPhase1Attack { NONE, FIREBALL, FIRE_FIST }

public class DroxenPhase1Behiavor : MonoBehaviour
{
    [Header("Boss Settings")]
    public float timeMinNewAttack = 1f;
    public float timeMaxNewAttack = 3f;

    [Header("Specific Gameobjects")]
    public GameObject fireball;

    LifeManager lifeManager;
    NewMonsterMovement monsterMovement;
    SoundContainer soundContainer;
    Stats stats;
    Coroutine attackRoutine;
    GameObject bossBar;

    DroxenPhase1Attack actualAttack = DroxenPhase1Attack.NONE;

    bool death = false;
    string id = "DROXEN_PHASE1";

    [Header("Attack Probabilities")]
    public float fireballChance = 50f;
    public float fireFistChance = 50f;

    private void Start()
    {
        lifeManager = GetComponent<LifeManager>();
        monsterMovement = GetComponent<NewMonsterMovement>();
        soundContainer = GetComponent<SoundContainer>();
        stats = GetComponent<Stats>();

        InitBoss();
    }

    private void Update()
    {
        if (bossBar)
            bossBar.GetComponent<BossBarUI>().UpdateBossLife(lifeManager.life);

        if (lifeManager.life <= 0 && !death)
        {
            death = true;
            StopAllCoroutines();
            attackRoutine = null;
            Destroy(bossBar);
            StartCoroutine(DeathRoutine());
            StartCoroutine(DeathSoundRoutine());
        }

        if(monsterMovement.GetDistanceToTarget() <= 3 && actualAttack != DroxenPhase1Attack.FIRE_FIST)
            monsterMovement.SetReversed(true);
        else
            monsterMovement.SetReversed(false);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (actualAttack == DroxenPhase1Attack.FIRE_FIST)
        {
            if(collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
            {
                collision.gameObject.GetComponent<LifeManager>().TakeDamage(stats.strength, gameObject, false, 1);
                collision.gameObject.GetComponent<EntityEffects>().SetState(Mathf.Min(stats.strength / 6, 1), true);
            }
        }
    }

    void InitBoss()
    {
        attackRoutine = StartCoroutine(AttackRoutine());
        bossBar = NotificationManager.instance.ShowBossBar(id, stats.health);
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

    #region Attack Logic

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(timeMinNewAttack, timeMaxNewAttack));

            if (actualAttack != DroxenPhase1Attack.NONE)
                continue;

            actualAttack = ChooseRandomAttack();

            switch (actualAttack)
            {
                case DroxenPhase1Attack.NONE:
                    break;
                case DroxenPhase1Attack.FIREBALL:
                    yield return StartCoroutine(FireballRoutine());
                    break;
                case DroxenPhase1Attack.FIRE_FIST:
                    yield return StartCoroutine(FireFistRoutine());
                    break;
            }

            actualAttack = DroxenPhase1Attack.NONE;
        }
    }

    DroxenPhase1Attack ChooseRandomAttack()
    {
        // Probabilités fixes à 50/50
        List<(DroxenPhase1Attack, float)> attackChances = new List<(DroxenPhase1Attack, float)>
        {
            (DroxenPhase1Attack.FIREBALL, fireballChance),
            (DroxenPhase1Attack.FIRE_FIST, fireFistChance)
        };

        float total = fireballChance + fireFistChance;
        float randomValue = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var atk in attackChances)
        {
            cumulative += atk.Item2;
            if (randomValue <= cumulative)
                return atk.Item1;
        }

        return DroxenPhase1Attack.NONE;
    }

    #endregion

    #region Attack Coroutines

    IEnumerator FireFistRoutine()
    {
        monsterMovement.EnableAnimations = false;
        monsterMovement.SetSpeedMultiplier(0);
        GetComponent<EntityLight>().TransitionLightIntensity(5, 5, 1);
        GetComponent<EntityLight>().TransitionLightColor(Color.red, 1);
        GetComponent<SoundContainer>().PlaySound("Fireball", 0);
        yield return GetComponent<ObjectAnimation>().PlayAnimationCoroutine("FirefistActivation", true);

        monsterMovement.SetSpeedMultiplier(1.5f);
        stats.doingAttack = true;

        GetComponent<ObjectAnimation>().PlayAnimation("FirefistAttack");
        float iterations = Random.Range(10, 30);
        
        for (int i = 0; i < iterations; i++)
        {
            GetComponent<SoundContainer>().PlaySound("FirePunch", 3);
            GetComponent<SoundContainer>().PlaySound("Attack", 3);
            yield return new WaitForSeconds(.125f);
        }

        // Fin
        GetComponent<EntityLight>().TransitionLightIntensity(.25f, 1, .5f);
        GetComponent<EntityLight>().TransitionLightColor(Color.white, .5f);
        monsterMovement.EnableAnimations = true;

        yield return new WaitForSeconds(.5f);
        monsterMovement.SetSpeedMultiplier(1f);
        stats.doingAttack = false;
    }

    IEnumerator FireballRoutine()
    {
        monsterMovement.EnableAnimations = false;
        monsterMovement.SetSpeedMultiplier(0);
        GetComponent<ObjectAnimation>().PlayAnimation("Fireball", true);
        GetComponent<LifeManager>().KnockBack(PlayerManager.instance.player, 15, gameObject);

        GetComponent<EntityLight>().TransitionLightIntensity(5, 5, 1);
        GetComponent<EntityLight>().TransitionLightColor(Color.red, 1);
        GetComponent<SoundContainer>().PlaySound("Fireball", 0);
        yield return new WaitForSeconds(1f);

        int nbFireball = Random.Range(5, 20);
        
        for (int i = 0; i < nbFireball; i++)
        {
            GameObject fireballInstance = Instantiate(fireball, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
            fireballInstance.GetComponent<FireballBehiavor>().Init(gameObject, PlayerManager.instance.player, 5.5f, GetComponent<Stats>().strength / 2);
            yield return new WaitForSeconds(.25f);
        }

        GetComponent<EntityLight>().TransitionLightIntensity(.25f, 1, 1);
        GetComponent<EntityLight>().TransitionLightColor(Color.white, 1);
        yield return new WaitForSeconds(1f);
        monsterMovement.EnableAnimations = true;
        monsterMovement.SetSpeedMultiplier(1);

    }

    #endregion
}
