using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EventType
{
    PNJ,
    BATTLE,
    CAMERA,
    WAIT,
    TEXT,
    CHANGE_SCENE,
    SPECIAL_METHODS,
    BOOK,
    CINEMATIC
}

public enum CameraEventType
{
    SPAWN,
    REMOVE,
    MOVE,
    EFFECT
}

public enum CameraEffect
{
    ZOOM,
    DEZOOM,
    COLOR_CHANGE,
    SHAKE
}

public enum BattleEventType
{
    SPAWN,
}

[System.Serializable]
public enum PnjEventType
{
    SPAWN,
    MOVE,
    EMOTIONS,
    SPEAK,
    ANIM,
    REMOVE,
}

[System.Serializable]
public enum PnjEmotions
{
    IN_LOVE,
    ANGRY,
    EMBARASSED,
    JUMP,
    SPEAKING,
}

[System.Serializable]
public enum CinematicSpriteEffect
{
    NONE,
    SHAKING,
    SATURATION,
    NEGATIVE,
}

[System.Serializable]
public class TextSegment
{
    public string textID;
    public float duration;
    public AudioClip sound;
}

[System.Serializable]
public class CinematicContainer
{
    public Sprite sprite;
    public AudioClip music;
    public List<TextSegment> texts;
    public AudioClip sound;

    public CinematicSpriteEffect spriteEffect;
    public float durationEffect;
    public float powerEffect;
}


[CreateAssetMenu(fileName = "new Event", menuName = "Events/Event", order = 1)]
public class Event : ScriptableObject
{
    public EventType eventType;
    public AudioClip music;
    public AudioClip sound;
    public bool stopTime;
    public bool playerCanMove;
    public bool stopMusic;

    // common attributes
    public Vector2 position;
    public float duration;

    // cinematic type
    public List<CinematicContainer> cinematicContainer;

    // pnj types
    public bool alreadyOnScene;
    public PnjEventType pnjType;
    public List<string> idPnj;
    public bool absolutePosition;

    // attributes pnj
    public string idText;
    public PnjEmotions emotions;
    public bool lastSpriteStay;

    // camera type
    public CameraEventType cameraType;
    public CameraEffect cameraEffect;
    public float amplitudeShake;
    public float frequencyShake;
    public float zoomPower;
    public Color colorChange;

    // battle type
    public bool canLeave;
    public BattleEventType battleType;
    public float colliderRadius;
    public List<string> idMonster;
    public List<Vector2> spawnMonsterPosition;

    // SPECIAL METHODS
    public string targetObjectName; // Nom de l'objet sur lequel exécuter la méthode
    public string componentType;    // Type du composant (ex: "LifeManager")
    public string methodName;       // Nom de la méthode (ex: "TakeDamage")
    public string[] parameters;     // Paramètres sous forme de chaînes (ex: ["4"])

    //BOOK
    public Sprite bookPage;
}
