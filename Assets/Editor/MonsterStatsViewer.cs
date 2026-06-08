using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MonsterStatsViewer : EditorWindow
{
    private Vector2 scrollPos;
    private List<GameObject> monsterPrefabs = new List<GameObject>();

    // Ajoute un nouvel onglet dans le menu Unity sous "Tools > Visualiseur Stats Monstres"
    [MenuItem("Tools/Visualiseur Stats Monstres")]
    public static void ShowWindow()
    {
        GetWindow<MonsterStatsViewer>("Stats Monstres");
    }

    private void OnEnable()
    {
        LoadPrefabs();
    }

    private void LoadPrefabs()
    {
        monsterPrefabs.Clear();
        // Dossiers cibles (à modifier si vous changez l'emplacement de vos monstres)
        string[] searchFolders = new string[] 
        { 
            "Assets/Prefabs/Game/Entities/LivingEntity/Monster", 
            "Assets/Prefabs/Game/Entities/LivingEntity/Bosses" 
        };

        // Recherche des prefabs dans les dossiers
        string[] guids = AssetDatabase.FindAssets("t:GameObject", searchFolders);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            // Si le prefab a un Composant Stats, on l'ajoute
            if (prefab != null && prefab.GetComponent<Stats>() != null)
            {
                monsterPrefabs.Add(prefab);
            }
        }
        
        // Trie par ordre alphabétique
        monsterPrefabs.Sort((a, b) => a.name.CompareTo(b.name));
    }

    private void OnGUI()
    {
        GUILayout.Space(5);
        if (GUILayout.Button("Rafraîchir les données", GUILayout.Width(200), GUILayout.Height(25)))
        {
            LoadPrefabs();
        }
        GUILayout.Space(5);

        // En-têtes fixes en haut
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Nom", GUILayout.Width(140));
        GUILayout.Label("Santé", GUILayout.Width(50));
        GUILayout.Label("Force", GUILayout.Width(50));
        GUILayout.Label("Vitesse", GUILayout.Width(60));
        GUILayout.Label("Défense", GUILayout.Width(60));
        GUILayout.Label("Chance", GUILayout.Width(60));
        GUILayout.Label("Dégâts Crit", GUILayout.Width(80));
        GUILayout.Label("Chance Crit", GUILayout.Width(80));
        GUILayout.Label("Résist. Recul", GUILayout.Width(80));
        GUILayout.Label("Puiss. Recul", GUILayout.Width(80));
        GUILayout.Label("Argent", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        // Zone défilable (Scroll View) pour les monstres
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var prefab in monsterPrefabs)
        {
            Stats stats = prefab.GetComponent<Stats>();
            if (stats == null) continue;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Affichage des données
            // En cliquant sur le nom, on sélectionne le prefab dans Unity
            if (GUILayout.Button(prefab.name, EditorStyles.label, GUILayout.Width(140)))
            {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }

            GUILayout.Label(stats.health.ToString(), GUILayout.Width(50));
            GUILayout.Label(stats.strength.ToString(), GUILayout.Width(50));
            GUILayout.Label(stats.speed.ToString(), GUILayout.Width(60));
            GUILayout.Label(stats.defense.ToString(), GUILayout.Width(60));
            GUILayout.Label(stats.luck.ToString(), GUILayout.Width(60));
            GUILayout.Label(stats.critDamage.ToString(), GUILayout.Width(80));
            GUILayout.Label(stats.critChance.ToString(), GUILayout.Width(80));
            GUILayout.Label(stats.knockbackResistance.ToString(), GUILayout.Width(80));
            GUILayout.Label(stats.knockbackPower.ToString(), GUILayout.Width(80));
            GUILayout.Label(stats.money.ToString(), GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}
