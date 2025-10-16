using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainTitleManager : MonoBehaviour
{
    public static MainTitleManager instance;

    public GameObject ui;
    public GameObject whiteTransition;
    public Image earthBackground;
    public Image asteroid;
    public GameObject settings;

    public AudioClip highSpeed;
    public AudioClip distantExplosion;

    public string begginingScene;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            StartCoroutine(AnimationRoutine());
        }
    }

    IEnumerator AnimationRoutine()
    {
        ToggleUI();

        yield return new WaitForSecondsRealtime(2);

        yield return StartCoroutine(AnimateAsteroid());

        asteroid.gameObject.SetActive(false);
        SoundManager.instance.PlaySound(distantExplosion, 1);


        yield return StartCoroutine(UIAnimator.instance.ActivateObjectCoroutine(whiteTransition, 1f));

        ToggleUI();
        SoundManager.instance.PlayMusic("MainMenu");
        UIAnimator.instance.DeactivateObjectWithTransition(whiteTransition, 1f);
        earthBackground.gameObject.GetComponent<Animator>().enabled = true;
        StartCoroutine(FlickerColor(earthBackground));
    }

    IEnumerator AnimateAsteroid()
    {
        // Étape 1 : départ
        asteroid.rectTransform.anchoredPosition = new Vector2(1217, 683);
        asteroid.rectTransform.localScale = Vector3.one * 2f;

        yield return null; // attendre au moins un frame pour appliquer la position initiale

        SoundManager.instance.PlaySound(highSpeed, 1);

        // Étape 2 : aller vers position (605, 266), scale 1 en 1 seconde
        float duration = 1f;
        float timer = 0f;
        Vector2 startPos = asteroid.rectTransform.anchoredPosition;
        Vector2 targetPos = new Vector2(605, 266);
        Vector3 startScale = asteroid.rectTransform.localScale;
        Vector3 targetScale = Vector3.one * 1f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            asteroid.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            asteroid.rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Étape 3 : aller vers position (113, -106), scale 0.1 en 1 seconde
        timer = 0f;
        startPos = asteroid.rectTransform.anchoredPosition;
        targetPos = new Vector2(113, -106);
        startScale = asteroid.rectTransform.localScale;
        targetScale = Vector3.one * 0.1f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            asteroid.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            asteroid.rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
    }


    public IEnumerator FlickerColor(Image image)
    {
        Color color1 = Color.white;
        Color color2 = new Color(0.588f, 0.165f, 0.165f); // #962A2A en RGB (float 0-1)

        float duration = 2f; // durée d'une transition (blanc -> 962A2A ou inverse)
        float timer = 0f;

        while (true)
        {
            // Transition de color1 à color2
            timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                image.color = Color.Lerp(color1, color2, timer / duration);
                yield return null;
            }

            // Transition de color2 à color1
            timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                image.color = Color.Lerp(color2, color1, timer / duration);
                yield return null;
            }
        }
    }

    void ToggleUI()
    {
        ui.SetActive(!ui.activeSelf);
    }

    public void StartGame()
    {
        if (SaveManager.instance.CheckIfSave())
        {
            SaveManager.instance.Load();
            PlayerManager.instance.TogglePlayer(2);
        }
        else
        {
            ScenesManager.instance.ChangeScene(begginingScene);
            StopAllCoroutines();
        }

    }

    public void LeaveGame()
    {
        Debug.Log("Leave");

    }

    public void ToggleSettings()
    {
        settings.SetActive(!settings.activeSelf);

    }
}
