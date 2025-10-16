using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ShyronBehiavor : MonoBehaviour
{
    public float detectionRadius = 5;
    public float ambienceRadius = 10;
    public List<Sprite> walkSprites = new List<Sprite>();
    public Stats stats;
    public float moveSpeed = .5f; // Vitesse de déplacement
    public AudioClip music;
    public GameObject shadow;

    public GameObject jumpEffectPrefab;

    private bool isWalking = false;
    private bool isSpecialAttacking = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalShadowScale;
    private Coroutine walkRoutine; // Pour stocker la référence à la coroutine de marche
    private Vector3 baseLocalPosition;


    enum SpecialAttack
    {
        JUMP,
        DARK_CHRIST, // à 5 unités de radius, lumière démoniaque venant du ciel tuant le joueur
    }

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        walkRoutine = StartCoroutine(Walks());
        StartCoroutine(AmbientSoundRoutine());

        // On stocke l'échelle originale de l'ombre
        if (shadow != null)
            originalShadowScale = shadow.transform.localScale;

        baseLocalPosition = spriteRenderer.transform.localPosition;

    }

    SpecialAttack GetSpecialAttack()
    {
        if (Random.Range(0, 101) >= 60)
            return SpecialAttack.JUMP;
        else
            return SpecialAttack.DARK_CHRIST;
    }

    bool PlayerDetected(float radius)
    {
        return radius >= Vector2.Distance(transform.position, PlayerManager.instance.player.transform.position);
    }

    private void Update()
    {
        // Ne met à jour isWalking que si aucune attaque spéciale n'est en cours
        if (!isSpecialAttacking)
        {
            isWalking = PlayerDetected(detectionRadius);
        }
        else
        {
            // S'assurer que isWalking est false pendant une attaque spéciale
            isWalking = false;
        }

        if (PlayerDetected(ambienceRadius) && CameraManager.instance.GetFilter() != CameraFilter.SHYRON)
        {
            CameraManager.instance.SetFilter(CameraFilter.SHYRON);
            CameraManager.instance.UpdateFilter();
            SoundManager.instance.PlayMusic(music);
        }
        else if (!PlayerDetected(ambienceRadius) && CameraManager.instance.GetFilter() == CameraFilter.SHYRON)
        {
            CameraManager.instance.SetFilter(CameraFilter.NONE);
            CameraManager.instance.UpdateFilter();
            SoundManager.instance.PlayMusic(MeteoManager.instance.actualScene.music);
        }
    }

    IEnumerator ScaleShadow(float duration, bool reverse)
    {
        if (shadow == null)
            yield break;

        Vector3 startScale = reverse ? Vector3.zero : originalShadowScale;
        Vector3 endScale = reverse ? originalShadowScale : Vector3.zero;

        float elapsed = 0;
        while (elapsed < duration)
        {
            shadow.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        shadow.transform.localScale = endScale;
    }

    IEnumerator JumpRoutineMultiple()
    {
        isSpecialAttacking = true; // Marquer comme en attaque spéciale au début
        GetComponent<EntityLight>().TransitionLightIntensity(0, 0, 1);


        // Nombre aléatoire de sauts entre 1 et 10
        int numberOfJumps = Random.Range(1, 10);
        float speedMultiplier = 1.0f;

        for (int i = 0; i < numberOfJumps; i++)
        {
            // Effectuer un saut avec le multiplicateur de vitesse actuel
            yield return StartCoroutine(SingleJump(speedMultiplier));

            // Augmenter la vitesse de 10% pour le prochain saut
            speedMultiplier *= 1.2f;

            // Petit délai entre les sauts (optionnel)
            if (i < numberOfJumps - 1)
            {
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        isSpecialAttacking = false; // Fin de l'attaque spéciale

        GetComponent<EntityLight>().TransitionLightIntensity(1, 1, 1);
    }

    // Méthode pour un seul saut, avec multiplicateur de vitesse
    IEnumerator SingleJump(float speedMultiplier)
    {
        // Calcul des durées ajustées selon le multiplicateur de vitesse
        float animationDuration = 1.0f / speedMultiplier;
        float jumpDuration = 0.7f / speedMultiplier;
        float waitDuration = 2.0f / speedMultiplier;
        float landDuration = 0.7f / speedMultiplier;

        // Animation de préparation du saut
        GetComponent<ObjectAnimation>().PlayAnimation("Jump", false, false, speedMultiplier);
        yield return new WaitForSecondsRealtime(animationDuration);
        GetComponent<ObjectAnimation>().StopAnimation();

        // Effet du saut
        CameraManager.instance.ShakeCamera(3, 3, 1f);
        Instantiate(jumpEffectPrefab, transform.position, Quaternion.identity);
        GetComponent<Collider2D>().enabled = false;

        // Disparition (montée)
        StartCoroutine(ScaleShadow(jumpDuration, false));
        StartCoroutine(MoveSpriteVertical(jumpDuration, false));
        GetComponent<SoundContainer>().PlaySound("Earthquake", 2);

        // Attente pendant que le monstre est "en l'air"
        yield return new WaitForSecondsRealtime(waitDuration);

        // Repositionner sur le joueur
        transform.position = PlayerManager.instance.player.transform.position;

        // Réapparition (descente)
        StartCoroutine(ScaleShadow(landDuration, true));
        StartCoroutine(MoveSpriteVertical(landDuration, true));
        yield return new WaitForSecondsRealtime(landDuration);

        // Impact au sol
        CameraManager.instance.ShakeCamera(3, 3, 1f);
        GetComponent<Collider2D>().enabled = true;
        GetComponent<SoundContainer>().PlaySound("Earthquake", 2);
    }

    IEnumerator MoveSpriteVertical(float duration, bool reverse)
    {
        if (spriteRenderer == null)
            yield break;

        Vector3 startPos = baseLocalPosition;
        Vector3 endPos = baseLocalPosition + new Vector3(0, 100, 0);
        Vector3 origin = reverse ? endPos : startPos;
        Vector3 target = reverse ? startPos : endPos;

        // Si le sprite est désactivé pendant la phase montante, l'activer pour la descente
        if (reverse && !spriteRenderer.gameObject.activeSelf)
        {
            spriteRenderer.transform.localPosition = endPos; // Position initiale pour la descente
            spriteRenderer.gameObject.SetActive(true);
        }

        float elapsed = 0;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Utiliser une courbe plus naturelle pour le mouvement
            float easeValue = reverse ? 1 - Mathf.Pow(1 - t, 2) : Mathf.Pow(t, 2);
            spriteRenderer.transform.localPosition = Vector3.Lerp(origin, target, easeValue);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.transform.localPosition = target;

        // Désactiver le sprite seulement à la fin de la montée, pas pendant la descente
        if (!reverse)
        {
            spriteRenderer.gameObject.SetActive(false);
        }
    }

    public GameObject shyronHandPrefab;

    IEnumerator DarkChristRoutine()
    {
        isSpecialAttacking = true;

        GetComponent<ObjectAnimation>().PlayAnimation("DarkChrist", true);
        GetComponent<EntityLight>().TransitionLightIntensity(5, 5, 1);
        CameraManager.instance.ShakeCamera(5, 5, 1f);
        yield return new WaitForSecondsRealtime(1);

        float spawnTime = 0f;
        float totalDuration = 8f;

        while (spawnTime < totalDuration)
        {
            Vector2 spawnPosition = Vector2.zero;
            bool positionFound = false;

            for (int i = 0; i < 10; i++) // Jusqu'à 10 tentatives
            {
                Vector2 randomOffset = Random.insideUnitCircle * 3f;
                spawnPosition = (Vector2)PlayerManager.instance.player.transform.position + randomOffset;

                Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, 0.3f);
                bool onlyPlayer = true;

                foreach (var collider in colliders)
                {
                    var stats = collider.gameObject.GetComponent<Stats>();
                    if (stats == null || stats.entityType != EntityType.Player)
                    {
                        onlyPlayer = false;
                        break;
                    }
                }

                if (onlyPlayer)
                {
                    positionFound = true;
                    break;
                }
            }

            if (positionFound)
            {
                GameObject shyronHandInstance = Instantiate(shyronHandPrefab, spawnPosition, Quaternion.identity);
                shyronHandInstance.GetComponent<ShyronHandBehiavor>().InitHand(GetComponent<Stats>().strength);
            }

            float waitTime = Random.Range(0.5f - (spawnTime / 10f), 1f - (spawnTime / 10f));
            spawnTime += waitTime;

            yield return new WaitForSecondsRealtime(waitTime);
        }

        GetComponent<EntityLight>().TransitionLightIntensity(1, 1, 1);

        isSpecialAttacking = false;
    }



    IEnumerator AmbientSoundRoutine()
    {
        while (true)
        {
            if(PlayerDetected(ambienceRadius))
                GetComponent<SoundContainer>().PlaySound("Ambient", 2);

            yield return new WaitForSecondsRealtime(Random.Range(3, 20));
        }
    }

    IEnumerator Walks()
    {
        while (true)
        {
            // Vérifier à chaque itération si on peut marcher
            if (isWalking && !isSpecialAttacking)
            {
                // Liste des groupes d'index à parcourir
                List<int[]> spriteGroups = new List<int[]>
                {
                    new int[] { 0, 1, 2 },
                    new int[] { 3 },
                    new int[] { 2, 1 },
                    new int[] { 0, 4, 5 },
                    new int[] { 6 },
                    new int[] { 5, 4, 0 }
                };

                // Durées pour chaque groupe
                float[] durations = { 0.15f, 0.5f, 0.05f, 0.15f, 0.5f, 0.05f };

                // Parcours des groupes et application des sprites
                for (int i = 0; i < spriteGroups.Count; i++)
                {
                    // Vérifier à nouveau si une attaque spéciale n'a pas commencé
                    if (isSpecialAttacking)
                    {
                        break; // Sortir de la boucle si une attaque spéciale a commencé
                    }

                    int[] group = spriteGroups[i];
                    float duration = durations[i];

                    // Pour les indices 3 et 6, approche du joueur
                    bool moveTowardsPlayer = (i == 1 || i == 4); // Ces indices correspondent aux groupes contenant les sprites 3 et 6

                    foreach (int index in group)
                    {
                        // Vérifier à nouveau si une attaque spéciale n'a pas commencé
                        if (isSpecialAttacking)
                        {
                            break; // Sortir de la boucle si une attaque spéciale a commencé
                        }

                        spriteRenderer.sprite = walkSprites[index];

                        // Si c'est un moment où on doit se déplacer vers le joueur
                        if (moveTowardsPlayer)
                        {
                            GetComponent<SoundContainer>().PlaySound("Rumble", 2);
                            CameraManager.instance.ShakeCamera(2, 2, .5f);
                            // Durée totale de mouvement pour ce sprite
                            float moveTime = 0;
                            float totalMoveTime = duration;

                            // Calcul de la direction vers le joueur
                            Vector2 direction = (PlayerManager.instance.player.transform.position - transform.position).normalized;

                            while (moveTime < totalMoveTime && !isSpecialAttacking) // Vérifier aussi pendant le mouvement
                            {
                                // Calcul de la distance à parcourir pendant cette frame
                                float frameDistance = moveSpeed * Time.deltaTime / totalMoveTime;

                                // Déplacement vers le joueur (limité à 1 unité au total)
                                transform.position += (Vector3)direction * frameDistance;

                                moveTime += Time.deltaTime;
                                yield return null;
                            }

                            // Après le pas, regarde si attaque spéciale (20% après chaque pas)
                            if (!isSpecialAttacking && Random.Range(0, 101) >= 80)
                            {
                                isSpecialAttacking = true;

                                yield return new WaitForSecondsRealtime(.5f);

                                switch (GetSpecialAttack())
                                {
                                    case SpecialAttack.JUMP:
                                        StartCoroutine(JumpRoutineMultiple());
                                        break;
                                    case SpecialAttack.DARK_CHRIST:
                                        StartCoroutine(DarkChristRoutine());
                                        break;
                                    default:
                                        break;
                                }

                                break; // Sortir de la boucle des sprites après avoir lancé une attaque spéciale
                            }
                        }
                        else
                        {
                            // Pour les autres sprites, attendre tout en vérifiant si une attaque spéciale commence
                            float waitTime = 0;
                            while (waitTime < duration && !isSpecialAttacking)
                            {
                                waitTime += Time.deltaTime;
                                yield return null;
                            }
                        }

                        // Si une attaque spéciale a commencé, sortir de la boucle
                        if (isSpecialAttacking)
                        {
                            break;
                        }
                    }

                    // Si une attaque spéciale a commencé, sortir de la boucle des groupes
                    if (isSpecialAttacking)
                    {
                        break;
                    }
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(.1f); // Temps d'attente plus court pour réagir rapidement
            }
        }
    }
}