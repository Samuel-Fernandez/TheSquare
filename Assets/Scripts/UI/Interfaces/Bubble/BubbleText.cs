using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BubbleText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public List<string> texts;
    public float typingSpeed = 0.01f;

    [Header("Cinematic Settings")]
    public bool isCinematicBubble = false;
    public float cinematicDisplayDuration = 2.5f;

    private int currentTextIndex = 0;
    private bool isTyping = false;
    private string currentText = "";

    public event System.Action OnBubbleTextFinished;

    private void Awake()
    {
        Time.timeScale = 0;
        InventoryManager.instance.canOpenInventory = false;
        QuestManager.instance.canOpenQuests = false;

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
        if (isCinematicBubble) return; // Ne rien faire si c'est une bulle cin�matique

        if (isTyping)
        {
            StopAllCoroutines();
            textMeshPro.text = currentText;
            isTyping = false;
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

        Coroutine displayTimer = null;

        // Commencer le timer en parall�le si c�est une bulle cin�matique
        if (isCinematicBubble)
        {
            displayTimer = StartCoroutine(CinematicTimer());
        }

        foreach (char letter in currentText.ToCharArray())
        {
            textMeshPro.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;

        // Si ce n�est pas une bulle cin�matique, on attend la fin du texte manuellement
        if (!isCinematicBubble)
        {
            // Attend qu�on clique pour passer
        }
    }


    private void CloseBubble()
    {
        Time.timeScale = 1;
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
        yield return new WaitForSecondsRealtime(cinematicDisplayDuration);

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
