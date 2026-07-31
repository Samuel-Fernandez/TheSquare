using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianHeartBehiavor : MonoBehaviour
{
    [Header("Settings")]
    public float speedDivider = 1.5f;
    public float throwDistance = 1f;
    public float throwDuration = 0.15f;
    public Vector3 carriedOffset = new Vector3(0.2f, 0.3f, 0f);

    [Header("Heartbeat Settings")]
    public float beatInterval = 1f;
    public float firstBeatDelay = 0.5f;
    public Color beatLightColor = new Color(0.6f, 0f, 0.8f);
    public float lightIntensityMax = 2.5f;
    public float lightIntensityMin = 0.25f;
    public float lightRadiusMax = 2.5f;
    public float lightRadiusMin = 1.5f;
    public float transitionTimeUp = 0.1f;
    public float transitionTimeDown = 0.4f;

    public bool isCarried = false;
    public bool isAbsorbing = false;
    private Coroutine heartbeatCoroutine;
    private EntityLight entityLight;
    private SoundContainer soundContainer;
    private int baseSortingOrder = 0;

    private void Awake()
    {
        entityLight = GetComponent<EntityLight>();
        soundContainer = GetComponent<SoundContainer>();
    }

    private void Start()
    {
        if (entityLight != null)
        {
            // Set light color from inspector parameter
            entityLight.SetLightColor(beatLightColor);
        }

        // Démarre le battement dès le début, que le joueur l'ait pris ou non
        heartbeatCoroutine = StartCoroutine(HeartbeatRoutine());

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) baseSortingOrder = sr.sortingOrder;
    }

    private void Update()
    {
        if (isAbsorbing)
        {
            // Masquer le prompt d'interaction dès que l'absorption a commencé, même le temps d'une frame
            InteractableBehiavor interactable = GetComponent<InteractableBehiavor>();
            if (interactable != null) interactable.forceHideUI = true;
        }

        if (isCarried && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            // Forcer la position pour éviter que le moteur physique ou autre fasse glisser le coeur
            transform.position = PlayerManager.instance.player.transform.position + carriedOffset;

            // Gérer le sorting order en continu
            SpriteRenderer playerSr = PlayerManager.instance.player.GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer mySr = GetComponentInChildren<SpriteRenderer>();
            if (playerSr != null && mySr != null)
            {
                mySr.sortingOrder = playerSr.sortingOrder + 10;
                mySr.sortingLayerID = playerSr.sortingLayerID;
            }
        }
    }

    public void Interaction()
    {
        // Le coeur est déjà en cours d'absorption par le manager : il ne doit plus être interactible
        if (isAbsorbing) return;

        InteractableBehiavor interactable = GetComponent<InteractableBehiavor>();
        if (interactable != null)
        {
            interactable.inactiveTime = 1f; // Bloque la ré-interaction pour 1 seconde via le script parent
        }

        StartCoroutine(FreezePlayerRoutine(1f));

        if (!isCarried)
        {
            PickUp();
        }
        else
        {
            Throw();
        }
    }

    private IEnumerator FreezePlayerRoutine(float duration)
    {
        Stats playerStats = PlayerManager.instance.player.GetComponent<Stats>();
        if (playerStats != null)
        {
            playerStats.canMove = false;
        }

        yield return new WaitForSeconds(duration);

        if (playerStats != null)
        {
            playerStats.canMove = true;
        }
    }

    private void PickUp()
    {
        isCarried = true;
        Transform playerTransform = PlayerManager.instance.player.transform;

        // Stopper la physique
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Désactiver ObjectPerspective pendant le port
        ObjectPerspective op = GetComponent<ObjectPerspective>();
        if (op != null)
        {
            op.enabled = false;
        }

        // Cacher l'UI d'interaction pendant le port
        InteractableBehiavor interactable = GetComponent<InteractableBehiavor>();
        if (interactable != null)
        {
            interactable.forceHideUI = true;
        }

        // S'attacher au joueur (au-dessus de la tête)
        transform.SetParent(playerTransform);
        transform.localPosition = carriedOffset;
        transform.position = playerTransform.position + carriedOffset;

        if (soundContainer != null)
        {
            soundContainer.PlaySound("Hold", 1);
        }

        // Modifier la vitesse du joueur
        Stats playerStats = PlayerManager.instance.player.GetComponent<Stats>();
        if (playerStats != null)
        {
            playerStats.speed /= speedDivider;
            PlayerController pc = PlayerManager.instance.player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.isHoldingObject = true;
                pc.UpdateSpeed(playerStats.speed);
            }
        }
    }

    public void ForceDrop()
    {
        isCarried = false;
        transform.SetParent(null); // Détacher du joueur

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Réactiver ObjectPerspective après le lancer
        ObjectPerspective op = GetComponent<ObjectPerspective>();
        if (op != null)
        {
            op.enabled = true;
        }

        // Réactiver l'UI d'interaction après le lancer
        InteractableBehiavor interactable = GetComponent<InteractableBehiavor>();
        if (interactable != null)
        {
            interactable.forceHideUI = false;
            interactable.canInteract = true; // S'assurer qu'il peut de nouveau être interactible si besoin
        }

        SpriteRenderer mySr = GetComponentInChildren<SpriteRenderer>();
        if (mySr != null)
        {
            mySr.sortingOrder = baseSortingOrder;
        }

        // Rétablir la vitesse du joueur
        Stats playerStats = PlayerManager.instance.player.GetComponent<Stats>();
        if (playerStats != null)
        {
            playerStats.UpdateStats(); // Cela va recalculer et réappliquer la vitesse normale
        }

        PlayerController pc = PlayerManager.instance.player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.isHoldingObject = false;
        }
    }

    private void Throw()
    {
        ForceDrop();

        if (soundContainer != null)
        {
            soundContainer.PlaySound("Throw", 1);
        }

        // Logique de lancer directionnel
        PlayerController pc = PlayerManager.instance.player.GetComponent<PlayerController>();
        Vector2 throwDirection = Vector2.right; // par défaut
        if (pc != null)
        {
            throwDirection = pc.GetAttackDirection();
        }

        Vector2 targetPosition = (Vector2)transform.position + throwDirection * throwDistance;

        // Vérifier les obstacles
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, throwDirection, throwDistance);
        Vector3 finalPos = targetPosition;
        foreach (var hit in hits)
        {
            if (hit.collider != null && !hit.collider.isTrigger && hit.collider.gameObject != PlayerManager.instance.player)
            {
                // Si on touche un obstacle non-trigger qui n'est pas le joueur
                finalPos = hit.point;
                break;
            }
        }

        StartCoroutine(ThrowRoutine(finalPos));
    }

    private IEnumerator ThrowRoutine(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / throwDuration);
            yield return null;
        }
        transform.position = targetPos;
    }

    private IEnumerator HeartbeatRoutine()
    {
        // Délai de départ
        yield return new WaitForSeconds(firstBeatDelay);

        while (true)
        {
            if (soundContainer != null)
            {
                soundContainer.PlaySound("Beat", 1);
            }

            if (entityLight != null)
            {
                // La lumière s'agrandit
                entityLight.TransitionLightIntensity(lightIntensityMax, lightRadiusMax, transitionTimeUp);
                yield return new WaitForSeconds(transitionTimeUp);

                // La lumière se réduit
                entityLight.TransitionLightIntensity(lightIntensityMin, lightRadiusMin, transitionTimeDown);

                // Attente du temps restant jusqu'au prochain battement
                float waitTime = beatInterval - transitionTimeUp;
                if (waitTime < 0) waitTime = 0;
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                yield return new WaitForSeconds(beatInterval);
            }
        }
    }
}
