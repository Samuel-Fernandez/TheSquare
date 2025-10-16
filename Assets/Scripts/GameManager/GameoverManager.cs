using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameoverManager : MonoBehaviour
{
    public static GameoverManager instance;
    public GameObject gameOverUI;
    public GameObject transitionUI;
    public GameObject vignetteUI;
    public GameObject soulPrefab;

    public bool isSoul;
    public string deathScene;
    public Vector2 deathPosition;
    public int money;
    public float elapsedTime;
    private GameObject soulInstantiation;
    private Coroutine soulMoneyReductionRoutine;

    public TextMeshProUGUI continueText;
    public TextMeshProUGUI leaveText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        continueText.text = LocalizationManager.instance.GetText("UI", "GAME_OVER_CONTINUE");
        leaveText.text = LocalizationManager.instance.GetText("UI", "MAIN_MENU_LEAVE");
    }

    private void Update()
    {
        if (isSoul)
        {
            if (SceneManager.GetActiveScene().name == deathScene && !soulInstantiation)
            {
                soulInstantiation = Instantiate(soulPrefab, deathPosition, Quaternion.identity);
                soulInstantiation.GetComponent<SoulBehiavor>().money = money;

                if (soulMoneyReductionRoutine == null)
                {
                    soulMoneyReductionRoutine = StartCoroutine(SoulMoneyReductionRoutine());
                }
            }
            else if (SceneManager.GetActiveScene().name != deathScene)
            {
                if (soulInstantiation)
                {
                    Destroy(soulInstantiation);
                }
                soulInstantiation = null;
            }
        }
        else
        {
            if (soulMoneyReductionRoutine != null)
            {
                StopCoroutine(soulMoneyReductionRoutine);
                soulMoneyReductionRoutine = null;
            }

            if (soulInstantiation)
            {
                Destroy(soulInstantiation);
            }
        }
    }

    public void ActiveGameOver()
    {
        UIAnimator.instance.ActivateObjectWithTransition(gameOverUI, 1);
        deathScene = SceneManager.GetActiveScene().name;
        money = PlayerManager.instance.player.GetComponent<Stats>().money;
        isSoul = true;
        elapsedTime = 0; // Reset elapsedTime when game over is activated
        Time.timeScale = 0f;

        if (soulMoneyReductionRoutine == null)
        {
            soulMoneyReductionRoutine = StartCoroutine(SoulMoneyReductionRoutine());
        }
    }

    public void CloseGameOver()
    {
        StartCoroutine(CloseGameOverRoutine());
    }

    IEnumerator CloseGameOverRoutine()
    {
        SaveManager.instance.Load(true, true, false);
        yield return new WaitForSecondsRealtime(1);
        UIAnimator.instance.DeactivateObjectWithTransition(gameOverUI, 1);
        PlayerManager.instance.player.GetComponent<LifeManager>().FullHealth();
        PlayerManager.instance.player.GetComponent<Stats>().money = 0;
        yield return new WaitForSecondsRealtime(1);
        PlayerManager.instance.TogglePlayer(0);

        SaveManager.instance.Save();
        Time.timeScale = 1f;
    }

    private IEnumerator SoulMoneyReductionRoutine()
    {
        float totalTime = 10 * 60f; // 10 minutes en secondes
        int moneyPunition = (money / (int)totalTime) + 1;

        while (elapsedTime < totalTime && money > 0 && isSoul)
        {
            yield return new WaitForSeconds(1f);

            elapsedTime += 1f;

            if (Random.Range(0, 600) < elapsedTime)
            {
                GetComponent<SoundContainer>().PlaySound("pression", 1);
            }

            money -= moneyPunition;

            if (money <= 0)
            {
                money = 0;
                if (soulInstantiation)
                {
                    Destroy(soulInstantiation);
                }
                break;
            }

            if (soulInstantiation)
            {
                soulInstantiation.GetComponent<SoulBehiavor>().money = money;
                // Calculer la couleur en fonction du temps restant
                float timeFraction = elapsedTime / totalTime;
                Color newColor = Color.Lerp(Color.green, Color.red, timeFraction);
                SpriteRenderer spriteRenderer = soulInstantiation.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer)
                {
                    spriteRenderer.color = newColor;
                }
            }

        }

        if (isSoul)
        {
            if (money <= 0 && soulInstantiation)
            {
                Destroy(soulInstantiation);
                isSoul = false;

                NotificationManager.instance.ShowPopup("Your square coins have been consumed.");
                GetComponent<SoundContainer>().PlaySound("endSquareCoins", 0);
            }
        }

        soulMoneyReductionRoutine = null;
    }

    public void RestartSoulMoneyReductionRoutine()
    {
        if (isSoul)
        {
            if (soulMoneyReductionRoutine != null)
            {
                StopCoroutine(soulMoneyReductionRoutine); // Arrête la coroutine en cours
            }
            soulMoneyReductionRoutine = StartCoroutine(SoulMoneyReductionRoutine()); // Démarre une nouvelle coroutine
        }
    }
}
