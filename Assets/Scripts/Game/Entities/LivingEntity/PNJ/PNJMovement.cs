using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]  // Ajoutez ceci pour rendre la classe sérialisable et visible dans l'inspecteur
public class PNJMovements
{
    public List<Vector2> movement;
    public float duration;
    public bool isWaiting;
}

[CreateAssetMenu(fileName = "New pnjMovement", menuName = "PNJ/Movement")]
public class PNJMovement : ScriptableObject
{
    [SerializeField]
    public List<PNJMovements> movements;
}
