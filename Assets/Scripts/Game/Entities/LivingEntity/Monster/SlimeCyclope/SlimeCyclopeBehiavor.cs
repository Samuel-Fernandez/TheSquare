using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeCyclopeBehiavor : MonoBehaviour
{
    private NewMonsterMovement monsterMovement;
    private bool isAttacking = false;

    private void Start()
    {
        monsterMovement = GetComponent<NewMonsterMovement>();
    }

    private void Update()
    {
        if (!isAttacking && monsterMovement != null && monsterMovement.IsInDetectionZone && HasClearLineOfSight())
        {
            StartCoroutine(AttackRoutine());
        }
    }
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        GetComponent<NewMonsterMovement>().EnableAnimations = false;
        GetComponent<ObjectAnimation>().PlayAnimation("RedEye");
        yield return new WaitForSeconds(0.25f);

        GetComponent<ObjectAnimation>().PlayAnimation("HeadShake");
        GetComponent<SoundContainer>().PlaySound("ReverseExplosion", 1);


        GetComponent<EntityLight>().TransitionLightIntensity(25, 25, 3);

        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForSeconds(0.25f);
            GetComponent<SoundContainer>().PlaySound("Attack", 3);
        }

        CameraManager.instance.SetFilter(CameraFilter.SHYRON);
        yield return new WaitForSeconds(1);

        GetComponent<SoundContainer>().PlaySound("Explosion", 1);
        GetComponent<SoundContainer>().PlaySound("Explosion2", 1);

        CameraManager.instance.ShakeCamera(5, 5, 1);

        if (HasClearLineOfSight())
        {
            PlayerManager.instance.player.GetComponent<LifeManager>().TakeDamage(9999, gameObject, false);
        }

        CameraManager.instance.SetFilter(CameraFilter.NONE);
        GetComponent<EntityLight>().TransitionLightIntensity(.25f, .75f, 1);
        GetComponent<ObjectAnimation>().PlayAnimation("Afk");
        yield return new WaitForSeconds(3);

        GetComponent<NewMonsterMovement>().EnableAnimations = true;
        isAttacking = false;
    }

    private bool HasClearLineOfSight()
    {
        Vector3 start = transform.position;
        Vector3 end = PlayerManager.instance.player.transform.position;
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance);

        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger)
                continue;

            // Ignorer les objets sur le layer "LowHeight"
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("LowHeight"))
                continue;

            // Ignorer les entités non-joueur
            Stats stats = hit.collider.GetComponent<Stats>();
            if (stats != null && stats.entityType != EntityType.Player)
                continue;

            // Ligne de vue claire si le joueur est touché
            if (hit.collider.gameObject == PlayerManager.instance.player)
                return true;

            // Obstruction détectée
            return false;
        }

        return true;
    }

}
