using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DroxenPhase2Attack { NONE, BIG_PUNCH, MAGICAL_ROCKS }

public class DroxenPhase2Behiavor : MonoBehaviour
{
    [Header("Boss Settings")]
    public float timeMinNewAttack = 1f;
    public float timeMaxNewAttack = 3f;

    [Header("Boss composition")]
    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;
    public GameObject fireballPrefab;
    public GameObject fallingRockPrefab;
    public GameObject magicFireShield;

    List<GameObject> handsInstance = new List<GameObject>();
    Coroutine leftHandRespawnCoroutine;
    Coroutine rightHandRespawnCoroutine;
    Coroutine vulnerabilityCoroutine;

    LifeManager lifeManager;
    NewMonsterMovement monsterMovement;
    SoundContainer soundContainer;
    Stats stats;
    Coroutine attackRoutine;
    GameObject bossBar;
    EntityLight entityLight;

    DroxenPhase2Attack actualAttack = DroxenPhase2Attack.NONE;
    bool death = false;
    bool isVulnerable = false;
    string id = "DROXEN_PHASE2";

    [Header("Attack Probabilities")]
    public float attack1Chance = 50f;
    public float attack2Chance = 50f;

    private void Start()
    {
        lifeManager = GetComponent<LifeManager>();
        monsterMovement = GetComponent<NewMonsterMovement>();
        soundContainer = GetComponent<SoundContainer>();
        stats = GetComponent<Stats>();
        entityLight = GetComponent<EntityLight>();


        InitBoss();
    }

    private void Update()
    {
        if (bossBar)
            bossBar.GetComponent<BossBarUI>().UpdateBossLife(lifeManager.life);

        if (lifeManager.life <= 0 && !death)
        {
            death = true;
            isVulnerable = false;
            StopAllCoroutines();
            attackRoutine = null;

            // D�truire les mains restantes
            foreach (var hand in handsInstance)
            {
                if (hand != null)
                    Destroy(hand);
            }
            handsInstance.Clear();

            // D�sactiver le shield et le collider
            if (magicFireShield != null)
                magicFireShield.SetActive(false);
            GetComponent<CircleCollider2D>().enabled = false;

            Destroy(bossBar);
            StartCoroutine(DeathRoutine());
            StartCoroutine(DeathSoundRoutine());
        }

        // V�rification des mains d�truites
        CheckHandsStatus();
    }

    void CheckHandsStatus()
    {
        if (death) return;

        bool leftHandDestroyed = handsInstance[0] == null;
        bool rightHandDestroyed = handsInstance[1] == null;

        // Si les deux mains sont d�truites
        if (leftHandDestroyed && rightHandDestroyed)
        {
            // Annuler les coroutines individuelles de respawn
            if (leftHandRespawnCoroutine != null)
            {
                StopCoroutine(leftHandRespawnCoroutine);
                leftHandRespawnCoroutine = null;
            }
            if (rightHandRespawnCoroutine != null)
            {
                StopCoroutine(rightHandRespawnCoroutine);
                rightHandRespawnCoroutine = null;
            }

            // Lancer la phase de vuln�rabilit� si pas d�j� en cours
            if (vulnerabilityCoroutine == null)
            {
                vulnerabilityCoroutine = StartCoroutine(VulnerabilityPhase());
            }
        }
        else
        {
            // G�rer le respawn individuel des mains
            if (leftHandDestroyed && leftHandRespawnCoroutine == null && vulnerabilityCoroutine == null)
            {
                leftHandRespawnCoroutine = StartCoroutine(RespawnHandRoutine(0, 20f));
            }

            if (rightHandDestroyed && rightHandRespawnCoroutine == null && vulnerabilityCoroutine == null)
            {
                rightHandRespawnCoroutine = StartCoroutine(RespawnHandRoutine(1, 20f));
            }
        }
    }

    IEnumerator VulnerabilityPhase()
    {
        GetComponent<SoundContainer>().PlaySound("Hurt", 0);
        entityLight.TransitionLightIntensity(.5f, .5f, .5f);
        // ACTIVATION DE LA VULN�RABILIT�
        isVulnerable = true;

        // D�sactiver le bouclier et activer le collider
        magicFireShield.SetActive(false);
        GetComponent<CircleCollider2D>().enabled = true;

        // Attendre 8 secondes
        yield return new WaitForSeconds(5f);

        entityLight.TransitionLightIntensity(3, 3, .5f);
        GetComponent<SoundContainer>().PlaySound("MagicShield", 0);

        lifeManager.KnockBack(PlayerManager.instance.player, 15, gameObject);

        // FIN DE LA VULN�RABILIT�
        isVulnerable = false;

        // R�activer le bouclier et d�sactiver le collider
        magicFireShield.SetActive(true);
        GetComponent<CircleCollider2D>().enabled = false;

        // Faire r�appara�tre les deux mains
        RespawnHand(0);
        RespawnHand(1);

        vulnerabilityCoroutine = null;
    }

    IEnumerator RespawnHandRoutine(int handIndex, float delay)
    {
        yield return new WaitForSeconds(delay);

        RespawnHand(handIndex);

        // R�initialiser la r�f�rence de la coroutine
        if (handIndex == 0)
            leftHandRespawnCoroutine = null;
        else
            rightHandRespawnCoroutine = null;
    }

    void RespawnHand(int handIndex)
    {
        if (death) return;

        Vector2 spawnPos;
        GameObject prefab;

        if (handIndex == 0) // Main gauche
        {
            spawnPos = new Vector2(transform.position.x - 4, transform.position.y - 1);
            prefab = leftHandPrefab;
        }
        else // Main droite
        {
            spawnPos = new Vector2(transform.position.x + 4, transform.position.y - 1);
            prefab = rightHandPrefab;
        }

        GameObject newHand = Instantiate(prefab, spawnPos, Quaternion.identity);
        handsInstance[handIndex] = newHand;
    }

    void InitBoss()
    {
        attackRoutine = StartCoroutine(AttackRoutine());
        bossBar = NotificationManager.instance.ShowBossBar(id, stats.health);

        GameObject leftHandInstance = Instantiate(leftHandPrefab, new Vector2(transform.position.x - 4, transform.position.y - 1), Quaternion.identity);
        GameObject rightHandInstance = Instantiate(rightHandPrefab, new Vector2(transform.position.x + 4, transform.position.y - 1), Quaternion.identity);
        handsInstance.Add(leftHandInstance);
        handsInstance.Add(rightHandInstance);
        magicFireShield.SetActive(true);
        GetComponent<CircleCollider2D>().enabled = false;
        entityLight.SetLightColor(Color.yellow);
        entityLight.TransitionLightIntensity(3, 3, 3);

        StartCoroutine(LaunchFireballRoutine());
    }

    IEnumerator LaunchFireballRoutine()
    {
        while (!death)
        {
            // Ne pas lancer de boules de feu si le boss est vuln�rable
            if (isVulnerable)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // Calcul du ratio de vie (entre 0 et 1)
            float lifeRatio = Mathf.Clamp01((float)lifeManager.life / stats.health);

            // Moins il a de vie, plus vite il tire (entre 0.5 et 5 secondes)
            float delay = Mathf.Max(5f * lifeRatio, 2f);

            yield return new WaitForSeconds(delay);

            // V�rifier � nouveau la vuln�rabilit� avant de tirer
            if (isVulnerable || death)
                continue;

            // Instanciation de la boule de feu
            GameObject fireballInstance = Instantiate(
                fireballPrefab,
                new Vector2(transform.position.x, transform.position.y - 1.5f),
                Quaternion.identity
            );

            // Initialisation de la boule de feu
            fireballInstance.GetComponent<FireballBehiavor>().Init(
                gameObject,
                PlayerManager.instance.player,
                5.5f,
                stats.strength / 2
            );
        }
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

            // Ne pas attaquer si vuln�rable ou mort
            if (isVulnerable || death)
                continue;

            if (actualAttack != DroxenPhase2Attack.NONE)
                continue;

            actualAttack = ChooseRandomAttack();

            switch (actualAttack)
            {
                case DroxenPhase2Attack.NONE:
                    break;
                case DroxenPhase2Attack.BIG_PUNCH:
                    yield return StartCoroutine(BigPunchRoutine());
                    break;
                case DroxenPhase2Attack.MAGICAL_ROCKS:
                    yield return StartCoroutine(MagicalRocksRoutine());
                    break;
            }

            actualAttack = DroxenPhase2Attack.NONE;
        }
    }

    DroxenPhase2Attack ChooseRandomAttack()
    {
        List<(DroxenPhase2Attack, float)> attackChances = new List<(DroxenPhase2Attack, float)>
        {
            (DroxenPhase2Attack.BIG_PUNCH, attack1Chance),
            (DroxenPhase2Attack.MAGICAL_ROCKS, attack2Chance),
        };

        float total = attack1Chance + attack2Chance;
        float randomValue = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var atk in attackChances)
        {
            cumulative += atk.Item2;
            if (randomValue <= cumulative)
                return atk.Item1;
        }

        return DroxenPhase2Attack.NONE;
    }

    #endregion

    #region Attack Coroutines

    IEnumerator BigPunchRoutine()
    {
        foreach (var item in handsInstance)
        {
            if (item)
                StartCoroutine(item.GetComponent<DroxenHandBehiavor>().PunchRoutine());
        }

        yield return new WaitForSeconds(3f);
    }

    IEnumerator MagicalRocksRoutine()
    {
        // La main effectue son smash
        foreach (var item in handsInstance)
        {
            if (item)
            {
                StartCoroutine(item.GetComponent<DroxenHandBehiavor>().SmashGround());
            }
        }

        yield return new WaitForSeconds(2f); // Attente avant la pluie de rochers

        int rockCount = 15;

        for (int i = 0; i < rockCount; i++)
        {
            // Position al�atoire autour du joueur dans un rayon de 5 unit�s
            Vector2 randomOffset = Random.insideUnitCircle * 4f;
            Vector2 spawnPos = (Vector2)PlayerManager.instance.player.transform.position + randomOffset;

            GameObject rock = Instantiate(fallingRockPrefab, spawnPos, Quaternion.identity);
            rock.GetComponent<FallingRockBehiavor>().damage = GetComponent<Stats>().strength;

            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(1f);
    }


    #endregion
}