using UnityEngine;
using UnityEngine.EventSystems;

public class UICursor : MonoBehaviour
{
    public GameObject cursor; // Le prefab de curseur
    private RectTransform cursorRectTransform;

    void Start()
    {
        cursorRectTransform = cursor.GetComponent<RectTransform>();
        cursor.SetActive(false); // Désactiver le curseur au démarrage
    }

    void Update()
    {
        // Obtenir le bouton actuellement sélectionné par l'EventSystem
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;

        if (selectedButton != null)
        {
            // Activer le curseur et le placer sur le bouton sélectionné
            cursor.SetActive(true);
            RectTransform selectedRectTransform = selectedButton.GetComponent<RectTransform>();

            if (selectedRectTransform != null)
            {
                cursorRectTransform.position = selectedRectTransform.position;
                // Optionnel : Ajuster la taille du curseur pour correspondre à la taille du bouton
                cursorRectTransform.sizeDelta = selectedRectTransform.sizeDelta;
            }
        }
        else
        {
            // Désactiver le curseur si aucun bouton n'est sélectionné
            cursor.SetActive(false);
        }
    }
}
