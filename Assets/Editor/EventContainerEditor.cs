#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CustomEditor(typeof(EventContainer))]
public class EventContainerEditor : Editor
{
    private SerializedProperty idProp;
    private SerializedProperty cameraProp;
    private SerializedProperty pnjContainerProp;
    private SerializedProperty monsterContainerProp;
    private SerializedProperty eventsListProp;
    private SerializedProperty requirementsProp;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        idProp = serializedObject.FindProperty("ID");
        cameraProp = serializedObject.FindProperty("camera");
        pnjContainerProp = serializedObject.FindProperty("pnjContainer");
        monsterContainerProp = serializedObject.FindProperty("monsterContainer");
        eventsListProp = serializedObject.FindProperty("eventsList");
        requirementsProp = serializedObject.FindProperty("requirements");

        EditorGUILayout.PropertyField(idProp, new GUIContent("Event ID"));
        EditorGUILayout.PropertyField(cameraProp, new GUIContent("Camera"));
        EditorGUILayout.PropertyField(pnjContainerProp, new GUIContent("PNJ Container"), true);
        EditorGUILayout.PropertyField(monsterContainerProp, new GUIContent("Monster Container"), true);
        EditorGUILayout.PropertyField(eventsListProp, new GUIContent("Event Steps"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Requirements", EditorStyles.boldLabel);

        for (int i = 0; i < requirementsProp.arraySize; i++)
        {
            SerializedProperty requirement = requirementsProp.GetArrayElementAtIndex(i);
            SerializedProperty typeProp = requirement.FindPropertyRelative("requirementType");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(typeProp, new GUIContent("Requirement Type"));

            switch ((EventRequirementType)typeProp.enumValueIndex)
            {
                case EventRequirementType.DISCOVERY:
                    EditorGUILayout.PropertyField(requirement.FindPropertyRelative("sceneData"), new GUIContent("Scene"));
                    break;

                case EventRequirementType.COMMUNICATION:
                    EditorGUILayout.PropertyField(requirement.FindPropertyRelative("pnjIDSpoken"), new GUIContent("PNJ ID"));
                    break;

                case EventRequirementType.EVENT:
                    EditorGUILayout.PropertyField(requirement.FindPropertyRelative("eventRequired"), new GUIContent("Event Required"));
                    break;

                case EventRequirementType.SPECIAL_OBJECT:
                    EditorGUILayout.PropertyField(requirement.FindPropertyRelative("specialObject"), new GUIContent("Special Object"));
                    break;

                case EventRequirementType.MONSTER_KILLED:
                    EditorGUILayout.PropertyField(requirement.FindPropertyRelative("monsterRequired"), new GUIContent("Monster"));
                    EditorGUILayout.PropertyField(requirement.FindPropertyRelative("nbMonsterRequired"), new GUIContent("Quantity"));
                    break;
            }

            if (GUILayout.Button("Remove Condition"))
            {
                requirementsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add New Condition"))
        {
            requirementsProp.InsertArrayElementAtIndex(requirementsProp.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
