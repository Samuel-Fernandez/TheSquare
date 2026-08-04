using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicalEye : MonoBehaviour
{
    [Header("Identification")]
    public string ID;

    [Header("State")]
    public bool isOn = false;
    public bool isOneShot = false; // Si activable qu�une seule fois
    public float timerBeforeOff = 3f; // Dur�e avant extinction automatique

    [Header("Logic Links")]
    public List<GameObject> logicalObjects;

    [Header("Visuals & Sound")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;

    private SpriteRenderer spriteRenderer;
    private Coroutine deactivateRoutine;
    private bool hasTriggeredEntities = false; // Pour �viter de relancer les entit�s plusieurs fois

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Charger l��tat sauvegard�
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
        // Ne r�agit qu�� un projectile, et seulement si pas d�j� activ�
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

        // Si pas en one-shot, d�marrer la d�sactivation automatique
        if (!isOneShot)
        {
            if (deactivateRoutine != null)
                StopCoroutine(deactivateRoutine);
            deactivateRoutine = StartCoroutine(AutoDeactivateRoutine());
        }

        // V�rifie si tous les yeux li�s sont actifs avant d'activer les entit�s
        if (AllLinkedEyesActive() && !hasTriggeredEntities)
        {
            hasTriggeredEntities = true;
            // On arr�te les timers de tous les yeux li�s
            StopAllLinkedTimersAndLockState();
            StartCoroutine(ToggleLogicalEntities(triggerEvents));
        }
    }

    public void Deactivate()
    {
        if (!isOn || isOneShot) return; // Un �il one-shot ne peut pas �tre d�sactiv�

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
        // V�rifie si tous les LogicalEye partageant au moins un logicalObject sont actifs
        LogicalEye[] allEyes = FindObjectsOfType<LogicalEye>();
        foreach (LogicalEye eye in allEyes)
        {
            foreach (GameObject obj in logicalObjects)
            {
                if (eye.logicalObjects.Contains(obj) && !eye.isOn)
                    return false; // Un �il li� n�est pas encore activ�
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

                // Sauvegarde l��tat permanent
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
            else if (entity.TryGetComponent(out RustCircleBehiavor rustCircle))
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
