using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class RustCircleBehiavor : MonoBehaviour
{
    [Header("State")]
    public bool isOn = false;

    [Header("Rotation")]
    public float rotationSpeed = 90f;
    public bool clockwise = true;
    public float turnOnTransitionDuration = 1f;

    private Collider2D circleCollider;
    private SoundContainer soundContainer;

    private float currentRotationSpeed = 0f;
    private Coroutine transitionCoroutine;

    private bool isOverHole = false;

    private Transform carriedPlayer;
    private PlayerController carriedPlayerController;

    private void Awake()
    {
        circleCollider = GetComponent<Collider2D>();
        circleCollider.isTrigger = true;
        soundContainer = GetComponent<SoundContainer>();
    }

    private void Start()
    {
        if (isOn)
            currentRotationSpeed = rotationSpeed;
    }

    private void OnDestroy()
    {
        if (carriedPlayerController != null)
            carriedPlayerController.cantFall = false;
    }

    private void LateUpdate()
    {
        if (currentRotationSpeed > 0f)
        {
            float delta = currentRotationSpeed * Time.deltaTime * (clockwise ? -1f : 1f);
            transform.Rotate(0f, 0f, delta);

            if (carriedPlayer != null)
            {
                Vector3 offset = carriedPlayer.position - transform.position;
                offset = Quaternion.Euler(0f, 0f, delta) * offset;
                carriedPlayer.position = transform.position + offset;
            }
        }

        if (carriedPlayerController != null)
            carriedPlayerController.cantFall = isOn && isOverHole;
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

        if (soundContainer != null)
            soundContainer.PlaySound("On", 1);

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionSpeedRoutine(rotationSpeed, turnOnTransitionDuration));
    }

    public void TurnOff()
    {
        if (!isOn) return;
        isOn = false;

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
            if (carriedPlayerController != null)
                carriedPlayerController.cantFall = false;

            carriedPlayer = null;
            carriedPlayerController = null;
        }

        if (other.tag != null && other.tag.StartsWith("Hole"))
            isOverHole = false;
    }
}
