#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CustomEditor(typeof(Quests))]
public class QuestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Synchronisation entre l'objet et le serializedObject
        serializedObject.Update();

        SerializedProperty id = serializedObject.FindProperty("id");
        SerializedProperty requirement = serializedObject.FindProperty("requirement");
        SerializedProperty completionCondition = serializedObject.FindProperty("completionCondition");
        SerializedProperty reward = serializedObject.FindProperty("reward");

        // Identification de la quête
        EditorGUILayout.PropertyField(id, new GUIContent("Quest ID"));

        EditorGUILayout.Space();

        // Quest Requirement
        EditorGUILayout.LabelField("Requirements", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(requirement, new GUIContent("Requirement"));

        switch ((QuestRequirement)requirement.enumValueIndex)
        {
            case QuestRequirement.DISCOVERY:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneData"), new GUIContent("Scene Data"));
                break;

            case QuestRequirement.COMMUNICATION:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pnjID"), new GUIContent("PNJ ID"));
                break;

            case QuestRequirement.EVENT:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventRequired"), new GUIContent("Event Required"));
                break;

            case QuestRequirement.SPECIAL_OBJECT:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("specialObject"), new GUIContent("Special Object"));
                break;

            case QuestRequirement.MONSTER_KILLED:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterRequired"), new GUIContent("Monster Required"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("nbMonsterRequired"), new GUIContent("Number of Monsters"));
                break;

            case QuestRequirement.QUEST:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("questsRequired"), new GUIContent("Required Quest"));
                break;

        }

        EditorGUILayout.Space();

        // Quest Completion Condition
        EditorGUILayout.LabelField("Completion Conditions", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(completionCondition, new GUIContent("Completion Condition"));

        switch ((QuestCompletionCondition)completionCondition.enumValueIndex)
        {
            case QuestCompletionCondition.KILL_MONSTER:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterObjectiveList"), new GUIContent("Monster Objectives"), true);
                break;

            case QuestCompletionCondition.RESOURCES:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("resourcesObjective"), new GUIContent("Resources Objectives"), true);
                break;

            case QuestCompletionCondition.DISCOVERY:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneDataObjective"), new GUIContent("Scene Objective"));
                break;

            case QuestCompletionCondition.COMMUNICATION:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pnjIDObjective"), new GUIContent("PNJ Objective ID"));
                break;

            case QuestCompletionCondition.EVENT:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventObjective"), new GUIContent("Event Objective"));
                break;
        }

        EditorGUILayout.Space();

        // Quest Reward
        EditorGUILayout.LabelField("Rewards", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(reward, new GUIContent("Reward"));

        switch ((QuestReward)reward.enumValueIndex)
        {
            case QuestReward.SQUARE_COINS:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("nbSquareCoins"), new GUIContent("Number of Square Coins"));
                break;

            case QuestReward.ITEMS:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rewardSpecialItem"), new GUIContent("Special Item Rewards"), true);
                break;

            case QuestReward.EQUIPEMENT:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rewardEquipement"), new GUIContent("Equipment Rewards"), true);
                break;
        }

        // Appliquer les modifications
        serializedObject.ApplyModifiedProperties();
    }
}
