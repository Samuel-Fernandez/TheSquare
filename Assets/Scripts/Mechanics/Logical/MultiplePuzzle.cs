using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplePuzzle : MonoBehaviour
{
    [Header("Identifiant unique pour la sauvegarde")]
    public string id;

    [Header("Objets nécessitant une activation simultanée")]
    public List<GameObject> logicalObjects; 
    public List<GameObject> logicalEntites; // Objets affectés quand l'énigme est réussie

    [Header("Paramètres du Puzzle")]
    [Tooltip("Si vrai, tous les objets doivent rester activés. Si un se désactive, le puzzle s'invalide.")]
    public bool requireContinuousActivation = false;

    private bool isSolved = false;

    private void Start()
    {
        // Charger l'état sauvegardé
        bool savedState;
        if (SaveManager.instance.twoStateContainer.TryGetState(id, out savedState) && savedState)
        {
            Debug.Log($"[MultiplePuzzle:{id}] Énigme déjà résolue (chargée depuis sauvegarde)");
            isSolved = true;
            ActivateLogicalEntities();
            
            if (!requireContinuousActivation)
            {
                enabled = false; // plus besoin de surveiller si ce n'est pas continu
            }
        }
    }

    private void Update()
    {
        // Si le puzzle est résolu et qu'on ne demande pas de vérification continue, on ne fait rien
        if (!requireContinuousActivation && isSolved)
            return;

        int activeCount = 0;
        foreach (var obj in logicalObjects)
        {
            var button = obj.GetComponent<GroundButton>();
            if (button != null && button.isOn)
            {
                activeCount++;
            }
        }

        bool allActive = (activeCount == logicalObjects.Count);

        // Si tout est activé et que ce n'était pas résolu
        if (allActive && !isSolved)
        {
            isSolved = true;
            SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, true);
            Debug.Log($"[MultiplePuzzle:{id}] Énigme résolue !");

            // Si c'est un one-shot (non continu), on joue la cinématique s'il y en a une
            if (!requireContinuousActivation)
            {
                UpdateEventContainer();
                if (logicalEntites.Count > 0 && TryGetComponent<EventPlayer>(out var eventPlayer))
                {
                    eventPlayer.PlayAnimation();
                }
            }

            ActivateLogicalEntities();

            if (!requireContinuousActivation)
            {
                enabled = false;
            }
        }
        // Si la vérification continue est active et qu'un bouton s'est relâché
        else if (!allActive && isSolved && requireContinuousActivation)
        {
            isSolved = false;
            SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, false);
            Debug.Log($"[MultiplePuzzle:{id}] Énigme invalidée ! Au moins un objet s'est désactivé.");
            
            DeactivateLogicalEntities();
        }
    }

    private void UpdateEventContainer()
    {
        if (logicalEntites == null || logicalEntites.Count == 0 || logicalEntites[0] == null)
            return;

        Vector3 logicPos = logicalEntites[0].transform.position;
        Vector3 btnPos = transform.position;
        Vector3 relative = logicPos - btnPos;

        if (TryGetComponent<EventPlayer>(out var eventPlayer))
        {
            eventPlayer.eventContainer = EventGeneratorManager.instance.MoveCamera(Vector2.zero, relative, 1f, 2f);
        }
    }

    private void ActivateLogicalEntities()
    {
        foreach (GameObject entity in logicalEntites)
        {
            if (entity == null) continue;

            if (entity.GetComponent<DoorBehiavor>() is DoorBehiavor door)
            {
                if (!door.isOpen) door.OpenDoor();
            }
            else if (entity.GetComponent<Spades>() is Spades spades)
            {
                StartCoroutine(spades.RoutineSpades());
            }
            else if (entity.GetComponent<SkeletonBridgeBehiavor>() is SkeletonBridgeBehiavor bridge)
            {
                bridge.Activate();
            }
            else
            {
                entity.SetActive(true); // On force l'activation au lieu d'un toggle
            }
        }
    }

    private void DeactivateLogicalEntities()
    {
        foreach (GameObject entity in logicalEntites)
        {
            if (entity == null) continue;

            if (entity.GetComponent<DoorBehiavor>() is DoorBehiavor door)
            {
                if (door.isOpen) door.CloseDoor();
            }
            else if (entity.GetComponent<Spades>() is Spades spades)
            {
                // Pas de routine inverse par défaut connue pour les piques dans ce contexte
            }
            else if (entity.GetComponent<SkeletonBridgeBehiavor>() is SkeletonBridgeBehiavor bridge)
            {
                // bridge.Deactivate(); // A décommenter si SkeletonBridgeBehiavor possède une méthode Deactivate()
            }
            else
            {
                entity.SetActive(false); // On force la désactivation
            }
        }
    }
}
