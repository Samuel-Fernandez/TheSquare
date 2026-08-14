using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class LifeManager : MonoBehaviour
{
    private Stats stats;
    public int life;
    public float invulnerabilityTime;
    public GameObject damageText;
    public GameObject criticalDamageText;
    public bool isKnockbacking = false;
    public GameObject deathParticle;
    SoundContainer soundContainer;

    private void Awake()
    {
        stats = GetComponent<Stats>();
        soundContainer = GetComponent<SoundContainer>();
    }

    private void Start()
    {
        if (stats != null)
        {
            life = stats.health;
        }

        StartCoroutine(RoutineRegeneration());
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().isVulnerable && stats.entityType == EntityType.Monster && stats.doingAttack && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
        {
            Attack(collision.gameObject);

            // L'IceBumper ne s'arrête pas après avoir attaqué, il doit rebondir et continuer immédiatement son rush
            if (GetComponent<IceBumperBehiavor>() == null)
            {
                StartCoroutine(MonsterWaitAfterAttackRoutine());
            }
        }
    }

    private void Update()
    {
        if (this.life > stats.health)
            life = stats.health;
    }

    IEnumerator MonsterWaitAfterAttackRoutine()
    {
        stats.canMove = false;
        yield return new WaitForSeconds(.25f);
        stats.canMove = true;
    }

    public void Regeneration()
    {
        StartCoroutine(RoutineRegeneration());
    }

    IEnumerator RoutineRegeneration()
    {
        // Variable temporaire pour accumuler la r�g�n�ration
        float tempRegen = 0f;

        while (true)
        {
            // Ajouter la r�g�n�ration par seconde � la variable temporaire
            tempRegen += PlayerManager.instance.regenRate / 60f;

            // Si la variable temporaire atteint ou d�passe 1, r�g�n�rer la vie
            if (tempRegen >= 1f)
            {
                // Incr�menter la vie actuelle
                if (life < stats.health)
                    life += 1;

                // D�cr�menter la variable temporaire de 1
                tempRegen -= 1f;
            }

            // Attendre une seconde avant de continuer la boucle
            yield return new WaitForSeconds(1f);
        }
    }


    public void FullHealth()
    {
        life = stats.health;
    }

    public void Heal(int health)
    {
        life = Mathf.Min(stats.health, life + health);

        GameObject damageTextTemp = null;
        damageTextTemp = Instantiate(damageText, transform.position, Quaternion.identity);

        damageTextTemp.GetComponentInChildren<TextMeshProUGUI>().text = health.ToString();
        damageTextTemp.GetComponentInChildren<TextMeshProUGUI>().color = Color.green;

        float randomX = UnityEngine.Random.Range(-1f, 1f);
        float randomY = UnityEngine.Random.Range(-1f, 1f);
        damageTextTemp.transform.position += new Vector3(randomX, randomY, 0);
        Destroy(damageTextTemp, 0.9f);
    }

    public void Attack(GameObject target, float multiplier = 1, float knockbackMultiplier = 1)
    {
        if (PlayerManager.instance.isDogingTime && stats.entityType == EntityType.Player)
        {
            multiplier = 1.5f;
            knockbackMultiplier = 1.5f;
        }

        bool isCritical = false;
        if (target.GetComponent<Stats>().isVulnerable && target.GetComponent<LifeManager>().life > 0)
        {
            int baseDamage = Mathf.RoundToInt(stats.strength * multiplier);

            // Appliquer réduction dragon skin (dégâts absorbés)
            if (target.GetComponent<Stats>().entityType == EntityType.Player && PlayerManager.instance.dragonSkin > 0)
                baseDamage = Mathf.RoundToInt(baseDamage * (1f - PlayerManager.instance.dragonSkin));

            // Critiques
            if (Random.Range(0f, 1f) < stats.critChance)
            {
                baseDamage = Mathf.RoundToInt(baseDamage * (1 + stats.critDamage));
                isCritical = true;
            }

            // Envoyer les dgts finaux
            target.GetComponent<LifeManager>().TakeDamage(baseDamage, gameObject, isCritical, knockbackMultiplier);
        }
    }

    // Dgts simples, pour cinmatique ?
    public void TakeDamage(int damage)
    {
        if (stats.entityType == EntityType.Player && PlayerManager.instance != null && PlayerManager.instance.isEventPlaying) return;
        life -= damage;
        GetComponent<DamageEffect>().DamageEffects(false);
    }

    // Dgts sans entit prcise mais en considrant les bonus du joueur et la vulnrabilit
    public void TakeDamage(int damage, bool isCritical)
    {
        if (stats.entityType == EntityType.Player && PlayerManager.instance != null && PlayerManager.instance.isEventPlaying) return;

        if (stats.isVulnerable && !stats.isDying)
        {
            if ((stats.entityType == EntityType.Player && !GetComponent<UseSpecialObject>().isShadowing) ||
                stats.entityType == EntityType.Monster ||
                stats.entityType == EntityType.Boss)
            {
                if (stats.entityType == EntityType.Monster ||
                    stats.entityType == EntityType.Boss ||
                    (stats.entityType == EntityType.Player && Random.Range(0, 100) >= PlayerManager.instance.dodgeChance * 100))
                {
                    if (stats.entityType == EntityType.Player)
                    {
                        CameraManager.instance.ShakeCamera(1, 1, 0.5f);
                    }

                    int damageTaken = Mathf.Max(damage - stats.defense, 1);
                    life -= damageTaken;

                    GetComponent<DamageEffect>().DamageEffects();

                    GameObject damageTextTemp = Instantiate(damageText, transform.position, Quaternion.identity);
                    var textComponent = damageTextTemp.GetComponentInChildren<TextMeshProUGUI>();

                    textComponent.color = isCritical ? Color.yellow : Color.red;
                    textComponent.text = damageTaken.ToString();

                    float randomX = Random.Range(-1f, 1f);
                    float randomY = Random.Range(-1f, 1f);
                    damageTextTemp.transform.position += new Vector3(randomX, randomY, 0);
                    Destroy(damageTextTemp, 0.9f);

                    if (life <= 0)
                        Die();
                    else
                        soundContainer.PlaySound("Hurt", 1);
                }
            }
            else if (stats.entityType == EntityType.Player && GetComponent<UseSpecialObject>().isShadowing)
            {
                GetComponent<UseSpecialObject>().UsingShadowMedal();
                GetComponent<DamageEffect>().DamageEffects();
            }
        }
    }


    public void TakeDamage(int damage, GameObject attackingEntity, bool isCritical, float knockbackMultiplier = 1, bool ignoreDefense = false)
    {
        if (stats.entityType == EntityType.Player && PlayerManager.instance != null && PlayerManager.instance.isEventPlaying) return;

        if (stats.isVulnerable && !stats.isDying)
        {
            if ((stats.entityType == EntityType.Player && !GetComponent<UseSpecialObject>().isShadowing) ||
                stats.entityType == EntityType.Monster ||
                stats.entityType == EntityType.Boss)
            {
                if (stats.entityType == EntityType.Monster ||
                    stats.entityType == EntityType.Boss ||
                    (stats.entityType == EntityType.Player && Random.Range(0, 100) >= PlayerManager.instance.dodgeChance * 100))
                {
                    if (stats.entityType == EntityType.Player)
                    {
                        CameraManager.instance.ShakeCamera(1, 1, 0.5f);
                    }

                    // EFFECTS ATTACK
                    if (stats.entityType == EntityType.Monster &&
                        attackingEntity.GetComponent<Stats>() &&
                        attackingEntity.GetComponent<Stats>().entityType == EntityType.Player)
                    {
                        if (Random.Range(0, 100) < PlayerManager.instance.fireAttackChance * 100)
                            GetComponent<EntityEffects>().SetState(damage, true, false, false);
                        if (Random.Range(0, 100) < PlayerManager.instance.iceAttackChance * 100)
                            GetComponent<EntityEffects>().SetState(damage, false, true, false);
                        if (Random.Range(0, 100) < PlayerManager.instance.poisonAttackChance * 100)
                            GetComponent<EntityEffects>().SetState(damage, false, false, true);
                    }
                    else if (stats.entityType == EntityType.Player &&
                        attackingEntity.GetComponent<Stats>() &&
                        (attackingEntity.GetComponent<Stats>().entityType == EntityType.Monster || attackingEntity.GetComponent<Stats>().entityType == EntityType.Boss))
                    {
                        EntityEffects attackerEffects = attackingEntity.GetComponent<EntityEffects>();
                        if (attackerEffects != null)
                        {
                            if (Random.Range(0, 100) < attackerEffects.fireChance * 100)
                                GetComponent<EntityEffects>().SetState(damage, true, false, false);
                            if (Random.Range(0, 100) < attackerEffects.freezeChance * 100)
                                GetComponent<EntityEffects>().SetState(damage, false, true, false);
                            if (Random.Range(0, 100) < attackerEffects.poisonChance * 100)
                                GetComponent<EntityEffects>().SetState(damage, false, false, true);
                            if (Random.Range(0, 100) < attackerEffects.slimeChance * 100)
                                GetComponent<EntityEffects>().SetState(damage, false, false, false, true);
                        }
                    }

                    float stanceMultiplier = 1f;

                    if ((stats.entityType == EntityType.Monster || stats.entityType == EntityType.Boss) && 
                        attackingEntity.GetComponent<Stats>() && 
                        attackingEntity.GetComponent<Stats>().entityType == EntityType.Player)
                    {
                        if (StanceAndRunicManager.instance != null)
                        {
                            var activeStance = StanceAndRunicManager.instance.stancesList.Find(s => s.isEquipped);
                            if (activeStance != null && activeStance.stance != null)
                            {
                                switch (activeStance.stance.stanceType)
                                {
                                    case StanceType.Slashing:
                                        if (stats.monsterType == MonsterType.Amorphous) stanceMultiplier = 1.5f;
                                        else if (stats.monsterType == MonsterType.Rigid) stanceMultiplier = 0.5f;
                                        break;
                                    case StanceType.Blunt:
                                        if (stats.monsterType == MonsterType.Rigid) stanceMultiplier = 1.5f;
                                        else if (stats.monsterType == MonsterType.Amorphous) stanceMultiplier = 0.5f;
                                        break;
                                    case StanceType.Piercing:
                                        if (stats.monsterType == MonsterType.Fleshy) stanceMultiplier = 1.5f;
                                        else if (stats.monsterType == MonsterType.Ethereal) stanceMultiplier = 0.5f;
                                        break;
                                    case StanceType.Spiritual:
                                        if (stats.monsterType == MonsterType.Ethereal) stanceMultiplier = 1.5f;
                                        else if (stats.monsterType == MonsterType.Fleshy) stanceMultiplier = 0.5f;
                                        break;
                                    case StanceType.AntiSquare:
                                        if (stats.monsterType == MonsterType.Anomaly) stanceMultiplier = 1.5f;
                                        else stanceMultiplier = 0.5f;
                                        break;
                                    case StanceType.Neutral:
                                    default:
                                        break;
                                }
                                // Mimetisme
                                if (StanceAndRunicManager.instance.GetActiveRuneType() == RuneType.Mimetisme)
                                {
                                    if (stanceMultiplier > 1.1f) stanceMultiplier = 3.0f;
                                    else if (stanceMultiplier < 0.9f) stanceMultiplier = 0.25f;
                                }

                                if (stanceMultiplier > 1.1f)
                                {
                                    var sc = attackingEntity.GetComponent<SoundContainer>();
                                    if (sc != null) sc.PlaySound("StanceBonus", 1);
                                    
                                    if (StanceAndRunicManager.instance.stanceLightPrefab != null)
                                    {
                                        GameObject lightObj = Instantiate(StanceAndRunicManager.instance.stanceLightPrefab, transform.position, Quaternion.identity, transform);
                                        var light2D = lightObj.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
                                        if (light2D != null)
                                        {
                                            light2D.color = Color.yellow;
                                            StartCoroutine(FadeAndDestroyLightCoroutine(lightObj, light2D, 0.5f));
                                        }
                                        else
                                        {
                                            Destroy(lightObj, 0.5f);
                                        }
                                    }
                                }
                                else if (stanceMultiplier < 0.9f)
                                {
                                    var sc = attackingEntity.GetComponent<SoundContainer>();
                                    if (sc != null) sc.PlaySound("StanceMalus", 1);
                                    // Pas de lumière pour le malus comme demandé
                                }
                            }
                        }
                    }

                    int actualDamage = Mathf.RoundToInt(damage * stanceMultiplier);

                    // -- RUNES (Dégâts infligés par le joueur) --
                    if (attackingEntity != null && attackingEntity.GetComponent<Stats>() && attackingEntity.GetComponent<Stats>().entityType == EntityType.Player && StanceAndRunicManager.instance != null)
                    {
                        float runicDamageMult = StanceAndRunicManager.instance.GetRunicDamageMultiplier(attackingEntity.GetComponent<Stats>(), stats);
                        actualDamage = Mathf.RoundToInt(actualDamage * runicDamageMult);
                    }
                    
                    // -- RUNES (Dégâts reçus par le joueur) --
                    if (stats.entityType == EntityType.Player && StanceAndRunicManager.instance != null)
                    {
                        float runicTakenMult = StanceAndRunicManager.instance.GetRunicDamageTakenMultiplier(stats);
                        actualDamage = Mathf.RoundToInt(actualDamage * runicTakenMult);
                    }

                    int damageTaken = ignoreDefense ? Mathf.Max(actualDamage, 1) : Mathf.Max(actualDamage - stats.defense, 1);

                    // -- RUNES (Coup Critique) --
                    if (isCritical && attackingEntity != null && attackingEntity.GetComponent<Stats>() && attackingEntity.GetComponent<Stats>().entityType == EntityType.Player && StanceAndRunicManager.instance != null)
                    {
                        StanceAndRunicManager.instance.OnCriticalHitDealt(attackingEntity.GetComponent<Stats>(), damageTaken);
                    }

                    // VAMPIRE
                    if (attackingEntity.GetComponent<Stats>() &&
                        attackingEntity.GetComponent<Stats>().entityType == EntityType.Player &&
                        Random.Range(0, 100) < PlayerManager.instance.vampire * 100)
                    {
                        int healAmount = (int)(damageTaken * 0.1f);
                        if (healAmount > 0)
                        {
                            attackingEntity.GetComponent<LifeManager>().Heal(healAmount);
                        }
                    }

                    life -= damageTaken;

                    // -- GESTION DU REBOND SPECIFIQUE DE L'ICEBUMPER --
                    IceBumperBehiavor iceBumper = GetComponent<IceBumperBehiavor>();
                    if (iceBumper != null)
                    {
                        iceBumper.OnDamageTaken();
                    }
                    else
                    {
                        if (attackingEntity.GetComponent<Stats>())
                            KnockBack(gameObject, attackingEntity.GetComponent<Stats>().knockbackPower * knockbackMultiplier, attackingEntity);

                        if (attackingEntity.GetComponent<ProjectileBehavior>())
                            KnockBack(gameObject, attackingEntity.GetComponent<ProjectileBehavior>().knockbackPower * knockbackMultiplier, attackingEntity);
                    }

                    if (GetComponent<DamageEffect>())
                        GetComponent<DamageEffect>().DamageEffects(true);

                    GameObject damageTextTemp = Instantiate(damageText, transform.position, Quaternion.identity);
                    var textComponent = damageTextTemp.GetComponentInChildren<TextMeshProUGUI>();

                    textComponent.color = isCritical ? Color.yellow : Color.red;
                    textComponent.text = damageTaken.ToString();

                    float randomX = Random.Range(-1f, 1f);
                    float randomY = Random.Range(-1f, 1f);
                    damageTextTemp.transform.position += new Vector3(randomX, randomY, 0);
                    Destroy(damageTextTemp, 0.9f);

                    if (life <= 0)
                        Die();
                    else
                        soundContainer.PlaySound("Hurt", 1);
                }
            }
            else if (stats.entityType == EntityType.Player && GetComponent<UseSpecialObject>().isShadowing)
            {
                GetComponent<UseSpecialObject>().UsingShadowMedal();
                GetComponent<DamageEffect>().DamageEffects();
            }
        }
    }


    // Pour les dgts d'effets notamment
    public void TakeDamage(int damage, Color specificColor, bool sound = true, bool ignoreVulnerability = false)
    {
        if (stats.entityType == EntityType.Player && PlayerManager.instance != null && PlayerManager.instance.isEventPlaying) return;

        if (stats.isVulnerable || ignoreVulnerability)
        {
            if (stats.entityType != EntityType.Player || (stats.entityType == EntityType.Player && !stats.isDying))
            {
                int damageTaken = damage;

                life -= damageTaken;

                GetComponent<DamageEffect>().DamageEffects(false, specificColor);

                GameObject damageTextTemp = Instantiate(criticalDamageText, transform.position, Quaternion.identity);
                damageTextTemp.GetComponentInChildren<TextMeshProUGUI>().color = specificColor;

                damageTextTemp.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = damageTaken.ToString();
                float randomX = UnityEngine.Random.Range(-1f, 1f);
                float randomY = UnityEngine.Random.Range(-1f, 1f);
                damageTextTemp.transform.position += new Vector3(randomX, randomY, 0);
                Destroy(damageTextTemp, 0.9f);

                if (life <= 0)
                    Die();
                else if (sound)
                    soundContainer.PlaySound("Hurt", 1);
            }
        }

    }


    public void Die()
    {
        GameObject deathParticleInstance = Instantiate(deathParticle, transform.position, Quaternion.identity);

        if (stats.entityType == EntityType.Monster)
        {
            StatsManager.instance.MonsterKilled(this.gameObject.name);

            GetComponent<ObjectParticles>().StopAllCoroutines();
            stats.isDying = true;
            soundContainer.PlaySound("Death", 1);
            if (GetComponent<LootChance>())
                GetComponent<LootChance>().Drop();


            if (Random.Range(0, 100) <= PlayerManager.instance.doubleSquareCoinsChances * 100)
            {
                NotificationManager.instance.ShowSpecialPopUpSquareCoins(
                PlayerManager.instance.player.GetComponent<Stats>().money.ToString(),
                (PlayerManager.instance.player.GetComponent<Stats>().money + GetComponent<Stats>().money * 2).ToString());
                PlayerManager.instance.player.GetComponent<Stats>().money += GetComponent<Stats>().money * 2;
            }
            else
            {
                NotificationManager.instance.ShowSpecialPopUpSquareCoins(
                PlayerManager.instance.player.GetComponent<Stats>().money.ToString(),
                (PlayerManager.instance.player.GetComponent<Stats>().money + GetComponent<Stats>().money).ToString());
                PlayerManager.instance.player.GetComponent<Stats>().money += GetComponent<Stats>().money;
            }

            GetComponent<MonsterDeath>().OnMonsterDeath();

            if (StanceAndRunicManager.instance != null && PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                StanceAndRunicManager.instance.OnMonsterKilled(this.gameObject, PlayerManager.instance.player.GetComponent<Stats>());
            }

            // Comportements spcifiques
            if (GetComponent<DroxenHandBehiavor>())
            {
                Instantiate(GetComponent<DroxenHandBehiavor>().explosionEffect, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
                GetComponent<SoundContainer>().PlaySound("Explosion", 2);
            }
            Destroy(gameObject);
        }
        else if (stats.entityType == EntityType.Player && !PlayerManager.instance.cantDie)
        {
            if (!stats.isDying)
                StartCoroutine(PlayerDie());
        }
        else if (stats.entityType == EntityType.Player && PlayerManager.instance.cantDie)
        {

            this.Heal(Mathf.Abs(life) + 1);
        }


    }

    IEnumerator PlayerDie()
    {
        if (GetComponent<EntityEffects>().isFire)
            GetComponent<EntityEffects>().ResetEffect();
        GetComponent<EntityEffects>().stopEffects = true;

        GameoverManager.instance.deathPosition = transform.position;

        SoundManager.instance.StopMusic(.5f);
        stats.canMove = false;
        CameraManager.instance.ZoomCamera(2, 2);
        UIAnimator.instance.ActivateObjectWithTransition(GameoverManager.instance.vignetteUI, 2f);
        soundContainer.PlaySound("DeathSound", 0);

        yield return new WaitForSecondsRealtime(2);
        soundContainer.PlaySound("Death", 0);
        stats.isDying = true;

        yield return new WaitForSecondsRealtime(1f);
        CameraManager.instance.ResetCameraZoom();
        GameoverManager.instance.ActiveGameOver();
        UIAnimator.instance.DeactivateObjectWithTransition(GameoverManager.instance.vignetteUI, 1f);
        SoundManager.instance.PlayMusic("Death");
        PlayerManager.instance.TogglePlayer(0);
        GetComponent<EntityEffects>().stopEffects = false;
        stats.canMove = true;
        stats.isDying = false;
    }

    private Coroutine knockbackCoroutine;

    public void KnockBack(GameObject target, float knockbackPower, GameObject attackingEntity)
    {
        isKnockbacking = true;

        // Calculer la direction du recul en fonction de la position de la cible et de l'entit� de ce script
        Vector2 diff = target.transform.position - attackingEntity.transform.position;
        Vector2 direction = diff.normalized;
        if (diff.sqrMagnitude < 0.001f)
        {
            direction = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            if (direction == Vector2.zero) direction = Vector2.up;
        }

        float knockbackApplied = Mathf.Max(knockbackPower - stats.knockbackResistance, 0);
        // Appliquer le recul initial � la cible en fonction de la direction et de la puissance de recul
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null) targetRb.velocity = direction * knockbackApplied * 2;

        // D�marrer la coroutine pour d�c�l�rer le recul
        if (targetRb != null)
        {
            if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = StartCoroutine(DecelerateKnockback(targetRb, direction));
        }
    }

    private IEnumerator DecelerateKnockback(Rigidbody2D rbTarget, Vector2 direction)
    {
        // D�finir une force de frottement pour simuler la d�c�l�ration
        float friction = 0.9f;

        // Tant que la v�locit� de l'objet touch� est significative
        while (rbTarget != null && rbTarget.velocity.magnitude > 0.1f)
        {
            // Appliquer une force de frottement pour d�c�l�rer le recul
            rbTarget.velocity *= friction;

            // Attendre un court laps de temps avant la prochaine it�ration
            yield return new WaitForFixedUpdate();
        }

        // Assurer que la v�locit� devienne exactement z�ro une fois que le recul est termin�
        if (rbTarget != null) rbTarget.velocity = Vector2.zero;

        isKnockbacking = false;
        knockbackCoroutine = null;
    }

    private IEnumerator FadeAndDestroyLightCoroutine(GameObject lightObj, UnityEngine.Rendering.Universal.Light2D light2D, float duration)
    {
        float startIntensity = light2D.intensity;
        float startRadius = light2D.pointLightOuterRadius;
        float time = 0f;

        while (time < duration)
        {
            if (light2D == null) yield break;
            time += Time.deltaTime;
            light2D.intensity = Mathf.Lerp(startIntensity, 0f, time / duration);
            light2D.pointLightOuterRadius = Mathf.Lerp(startRadius, 0f, time / duration);
            yield return null;
        }

        if (lightObj != null)
        {
            Destroy(lightObj);
        }
    }
}
