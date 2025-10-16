using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldMapGenerator))]
public class WorldMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldMapGenerator generator = (WorldMapGenerator)target;

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto-détecter\nTilemaps"))
        {
            generator.AutoDetectTilemaps();
        }

        if (GUILayout.Button("Auto-détecter\nSprites"))
        {
            generator.AutoDetectSpriteRenderers();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Auto-détecter TOUT", GUILayout.Height(30)))
        {
            generator.AutoDetectAll();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Générer la carte PNG", GUILayout.Height(40)))
        {
            generator.GenerateMap();
        }
    }
}