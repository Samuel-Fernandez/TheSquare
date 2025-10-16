using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialPower { LASER }

[System.Serializable]
public class EquipementPower
{
    public SpecialPower power;
    public List<Item> itemsConcerned;
}


public class SpecialEquipementManager : MonoBehaviour
{
    public static SpecialEquipementManager instance;

    public List<EquipementPower> equipementPowers;

    public GameObject littleLaserPrefab;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public bool CheckPower(SpecialPower power)
    {
        string helmet = Equipement.instance.equippedSlots[0].GetComponent<EquipementSlot>().actualItem?.itemName;
        string chestplate = Equipement.instance.equippedSlots[1].GetComponent<EquipementSlot>().actualItem?.itemName;
        string leggings = Equipement.instance.equippedSlots[2].GetComponent<EquipementSlot>().actualItem?.itemName;
        string boots = Equipement.instance.equippedSlots[3].GetComponent<EquipementSlot>().actualItem?.itemName;
        string weapon = Equipement.instance.equippedSlots[4].GetComponent<EquipementSlot>().actualItem?.itemName;

        foreach (var equipementPower in equipementPowers)
        {
            if (equipementPower.power != power)
                continue;

            foreach (var item in equipementPower.itemsConcerned)
            {
                if (helmet == item.itemName || chestplate == item.itemName || leggings == item.itemName || boots == item.itemName || weapon == item.itemName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void LaunchLaser()
    {
        if (PlayerManager.instance.player.GetComponent<LifeManager>().life == PlayerManager.instance.player.GetComponent<Stats>().health && Random.Range(0, 100) > 70)
        {
            GameObject laserInstance = Instantiate(littleLaserPrefab, new Vector2(PlayerManager.instance.player.transform.position.x, PlayerManager.instance.player.transform.position.y), Quaternion.identity);
            laserInstance.GetComponent<ProjectileBehavior>().InitProjectile(Mathf.Min(1, PlayerManager.instance.player.GetComponent<Stats>().strength / 4), 8, PlayerManager.instance.player.GetComponent<PlayerController>().currentDirection, true, 0, PlayerManager.instance.player);
            laserInstance.GetComponent<SoundContainer>().PlaySound("Laser", 3);
        }
        
    }

}
