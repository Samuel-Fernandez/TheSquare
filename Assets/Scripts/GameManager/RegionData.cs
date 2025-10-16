using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public enum RegionType
{
    NONE,
    CINEMATIC,
    FOREST,
    PLAIN,
    DESERT
}


[CreateAssetMenu(fileName = "Regions Database", menuName = "WorldData/Regions")]
[Serializable]
public class RegionData : ScriptableObject
{
    public RegionType type;
    public string regionID;
    public List<SceneData> scenes;
}