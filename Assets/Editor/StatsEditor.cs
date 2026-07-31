using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Stats))]
public class StatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty entityTypeProp = serializedObject.FindProperty("entityType");

        // Dessine toutes les propriétés sauf monsterType
        DrawPropertiesExcluding(serializedObject, "monsterType");

        // N'affiche monsterType que si le type d'entité est Monster ou Boss
        if (entityTypeProp != null && (entityTypeProp.enumValueIndex == (int)EntityType.Monster || entityTypeProp.enumValueIndex == (int)EntityType.Boss))
        {
            SerializedProperty monsterTypeProp = serializedObject.FindProperty("monsterType");
            if (monsterTypeProp != null)
            {
                EditorGUILayout.PropertyField(monsterTypeProp);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
