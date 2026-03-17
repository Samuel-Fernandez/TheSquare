using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealManager : MonoBehaviour
{
    public static SealManager instance;

    [Header("Current Seal")]
    public Seal equippedSeal;
    public bool sealChanged = false;

    [Header("Seal Database")]
    [Tooltip("Pour sauvegarder les sceaux crs")]
    public List<Seal> createdSeals = new List<Seal>();

    [Header("Archetype Sprites")]
    public Sprite buffSprite;
    public Sprite resonanceSprite;
    public Sprite auraSprite;
    public Sprite momentumSprite;
    public Sprite prismaticSprite;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Gnre le seal et l'quipe directement
    public Seal GenerateSealObject(SpecialItems[] specialItems)
    {
        // Créer une nouvelle instance de Seal
        Seal newSeal = ScriptableObject.CreateInstance<Seal>();

        // Générer le sceau avec les 4 items d'abord afin que ses stats et sa composition d'essences soient générées
        newSeal.GenerateSeal(specialItems);

        // Assigner le sprite d'archétype
        int activeArchetypesCount = 0;
        if (newSeal.isResonanceActive) activeArchetypesCount++;
        if (newSeal.isBuffActive) activeArchetypesCount++;
        if (newSeal.isAuraActive) activeArchetypesCount++;
        if (newSeal.isMomentumActive) activeArchetypesCount++;

        if (activeArchetypesCount >= 2) newSeal.archetypeSprite = prismaticSprite;
        else if (newSeal.isBuffActive) newSeal.archetypeSprite = buffSprite;
        else if (newSeal.isResonanceActive) newSeal.archetypeSprite = resonanceSprite;
        else if (newSeal.isAuraActive) newSeal.archetypeSprite = auraSprite;
        else if (newSeal.isMomentumActive) newSeal.archetypeSprite = momentumSprite;

        // Maintenant qu'il a ses stats, calculer un nom via GenerateSealName
        newSeal.name = GenerateSealName(newSeal);

        newSeal.originalItemIDs = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            newSeal.originalItemIDs.Add(specialItems[i].itemId);
        }

        return newSeal;
    }

    // Génère le seal et l'équipe directement
    public void CreateSeal(SpecialItems[] specialItems)
    {
        if (specialItems == null || specialItems.Length != 4)
        {
            Debug.LogError("CreateSeal requires exactly 4 SpecialItems!");
            return;
        }

        // Vérifier que tous les items sont valides
        for (int i = 0; i < specialItems.Length; i++)
        {
            if (specialItems[i] == null)
            {
                Debug.LogError($"SpecialItem at index {i} is null!");
                return;
            }
        }

        Seal newSeal = GenerateSealObject(specialItems);

        // Équiper le nouveau sceau
        equippedSeal = newSeal;
        sealChanged = true;

        // Optionnel : Sauvegarder dans la database
        createdSeals.Add(newSeal);
    }

    public SealManagerData GetSaveData()
    {
        List<SaveSealData> sealsData = new List<SaveSealData>();
        int equippedIndex = -1;
        for (int i = 0; i < createdSeals.Count; i++)
        {
            sealsData.Add(new SaveSealData(createdSeals[i].originalItemIDs));
            if (equippedSeal == createdSeals[i])
            {
                equippedIndex = i;
            }
        }
        return new SealManagerData(sealsData, equippedIndex);
    }

    public void LoadSaveData(SealManagerData data)
    {
        createdSeals.Clear();
        equippedSeal = null;

        if (data.createdSeals == null) return;

        for (int i = 0; i < data.createdSeals.Count; i++)
        {
            SpecialItems[] items = new SpecialItems[4];
            bool valid = true;
            for (int j = 0; j < 4; j++)
            {
                string id = data.createdSeals[i].originalItemIDs[j];
                Item fetchedItem = SaveManager.instance.itemDatabase.GetItemByID(id);
                if (fetchedItem != null && fetchedItem is SpecialItems)
                {
                    items[j] = (SpecialItems)ScriptableObjectUtility.Clone(fetchedItem);
                    items[j].itemId = id;
                }
                else
                {
                    valid = false;
                }
            }
            if (valid)
            {
                Seal newSeal = GenerateSealObject(items);
                createdSeals.Add(newSeal);
            }
        }

        if (data.equippedSealIndex >= 0 && data.equippedSealIndex < createdSeals.Count)
        {
            equippedSeal = createdSeals[data.equippedSealIndex];
            sealChanged = true;
        }
    }

    // Gnre un nom unique pour le sceau bas sur les archtypes et l'lment
    private string GenerateSealName(Seal seal)
    {
        // 1. Déterminer l'Archétype
        string archetypeAdjKey = "ARCHETYPE_NONE";
        int activeArchetypesCount = 0;
        if (seal.isResonanceActive) activeArchetypesCount++;
        if (seal.isBuffActive) activeArchetypesCount++;
        if (seal.isAuraActive) activeArchetypesCount++;
        if (seal.isMomentumActive) activeArchetypesCount++;

        if (activeArchetypesCount >= 2)
        {
            archetypeAdjKey = "ARCHETYPE_MULTIPLE";
        }
        else if (activeArchetypesCount == 1)
        {
            if (seal.isResonanceActive) archetypeAdjKey = "ARCHETYPE_RESONANCE";
            else if (seal.isBuffActive) archetypeAdjKey = "ARCHETYPE_BUFF";
            else if (seal.isAuraActive) archetypeAdjKey = "ARCHETYPE_AURA";
            else if (seal.isMomentumActive) archetypeAdjKey = "ARCHETYPE_MOMENTUM";
        }

        string archetypeAdj = LocalizationManager.instance?.GetText("SEALS_NAMES", archetypeAdjKey) ?? archetypeAdjKey;

        // Le mot de base "Sceau" / "Seal"
        string sealBaseName = LocalizationManager.instance?.GetText("SEALS_NAMES", "SEAL_BASE_NAME") ?? "Seal";

        // 2. Récupérer les 2 meilleurs éléments
        // essenceComposition est déjà trié par pourcentage décroissant (grâce au tri dans Seal.GenerateSeal)
        Spirit primarySpirit = null;
        Spirit secondarySpirit = null;

        if (seal.essenceComposition.Count > 0)
        {
            primarySpirit = seal.essenceComposition[0].essence;
        }
        if (seal.essenceComposition.Count > 1)
        {
            secondarySpirit = seal.essenceComposition[1].essence;
        }

        if (primarySpirit == null) // Cas de figure anormal
        {
            return $"{archetypeAdj} {sealBaseName}";
        }

        string primaryId = primarySpirit.essenceID;
        if (!string.IsNullOrEmpty(primaryId)) primaryId = char.ToUpper(primaryId[0]) + primaryId.Substring(1).ToLower();

        string primaryElementNounKey = "ELEMENT_NOUN_" + primaryId;
        string primaryElementNoun = LocalizationManager.instance?.GetText("SEALS_NAMES", primaryElementNounKey) ?? primaryId;

        // S'il n'y a pas d'élément secondaire
        if (secondarySpirit == null)
        {
            // Format: {0} {1} {2} (ex: Radiant Seal of Water)
            string result = LocalizationManager.instance?.GetText("SEALS_NAMES", "FORMAT_NO_SEC_ELEMENT", archetypeAdj, sealBaseName, primaryElementNoun);
            return result ?? string.Format("{0} {1} {2}", archetypeAdj, sealBaseName, primaryElementNoun);
        }
        else
        {
            string secondaryId = secondarySpirit.essenceID;
            if (!string.IsNullOrEmpty(secondaryId)) secondaryId = char.ToUpper(secondaryId[0]) + secondaryId.Substring(1).ToLower();

            // Format: {0} {1} {2} {3} (ex: Radiant Explosive Seal of Water)
            string secondaryElementAdjKey = "ELEMENT_ADJ_" + secondaryId;
            string secondaryElementAdj = LocalizationManager.instance?.GetText("SEALS_NAMES", secondaryElementAdjKey) ?? secondaryId;

            string result = LocalizationManager.instance?.GetText("SEALS_NAMES", "FORMAT_FULL_NAME", archetypeAdj, secondaryElementAdj, sealBaseName, primaryElementNoun);
            return result ?? string.Format("{0} {1} {2} {3}", archetypeAdj, secondaryElementAdj, sealBaseName, primaryElementNoun);
        }
    }

    // Rcupre les stats du sceau quip (utilitaire)
    public bool HasSealEquipped()
    {
        return equippedSeal != null;
    }
}