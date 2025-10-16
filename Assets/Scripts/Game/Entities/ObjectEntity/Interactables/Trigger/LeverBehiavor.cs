using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverBehiavor : MonoBehaviour
{
    public List<GameObject> logicalEntites;
    public string id;
    public Sprite openSprite;
    public Sprite closedSprite;
    public bool isActivated;
    public bool isOneShot;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        bool state;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        SaveManager.instance.twoStateContainer.TryGetState(id, out state);

        if (state)
        {
            ToggleLever(false);

            if (isOneShot)
                GetComponent<InteractableBehiavor>().canInteract = false;
        }
    }

    public void ToggleLever(bool playerInteraction, bool playAnimation = true)
    {
        StartCoroutine(RoutineToggleLever(playerInteraction, playAnimation));
    }

    IEnumerator RoutineToggleLever(bool playerInteraction, bool playAnimation = true)
    {
        isActivated = !isActivated;

        
        GetComponent<SoundContainer>().PlaySound("Toggle", 1);


        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, isActivated);

        if(playAnimation)
        {
            if (isActivated || isOneShot)
            {
                GetComponent<ObjectAnimation>().PlayAnimation("Activate");
            }
            else
            {
                GetComponent<ObjectAnimation>().PlayAnimation("Desactivate");

            }

            yield return new WaitForSeconds(.5f);

            GetComponent<ObjectAnimation>().StopAnimation();
        }
        

        if (isActivated)
        {
            spriteRenderer.sprite = openSprite;
        }
        else
        {
            spriteRenderer.sprite = closedSprite;
        }


        StartCoroutine(ToggleLogicalEntities(playerInteraction, playAnimation));

        yield return new WaitForSeconds(.3f);

        if (isOneShot)
            GetComponent<SoundContainer>().PlaySound("ToggleOneShot", 1);


    }

    IEnumerator ToggleLogicalEntities(bool playerInteraction, bool playAnimation = true)
    {
        if (playerInteraction && playAnimation)
        {
            GetComponent<EventPlayer>().eventContainer = EventGeneratorManager.instance.MoveCamera(new Vector2(0, 0), logicalEntites[0].transform.position - gameObject.transform.position, 1f, 1.5f);
            GetComponent<EventPlayer>().PlayAnimation();
        }

        yield return new WaitForSecondsRealtime(1.5f);

        foreach (GameObject entity in logicalEntites)
        {
            if (entity.GetComponent<DoorBehiavor>())
            {
                if (entity.GetComponent<DoorBehiavor>().isOpen)
                    entity.GetComponent<DoorBehiavor>().CloseDoor();
                else
                    entity.GetComponent<DoorBehiavor>().OpenDoor();
            }
            else if (entity.GetComponent<Spades>())
            {
                StartCoroutine(entity.GetComponent<Spades>().RoutineSpades());
            }
            else if (entity.GetComponent<SkeletonBridgeBehiavor>())
            {
                entity.GetComponent<SkeletonBridgeBehiavor>().Activate();
            }
            else if(entity.GetComponent<LeverBehiavor>())
            {
                entity.GetComponent<LeverBehiavor>().ToggleLever(playerInteraction, false);
            }
            else
            {
                entity.SetActive(!entity.activeSelf);
            }


        }

        yield return new WaitForSecondsRealtime(1.5f);

    }
}
