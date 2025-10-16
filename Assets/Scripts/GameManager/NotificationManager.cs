using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager instance;

    public Canvas canvas;
    public GameObject popup;
    public GameObject bubbleCanvas;
    public GameObject cinematicBubbleCanvas;

    public GameObject specialArrow;
    public GameObject specialSquareCoins;

    GameObject popupInstanceSpecial;

    public GameObject titleNotification;

    // Utilisé pour savoir si une instance existe actuellement
    public GameObject bubbleInstance;

    public GameObject littleBubblePrefab;

    public GameObject itemResumePrefab;

    public GameObject cinematicImagePrefab;
    GameObject cinematicImageInstance;

    public GameObject bossFightLife;

    public GameObject transitionPanel;

    private System.Action onBubbleComplete;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void FadeTransitionPanel(float duration)
    {
        StartCoroutine(FadeTransitionPanelRoutine(duration));
    }

    IEnumerator FadeTransitionPanelRoutine(float duration)
    {
        yield return UIAnimator.instance.ActivateObjectCoroutine(transitionPanel, duration / 4);
        yield return new WaitForSecondsRealtime(duration / 2);
        UIAnimator.instance.DeactivateObjectWithTransition(transitionPanel, duration / 4);
    }

    public GameObject ShowBossBar(string bossID, int bossMaxLife)
    {
        GameObject bossBarInstance = Instantiate(bossFightLife);
        bossBarInstance.GetComponent<BossBarUI>().InitBossBar(bossID, bossMaxLife);
        return bossBarInstance;
    }

    // IMPORTANT, IL FAUDRA DETRUIRE L'OBJET UNE FOIS PLUS UTILE
    public CinematicSpriteBehiavor ShowCinematicImage()
    {
        cinematicImageInstance = Instantiate(cinematicImagePrefab);

        return cinematicImageInstance.GetComponent<CinematicSpriteBehiavor>();
    }

    public void DestroyCinematicImage()
    {
        Destroy(cinematicImageInstance);
        cinematicImageInstance = null;
    }


    public void ShowItemResume(Item item)
    {
        GameObject itemResume = Instantiate(itemResumePrefab);
        UIAnimator.instance.ActivateObjectWithTransition(itemResume, .5f);
        itemResume.GetComponent<ItemResume>().Initialize(item);
    }

    public void ShowItemResume(string id)
    {
        GameObject itemResume = Instantiate(itemResumePrefab);
        UIAnimator.instance.ActivateObjectWithTransition(itemResume, .5f);
        itemResume.GetComponent<ItemResume>().resumeType = ItemResumeType.KEY_ITEM;
        itemResume.GetComponent<ItemResume>().Initialize(id);
    }

    public void ShowItemResume(ToolType tool = ToolType.NONE)
    {
        GameObject itemResume = Instantiate(itemResumePrefab);
        UIAnimator.instance.ActivateObjectWithTransition(itemResume, .5f);
        itemResume.GetComponent<ItemResume>().resumeType = ItemResumeType.SPECIAL_OBJECT;
        itemResume.GetComponent<ItemResume>().Initialize(tool);
    }

    public void ShowTitle(string title, string subTitle)
    {
        if(title == null || subTitle == null)
        {
            Debug.Log("No Title or subtitle");
            return;
        }
        titleNotification.GetComponentsInChildren<TextMeshProUGUI>()[0].text = title;
        titleNotification.GetComponentsInChildren<TextMeshProUGUI>()[1].text = subTitle;

        GameObject titleNotifTemp = Instantiate(titleNotification, canvas.transform);

        Destroy(titleNotifTemp, 3);
    }

    public void ShowSpecialPopUpArrow(string baseValue, string finalValue)
    {
        if(popupInstanceSpecial != null)
            Destroy(popupInstanceSpecial);

        popupInstanceSpecial = Instantiate(specialArrow, canvas.transform);
        TextMeshProUGUI textComponent = popupInstanceSpecial.GetComponentInChildren<TextMeshProUGUI>();

        int startValue = int.Parse(baseValue);
        int endValue = int.Parse(finalValue);

        StartCoroutine(AnimateValue(textComponent, startValue, endValue, 1f));

        Destroy(popupInstanceSpecial, 2f);
    }

    public void ShowSpecialPopUpSquareCoins(string baseValue, string finalValue)
    {
        if (popupInstanceSpecial != null)
            Destroy(popupInstanceSpecial);

        popupInstanceSpecial = Instantiate(specialSquareCoins, canvas.transform);
        TextMeshProUGUI textComponent = popupInstanceSpecial.GetComponentInChildren<TextMeshProUGUI>();

        int startValue = int.Parse(baseValue);
        int endValue = int.Parse(finalValue);

        StartCoroutine(AnimateValue(textComponent, startValue, endValue, 1f));

        Destroy(popupInstanceSpecial, 2f);
    }


    private IEnumerator AnimateValue(TextMeshProUGUI textComponent, int startValue, int endValue, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            int currentValue = Mathf.FloorToInt(Mathf.Lerp(startValue, endValue, elapsedTime / duration));
            textComponent.text = currentValue.ToString();
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textComponent.text = endValue.ToString(); // Assure que la valeur finale est bien affichée
    }


    public void ShowPopup(string text)
    {
        // Instancier le popup dans le canvas
        GameObject popupInstance = Instantiate(popup, canvas.transform);

        // Trouver le TextMeshPro enfant et mettre à jour le texte
        TextMeshProUGUI textComponent = popupInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI component not found in popup prefab.");
        }

        // Détruire le popup après 2 secondes
        Destroy(popupInstance, 2f);
    }

    private BubbleText activeBubble;

    public BubbleText ShowCinematicBubble(string text, float duration = 2.5f, System.Action callback = null)
    {
        // Empêche une nouvelle bulle si une est déjà active
        if (activeBubble != null)
        {
            Debug.LogWarning("Une bulle est déjà active !");
            return null;
        }

        GameObject cinematicInstance = Instantiate(cinematicBubbleCanvas);
        BubbleText bubbleText = cinematicInstance.GetComponent<BubbleText>();

        activeBubble = bubbleText;

        bubbleText.texts = new List<string> { text };
        bubbleText.isCinematicBubble = true;
        bubbleText.cinematicDisplayDuration = duration;
        bubbleText.Init();

        bubbleText.OnBubbleTextFinished += () =>
        {
            activeBubble = null;
            callback?.Invoke();
        };

        return bubbleText;
    }






    public void ShowBubble(List<string> texts, System.Action callback = null)
    {
        onBubbleComplete = callback;
        bubbleCanvas.GetComponent<BubbleText>().texts = texts;
        bubbleInstance = Instantiate(bubbleCanvas);
    }

    public void OnBubbleDestroyed()
    {
        if (onBubbleComplete != null)
        {
            onBubbleComplete.Invoke();
            onBubbleComplete = null;
        }
    }





    public void ShowLittleBubble(GameObject gameObject, string text, float duration, float upPosition = 0)
    {
        // Instancier le prefab et le faire devenir un enfant de gameObject
        GameObject littleBubble = Instantiate(littleBubblePrefab, gameObject.transform.position + gameObject.transform.position + new Vector3(0, 1 + upPosition, 0), Quaternion.identity);

        littleBubble.GetComponent<LittleBubbleText>().objectToFollow = gameObject;
        littleBubble.GetComponent<LittleBubbleText>().upOffset = upPosition;
        littleBubble.GetComponent<LittleBubbleText>().textToWrite = text;
        littleBubble.GetComponent<LittleBubbleText>().duration = duration;
    }

   




}
