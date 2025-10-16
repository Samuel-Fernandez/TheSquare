using UnityEngine;

public enum ConsumableType
{
    NONE,
    HEALING
}

public enum HealingType
{
    NONE,
    PERCENTAGE,
    LIFE_POINT
}

[CreateAssetMenu(fileName = "New Consumables", menuName = "Items/Consumables")]
public class Consumables : Item
{
    public ConsumableType type;
    public HealingType typeHealing;
    public float power;
}