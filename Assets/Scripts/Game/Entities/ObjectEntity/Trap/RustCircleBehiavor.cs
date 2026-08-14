using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class RustCircleBehiavor : MonoBehaviour
{
    [Header("State")]
    public string id;
    public bool isOn = false;

    [Header("Rotation")]
    public float rotationSpeed = 90f;
    public bool clockwise = true;
    public float turnOnTransitionDuration = 1f;

    private class CarriedObject
    {
        public Transform transform;
        public Rigidbody2D rb;
    }

    private Collider2D circleCollider;
    private SoundContainer soundContainer;

    private float currentRotationSpeed = 0f;
    private Coroutine transitionCoroutine;

    private bool isOverHole = false;

    private readonly List<CarriedObject> carriedObjects = new List<CarriedObject>();

    private Transform carriedPlayer;
    private PlayerController carriedPlayerController;

    // Plusieurs RustCircle peuvent se chevaucher sous le joueur en même temps (transition d'un cercle à l'autre) :
    // on agrège leur isOverHole via ce registre partagé pour éviter qu'un cercle qui n'est pas au-dessus du trou
    // n'écrase, dans son propre LateUpdate, la protection accordée par un autre cercle qui l'est.
    private static readonly Dictionary<PlayerController, List<RustCircleBehiavor>> circlesByPlayer = new Dictionary<PlayerController, List<RustCircleBehiavor>>();

    private static void RecomputeCantFall(PlayerController controller)
    {
        if (controller == null) return;

        bool protect = circlesByPlayer.TryGetValue(controller, out var list) && list.Exists(c => c.isOverHole);
        controller.cantFall = protect;
    }

    private void Awake()
    {
        circleCollider = GetComponent<Collider2D>();
        circleCollider.isTrigger = true;
        soundContainer = GetComponent<SoundContainer>();
    }

    private void Start()
    {
        bool state;
        if (SaveManager.instance.twoStateContainer.TryGetState(id, out state))
            isOn = state;

        if (isOn)
            currentRotationSpeed = rotationSpeed;
    }

    private void OnDestroy()
    {
        if (carriedPlayerController != null)
        {
            PlayerController pc = carriedPlayerController;

            if (circlesByPlayer.TryGetValue(pc, out var list))
            {
                list.Remove(this);
                if (list.Count == 0)
                    circlesByPlayer.Remove(pc);
            }

            RecomputeCantFall(pc);
        }
    }

    private void LateUpdate()
    {
        // Rotation visuelle du cercle lui-m�me : reste en LateUpdate pour rester fluide (li�e au framerate).
        if (currentRotationSpeed > 0f)
        {
            float delta = currentRotationSpeed * Time.deltaTime * (clockwise ? -1f : 1f);
            transform.Rotate(0f, 0f, delta);
        }

        if (carriedPlayerController != null)
            RecomputeCantFall(carriedPlayerController);
    }

    private void FixedUpdate()
    {
        // Le d�placement des objets/entit�s transport�s se fait au m�me rythme que la physique
        // (comme NewMonsterMovement en FixedUpdate) et via rb.MovePosition : un set direct de rb.position
        // depuis LateUpdate se fait �craser par l'interpolation du Rigidbody2D au pas physique suivant,
        // ce qui figeait les monstres port�s au lieu de les faire tourner avec le cercle.
        if (currentRotationSpeed <= 0f || carriedObjects.Count == 0) return;

        float delta = currentRotationSpeed * Time.fixedDeltaTime * (clockwise ? -1f : 1f);
        Quaternion deltaRotation = Quaternion.Euler(0f, 0f, delta);

        for (int i = 0; i < carriedObjects.Count; i++)
        {
            CarriedObject carried = carriedObjects[i];
            Vector3 offset = carried.transform.position - transform.position;
            offset = deltaRotation * offset;
            Vector3 newPosition = transform.position + offset;

            if (carried.rb != null)
                carried.rb.MovePosition(newPosition);
            else
                carried.transform.position = newPosition;
        }
    }

    public void Toggle()
    {
        if (isOn) TurnOff();
        else TurnOn();
    }

    public void TurnOn()
    {
        if (isOn) return;
        isOn = true;

        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, isOn);

        if (soundContainer != null)
            soundContainer.PlaySound("On", 1);

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionSpeedRoutine(rotationSpeed, turnOnTransitionDuration));
    }

    public void TurnOff()
    {
        if (!isOn) return;
        isOn = false;

        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, isOn);

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        currentRotationSpeed = 0f;
    }

    private IEnumerator TransitionSpeedRoutine(float targetSpeed, float duration)
    {
        float startSpeed = currentRotationSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            currentRotationSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentRotationSpeed = targetSpeed;
        transitionCoroutine = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Stats stats = other.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            carriedPlayer = other.transform;
            carriedPlayerController = other.GetComponent<PlayerController>();

            if (carriedPlayerController != null)
            {
                if (!circlesByPlayer.TryGetValue(carriedPlayerController, out var list))
                {
                    list = new List<RustCircleBehiavor>();
                    circlesByPlayer[carriedPlayerController] = list;
                }

                if (!list.Contains(this))
                    list.Add(this);

                RecomputeCantFall(carriedPlayerController);
            }
        }

        bool isHole = other.tag != null && other.tag.StartsWith("Hole");
        bool isOtherRustCircle = other.GetComponent<RustCircleBehiavor>() != null;
        bool isProjectile = other.GetComponent<ProjectileBehavior>() != null;
        if (!isHole && !isOtherRustCircle && !isProjectile && other.GetComponent<Tilemap>() == null && !carriedObjects.Exists(c => c.transform == other.transform))
        {
            carriedObjects.Add(new CarriedObject
            {
                transform = other.transform,
                rb = other.attachedRigidbody
            });
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == null || !other.tag.StartsWith("Hole")) return;

        Tilemap tilemap = other.GetComponent<Tilemap>();
        if (tilemap == null) return;

        Vector3Int cell = tilemap.WorldToCell(transform.position);
        isOverHole = tilemap.HasTile(cell);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform == carriedPlayer)
        {
            PlayerController pc = carriedPlayerController;

            if (pc != null && circlesByPlayer.TryGetValue(pc, out var list))
            {
                list.Remove(this);
                if (list.Count == 0)
                    circlesByPlayer.Remove(pc);
            }

            carriedPlayer = null;
            carriedPlayerController = null;

            RecomputeCantFall(pc);
        }

        carriedObjects.RemoveAll(c => c.transform == other.transform);

        if (other.tag != null && other.tag.StartsWith("Hole"))
            isOverHole = false;
    }
}
