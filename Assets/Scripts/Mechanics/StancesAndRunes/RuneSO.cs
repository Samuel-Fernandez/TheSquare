using UnityEngine;

[CreateAssetMenu(fileName = "New Rune", menuName = "Combat/Rune", order = 2)]
public class RuneSO : ScriptableObject
{
    [Tooltip("Identifiant unique de la rune (ex: 'rune_standard')")]
    public string id;
    
    [Tooltip("L'icône qui sera affichée dans l'UI")]
    public Sprite iconSprite;

    [Tooltip("Le type de rune pour l'application des modificateurs")]
    public RuneType runeType;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id)) return;
        
        string lowerId = id.ToLower();
        if (lowerId.Contains("battle") || lowerId.Contains("standard")) runeType = RuneType.Standard;
        else if (lowerId.Contains("rage")) runeType = RuneType.Rage;
        else if (lowerId.Contains("impetus") || lowerId.Contains("elan")) runeType = RuneType.Elan;
        else if (lowerId.Contains("sacrifice")) runeType = RuneType.Sacrifice;
        else if (lowerId.Contains("serenity") || lowerId.Contains("plenitude")) runeType = RuneType.Plenitude;
        else if (lowerId.Contains("secondbreath") || lowerId.Contains("triomphe")) runeType = RuneType.Triomphe;
        else if (lowerId.Contains("prosperity") || lowerId.Contains("prosperite")) runeType = RuneType.Prosperite;
        else if (lowerId.Contains("overvoltage") || lowerId.Contains("surtension")) runeType = RuneType.Surtension;
        else if (lowerId.Contains("chaos") || lowerId.Contains("instabilite")) runeType = RuneType.Instabilite;
        else if (lowerId.Contains("mimesis") || lowerId.Contains("mimetisme")) runeType = RuneType.Mimetisme;
        else if (lowerId.Contains("meteo") || lowerId.Contains("tempete")) runeType = RuneType.Tempete;
        else if (lowerId.Contains("surcharge")) runeType = RuneType.Surcharge;
        else if (lowerId.Contains("conversion")) runeType = RuneType.Conversion;
        else if (lowerId.Contains("eclipse")) runeType = RuneType.Eclipse;
        else if (lowerId.Contains("encirclement")) runeType = RuneType.Encerclement;
    }
}

public enum RuneType
{
    Standard,
    Rage,
    Elan,
    Sacrifice,
    Plenitude,
    Triomphe,
    Prosperite,
    Surtension,
    Instabilite,
    Mimetisme,
    Tempete,
    Surcharge,
    Conversion,
    Eclipse,
    Encerclement
}
