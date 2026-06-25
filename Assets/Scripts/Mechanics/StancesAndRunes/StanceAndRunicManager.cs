using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[System.Serializable]
public class StanceData
{
    [Tooltip("La référence vers le ScriptableObject de la posture")]
    public StanceSO stance;
    [Tooltip("Est-ce que cette posture a été débloquée par le joueur ?")]
    public bool isUnlocked;
    [Tooltip("Est-ce que cette posture est actuellement équipée/active ?")]
    public bool isEquipped;
}

[System.Serializable]
public class RuneData
{
    [Tooltip("La référence vers le ScriptableObject de la rune")]
    public RuneSO rune;
    [Tooltip("Est-ce que cette rune a été débloquée par le joueur ?")]
    public bool isUnlocked;
    [Tooltip("Est-ce que cette rune est actuellement équipée/active ?")]
    public bool isEquipped;
}

public class StanceAndRunicManager : MonoBehaviour
{
    [Header("UI References - Stances (Top-Left)")]
    [Tooltip("L'image de fond pour la posture (ex: le cadre)")]
    public Image stanceBackgroundImage;
    [Tooltip("L'image qui affichera l'icône de la posture")]
    public Image stanceIconImage;

    [Header("UI References - Runes (Top-Right)")]
    [Tooltip("L'image de fond pour la rune (ex: le cadre)")]
    public Image runeBackgroundImage;
    [Tooltip("L'image qui affichera l'icône de la rune")]
    public Image runeIconImage;

    [Header("Data")]
    [Tooltip("Liste de toutes les postures gérées par ce système")]
    public List<StanceData> stancesList = new List<StanceData>();
    [Tooltip("Liste de toutes les runes gérées par ce système")]
    public List<RuneData> runesList = new List<RuneData>();

    [Header("Animation Settings")]
    [Tooltip("Durée de l'animation de changement en secondes")]
    public float animationDuration = 0.3f;
    [Tooltip("Taille maximum atteinte pendant l'effet Pop")]
    public float popScaleMultiplier = 1.3f;
    [Tooltip("Couleur du flash sur le fond (impulsion lumineuse)")]
    public Color flashColor = Color.white;

    private PlayerInputActions inputActions;
    private Coroutine stanceAnimCoroutine;
    private Coroutine runeAnimCoroutine;
    private Color originalStanceBgColor;
    private Color originalRuneBgColor;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        // S'abonner aux événements d'input
        inputActions.Gameplay.ChangeStance.performed += ctx => CycleNextStance();
        inputActions.Gameplay.ChangeRune.performed += ctx => CycleNextRune();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        // Sauvegarde des couleurs d'origine
        if (stanceBackgroundImage != null) originalStanceBgColor = stanceBackgroundImage.color;
        if (runeBackgroundImage != null) originalRuneBgColor = runeBackgroundImage.color;

        // Initialisation de l'UI avec les éléments équipés par défaut (s'il y en a)
        UpdateStanceUI();
        UpdateRuneUI();
    }

    /// <summary>
    /// Met à jour l'image de la posture équipée.
    /// </summary>
    public void UpdateStanceUI()
    {
        StanceData equippedStance = GetEquippedStance();
        if (equippedStance != null && equippedStance.stance != null && stanceIconImage != null)
        {
            stanceIconImage.sprite = equippedStance.stance.iconSprite;
            stanceIconImage.enabled = true;
        }
        else if (stanceIconImage != null)
        {
            stanceIconImage.enabled = false;
        }
    }

    /// <summary>
    /// Met à jour l'image de la rune équipée.
    /// </summary>
    public void UpdateRuneUI()
    {
        RuneData equippedRune = GetEquippedRune();
        if (equippedRune != null && equippedRune.rune != null && runeIconImage != null)
        {
            runeIconImage.sprite = equippedRune.rune.iconSprite;
            runeIconImage.enabled = true;
        }
        else if (runeIconImage != null)
        {
            runeIconImage.enabled = false;
        }
    }

    // --- Méthodes Utilitaires ---

    /// <summary>
    /// Récupère la posture actuellement équipée.
    /// </summary>
    public StanceData GetEquippedStance()
    {
        return stancesList.Find(s => s.isEquipped);
    }

    /// <summary>
    /// Récupère la rune actuellement équipée.
    /// </summary>
    public RuneData GetEquippedRune()
    {
        return runesList.Find(r => r.isEquipped);
    }

    // --- Méthodes de Cyclage ---

    /// <summary>
    /// Passe à la posture débloquée suivante.
    /// </summary>
    public void CycleNextStance()
    {
        if (stancesList.Count == 0) return;

        int currentIndex = stancesList.FindIndex(s => s.isEquipped);

        if (currentIndex != -1)
            stancesList[currentIndex].isEquipped = false;

        int nextIndex = currentIndex == -1 ? 0 : currentIndex;
        int loopCount = 0;

        do
        {
            nextIndex = (nextIndex + 1) % stancesList.Count;
            loopCount++;

            // Sécurité anti-boucle infinie (si aucune n'est débloquée)
            if (loopCount > stancesList.Count)
                break;

        } while (!stancesList[nextIndex].isUnlocked);

        stancesList[nextIndex].isEquipped = true;
        UpdateStanceUI();

        // Jouer le son de changement via le SoundContainer
        SoundContainer soundContainer = GetComponent<SoundContainer>();
        if (soundContainer != null)
        {
            soundContainer.PlayUISound("StanceChangement", 1);
        }

        // Relancer l'animation
        if (stanceAnimCoroutine != null) StopCoroutine(stanceAnimCoroutine);
        stanceAnimCoroutine = StartCoroutine(AnimateUI(stanceIconImage, stanceBackgroundImage, originalStanceBgColor));
    }

    /// <summary>
    /// Passe à la rune débloquée suivante.
    /// </summary>
    public void CycleNextRune()
    {
        if (runesList.Count == 0) return;

        int currentIndex = runesList.FindIndex(r => r.isEquipped);

        if (currentIndex != -1)
            runesList[currentIndex].isEquipped = false;

        int nextIndex = currentIndex == -1 ? 0 : currentIndex;
        int loopCount = 0;

        do
        {
            nextIndex = (nextIndex + 1) % runesList.Count;
            loopCount++;

            // Sécurité anti-boucle infinie (si aucune n'est débloquée)
            if (loopCount > runesList.Count)
                break;

        } while (!runesList[nextIndex].isUnlocked);

        runesList[nextIndex].isEquipped = true;
        UpdateRuneUI();

        // Jouer le son de changement via le SoundContainer
        SoundContainer soundContainer = GetComponent<SoundContainer>();
        if (soundContainer != null)
        {
            soundContainer.PlayUISound("RuneChangement", 1);
        }

        // Relancer l'animation
        if (runeAnimCoroutine != null) StopCoroutine(runeAnimCoroutine);
        runeAnimCoroutine = StartCoroutine(AnimateUI(runeIconImage, runeBackgroundImage, originalRuneBgColor));
    }

    // --- Animation ---

    private IEnumerator AnimateUI(Image iconImage, Image bgImage, Color originalBgColor)
    {
        if (iconImage == null || bgImage == null) yield break;

        float time = 0;
        Vector3 originalScale = Vector3.one;

        while (time < animationDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / animationDuration;

            // Effet "Pop" : La courbe Mathf.Sin(t * PI) crée une cloche parfaite (0 -> 1 -> 0)
            float scaleBump = Mathf.Sin(t * Mathf.PI);
            float currentScale = 1f + (scaleBump * (popScaleMultiplier - 1f));
            iconImage.transform.localScale = originalScale * currentScale;

            // Flash d'impulsion : On fade de la flashColor vers la couleur d'origine
            bgImage.color = Color.Lerp(flashColor, originalBgColor, t);

            yield return null;
        }

        // S'assurer de remettre l'état final exact
        iconImage.transform.localScale = originalScale;
        bgImage.color = originalBgColor;
    }
}
