using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class DamageZoneBehiavor : MonoBehaviour
{
    private GameObject owner;
    public bool swordHitOnce = false;
    public bool playerTouched;

    // Nouveau booléen pour activer/désactiver la détection d’impact d’épée
    public bool isDetectingSwordImpact = true;

    public void Init(GameObject owner, float radius)
    {
        this.owner = owner;
        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        if (cc != null)
        {
            cc.radius = radius;
        }

        GetComponent<ObjectPerspective>().level = owner.GetComponent<ObjectPerspective>().level;
    }

    public void Init(GameObject owner)
    {
        this.owner = owner;
        GetComponent<ObjectPerspective>().level = owner.GetComponent<ObjectPerspective>().level;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (playerTouched)
            return;

        Stats otherStats = other.GetComponent<Stats>();
        if (otherStats != null && otherStats.isVulnerable
            && otherStats.entityType == EntityType.Player)
        {
            owner.GetComponent<LifeManager>().Attack(other.gameObject);
            playerTouched = true;
        }
    }

    public void SwordCollide()
    {
        if (!swordHitOnce && isDetectingSwordImpact)
        {
            GetComponent<SoundContainer>().PlaySound("ShieldImpact", 2);
            GetComponent<ObjectParticles>().SpawnParticle("Hit", transform.position);
            owner.GetComponent<LifeManager>().KnockBack(owner.gameObject, 10, PlayerManager.instance.player);
            PlayerManager.instance.player.GetComponent<LifeManager>().KnockBack(PlayerManager.instance.player, 10, owner.gameObject);
            swordHitOnce = true;
        }
    }
}
