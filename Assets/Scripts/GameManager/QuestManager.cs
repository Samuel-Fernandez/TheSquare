using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[System.Serializable]
public class QuestContainer
{
    public string id;
    public string idLocation;
    public string pnjID;
    public List<int> nbMonsterKilledSinceAccepted;
    public List<string> idEquipement;

    public QuestContainer(string id, string idLocation, string pnjID, List<int> nbMonsterKilledSinceAccepted, List<string> idEquipement)
    {
        this.id = id;
        this.idLocation = idLocation;
        this.pnjID = pnjID;
        this.nbMonsterKilledSinceAccepted = nbMonsterKilledSinceAccepted ?? new List<int>();
        this.idEquipement = idEquipement ?? new List<string>();

    }
}


public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    // Si le menu est ouvert
    bool isOpen = false;
    public bool canOpenQuests = true;

    // Censé contenir toutes les quêtes
    public List<Quests> questsDB;

    // Quêtes terminées ou en attente (identifiant)
    public List<QuestContainer> completedQuests = new List<QuestContainer>();
    public List<QuestContainer> waitingQuests = new List<QuestContainer>();

    public GameObject rewardSlotPrefab;
    public GameObject rewardContainer;
    public GameObject equipementStats;

    // UI pour accepter une quête
    public GameObject uiAcceptQuest;
    Quests actualQuest;
    public GameObject acceptButton;

    // Text à traduire
    public TextMeshProUGUI questObjectiveTitleTxt;
    public TextMeshProUGUI questRewardTitleTxt;

    // Text à mettre à jour avec les infos de la quête
    public TextMeshProUGUI questNameTxt;
    public TextMeshProUGUI questDescriptionTxt;
    public TextMeshProUGUI questObjectiveTxt;
    public TextMeshProUGUI questRewardSquareCoinsTxt;
    public TextMeshProUGUI questLocationTxt;
    public TextMeshProUGUI questPNJTxt;


    // Contient tous les selecteurs de quête
    public GameObject uiQuestSelector;
    public GameObject gridView;
    public GameObject questSelectorPrefab;
    public List<GameObject> questSelectors = new List<GameObject>();

    private SoundContainer soundContainer;



    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        soundContainer = GetComponent<SoundContainer>();
    }

    private void Update()
    {
        if(PlayerManager.instance.playerInputActions.Menu.Quests.triggered && canOpenQuests)
        {
            if(!isOpen)
            {
                OpenQuestMenu();
                uiQuestSelector.SetActive(true);
            }
            else
            {
                // Le jeu reprend

                Time.timeScale = 1;
                InventoryManager.instance.canOpenInventory = true;
                uiQuestSelector.SetActive(false);
                uiAcceptQuest.SetActive(false);
                
            }

            isOpen = !isOpen;

        }
    }

    // FAIRE QUAND INVENTAIRE PEUT PAS CONTENIR TOUTES LES RECOMPENSES
    public void FinishQuest(Quests quest)
    {
        // Créer un QuestContainer correspondant
        QuestContainer completedQuest = new QuestContainer(
            quest.id,
            quest.location?.sceneID ?? "",
            quest.pnjID,
            quest.monsterObjectiveList?.ConvertAll(obj => obj.nbMonsterKilledSinceAccepted) ?? new List<int>(),
            quest.rewardEquipement?.ConvertAll(item => item.itemId) ?? new List<string>()
        );

        // Ajouter la quête au journal des quêtes terminées
        completedQuests.Add(completedQuest);

        if (quest.reward == QuestReward.EQUIPEMENT)
        {
            foreach (var waitingQuest in waitingQuests)
            {
                if(waitingQuest.id == quest.id)
                {
                    for (int i = 0; i < waitingQuest.idEquipement.Count; i++)
                    {
                        quest.rewardEquipement[i].itemId = waitingQuest.idEquipement[i];
                    }
                }
            }
        }

        // Retirer la quête des quêtes en attente
        waitingQuests.RemoveAll(q => q.id == quest.id);

        NotificationManager.instance.ShowTitle(
            LocalizationManager.instance.GetText("QUEST", quest.id + "_NAME"),
            LocalizationManager.instance.GetText("UI", "COMPLETED_QUEST")
        );

        

        //Retire les ressources demandées
        if(quest.completionCondition == QuestCompletionCondition.RESOURCES)
        {
            foreach (var item in quest.resourcesObjective)
            {
                PlayerManager.instance.GetSpecialItem(item.item.GetID()).nb -= item.nb;
            }
        }

        switch (quest.reward)
        {
            case QuestReward.NONE:
                break;
            case QuestReward.SQUARE_COINS:
                PlayerManager.instance.player.GetComponent<Stats>().money += quest.nbSquareCoins;
                NotificationManager.instance.ShowSpecialPopUpSquareCoins(
                    PlayerManager.instance.player.GetComponent<Stats>().money.ToString(),
                    (PlayerManager.instance.player.GetComponent<Stats>().money + quest.nbSquareCoins).ToString()
                );
                break;
            case QuestReward.ITEMS:
                foreach (var item in quest.rewardSpecialItem)
                {
                    NotificationManager.instance.ShowPopup(
                        LocalizationManager.instance.GetText("UI", "ITEM_RESUME", item.nb) +
                        LocalizationManager.instance.GetText("items", item.rewardItem.itemId + "_NAME")
                    );

                    PlayerManager.instance.GetSpecialItem(item.rewardItem.GetID()).nb += item.nb;
                }
                break;
            case QuestReward.EQUIPEMENT:
                foreach (var item in quest.rewardEquipement)
                {
                    if (!Equipement.instance.InventoryFull())
                    {
                        Equipement.instance.AddItem(item);
                        if(item is Weapon weapon)
                            weapon.GenerateStats();
                        if (item is Helmet helmet)
                            helmet.GenerateStats();
                        if (item is Chestplate chestplate)
                            chestplate.GenerateStats();
                        if (item is Leggings leggings)
                            leggings.GenerateStats();
                        if (item is Boots boots)
                            boots.GenerateStats();

                        NotificationManager.instance.ShowPopup(
                            LocalizationManager.instance.GetText("UI", "NOTIFICATION_GET_TEXT") +
                            " " + LocalizationManager.instance.GetText("items", item.GetID() + "_NAME")
                        );
                    }
                }
                break;
            default:
                break;
        }

        soundContainer.PlaySound("QuestCompletion", 0);

    }


    public void ResetSelectors()
    {

        questSelectors.Clear();

        // Supprimer les selectors
        foreach (Transform child in gridView.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator SelectFirstQuestNextFrame()
    {
        yield return null; // attendre une frame

        if (questSelectors.Count > 0 && questSelectors[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(questSelectors[0]);
            questSelectors[0].GetComponent<QuestSelection>().Selection();
        }
    }


    public void OpenQuestMenu()
    {
        ResetSelectors();

        foreach (var waitingQuest in waitingQuests)
        {
            foreach (var quest in questsDB)
            {
                if(quest.id == waitingQuest.id)
                {
                    Quests questCopy = ScriptableObjectUtility.Clone(quest);

                    questCopy.pnjID = waitingQuest.pnjID;


                    // Récupération de l'endroit
                    foreach (var region in MeteoManager.instance.regions)
                    {
                        foreach (var scene in region.scenes)
                        {
                            if (scene.SceneName == waitingQuest.idLocation)
                            {
                                questCopy.location = scene;
                            }
                        }
                    }

                    // Récupération des nbMonsterKilled...
                    if (waitingQuest.nbMonsterKilledSinceAccepted.Count > 0)
                    {
                        for (int i = 0; i < waitingQuest.nbMonsterKilledSinceAccepted.Count; i++)
                        {
                            questCopy.monsterObjectiveList[i].nbMonsterKilledSinceAccepted = waitingQuest.nbMonsterKilledSinceAccepted[i];
                        } 
                    }

                    // Récupération de l'équipement
                    if(waitingQuest.idEquipement.Count > 0)
                    {
                        for(int i = 0; i < waitingQuest.idEquipement.Count; i++)
                        {
                            questCopy.rewardEquipement[i].itemId = waitingQuest.idEquipement[i];

                            if (questCopy.rewardEquipement[i] is Weapon weapon)
                            {
                                weapon.GenerateStats();
                            }
                            else if (questCopy.rewardEquipement[i] is Boots boots)
                            {
                                boots.GenerateStats();
                            }
                            else if (questCopy.rewardEquipement[i] is Chestplate chestplate)
                            {
                                chestplate.GenerateStats();
                            }
                            else if (questCopy.rewardEquipement[i] is Leggings leggings)
                            {
                                leggings.GenerateStats();
                            }
                            else if (questCopy.rewardEquipement[i] is Helmet helmet)
                            {
                                helmet.GenerateStats();
                            }

                        }
                    }


                    // SELECTOR ??
                    GameObject questSelectorInstance = Instantiate(questSelectorPrefab, gridView.transform);
                    QuestSelection questSelectionComponent = questSelectorInstance.GetComponent<QuestSelection>();
                    questSelectionComponent.quest = questCopy;
                    questSelectionComponent.SetQuestSelection(questCopy); // AJOUT DE CETTE LIGNE
                    questSelectors.Add(questSelectorInstance);
                }
            }

            StartCoroutine(SelectFirstQuestNextFrame());
        }

        foreach (var completedQuest in completedQuests)
        {
            foreach (var quest in questsDB)
            {
                if (quest.id == completedQuest.id)
                {
                    Quests questCopy = ScriptableObjectUtility.Clone(quest);

                    questCopy.pnjID = completedQuest.pnjID;

                    // Récupération de l'endroit
                    foreach (var region in MeteoManager.instance.regions)
                    {
                        foreach (var scene in region.scenes)
                        {
                            if (scene.SceneName == completedQuest.idLocation)
                            {
                                questCopy.location = scene;
                            }
                        }
                    }

                    // Récupération des nbMonsterKilled...
                    if (completedQuest.nbMonsterKilledSinceAccepted.Count > 0)
                    {
                        for (int i = 0; i < completedQuest.nbMonsterKilledSinceAccepted.Count; i++)
                        {
                            questCopy.monsterObjectiveList[i].nbMonsterKilledSinceAccepted = completedQuest.nbMonsterKilledSinceAccepted[i];
                        }
                    }

                    // Récupération de l'équipement
                    if (completedQuest.idEquipement.Count > 0)
                    {
                        for (int i = 0; i < completedQuest.idEquipement.Count; i++)
                        {
                            questCopy.rewardEquipement[i].itemId = completedQuest.idEquipement[i];

                            if (questCopy.rewardEquipement[i] is Weapon weapon)
                            {
                                weapon.GenerateStats();
                            }
                            else if (questCopy.rewardEquipement[i] is Boots boots)
                            {
                                boots.GenerateStats();
                            }
                            else if (questCopy.rewardEquipement[i] is Chestplate chestplate)
                            {
                                chestplate.GenerateStats();
                            }
                            else if (questCopy.rewardEquipement[i] is Leggings leggings)
                            {
                                leggings.GenerateStats();
                            }
                            else if (questCopy.rewardEquipement[i] is Helmet helmet)
                            {
                                helmet.GenerateStats();
                            }

                        }
                    }

                    // SELECTOR ??
                    GameObject questSelectorInstance = Instantiate(questSelectorPrefab, gridView.transform);
                    QuestSelection questSelectionComponent = questSelectorInstance.GetComponent<QuestSelection>();
                    questSelectionComponent.quest = questCopy;
                    questSelectionComponent.SetQuestSelection(questCopy); // AJOUT DE CETTE LIGNE
                    questSelectors.Add(questSelectorInstance);

                }
            }
        }

        if(questSelectors.Count > 0)
            questSelectors[0].GetComponent<QuestSelection>().Selection();

    }

    public void AcceptQuest()
    {
        // Le jeu reprend
        Time.timeScale = 1;
        InventoryManager.instance.canOpenInventory = true;

        // TODO: FAIRE LE LOCALIZATION MANAGER ET VERIFIER
        if(completedQuests.Count == 0 && waitingQuests.Count == 0)
            NotificationManager.instance.ShowPopup("You can access the Quest menu with Q or SELECT");

        // Initialiser les listes avec des valeurs par défaut vides
        List<string> equipementID = actualQuest.reward == QuestReward.EQUIPEMENT
            ? actualQuest.rewardEquipement.ConvertAll(item => item.itemId)
            : new List<string>();

        List<int> nbKilledProgressionSinceAccepted = actualQuest.completionCondition == QuestCompletionCondition.KILL_MONSTER
            ? actualQuest.monsterObjectiveList.ConvertAll(item => item.nbMonsterKilledSinceAccepted)
            : new List<int>();

        // Ajouter la quête en attente avec les données nécessaires
        waitingQuests.Add(new QuestContainer(
            actualQuest.id,
            actualQuest.location?.sceneID ?? "",
            actualQuest.pnjID,
            nbKilledProgressionSinceAccepted,
            equipementID
        ));

        uiAcceptQuest.gameObject.SetActive(false);
    }


    public Quests GetQuestByID(string id)
    {
        foreach (var quest in questsDB)
        {
            if(id == quest.id)
            {
                return ScriptableObjectUtility.Clone(quest);
            }
        }

        return null;
    }

    public void SelectQuest(Quests quest)
    {
        OpenAcceptUI(quest, false);
    }

    public void ResetAllSelector()
    {
        foreach (var item in questSelectors)
        {
            item.GetComponent<QuestSelection>().ResetColor();
        }
    }

    public void ResetField()
    {
        // Réinitialiser la quête actuelle
        this.actualQuest = null;

        // Réinitialiser les textes
        questNameTxt.text = "";
        questDescriptionTxt.text = "";
        questObjectiveTxt.text = "";
        questRewardSquareCoinsTxt.text = "";
        questLocationTxt.text = "";
        questPNJTxt.text = "";

        // Supprimer les slots de récompenses
        foreach (Transform child in rewardContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }


    public void OpenAcceptUI(Quests quest, bool fromPNJ = true)
    {
        // Stop le temps
        Time.timeScale = 0;
        InventoryManager.instance.canOpenInventory = false;

        ResetField();

        this.actualQuest = quest;
        acceptButton.SetActive(fromPNJ);
        gridView.SetActive(!fromPNJ);


        // Ouverture de l'UI
        uiAcceptQuest.SetActive(true);

        if(fromPNJ)
            EventSystem.current.SetSelectedGameObject(acceptButton);

        questNameTxt.text = LocalizationManager.instance.GetText("QUEST", quest.id + "_NAME");
        questDescriptionTxt.text = LocalizationManager.instance.GetText("QUEST", quest.id + "_DESCRIPTION");
        questPNJTxt.text = LocalizationManager.instance.GetText("QUEST", quest.id + "_PNJNAME");



        // OBJECTIFS
        switch (quest.completionCondition)
        {
            case QuestCompletionCondition.KILL_MONSTER:
                foreach (var item in quest.monsterObjectiveList)
                {
                    if(fromPNJ)
                        quest.UpdateMonsterProgression();
                    questObjectiveTxt.text = LocalizationManager.instance.GetText("UI", "KILL_MONSTER_QUEST") + item.nb + " " + LocalizationManager.instance.GetText("MONSTER", item.monster.name) + "\n";

                    if (!fromPNJ)
                    {
                        foreach (var monsterKilled in StatsManager.instance.monsterKilled)
                        {
                            questObjectiveTxt.text += $"({monsterKilled.nb - item.nbMonsterKilledSinceAccepted}) \n";

                        }
                    }
                }
                break;
            case QuestCompletionCondition.RESOURCES:
                foreach (var item in quest.resourcesObjective)
                {
                    questObjectiveTxt.text = LocalizationManager.instance.GetText("UI", "GET_RESOURCES_QUEST") + item.nb + " " + LocalizationManager.instance.GetText("items", item.item.itemId + "_NAME") + "\n";

                    if (!fromPNJ)
                        questObjectiveTxt.text += $"({PlayerManager.instance.GetSpecialItem(item.item.GetID()).nb}) \n";
                }
                break;
            case QuestCompletionCondition.DISCOVERY:
                questObjectiveTxt.text = LocalizationManager.instance.GetText("UI", "FIND_LOCATION_QUEST") + LocalizationManager.instance.GetText("LOCATION", quest.sceneDataObjective.sceneID + "_SCENE");

                if (!fromPNJ)
                {
                    foreach (var item in StatsManager.instance.locationFound)
                    {
                        if (quest.location.sceneID == item)
                        {
                            questObjectiveTxt.text += $"({LocalizationManager.instance.GetText("UI", "DONE")}) \n";
                            break;
                        }
                    }
                }

                break;
            case QuestCompletionCondition.COMMUNICATION:
                questObjectiveTxt.text = LocalizationManager.instance.GetText("UI", "SPEAK_TO_PNJ_QUEST") + quest.pnjIDObjective;

                if (!fromPNJ)
                {
                    foreach (var item in StatsManager.instance.pnjSpoken)
                    {
                        if (quest.pnjIDSpoken == item)
                        {
                            questObjectiveTxt.text += $"({LocalizationManager.instance.GetText("UI", "DONE")}) \n";
                            break;
                        }
                    }
                }

                break;
            case QuestCompletionCondition.EVENT:
                questObjectiveTxt.text = "";

                if(!fromPNJ)
                {
                    bool eventDone;
                    SaveManager.instance.twoStateContainer.TryGetState(quest.eventObjective.ID, out eventDone);

                    if(eventDone)
                        questObjectiveTxt.text += $"({LocalizationManager.instance.GetText("UI", "DONE")}) \n";
                }
                break;
            default:
                break;
        }

        switch (quest.reward)
        {
            case QuestReward.NONE:
                questRewardSquareCoinsTxt.text = "";
                break;
            case QuestReward.SQUARE_COINS:
                questRewardSquareCoinsTxt.text = quest.nbSquareCoins + " " + LocalizationManager.instance.GetText("UI", "SQUARE_COINS");
                break;
            case QuestReward.ITEMS:
                foreach (var item in quest.rewardSpecialItem)
                {
                    GameObject rewardSlot = Instantiate(rewardSlotPrefab, rewardContainer.transform);
                    rewardSlot.GetComponent<RewardItemSlot>().CreateRewardSlot(item.rewardItem, equipementStats.GetComponent<EquipementStats>(), item.nb);
                }
                break;
            case QuestReward.EQUIPEMENT:
                List<Item> clonedEquipements = new List<Item>(); // Nouvelle liste pour stocker les copies

                foreach (var item in actualQuest.rewardEquipement)
                {
                    Item itemCopy = ScriptableObjectUtility.Clone(item);

                    itemCopy.GenerateID();

                    if (itemCopy is Weapon weapon)
                    {
                        weapon.GenerateStats();
                    }
                    else if (itemCopy is Boots boots)
                    {
                        boots.GenerateStats();
                    }
                    else if (itemCopy is Chestplate chestplate)
                    {
                        chestplate.GenerateStats();
                    }
                    else if (itemCopy is Leggings leggings)
                    {
                        leggings.GenerateStats();
                    }
                    else if (itemCopy is Helmet helmet)
                    {
                        helmet.GenerateStats();
                    }

                    // Ajouter la copie dans la nouvelle liste
                    clonedEquipements.Add(itemCopy);

                    // Créer l'interface avec la copie
                    GameObject rewardSlot = Instantiate(rewardSlotPrefab, rewardContainer.transform);
                    rewardSlot.GetComponent<RewardItemSlot>().CreateRewardSlot(itemCopy, equipementStats.GetComponent<EquipementStats>());
                }

                // Remplace la liste originale par les clones
                actualQuest.rewardEquipement = clonedEquipements;
                break;
            default:
                break;
        }


        foreach (var region in MeteoManager.instance.regions)
        {
            foreach (var scene in region.scenes)
            {
                if (scene.SceneName == SceneManager.GetActiveScene().name)
                {
                    questLocationTxt.text = LocalizationManager.instance.GetText("LOCATION", region.regionID + "_REGION") + " - " + LocalizationManager.instance.GetText("LOCATION", scene.sceneID + "_SCENE");
                    this.actualQuest.location = scene;
                }
            }
        }
    }

    public bool RequirementDone(Quests quest)
    {
        switch (quest.requirement)
        {
            case QuestRequirement.NONE:
                return true;
            case QuestRequirement.DISCOVERY:
                return StatsManager.instance.locationFound.Contains(quest.sceneData.sceneID);
            case QuestRequirement.COMMUNICATION:
                return StatsManager.instance.pnjSpoken.Contains(quest.pnjIDSpoken);
            case QuestRequirement.EVENT:
                bool eventGood = false;
                SaveManager.instance.twoStateContainer.TryGetState(quest.eventRequired.ID, out eventGood);
                return eventGood;
            case QuestRequirement.SPECIAL_OBJECT:
                foreach (var specialObject in SpecialObjectsManager.instance.availableObjects)
                {
                    if (specialObject.toolType == quest.specialObject)
                        return true;
                }
                return false;
            case QuestRequirement.MONSTER_KILLED:
                foreach (var monsterKilled in StatsManager.instance.monsterKilled)
                {
                    if (monsterKilled.idMonster == quest.monsterRequired.name && monsterKilled.nb == quest.nbMonsterRequired)
                        return true;
                }
                return false;
            case QuestRequirement.QUEST:
                foreach (var completedQuest in completedQuests)
                {
                    if (completedQuest.id == quest.questsRequired.id)  // CORRECTION ICI
                        return true;
                }
                return false;
            default:
                return false;
        }
    }

    public bool ObjectivesDone(Quests quest)
    {
        if (quest == null)
        {
            return false;
        }

        // Récupérer la quête en attente correspondante
        QuestContainer waitingQuest = waitingQuests.FirstOrDefault(q => q.id == quest.id);
        if (waitingQuest == null)
        {
            return false;  // Si la quête n'est pas en attente
        }

        // Cloner la quête pour comparaison
        Quests questCopy = ScriptableObjectUtility.Clone(quest);

        questCopy.pnjID = waitingQuest.pnjID;

        foreach (var region in MeteoManager.instance.regions)
        {
            foreach (var scene in region.scenes)
            {
                if (scene.SceneName == waitingQuest.idLocation)
                {
                    questCopy.location = scene;
                }
            }
        }

        if(questCopy.monsterObjectiveList.Count > 0 && waitingQuest.nbMonsterKilledSinceAccepted.Count > 0)
            for (int i = 0; i < questCopy.monsterObjectiveList.Count; i++)
            {
                questCopy.monsterObjectiveList[i].nbMonsterKilledSinceAccepted = waitingQuest.nbMonsterKilledSinceAccepted[i];
            }


        // Comparaison des objectifs
        switch (quest.completionCondition)
        {
            case QuestCompletionCondition.KILL_MONSTER:
                bool monstersDone = quest.monsterObjectiveList.All(obj => {
                    // Chercher le nombre de monstres tués avec l'id spécifique
                    MonsterKilled killedMonster = StatsManager.instance.monsterKilled
                        .FirstOrDefault(m => m.idMonster == obj.monster.name);

                    int monstersKilled = killedMonster.idMonster != null ? killedMonster.nb : 0;

                    bool isDone = monstersKilled - obj.nbMonsterKilledSinceAccepted >= obj.nb;
                    return isDone;
                });
                return monstersDone;

            case QuestCompletionCondition.RESOURCES:
                bool resourcesDone = quest.resourcesObjective.All(obj => {
                    bool isEnough = obj.nb <= PlayerManager.instance.GetSpecialItem(obj.item.GetID()).nb;
                    return isEnough;
                });
                return resourcesDone;

            case QuestCompletionCondition.DISCOVERY:
                bool discoveryDone = StatsManager.instance.locationFound.Contains(quest.sceneDataObjective.sceneID);
                return discoveryDone;

            case QuestCompletionCondition.COMMUNICATION:
                bool communicationDone = StatsManager.instance.pnjSpoken.Contains(quest.pnjIDObjective);
                return communicationDone;

            case QuestCompletionCondition.EVENT:
                bool eventDone;
                SaveManager.instance.twoStateContainer.TryGetState(quest.eventObjective.ID, out eventDone);
                return eventDone;

            default:
                return false;
        }
    }



    // Vérifie si une quête est dans la liste des quêtes en attente
    public bool IsInWaiting(Quests quest)
    {
        return quest != null && waitingQuests.Any(q => q.id == quest.id);
    }

    // Vérifie si une quête est dans la liste des quêtes terminées
    public bool IsInFinished(Quests quest)
    {
        return quest != null && completedQuests.Any(q => q.id == quest.id);
    }

}
