using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerInputAction
{
    Up,
    Down,
    Left,
    Right,
    Dodge,
    Attack,
    HoldAttack,
    ReleaseAttack,
    SpecialItem
}

[CreateAssetMenu(fileName = "NewSpecialAttack", menuName = "Combat/Special Attack")]
public class SpecialAttack : ScriptableObject
{
    [Header("Attack Info")]
    public string attackName;
    public float duration; // durée max du combo
    public bool isAvailable = true;

    [Header("Valid Combos")]
    public List<Combo> combos = new List<Combo>();
}

[System.Serializable]
public class Combo
{
    public List<PlayerInputAction> inputSequence = new List<PlayerInputAction>();
}
