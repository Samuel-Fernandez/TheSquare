using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public enum SceneType
{
    NONE,
    OUTSIDE,
    CAVE,
    HOUSE,
    DUNGEON,
}

[CreateAssetMenu(fileName = "Scene", menuName = "WorldData/Scenes")]
public class SceneData : ScriptableObject
{
    public SceneType sceneType;
    public CameraFilter filter;
    public string sceneID; // Nom de scène utilisable
    public string dungeonName;
    public AudioClip music;
    public List<AudioClip> ambientSound;

    public float sunIntensity = 0;

    public bool playerCantOpenInventory;

#if UNITY_EDITOR
    [SerializeField] public SceneAsset sceneAsset; // Permet de sélectionner une scène dans l'inspecteur
#endif

    [SerializeField] private string sceneName; // Stocke le nom de la scène pour le runtime

    public string SceneName => sceneName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name; // Copie le nom de la scène pour le runtime
        }
    }
#endif

    // Méthode pour charger la scène
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}