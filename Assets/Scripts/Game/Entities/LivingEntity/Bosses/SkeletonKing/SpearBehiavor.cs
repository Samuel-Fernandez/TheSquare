using System.Collections;
using UnityEngine;

public class SpearBehiavor : MonoBehaviour
{
    void Start()
    {
    }

    public void InitSpear(int strength, int speed, bool ally, float knockBackPower, GameObject launcher)
    {
        StartCoroutine(RotateAndAimAtPlayer(strength, speed, ally, knockBackPower, launcher));
    }

    IEnumerator RotateAndAimAtPlayer(int strength, int speed, bool ally, float knockBackPower, GameObject launcher)
    {
        // Phase 1 : rotation rapide de 5 tours (1800 degrés)
        float duration = 1f;
        float rotations = 5f;
        float totalAngle = 360f * rotations;

        float elapsed = 0f;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = totalAngle * t;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // Phase 2 : orienter la lance vers le joueur
        Transform player = PlayerManager.instance?.player?.transform;

        if (player == null)
        {
            yield break;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayerRad = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x);
        float angleToPlayerDeg = angleToPlayerRad * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angleToPlayerDeg);


        // Phase 3 : recule de 1 unité en 0.5 seconde, avec ralentissement progressif
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - directionToPlayer;

        float backDuration = 0.5f;
        elapsed = 0f;

        while (elapsed < backDuration)
        {
            elapsed += Time.deltaTime;
            float linearT = Mathf.Clamp01(elapsed / backDuration);

            // Interpolation EaseOutQuad (ralentit vers la fin)
            float easedT = 1f - (1f - linearT) * (1f - linearT);

            transform.position = Vector3.Lerp(startPos, endPos, easedT);
            yield return null;
        }

        GetComponent<ProjectileBehavior>().enabled = true;
        GetComponent<ProjectileBehavior>().InitProjectile(strength, speed, angleToPlayerRad, ally, knockBackPower, launcher, false);


    }

}
