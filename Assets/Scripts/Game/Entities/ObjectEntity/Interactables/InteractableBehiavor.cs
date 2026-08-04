using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractableType
{
    NONE,
    CHECKPOINT,
    LEVER,
    STATUE_OF_POWER,
    ANVIL,
    CHEST,
    SIGN,
    TELEPORTER_STATUE,
    CRAFTING_TABLE,
    GUARDIAN_HEART,
    FIRE_STICK,
}

public class InteractableBehiavor : MonoBehaviour
{
    public InteractableType type;
    public bool canInteract = true;
    public float inactiveTime;
    public GameObject uiInteract;
    public bool oneShot;
    public bool forceHideUI = false;
    GameObject instanceUiInteract;

    public static List<InteractableBehiavor> nearbyInteractables = new List<InteractableBehiavor>();

    private void OnDisable()
    {
        if (nearbyInteractables.Contains(this))
            nearbyInteractables.Remove(this);
    }

    private void Start()
    {
        if (type == InteractableType.ANVIL)
        {
            GetComponent<ObjectAnimation>().PlayAnimation("Anvil");
            GetComponent<EntityLight>().SetLightColor(Color.red);
        }
    }

    private void Update()
    {
        if (canInteract && PlayerManager.instance != null && PlayerManager.instance.playerInputActions.Gameplay.Interaction.triggered)
        {
            if (nearbyInteractables.Contains(this))
            {
                PlayerController pc = PlayerManager.instance.player.GetComponent<PlayerController>();
                if (pc != null && pc.isHoldingObject)
                {
                    // Si on porte déjà un objet, la SEULE interaction autorisée est de lancer ce même objet
                    bool isThisTheCarriedObject = false;
                    if (type == InteractableType.GUARDIAN_HEART)
                    {
                        GuardianHeartBehiavor heart = GetComponent<GuardianHeartBehiavor>();
                        if (heart != null && heart.isCarried)
                        {
                            isThisTheCarriedObject = true;
                        }
                    }
                    else if (type == InteractableType.FIRE_STICK)
                    {
                        FireStickBehiavor fireStick = GetComponent<FireStickBehiavor>();
                        if (fireStick != null && fireStick.isCarried)
                        {
                            isThisTheCarriedObject = true;
                        }
                    }

                    if (!isThisTheCarriedObject)
                    {
                        return; // On ignore les autres objets (dont les autres coeurs au sol)
                    }
                }

                InteractableBehiavor closest = null;
                float minDistance = float.MaxValue;
                // Nettoyer la liste au cas où des objets auraient été détruits sans OnDisable
                nearbyInteractables.RemoveAll(item => item == null);

                foreach (var interactable in nearbyInteractables)
                {
                    if (interactable.canInteract)
                    {
                        float dist = Vector3.Distance(PlayerManager.instance.player.transform.position, interactable.transform.position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            closest = interactable;
                        }
                    }
                }

                if (closest == this)
                {
                    StartCoroutine(RoutineInteraction());
                    canInteract = false;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Stats stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            if (!nearbyInteractables.Contains(this))
            {
                nearbyInteractables.Add(this);
            }

            // Créer l'UI d'interaction si elle n'existe pas encore
            if (instanceUiInteract == null && canInteract && !forceHideUI)
            {
                Vector3 uiPosition = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                instanceUiInteract = Instantiate(uiInteract, uiPosition, Quaternion.identity);
            }

            if (instanceUiInteract != null && (!canInteract || forceHideUI))
            {
                Destroy(instanceUiInteract);
                instanceUiInteract = null;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Stats stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            if (nearbyInteractables.Contains(this))
            {
                nearbyInteractables.Remove(this);
            }

            if (instanceUiInteract != null)
            {
                Destroy(instanceUiInteract);
                instanceUiInteract = null;
            }
        }
    }



    IEnumerator RoutineInteraction()
    {
        switch (type)
        {
            case InteractableType.NONE:
                Debug.Log("Has no type !");
                break;
            case InteractableType.CHECKPOINT:
                CheckPoint();
                break;
            case InteractableType.LEVER:
                Lever();
                break;
            case InteractableType.STATUE_OF_POWER:
                StatueOfPower();
                break;
            case InteractableType.ANVIL:
                Anvil();
                break;
            case InteractableType.CHEST:
                Chest();
                break;
            case InteractableType.SIGN:
                Sign();
                break;
            case InteractableType.TELEPORTER_STATUE:
                TeleporterStatue();
                break;
            case InteractableType.CRAFTING_TABLE:
                CraftingTable();
                break;
            case InteractableType.GUARDIAN_HEART:
                GuardianHeart();
                break;
            case InteractableType.FIRE_STICK:
                FireStick();
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(inactiveTime);

        if (!oneShot)
            canInteract = true;
    }

    void TeleporterStatue()
    {
        GetComponent<TeleporterStatueBehiavor>().Interaction();
        GetComponent<SoundContainer>().PlaySound("Start", 1);
    }

    void Anvil()
    {
        AnvilUpgradeManager.instance.ToggleUI();
    }

    void StatueOfPower()
    {
        StartCoroutine(RoutineStatueOfPower());
    }

    IEnumerator RoutineStatueOfPower()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Activation");
        GetComponent<SoundContainer>().PlaySound("Activation", 1);
        GetComponent<EntityLight>().TransitionLightIntensity(2, 2, .5f);
        yield return new WaitForSeconds(.5f);
        GetComponent<EntityLight>().TransitionLightIntensity(.25f, 1, .1f);
        GetComponent<ObjectAnimation>().StopAnimation();
        PlayerLevels.instance.ToggleUI();
    }

    void CheckPoint()
    {
        GetComponent<CheckPointBehiavor>().ActiveCheckPoint();
    }

    void Lever()
    {
        GetComponent<LeverBehiavor>().ToggleLever(true);
    }

    void Chest()
    {
        GetComponent<ChestBehiavor>().Interaction();
    }

    void Sign()
    {
        GetComponent<SignBehiavor>().ShowText();
    }

    void CraftingTable()
    {
        if (GameManager.CraftingManager.instance != null)
        {
            GameManager.CraftingManager.instance.OpenCrafting();
        }
        else
        {
            Debug.LogError("CraftingManager.instance est NULL !");
        }
    }

    void GuardianHeart()
    {
        GetComponent<GuardianHeartBehiavor>().Interaction();
    }

    void FireStick()
    {
        GetComponent<FireStickBehiavor>().Interaction();
    }
}
