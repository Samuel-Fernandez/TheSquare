using UnityEngine;

[CreateAssetMenu(fileName = "New Stance", menuName = "Combat/Stance", order = 1)]
public class StanceSO : ScriptableObject
{
    [Tooltip("Identifiant unique de la posture (ex: 'stance_neutral')")]
    public string id;
    
    [Tooltip("L'icône qui sera affichée dans l'UI")]
    public Sprite iconSprite;
}
