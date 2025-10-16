using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LittleBubbleText : MonoBehaviour
{
    public GameObject objectToFollow;  // Le GameObject que la bulle de texte suivra
    public float duration;             // Durée d'affichage de la bulle de texte
    public string textToWrite;         // Texte à afficher
    public float upOffset;
    public TextMeshProUGUI text;       // Composant TextMeshProUGUI pour le texte

    private void Start()
    {
        StartCoroutine(TypeText(text, textToWrite, duration));
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        if (objectToFollow != null)
        {
            // Mettre à jour la position de la bulle de texte
            transform.position = objectToFollow.transform.position + new Vector3(0, 1 + upOffset, 0);
        }
    }

    private IEnumerator TypeText(TextMeshProUGUI textMeshPro, string text, float duration)
    {
        textMeshPro.text = ""; // Réinitialiser le texte
        foreach (char letter in text.ToCharArray())
        {
            textMeshPro.text += letter;
            yield return new WaitForSeconds((duration - 2) / text.Length);
        }
    }
}
