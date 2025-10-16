using System.Collections;
using UnityEngine;

public class SlimeSwordsmanBehiavor : MonoBehaviour
{
    public GameObject swordSlashPrefab;
    public GameObject damageZonePrefab;

    private GameObject currentDamageZone;

    private void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2, 10));

            if (GetComponent<NewMonsterMovement>().IsInDetectionZone)
            {
                Stats stats = GetComponent<Stats>();
                NewMonsterMovement movement = GetComponent<NewMonsterMovement>();
                ObjectAnimation anim = GetComponent<ObjectAnimation>();

                stats.doingAttack = true;
                movement.EnableAnimations = false;

                Vector3 attackDirection = movement.Direction.normalized;
                movement.enabled = false;

                // Choisir animation selon direction figée
                string animName;
                if (Mathf.Abs(attackDirection.y) > Mathf.Abs(attackDirection.x))
                {
                    animName = attackDirection.y > 0 ? "AttackUp" : "AttackDown";
                }
                else
                {
                    animName = "AttackSide";
                }


                yield return new WaitForSeconds(.5f);

                // Instancier la zone de dégâts une seule fois, en enfant du monstre, position relative = attackDirection * 1f
                if (currentDamageZone != null)
                    Destroy(currentDamageZone);

                currentDamageZone = Instantiate(damageZonePrefab, transform.position, Quaternion.identity, transform);
                currentDamageZone.transform.localPosition = attackDirection * .25f;

                DamageZoneBehiavor damageZoneScript = currentDamageZone.GetComponent<DamageZoneBehiavor>();
                if (damageZoneScript != null)
                {
                    damageZoneScript.Init(gameObject, 0.5f);
                }


                for (int i = 0; i < 3; i++)
                {
                    anim.StopAnimation();
                    anim.PlayAnimation(animName, !(i == 2), i == 1);

                    damageZoneScript.playerTouched = false;

                    // Instancier swordSlash 1 unité devant à chaque attaque
                    Vector3 spawnPos = transform.position + attackDirection * .5f;
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, attackDirection);
                    Instantiate(swordSlashPrefab, spawnPos, rotation);

                    float t = 0f;
                    while (t < 0.25f)
                    {
                        transform.position += attackDirection * 2f * Time.deltaTime;
                        t += Time.deltaTime;
                        yield return null;
                    }

                    GetComponent<SoundContainer>().PlaySound("SwordSlash", 2);

                    yield return new WaitForSeconds(0.25f);
                }

                // Supprimer la zone de dégâts à la fin de l’attaque
                if (currentDamageZone != null)
                {
                    Destroy(currentDamageZone);
                    currentDamageZone = null;
                }

                movement.enabled = true;
                stats.doingAttack = false;
                movement.EnableAnimations = true;
            }

        }
    }
}
