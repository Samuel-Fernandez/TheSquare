using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // Nouveau système de téléportation plus robuste - PAR SCÈNE
    private static readonly Dictionary<int, Dictionary<Transform, TeleportInfo>> playerTeleportInfoByScene = new();

    // NOUVEAU : Tracking des plateformes actives par joueur
    private static readonly Dictionary<int, Dictionary<Transform, HashSet<PlatformBehavior>>> activePlatformsByScene = new();

    private static bool isCleanupRegistered = false;

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

    private void Awake()
    {
        // Enregistrer le callback de nettoyage de scène une seule fois
        if (!isCleanupRegistered)
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            isCleanupRegistered = true;
        }
    }

    private void Start()
    {
        lastPosition = transform.position;

        // S'assurer que les dictionnaires existent pour cette scène
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (!playerTeleportInfoByScene.ContainsKey(sceneIndex))
        {
            playerTeleportInfoByScene[sceneIndex] = new Dictionary<Transform, TeleportInfo>();
        }
        if (!activePlatformsByScene.ContainsKey(sceneIndex))
        {
            activePlatformsByScene[sceneIndex] = new Dictionary<Transform, HashSet<PlatformBehavior>>();
        }
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        // Nettoyer les données de téléportation pour cette scène
        if (playerTeleportInfoByScene.ContainsKey(scene.buildIndex))
        {
            playerTeleportInfoByScene.Remove(scene.buildIndex);
        }
        if (activePlatformsByScene.ContainsKey(scene.buildIndex))
        {
            activePlatformsByScene.Remove(scene.buildIndex);
        }
    }

    private Dictionary<Transform, TeleportInfo> GetCurrentSceneTeleportInfo()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (!playerTeleportInfoByScene.ContainsKey(sceneIndex))
        {
            playerTeleportInfoByScene[sceneIndex] = new Dictionary<Transform, TeleportInfo>();
        }
        return playerTeleportInfoByScene[sceneIndex];
    }

    private Dictionary<Transform, HashSet<PlatformBehavior>> GetCurrentSceneActivePlatforms()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (!activePlatformsByScene.ContainsKey(sceneIndex))
        {
            activePlatformsByScene[sceneIndex] = new Dictionary<Transform, HashSet<PlatformBehavior>>();
        }
        return activePlatformsByScene[sceneIndex];
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

        // NOUVEAU : Ajouter cette plateforme à la liste des plateformes actives
        var activePlatforms = GetCurrentSceneActivePlatforms();
        if (!activePlatforms.ContainsKey(other.transform))
        {
            activePlatforms[other.transform] = new HashSet<PlatformBehavior>();
        }
        activePlatforms[other.transform].Add(this);

        // Gestion du système de comptage de contacts
        if (!contactCounts.ContainsKey(rb))
        {
            contactCounts[rb] = 0;
            var stats = other.GetComponent<Stats>();
            if (stats != null)
            {
                if (stats.entityType == EntityType.Player)
                {
                    var playerController = other.GetComponent<PlayerController>();
                    playerController.cantFall = true;

                    // Nouveau système de téléportation
                    SetPlayerTeleportPosition(other.transform, playerController);
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

        // NOUVEAU : Retirer cette plateforme de la liste des plateformes actives
        var activePlatforms = GetCurrentSceneActivePlatforms();
        if (activePlatforms.ContainsKey(other.transform))
        {
            activePlatforms[other.transform].Remove(this);
            if (activePlatforms[other.transform].Count == 0)
            {
                activePlatforms.Remove(other.transform);
            }
        }

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
                    var playerController = other.GetComponent<PlayerController>();

                    // Nettoyer le système de téléportation et mettre à jour
                    RemovePlayerTeleportPosition(other.transform, playerController);

                    // MODIFIÉ : Désactiver cantFall seulement s'il n'y a plus AUCUNE plateforme
                    if (!activePlatforms.ContainsKey(other.transform) || activePlatforms[other.transform].Count == 0)
                    {
                        playerController.cantFall = false;
                        Debug.Log($"Platform: cantFall désactivé, safeTeleportation = {(playerController.safeTeleportation != null ? playerController.safeTeleportation.position.ToString() : "null")}");
                    }
                    else
                    {
                        Debug.Log($"Platform: cantFall reste actif, {activePlatforms[other.transform].Count} plateforme(s) restante(s)");
                    }
                }
                else if (stats.entityType == EntityType.Monster)
                    other.GetComponent<HoleCollider>().canFallInHoles = true;
            }
        }
    }

    private void SetPlayerTeleportPosition(Transform playerTransform, PlayerController playerController)
    {
        if (teleportPosition == null) return;

        var playerTeleportInfo = GetCurrentSceneTeleportInfo();

        // Vérifier s'il y a déjà une info de téléportation
        if (playerTeleportInfo.ContainsKey(playerTransform))
        {
            var currentInfo = playerTeleportInfo[playerTransform];

            // Si cette plateforme a une priorité plus élevée, remplacer
            if (teleportPriority > currentInfo.priority)
            {
                playerTeleportInfo[playerTransform] = new TeleportInfo(teleportPosition, teleportPriority, this);
                playerController.safeTeleportation = teleportPosition;
                Debug.Log($"Platform {gameObject.name}: safeTeleportation mis à jour (priorité +) = {teleportPosition.position}");
            }
            // Si même priorité, garder la plus récente
            else if (teleportPriority == currentInfo.priority)
            {
                playerTeleportInfo[playerTransform] = new TeleportInfo(teleportPosition, teleportPriority, this);
                playerController.safeTeleportation = teleportPosition;
                Debug.Log($"Platform {gameObject.name}: safeTeleportation mis à jour (même priorité) = {teleportPosition.position}");
            }
        }
        else
        {
            // Première plateforme pour ce joueur
            playerTeleportInfo[playerTransform] = new TeleportInfo(teleportPosition, teleportPriority, this);
            playerController.safeTeleportation = teleportPosition;
            Debug.Log($"Platform {gameObject.name}: safeTeleportation initialisé = {teleportPosition.position}");
        }
    }

    private void RemovePlayerTeleportPosition(Transform playerTransform, PlayerController playerController)
    {
        var playerTeleportInfo = GetCurrentSceneTeleportInfo();
        var activePlatforms = GetCurrentSceneActivePlatforms();

        if (!playerTeleportInfo.ContainsKey(playerTransform)) return;

        var currentInfo = playerTeleportInfo[playerTransform];

        // Ne supprimer que si c'est cette plateforme qui était active
        if (currentInfo.sourcePlatform == this)
        {
            Debug.Log($"Platform {gameObject.name}: Suppression de safeTeleportation de cette plateforme");

            playerTeleportInfo.Remove(playerTransform);

            // CORRIGÉ : Chercher parmi les plateformes ENCORE ACTIVES
            TeleportInfo bestInfo = null;

            if (activePlatforms.ContainsKey(playerTransform))
            {
                foreach (var platform in activePlatforms[playerTransform])
                {
                    // Ignorer cette plateforme (celle qui vient d'être quittée)
                    if (platform == this || platform.teleportPosition == null)
                        continue;

                    var info = new TeleportInfo(platform.teleportPosition, platform.teleportPriority, platform);

                    if (bestInfo == null || info.priority > bestInfo.priority ||
                        (info.priority == bestInfo.priority && info.lastUpdateTime > bestInfo.lastUpdateTime))
                    {
                        bestInfo = info;
                    }
                }
            }

            // Mettre à jour safeTeleportation
            if (bestInfo != null)
            {
                playerTeleportInfo[playerTransform] = bestInfo;
                playerController.safeTeleportation = bestInfo.teleportPosition;
                Debug.Log($"Platform: Nouvelle safeTeleportation trouvée = {bestInfo.teleportPosition.position} (priorité {bestInfo.priority})");
            }
            else
            {
                playerController.safeTeleportation = null;
                Debug.Log("Platform: safeTeleportation remis à null (aucune autre plateforme)");
            }
        }
        else
        {
            Debug.Log($"Platform {gameObject.name}: Pas la plateforme active, pas de suppression");
        }
    }

    private void OnDestroy()
    {
        // Nettoyer les références à cette plateforme
        var playerTeleportInfo = GetCurrentSceneTeleportInfo();
        var activePlatforms = GetCurrentSceneActivePlatforms();
        var keysToUpdate = new List<Transform>();

        foreach (var kvp in playerTeleportInfo)
        {
            if (kvp.Value.sourcePlatform == this)
            {
                keysToUpdate.Add(kvp.Key);
            }
        }

        foreach (var key in keysToUpdate)
        {
            if (key != null && key.TryGetComponent<PlayerController>(out var playerController))
            {
                RemovePlayerTeleportPosition(key, playerController);
            }
        }

        // Nettoyer des listes de plateformes actives
        var playersToClean = new List<Transform>();
        foreach (var kvp in activePlatforms)
        {
            if (kvp.Value.Contains(this))
            {
                playersToClean.Add(kvp.Key);
            }
        }

        foreach (var player in playersToClean)
        {
            activePlatforms[player].Remove(this);
            if (activePlatforms[player].Count == 0)
            {
                activePlatforms.Remove(player);
            }
        }
    }

    // Méthode statique pour nettoyer les références obsolètes
    public static void CleanupTeleportReferences()
    {
        foreach (var sceneDict in playerTeleportInfoByScene.Values)
        {
            var keysToRemove = new List<Transform>();

            foreach (var kvp in sceneDict)
            {
                if (kvp.Key == null || kvp.Value.sourcePlatform == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                sceneDict.Remove(key);

                if (key != null && key.TryGetComponent<PlayerController>(out var playerController))
                {
                    playerController.safeTeleportation = null;
                }
            }
        }
    }
}