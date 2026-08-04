using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundButton : MonoBehaviour
{
    public string id;
    public List<GameObject> logicalEntites;
    public bool isOneShot;

    public Sprite offSprite;
    public Sprite onSprite;

    public bool playerInteraction = false;

    public bool isOn;
    private HashSet<GameObject> activeObjects = new HashSet<GameObject>();

    SpriteRenderer spriteRenderer;

    // Syst�me de synchronisation multi-boutons
    private static Dictionary<GameObject, List<GroundButton>> sharedEntityButtons = new Dictionary<GameObject, List<GroundButton>>();

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Si pas d'entit�s logiques -> pas d'erreur bloquante, mais on continue
        if (logicalEntites == null || logicalEntites.Count == 0 || logicalEntites[0] == null)
        {
            Debug.LogWarning($"[GroundButton:{name}] Aucun logicalEntity assign� -> le bouton sera pressable mais n'activera rien.");
        }
        else
        {
            // Enregistrer ce bouton pour chaque entit� logique qu'il contr�le
            RegisterSharedEntities();

            // Initialisation de l'eventContainer (event camera move)
            UpdateEventContainer();
        }

        if (isOneShot)
        {
            bool state;
            SaveManager.instance.twoStateContainer.TryGetState(id, out state);

            if (state)
            {
                isOn = true;
                spriteRenderer.sprite = onSprite;

                // Ex�cute seulement si des entit�s existent
                if (logicalEntites != null && logicalEntites.Count > 0 && logicalEntites[0] != null)
                    ToggleGenericEntities();

                GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        // Nettoyer les r�f�rences lors de la destruction
        UnregisterSharedEntities();
    }

    // Enregistre ce bouton dans le dictionnaire pour chaque entit� partag�e
    private void RegisterSharedEntities()
    {
        if (logicalEntites == null) return;

        foreach (GameObject entity in logicalEntites)
        {
            if (entity == null) continue;

            if (!sharedEntityButtons.ContainsKey(entity))
            {
                sharedEntityButtons[entity] = new List<GroundButton>();
            }

            if (!sharedEntityButtons[entity].Contains(this))
            {
                sharedEntityButtons[entity].Add(this);
            }
        }

        // Debug: afficher les entit�s partag�es
        Debug.Log($"[{name}] Registered entities:");
        foreach (GameObject entity in logicalEntites)
        {
            if (entity != null && sharedEntityButtons.ContainsKey(entity))
            {
                Debug.Log($"  - {entity.name}: shared with {sharedEntityButtons[entity].Count} buttons");
            }
        }
    }

    // Retire ce bouton du dictionnaire
    private void UnregisterSharedEntities()
    {
        if (logicalEntites == null) return;

        foreach (GameObject entity in logicalEntites)
        {
            if (entity == null) continue;

            if (sharedEntityButtons.ContainsKey(entity))
            {
                sharedEntityButtons[entity].Remove(this);

                if (sharedEntityButtons[entity].Count == 0)
                {
                    sharedEntityButtons.Remove(entity);
                }
            }
        }
    }

    // V�rifie si une entit� est partag�e avec d'autres boutons
    private bool IsEntityShared(GameObject entity)
    {
        return sharedEntityButtons.ContainsKey(entity) && sharedEntityButtons[entity].Count > 1;
    }

    // V�rifie si tous les boutons partageant cette entit� sont activ�s
    private bool AreAllSharedButtonsActive(GameObject entity)
    {
        if (!sharedEntityButtons.ContainsKey(entity))
            return true;

        return sharedEntityButtons[entity].All(button => button.isOn);
    }

    // V�rifie si ce bouton est le dernier � �tre activ� pour une entit� partag�e
    private bool IsLastButtonActivated(GameObject entity)
    {
        if (!sharedEntityButtons.ContainsKey(entity))
            return true;

        var buttons = sharedEntityButtons[entity];
        int activeCount = buttons.Count(button => button.isOn);

        return activeCount == buttons.Count;
    }


    // Nouvelle m�thode pour appliquer l'�tat uniquement aux entit�s g�n�riques
    private void ToggleGenericEntities()
    {
        foreach (GameObject entity in logicalEntites)
        {
            if (entity == null) continue;

            bool hasSpecificScript = entity.GetComponent<DoorBehiavor>() != null ||
                                     entity.GetComponent<Spades>() != null ||
                                     entity.GetComponent<SkeletonBridgeBehiavor>() != null ||
                                     entity.GetComponent<RustCircleBehiavor>() != null;

            if (!hasSpecificScript)
            {
                entity.SetActive(!entity.activeSelf);
            }
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


    private void OnTriggerStay2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;

        bool isValidPlayer = obj.GetComponent<Stats>() != null && obj.GetComponent<Stats>().entityType == EntityType.Player;
        bool isValidCrate = obj.GetComponent<WoodenCrateBehavior>() != null && CheckDistanceCrate(obj);
        RockBoulderBehiavor boulder = obj.GetComponent<RockBoulderBehiavor>();
        bool isValidBoulder = boulder != null && !boulder.IsSliding && CheckDistanceCrate(obj);

        if (!activeObjects.Contains(obj) && (isValidPlayer || isValidCrate || isValidBoulder))
        {
            activeObjects.Add(obj);

            if (activeObjects.Count == 1)
            {
                obj.transform.position = transform.position + new Vector3(0, 0.5f, 0);
                playerInteraction = true;

                // Met � jour l'eventContainer � chaque activation
                UpdateEventContainer();

                ToggleButton();

                // Ne pas jouer l'�v�nement ici, il sera g�r� dans ToggleLogicalEntities

                if (isOneShot)
                    GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;

        bool isValidPlayer = obj.GetComponent<Stats>() != null && obj.GetComponent<Stats>().entityType == EntityType.Player;
        bool isValidCrate = obj.GetComponent<WoodenCrateBehavior>() != null && !CheckDistanceCrate(obj);
        RockBoulderBehiavor boulder = obj.GetComponent<RockBoulderBehiavor>();
        bool isValidBoulder = boulder != null && !CheckDistanceCrate(obj);

        if (activeObjects.Contains(obj))
        {
            activeObjects.Remove(obj);

            if (isValidPlayer)
                StartCoroutine(WaitPlayerRoutine());

            if (activeObjects.Count == 0 && !isOneShot)
            {
                playerInteraction = true;
                ToggleButton();
            }
        }
    }

    IEnumerator WaitPlayerRoutine()
    {
        if(!isOneShot)
        {
            PlayerManager.instance.player.GetComponent<Stats>().canMove = false;
            yield return new WaitForSeconds(1f);
            PlayerManager.instance.player.GetComponent<Stats>().canMove = true;
        }
        else
        {
            yield return null;
        }
    }

    bool CheckDistanceCrate(GameObject crate)
    {
        float dist = Vector2.Distance(transform.position, crate.transform.position);
        return dist < 0.1f;
    }

    public void ToggleButton()
    {
        isOn = !isOn;

        GetComponent<SoundContainer>().PlaySound("Pushed", 3);

        if (isOneShot)
            SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, isOn);

        spriteRenderer.sprite = isOn ? onSprite : offSprite;

        if (playerInteraction && logicalEntites.Count > 0)
            StartCoroutine(ToggleLogicalEntities());
    }

    IEnumerator ToggleLogicalEntities()
    {
        bool shouldPlayEvent = false;
        bool hasSharedEntities = logicalEntites.Any(e => e != null && IsEntityShared(e));

        Debug.Log($"[{name}] ToggleLogicalEntities START - playerInteraction: {playerInteraction}, isOn: {isOn}, hasShared: {hasSharedEntities}");

        if (playerInteraction && isOn)
        {
            // Si au moins une entit� est partag�e
            if (hasSharedEntities)
            {
                Debug.Log($"[{name}] Has shared entities, checking if all buttons are active...");

                // V�rifier si TOUTES les entit�s partag�es ont tous leurs boutons activ�s
                bool allSharedEntitiesReady = true;
                foreach (GameObject entity in logicalEntites)
                {
                    if (entity != null && IsEntityShared(entity))
                    {
                        bool ready = AreAllSharedButtonsActive(entity);
                        int buttonCount = sharedEntityButtons[entity].Count;
                        int activeCount = sharedEntityButtons[entity].Count(b => b.isOn);

                        Debug.Log($"[{name}] Entity '{entity.name}' - Active buttons: {activeCount}/{buttonCount}, Ready: {ready}");

                        if (!ready)
                        {
                            allSharedEntitiesReady = false;
                        }
                    }
                }
                shouldPlayEvent = allSharedEntitiesReady;
                Debug.Log($"[{name}] All shared entities ready: {allSharedEntitiesReady}");
            }
            else
            {
                // Aucune entit� partag�e, comportement normal
                Debug.Log($"[{name}] No shared entities, playing event normally");
                shouldPlayEvent = true;
            }

            if (shouldPlayEvent)
            {
                Debug.Log($"[{name}] >>> PLAYING EVENT <<<");
                GetComponent<EventPlayer>().PlayAnimation();
            }
            else
            {
                Debug.Log($"[{name}] Event NOT played (waiting for other buttons)");
            }

            playerInteraction = false;
        }

        yield return new WaitForSecondsRealtime(1.5f);

        foreach (GameObject entity in logicalEntites)
        {
            if (entity == null) continue;

            // Si l'entit� est partag�e, ne l'activer que si tous les boutons sont press�s
            if (IsEntityShared(entity) && !AreAllSharedButtonsActive(entity))
            {
                Debug.Log($"[{name}] Skipping entity '{entity.name}' (not all buttons active)");
                continue; // Passer � l'entit� suivante sans activer celle-ci
            }

            Debug.Log($"[{name}] Activating entity '{entity.name}'");

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

        yield return new WaitForSecondsRealtime(1.5f);
    }
}