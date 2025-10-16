using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DestroyableType
{
    NORMAL,
    DOOR,
    DESTROY_EVENT
}

public class DestroyableBehiavor : MonoBehaviour
{
    public int powerRequired;
    public bool isLower;
    public bool needFire;
    bool isDestroying;
    public int life = 2;

    public DestroyableType destroyableType;
    
    // Uniquement pour door type
    public Sprite openedDoor;
    public BoxCollider2D colliderToRemove;
    public string id;

    public List<GameObject> affectedObject;

    private void Start()
    {
        if(id != null && destroyableType == DestroyableType.DESTROY_EVENT)
        {
            bool state;

            SaveManager.instance.twoStateContainer.TryGetState(id, out state);

            if (state)
            {
                Destroy(gameObject);
            }
        }
        else if(destroyableType == DestroyableType.DOOR)
        {
            bool state;

            SaveManager.instance.twoStateContainer.TryGetState(id, out state);

            if (state)
            {
                // FAIRE SYSTEME DE SAUVEGARDE ICI, PAS TERMINE
                GetComponent<SoundContainer>().PlaySound("Destroy", 1);
                GetComponentInChildren<SpriteRenderer>().sprite = openedDoor;
                colliderToRemove.enabled = false;
                life = 0;
            }
            
        }
    }

    public void ForceDestroy()
    {
        isDestroying = true;
        GetComponent<ObjectParticles>().SpawnParticle("Destroyed", transform.position);
        GetComponent<SoundContainer>().PlaySound("Destroy", 1);
        GetComponent<LootChance>().Drop();
        Destroy(gameObject);
    }

    private void HideChildren()
    {
        // Tous les SpriteRenderer enfants
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in sprites)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0f);
        }

        // Tous les EntityLight enfants
        EntityLight[] lights = GetComponentsInChildren<EntityLight>();
        foreach (var l in lights)
        {
            l.TransitionLightIntensity(0f, 0f, 0f); // éteint immédiatement
        }
    }


    public void DestroyObject(int power)
    {
        if((power >=  powerRequired && !isDestroying && !needFire) || (needFire && GetComponent<EntityEffects>().isFire))
        {
            if (DestroyableType.NORMAL == destroyableType)
            {
                GetComponent<ObjectParticles>().SpawnParticle("Destroyed", transform.position);

                if (life > 0)
                {
                    life--;
                }
                else
                {
                    isDestroying = true;
                    GetComponent<ObjectParticles>().SpawnParticle("Destroyed", transform.position);
                    GetComponent<SoundContainer>().PlaySound("Destroy", 1);
                    GetComponent<LootChance>().Drop();
                    Destroy(gameObject);
                }
            }
            else if (DestroyableType.DOOR == destroyableType)
            {
                GetComponent<ObjectParticles>().SpawnParticle("Destroyed", transform.position);

                if (life > 0)
                {
                    life--;
                }
                else
                {
                    GetComponent<SoundContainer>().PlaySound("Destroy", 1);
                    GetComponentInChildren<SpriteRenderer>().sprite = openedDoor;
                    colliderToRemove.enabled = false;
                    SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, true);

                }
            }
            else if (destroyableType == DestroyableType.DESTROY_EVENT)
            {
                if (life > 0)
                {
                    life--;
                    GetComponent<SoundContainer>().PlaySound("Hit", 1);

                    PlayerManager.instance.player.GetComponent<LifeManager>().KnockBack(PlayerManager.instance.player, 30, gameObject);
                    CameraManager.instance.ShakeCamera(5, 5, 1);
                    GetComponentInChildren<EntityLight>().SetLightIntensity(10, 10);
                    GetComponentInChildren<EntityLight>().TransitionLightIntensity(1, 3, 1);

                }
                else
                {
                    HideChildren();

                    SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, true);

                    GetComponent<EventPlayer>().eventContainer = EventGeneratorManager.instance.MoveCamera(new Vector2(0, 0), affectedObject[0].transform.position - gameObject.transform.position, 1f, 4f);
                    GetComponent<EventPlayer>().PlayAnimation();

                    foreach (var entity in affectedObject)
                    {
                        if (entity.GetComponent<GiganticMagicShield>() != null)
                        {
                            entity.GetComponent<GiganticMagicShield>().FadeAndDestroy();
                        }
                        else
                        {
                            entity.SetActive(false);
                        }
                    }

                    isDestroying = true;
                    GetComponent<ObjectParticles>().SpawnParticle("Destroyed", transform.position);
                    GetComponent<SoundContainer>().PlaySound("Destroy", 1);
                    Destroy(gameObject, 6);
                }
                
            }
            
        }
    }
}
