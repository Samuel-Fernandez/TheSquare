using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;


public class EventPlayer : MonoBehaviour
{
    public EventContainer eventContainer;
    public GameObject cloudPrefab;
    string id;
    bool state = true;

    bool eventStarted = false;

    public bool removeObjectWhenEventEnd = true;
    public bool isTriggerCollisionEvent = true;

    //Book Event
    public GameObject container;
    public Image bookImage;

    private void Start()
    {
        if (eventContainer && eventContainer.ID != null)
            id = eventContainer.ID;

        if (id != null && SaveManager.instance.twoStateContainer.TryGetState(id, out state))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isTriggerCollisionEvent && collision.GetComponent<Stats>() && collision.GetComponent<Stats>().entityType == EntityType.Player && !eventStarted && eventContainer.RequirementsGood())
        {
            PlayAnimation();
        }
    }

    public void PlayAnimation()
    {

        // V�rifie si l'ID existe avec n'importe quel �tat
        if (id != null && !SaveManager.instance.twoStateContainer.TryGetState(id, out state) && eventContainer.RequirementsGood())
        {
            eventStarted = true;
            StartCoroutine(RoutinePlayAnimation());
        }
        // Ne pas mettre d'ID pour les events g�n�r�s
        else if (id == null)
        {
            eventStarted = true;
            StartCoroutine(RoutinePlayAnimation());
        }
    }


    void ResetActions()
    {
        // Considrer l'vnement comme effectu
        if (id != null)
            SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, true);

        PlayerManager.instance.isEventPlaying = false;
        PlayerManager.instance.player.GetComponent<PlayerAnimation>().off = false;
        PlayerManager.instance.player.GetComponent<Stats>().canMove = true;
        Time.timeScale = 1f;
        MeteoManager.instance.SetTime(true);
        SoundManager.instance.PlayMusic(MeteoManager.instance.actualScene.music);
        MeteoManager.instance.SetSceneFilter();

        if (removeObjectWhenEventEnd)
            gameObject.SetActive(false);

        if (MeteoManager.instance.actualScene.playerCantOpenInventory)
        {
            InventoryManager.instance.canOpenInventory = false;
            QuestManager.instance.canOpenQuests = false;
        }
        else
        {
            InventoryManager.instance.canOpenInventory = true;
            QuestManager.instance.canOpenQuests = true;
        }


    }

    IEnumerator RoutinePlayAnimation()
    {
        PlayerManager.instance.isEventPlaying = true;
        // Clone eventContainer
        EventContainer clonedEventContainer = ScriptableObjectUtility.Clone(eventContainer);

        foreach (Event actualEvent in clonedEventContainer.eventsList)
        {

            PlayerManager.instance.player.GetComponent<Stats>().canMove = actualEvent.playerCanMove;
            InventoryManager.instance.canOpenInventory = false;
            QuestManager.instance.canOpenQuests = false;

            // V�rifiez et jouez le son si non null
            if (actualEvent.sound != null)
            {
                SoundManager.instance.PlayUISound(actualEvent.sound, 1.0f);
            }

            if (actualEvent.stopMusic)
                SoundManager.instance.StopMusic();

            if (actualEvent.music != null)
                SoundManager.instance.PlayMusic(actualEvent.music);



            if (actualEvent.stopTime)
                MeteoManager.instance.SetTime(false);
            else
                MeteoManager.instance.SetTime(true);

            if (actualEvent.eventType == EventType.PNJ)
            {
                if (actualEvent.pnjType == PnjEventType.SPAWN)
                {
                    SpawnPNJ(clonedEventContainer, actualEvent.idPnj, actualEvent.position, actualEvent.alreadyOnScene);
                }
                else if (actualEvent.pnjType == PnjEventType.MOVE)
                {
                    yield return StartCoroutine(MovePNJ(clonedEventContainer, actualEvent.idPnj, actualEvent.position, actualEvent.duration, actualEvent.absolutePosition));
                }
                else if (actualEvent.pnjType == PnjEventType.EMOTIONS)
                {
                    foreach (string idPnj in actualEvent.idPnj)
                    {
                        for (int i = 0; i < eventContainer.pnjContainer.Count; i++)
                        {
                            if (idPnj == "Player" || idPnj == "player")
                            {
                                PlayerManager.instance.player.GetComponent<PNJEmotions>().Emotion(actualEvent.emotions);
                            }
                            else if (eventContainer.pnjContainer[i].id == idPnj)
                            {
                                clonedEventContainer.pnjContainer[i].pnj.GetComponent<PNJEmotions>().Emotion(actualEvent.emotions);
                            }
                        }
                    }

                    yield return new WaitForSecondsRealtime(1); // dur�e d'une �motion
                }
                else if (actualEvent.pnjType == PnjEventType.SPEAK)
                {
                    NotificationManager.instance.ShowBubble(LocalizationManager.instance.GetTexts("pnj_text", actualEvent.idText));

                    while (NotificationManager.instance.bubbleInstance != null)
                    {
                        yield return null;
                    }
                }
                else if (actualEvent.pnjType == PnjEventType.ANIM)
                {
                    // idText est recycl� pour correspondre � l'animation
                    AnimPNJ(clonedEventContainer, actualEvent.idPnj, actualEvent.idText, actualEvent.lastSpriteStay);
                }
                else if (actualEvent.pnjType == PnjEventType.REMOVE)
                {
                    RemovePNJ(clonedEventContainer, actualEvent.idPnj);
                }
            }
            else if (actualEvent.eventType == EventType.BATTLE)
            {
                PlayerAnimation playerAnim = PlayerManager.instance.player.GetComponent<PlayerAnimation>();
                playerAnim.off = false;

                if (actualEvent.battleType == BattleEventType.SPAWN)
                {
                    SpawnMonster(clonedEventContainer, actualEvent.idMonster, actualEvent.spawnMonsterPosition, actualEvent.colliderRadius);
                    InventoryManager.instance.canOpenInventory = true;
                    QuestManager.instance.canOpenQuests = true;
                    GameObject battleZone = null; // Utiliser le premier point comme centre du cercle



                    if (!actualEvent.canLeave)
                    {
                        battleZone = CreateBattleZone(actualEvent.position, actualEvent.colliderRadius, actualEvent.colliderRadius + 2); // Utiliser le premier point comme centre du cercle

                        while (!AllMonstersDead(clonedEventContainer))
                        {
                            yield return null;
                        }
                    }

                    // Tous les monstres restants sont d�truits ici
                    foreach (var monsterContainer in clonedEventContainer.monsterContainer)
                    {
                        if (monsterContainer.monster != null)
                        {
                            Destroy(monsterContainer.monster);
                        }
                    }


                    InventoryManager.instance.canOpenInventory = false;
                    QuestManager.instance.canOpenQuests = false;

                    if (battleZone != null)
                    {
                        Destroy(battleZone);
                        battleZone = null;
                    }

                    playerAnim.off = true;



                }
            }
            else if (actualEvent.eventType == EventType.CAMERA)
            {
                if (actualEvent.cameraType == CameraEventType.SPAWN)
                {
                    SpawnCamera(clonedEventContainer, actualEvent.position);
                }
                else if (actualEvent.cameraType == CameraEventType.MOVE)
                {
                    MoveCamera(clonedEventContainer.camera, actualEvent.position, actualEvent.duration);
                    yield return new WaitForSecondsRealtime(actualEvent.duration);
                }
                else if (actualEvent.cameraType == CameraEventType.EFFECT)
                {
                    if (actualEvent.cameraEffect == CameraEffect.SHAKE)
                    {
                        CameraManager.instance.ShakeCamera(actualEvent.amplitudeShake, actualEvent.frequencyShake, actualEvent.duration, clonedEventContainer.camera.GetComponent<CinemachineVirtualCamera>());
                        yield return new WaitForSecondsRealtime(actualEvent.duration);

                    }
                    else if (actualEvent.cameraEffect == CameraEffect.COLOR_CHANGE)
                    {
                        CameraManager.instance.ChangeCameraColor(actualEvent.colorChange, actualEvent.duration, CameraManager.instance.defaultCamera);
                        yield return new WaitForSecondsRealtime(actualEvent.duration);

                    }
                    else if (actualEvent.cameraEffect == CameraEffect.ZOOM)
                    {
                        CameraManager.instance.ZoomCamera(actualEvent.zoomPower, actualEvent.duration, clonedEventContainer.camera.GetComponent<CinemachineVirtualCamera>());
                        yield return new WaitForSecondsRealtime(actualEvent.duration);


                    }
                    else if (actualEvent.cameraEffect == CameraEffect.DEZOOM)
                    {
                        CameraManager.instance.DezoomCamera(actualEvent.zoomPower, actualEvent.duration, clonedEventContainer.camera.GetComponent<CinemachineVirtualCamera>());
                        yield return new WaitForSecondsRealtime(actualEvent.duration);

                    }
                }
                else if (actualEvent.cameraType == CameraEventType.REMOVE)
                {
                    RemoveCamera(clonedEventContainer);
                }
            }
            else if (actualEvent.eventType == EventType.WAIT)
            {
                yield return new WaitForSecondsRealtime(actualEvent.duration);
            }
            else if (actualEvent.eventType == EventType.TEXT)
            {
                NotificationManager.instance.ShowBubble(LocalizationManager.instance.GetTexts("pnj_text", actualEvent.idText));

                while (NotificationManager.instance.bubbleInstance != null)
                {
                    yield return null;
                }
            }
            else if (actualEvent.eventType == EventType.CHANGE_SCENE)
            {
                ScenesManager.instance.ChangeScene(actualEvent.idText);
            }
            else if (actualEvent.eventType == EventType.SPECIAL_METHODS)
            {
                GameObject targetObject = GameObject.Find(actualEvent.targetObjectName);
                if (targetObject == null)
                {
                    Debug.LogWarning($"Objet '{actualEvent.targetObjectName}' non trouv�.");
                    yield break;
                }

                Component targetComponent = targetObject.GetComponent(actualEvent.componentType);
                if (targetComponent == null)
                {
                    Debug.LogWarning($"Composant '{actualEvent.componentType}' non trouv� sur '{actualEvent.targetObjectName}'.");
                    yield break;
                }

                // R�cup�rer toutes les m�thodes avec le nom donn�
                MethodInfo[] methods = targetComponent.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                // V�rifier si des param�tres sont fournis
                object[] parsedParams = actualEvent.parameters
                    .Select(param =>
                    {
                        if (int.TryParse(param, out int intValue)) return (object)intValue;
                        if (float.TryParse(param, out float floatValue)) return (object)floatValue;
                        if (bool.TryParse(param, out bool boolValue)) return (object)boolValue;

                        // Cas sp�ciaux pour Color
                        if (param.StartsWith("Color."))
                        {
                            var colorProp = typeof(Color).GetProperty(param.Substring(6), BindingFlags.Static | BindingFlags.Public);
                            if (colorProp != null) return (object)colorProp.GetValue(null);
                        }

                        return param;
                    })
                    .ToArray();


                // Trouver la m�thode qui correspond exactement aux types des param�tres
                MethodInfo method = methods.FirstOrDefault(m =>
                    m.Name == actualEvent.methodName &&
                    m.GetParameters().Length == parsedParams.Length &&
                    m.GetParameters()
                     .Select(p => p.ParameterType)
                     .Zip(parsedParams.Select(p => p?.GetType() ?? typeof(object)), (expected, actual) => expected == actual || expected.IsAssignableFrom(actual))
                     .All(match => match));


                if (method == null)
                {
                    Debug.LogWarning($"M�thode '{actualEvent.methodName}' avec les param�tres sp�cifi�s non trouv�e sur {targetComponent.name}");
                    yield break;
                }

                // Ex�cution de la m�thode
                method.Invoke(targetComponent, parsedParams);
            }
            else if (actualEvent.eventType == EventType.BOOK)
            {
                container.gameObject.SetActive(true);
                bookImage.sprite = actualEvent.bookPage;

                NotificationManager.instance.ShowBubble(LocalizationManager.instance.GetTexts("pnj_text", actualEvent.idText));

                while (NotificationManager.instance.bubbleInstance != null)
                {
                    yield return null;
                }

                // V�rifie si l'�v�nement suivant est aussi un BOOK
                int currentIndex = clonedEventContainer.eventsList.IndexOf(actualEvent);
                bool isNextEventBook = currentIndex + 1 < clonedEventContainer.eventsList.Count &&
                                       clonedEventContainer.eventsList[currentIndex + 1].eventType == EventType.BOOK;

                if (!isNextEventBook)
                {
                    container.gameObject.SetActive(false);
                }
            }
            else if (actualEvent.eventType == EventType.CINEMATIC)
            {
                yield return CinematicRoutine(actualEvent);
            }
            yield return null;
        }

        ResetActions();
    }

    IEnumerator CinematicRoutine(Event actualEvent)
    {
        CinematicSpriteBehiavor cinematicSprite = NotificationManager.instance.ShowCinematicImage();

        foreach (CinematicContainer frame in actualEvent.cinematicContainer)
        {
            yield return cinematicSprite.NewImageRoutine(frame.sprite, 1f);

            SoundManager.instance.PlayUISound(frame.sound, 1);


            switch (frame.spriteEffect)
            {
                case CinematicSpriteEffect.NONE:
                    break;
                case CinematicSpriteEffect.SHAKING:
                    cinematicSprite.Shaking(frame.durationEffect);
                    break;
                case CinematicSpriteEffect.SATURATION:
                    cinematicSprite.Saturation(frame.durationEffect, frame.powerEffect);
                    break;
                case CinematicSpriteEffect.NEGATIVE:
                    cinematicSprite.Negative(frame.durationEffect);
                    break;
            }

            foreach (var textID in frame.texts)
            {
                if (textID.textID != "")
                {
                    SoundManager.instance.PlayUISound(textID.sound, 1);
                    bool bubbleFinished = false;

                    BubbleText bubble = NotificationManager.instance.ShowCinematicBubble(
                        LocalizationManager.instance.GetText("CINEMATIC", textID.textID),
                        textID.duration,
                        () => { bubbleFinished = true; }
                    );

                    // Option 1 : attendre avec WaitUntil
                    yield return new WaitUntil(() => bubbleFinished);
                }
                else
                {
                    SoundManager.instance.PlayUISound(textID.sound, 1);
                    yield return new WaitForSecondsRealtime(textID.duration);
                }

            }
        }

        NotificationManager.instance.DestroyCinematicImage();
    }




    bool AllMonstersDead(EventContainer eventContainer)
    {
        foreach (MonsterContainer monster in eventContainer.monsterContainer)
        {
            if (!monster.isBattleObject && monster.monster && monster.monster.GetComponent<Stats>() && monster.monster.GetComponent<Stats>().entityType == EntityType.Monster)
            {
                return false;
            }
        }

        foreach (MonsterContainer monster in eventContainer.monsterContainer)
        {
            if (!monster.isBattleObject && monster.monster && monster.monster.GetComponent<LifeManager>() && monster.monster.GetComponent<LifeManager>().life > 0 && monster.monster.GetComponent<Stats>() && monster.monster.GetComponent<Stats>().entityType == EntityType.Boss)
            {
                return false;
            }
        }

        return true;
    }

    void SpawnMonster(EventContainer eventContainer, List<string> id, List<Vector2> position, float colliderRadius)
    {
        for (int i = 0; i < id.Count; i++)
        {
            string monsterID = id[i];

            // Chercher d'abord un GameObject existant dans la sc�ne
            GameObject existingMonster = GameObject.Find(monsterID);

            if (existingMonster != null)
            {
                // Utiliser l'entit� existante
                if (position.Count > 0)
                    existingMonster.transform.position = (Vector2)transform.position + position[i];

                existingMonster.SetActive(true);

                // Cr�er un nouveau MonsterContainer pour cette entit� existante
                MonsterContainer newMonsterContainer = new MonsterContainer
                {
                    id = monsterID,
                    monster = existingMonster,
                    isBattleObject = false
                };

                // L'ajouter au eventContainer
                eventContainer.monsterContainer.Add(newMonsterContainer);

                // Effet visuel du spawn
                if (position.Count > 0)
                    Instantiate(cloudPrefab, (Vector2)transform.position + position[i], Quaternion.identity);

                // R�initialiser la vie si c'est un boss
                if (existingMonster.GetComponent<Stats>() && existingMonster.GetComponent<Stats>().entityType == EntityType.Boss)
                {
                    existingMonster.GetComponent<LifeManager>().life = existingMonster.GetComponent<Stats>().health;
                }

                Debug.Log($"Entit� existante trouv�e et utilis�e: {monsterID}");
            }
            else
            {
                // Fallback: chercher dans le MonsterContainer existant (comportement original)
                bool found = false;
                for (int j = 0; j < eventContainer.monsterContainer.Count; j++)
                {
                    if (eventContainer.monsterContainer[j].id == monsterID)
                    {
                        MonsterContainer monster = eventContainer.monsterContainer[j];
                        monster.monster = Instantiate(monster.monster, (Vector2)transform.position + position[i], Quaternion.identity);
                        Instantiate(cloudPrefab, (Vector2)transform.position + position[i], Quaternion.identity);
                        eventContainer.monsterContainer[j] = monster;

                        if (monster.monster.GetComponent<Stats>() && monster.monster.GetComponent<Stats>().entityType == EntityType.Boss)
                            monster.monster.GetComponent<LifeManager>().life = monster.monster.GetComponent<Stats>().health;

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogWarning($"Aucune entit� trouv�e avec l'ID: {monsterID}");
                }
            }
        }
    }

    GameObject CreateBattleZone(Vector2 center, float innerRadius, float outerRadius)
    {
        GameObject battleZone = new GameObject("BattleZone");
        battleZone.transform.position = center;

        EdgeCollider2D edgeCollider = battleZone.AddComponent<EdgeCollider2D>();

        int pointCount = 50;
        Vector2[] points = new Vector2[pointCount + 1]; // +1 pour boucler

        for (int i = 0; i <= pointCount; i++)
        {
            float angle = (float)i / pointCount * Mathf.PI * 2f;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * outerRadius;
        }

        edgeCollider.points = points;

        return battleZone;
    }





    public void MoveCamera(GameObject cam, Vector2 relativePosition, float duration)
    {
        // Utiliser la cam�ra par d�faut si aucune n'est fournie
        if (cam == null)
        {
            if (CameraManager.instance == null || CameraManager.instance.defaultVirtualCamera == null)
            {
                Debug.LogError("Aucune cam�ra fournie et aucune cam�ra par d�faut trouv�e.");
                return;
            }

            cam = CameraManager.instance.defaultVirtualCamera.gameObject;
        }

        CinemachineVirtualCamera vcam = cam.GetComponent<CinemachineVirtualCamera>();
        if (vcam == null)
        {
            Debug.LogError("Le GameObject ne contient pas de CinemachineVirtualCamera.");
            return;
        }

        Vector2 startPos = vcam.transform.position;
        Vector2 targetPos = startPos + relativePosition;

        StartCoroutine(CameraMoveOverTime(vcam, startPos, targetPos, duration));
    }


    private IEnumerator CameraMoveOverTime(CinemachineVirtualCamera vcam, Vector2 startPos, Vector2 targetPos, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (vcam == null)
                yield break;

            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smoothstep

            Vector2 newPosition = Vector2.Lerp(startPos, targetPos, t);
            vcam.transform.position = new Vector3(newPosition.x, newPosition.y, vcam.transform.position.z);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (vcam != null)
        {
            vcam.transform.position = new Vector3(targetPos.x, targetPos.y, vcam.transform.position.z);
        }
    }


    void RemoveCamera(EventContainer eventContainer)
    {
        Destroy(eventContainer.camera);
    }

    void RemovePNJ(EventContainer eventContainer, List<string> id)
    {
        foreach (string idPnj in id)
        {
            for (int i = 0; i < eventContainer.pnjContainer.Count; i++)
            {
                if (eventContainer.pnjContainer[i].id == idPnj)
                {
                    Destroy(eventContainer.pnjContainer[i].pnj);
                }
            }
        }
    }

    void AnimPNJ(EventContainer eventContainer, List<string> id, string animationName, bool lastSpriteStay)
    {
        // V�rification que les containers n�cessaires existent
        if (eventContainer == null || eventContainer.pnjContainer == null || id == null)
            return;

        foreach (string idPnj in id)
        {
            // Traitement du Player en dehors de la boucle des PNJs
            if (idPnj == "player" || idPnj == "Player")
            {
                if (PlayerManager.instance != null && PlayerManager.instance.player != null)
                {
                    PlayerAnimation playerAnim = PlayerManager.instance.player.GetComponent<PlayerAnimation>();
                    ObjectAnimation objAnim = PlayerManager.instance.player.GetComponent<ObjectAnimation>();

                    if (playerAnim != null) playerAnim.off = true;
                    if (objAnim != null) objAnim.PlayAnimation(animationName, lastSpriteStay);
                }
                continue; // Passer � l'ID suivant si c'�tait le player
            }

            // Traitement des autres PNJs
            for (int i = 0; i < eventContainer.pnjContainer.Count; i++)
            {
                if (eventContainer.pnjContainer[i].id == idPnj)
                {
                    // V�rifier que le PNJ et son gameObject existent
                    if (eventContainer.pnjContainer[i].pnj != null &&
                        eventContainer.pnjContainer[i].pnj.gameObject != null)
                    {
                        ObjectAnimation objAnim = eventContainer.pnjContainer[i].pnj.GetComponent<ObjectAnimation>();
                        if (objAnim != null)
                        {
                            objAnim.PlayAnimation(animationName, lastSpriteStay);
                        }
                    }
                }
            }
        }
    }

    void SpawnCamera(EventContainer eventContainer, Vector2 position)
    {
        Vector3 relativePosition = (Vector3)transform.position + (Vector3)position;
        relativePosition.z = -10f; // Assurez-vous que la cam�ra est bien en arri�re des objets

        eventContainer.camera = Instantiate(eventContainer.camera, relativePosition, Quaternion.identity);
    }

    void SpawnPNJ(EventContainer eventContainer, List<string> id, Vector2 position, bool alreadyOnScene)
    {
        Vector2 relativePosition = (Vector2)transform.position + position;

        foreach (string idPnj in id)
        {
            for (int i = 0; i < eventContainer.pnjContainer.Count; i++)
            {
                if (eventContainer.pnjContainer[i].id == idPnj)
                {
                    PNJContainer pnj = eventContainer.pnjContainer[i];

                    if (alreadyOnScene)
                    {
                        GameObject existingPnj = GameObject.Find(pnj.id);
                        if (existingPnj != null)
                        {
                            pnj.pnj = existingPnj;
                            eventContainer.pnjContainer[i] = pnj;
                            continue;
                        }
                    }

                    pnj.pnj = Instantiate(pnj.pnj, relativePosition, Quaternion.identity);
                    pnj.pnj.name = pnj.pnj.name.Replace("(Clone)", "").Trim(); // pour que Find fonctionne plus tard
                    eventContainer.pnjContainer[i] = pnj;
                }
            }
        }
    }


    // Modifier la signature pour retourner un IEnumerator
    public IEnumerator MovePNJ(EventContainer eventContainer, List<string> id, Vector2 position, float duration, bool absolutePosition)
    {
        Vector2 referencePosition = absolutePosition ? gameObject.transform.position : Vector2.zero;
        List<Coroutine> activeCoroutines = new List<Coroutine>();

        foreach (string idPnj in id)
        {
            for (int i = 0; i < eventContainer.pnjContainer.Count; i++)
            {
                if (eventContainer.pnjContainer[i].id == idPnj)
                {
                    PNJContainer pnj = eventContainer.pnjContainer[i];
                    if (pnj.pnj != null)
                    {
                        Vector2 currentPos = pnj.pnj.transform.position;
                        Vector2 targetPos = absolutePosition ? referencePosition + position : currentPos + position;

                        // Flip du sprite selon la direction
                        SpriteRenderer sr = pnj.pnj.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.flipX = (targetPos.x - currentPos.x) > 0;
                        }

                        // Stocker la coroutine pour l'attendre plus tard
                        Coroutine moveCoroutine = StartCoroutine(MoveOverTime(pnj.pnj, currentPos, targetPos, duration));
                        activeCoroutines.Add(moveCoroutine);
                    }
                }
            }

            if (idPnj == "player" || idPnj == "Player")
            {
                Vector2 currentPos = PlayerManager.instance.player.transform.position;
                Vector2 targetPos = absolutePosition ? referencePosition + position : currentPos + position;

                SpriteRenderer sr = PlayerManager.instance.player.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.flipX = (targetPos.x - currentPos.x) > 0;
                }

                Coroutine playerMoveCoroutine = StartCoroutine(MoveOverTime(PlayerManager.instance.player, currentPos, targetPos, duration));
                activeCoroutines.Add(playerMoveCoroutine);
            }
        }

        // Attendre que toutes les coroutines soient termin�es
        foreach (Coroutine coroutine in activeCoroutines)
        {
            yield return coroutine;
        }
    }

    private IEnumerator MoveOverTime(GameObject pnj, Vector2 startPos, Vector2 targetPos, float duration)
    {
        float elapsedTime = 0f;
        Rigidbody2D rb2d = pnj.GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            Debug.LogError("Rigidbody2D is missing on " + pnj.name);
            yield break;
        }

        bool wasKinematic = rb2d.isKinematic;
        rb2d.isKinematic = true; // D�sactiver la physique temporairement

        while (elapsedTime < duration)
        {
            if (pnj == null || rb2d == null)
            {
                yield break;
            }

            float t = elapsedTime / duration;
            Vector2 newPosition = Vector2.Lerp(startPos, targetPos, t);
            rb2d.MovePosition(newPosition);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // S'assurer que la position finale est exacte
        if (pnj != null && rb2d != null)
        {
            rb2d.MovePosition(targetPos);
            rb2d.isKinematic = wasKinematic; // Restaurer l'�tat initial
        }
    }


}