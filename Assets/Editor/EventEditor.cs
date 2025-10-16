#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CustomEditor(typeof(Event))]
public class EventEditor : Editor
{
    SerializedProperty eventTypeProp;
    SerializedProperty musicProp;
    SerializedProperty soundProp;
    SerializedProperty stopTimeProp;
    SerializedProperty playerCanMoveProp;
    SerializedProperty stopMusicProp;
    SerializedProperty bookPageProp;
    SerializedProperty idTextProp;
    SerializedProperty cinematicContainerProp;
    SerializedProperty idPnjProp;
    SerializedProperty pnjTypeProp;
    SerializedProperty alreadyOnSceneProp;
    SerializedProperty positionProp;
    SerializedProperty durationProp;
    SerializedProperty absolutePositionProp;
    SerializedProperty emotionsProp;
    SerializedProperty lastSpriteStayProp;
    SerializedProperty battleTypeProp;
    SerializedProperty idMonsterProp;
    SerializedProperty spawnMonsterPositionProp;
    SerializedProperty canLeaveProp;
    SerializedProperty colliderRadiusProp;
    SerializedProperty cameraTypeProp;
    SerializedProperty cameraEffectProp;
    SerializedProperty zoomPowerProp;
    SerializedProperty frequencyShakeProp;
    SerializedProperty amplitudeShakeProp;
    SerializedProperty colorChangeProp;
    SerializedProperty targetObjectNameProp;
    SerializedProperty componentTypeProp;
    SerializedProperty methodNameProp;
    SerializedProperty parametersProp;

    void OnEnable()
    {
        eventTypeProp = serializedObject.FindProperty("eventType");
        musicProp = serializedObject.FindProperty("music");
        soundProp = serializedObject.FindProperty("sound");
        stopTimeProp = serializedObject.FindProperty("stopTime");
        playerCanMoveProp = serializedObject.FindProperty("playerCanMove");
        stopMusicProp = serializedObject.FindProperty("stopMusic");

        bookPageProp = serializedObject.FindProperty("bookPage");
        idTextProp = serializedObject.FindProperty("idText");
        cinematicContainerProp = serializedObject.FindProperty("cinematicContainer");
        idPnjProp = serializedObject.FindProperty("idPnj");
        pnjTypeProp = serializedObject.FindProperty("pnjType");
        alreadyOnSceneProp = serializedObject.FindProperty("alreadyOnScene");

        positionProp = serializedObject.FindProperty("position");
        durationProp = serializedObject.FindProperty("duration");
        absolutePositionProp = serializedObject.FindProperty("absolutePosition");
        emotionsProp = serializedObject.FindProperty("emotions");
        lastSpriteStayProp = serializedObject.FindProperty("lastSpriteStay");

        battleTypeProp = serializedObject.FindProperty("battleType");
        idMonsterProp = serializedObject.FindProperty("idMonster");
        spawnMonsterPositionProp = serializedObject.FindProperty("spawnMonsterPosition");
        canLeaveProp = serializedObject.FindProperty("canLeave");
        colliderRadiusProp = serializedObject.FindProperty("colliderRadius");

        cameraTypeProp = serializedObject.FindProperty("cameraType");
        cameraEffectProp = serializedObject.FindProperty("cameraEffect");
        zoomPowerProp = serializedObject.FindProperty("zoomPower");
        frequencyShakeProp = serializedObject.FindProperty("frequencyShake");
        amplitudeShakeProp = serializedObject.FindProperty("amplitudeShake");
        colorChangeProp = serializedObject.FindProperty("colorChange");

        targetObjectNameProp = serializedObject.FindProperty("targetObjectName");
        componentTypeProp = serializedObject.FindProperty("componentType");
        methodNameProp = serializedObject.FindProperty("methodName");
        parametersProp = serializedObject.FindProperty("parameters");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(eventTypeProp);
        EditorGUILayout.PropertyField(musicProp);
        EditorGUILayout.PropertyField(soundProp);
        EditorGUILayout.PropertyField(stopTimeProp);
        EditorGUILayout.PropertyField(playerCanMoveProp);
        EditorGUILayout.PropertyField(stopMusicProp);

        EventType currentType = (EventType)eventTypeProp.enumValueIndex;

        switch (currentType)
        {
            case EventType.BOOK:
                EditorGUILayout.PropertyField(bookPageProp);
                EditorGUILayout.PropertyField(idTextProp);
                break;

            case EventType.CINEMATIC:
                EditorGUILayout.PropertyField(cinematicContainerProp, new GUIContent("Cinematic Container"), true);
                break;

            case EventType.PNJ:
                EditorGUILayout.PropertyField(idPnjProp, new GUIContent("PNJ IDs"), true);
                EditorGUILayout.PropertyField(pnjTypeProp);
                EditorGUILayout.PropertyField(alreadyOnSceneProp);

                var pnjType = (PnjEventType)pnjTypeProp.enumValueIndex;
                switch (pnjType)
                {
                    case PnjEventType.SPAWN:
                        EditorGUILayout.PropertyField(positionProp, new GUIContent("Spawn Position"));
                        break;
                    case PnjEventType.MOVE:
                        EditorGUILayout.PropertyField(positionProp, new GUIContent("Move Position"));
                        EditorGUILayout.PropertyField(durationProp);
                        EditorGUILayout.PropertyField(absolutePositionProp);
                        break;
                    case PnjEventType.EMOTIONS:
                        EditorGUILayout.PropertyField(emotionsProp);
                        break;
                    case PnjEventType.SPEAK:
                        EditorGUILayout.PropertyField(idTextProp);
                        break;
                    case PnjEventType.ANIM:
                        EditorGUILayout.PropertyField(idTextProp, new GUIContent("Animation Name"));
                        EditorGUILayout.PropertyField(lastSpriteStayProp);
                        break;
                }
                break;

            case EventType.BATTLE:
                EditorGUILayout.PropertyField(battleTypeProp);
                EditorGUILayout.PropertyField(idMonsterProp, new GUIContent("Monsters IDs"), true);
                EditorGUILayout.PropertyField(spawnMonsterPositionProp, new GUIContent("Spawn Position"), true);

                var battleType = (BattleEventType)battleTypeProp.enumValueIndex;
                if (battleType == BattleEventType.SPAWN)
                {
                    EditorGUILayout.PropertyField(canLeaveProp);
                    if (!canLeaveProp.boolValue)
                        EditorGUILayout.PropertyField(colliderRadiusProp);

                    EditorGUILayout.PropertyField(positionProp, new GUIContent("Collider Center"));
                }
                break;

            case EventType.CAMERA:
                EditorGUILayout.PropertyField(cameraTypeProp);
                var camType = (CameraEventType)cameraTypeProp.enumValueIndex;
                if (camType == CameraEventType.SPAWN || camType == CameraEventType.MOVE)
                {
                    EditorGUILayout.PropertyField(positionProp);
                    if (camType == CameraEventType.MOVE)
                        EditorGUILayout.PropertyField(durationProp);
                }
                else if (camType == CameraEventType.EFFECT)
                {
                    EditorGUILayout.PropertyField(cameraEffectProp);
                    EditorGUILayout.PropertyField(durationProp);

                    var effect = (CameraEffect)cameraEffectProp.enumValueIndex;
                    if (effect == CameraEffect.DEZOOM || effect == CameraEffect.ZOOM)
                        EditorGUILayout.PropertyField(zoomPowerProp);
                    else if (effect == CameraEffect.SHAKE)
                    {
                        EditorGUILayout.PropertyField(frequencyShakeProp);
                        EditorGUILayout.PropertyField(amplitudeShakeProp);
                    }
                    else if (effect == CameraEffect.COLOR_CHANGE)
                        EditorGUILayout.PropertyField(colorChangeProp);
                }
                break;

            case EventType.WAIT:
                EditorGUILayout.PropertyField(durationProp);
                break;

            case EventType.TEXT:
                EditorGUILayout.PropertyField(idTextProp);
                break;

            case EventType.CHANGE_SCENE:
                EditorGUILayout.PropertyField(idTextProp, new GUIContent("Scene Name"));
                break;

            case EventType.SPECIAL_METHODS:
                EditorGUILayout.PropertyField(targetObjectNameProp);
                EditorGUILayout.PropertyField(componentTypeProp);
                EditorGUILayout.PropertyField(methodNameProp);
                EditorGUILayout.PropertyField(parametersProp, true);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
