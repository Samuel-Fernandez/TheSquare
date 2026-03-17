using UnityEngine;

[CreateAssetMenu(fileName = "NewKeyItem", menuName = "Key Item/New Key Item")]
public class KeyItemObject : ScriptableObject
{
    public string id;
    public Sprite sprite;
    public bool isAcquired;
}
