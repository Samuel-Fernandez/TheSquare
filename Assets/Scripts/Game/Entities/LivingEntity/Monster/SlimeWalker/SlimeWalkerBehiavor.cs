using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class SlimeWalkerBehiavor : MonoBehaviour
{
    public GameObject launchedSlimeBall;

    private void Start()
    {
        StartCoroutine(LaunchRoutine());
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
        {
            if(!collision.gameObject.GetComponent<EntityEffects>().isSlimed)
                collision.gameObject.GetComponent<EntityEffects>().SetState(isSlimed: true);
        }
    }

    IEnumerator LaunchRoutine()
    {
        while (true)
        {
            if(GetComponent<NewMonsterMovement>().IsInDetectionZone)
            {
                yield return new WaitForSeconds(Random.Range(2, 6));
                GetComponent<ObjectAnimation>().PlayAnimation("Throw");
                GetComponent<NewMonsterMovement>().EnableAnimations = false;
                GetComponent<NewMonsterMovement>().SetSpeedMultiplier(0);
                yield return new WaitForSeconds(.75f);
                GetComponent<SoundContainer>().PlaySound("Throw", 2);
                GameObject launchedSlimeBallInstance = Instantiate(launchedSlimeBall, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
                launchedSlimeBallInstance.GetComponent<LaunchedSlimeball>().Init(GetComponent<Stats>().strength, this.gameObject);
                yield return new WaitForSeconds(.25f);
                GetComponent<NewMonsterMovement>().EnableAnimations = true;
                GetComponent<NewMonsterMovement>().SetSpeedMultiplier(1);
            }
            else
            {
                yield return null;
            }
        }

    }
}


