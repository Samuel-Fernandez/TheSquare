using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager instance;
    public GameObject transitionPanel;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ShowSceneTitle();
    }

    public void ChangeSceneObject(string sceneName, Vector2 newPosition, float transitionDuration = .5f)
    {
        PlayerManager.instance.player.GetComponent<Stats>().canMove = false;
        StartCoroutine(RoutineChangeSceneObject(transitionDuration, newPosition, sceneName));
    }

    IEnumerator RoutineChangeSceneObject(float transitionDuration, Vector2 newPosition, string sceneName)
    {
        canTeleportPlayer = false;
        // Lancer le changement de sc�ne
        StartCoroutine(ChangeSceneWithDelay(sceneName, transitionDuration));

        // Attendre que la sc�ne soit compl�tement charg�e
        while (!isSceneLoaded)
        {
            yield return null;
        }

        // Maintenant la sc�ne est charg�e, on peut t�l�porter le joueur
        PlayerManager.instance.player.transform.position = newPosition;
        PlayerManager.instance.player.GetComponent<Stats>().canMove = true;

        canTeleportPlayer = true;

    }


    public void ChangeScene(string sceneName, float transitionDuration = .5f)
    {
        StartCoroutine(ChangeSceneWithDelay(sceneName, transitionDuration));
    }

    public bool isSceneLoaded = false;
    public bool canTeleportPlayer = true;
    private IEnumerator ChangeSceneWithDelay(string sceneName, float transitionDuration)
    {
        isSceneLoaded = false;
        UIAnimator.instance.ActivateObjectWithTransition(transitionPanel, transitionDuration);
        yield return new WaitForSecondsRealtime(transitionDuration);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(transitionDuration);

        SoundManager.instance.PlayMusic(sceneName);
        MeteoManager.instance.UpdateActualScene(SceneManager.GetActiveScene());
        ShowSceneTitle();
        FoundLocation();

        // Signaler que c'est charg� AVANT de fermer la transition
        isSceneLoaded = true;

        // Petit d�lai suppl�mentaire pour �tre s�r que tout est en place
        yield return new WaitForSecondsRealtime(0.5f);

        UIAnimator.instance.DeactivateObjectWithTransition(transitionPanel, transitionDuration);
    }

    void ShowSceneTitle()
    {
        foreach (var region in MeteoManager.instance.regions)
        {
            foreach (var scene in region.scenes)
            {
                if (scene.SceneName == SceneManager.GetActiveScene().name)
                {
                    NotificationManager.instance.ShowTitle(LocalizationManager.instance.GetText("LOCATION", region.regionID + "_REGION"), LocalizationManager.instance.GetText("LOCATION", scene.sceneID + "_SCENE"));
                }
            }
        }

    }

    void FoundLocation()
    {
        foreach (var region in MeteoManager.instance.regions)
        {
            foreach (var scene in region.scenes)
            {
                if (scene.SceneName == SceneManager.GetActiveScene().name)
                {
                    StatsManager.instance.LocationFound(scene.sceneID);
                }
            }
        }
    }
}
