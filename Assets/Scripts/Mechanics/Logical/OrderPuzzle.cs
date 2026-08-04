using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderPuzzle : MonoBehaviour
{
    [Header("Identifiant unique pour la sauvegarde")]
    public string id;

    [Header("Ordre d�fini par la position dans la liste")]
    public List<GameObject> logicalObjects; // Ordre attendu
    public List<GameObject> logicalEntites; // Objets affect�s quand l'�nigme est r�ussie

    private List<GameObject> pressedOrder = new List<GameObject>();
    private Dictionary<GameObject, bool> previousStates = new Dictionary<GameObject, bool>();

    private void Start()
    {
        // Charger l'�tat sauvegard�
        bool savedState;
        if (SaveManager.instance.twoStateContainer.TryGetState(id, out savedState) && savedState)
        {
            Debug.Log($"[OrderPuzzle:{id}] �nigme d�j� r�solue (charg�e depuis sauvegarde)");
            ActivateLogicalEntities();
            enabled = false; // plus besoin de surveiller
            return;
        }

        // Sauvegarder l'�tat initial de chaque bouton
        foreach (var obj in logicalObjects)
        {
            previousStates[obj] = obj.GetComponent<GroundButton>().isOn;
        }
    }

    private void Update()
    {
        foreach (var obj in logicalObjects)
        {
            var button = obj.GetComponent<GroundButton>();
            bool currentState = button.isOn;

            // D�tection d'un passage False -> True
            if (!previousStates[obj] && currentState)
            {
                pressedOrder.Add(obj);

                // Si on a atteint le nombre total de boutons attendus
                if (pressedOrder.Count == logicalObjects.Count)
                {
                    CheckOrder();
                }
            }

            // Mettre � jour l'�tat pr�c�dent
            previousStates[obj] = currentState;
        }
    }

    private void CheckOrder()
    {
        bool correct = true;

        for (int i = 0; i < logicalObjects.Count; i++)
        {
            if (pressedOrder[i] != logicalObjects[i])
            {
                correct = false;
                break;
            }
        }

        if (correct)
        {
            SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, true);
            Debug.Log($"[OrderPuzzle:{id}] �nigme r�solue !");

            // Met � jour l�eventContainer avant de jouer
            UpdateEventContainer();

            // D�clenche la cam�ra si EventPlayer est pr�sent
            if (logicalEntites.Count > 0 && TryGetComponent<EventPlayer>(out var eventPlayer))
            {
                eventPlayer.PlayAnimation();
            }

            ActivateLogicalEntities();
            enabled = false;
        }
        else
        {
            Debug.Log($"[OrderPuzzle:{id}] Mauvais ordre, reset dans 2.5 secondes...");
            StartCoroutine(ResetAfterDelay(2.5f));
        }
    }

    private void UpdateEventContainer()
    {
        if (logicalEntites == null || logicalEntites.Count == 0 || logicalEntites[0] == null)
            return;

        Vector3 logicPos = logicalEntites[0].transform.position;
        Vector3 btnPos = transform.position;
        Vector3 relative = logicPos - btnPos;

        GetComponent<EventPlayer>().eventContainer =
            EventGeneratorManager.instance.MoveCamera(Vector2.zero, relative, 1f, 2f);
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetButtons();
    }

    private void ResetButtons()
    {
        pressedOrder.Clear();

        foreach (var obj in logicalObjects)
        {
            obj.GetComponent<GroundButton>().ToggleButton(); // on repart � z�ro
            obj.GetComponent<Collider2D>().enabled = true;
            previousStates[obj] = false;
        }
    }

    private void ActivateLogicalEntities()
    {
        foreach (GameObject entity in logicalEntites)
        {
            if (entity == null) continue;

            if (entity.GetComponent<DoorBehiavor>() is DoorBehiavor door)
            {
                if (door.isOpen) door.CloseDoor();
                else door.OpenDoor();
            }
            else if (entity.GetComponent<Spades>() is Spades spades)
            {
                StartCoroutine(spades.RoutineSpades());
            }
            else if (entity.GetComponent<SkeletonBridgeBehiavor>() is SkeletonBridgeBehiavor bridge)
            {
                bridge.Activate();
            }
            else if (entity.GetComponent<RustCircleBehiavor>() is RustCircleBehiavor rustCircle)
            {
                rustCircle.Toggle();
            }
            else
            {
                entity.SetActive(!entity.activeSelf);
            }
        }
    }
}
