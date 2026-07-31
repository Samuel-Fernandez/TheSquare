using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class RuneSlotButton : MonoBehaviour
{
    [Header("Rune Reference")]
    [Tooltip("Le ScriptableObject de la rune associée à ce bouton")]
    public RuneSO runeSO;

    [Header("UI References (Children)")]
    [Tooltip("L'image affichant l'icône de la rune")]
    public Image runeImage;
    [Tooltip("Le texte TextMeshProUGUI qui affiche 'E' si équipée")]
    public TextMeshProUGUI equippedText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void Start()
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    /// <summary>
    /// Met à jour l'aspect visuel du bouton selon l'état de la rune dans le StanceAndRunicManager.
    /// </summary>
    public void RefreshUI()
    {
        if (runeSO == null) return;

        StanceAndRunicManager manager = StanceAndRunicManager.instance;
        if (manager == null) return;

        RuneData data = manager.runesList.Find(r => r.rune == runeSO);
        if (data == null) return;

        if (!data.isUnlocked)
        {
            // Rune non débloquée : bouton désactivé/grisé, pas d'image
            if (runeImage != null)
            {
                runeImage.sprite = null;
                runeImage.enabled = false;
            }
            if (equippedText != null)
            {
                equippedText.gameObject.SetActive(false);
            }

            button.interactable = false;
        }
        else
        {
            // Rune débloquée
            button.interactable = true;

            if (runeImage != null)
            {
                runeImage.sprite = data.rune.iconSprite;
                runeImage.enabled = data.rune.iconSprite != null;
                runeImage.color = Color.white;
            }

            if (equippedText != null)
            {
                // Afficher le texte "E" si la rune est équipée
                equippedText.text = "E";
                equippedText.gameObject.SetActive(data.isEquipped);
            }
        }
    }

    /// <summary>
    /// Appelé lors du clic sur le bouton.
    /// </summary>
    private void OnButtonClick()
    {
        if (runeSO == null) return;

        StanceAndRunicManager manager = StanceAndRunicManager.instance;
        if (manager == null) return;

        // Tenter d'équiper/déséquiper
        if (manager.ToggleRuneEquipment(runeSO))
        {
            // Rafraîchir tous les boutons de runes dans la scène pour mettre à jour l'affichage
            RuneSlotButton[] allButtons = FindObjectsOfType<RuneSlotButton>();
            foreach (RuneSlotButton btn in allButtons)
            {
                btn.RefreshUI();
            }
        }
        else
        {
            // Optionnel : Jouer un son d'erreur
            SoundContainer soundContainer = manager.GetComponent<SoundContainer>();
            if (soundContainer != null)
            {
                soundContainer.PlayUISound("denied", 1);
            }
        }
    }
}
