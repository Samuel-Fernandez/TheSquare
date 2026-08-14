using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UseSpecialObject : MonoBehaviour
{
    Stats stats;
    public GameObject arrowPrefab;
    private Coroutine checkBowCoroutine;

    public bool isHammering;
    public GameObject hammerImpact;

    public bool isPickaxing;
    public int pickaxePower;

    // Animation of using lantern
    public GameObject lanternLight;
    public bool isLightning;
    public bool lanternIsOn;
    public GameObject lanternActivationEffectPrefab;
    // Corrige le décalage vertical du point de détection/effet quand le joueur regarde sur le côté
    // (le pivot du joueur n'est pas à la même hauteur que pour les directions haut/bas)
    public float sideDetectionVerticalOffset = -0.15f;

    // Huile de la lanterne (lanterne magique : se régénère toute seule)
    [Header("Huile de la lanterne")]
    public GameObject lanternOilGaugePrefab;
    public float oilGaugeHeightOffset = 1.1f;
    public float maxOil = 100f;
    public float currentOil;
    public float igniteOilCost = 20f;
    public float minOilToIgnite = 20f;
    public float oilDrainPerSecond = 10f;
    public float oilRegenPerSecond = 10f;
    public float refuseCooldown = 0.5f;
    private GameObject lanternOilGaugeInstance;

    // Shadow Medal
    public bool isShadowing;
    public GameObject playerSleepPrefab;
    GameObject playerSleepInstance;

    // Shield
    public bool isShielding;
    public GameObject shieldParticle;

    private void Start()
    {
        stats = GetComponent<Stats>();

        currentOil = maxOil;
    }

    private void Update()
    {
        if (lanternIsOn)
        {
            currentOil = Mathf.Max(0f, currentOil - oilDrainPerSecond * Time.deltaTime);

            if (currentOil <= 0f)
                ForceExtinguishLantern();
        }
        else if (currentOil < maxOil)
        {
            currentOil = Mathf.Min(maxOil, currentOil + oilRegenPerSecond * Time.deltaTime);
        }

        UpdateOilGauge();
    }

    private void ForceExtinguishLantern()
    {
        if (!lanternIsOn) return;

        lanternIsOn = false;
        lanternLight.SetActive(false);
        GetComponent<SoundContainer>().PlaySound("StopLantern", 1);
    }

    private void UpdateOilGauge()
    {
        if (currentOil >= maxOil)
        {
            if (lanternOilGaugeInstance != null)
            {
                Destroy(lanternOilGaugeInstance);
                lanternOilGaugeInstance = null;
            }

            return;
        }

        if (lanternOilGaugeInstance == null && lanternOilGaugePrefab != null)
        {
            lanternOilGaugeInstance = Instantiate(lanternOilGaugePrefab, transform.position + Vector3.up * oilGaugeHeightOffset, Quaternion.identity, transform);
        }

        if (lanternOilGaugeInstance != null)
        {
            Scrollbar scrollbar = lanternOilGaugeInstance.GetComponentInChildren<Scrollbar>();
            if (scrollbar != null)
                scrollbar.size = currentOil / maxOil;
        }
    }

    public void UsingShield(int direction)
    {
        isShielding = true;
        StartCoroutine(RoutineUsingShield(direction));
    }

    IEnumerator RoutineUsingShield(int direction)
    {
        // Cr�er un GameObject pour le bouclier
        GameObject shieldObject = new GameObject("Shield");
        shieldObject.transform.SetParent(transform);
        GetComponent<SoundContainer>().PlaySound("ShieldEquip", 1);

        // Ajouter un BoxCollider2D pour repr�senter la zone de protection
        BoxCollider2D collider = shieldObject.AddComponent<BoxCollider2D>();
        ShieldBehiavor shield = shieldObject.AddComponent<ShieldBehiavor>();
        ObjectParticles particles = shieldObject.AddComponent<ObjectParticles>();

        particles.particles.Add(new Particles { particleSystem = shieldParticle, id = "HitShield" });

        shield.SetShield(true, gameObject);
        collider.isTrigger = true;

        // Positionner et orienter le bouclier en fonction de la direction
        Vector2 offset = Vector2.zero;
        Vector2 size = Vector2.zero;
        float distanceOffset = 0.5f; // Distance divis�e par 2

        switch (direction)
        {
            case 0: // Up
                offset = new Vector2(0, distanceOffset);
                size = new Vector2(0.5f, 0.25f); // Taille divis�e par 2
                break;
            case 1: // Left
                offset = new Vector2(-distanceOffset, 0);
                size = new Vector2(0.25f, 0.5f); // Taille divis�e par 2
                break;
            case 2: // Right
                offset = new Vector2(distanceOffset, 0);
                size = new Vector2(0.25f, 0.5f); // Taille divis�e par 2
                break;
            case 3: // Down
                offset = new Vector2(0, -distanceOffset);
                size = new Vector2(0.5f, 0.25f); // Taille divis�e par 2
                break;
        }

        collider.size = size;
        shieldObject.transform.localPosition = offset;

        // Attendre que le joueur rel�che le bouton de bouclier
        while (isShielding)
        {

            if (!PlayerManager.instance.playerInputActions.Gameplay.SpecialItem.IsPressed())
            {
                isShielding = false;
                GetComponent<PlayerController>().actualSpeed = stats.speed;
            }
            yield return null; // Attendre la prochaine frame
        }

        // D�truire le bouclier lorsque l'action est finie
        Destroy(shieldObject);
    }

    private float maxDistance = 10f; // Distance maximale pour arr�ter l'attaque sp�ciale

    public void UsingShadowMedal()
    {
        if (!isShadowing)
        {
            StartCoroutine(RoutineUsingShadowMedal());
            isShadowing = true;
            GetComponent<EntityEffects>().ToggleAffectations();
        }
        else
        {
            StartCoroutine(EndShadowMedal());
            isShadowing = false;
            GetComponent<EntityEffects>().ToggleAffectations();

        }
    }

    IEnumerator RoutineUsingShadowMedal()
    {
        stats.canMove = false;
        GetComponent<SoundContainer>().PlaySound("DarkMedal", 1);
        GetComponentInChildren<SpriteRenderer>().color = new Color(78f / 255f, 0f / 255f, 78f / 255f);
        CameraManager.instance.SetFilter(CameraFilter.SHADOW_MEDAL);
        GetComponent<EntityLight>().TransitionLightColor(Color.magenta, 1f);
        GetComponent<EntityLight>().TransitionLightIntensity(2, 3f, 1f);

        yield return new WaitForSecondsRealtime(1f);
        CameraManager.instance.SetChromaticAberrationEffect(0, 1f, CameraManager.instance.defaultCamera);
        stats.canMove = true;
        playerSleepInstance = Instantiate(playerSleepPrefab, transform.position, Quaternion.identity);
        StartCoroutine(CheckDistanceAndUpdateEffects());
        GetComponent<EntityLight>().TransitionLightColor(Color.white, 1f);
        GetComponent<EntityLight>().TransitionLightIntensity(.25f, 1f, 1f);
    }

    IEnumerator EndShadowMedal()
    {
        GetComponent<SoundContainer>().PlaySound("DarkMedal", 1);
        StopCoroutine(CheckDistanceAndUpdateEffects());
        CameraManager.instance.ChangeCameraColor(Color.white, 0.5f, CameraManager.instance.defaultCamera);
        CameraManager.instance.ShakeCamera(3, 3, 1f);
        CameraManager.instance.SetVignetteEffect(0, 0, 1f, CameraManager.instance.defaultCamera);
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
        CameraManager.instance.SetChromaticAberrationEffect(2, 1f, CameraManager.instance.defaultCamera);
        stats.canMove = false;
        GetComponent<EntityLight>().TransitionLightColor(Color.magenta, 1f);
        GetComponent<EntityLight>().TransitionLightIntensity(2, 3f, 1f);

        yield return new WaitForSecondsRealtime(0.5f);
        transform.position = playerSleepInstance.transform.position;

        yield return new WaitForSecondsRealtime(0.5f);
        MeteoManager.instance.SetSceneFilter();
        Destroy(playerSleepInstance);
        stats.canMove = true;
        GetComponent<EntityLight>().TransitionLightColor(Color.white, 1f);
        GetComponent<EntityLight>().TransitionLightIntensity(.25f, 1f, 1f);

    }

    private IEnumerator CheckDistanceAndUpdateEffects()
    {
        while (isShadowing)
        {
            if (playerSleepInstance != null)
            {
                float distance = Vector2.Distance(transform.position, playerSleepInstance.transform.position);
                if (distance > maxDistance)
                {
                    // Arr�tez l'attaque sp�ciale si la distance est trop grande
                    UsingShadowMedal();
                    yield break; // Quittez la coroutine si l'attaque est termin�e
                }

                // Ajustez les effets en fonction de la distance
                float t = Mathf.Clamp01(distance / maxDistance);
                float vignetteIntensity = Mathf.Lerp(0.5f, 1f, t);
                float chromaticAberrationIntensity = Mathf.Lerp(0f, 2f, t); // Invers� pour augmenter avec la distance

                CameraManager.instance.SetVignetteEffect(vignetteIntensity, 2, 0f, CameraManager.instance.defaultCamera);
                CameraManager.instance.SetChromaticAberrationEffect(chromaticAberrationIntensity, 0f, CameraManager.instance.defaultCamera);
            }

            yield return null; // V�rifiez chaque frame
        }
    }


    public void UsingLantern(int direction)
    {
        isLightning = true;
        StartCoroutine(UsingLanternRoutine(direction));
    }

    IEnumerator UsingLanternRoutine(int direction)
    {
        if (!lanternIsOn)
        {
            if (currentOil < minOilToIgnite)
            {
                GetComponent<SoundContainer>().PlaySound("LanternRefuse", 1);

                // isLightning reste vrai le temps du cooldown pour emp�cher le son de spammer
                // si le joueur maintient l'action enfonc�e.
                yield return new WaitForSeconds(refuseCooldown);

                isLightning = false;
                GetComponent<PlayerController>().actualSpeed = stats.speed;
                yield break;
            }

            currentOil -= igniteOilCost;
            UpdateOilGauge();

            // Calculer la position de d�tection en fonction de la direction
            Vector2 detectionPosition = transform.position;
            switch (direction)
            {
                case 0:
                    detectionPosition += new Vector2(0, 0.5f); // +0.5 en Y
                    break;
                case 1:
                    detectionPosition += new Vector2(-0.5f, sideDetectionVerticalOffset); // -0.5 en X
                    break;
                case 2:
                    detectionPosition += new Vector2(0.5f, sideDetectionVerticalOffset); // +0.5 en X
                    break;
                case 3:
                    detectionPosition += new Vector2(0, -0.5f); // -0.5 en Y
                    break;
            }

            if (lanternActivationEffectPrefab != null)
                Instantiate(lanternActivationEffectPrefab, detectionPosition, Quaternion.identity);

            // D�tection de l'objet avec un collider de 0.5 de rayon
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(detectionPosition, 0.1f);
            foreach (Collider2D hitCollider in hitColliders)
            {
                // GetComponentInParent (et non GetComponent) : certains prefabs portent le collider
                // trigger sur un enfant visuel distinct du GameObject racine qui porte EntityEffects.
                EntityEffects hitEntityEffects = hitCollider.GetComponentInParent<EntityEffects>();
                if (hitEntityEffects != null && hitEntityEffects.canBeFire && hitCollider != this.gameObject.GetComponent<Collider2D>())
                {
                    hitEntityEffects.SetState(Mathf.Max(GetComponent<Stats>().strength / 2, 1), true);
                }
            }
        }

        lanternIsOn = !lanternIsOn;

        lanternLight.SetActive(lanternIsOn);

        GetComponent<SoundContainer>().PlaySound(lanternIsOn ? "InitLantern" : "StopLantern", 1);

        yield return new WaitForSeconds(0.5f);
        isLightning = false;
        GetComponent<PlayerController>().actualSpeed = stats.speed;
    }

    public void UsingPickaxe(int direction)
    {
        isPickaxing = true;
        StartCoroutine(UsingPickaxeRoutine(direction));
    }

    IEnumerator UsingPickaxeRoutine(int direction)
    {
        GetComponent<SoundContainer>().PlaySound("Attack", 1);
        yield return new WaitForSeconds(0.5f - (.5f * PlayerManager.instance.pickaxeSpeed));

        // Calculer la position de d�tection en fonction de la direction
        Vector2 detectionPosition = transform.position;
        switch (direction)
        {
            case 0:
                detectionPosition += new Vector2(0, 0.5f); // +0.5 en Y
                break;
            case 1:
                detectionPosition += new Vector2(-0.5f, 0); // -0.5 en X
                break;
            case 2:
                detectionPosition += new Vector2(0.5f, 0); // +0.5 en X
                break;
            case 3:
                detectionPosition += new Vector2(0, -0.5f); // -0.5 en Y
                break;
        }

        // D�tection de l'objet avec un collider de 0.5 de rayon
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(detectionPosition, 0.1f);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<DestroyableBehiavor>() != null)
            {
                hitCollider.GetComponent<DestroyableBehiavor>().DestroyObject(3);
                break;
            }
            else if (hitCollider.GetComponent<MineralBehiavor>() != null)
            {
                hitCollider.GetComponent<MineralBehiavor>().HitMineral(pickaxePower);
            }
            else if (hitCollider.GetComponent<RadioActiveIcePuzzle>() != null)
            {
                hitCollider.GetComponent<RadioActiveIcePuzzle>().HitIce(pickaxePower);
            }
            else if (hitCollider.GetComponent<RockBoulderBehiavor>() != null)
            {
                hitCollider.GetComponent<RockBoulderBehiavor>().Hit(direction);
            }
        }

        GetComponent<SoundContainer>().PlaySound("PickaxeHit", 1);

        yield return new WaitForSeconds(0.25f - (.25f * PlayerManager.instance.pickaxeSpeed));
        GetComponent<PlayerController>().actualSpeed = stats.speed;
        isPickaxing = false;
    }

    public void HammerAttack(int direction)
    {
        isHammering = true;
        StartCoroutine(HammerAttackRoutine(direction));
    }

    IEnumerator HammerAttackRoutine(int direction)
    {
        GetComponent<SoundContainer>().PlaySound("Attack", 1);
        yield return new WaitForSeconds(1);
        CameraManager.instance.ShakeCamera(1.5f, 1.5f, 1);

        // Calculer la position d'impact en fonction de la direction
        Vector3 impactPosition = transform.position;
        switch (direction)
        {
            case 0:
                impactPosition += new Vector3(0, 0.5f, 0); // +0.5 en Y
                break;
            case 1:
                impactPosition += new Vector3(-1f, -0.5f, 0); // -0.5 en X
                break;
            case 2:
                impactPosition += new Vector3(1f, -0.5f, 0); // +0.5 en X
                break;
            case 3:
                impactPosition += new Vector3(0, -0.5f, 0); // -0.5 en Y
                break;
        }

        // Instancier l'impact du marteau � la position calcul�e
        GameObject hammerImpactInstance = Instantiate(hammerImpact, impactPosition, Quaternion.identity);
        hammerImpactInstance.GetComponent<HammerImpact>().SetHammerImpact(stats.strength);
        GetComponent<SoundContainer>().PlaySound("HammerImpact", 1);

        yield return new WaitForSeconds(1);
        isHammering = false;
        GetComponent<PlayerController>().actualSpeed = stats.speed;
    }


    private int bowChargeDirection = -1;

    public void ShootBow(int direction)
    {
        if (checkBowCoroutine != null)
            StopCoroutine(checkBowCoroutine);

        stats.isBowShooting = true;
        GetComponent<SoundContainer>().PlaySound("BowCharge", 1);

        // --- Capture de la direction au moment du d�but de la charge ---
        PlayerAnimation pa = GetComponent<PlayerAnimation>();
        if (pa != null)
        {
            int lm = pa.GetLastMove();
            Debug.Log($"LastMove: {lm}, LocalScale.x: {transform.localScale.x}");

            if (lm == 1) // Up
            {
                bowChargeDirection = 0;
            }
            else if (lm == 2) // Down
            {
                bowChargeDirection = 3;
            }
            else if (lm == 3) // Side (mouvement lat�ral)
            {
                // Utiliser SpriteRenderer.flipX au lieu de localScale.x
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (sr.flipX)
                    {
                        bowChargeDirection = 1; // Gauche (sprite flipp�)
                        Debug.Log("Direction: GAUCHE (flipX = true)");
                    }
                    else
                    {
                        bowChargeDirection = 2; // Droite (sprite normal)
                        Debug.Log("Direction: DROITE (flipX = false)");
                    }
                }
                else
                {
                    // Fallback si pas de SpriteRenderer
                    Debug.LogWarning("Pas de SpriteRenderer trouv�!");
                    bowChargeDirection = direction;
                }
            }
            else
            {
                // Fallback : utiliser la direction pass�e en param�tre
                bowChargeDirection = direction;
            }

            Debug.Log($"Final bowChargeDirection: {bowChargeDirection}");
        }

        checkBowCoroutine = StartCoroutine(CheckBowRelease());
    }

    private IEnumerator CheckBowRelease()
    {
        float startTime = Time.time;
        bool buttonReleased = false;
        float chargeTime = .65f - (.65f * PlayerManager.instance.bowSpeed);

        while (stats.isBowShooting)
        {
            if (!PlayerManager.instance.playerInputActions.Gameplay.SpecialItem.IsPressed())
                buttonReleased = true;

            if (buttonReleased && Time.time - startTime >= chargeTime)
            {
                stats.isBowShooting = false;

                // --- Utiliser la direction captur�e au d�but ---
                int finalDirection = bowChargeDirection;

                GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
                arrow.GetComponent<ProjectileBehavior>().InitProjectile(
                    stats.strength, 10, finalDirection, true, stats.knockbackPower, gameObject);

                GetComponent<PlayerController>().actualSpeed = stats.speed;
                PlayerManager.instance.GetSpecialItem(SpecialItemType.ARROW).nb--;
                GetComponent<SoundContainer>().PlaySound("BowRelease", 1);

                yield break;
            }
            else if (buttonReleased)
            {
                stats.isBowShooting = false;
                GetComponent<PlayerController>().actualSpeed = stats.speed;
                yield break;
            }

            yield return null;
        }
    }





}
