using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UI.Interfaces.Crafting;

[CustomEditor(typeof(CraftingGridFiller))]
public class CraftingGridFillerEditor : Editor
{
    private bool showGridSettings = true;
    private bool showConnections = true;

    public override void OnInspectorGUI()
    {
        CraftingGridFiller filler = (CraftingGridFiller)target;

        // Synchroniser
        serializedObject.Update();

        // Propriétés de base
        SerializedProperty scrollViewport = serializedObject.FindProperty("scrollViewport");
        SerializedProperty contentPanel = serializedObject.FindProperty("contentPanel");
        SerializedProperty nodePrefab = serializedObject.FindProperty("nodePrefab");
        SerializedProperty linePrefab = serializedObject.FindProperty("linePrefab");
        SerializedProperty itemDescriptionPanel = serializedObject.FindProperty("itemDescriptionPanel");
        SerializedProperty rowsProp = serializedObject.FindProperty("rows");
        SerializedProperty columnsProp = serializedObject.FindProperty("columns");
        
        SerializedProperty topPadding = serializedObject.FindProperty("topPadding");
        SerializedProperty bottomPadding = serializedObject.FindProperty("bottomPadding");
        SerializedProperty sideMargin = serializedObject.FindProperty("sideMargin");
        SerializedProperty rowSpacing = serializedObject.FindProperty("rowSpacing");
        SerializedProperty lineThickness = serializedObject.FindProperty("lineThickness");

        // Dessiner la configuration de structure
        EditorGUILayout.LabelField("Structure & Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(scrollViewport);
        EditorGUILayout.PropertyField(contentPanel);
        EditorGUILayout.PropertyField(nodePrefab);
        EditorGUILayout.PropertyField(linePrefab);
        EditorGUILayout.PropertyField(itemDescriptionPanel);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Taille de la Grille", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rowsProp);
        EditorGUILayout.PropertyField(columnsProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mise en Page", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(topPadding);
        EditorGUILayout.PropertyField(bottomPadding);
        EditorGUILayout.PropertyField(sideMargin);
        EditorGUILayout.PropertyField(rowSpacing);
        EditorGUILayout.PropertyField(lineThickness);

        int cols = filler.Columns;
        int rows = filler.Rows;

        // ----------------------------------------------------
        // SECTION 1 : ÉDITEUR VISUEL DE LA GRILLE
        // ----------------------------------------------------
        showGridSettings = EditorGUILayout.Foldout(showGridSettings, "Disposition Visuelle de la Grille", true);

        if (showGridSettings)
        {
            EditorGUILayout.HelpBox("Cliquez sur une case pour ACTIVER ou DÉSACTIVER un nœud à cet emplacement.", MessageType.Info);

            for (int r = 0; r < rows; r++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Ligne {r}", GUILayout.Width(60));

                for (int c = 0; c < cols; c++)
                {
                    CraftingGridFiller.GridCell cell = filler.GetCell(c, r);

                    // Couleur du bouton : Vert si actif, Gris si inactif
                    Color originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = cell.isActive ? Color.green : Color.gray;

                    string btnLabel = cell.isActive 
                        ? (string.IsNullOrEmpty(cell.nodeName) ? "Actif" : cell.nodeName) 
                        : "Vide";

                    if (GUILayout.Button(btnLabel, GUILayout.Width(80), GUILayout.Height(30)))
                    {
                        cell.isActive = !cell.isActive;
                        filler.SetCell(c, r, cell);
                        EditorUtility.SetDirty(filler);
                    }

                    GUI.backgroundColor = originalColor;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuration des Nœuds Actifs", EditorStyles.boldLabel);

            SerializedProperty gridCellsProp = serializedObject.FindProperty("gridCells");

            // Permettre de configurer l'item produit et les SpecialItems requis pour chaque cellule active
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    CraftingGridFiller.GridCell cell = filler.GetCell(c, r);
                    if (cell.isActive)
                    {
                        int flatIndex = r * cols + c;
                        SerializedProperty cellProp = gridCellsProp.GetArrayElementAtIndex(flatIndex);

                        SerializedProperty itemProp = cellProp.FindPropertyRelative("item");
                        SerializedProperty nodeNameProp = cellProp.FindPropertyRelative("nodeName");
                        SerializedProperty specialItemsProp = cellProp.FindPropertyRelative("specialItemsRequired");

                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"Nœud [{c}, {r}]", EditorStyles.miniBoldLabel);
                        
                        EditorGUILayout.PropertyField(nodeNameProp, new GUIContent("Nom du Nœud"));
                        EditorGUILayout.PropertyField(itemProp, new GUIContent("Item produit"));
                        
                        // Dessin manuel et ergonomique de la liste des Special Items requis
                        EditorGUILayout.Space(2);
                        EditorGUILayout.LabelField("Special Items requis :", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        
                        int listSize = EditorGUILayout.IntField("Nombre d'ingrédients", specialItemsProp.arraySize);
                        if (listSize != specialItemsProp.arraySize)
                        {
                            specialItemsProp.arraySize = listSize;
                        }
                        
                        for (int i = 0; i < specialItemsProp.arraySize; i++)
                        {
                            SerializedProperty elementProp = specialItemsProp.GetArrayElementAtIndex(i);
                            SerializedProperty typeProp = elementProp.FindPropertyRelative("specialItem");
                            SerializedProperty amountProp = elementProp.FindPropertyRelative("amount");
                            
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(typeProp, GUIContent.none, GUILayout.MinWidth(120));
                            EditorGUILayout.LabelField("Qté :", GUILayout.Width(35));
                            EditorGUILayout.PropertyField(amountProp, GUIContent.none, GUILayout.Width(50));
                            
                            if (GUILayout.Button("-", GUILayout.Width(20)))
                            {
                                specialItemsProp.DeleteArrayElementAtIndex(i);
                                break;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        
                        if (GUILayout.Button("+ Ajouter un ingrédient", GUILayout.Height(20)))
                        {
                            specialItemsProp.arraySize++;
                        }
                        
                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }

        // ----------------------------------------------------
        // SECTION 2 : CONNEXIONS DE L'ARBRE (PARENT / ENFANT)
        // ----------------------------------------------------
        showConnections = EditorGUILayout.Foldout(showConnections, "Connexions (Parent = Requiert cet équipement)", true);

        if (showConnections)
        {
            EditorGUILayout.HelpBox("Créer une connexion signifie que l'équipement Parent sera REQUIS dans l'inventaire pour crafter l'équipement Enfant.", MessageType.Info);

            // Récupérer la liste des nœuds actifs pour peupler les listes déroulantes (Popups)
            List<int> activeFlatIndices = new List<int>();
            List<string> dropdownOptions = new List<string>();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    CraftingGridFiller.GridCell cell = filler.GetCell(c, r);
                    if (cell.isActive)
                    {
                        int flatIndex = r * cols + c;
                        activeFlatIndices.Add(flatIndex);
                        
                        string displayName = string.IsNullOrEmpty(cell.nodeName) ? $"Nœud [{c},{r}]" : cell.nodeName;
                        dropdownOptions.Add($"{displayName} ({c}, {r})");
                    }
                }
            }

            if (activeFlatIndices.Count < 2)
            {
                EditorGUILayout.HelpBox("Activez au moins deux nœuds dans la grille ci-dessus pour pouvoir créer des connexions.", MessageType.Warning);
            }
            else
            {
                SerializedProperty connectionsProp = serializedObject.FindProperty("connections");
                
                // Affichage et édition des connexions existantes
                for (int i = 0; i < connectionsProp.arraySize; i++)
                {
                    SerializedProperty connElement = connectionsProp.GetArrayElementAtIndex(i);
                    SerializedProperty parentFlatIndexProp = connElement.FindPropertyRelative("parentFlatIndex");
                    SerializedProperty childFlatIndexProp = connElement.FindPropertyRelative("childFlatIndex");

                    EditorGUILayout.BeginHorizontal("box");

                    // 1. Dropdown Parent (Equipement requis)
                    int currentParentIndex = activeFlatIndices.IndexOf(parentFlatIndexProp.intValue);
                    if (currentParentIndex == -1) currentParentIndex = 0;

                    EditorGUILayout.LabelField("Requis :", GUILayout.Width(55));
                    int newParentIndex = EditorGUILayout.Popup(currentParentIndex, dropdownOptions.ToArray(), GUILayout.MinWidth(120));
                    parentFlatIndexProp.intValue = activeFlatIndices[newParentIndex];

                    EditorGUILayout.Space();

                    // 2. Dropdown Enfant (Le craft produit)
                    int currentChildIndex = activeFlatIndices.IndexOf(childFlatIndexProp.intValue);
                    if (currentChildIndex == -1) currentChildIndex = 0;

                    EditorGUILayout.LabelField("Pour crafter :", GUILayout.Width(80));
                    int newChildIndex = EditorGUILayout.Popup(currentChildIndex, dropdownOptions.ToArray(), GUILayout.MinWidth(120));
                    childFlatIndexProp.intValue = activeFlatIndices[newChildIndex];

                    // 3. Bouton supprimer
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        connectionsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Ajouter une Connexion", GUILayout.Height(25)))
                {
                    connectionsProp.arraySize++;
                    // Assigner des valeurs de départ sûres
                    SerializedProperty newConn = connectionsProp.GetArrayElementAtIndex(connectionsProp.arraySize - 1);
                    newConn.FindPropertyRelative("parentFlatIndex").intValue = activeFlatIndices[0];
                    newConn.FindPropertyRelative("childFlatIndex").intValue = activeFlatIndices[activeFlatIndices.Count > 1 ? 1 : 0];
                }
            }
        }

        // ----------------------------------------------------
        // BOUTONS D'ACTION
        // ----------------------------------------------------
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Générer l'arbre (Scène)", GUILayout.Height(40)))
        {
            filler.GenerateGrid();
        }
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("Nettoyer", GUILayout.Height(40)))
        {
            filler.ClearGrid();
        }
        GUILayout.EndHorizontal();

        // Appliquer les changements sérialisés
        serializedObject.ApplyModifiedProperties();
    }
}
