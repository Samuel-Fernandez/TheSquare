using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SceneChanger))]
public class SceneChangerEditor : Editor
{
    private SceneChanger sceneChanger;
    private bool isEditingPosition = false;
    private Scene loadedTargetScene;

    private void OnEnable()
    {
        sceneChanger = (SceneChanger)target;
    }

    private void OnDisable()
    {
        // Nettoyage au cas où on perd le focus (clic ailleurs)
        if (isEditingPosition)
        {
            CloseTargetScene();
        }
    }

    public override void OnInspectorGUI()
    {
        // Dessiner l'inspecteur par défaut
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Outils d'Édition Visuelle", EditorStyles.boldLabel);

        if (string.IsNullOrEmpty(sceneChanger.scene))
        {
            EditorGUILayout.HelpBox("Veuillez renseigner le nom de la scène cible pour utiliser l'outil visuel.", MessageType.Warning);
            return;
        }

        if (!isEditingPosition)
        {
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Éditer la destination visuellement", GUILayout.Height(30)))
            {
                OpenTargetScene();
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Confirmer et fermer la vue cible", GUILayout.Height(30)))
            {
                CloseTargetScene();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.HelpBox("Déplacez la petite sphère/poignée dans la fenêtre 'Scène' pour définir les coordonnées d'arrivée.", MessageType.Info);
        }
    }

    private void OnSceneGUI()
    {
        if (isEditingPosition && sceneChanger != null)
        {
            EditorGUI.BeginChangeCheck();

            // S'assurer que les coordonnées modifiées sont bien dans le monde 2D.
            // Affiche un pointeur de positionnement manipulable dans la vue scène
            Vector3 targetPosition3D = new Vector3(sceneChanger.newPosition.x, sceneChanger.newPosition.y, 0);
            Vector3 newTargetPosition = Handles.PositionHandle(targetPosition3D, Quaternion.identity);

            // Ajouter une sphère visuelle pour qu'elle soit plus visible
            Handles.color = Color.green;
            Handles.DrawWireDisc(newTargetPosition, Vector3.forward, 0.5f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sceneChanger, "Changement de destination SceneChanger");
                sceneChanger.newPosition = new Vector2(newTargetPosition.x, newTargetPosition.y);
                
                // Forcer la sauvegarde des modifications
                EditorUtility.SetDirty(sceneChanger);
            }

            // Dessiner un texte
            Handles.Label(newTargetPosition + Vector3.up * 0.75f, "Point d'arrivée\n(" + sceneChanger.scene + ")", EditorStyles.whiteBoldLabel);
        }
    }

    private void OpenTargetScene()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Cette fonctionnalité est réservée à l'éditeur, en dehors du mode Jeu.");
            return;
        }

        // 1. Chercher le chemin de la scène cible dans le projet
        string scenePath = GetScenePathByName(sceneChanger.scene);

        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError($"Scène '{sceneChanger.scene}' introuvable. Avez-vous oublié de vérifier son orthographe ?");
            return;
        }

        // 2. Sauvegarder la scène actuelle au cas où pour éviter les messages d'erreur Unity
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorSceneManager.SaveOpenScenes();
        }

        // 3. Charger la scène de manière additive en fond
        loadedTargetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        isEditingPosition = true;

        // 3.5 Masquer les autres scènes pour ne voir que la destination
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s != loadedTargetScene)
            {
                foreach (var go in s.GetRootGameObjects())
                    SceneVisibilityManager.instance.Hide(go, true);
            }
        }

        // 3.6 S'assurer que l'objet édité reste visible (sinon ses poignées et événements OnSceneGUI sont désactivés par Unity)
        if (sceneChanger != null)
        {
            SceneVisibilityManager.instance.Show(sceneChanger.gameObject, false);
        }
        
        // 4. Centrer la caméra de la "vue scène" sur le point d'apparition actuel
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.Frame(new Bounds(new Vector3(sceneChanger.newPosition.x, sceneChanger.newPosition.y, 0), Vector3.one * 5f), false);
        }
    }

    private void CloseTargetScene()
    {
        if (loadedTargetScene.IsValid() && loadedTargetScene.isLoaded)
        {
            // Fermer la scène additive
            EditorSceneManager.CloseScene(loadedTargetScene, true);
        }
        isEditingPosition = false;

        // Réafficher les autres scènes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            foreach (var go in s.GetRootGameObjects())
            {
                SceneVisibilityManager.instance.Show(go, true);
            }
        }
        
        // Sauvegarder explicitement l'objet modifié dans sa scène d'origine
        if(sceneChanger != null)
        {
             EditorSceneManager.MarkSceneDirty(sceneChanger.gameObject.scene);
        }
    }

    private string GetScenePathByName(string sceneName)
    {
        // 1. Chercher dans les scènes du build
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (buildScene.path.Contains(sceneName + ".unity"))
            {
                return buildScene.path;
            }
        }
        
        // 2. Recherche globale dans tout le projet (si ce n'est pas ajouté au build)
        string[] guids = AssetDatabase.FindAssets("t:Scene " + sceneName);
        if (guids.Length > 0)
        {
            foreach(string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if(Path.GetFileNameWithoutExtension(path) == sceneName)
                {
                    return path;
                }
            }
        }

        return null; // Scène non trouvée
    }
}
