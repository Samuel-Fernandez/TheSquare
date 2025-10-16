using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum DoorType
{
    NORMAL,
    LOCKED
}
public class DoorBehiavor : MonoBehaviour
{
    // Seulement si locked
    public string id;

    public DoorType type;

    public BoxCollider2D doorCollider;
    public bool isOpen = false;
    public Sprite openedSprite;
    public Sprite closedSprite;

    SpriteRenderer spriteRenderer;

    // Seulement si locked
    public GameObject uiInteract;
    GameObject instanceUiInteract;

    

    private void OnTriggerStay2D(Collider2D collision)
    {
        Stats stats = collision.GetComponent<Stats>();
        if (type == DoorType.LOCKED && stats != null && stats.entityType == EntityType.Player)
        {
            // Créer l'UI d'interaction si elle n'existe pas encore
            if (instanceUiInteract == null && DungeonManager.instance.actualDungeon.nbKeys > 0)
            {
                Vector3 uiPosition = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                instanceUiInteract = Instantiate(uiInteract, uiPosition, Quaternion.identity);
            }

            // Lancer l'interaction si le joueur appuie sur le bouton et que l'interaction est possible
            if (PlayerManager.instance.playerInputActions.Gameplay.Interaction.triggered && DungeonManager.instance.actualDungeon.nbKeys > 0)
            {
                OpenDoor();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Stats stats = collision.GetComponent<Stats>();
        if (type == DoorType.LOCKED && stats != null && stats.entityType == EntityType.Player)
        {
            if (instanceUiInteract != null)
            {
                Destroy(instanceUiInteract);
                instanceUiInteract = null;
            }
        }
        
    }

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (transform.rotation.z > 0)
            GetComponent<ObjectPerspective>().bonusSortingOrder = 100;
        
        bool state;

        if (SaveManager.instance.twoStateContainer.TryGetState(id, out state))
        {
            if(state)
            {
                OpenDoor();
            }
        }
        else
        {
            if (isOpen)
            {
                OpenDoor();
            }
        }
    }

    public void OpenDoor()
    {
        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, true);

        if (type == DoorType.NORMAL)
            StartCoroutine(RoutineOpenDoor());
        else if (type == DoorType.LOCKED)
        {
            StartCoroutine(RoutineOpenDoorLocked());
            DungeonManager.instance.RemoveKey();

            if (instanceUiInteract != null)
            {
                Destroy(instanceUiInteract);
                instanceUiInteract = null;
            }
        }
    }

    public void CloseDoor()
    {
        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, false);

        StartCoroutine(RoutineCloseDoor());
    }

    IEnumerator RoutineOpenDoorLocked()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Lock");
        GetComponent<SoundContainer>().PlaySound("Lock", 1);
        yield return new WaitForSeconds(.5f);
        GetComponent<ObjectAnimation>().StopAnimation();
        StartCoroutine(RoutineOpenDoor());
    }

    IEnumerator RoutineOpenDoor()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Open");
        GetComponent<SoundContainer>().PlaySound("Toggle", 1);
        yield return new WaitForSeconds(.5f);
        GetComponent<ObjectAnimation>().StopAnimation();
        spriteRenderer.sprite = openedSprite;
        doorCollider.enabled = false;
        isOpen = true;
    }

    IEnumerator RoutineCloseDoor()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("Close");
        GetComponent<SoundContainer>().PlaySound("Toggle", 1);

        yield return new WaitForSeconds(.5f);
        GetComponent<ObjectAnimation>().StopAnimation();
        spriteRenderer.sprite = closedSprite;
        doorCollider.enabled = true;
        isOpen = false;
    }


}
