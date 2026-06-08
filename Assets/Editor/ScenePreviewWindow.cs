using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class ScenePreviewWindow : EditorWindow
{
    private Scene loadedPreviewScene;
    private bool previewActive = false;
    private string previewingScenePath = "";
    private Vector2 scrollPos;
    private string searchQuery = "";

    [MenuItem("Tools/Aperçu des Scènes")]
    public static void ShowWindow()
    {
        GetWindow<ScenePreviewWindow>("Aperçu de Scènes");
    }

    private void OnDestroy()
    {
        // Nettoyer si la fenêtre est fermée avec l'aperçu encore ouvert
        ClosePreview();
    }

    private void OnGUI()
    {
        GUILayout.Label("Navigateur et Aperçu des Scènes", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Chargez visuellement une scène en arrière-plan (Additif) pour la consulter sans quitter votre scène actuelle, puis fermez l'aperçu.", MessageType.Info);

        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("L'outil est désactivé en mode Jeu.", MessageType.Warning);
            return;
        }

        if (previewActive)
        {
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Fermer l'aperçu actuel (" + Path.GetFileNameWithoutExtension(previewingScenePath) + ")", GUILayout.Height(30)))
            {
                ClosePreview();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();
        }

        // Barre de recherche
        GUILayout.BeginHorizontal();
        GUILayout.Label("Recherche :", GUILayout.Width(75));
        searchQuery = EditorGUILayout.TextField(searchQuery);
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            searchQuery = "";
            GUI.FocusControl(null);
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        string[] guids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Ignorer les scènes venant de packages pour ne garder que celles de votre jeu
            if (!path.StartsWith("Assets/")) continue;

            string sceneName = Path.GetFileNameWithoutExtension(path);

            if (!string.IsNullOrEmpty(searchQuery) && !sceneName.ToLower().Contains(searchQuery.ToLower()))
                continue;

            GUILayout.BeginHorizontal("box");
            GUILayout.Label(sceneName, GUILayout.Width(150));

            bool isCurrentScene = EditorSceneManager.GetActiveScene().path == path;

            // Bouton Aperçu
            if (!previewActive && !isCurrentScene)
            {
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("Aperçu visuel"))
                {
                    PreviewScene(path);
                }
                GUI.backgroundColor = Color.white;
            }
            else if (previewActive && previewingScenePath == path)
            {
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Centrer la vue"))
                {
                    if (SceneView.lastActiveSceneView != null)
                    {
                        SceneView.lastActiveSceneView.Frame(new Bounds(Vector3.zero, Vector3.one * 10f), false);
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button(isCurrentScene ? "Déjà ouverte" : "Aperçu visuel");
                GUI.enabled = true;
            }

            // Bouton Ouvrir normalement
            if (!isCurrentScene && (!previewActive || previewingScenePath != path))
            {
                if (GUILayout.Button("Ouvrir la scène", GUILayout.Width(120)))
                {
                    if (previewActive) ClosePreview();

                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    }
                }
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button("Ouvrir la scène", GUILayout.Width(120));
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void PreviewScene(string scenePath)
    {
        // Sauvegarder la scène courante si elle a été modifiée pour éviter de perdre le travail
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorSceneManager.SaveOpenScenes();
        }

        try
        {
            loadedPreviewScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            previewActive = true;
            previewingScenePath = scenePath;

            // Masquer les autres scènes
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s != loadedPreviewScene)
                {
                    foreach (var go in s.GetRootGameObjects())
                        SceneVisibilityManager.instance.Hide(go, true);
                }
            }
            
            // Recadrer la vue scène sur le centre (0,0,0)
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.Frame(new Bounds(Vector3.zero, Vector3.one * 10f), false);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Impossible de charger l'aperçu : " + e.Message);
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

        // Réafficher les autres scènes
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
