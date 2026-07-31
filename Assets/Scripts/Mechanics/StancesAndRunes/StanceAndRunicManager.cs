using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TheSquare.Mechanics.UniverseHeart;

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
    [Tooltip("Est-ce que cette rune est équipée dans le deck de 3 ?")]
    public bool isEquipped;
    [Tooltip("Est-ce que cette rune est active en combat ?")]
    public bool isActive;
}

public class StanceAndRunicManager : MonoBehaviour
{
    public static StanceAndRunicManager instance;

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

    [Header("Prefabs & Visuals")]
    [Tooltip("Prefab contenant un composant Light2D pour les bonus/malus de posture")]
    public GameObject stanceLightPrefab;

    [Header("UI References - Rune Description Panel")]
    [Tooltip("Le panneau GameObject qui contient la description de la rune")]
    public GameObject runeDescriptionPanel;
    [Tooltip("Le texte TextMeshProUGUI pour afficher le titre de la rune")]
    public TMPro.TextMeshProUGUI runeDescriptionTitleText;
    [Tooltip("Le texte TextMeshProUGUI pour afficher la description de la rune")]
    public TMPro.TextMeshProUGUI runeDescriptionContentText;

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
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        inputActions = new PlayerInputActions();

        // S'abonner aux événements d'input
        inputActions.Gameplay.ChangeStance.performed += ctx => CycleNextStance();
        inputActions.Gameplay.ChangeRune.performed += ctx => CycleNextRune();
    }

    private void OnEnable()
    {
        if (inputActions != null)
            inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();
    }

    private void Start()
    {
        // Sauvegarde des couleurs d'origine
        if (stanceBackgroundImage != null) originalStanceBgColor = stanceBackgroundImage.color;
        if (runeBackgroundImage != null) originalRuneBgColor = runeBackgroundImage.color;

        // S'assurer de la cohérence de l'état des runes équipées et actives au démarrage
        bool hasEquipped = runesList.Exists(r => r.isEquipped);
        if (hasEquipped)
        {
            bool hasActive = runesList.Exists(r => r.isEquipped && r.isActive);
            if (!hasActive)
            {
                RuneData firstEquipped = runesList.Find(r => r.isEquipped);
                if (firstEquipped != null) firstEquipped.isActive = true;
            }
        }

        // Initialisation automatique des TextMeshPro de description si non assignés
        if (runeDescriptionPanel != null)
        {
            if (runeDescriptionTitleText == null || runeDescriptionContentText == null)
            {
                var texts = runeDescriptionPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                if (texts.Length >= 1 && runeDescriptionTitleText == null) runeDescriptionTitleText = texts[0];
                if (texts.Length >= 2 && runeDescriptionContentText == null) runeDescriptionContentText = texts[1];
            }
            HideRuneDescription();
        }

        // Initialisation de l'UI avec les éléments équipés par défaut (s'il y en a)
        UpdateStanceUI();
        UpdateRuneUI();
    }

    private void Update()
    {
        // Si le panneau de description est affiché mais que l'inventaire est fermé, on le cache
        if (runeDescriptionPanel != null && runeDescriptionPanel.activeSelf)
        {
            if (InventoryManager.instance != null && InventoryManager.instance.inventory != null && !InventoryManager.instance.inventory.activeSelf)
            {
                HideRuneDescription();
            }
        }
    }

    /// <summary>
    /// Met à jour l'image de la posture équipée.
    /// </summary>
    public void UpdateStanceUI()
    {
        // Si on est dans le mode InsideTheSquare, on cache de force
        if (InsideTheSquareManager.instance != null)
        {
            if (stanceIconImage != null) stanceIconImage.enabled = false;
            if (stanceBackgroundImage != null) stanceBackgroundImage.enabled = false;
            return;
        }

        // Vérifier s'il y a au moins une posture débloquée
        bool hasUnlockedStance = stancesList.Exists(s => s.isUnlocked);
        if (!hasUnlockedStance)
        {
            if (stanceIconImage != null) stanceIconImage.enabled = false;
            if (stanceBackgroundImage != null) stanceBackgroundImage.enabled = false;
            return;
        }

        // S'assurer qu'au moins une posture débloquée est équipée (active)
        StanceData equippedStance = GetEquippedStance();
        if (equippedStance == null || !equippedStance.isUnlocked)
        {
            foreach (var s in stancesList) s.isEquipped = false;
            equippedStance = stancesList.Find(s => s.isUnlocked);
            if (equippedStance != null) equippedStance.isEquipped = true;
        }

        if (equippedStance != null && equippedStance.stance != null)
        {
            if (stanceBackgroundImage != null) stanceBackgroundImage.enabled = true;
            if (stanceIconImage != null)
            {
                stanceIconImage.sprite = equippedStance.stance.iconSprite;
                stanceIconImage.enabled = true;
            }
        }
        else
        {
            if (stanceIconImage != null) stanceIconImage.enabled = false;
            if (stanceBackgroundImage != null) stanceBackgroundImage.enabled = false;
        }
    }

    /// <summary>
    /// Met à jour l'image de la rune équipée.
    /// </summary>
    public void UpdateRuneUI()
    {
        // Si on est dans le mode InsideTheSquare, on cache de force
        if (InsideTheSquareManager.instance != null)
        {
            if (runeIconImage != null) runeIconImage.enabled = false;
            if (runeBackgroundImage != null) runeBackgroundImage.enabled = false;
            return;
        }

        // Vérifier si des runes sont équipées
        bool hasEquippedRune = runesList.Exists(r => r.isEquipped);
        if (!hasEquippedRune)
        {
            if (runeIconImage != null) runeIconImage.enabled = false;
            if (runeBackgroundImage != null) runeBackgroundImage.enabled = false;
            return;
        }

        // S'assurer qu'au moins une rune équipée est active
        RuneData activeRune = runesList.Find(r => r.isEquipped && r.isActive);
        if (activeRune == null)
        {
            activeRune = runesList.Find(r => r.isEquipped);
            if (activeRune != null) activeRune.isActive = true;
        }

        // Afficher la rune active
        if (activeRune != null && activeRune.rune != null)
        {
            if (runeBackgroundImage != null) runeBackgroundImage.enabled = true;
            if (runeIconImage != null)
            {
                runeIconImage.sprite = activeRune.rune.iconSprite;
                runeIconImage.enabled = true;
            }
        }
        else
        {
            if (runeIconImage != null) runeIconImage.enabled = false;
            if (runeBackgroundImage != null) runeBackgroundImage.enabled = false;
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
    /// Récupère la rune actuellement active.
    /// </summary>
    public RuneData GetEquippedRune()
    {
        return runesList.Find(r => r.isEquipped && r.isActive);
    }

    /// <summary>
    /// Tente d'équiper ou déséquiper une rune dans le deck de 3.
    /// </summary>
    public bool ToggleRuneEquipment(RuneSO runeSO)
    {
        RuneData data = runesList.Find(r => r.rune == runeSO);
        if (data == null) return false;

        // Si elle n'est pas débloquée, impossible de faire quoi que ce soit
        if (!data.isUnlocked) return false;

        if (data.isEquipped)
        {
            // Déséquiper
            data.isEquipped = false;
            data.isActive = false;

            // Si la rune déséquipée était active, on en active une autre parmi celles restantes équipées
            RuneData otherEquipped = runesList.Find(r => r.isEquipped);
            if (otherEquipped != null)
            {
                otherEquipped.isActive = true;
            }

            UpdateRuneUI();
            UpdatePlayerStats();
            HideRuneDescription();
            return true;
        }
        else
        {
            // Limite à 3 runes équipées
            int equippedCount = runesList.FindAll(r => r.isEquipped).Count;
            if (equippedCount >= 3)
            {
                return false;
            }

            data.isEquipped = true;

            // Si aucune rune n'est active actuellement, celle-ci le devient
            if (!runesList.Exists(r => r.isEquipped && r.isActive))
            {
                data.isActive = true;
            }

            UpdateRuneUI();
            UpdatePlayerStats();
            ShowRuneDescription(runeSO);
            return true;
        }
    }

    public void LoadSaveData(StanceAndRunicSaveData data)
    {
        if (data.stances != null)
        {
            foreach (var savedStance in data.stances)
            {
                var stanceData = stancesList.Find(s => s != null && s.stance != null && s.stance.id == savedStance.id);
                if (stanceData != null)
                {
                    stanceData.isUnlocked = savedStance.isUnlocked;
                    stanceData.isEquipped = savedStance.isEquipped;
                }
            }
        }

        if (data.runes != null)
        {
            foreach (var savedRune in data.runes)
            {
                var runeData = runesList.Find(r => r != null && r.rune != null && r.rune.id == savedRune.id);
                if (runeData != null)
                {
                    runeData.isUnlocked = savedRune.isUnlocked;
                    runeData.isEquipped = savedRune.isEquipped;
                    runeData.isActive = savedRune.isActive;
                }
            }
        }
        
        UpdateStanceUI();
        UpdateRuneUI();
    }

    /// <summary>
    /// Débloque une posture par son identifiant unique.
    /// </summary>
    public void UnlockStance(string stanceId)
    {
        StanceData data = stancesList.Find(s => s != null && s.stance != null && s.stance.id == stanceId);
        if (data != null)
        {
            data.isUnlocked = true;
            UpdateStanceUI();
        }
        else
        {
            Debug.LogWarning($"Posture avec l'ID {stanceId} introuvable dans la liste.");
        }
    }

    /// <summary>
    /// Débloque une rune par son identifiant unique.
    /// </summary>
    public void UnlockRune(string runeId)
    {
        RuneData data = runesList.Find(r => r != null && r.rune != null && r.rune.id == runeId);
        if (data != null)
        {
            data.isUnlocked = true;
            UpdateRuneUI();

            RuneSlotButton[] allButtons = FindObjectsOfType<RuneSlotButton>();
            foreach (RuneSlotButton btn in allButtons)
            {
                btn.RefreshUI();
            }
        }
        else
        {
            Debug.LogWarning($"Rune avec l'ID {runeId} introuvable dans la liste.");
        }
    }

    /// <summary>
    /// Affiche la description d'une rune donnée dans le panneau d'UI.
    /// </summary>
    public void ShowRuneDescription(RuneSO rune)
    {
        if (runeDescriptionPanel == null) return;

        if (runeDescriptionTitleText == null || runeDescriptionContentText == null)
        {
            var texts = runeDescriptionPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            if (texts.Length >= 1 && runeDescriptionTitleText == null) runeDescriptionTitleText = texts[0];
            if (texts.Length >= 2 && runeDescriptionContentText == null) runeDescriptionContentText = texts[1];
        }

        if (runeDescriptionTitleText != null && runeDescriptionContentText != null)
        {
            string titleKey = rune.id + "_Title";
            string descKey = rune.id + "_Description";

            string title = LocalizationManager.instance != null ? LocalizationManager.instance.GetText("RUNES", titleKey) : rune.id;
            string description = LocalizationManager.instance != null ? LocalizationManager.instance.GetText("RUNES", descKey) : "";

            runeDescriptionTitleText.text = title ?? rune.id;
            runeDescriptionContentText.text = description ?? "";
        }

        runeDescriptionPanel.SetActive(true);
    }

    /// <summary>
    /// Masque le panneau de description de rune.
    /// </summary>
    public void HideRuneDescription()
    {
        if (runeDescriptionPanel != null)
        {
            runeDescriptionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Vérifie si une interface utilisateur (inventaire, crafting, etc.) est ouverte ou si le jeu est en pause.
    /// </summary>
    private bool IsAnyUIOpen()
    {
        if (Time.timeScale == 0f) return true;

        if (InventoryManager.instance != null && InventoryManager.instance.inventory != null && InventoryManager.instance.inventory.activeSelf)
            return true;

        if (PlayerLevels.instance != null && PlayerLevels.instance.UIPlayerLevels != null && PlayerLevels.instance.UIPlayerLevels.activeSelf)
            return true;

        if (GameManager.CraftingManager.instance != null && GameManager.CraftingManager.instance.craftingMenu != null && GameManager.CraftingManager.instance.craftingMenu.activeSelf)
            return true;

        if (PlayerManager.instance != null && PlayerManager.instance.isEventPlaying)
            return true;

        return false;
    }

    // --- Méthodes de Cyclage ---

    /// <summary>
    /// Passe à la posture débloquée suivante.
    /// </summary>
    public void CycleNextStance()
    {
        if (IsAnyUIOpen()) return;
        if (InsideTheSquareManager.instance != null) return;
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
    /// Passe à la rune équipée suivante dans le deck.
    /// </summary>
    public void CycleNextRune()
    {
        if (IsAnyUIOpen()) return;
        if (InsideTheSquareManager.instance != null) return;
        List<RuneData> equippedRunes = runesList.FindAll(r => r.isEquipped);
        if (equippedRunes.Count == 0) return;

        int currentIndex = equippedRunes.FindIndex(r => r.isActive);

        if (currentIndex != -1)
            equippedRunes[currentIndex].isActive = false;

        int nextIndex = currentIndex == -1 ? 0 : (currentIndex + 1) % equippedRunes.Count;

        equippedRunes[nextIndex].isActive = true;
        UpdateRuneUI();
        UpdatePlayerStats();

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

    // ==========================================
    //               RUNES HOOKS
    // ==========================================
    
    public RuneType? GetActiveRuneType()
    {
        if (runesList == null) return null;
        var activeRune = runesList.Find(r => r.isActive && r.rune != null);
        return activeRune?.rune.runeType;
    }

    // Utilisé pour les modificateurs de statistiques passives dynamiques
    public void ApplyPassiveRuneStats(Stats playerStats)
    {
        RuneType? activeRune = GetActiveRuneType();
        if (activeRune == null) return;

        switch (activeRune)
        {
            case RuneType.Standard:
                playerStats.strength += Mathf.RoundToInt(playerStats.strength * 0.10f);
                break;
            case RuneType.Plenitude:
                LifeManager lm = playerStats.GetComponent<LifeManager>();
                if (lm != null && lm.life >= playerStats.health)
                {
                    playerStats.strength += Mathf.RoundToInt(playerStats.strength * 0.25f);
                    playerStats.critChance += 0.25f;
                    playerStats.critDamage += 0.25f;
                }
                break;
            case RuneType.Tempete:
                if (MeteoManager.instance != null)
                {
                    bool isBadWeather = false;
                    if (MeteoManager.instance.IsBlizzardActive()) isBadWeather = true;
                    if (MeteoManager.instance.rain != null && MeteoManager.instance.rain.activeSelf) isBadWeather = true;
                    if (MeteoManager.instance.dustStorm != null && MeteoManager.instance.dustStorm.activeSelf) isBadWeather = true;

                    if (isBadWeather)
                        playerStats.critDamage += 0.50f;
                    else
                        playerStats.critChance += 0.15f;
                }
                break;
            case RuneType.Conversion:
                int defenseToConvert = playerStats.defense / 2;
                playerStats.defense -= defenseToConvert;
                playerStats.strength += Mathf.RoundToInt(playerStats.strength * (defenseToConvert * 0.05f));
                break;
            case RuneType.Eclipse:
                if (MeteoManager.instance != null)
                {
                    if (MeteoManager.instance.time == false) // Night
                        playerStats.defense += Mathf.RoundToInt(playerStats.defense * 0.20f);
                    else
                        playerStats.defense -= Mathf.RoundToInt(playerStats.defense * 0.20f);
                }
                break;
            case RuneType.Encerclement:
                int monsterCount = 0;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(playerStats.transform.position, 5f);
                foreach (var hit in colliders)
                {
                    Stats s = hit.GetComponent<Stats>();
                    if (s != null && (s.entityType == EntityType.Monster || s.entityType == EntityType.Boss) && !s.isDying)
                        monsterCount++;
                }
                playerStats.defense += Mathf.RoundToInt(playerStats.defense * (0.05f * monsterCount));
                playerStats.critChance += 0.05f * monsterCount;
                break;
        }
    }

    public float GetRunicDamageMultiplier(Stats playerStats, Stats targetStats)
    {
        RuneType? activeRune = GetActiveRuneType();
        if (activeRune == null) return 1f;

        float multiplier = 1f;
        switch (activeRune)
        {
            case RuneType.Rage:
                LifeManager lm = playerStats.GetComponent<LifeManager>();
                if (lm != null)
                {
                    float missingHealthPct = 1f - ((float)lm.life / playerStats.health);
                    multiplier += missingHealthPct; // +1% per 1% lost
                }
                break;
            case RuneType.Elan:
                // Pour l'Élan, on vérifiera dans PlayerController ou ici via un bool
                // On peut utiliser PlayerManager.instance.isDogingTime ? (Qui est le slowmo de l'esquive)
                if (PlayerManager.instance != null && PlayerManager.instance.isDogingTime)
                {
                    multiplier += 0.20f;
                }
                break;
            case RuneType.Sacrifice:
                LifeManager pLm = playerStats.GetComponent<LifeManager>();
                if (pLm != null)
                {
                    int healthCost = Mathf.RoundToInt(playerStats.health * 0.10f);
                    if (healthCost < 1) healthCost = 1;
                    pLm.TakeDamage(healthCost, Color.red, false, true);
                    multiplier += 0.50f;
                }
                break;
            case RuneType.Surtension:
                EntityEffects eff = targetStats.GetComponent<EntityEffects>();
                if (eff != null && (eff.isFire || eff.isFreeze || eff.isPoison || eff.isSlimed))
                    multiplier += 0.50f;
                break;
            case RuneType.Instabilite:
                multiplier += Random.Range(-0.50f + (playerStats.luck * 0.01f), 0.50f + (playerStats.luck * 0.01f));
                break;
            case RuneType.Surcharge:
                multiplier += 0.40f;
                break;
        }
        return Mathf.Max(0f, multiplier);
    }

    public float GetRunicDamageTakenMultiplier(Stats playerStats)
    {
        RuneType? activeRune = GetActiveRuneType();
        if (activeRune == RuneType.Surcharge)
        {
            return 1.30f; // +30% pris
        }
        return 1f;
    }

    public void OnMonsterKilled(GameObject monster, Stats playerStats)
    {
        if (GetActiveRuneType() == RuneType.Triomphe)
        {
            Stats monsterStats = monster.GetComponent<Stats>();
            LifeManager pLm = playerStats.GetComponent<LifeManager>();
            if (monsterStats != null && pLm != null)
            {
                int healAmount = Mathf.RoundToInt(monsterStats.health * 0.05f);
                if (healAmount < 1) healAmount = 1;
                pLm.life = Mathf.Min(pLm.life + healAmount, playerStats.health);
            }
        }
    }

    public void OnCriticalHitDealt(Stats playerStats, int damageAmount)
    {
        if (GetActiveRuneType() == RuneType.Prosperite)
        {
            playerStats.money += damageAmount;
            if (NotificationManager.instance != null)
            {
                NotificationManager.instance.ShowSpecialPopUpSquareCoins(
                    (playerStats.money - damageAmount).ToString(),
                    playerStats.money.ToString());
            }
        }
    }

    private void UpdatePlayerStats()
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            Stats playerStats = PlayerManager.instance.player.GetComponent<Stats>();
            if (playerStats != null)
            {
                playerStats.UpdateStats();
            }
        }
    }
}
