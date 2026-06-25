using UnityEngine;
using System.Collections.Generic;

public enum SpecialItemType
{
    ARROW,
    IRON,
    SILVER,
    DIAMOND,
    ANTIMATTER,
    SQUAREBLOCK,
    WOOD,
    STONE,
    AMETHYST,
    COAL,
    HAY,
    SLIME_BALL,
    SLIME_HEART,
    SQUARE_ANT,
}

[CreateAssetMenu(fileName = "New Special Item", menuName = "Items/Special Item")]
public class SpecialItems : Item
{
    [Header("Special Item Properties")]
    public SpecialItemType type;
    public int nb;


    public int CalculateValue()
    {
        return value * nb;
    }
}