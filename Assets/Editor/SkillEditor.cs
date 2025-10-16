#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

[CustomEditor(typeof(Skill))]
public class SkillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Skill skill = (Skill)target;

        // Affichage des variables de la classe Skill
        skill.id = EditorGUILayout.TextField("ID", skill.id);
        skill.lvlLifeMin = EditorGUILayout.IntField("Level Vie Min", skill.lvlLifeMin);
        skill.lvlStrengthMin = EditorGUILayout.IntField("Level Force Min", skill.lvlStrengthMin);
        skill.lvlLuckMin = EditorGUILayout.IntField("Level Chance Min", skill.lvlLuckMin);
        skill.cost = EditorGUILayout.IntField("Coût", skill.cost);

        // Liste des compétences requises
        SerializedProperty previousSkillsProperty = serializedObject.FindProperty("previousSkills");
        EditorGUILayout.PropertyField(previousSkillsProperty, new GUIContent("Compétences Précédentes"), true);

        skill.img = (Sprite)EditorGUILayout.ObjectField("Image", skill.img, typeof(Sprite), false);
        skill.type = (BONUS_TYPE)EditorGUILayout.EnumPopup("Type de Bonus", skill.type);

        // Afficher le champ "statAdd" uniquement si le type de bonus est ADD_STATS
        if (skill.type == BONUS_TYPE.ADD_STATS)
        {
            skill.statAdd = (STATS_ADD)EditorGUILayout.EnumPopup("Statistique à Ajouter", skill.statAdd);

            // Afficher intValue si statAdd est HP ou STR
            if (skill.statAdd == STATS_ADD.HP || skill.statAdd == STATS_ADD.STR || skill.statAdd == STATS_ADD.LUCK)
            {
                skill.intValue = EditorGUILayout.IntField("Valeur Int", skill.intValue);
            }
            // Afficher floatValue pour SPE, KBP, KBR, CRITC, CRITD
            else if (skill.statAdd == STATS_ADD.SPE || skill.statAdd == STATS_ADD.KBP || skill.statAdd == STATS_ADD.KBR || skill.statAdd == STATS_ADD.CRITC || skill.statAdd == STATS_ADD.CRITD)
            {
                skill.floatValue = EditorGUILayout.FloatField("Valeur Float", skill.floatValue);
            }
        }
        else if (skill.type == BONUS_TYPE.REGEN)
        {
            skill.intValue = EditorGUILayout.IntField("Valeur Int", skill.intValue);
        }
        else
        {
            // Afficher uniquement floatValue pour les autres types de bonus
            skill.floatValue = EditorGUILayout.FloatField("Valeur Float", skill.floatValue);
        }

        // Appliquer les modifications
        serializedObject.ApplyModifiedProperties();

        // Si d'autres modifications ont été effectuées, marque l'objet comme modifié
        if (GUI.changed)
        {
            EditorUtility.SetDirty(skill);
        }
    }
}
