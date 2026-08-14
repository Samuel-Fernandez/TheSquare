using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Variante de BossAttackTextUI utilisable depuis les evenements : au lieu de piocher au hasard
// parmi des variantes "{bossId}_ATTACK_TEXT-n", on choisit directement la cle du texte a afficher
// (categorie "BOSS" de LocalizationManager, meme convention "|" pour separer les lignes) ainsi que
// le temps d'affichage (en secondes) de chaque ligne.
public class EventTextUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;
    public GameObject rootToToggle;
    public AudioSource audioSource;

    [Header("Localisation")]
    public string category = "BOSS";

    [Header("Timing par ligne")]
    public float lineFadeInDuration = 0.5f;
    public float lineFadeOutDuration = 0.4f;
    public float delayBetweenLines = 0.15f;

    [Header("Entree de ligne (impact)")]
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float startScale = 1.4f;
    public float startVerticalOffset = 30f;

    [Header("Machine a ecrire")]
    public bool useTypewriterReveal = true;
    public float charactersPerSecond = 35f;

    [Header("Pulsation de couleur")]
    public bool usePulsingColor = true;
    public Color baseColor = Color.white;
    public Color pulseColor = new Color(1f, 0.85f, 0.4f);
    public float pulseSpeed = 2f;

    [Header("Camera - impact par ligne")]
    public bool shakeCameraPerLine = true;
    public float lineShakeAmplitude = 1.5f;
    public float lineShakeFrequency = 2f;
    public float lineShakeDuration = 0.2f;

    [Header("Camera - aura pendant toute la sequence")]
    public bool playSequenceCameraEffect = true;
    public float sequenceChromaticAberrationIntensity = 0.6f;
    public float sequenceVignetteIntensity = 0.4f;
    public float sequenceEffectTransitionTime = 0.3f;
    public float introShakeAmplitude = 3f;
    public float introShakeFrequency = 3f;
    public float introShakeDuration = 0.4f;
    public float outroShakeAmplitude = 4f;
    public float outroShakeFrequency = 3f;
    public float outroShakeDuration = 0.5f;

    [Header("Son")]
    public AudioClip introSound;
    public AudioClip lineAppearSound;

    [Header("Fin d'affichage")]
    public bool destroyRootAfterPlaying = true;
    public float postDisplayDelay = 0.5f;

    void Awake()
    {
        if (text == null) text = GetComponentInChildren<TextMeshProUGUI>();
        if (canvasGroup == null) canvasGroup = text.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = text.gameObject.AddComponent<CanvasGroup>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (rootToToggle == null) rootToToggle = gameObject;
    }

    // Point d'entree principal : a appeler depuis un evenement (ex: eventTextUI.Play("MyliaMeeting-1", 3f)).
    public void Play(string textId, float displayDuration)
    {
        StartCoroutine(PlayRoutine(textId, displayDuration));
    }

    // Variante attendue par un evenement qui veut attendre la fin de l'affichage avant d'enchainer :
    // yield return StartCoroutine(eventTextUI.PlayRoutine(textId, displayDuration));
    public IEnumerator PlayRoutine(string textId, float displayDuration)
    {
        List<string> lines = LocalizationManager.instance.GetTexts(category, textId);
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning($"EventTextUI : aucune cle {textId} trouvee dans la categorie {category}.");
            yield break;
        }

        rootToToggle.SetActive(true);

        if (playSequenceCameraEffect)
        {
            TriggerCameraShake(introShakeAmplitude, introShakeFrequency, introShakeDuration);
            SetCameraAura(sequenceChromaticAberrationIntensity, sequenceVignetteIntensity);
        }

        if (introSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(introSound);
        }

        for (int i = 0; i < lines.Count; i++)
        {
            yield return StartCoroutine(PlayLine(lines[i].Trim(), displayDuration));

            if (i < lines.Count - 1 && delayBetweenLines > 0f)
            {
                yield return new WaitForSeconds(delayBetweenLines);
            }
        }

        if (playSequenceCameraEffect)
        {
            TriggerCameraShake(outroShakeAmplitude, outroShakeFrequency, outroShakeDuration);
            SetCameraAura(0f, 0f);
        }

        yield return new WaitForSeconds(postDisplayDelay);

        if (destroyRootAfterPlaying)
        {
            Destroy(rootToToggle);
        }
        else
        {
            rootToToggle.SetActive(false);
        }
    }

    IEnumerator PlayLine(string line, float displayDuration)
    {
        RectTransform rect = text.rectTransform;
        Vector2 restPosition = rect.anchoredPosition;
        Vector2 startPosition = restPosition + Vector2.up * startVerticalOffset;

        text.text = line;
        text.color = baseColor;
        canvasGroup.alpha = 0f;
        rect.anchoredPosition = startPosition;
        rect.localScale = Vector3.one * startScale;
        text.maxVisibleCharacters = useTypewriterReveal ? 0 : line.Length;

        if (lineAppearSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lineAppearSound);
        }

        if (shakeCameraPerLine)
        {
            TriggerCameraShake(lineShakeAmplitude, lineShakeFrequency, lineShakeDuration);
        }

        // Entree : fondu + pop d'echelle + glissement vertical, tous pilotes par la meme courbe.
        float elapsed = 0f;
        while (elapsed < lineFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeInCurve.Evaluate(Mathf.Clamp01(elapsed / lineFadeInDuration));

            canvasGroup.alpha = t;
            rect.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, t);
            rect.anchoredPosition = Vector2.Lerp(startPosition, restPosition, t);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rect.localScale = Vector3.one;
        rect.anchoredPosition = restPosition;

        if (useTypewriterReveal)
        {
            yield return StartCoroutine(TypewriterReveal(line));
        }

        // Lecture : maintien a l'ecran pendant displayDuration (fixe, choisi par l'appelant), avec pulsation.
        float holdElapsed = 0f;
        while (holdElapsed < displayDuration)
        {
            holdElapsed += Time.deltaTime;

            if (usePulsingColor)
            {
                float pulse = (Mathf.Sin(holdElapsed * pulseSpeed) + 1f) * 0.5f;
                text.color = Color.Lerp(baseColor, pulseColor, pulse);
            }

            yield return null;
        }

        float fadeElapsed = 0f;
        while (fadeElapsed < lineFadeOutDuration)
        {
            fadeElapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(fadeElapsed / lineFadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    IEnumerator TypewriterReveal(string line)
    {
        float delayPerChar = 1f / Mathf.Max(charactersPerSecond, 1f);

        for (int i = 0; i <= line.Length; i++)
        {
            text.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delayPerChar);
        }
    }

    void TriggerCameraShake(float amplitude, float frequency, float duration)
    {
        if (CameraManager.instance == null) return;
        CameraManager.instance.ShakeCamera(amplitude, frequency, duration);
    }

    void SetCameraAura(float chromaticAberrationIntensity, float vignetteIntensity)
    {
        if (CameraManager.instance == null) return;
        CameraManager.instance.SetChromaticAberrationEffect(chromaticAberrationIntensity, sequenceEffectTransitionTime);
        CameraManager.instance.SetVignetteEffect(vignetteIntensity, 0.5f, sequenceEffectTransitionTime);
    }
}
