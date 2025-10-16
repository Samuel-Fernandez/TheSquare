using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkBowerBehiavor : MonoBehaviour
{
    public int pointCount = 20;
    public float radius = 5f;
    public float checkRadius = 0.25f;
    public List<Vector2> validPoints = new List<Vector2>();

    public GameObject bowerClonePrefab;
    public GameObject arrowPrefab;

    // Si true, comportement juste de fantôme
    public bool bowerClone;

    int actualLife;
    SpriteRenderer spriteRenderer;
    Stats stats;
    LifeManager lifeManager;
    ObjectAnimation objectAnimation;

    List<GameObject> bowerClones = new List<GameObject>();

    // Pour éviter les coroutines multiples
    bool isProcessingHit = false;
    Coroutine attackCoroutine;
    Coroutine postAttackCoroutine;

    // Énumérateur pour l'orientation
    public enum Orientation
    {
        Down,
        Up,
        Left,
        Right
    }

    public Orientation actualOrientation = Orientation.Down;

    // Référence au bower principal (pour les clones)
    DarkBowerBehiavor mainBower;

    void Start()
    {
        lifeManager = GetComponent<LifeManager>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        stats = GetComponent<Stats>();
        objectAnimation = GetComponent<ObjectAnimation>();

        if (!bowerClone)
        {
            GenerateValidPoints();
            actualLife = lifeManager.life;
            mainBower = this; // Le principal se référence lui-même

            // Commencer le cycle dès le début
            StartCoroutine(InitialAppear());
        }
        else
        {
            stats.health = 1;
            stats.money = 0;
            lifeManager.life = 1;
            actualLife = 1;
        }
    }

    IEnumerator InitialAppear()
    {
        // Petit délai initial optionnel
        yield return new WaitForSeconds(0.5f);
        Appear();
    }

    private void Update()
    {
        if (!bowerClone && lifeManager != null)
        {
            int currentLife = lifeManager.life;

            // Vérifie si la vie a changé et qu'on ne traite pas déjà un hit
            if (currentLife != actualLife && !isProcessingHit)
            {
                GetComponent<SoundContainer>().PlaySound("Hit", 2);
                StartCoroutine(HitRoutine());
                actualLife = currentLife;
            }
        }
    }

    private IEnumerator Attack()
    {
        var objectAnim = GetComponent<ObjectAnimation>();
        string animName;
        int direction;

        switch (actualOrientation)
        {
            case Orientation.Down:
                animName = "AttackDown";
                direction = 3;
                break;
            case Orientation.Up:
                animName = "AttackUp";
                direction = 0;
                break;
            case Orientation.Left:
                animName = "AttackSide";
                direction = 1;
                break;
            case Orientation.Right:
                animName = "AttackSide";
                direction = 2;
                break;
            default:
                animName = "AttackDown";
                direction = 3;
                break;
        }

        GetComponent<SoundContainer>().PlaySound("ChargeBow", 2);

        // Joue l'animation d'attaque
        yield return StartCoroutine(objectAnim.PlayAnimationCoroutine(animName));

        // Crée et initialise la flèche
        var arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        var proj = arrow.GetComponent<ProjectileBehavior>();
        proj.InitProjectile(stats.strength, 6, direction, false, stats.knockbackPower, gameObject);

        GetComponent<SoundContainer>().PlaySound("ShootArrow", 2);

        // Rejoue l'animation de base correspondant à l'orientation
        string baseAnimName = "";
        switch (actualOrientation)
        {
            case Orientation.Down:
                baseAnimName = "Down";
                break;
            case Orientation.Up:
                baseAnimName = "Up";
                break;
            case Orientation.Left:
            case Orientation.Right:
                baseAnimName = "Side";
                break;
        }

        if (!string.IsNullOrEmpty(baseAnimName))
        {
            GetComponent<ObjectAnimation>().PlayAnimation(baseAnimName);
        }

        attackCoroutine = null;
    }

    void StopAllAttacks()
    {
        // Arrêter l'attaque du bower principal
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (postAttackCoroutine != null)
        {
            StopCoroutine(postAttackCoroutine);
            postAttackCoroutine = null;
        }

        // Arrêter les attaques de tous les clones
        foreach (var clone in bowerClones)
        {
            if (clone != null)
            {
                DarkBowerBehiavor cloneBehavior = clone.GetComponent<DarkBowerBehiavor>();
                if (cloneBehavior != null && cloneBehavior.attackCoroutine != null)
                {
                    cloneBehavior.StopCoroutine(cloneBehavior.attackCoroutine);
                    cloneBehavior.attackCoroutine = null;
                }
            }
        }
    }

    void UpdateOrientation()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
            return;

        Vector2 playerPos = PlayerManager.instance.player.transform.position;
        Vector2 myPos = transform.position;
        Vector2 direction = playerPos - myPos;

        // Déterminer l'orientation selon l'angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Orientation newOrientation;

        // Diviser en 4 quadrants (45° de chaque côté)
        if (angle >= -45f && angle < 45f)
        {
            // Droite
            newOrientation = Orientation.Right;
        }
        else if (angle >= 45f && angle < 135f)
        {
            // Haut
            newOrientation = Orientation.Up;
        }
        else if (angle >= -135f && angle < -45f)
        {
            // Bas
            newOrientation = Orientation.Down;
        }
        else
        {
            // Gauche
            newOrientation = Orientation.Left;
        }

        actualOrientation = newOrientation;
        ApplyOrientation();
    }

    void ApplyOrientation()
    {
        if (objectAnimation == null || spriteRenderer == null)
            return;

        switch (actualOrientation)
        {
            case Orientation.Down:
                objectAnimation.PlayAnimation("Down");
                spriteRenderer.flipX = false;
                break;

            case Orientation.Up:
                objectAnimation.PlayAnimation("Up");
                spriteRenderer.flipX = false;
                break;

            case Orientation.Left:
                objectAnimation.PlayAnimation("Side");
                spriteRenderer.flipX = false;
                break;

            case Orientation.Right:
                objectAnimation.PlayAnimation("Side");
                spriteRenderer.flipX = true;
                break;
        }
    }

    IEnumerator HitRoutine()
    {
        isProcessingHit = true;

        // Stopper toutes les attaques en cours
        StopAllAttacks();

        Disappear();
        yield return new WaitForSeconds(2);
        Appear();
        isProcessingHit = false;
    }

    IEnumerator PostAttackDisappear()
    {
        yield return new WaitForSeconds(3);

        // Vérifier si on n'est pas déjà en train de traiter un hit
        if (!isProcessingHit)
        {
            Disappear();
            yield return new WaitForSeconds(2);
            Appear();
        }

        postAttackCoroutine = null;
    }

    void GenerateValidPoints()
    {
        validPoints.Clear();
        int maxAttempts = 500;
        int attempts = 0;

        while (validPoints.Count < pointCount && attempts < maxAttempts)
        {
            attempts++;

            // Génération d'un point aléatoire dans un cercle autour de l'entité
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector2 candidate = (Vector2)transform.position + randomOffset;

            // Vérifie s'il y a un collider non-trigger à proximité (rayon checkRadius)
            Collider2D[] colliders = Physics2D.OverlapCircleAll(candidate, checkRadius);
            bool hasObstacle = false;

            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    hasObstacle = true;
                    break;
                }
            }

            if (!hasObstacle)
            {
                validPoints.Add(candidate);
            }
        }

        Debug.Log($"Generated {validPoints.Count} valid points after {attempts} attempts.");
    }

    // Filtre les points qui sont à une distance minimale du joueur
    List<Vector2> GetSafePointsAwayFromPlayer(List<Vector2> points, float minDistance)
    {
        List<Vector2> safePoints = new List<Vector2>();

        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
        {
            // Si le joueur n'est pas disponible, retourner tous les points
            return new List<Vector2>(points);
        }

        Vector2 playerPos = PlayerManager.instance.player.transform.position;

        foreach (Vector2 point in points)
        {
            float distance = Vector2.Distance(point, playerPos);
            if (distance >= minDistance)
            {
                safePoints.Add(point);
            }
        }

        return safePoints;
    }

    // Trouve le point le plus éloigné du joueur (fallback)
    Vector2 GetFarthestPointFromPlayer(List<Vector2> points)
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null || points.Count == 0)
        {
            return points[Random.Range(0, points.Count)];
        }

        Vector2 playerPos = PlayerManager.instance.player.transform.position;
        Vector2 farthestPoint = points[0];
        float maxDistance = 0f;

        foreach (Vector2 point in points)
        {
            float distance = Vector2.Distance(point, playerPos);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestPoint = point;
            }
        }

        return farthestPoint;
    }

    // Méthode pour faire disparaître le sprite
    public void Disappear()
    {
        if (spriteRenderer != null)
            StartCoroutine(FadeTo(0f, 0.5f));

        if (stats != null)
            stats.isVulnerable = false;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        if (!bowerClone)
        {
            // Nettoyer la liste en supprimant les références nulles
            bowerClones.RemoveAll(clone => clone == null);

            foreach (var clone in bowerClones)
            {
                if (clone != null)
                {
                    DarkBowerBehiavor cloneBehavior = clone.GetComponent<DarkBowerBehiavor>();
                    if (cloneBehavior != null)
                        cloneBehavior.Disappear();
                }
            }

            bowerClones.Clear();
        }
    }

    // Méthode pour faire apparaître le sprite
    public void Appear()
    {
        if (!bowerClone)
        {
            if (validPoints.Count > 0)
            {
                // Filtrer les points qui sont à au moins 2 unités du joueur
                List<Vector2> safePoints = GetSafePointsAwayFromPlayer(validPoints, 2f);

                if (safePoints.Count > 0)
                {
                    // Téléportation uniquement pour l'entité principale
                    Vector2 newPos = safePoints[Random.Range(0, safePoints.Count)];
                    transform.position = newPos;
                }
                else
                {
                    // Si aucun point n'est assez éloigné, prendre le plus éloigné disponible
                    Vector2 newPos = GetFarthestPointFromPlayer(validPoints);
                    transform.position = newPos;
                }
            }
        }
        else
        {
            // Pour les clones, on ne les téléporte pas, ils restent à leur position de création
        }

        if (spriteRenderer != null)
            StartCoroutine(FadeTo(1f, 0.5f));

        if (stats != null)
            stats.isVulnerable = true;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;

        // Mettre à jour l'orientation à l'apparition
        UpdateOrientation();

        GetComponent<SoundContainer>().PlaySound("Appear", 2);

        // Créer les clones avant de lancer les attaques (seulement pour le bower principal)
        if (!bowerClone && bowerClonePrefab != null)
        {
            int cloneCount = Random.Range(2, 8);

            // Filtrer les points valides pour les clones aussi
            List<Vector2> safeClonePoints = GetSafePointsAwayFromPlayer(validPoints, 2f);

            // Si aucun point sûr, utiliser tous les points disponibles
            if (safeClonePoints.Count == 0)
                safeClonePoints = new List<Vector2>(validPoints);

            for (int i = 0; i < cloneCount; i++)
            {
                if (safeClonePoints.Count == 0) break;

                Vector2 clonePos = safeClonePoints[Random.Range(0, safeClonePoints.Count)];
                GameObject bowerCloneInstance = Instantiate(bowerClonePrefab, clonePos, Quaternion.identity);

                // Marquer l'instance comme clone pour qu'elle ne crée pas d'autres clones
                DarkBowerBehiavor cloneBehavior = bowerCloneInstance.GetComponent<DarkBowerBehiavor>();
                if (cloneBehavior != null)
                {
                    cloneBehavior.bowerClone = true;
                    cloneBehavior.mainBower = this; // Référence au bower principal
                    bowerClones.Add(bowerCloneInstance);

                    // Mettre à jour l'orientation du clone
                    cloneBehavior.UpdateOrientation();
                }
                else
                {
                    Debug.LogWarning("Clone prefab missing DarkBowerBehiavor component!");
                    Destroy(bowerCloneInstance);
                }
            }
        }
        else if (!bowerClone && bowerClonePrefab == null)
        {
            Debug.LogWarning("bowerClonePrefab is not assigned!");
        }

        // Lancer l'attaque pour le bower principal et tous les clones
        attackCoroutine = StartCoroutine(Attack());

        // Lancer les attaques des clones (seulement pour le bower principal)
        if (!bowerClone)
        {
            foreach (var clone in bowerClones)
            {
                if (clone != null)
                {
                    DarkBowerBehiavor cloneBehavior = clone.GetComponent<DarkBowerBehiavor>();
                    if (cloneBehavior != null)
                    {
                        cloneBehavior.attackCoroutine = cloneBehavior.StartCoroutine(cloneBehavior.Attack());
                    }
                }
            }

            // Démarrer le timer de 3 secondes après l'attaque
            postAttackCoroutine = StartCoroutine(PostAttackDisappear());
        }
    }

    // Coroutine pour gérer la transition d'opacité
    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (spriteRenderer == null) yield break;

        float startAlpha = spriteRenderer.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(
                    spriteRenderer.color.r,
                    spriteRenderer.color.g,
                    spriteRenderer.color.b,
                    alpha
                );
            }

            yield return null;
        }

        // S'assurer que la valeur finale est exactement targetAlpha
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(
                spriteRenderer.color.r,
                spriteRenderer.color.g,
                spriteRenderer.color.b,
                targetAlpha
            );
        }

        if (bowerClone && targetAlpha == 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Nettoyer les clones si le bower principal est détruit
        if (!bowerClone)
        {
            foreach (var clone in bowerClones)
            {
                if (clone != null)
                    Destroy(clone);
            }
            bowerClones.Clear();
        }
    }
}