using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewMapPiece", menuName = "Map/Map piece")]
public class MapPiece : ScriptableObject
{
    [Header("Scènes")]
#if UNITY_EDITOR
    public List<SceneAsset> sceneAssets = new List<SceneAsset>(); // Sélection directe dans l’éditeur
#endif
    public List<string> scenes = new List<string>(); // Auto-rempli avec les noms des scènes

    [Header("Visuel")]
    public Sprite icon;

    [Header("Coordonnées")]
    public IntPair coords;

#if UNITY_EDITOR
    private void OnValidate()
    {
        scenes.Clear();
        foreach (var sceneAsset in sceneAssets)
        {
            if (sceneAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(sceneAsset);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                scenes.Add(sceneName);
            }
        }
    }
#endif
}

[System.Serializable]
public struct IntPair
{
    public int x;
    public int y;

    public IntPair(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
