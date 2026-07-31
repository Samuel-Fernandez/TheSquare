using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BubbleText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public Image backgroundImage;
    public List<string> texts;
    public float typingSpeed = 0.01f;

    [HideInInspector] public string soundIdToPlay = "";

    [Header("Cinematic Settings")]
    public bool isCinematicBubble = false;
    public float cinematicDisplayDuration = 2.5f;

    private int currentTextIndex = 0;
    private bool isTyping = false;
    private string currentText = "";

    [Header("Important Bubble Settings")]
    public bool isImportantBubble = false;
    public float shakeIntensity = 3f;
    [Range(0f, 1f)]
    public float glitchChance = 0.15f;
    public Color bubbleColor = Color.black;

    [Header("Floating Effect")]
    public RectTransform bubbleTransformToFloat;
    public float floatSpeed = 2f;
    public float floatAmplitude = 10f;

    private Vector3 originalTextPosition;
    private Vector3 originalBubblePosition;

    public event System.Action OnBubbleTextFinished;

    private void Update()
    {
        if (isImportantBubble && bubbleTransformToFloat != null)
        {
            float newY = originalBubblePosition.y + Mathf.Sin(Time.unscaledTime * floatSpeed) * floatAmplitude;
            bubbleTransformToFloat.localPosition = new Vector3(originalBubblePosition.x, newY, originalBubblePosition.z);
        }

        if (EventPlayer.GetSkipMultiplier() > 1f)
        {
            if (!isCinematicBubble && !isTyping)
            {
                SkipText();
            }
        }
    }

    private void Awake()
    {
        InventoryManager.instance.canOpenInventory = false;
        QuestManager.instance.canOpenQuests = false;

        if (isImportantBubble && backgroundImage != null)
        {
            // On garde l'alpha d'origine du fond (transparence du panneau) et on ne remplace que la teinte
            Color originalBackground = backgroundImage.color;
            backgroundImage.color = new Color(bubbleColor.r, bubbleColor.g, bubbleColor.b, originalBackground.a);
        }

        if (bubbleTransformToFloat != null)
        {
            originalBubblePosition = bubbleTransformToFloat.localPosition;
        }

        if (!isCinematicBubble)
        {
            StartCoroutine(TypeText());
        }
    }

    public void Init()
    {
        StartCoroutine(TypeText());
    }


    public void SkipText()
    {
        if (isCinematicBubble) return; // Ne rien faire si c'est une bulle cinématique

        if (isTyping)
        {
            StopAllCoroutines();
            textMeshPro.text = currentText;
            isTyping = false;
            if (isImportantBubble)
            {
                textMeshPro.rectTransform.localPosition = originalTextPosition;
            }
        }
        else
        {
            currentTextIndex++;
            if (currentTextIndex < texts.Count)
            {
                StartCoroutine(TypeText());
            }
            else
            {
                CloseBubble();
            }
        }
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        currentText = texts[currentTextIndex];
        textMeshPro.text = "";
        originalTextPosition = textMeshPro.rectTransform.localPosition;

        Coroutine displayTimer = null;

        // Commencer le timer en parallèle si c’est une bulle cinématique
        if (isCinematicBubble)
        {
            displayTimer = StartCoroutine(CinematicTimer());
        }

        string glitchChars = "!@#$%^&*()_+-=[]{}|;':,.<>?/";

        foreach (char letter in currentText.ToCharArray())
        {
            if (isImportantBubble && Random.value < glitchChance && letter != ' ')
            {
                char c = glitchChars[Random.Range(0, glitchChars.Length)];
                textMeshPro.text += c;
                yield return new WaitForSecondsRealtime(0.02f);
                textMeshPro.text = textMeshPro.text.Substring(0, textMeshPro.text.Length - 1);
            }

            textMeshPro.text += letter;

            bool isVowel = "aeiouyAEIOUYéèêëàâäîïôöùûü".IndexOf(letter) >= 0;
            if (isVowel && !string.IsNullOrEmpty(soundIdToPlay))
            {
                SoundContainer soundContainer = GetComponent<SoundContainer>();
                if (soundContainer != null)
                {
                    soundContainer.PlayUISound(soundIdToPlay, 2);
                }
            }

            float elapsed = 0f;
            while (elapsed < typingSpeed)
            {
                if (isImportantBubble)
                {
                    textMeshPro.rectTransform.localPosition = originalTextPosition + new Vector3(Random.Range(-shakeIntensity, shakeIntensity), Random.Range(-shakeIntensity, shakeIntensity), 0);
                }

                elapsed += Time.unscaledDeltaTime * EventPlayer.GetSkipMultiplier();
                yield return null;
            }
        }

        if (isImportantBubble)
        {
            textMeshPro.rectTransform.localPosition = originalTextPosition;
        }

        isTyping = false;

        // Si ce nest pas une bulle cinmatique, on attend la fin du texte manuellement
        if (!isCinematicBubble)
        {
            // Attend quon clique pour passer
        }
    }


    private void CloseBubble()
    {
        InventoryManager.instance.canOpenInventory = true;
        QuestManager.instance.canOpenQuests = true;
        OnBubbleTextFinished?.Invoke();
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        NotificationManager.instance.OnBubbleDestroyed();
    }

    private IEnumerator CinematicTimer()
    {
        float elapsed = 0f;
        while (elapsed < cinematicDisplayDuration)
        {
            elapsed += Time.unscaledDeltaTime * EventPlayer.GetSkipMultiplier();
            yield return null;
        }

        currentTextIndex++;
        if (currentTextIndex < texts.Count)
        {
            StartCoroutine(TypeText());
        }
        else
        {
            CloseBubble();
        }
    }

}
