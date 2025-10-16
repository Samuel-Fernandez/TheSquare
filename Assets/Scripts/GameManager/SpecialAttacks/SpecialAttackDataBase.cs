using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpecialAttackDataBase", menuName = "SpecialAttackDataBase")]
public class SpecialAttackDataBase : ScriptableObject
{
    public List<SpecialAttack> specialAttacks; // Liste de toutes les attaques spéciales disponibles dans le jeu

    // Méthode pour obtenir toutes les attaques spéciales
    public List<SpecialAttack> GetAllSpecialAttacks()
    {
        return specialAttacks;
    }
}