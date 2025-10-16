using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SwitchType
{
    ON_OFF,
    TIMER,
}

// AJOUTER L'INTERACTION AVEC FLECHES, MARTEAU ET PIOCHE
// FAIRE UNE ANIMATION AVEC LATENCE POUR VOIR LE LOGICAL OBJET CHANGER
public class SwitchBehiavor : MonoBehaviour
{
    public bool isOn = false;
    public float timerOffDuration = 5f;
    private bool wait = false;
    public SwitchType type = SwitchType.ON_OFF;
    private EntityLight entityLight;
    private ObjectAnimation objectAnimation;
    public GameObject logicalTarget; // Définit quel groupe ce switch appartient

    public string id;

    // Liste statique associant chaque logicalTarget à sa liste de switches
    private static Dictionary<GameObject, List<SwitchBehiavor>> switchesByLogicalTarget = new Dictionary<GameObject, List<SwitchBehiavor>>();

    bool playerInteracted = false;

    void Awake()
    {
        entityLight = GetComponent<EntityLight>();
        objectAnimation = GetComponent<ObjectAnimation>();

        // Si le logicalTarget est défini, ajouter ce switch au bon groupe
        if (logicalTarget != null)
        {
            if (!switchesByLogicalTarget.ContainsKey(logicalTarget))
            {
                switchesByLogicalTarget[logicalTarget] = new List<SwitchBehiavor>();
            }
            switchesByLogicalTarget[logicalTarget].Add(this);

            // Mettre à jour l'état initial du logicalTarget
            UpdateLogicalTargetState();
        }
    }

    private void Start()
    {
        bool state;

        if(SaveManager.instance.twoStateContainer.TryGetState(id, out state))
        {
            SwitchOn(false);
        }
    }

    void OnDestroy()
    {
        // Nettoyer la liste si ce switch est détruit
        if (logicalTarget != null && switchesByLogicalTarget.ContainsKey(logicalTarget))
        {
            switchesByLogicalTarget[logicalTarget].Remove(this);

            // Supprimer l'entrée si plus aucun switch n'est lié au logicalTarget
            if (switchesByLogicalTarget[logicalTarget].Count == 0)
            {
                switchesByLogicalTarget.Remove(logicalTarget);
            }
            else
            {
                // Mise à jour de l'état du groupe après la suppression de ce switch
                UpdateLogicalTargetState();
            }
        }
    }

    // Vérifie si tous les switches d'un groupe sont activés
    public bool AreAllSwitchesOn()
    {
        if (logicalTarget == null || !switchesByLogicalTarget.ContainsKey(logicalTarget))
            return false;

        foreach (var switchBeh in switchesByLogicalTarget[logicalTarget])
        {
            if (!switchBeh.isOn)
                return false;
        }

        return true;
    }

    // Méthode pour mettre à jour l'état du logicalTarget
    private void UpdateLogicalTargetState(bool playAnimation = true)
    {
        if (!gameObject.activeInHierarchy) return; // Ne fait rien si l'objet est inactif

        if (logicalTarget == null || !switchesByLogicalTarget.ContainsKey(logicalTarget))
            return;

        bool allOn = AreAllSwitchesOn();

        if (allOn)
        {
            StopAllTimersInGroup();

            StartCoroutine(DelayLogicalTargetActivation());

            if(playAnimation)
            {
                GetComponent<EventPlayer>().eventContainer = EventGeneratorManager.instance.MoveCamera(new Vector2(0, 0), logicalTarget.transform.position - gameObject.transform.position, 1.5f, 1f);

                GetComponent<EventPlayer>().PlayAnimation();
            }
            
            foreach (var switches in switchesByLogicalTarget[logicalTarget])
            {
                SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(switches.id, true);
            }
        }
    }


    // Arrête tous les timers pour tous les switches du groupe
    private void StopAllTimersInGroup()
    {
        if (logicalTarget == null || !switchesByLogicalTarget.ContainsKey(logicalTarget))
            return;

        foreach (var switchBeh in switchesByLogicalTarget[logicalTarget])
        {
            if (switchBeh.type == SwitchType.TIMER)
            {
                switchBeh.StopAllTimers();
            }
        }
    }

    // Arrête tous les timers pour ce switch
    private void StopAllTimers()
    {
        StopAllCoroutines();
    }

    public void SwitchOn(bool playAnimation = true)
    {
        if (isOn)
            return; // Déjà activé, ne rien faire

        isOn = true;

        // Effets visuels et sonores
        if (objectAnimation != null)
            objectAnimation.PlayAnimation("TurnOn", true);

        if (entityLight != null)
        {
            entityLight.TransitionLightColor(new Color(236f / 255f, 1f, 0f), 0.1f);
            entityLight.TransitionLightIntensity(2f, 2f, 0.05f);
        }

        var soundContainer = GetComponent<SoundContainer>();
        if (soundContainer != null)
            soundContainer.PlaySound("Switch", 2);

        // Mettre à jour l'état du logicalTarget
        UpdateLogicalTargetState(playAnimation);
    }

    public void SwitchOff()
    {
        if (!isOn)
            return; // Déjà désactivé, ne rien faire

        isOn = false;

        // Effets visuels et sonores
        if (objectAnimation != null)
            objectAnimation.PlayAnimation("TurnOff", true);

        if (entityLight != null)
        {
            entityLight.TransitionLightColor(new Color(1f, 1f, 1f), 0.1f);
            entityLight.TransitionLightIntensity(0.25f, 1f, 0.05f);
        }

        var soundContainer = GetComponent<SoundContainer>();
        if (soundContainer != null)
            soundContainer.PlaySound("Switch", 2);

        // Mettre à jour l'état du logicalTarget
        UpdateLogicalTargetState();
    }

    public void Switch(bool playerInteracted)
    {
        this.playerInteracted = playerInteracted;
        if (wait)
            return; // Éviter les activations multiples rapprochées

        StartCoroutine(WaitRoutine());

        if (type == SwitchType.ON_OFF)
        {
            if (isOn)
                SwitchOff();
            else
                SwitchOn();
        }
        else if (type == SwitchType.TIMER && !isOn)
        {
            SwitchOn();

            // Ne démarrer le timer que si tous les switches ne sont pas activés
            if (!AreAllSwitchesOn())
            {
                StartCoroutine(TimerRoutine());
            }
        }
    }

    IEnumerator TimerRoutine()
    {
        yield return new WaitForSecondsRealtime(timerOffDuration);

        // Ne désactiver que si tous les switches ne sont pas activés
        if (!AreAllSwitchesOn())
        {
            SwitchOff();
        }
    }

    IEnumerator WaitRoutine()
    {
        wait = true;
        yield return new WaitForSecondsRealtime(1);
        wait = false;
    }

    IEnumerator DelayLogicalTargetActivation()
    {
        yield return new WaitForSecondsRealtime(2f);

        DoorBehiavor door = logicalTarget.GetComponent<DoorBehiavor>();

        if (door != null)
        {
            door.OpenDoor();
        }
        else
        {
            logicalTarget.SetActive(!logicalTarget.activeSelf);
        }
    }
}