using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlatformBehavior : MonoBehaviour
{
    [Header("Platform Group")]
    [Tooltip("Toutes les plateformes avec le même ID de groupe bougent ensemble")]
    public int platformGroupId = 0;

    [Header("Teleportation")]
    [Tooltip("Position de téléportation si le joueur tombe de cette plateforme")]
    public Transform teleportPosition;

    [Tooltip("Priorité de téléportation (plus élevé = prioritaire)")]
    public int teleportPriority = 0;

    private Vector3 lastPosition;
    private Vector3 deltaMovement;
    private readonly HashSet<Transform> transformsOnPlatform = new();
    private static readonly Dictionary<Rigidbody2D, int> contactCounts = new();

    // Système global pour éviter les mouvements multiples PAR GROUPE
    private static readonly Dictionary<int, HashSet<Transform>> entitiesMovedByGroup = new();
    private static readonly Dictionary<int, Vector3> groupMovements = new();
    private static int currentFrame = -1;

    // Nouveau système de téléportation plus robuste
    private static readonly Dictionary<Transform, TeleportInfo> playerTeleportInfo = new();

    [System.Serializable]
    private class TeleportInfo
    {
        public Transform teleportPosition;
        public int priority;
        public float lastUpdateTime;
        public PlatformBehavior sourcePlatform;

        public TeleportInfo(Transform teleportPos, int prio, PlatformBehavior platform)
        {
            teleportPosition = teleportPos;
            priority = prio;
            lastUpdateTime = Time.time;
            sourcePlatform = platform;
        }
    }

    private void Start()
    {
        lastPosition = transform.position;
    }

    // Utilisation de LateUpdate pour s'exécuter après tous les mouvements des entités
    private void LateUpdate()
    {
        // Réinitialiser les listes si on est dans une nouvelle frame
        if (Time.frameCount != currentFrame)
        {
            entitiesMovedByGroup.Clear();
            groupMovements.Clear();
            currentFrame = Time.frameCount;
        }

        // Calculer le déplacement de la plateforme depuis la dernière frame
        deltaMovement = transform.position - lastPosition;

        // Si la plateforme a bougé
        if (deltaMovement.magnitude > 0.0001f)
        {
            // Initialiser le groupe s'il n'existe pas
            if (!entitiesMovedByGroup.ContainsKey(platformGroupId))
            {
                entitiesMovedByGroup[platformGroupId] = new HashSet<Transform>();
                groupMovements[platformGroupId] = deltaMovement;
            }
            else
            {
                // Vérifier que toutes les plateformes du groupe bougent de la même façon
                if ((groupMovements[platformGroupId] - deltaMovement).magnitude > 0.001f)
                {
                    Debug.LogWarning($"Plateformes du groupe {platformGroupId} ont des mouvements différents! " +
                                   $"Attendu: {groupMovements[platformGroupId]}, Reçu: {deltaMovement}");
                }
            }

            var movedEntities = entitiesMovedByGroup[platformGroupId];

            foreach (var entityTransform in transformsOnPlatform)
            {
                if (entityTransform != null && !movedEntities.Contains(entityTransform))
                {
                    // Appliquer directement le mouvement de la plateforme au transform
                    entityTransform.position += deltaMovement;

                    // Marquer cette entité comme déjà déplacée pour ce groupe
                    movedEntities.Add(entityTransform);
                }
            }
        }

        // Mettre à jour la position de référence
        lastPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlatformBehavior>())
            return;

        var stats1 = other.GetComponent<Stats>();
        if (stats1 == null)
            return;

        var rb = other.attachedRigidbody;
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic)
            return;

        // Ajouter le transform à la liste des entités sur la plateforme
        transformsOnPlatform.Add(other.transform);

        // Gestion du système de comptage de contacts
        if (!contactCounts.ContainsKey(rb))
        {
            contactCounts[rb] = 0;
            var stats = other.GetComponent<Stats>();
            if (stats != null)
            {
                if (stats.entityType == EntityType.Player)
                {
                    other.GetComponent<PlayerController>().cantFall = true;

                    // Nouveau système de téléportation
                    SetPlayerTeleportPosition(other.transform, other.GetComponent<PlayerController>());
                }
                else if (stats.entityType == EntityType.Monster)
                    other.GetComponent<HoleCollider>().canFallInHoles = false;
            }
        }
        contactCounts[rb]++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlatformBehavior>())
            return;

        var stats1 = other.GetComponent<Stats>();
        if (stats1 == null)
            return;

        var rb = other.attachedRigidbody;
        if (rb == null || !contactCounts.ContainsKey(rb))
            return;

        // Retirer le transform de la liste
        transformsOnPlatform.Remove(other.transform);

        // Gestion du système de comptage de contacts
        contactCounts[rb]--;
        if (contactCounts[rb] <= 0)
        {
            contactCounts.Remove(rb);
            var stats = other.GetComponent<Stats>();
            if (stats != null)
            {
                if (stats.entityType == EntityType.Player)
                {
                    other.GetComponent<PlayerController>().cantFall = false;

                    // Nettoyer le système de téléportation pour cette plateforme
                    RemovePlayerTeleportPosition(other.transform);
                }
                else if (stats.entityType == EntityType.Monster)
                    other.GetComponent<HoleCollider>().canFallInHoles = true;
            }
        }
    }

    private void SetPlayerTeleportPosition(Transform playerTransform, PlayerController playerController)
    {
        if (teleportPosition == null) return;

        // Vérifier s'il y a déjà une info de téléportation
        if (playerTeleportInfo.ContainsKey(playerTransform))
        {
            var currentInfo = playerTeleportInfo[playerTransform];

            // Si cette plateforme a une priorité plus élevée, remplacer
            if (teleportPriority > currentInfo.priority)
            {
                playerTeleportInfo[playerTransform] = new TeleportInfo(teleportPosition, teleportPriority, this);
                playerController.safeTeleportation = teleportPosition;
            }
            // Si même priorité, garder la plus récente (ce qui gère le cas des changements rapides)
            else if (teleportPriority == currentInfo.priority)
            {
                playerTeleportInfo[playerTransform] = new TeleportInfo(teleportPosition, teleportPriority, this);
                playerController.safeTeleportation = teleportPosition;
            }
        }
        else
        {
            // Première plateforme pour ce joueur
            playerTeleportInfo[playerTransform] = new TeleportInfo(teleportPosition, teleportPriority, this);
            playerController.safeTeleportation = teleportPosition;
        }
    }

    private void RemovePlayerTeleportPosition(Transform playerTransform)
    {
        if (!playerTeleportInfo.ContainsKey(playerTransform)) return;

        var currentInfo = playerTeleportInfo[playerTransform];

        // Ne supprimer que si c'est cette plateforme qui était active
        if (currentInfo.sourcePlatform == this)
        {
            playerTeleportInfo.Remove(playerTransform);

            // Chercher une autre plateforme qui pourrait gérer safeTeleportation
            PlayerController playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Parcourir les autres plateformes encore actives pour ce joueur
                TeleportInfo bestInfo = null;
                foreach (var kvp in playerTeleportInfo)
                {
                    if (kvp.Key == playerTransform)
                    {
                        if (bestInfo == null || kvp.Value.priority > bestInfo.priority)
                            bestInfo = kvp.Value;
                    }
                }

                if (bestInfo != null)
                    playerController.safeTeleportation = bestInfo.teleportPosition;
                else
                    playerController.safeTeleportation = null;
            }
        }
    }


    // Méthode statique pour nettoyer les références obsolètes (à appeler périodiquement si nécessaire)
    public static void CleanupTeleportReferences()
    {
        var keysToRemove = new List<Transform>();

        foreach (var kvp in playerTeleportInfo)
        {
            if (kvp.Key == null || kvp.Value.sourcePlatform == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            playerTeleportInfo.Remove(key);
        }
    }
}