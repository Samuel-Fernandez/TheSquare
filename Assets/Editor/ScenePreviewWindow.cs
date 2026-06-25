using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class ScenePreviewWindow : EditorWindow
{
    private struct SceneAssetInfo
    {
        public string name;
        public string path;
        public string guid;
    }

    private List<SceneAssetInfo> cachedScenes = new List<SceneAssetInfo>();
    private Scene loadedPreviewScene;
    private bool previewActive = false;
    private string previewingScenePath = "";
    private Vector2 scrollPos;
    private string searchQuery = "";

    [MenuItem("Tools/Aperçu des Scènes")]
    public static void ShowWindow()
    {
        ScenePreviewWindow window = GetWindow<ScenePreviewWindow>("Aperçu de Scènes");
        window.minSize = new Vector2(350, 400);
    }

    private void OnEnable()
    {
        RefreshSceneCache();
        // S'inscrire à l'événement de changement de scène pour garder la liste à jour
        EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
        ClosePreview();
    }

    private void OnDestroy()
    {
        ClosePreview();
    }

    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        Repaint();
    }

    /// <summary>
    /// Cache les scènes du projet pour éviter d'appeler AssetDatabase.FindAssets à chaque frame de OnGUI.
    /// Rend la fenêtre extrêmement légère et rapide.
    /// </summary>
    private void RefreshSceneCache()
    {
        cachedScenes.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Ignorer les scènes externes/packages pour n'afficher que le jeu
            if (!path.StartsWith("Assets/")) continue;

            cachedScenes.Add(new SceneAssetInfo
            {
                name = Path.GetFileNameWithoutExtension(path),
                path = path,
                guid = guid
            });
        }
    }

    private void OnGUI()
    {
        // En-tête stylisé
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            margin = new RectOffset(10, 10, 10, 5)
        };
        
        GUILayout.Label("Navigateur & Aperçu des Scènes", titleStyle);
        
        EditorGUILayout.HelpBox("Consultez et chargez visuellement une scène en tâche de fond (Additif) sans quitter votre scène actuelle.", MessageType.Info);

        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("L'outil est indisponible en mode Play.", MessageType.Warning);
            return;
        }

        // Barre d'outils supérieure
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // Recherche
        GUILayout.Label("Filtre :", GUILayout.Width(50));
        string newSearch = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField);
        if (newSearch != searchQuery)
        {
            searchQuery = newSearch;
        }
        
        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(45)))
        {
            searchQuery = "";
            GUI.FocusControl(null);
        }

        GUILayout.FlexibleSpace();

        // Rafraîchir le cache
        if (GUILayout.Button("Rafraîchir", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            RefreshSceneCache();
        }

        GUILayout.EndHorizontal();

        // Zone d'action de l'aperçu global
        if (previewActive)
        {
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = new Color(1f, 0.35f, 0.35f);
            string activePreviewName = Path.GetFileNameWithoutExtension(previewingScenePath);
            if (GUILayout.Button($"Fermer l'aperçu ({activePreviewName})", GUILayout.Height(28)))
            {
                ClosePreview();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(5);

        // Liste des scènes
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        string activeScenePath = EditorSceneManager.GetActiveScene().path;

        foreach (var scene in cachedScenes)
        {
            // Appliquer le filtre de recherche
            if (!string.IsNullOrEmpty(searchQuery) && !scene.name.ToLower().Contains(searchQuery.ToLower()))
                continue;

            bool isCurrent = activeScenePath == scene.path;
            bool isPreviewed = previewActive && previewingScenePath == scene.path;

            // Arrière-plan coloré pour la scène actuellement ouverte ou en aperçu
            if (isCurrent)
            {
                GUI.backgroundColor = new Color(0.3f, 0.75f, 0.3f, 0.25f);
            }
            else if (isPreviewed)
            {
                GUI.backgroundColor = new Color(0.2f, 0.6f, 0.9f, 0.25f);
            }

            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = Color.white;

            // Nom de la scène
            GUIStyle nameStyle = new GUIStyle(EditorStyles.label);
            if (isCurrent)
            {
                nameStyle.fontStyle = FontStyle.Bold;
                GUILayout.Label($"{scene.name} (Active)", nameStyle, GUILayout.ExpandWidth(true));
            }
            else if (isPreviewed)
            {
                nameStyle.fontStyle = FontStyle.BoldAndItalic;
                GUILayout.Label($"{scene.name} (Aperçu)", nameStyle, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label(scene.name, nameStyle, GUILayout.ExpandWidth(true));
            }

            // Gestion des boutons
            if (isCurrent)
            {
                // Pas d'actions requises pour la scène active
                GUI.enabled = false;
                GUILayout.Button("Ouverte", GUILayout.Width(95));
                GUI.enabled = true;
            }
            else
            {
                // Bouton Aperçu / Fermer l'aperçu
                if (isPreviewed)
                {
                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Centrer la vue", GUILayout.Width(95)))
                    {
                        FocusOnSceneObjects();
                    }
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    GUI.enabled = !previewActive;
                    if (GUILayout.Button("Aperçu", GUILayout.Width(95)))
                    {
                        PreviewScene(scene.path);
                    }
                    GUI.enabled = true;
                }

                // Bouton Ouvrir Scene (Ferme l'aperçu si nécessaire)
                if (GUILayout.Button("Ouvrir", GUILayout.Width(60)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        if (previewActive) ClosePreview();
                        EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void FocusOnSceneObjects()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.Frame(new Bounds(Vector3.zero, Vector3.one * 15f), false);
        }
    }

    private void PreviewScene(string scenePath)
    {
        // Sauvegarder la scène active si modifiée
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorSceneManager.SaveOpenScenes();
        }

        try
        {
            loadedPreviewScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            previewActive = true;
            previewingScenePath = scenePath;

            // Masquer visuellement toutes les autres scènes ouvertes pour n'avoir que l'aperçu
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s != loadedPreviewScene)
                {
                    foreach (var go in s.GetRootGameObjects())
                    {
                        SceneVisibilityManager.instance.Hide(go, true);
                    }
                }
            }

            FocusOnSceneObjects();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Impossible de charger l'aperçu de la scène '{scenePath}' : {e.Message}");
            ClosePreview();
        }
    }

    private void ClosePreview()
    {
        if (previewActive && loadedPreviewScene.IsValid() && loadedPreviewScene.isLoaded)
        {
            EditorSceneManager.CloseScene(loadedPreviewScene, true);
        }
        
        previewActive = false;
        previewingScenePath = "";

        // Réafficher toutes les scènes ouvertes à l'écran
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            foreach (var go in s.GetRootGameObjects())
            {
                SceneVisibilityManager.instance.Show(go, true);
            }
        }
    }
}
