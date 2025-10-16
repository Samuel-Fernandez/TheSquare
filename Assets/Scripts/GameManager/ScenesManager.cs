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

    public void ChangeSceneObject(string sceneName, Vector2 newPosition, float transitionDuration=.5f)
    {
        PlayerManager.instance.player.GetComponent<Stats>().canMove = false;
        ChangeScene(sceneName, transitionDuration);
        StartCoroutine(RoutineChangeSceneObject(transitionDuration, newPosition));
    }

    IEnumerator RoutineChangeSceneObject(float transitionDuration, Vector2 newPosition)
    {
        yield return new WaitForSeconds(transitionDuration);
        PlayerManager.instance.player.transform.position = newPosition;
        PlayerManager.instance.player.GetComponent<Stats>().canMove = true;
    }

    public void ChangeScene(string sceneName, float transitionDuration=.5f)
    {
        StartCoroutine(ChangeSceneWithDelay(sceneName, transitionDuration));
    }

    private IEnumerator ChangeSceneWithDelay(string sceneName, float transitionDuration)
    {
        UIAnimator.instance.ActivateObjectWithTransition(transitionPanel, transitionDuration);

        // Attendre le temps de la transition
        yield return new WaitForSeconds(transitionDuration);

        SceneManager.LoadScene(sceneName);

        yield return new WaitForSeconds(transitionDuration);


        SoundManager.instance.PlayMusic(sceneName);
        MeteoManager.instance.UpdateActualScene(SceneManager.GetActiveScene());
        ShowSceneTitle();
        FoundLocation();

        UIAnimator.instance.DeactivateObjectWithTransition(transitionPanel, transitionDuration);
    }

    void ShowSceneTitle()
    {
        foreach (var region in MeteoManager.instance.regions)
        {
            foreach (var scene in region.scenes)
            {
                if(scene.SceneName == SceneManager.GetActiveScene().name)
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
