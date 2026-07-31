using UnityEngine;
using UnityEditor;

public class GenerateurIDWindow : EditorWindow
{
    private string idActuel = "";

    [MenuItem("Tools/Générateur d'ID")]
    public static void AfficherFenetre()
    {
        GetWindow<GenerateurIDWindow>("Générateur ID");
    }

    private void OnGUI()
    {
        GUILayout.Label("Générer un ID (6 caractères)", EditorStyles.boldLabel);

        if (GUILayout.Button("Générer l'ID"))
        {
            idActuel = GenererID(6);
            EditorGUIUtility.systemCopyBuffer = idActuel;
            Debug.Log($"ID généré : {idActuel} (Copié dans le presse-papiers)");
        }

        EditorGUILayout.Space();

        if (!string.IsNullOrEmpty(idActuel))
        {
            EditorGUILayout.TextField("ID Actuel", idActuel);
            if (GUILayout.Button("Copier dans le presse-papiers"))
            {
                EditorGUIUtility.systemCopyBuffer = idActuel;
            }
        }
    }

    private string GenererID(int longueur)
    {
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] tableauCaracteres = new char[longueur];
        var aleatoire = new System.Random();

        for (int i = 0; i < longueur; i++)
        {
            tableauCaracteres[i] = caracteres[aleatoire.Next(caracteres.Length)];
        }

        return new string(tableauCaracteres);
    }
}
