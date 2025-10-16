using UnityEngine;

public static class ScriptableObjectUtility
{
    public static T Clone<T>(T source) where T : ScriptableObject
    {
        T clone = ScriptableObject.Instantiate(source);
        clone.name = source.name; // optional: change clone's name
        return clone;
    }
}
