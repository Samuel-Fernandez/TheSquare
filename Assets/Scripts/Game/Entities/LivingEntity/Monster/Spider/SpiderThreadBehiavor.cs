using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpiderThreadBehiavor : MonoBehaviour
{
    public float speed = 12f;
    public float maxDistance = 5f;
    
    private SpiderBehiavor owner;
    private Vector2 startPosition;
    private Vector2 movementDirection;
    
    private float currentDistance = 0f;
    private bool hasHit = false;
    private Transform attachedTarget;

    private BoxCollider2D col;
    private Transform threadVisual;

    public void InitThread(SpiderBehiavor spider, Vector3 targetPosition)
    {
        this.owner = spider;
        this.startPosition = spider.threadSpawnPoint != null ? (Vector2)spider.threadSpawnPoint.position : (Vector2)spider.transform.position;
        this.movementDirection = ((Vector2)targetPosition - startPosition).normalized;
        
        // Rotation (comme le laser, en supposant le sprite vertical par défaut)
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        
        // Le Sprite (ou le GameObject contenant le SpriteRenderer) doit être le 1er enfant
        if (transform.childCount > 0)
        {
            threadVisual = transform.GetChild(0);
        }
    }

    private void Update()
    {
        if (MeteoManager.instance != null && !MeteoManager.instance.time) return;

        // Met à jour l'origine si l'araignée a bougé (dash ou autre mouvement inattendu)
        if (owner != null)
        {
            startPosition = owner.threadSpawnPoint != null ? (Vector2)owner.threadSpawnPoint.position : (Vector2)owner.transform.position;
            transform.position = startPosition;
        }

        if (!hasHit)
        {
            // La "tête" du fil avance
            currentDistance += speed * Time.deltaTime;

            if (currentDistance >= maxDistance)
            {
                if (owner != null)
                {
                    owner.OnThreadMissed();
                }
                Destroy(gameObject);
                return;
            }

            // Mettre à jour visuellement le fil pour qu'il s'allonge
            UpdateThreadVisuals(startPosition, startPosition + movementDirection * currentDistance);
        }
        else if (attachedTarget != null)
        {
            // Le fil est accroché ! Il relie en temps réel l'araignée au joueur Target
            UpdateThreadVisuals(startPosition, attachedTarget.position);
        }
    }

    private void UpdateThreadVisuals(Vector2 a, Vector2 b)
    {
        Vector2 direction = (b - a).normalized;
        float finalDistance = Vector2.Distance(a, b);

        // Rotation dynamique continue
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        if (threadVisual != null)
        {
            // On étire l'enfant sur l'axe Y selon la distance
            Vector3 scale = threadVisual.localScale;
            scale.y = finalDistance;
            threadVisual.localScale = scale;
            
            // Le collider couvre toute la distance
            if (col != null)
            {
                col.isTrigger = true;
                col.size = new Vector2(col.size.x, finalDistance);
                col.offset = new Vector2(0, finalDistance / 2f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        var stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            hasHit = true;
            attachedTarget = collision.transform;
            
            if (owner != null)
            {
                owner.OnThreadHitPlayer(attachedTarget);
            }
        }
        else if (!collision.isTrigger && (owner == null || collision.gameObject != owner.gameObject))
        {
            // Si le fil tape un mur ou un élément de décor solide
            if (owner != null)
            {
                owner.OnThreadMissed();
            }
            Destroy(gameObject);
        }
    }
}
