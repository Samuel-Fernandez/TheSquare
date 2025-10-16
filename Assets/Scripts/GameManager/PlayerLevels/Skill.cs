using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BONUS_TYPE
{
    REGEN,
    ADD_STATS,
    DOUBLE_SQUARE,
    EFFECT_REDUCER,
    ITEM_CHANCE,
    MINERAL_CHANCE,
    DODGE_CHANCE,
    DOUBLE_MINERAL,
    DROP_CHANCE,
    PICKAXE_SPEED,
    BOW_SPEED,
    SHIELD_KNOCKBACK,
    VAMPIRE,
    DRAGON_SKIN,
}

public enum STATS_ADD
{
    HP,
    STR,
    SPE,
    KBP,
    KBR,
    CRITD,
    CRITC,
    LUCK
}

[CreateAssetMenu(fileName = "new Skill", menuName = "Skills/Skills")]
public class Skill : ScriptableObject
{
    public string id;
    public int lvlLifeMin;
    public int lvlStrengthMin;
    public int lvlLuckMin;
    public int cost;
    public List<Skill> previousSkills;
    public Sprite img;
    public BONUS_TYPE type;
    public STATS_ADD statAdd;
    public int intValue; // hp, str, regen
    public float floatValue; // le reste
}
