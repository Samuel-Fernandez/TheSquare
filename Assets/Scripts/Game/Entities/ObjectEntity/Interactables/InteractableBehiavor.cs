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
}

public class InteractableBehiavor : MonoBehaviour
{
    public InteractableType type;
    public bool canInteract = true;
    public float inactiveTime;
    public GameObject uiInteract;
    public bool oneShot;
    GameObject instanceUiInteract;

    private void Start()
    {
        if (type == InteractableType.ANVIL)
        {
            GetComponent<ObjectAnimation>().PlayAnimation("Anvil");
            GetComponent<EntityLight>().SetLightColor(Color.red);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Stats stats = collision.GetComponent<Stats>();
        if (stats != null && stats.entityType == EntityType.Player)
        {
            // Créer l'UI d'interaction si elle n'existe pas encore
            if (instanceUiInteract == null && canInteract)
            {
                Vector3 uiPosition = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                instanceUiInteract = Instantiate(uiInteract, uiPosition, Quaternion.identity);
            }

            // Lancer l'interaction si le joueur appuie sur le bouton et que l'interaction est possible
            if (PlayerManager.instance.playerInputActions.Gameplay.Interaction.triggered && canInteract)
            {
                StartCoroutine(RoutineInteraction());
                canInteract = false;
            }

            if(instanceUiInteract != null && !canInteract)
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

}
