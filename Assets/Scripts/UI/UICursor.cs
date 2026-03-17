using UnityEngine;
using UnityEngine.EventSystems;

public class UICursor : MonoBehaviour
{
    public GameObject cursor; // Le prefab de curseur
    private RectTransform cursorRectTransform;

    void Start()
    {
        cursorRectTransform = cursor.GetComponent<RectTransform>();
        cursor.SetActive(false); // D�sactiver le curseur au d�marrage
    }

    void Update()
    {
        // Obtenir le bouton actuellement s�lectionn� par l'EventSystem
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;

        if (selectedButton != null)
        {
            // Activer le curseur et le placer sur le bouton s�lectionn�
            cursor.SetActive(true);
            RectTransform selectedRectTransform = selectedButton.GetComponent<RectTransform>();

            if (selectedRectTransform != null)
            {
                cursorRectTransform.position = selectedRectTransform.position;
                // Optionnel : Ajuster la taille du curseur pour correspondre � la taille du bouton
                cursorRectTransform.sizeDelta = selectedRectTransform.sizeDelta;
            }
        }
        else
        {
            // D�sactiver le curseur si aucun bouton n'est s�lectionn�
            cursor.SetActive(false);
        }
    }
}
