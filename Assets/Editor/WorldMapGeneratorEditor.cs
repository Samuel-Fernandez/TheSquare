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
        if (GUILayout.Button("Auto-d�tecter\nTilemaps"))
        {
            generator.AutoDetectTilemaps();
        }

        if (GUILayout.Button("Auto-d�tecter\nSprites"))
        {
            generator.AutoDetectSpriteRenderers();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Auto-d�tecter TOUT", GUILayout.Height(30)))
        {
            generator.AutoDetectAll();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("G�n�rer la carte PNG", GUILayout.Height(40)))
        {
            generator.GenerateMap();
        }
    }
}