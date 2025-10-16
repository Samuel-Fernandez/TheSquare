using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGeneratorManager : MonoBehaviour
{
    public EventContainer moveCameraPrefab;

    public static EventGeneratorManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public EventContainer MoveCamera(Vector2 spawnPosition, Vector2 endPosition, float moveDuration, float waitDuration)
    {
        // Instancier une nouvelle copie au lieu de modifier la même instance
        EventContainer newMoveCamera = ScriptableObject.Instantiate(moveCameraPrefab);

        newMoveCamera.eventsList[0].position = spawnPosition;
        newMoveCamera.eventsList[1].position = endPosition;
        newMoveCamera.eventsList[1].duration = moveDuration;
        newMoveCamera.eventsList[2].duration = waitDuration;

        return newMoveCamera;
    }
}
