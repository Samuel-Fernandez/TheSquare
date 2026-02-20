using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicalEye : MonoBehaviour
{
    [Header("Identification")]
    public string ID;

    [Header("State")]
    public bool isOn = false;
    public bool isOneShot = false; // Si activable qu’une seule fois
    public float timerBeforeOff = 3f; // Durée avant extinction automatique

    [Header("Logic Links")]
    public List<GameObject> logicalObjects;

    [Header("Visuals & Sound")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;

    private SpriteRenderer spriteRenderer;
    private Coroutine deactivateRoutine;
    private bool hasTriggeredEntities = false; // Pour éviter de relancer les entités plusieurs fois

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Charger l’état sauvegardé
        bool state;
        SaveManager.instance.twoStateContainer.TryGetState(ID, out state);

        if (state)
        {
            isOn = true;
            spriteRenderer.sprite = activeSprite;
            GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            isOn = false;
            spriteRenderer.sprite = inactiveSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ne réagit qu’à un projectile, et seulement si pas déjà activé
        if (collision.gameObject.GetComponent<ProjectileBehavior>() && !isOn)
        {
            Activate(true);
        }
    }

    public void Activate(bool triggerEvents = true)
    {
        if (isOn) return;

        isOn = true;
        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(ID, isOn);

        GetComponent<SoundContainer>()?.PlaySound("EyeActivate", 1);
        GetComponent<ObjectAnimation>()?.PlayAnimation("Activate");
        spriteRenderer.sprite = activeSprite;

        // Si pas en one-shot, démarrer la désactivation automatique
        if (!isOneShot)
        {
            if (deactivateRoutine != null)
                StopCoroutine(deactivateRoutine);
            deactivateRoutine = StartCoroutine(AutoDeactivateRoutine());
        }

        // Vérifie si tous les yeux liés sont actifs avant d'activer les entités
        if (AllLinkedEyesActive() && !hasTriggeredEntities)
        {
            hasTriggeredEntities = true;
            // On arrête les timers de tous les yeux liés
            StopAllLinkedTimersAndLockState();
            StartCoroutine(ToggleLogicalEntities(triggerEvents));
        }
    }

    public void Deactivate()
    {
        if (!isOn || isOneShot) return; // Un œil one-shot ne peut pas être désactivé

        isOn = false;
        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(ID, isOn);

        GetComponent<SoundContainer>()?.PlaySound("EyeDeactivate", 1);
        GetComponent<ObjectAnimation>()?.PlayAnimation("Deactivate");
        spriteRenderer.sprite = inactiveSprite;
    }

    IEnumerator AutoDeactivateRoutine()
    {
        yield return new WaitForSeconds(timerBeforeOff);
        Deactivate();
    }

    private bool AllLinkedEyesActive()
    {
        // Vérifie si tous les LogicalEye partageant au moins un logicalObject sont actifs
        LogicalEye[] allEyes = FindObjectsOfType<LogicalEye>();
        foreach (LogicalEye eye in allEyes)
        {
            foreach (GameObject obj in logicalObjects)
            {
                if (eye.logicalObjects.Contains(obj) && !eye.isOn)
                    return false; // Un œil lié n’est pas encore activé
            }
        }
        return true;
    }

    private void StopAllLinkedTimersAndLockState()
    {
        LogicalEye[] allEyes = FindObjectsOfType<LogicalEye>();

        foreach (LogicalEye eye in allEyes)
        {
            bool sharesLink = false;

            foreach (GameObject obj in logicalObjects)
            {
                if (eye.logicalObjects.Contains(obj))
                {
                    sharesLink = true;
                    break;
                }
            }

            if (sharesLink)
            {
                if (eye.deactivateRoutine != null)
                    eye.StopCoroutine(eye.deactivateRoutine);

                eye.deactivateRoutine = null;
                eye.isOn = true;
                eye.spriteRenderer.sprite = eye.activeSprite;

                // Sauvegarde l’état permanent
                SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(eye.ID, true);
                GetComponent<Collider2D>().enabled = false;

            }
        }
    }

    IEnumerator ToggleLogicalEntities(bool triggerEvents)
    {
        if (triggerEvents)
        {
            GetComponent<EventPlayer>().eventContainer =
                EventGeneratorManager.instance.MoveCamera(
                    new Vector2(0, 0),
                    logicalObjects[0].transform.position - transform.position,
                    1f, 1.5f
                );
            GetComponent<EventPlayer>().PlayAnimation();
        }

        yield return new WaitForSecondsRealtime(1.5f);

        foreach (GameObject entity in logicalObjects)
        {
            if (entity.TryGetComponent(out DoorBehiavor door))
            {
                if (door.isOpen)
                    door.CloseDoor();
                else
                    door.OpenDoor();
            }
            else if (entity.TryGetComponent(out Spades spades))
            {
                StartCoroutine(spades.RoutineSpades());
            }
            else if (entity.TryGetComponent(out SkeletonBridgeBehiavor bridge))
            {
                bridge.Activate();
            }
            else if (entity.TryGetComponent(out LeverBehiavor lever))
            {
                lever.ToggleLever(false, false);
            }
            else
            {
                entity.SetActive(!entity.activeSelf);
            }
        }

        yield return new WaitForSecondsRealtime(1.5f);
    }
}
