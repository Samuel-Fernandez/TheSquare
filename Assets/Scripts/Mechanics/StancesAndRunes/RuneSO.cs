using UnityEngine;

[CreateAssetMenu(fileName = "New Rune", menuName = "Combat/Rune", order = 2)]
public class RuneSO : ScriptableObject
{
    [Tooltip("Identifiant unique de la rune (ex: 'rune_standard')")]
    public string id;
    
    [Tooltip("L'icône qui sera affichée dans l'UI")]
    public Sprite iconSprite;
}
