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

    private void Start()
    {
        stats = GetComponent<Stats>();
        life = stats.health;
        soundContainer = GetComponent<SoundContainer>();

        StartCoroutine(RoutineRegeneration());

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().isVulnerable && stats.entityType == EntityType.Monster && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
        {
            Attack(collision.gameObject);
            StartCoroutine(MonsterWaitAfterAttackRoutine());
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
        // Variable temporaire pour accumuler la régénération
        float tempRegen = 0f;

        while (true)
        {
            // Ajouter la régénération par seconde à la variable temporaire
            tempRegen += PlayerManager.instance.regenRate / 60f;

            // Si la variable temporaire atteint ou dépasse 1, régénérer la vie
            if (tempRegen >= 1f)
            {
                // Incrémenter la vie actuelle
                if(life < stats.health)
                    life += 1;

                // Décrémenter la variable temporaire de 1
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
            float variation = Random.Range(0.8f, 1.2f);
            int baseDamage = Mathf.RoundToInt(stats.strength * variation * multiplier);

            // Appliquer réduction dragon skin
            if (PlayerManager.instance.dragonSkin > 0)
                baseDamage = Mathf.RoundToInt(baseDamage * (1f - PlayerManager.instance.dragonSkin));

            // Critiques
            if (Random.Range(0f, 1f) < stats.critChance)
            {
                baseDamage = Mathf.RoundToInt(baseDamage * (1 + stats.critDamage));
                isCritical = true;
            }

            // Envoyer les dégâts finaux
            target.GetComponent<LifeManager>().TakeDamage(baseDamage, gameObject, isCritical, knockbackMultiplier);
        }
    }

    // Dégâts simples, pour cinématique ?
    public void TakeDamage(int damage)
    {
        life -= damage;
        GetComponent<DamageEffect>().DamageEffects(false);
    }

    // Dégâts sans entité précise mais en considérant les bonus du joueur et la vulnérabilité
    public void TakeDamage(int damage, bool isCritical)
    {
        if (stats.isVulnerable && !stats.isDying)
        {
            if ((stats.entityType == EntityType.Player && !GetComponent<UseSpecialObject>().isShadowing) ||
                stats.entityType == EntityType.Monster ||
                stats.entityType == EntityType.Boss)
            {
                if (stats.entityType == EntityType.Monster ||
                    stats.entityType == EntityType.Boss ||
                    (stats.entityType == EntityType.Player && Random.Range(0, 100) >= PlayerManager.instance.dodgeChance))
                {
                    if (stats.entityType == EntityType.Player)
                        CameraManager.instance.ShakeCamera(1, 1, 0.5f);

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


    public void TakeDamage(int damage, GameObject attackingEntity, bool isCritical, float knockbackMultiplier = 1)
    {
        if (stats.isVulnerable && !stats.isDying)
        {
            if ((stats.entityType == EntityType.Player && !GetComponent<UseSpecialObject>().isShadowing) ||
                stats.entityType == EntityType.Monster ||
                stats.entityType == EntityType.Boss)
            {
                if (stats.entityType == EntityType.Monster ||
                    stats.entityType == EntityType.Boss ||
                    (stats.entityType == EntityType.Player && Random.Range(0, 100) >= PlayerManager.instance.dodgeChance))
                {
                    if (stats.entityType == EntityType.Player)
                        CameraManager.instance.ShakeCamera(1, 1, 0.5f);

                    // EFFECTS ATTACK
                    if (stats.entityType == EntityType.Monster &&
                        attackingEntity.GetComponent<Stats>() &&
                        attackingEntity.GetComponent<Stats>().entityType == EntityType.Player)
                    {
                        if (Random.Range(0, 100) < PlayerManager.instance.fireAttackChance)
                            GetComponent<EntityEffects>().SetState(damage, true, false, false);
                        if (Random.Range(0, 100) < PlayerManager.instance.iceAttackChance)
                            GetComponent<EntityEffects>().SetState(damage, false, true, false);
                        if (Random.Range(0, 100) < PlayerManager.instance.poisonAttackChance)
                            GetComponent<EntityEffects>().SetState(damage, false, false, true);
                    }

                    int damageTaken = Mathf.Max(damage - stats.defense, 1);

                    // VAMPIRE
                    if (attackingEntity.GetComponent<Stats>() &&
                        attackingEntity.GetComponent<Stats>().entityType == EntityType.Player &&
                        Random.Range(0, 100) < PlayerManager.instance.vampire)
                    {
                        attackingEntity.GetComponent<LifeManager>().life += Mathf.Min(
                            attackingEntity.GetComponent<Stats>().health,
                            (int)(damageTaken * 0.1f)
                        );
                    }

                    life -= damageTaken;

                    if (attackingEntity.GetComponent<Stats>())
                        KnockBack(gameObject, attackingEntity.GetComponent<Stats>().knockbackPower * knockbackMultiplier, attackingEntity);

                    if (attackingEntity.GetComponent<ProjectileBehavior>())
                        KnockBack(gameObject, attackingEntity.GetComponent<ProjectileBehavior>().knockbackPower * knockbackMultiplier, attackingEntity);

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


    // Pour les dégâts d'effets notamment
    public void TakeDamage(int damage, Color specificColor, bool sound = true, bool ignoreVulnerability = false)
    {
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
        
        if(stats.entityType == EntityType.Monster)
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

            // Comportements spécifiques
            if(GetComponent<DroxenHandBehiavor>())
            {
                Instantiate(GetComponent<DroxenHandBehiavor>().explosionEffect, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
                GetComponent<SoundContainer>().PlaySound("Explosion", 2);
            }
            Destroy(gameObject);
        }
        else if (stats.entityType == EntityType.Player && !PlayerManager.instance.cantDie)
        {
            if(!stats.isDying)
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

    public void KnockBack(GameObject target, float knockbackPower, GameObject attackingEntity)
    {
        isKnockbacking = true;

        // Calculer la direction du recul en fonction de la position de la cible et de l'entité de ce script
        Vector2 direction = (target.transform.position - attackingEntity.transform.position).normalized;

        float knockbackApplied = Mathf.Max(knockbackPower - stats.knockbackResistance, 0);
        // Appliquer le recul initial à la cible en fonction de la direction et de la puissance de recul
        target.GetComponent<Rigidbody2D>().velocity = direction * knockbackApplied * 2;

        // Démarrer la coroutine pour décélérer le recul
        StartCoroutine(DecelerateKnockback(target.GetComponent<Rigidbody2D>(), direction));
    }

    private IEnumerator DecelerateKnockback(Rigidbody2D rbTarget, Vector2 direction)
    {
        // Définir une force de frottement pour simuler la décélération
        float friction = 0.9f;

        // Tant que la vélocité de l'objet touché est significative
        while (rbTarget.velocity.magnitude > 0.1f)
        {
            // Appliquer une force de frottement pour décélérer le recul
            rbTarget.velocity *= friction;

            // Attendre un court laps de temps avant la prochaine itération
            yield return new WaitForFixedUpdate();
        }

        // Assurer que la vélocité devienne exactement zéro une fois que le recul est terminé
        rbTarget.velocity = Vector2.zero;

        isKnockbacking = false;
    }
}
