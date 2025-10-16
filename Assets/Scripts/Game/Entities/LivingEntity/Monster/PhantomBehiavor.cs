using System.Collections;
using UnityEngine;

public class PhantomBehiavor : MonoBehaviour
{
    NewMonsterMovement movement;
    SpriteRenderer spriteRenderer;
    Stats stats;

    private void Start()
    {
        movement = GetComponent<NewMonsterMovement>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        stats = GetComponent<Stats>();
    }

    void Update()
    {
        float distance = movement.GetDistanceToTarget();

        float alpha = Mathf.InverseLerp(2.5f, 1.5f, distance);
        spriteRenderer.color = new Color(1f, 1f, 1f, alpha);

        // Déclenchement de l'attaque si proche
        if (distance <= 1.5f && !stats.doingAttack)
        {
            StartCoroutine(PhantomAttackRoutine());
        }
    }

    IEnumerator PhantomAttackRoutine()
    {
        stats.doingAttack = true;
        movement.SetSpeedMultiplier(5f);

        while (movement.GetDistanceToTarget() <= 2f)
        {
            GetComponent<SoundContainer>().PlaySound("Attack", 2);
            CameraManager.instance.ShakeCamera(3, 3, .5f);
            yield return new WaitForSeconds(.5f);
        }

        stats.doingAttack = false;
        movement.SetSpeedMultiplier(1f);

    }
}
