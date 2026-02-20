using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private string saveFilePath = "equipementSave.json";
    private string eventIDSaveFilePath = "eventIDSave.json";

    public static SaveManager instance;

    public string lastScene;

    public ItemDatabase itemDatabase;

    public TwoStateContainer twoStateContainer = new TwoStateContainer();

    private List<EquipementSlotData> equipementSlotData = new List<EquipementSlotData>();
    private List<EquipementSlotData> equippedSlotData = new List<EquipementSlotData>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            Debug.Log("Saving...");
            Save();
        }
        else if (Input.GetKeyUp(KeyCode.O))
        {
            Debug.Log("Loading...");
            Load();
        }
        else if (Input.GetKeyUp(KeyCode.Delete))
        {
            DeleteSave();
            Debug.Log("Save deleted !");
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
            File.Delete(saveFilePath);

        if (File.Exists(eventIDSaveFilePath))
            File.Delete(eventIDSaveFilePath);
    }

    public void SaveSpecialObjectsOnly()
    {
        if (SpecialObjectsManager.instance == null)
        {
            Debug.LogWarning("SpecialObjectManager not found, cannot save SpecialObjects.");
            return;
        }

        // Afficher le chemin du fichier de sauvegarde
        Debug.Log($"Save file path: {saveFilePath}");

        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("No main save file found! Cannot update SpecialObjects data.");
            return;
        }

        // Lire la sauvegarde actuelle
        string json = File.ReadAllText(saveFilePath);
        SaveData existingData = JsonUtility.FromJson<SaveData>(json);

        // Mettre à jour uniquement les SpecialObjects
        existingData.specialObjectsData.availableObjects = SpecialObjectsManager.instance.availableObjects;
        existingData.specialObjectsData.actualObject = SpecialObjectsManager.instance.actualObject;

        // Réécrire la sauvegarde complète avec les SpecialObjects mis à jour
        string updatedJson = JsonUtility.ToJson(existingData, true);
        File.WriteAllText(saveFilePath, updatedJson);

        Debug.Log("SpecialObjects data successfully updated in main save file!");
    }

    public void SaveDungeonsOnly()
    {
        if (DungeonManager.instance == null)
        {
            Debug.LogWarning("DungeonManager not found, cannot save dungeons.");
            return;
        }

        // Afficher le chemin du fichier de sauvegarde
        Debug.Log($"Save file path: {saveFilePath}");

        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("No main save file found! Cannot update dungeon data.");
            return;
        }

        // Lire la sauvegarde actuelle
        string json = File.ReadAllText(saveFilePath);
        SaveData existingData = JsonUtility.FromJson<SaveData>(json);

        // Mettre à jour uniquement les donjons
        existingData.dungeons = DungeonManager.instance.dungeonDB;

        // Réécrire la sauvegarde complète avec les donjons mis à jour
        string updatedJson = JsonUtility.ToJson(existingData, true);
        File.WriteAllText(saveFilePath, updatedJson);

        Debug.Log("Dungeon data successfully updated in main save file!");
    }




    public void Save()
    {
        SaveEquipementData();

        lastScene = SceneManager.GetActiveScene().name;

        List<string> unlockedAttacks = new List<string>();

        foreach (var attack in PlayerManager.instance.runtimeSpecialAttacks)
        {
            if (attack.isAvailable)
                unlockedAttacks.Add(attack.attackName);
        }

        string json = JsonUtility.ToJson(
            new SaveData(
                equipementSlotData,
                equippedSlotData,
                MineManager.instance.mineList,
                SceneManager.GetActiveScene().name,
                PlayerManager.instance.player.transform.position,
                PlayerManager.instance.player.GetComponent<Stats>().money,
                PlayerManager.instance.player.GetComponent<LifeManager>().life,
                new SoulData(
                    GameoverManager.instance.deathScene,
                    GameoverManager.instance.money,
                    GameoverManager.instance.isSoul,
                    GameoverManager.instance.deathPosition,
                    GameoverManager.instance.elapsedTime),
                new SpecialObjectsData(
                    SpecialObjectsManager.instance.availableObjects,
                    SpecialObjectsManager.instance.actualObject),
                GetAllSpecialItems(PlayerManager.instance.runtimeSpecialItems),
                MeteoManager.instance.timeWorld,
                MeteoManager.instance.isWeather,
                MeteoManager.instance.weatherDuration,
                new PlayerLevelData(PlayerLevels.instance.acquiredSkillsID,
                    PlayerLevels.instance.lvlHP,
                    PlayerLevels.instance.lvlSTR,
                    PlayerLevels.instance.lvlLuck),
                PlayerLevels.instance.explorerReputation,
                PlayerLevels.instance.armorerReputation,
                PlayerLevels.instance.healerReputation,
                new MarketData(MarketManager.instance.timerNewMarket),
                new StatsData(StatsManager.instance.monsterKilled, 
                    StatsManager.instance.locationFound, 
                    StatsManager.instance.pnjSpoken),
                new QuestContainerData(QuestManager.instance.waitingQuests),
                new QuestContainerData(QuestManager.instance.completedQuests),
                DungeonManager.instance.dungeonDB,
                unlockedAttacks,
                PlayerManager.instance.player.GetComponent<ObjectPerspective>().level,
                MonsterSpawnManager.instance.GetAllSpawnerStates(),
                TeleportationManager.instance.teleportationsAvailable,
                MapManager.instance.targetCoords));

        File.WriteAllText(saveFilePath, json);


        // Ajouter tous les identifiants temporaires aux identifiants permanents avant de sauvegarder
        foreach (var tempId in twoStateContainer.GetTemporaryIDs())
        {
            if (twoStateContainer.TryGetState(tempId, out bool state))
            {
                twoStateContainer.AddOrUpdatePermanentState(tempId, state);
            }
        }

        twoStateContainer.Save(eventIDSaveFilePath);
        Debug.Log("Save successful!");
    }

    public bool CheckIfSave()
    {
        return File.Exists(saveFilePath);
    }

    public void Load(bool equipement = true, bool eventID = true, bool soulData = true, bool specialObjects = true, bool mines = true, bool meteo = true, bool market = true, bool stats = true, bool quests = true, bool dungeon = true, bool specialAttacks = true, bool spawnerState = true, bool teleportationAvailable = true, bool targetCoords = true)
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            // A VOIR SI C'EST PAS CONTRAIGNANT POUR LE JOUEUR, GENRE IL ACHETE DIRECT DES LVL SANS SAUVEGARDER
            PlayerLevels.instance.acquiredSkillsID = saveData.playerLevelData.acquiredSkills;
            PlayerLevels.instance.lvlHP = saveData.playerLevelData.lvlHP;
            PlayerLevels.instance.lvlSTR = saveData.playerLevelData.lvlSTR;
            PlayerLevels.instance.lvlLuck = saveData.playerLevelData.lvlLUCK;
            PlayerLevels.instance.explorerReputation = saveData.explorerReputation;
            PlayerLevels.instance.armorerReputation = saveData.armorerReputation;
            PlayerLevels.instance.healerReputation = saveData.healerReputation;
            PlayerManager.instance.player.GetComponent<LifeManager>().life = saveData.playerLife;

            // NE PAS CHANGER LA POSITION ICI - sera fait après le changement de scène
            // PlayerManager.instance.player.transform.position = saveData.position;

            PlayerManager.instance.player.GetComponent<Stats>().money = saveData.money;
            PlayerManager.instance.runtimeSpecialItems = GetSpecialItemsFromSave(saveData.specialItems);
            PlayerManager.instance.ReassignItemsSprites();

            PlayerManager.instance.player.GetComponent<ObjectPerspective>().level = saveData.playerLevelPerspective;

            if (targetCoords)
            {
                MapManager.instance.targetCoords = saveData.targetCoords;
            }

            if (teleportationAvailable)
            {
                TeleportationManager.instance.teleportationsAvailable = saveData.teleportationsAvailable;
            }

            if (spawnerState)
            {
                MonsterSpawnManager.instance.LoadSpawnerStates(saveData.spawnerStates);
            }

            if (specialAttacks)
            {
                foreach (var attack in PlayerManager.instance.runtimeSpecialAttacks)
                {
                    attack.isAvailable = saveData.unlockedSpecialAttacks.Contains(attack.attackName);
                }
            }

            if (market)
            {
                MarketManager.instance.timerNewMarket = saveData.marketData.timerNewMarket;
            }

            if (equipement)
            {
                equipementSlotData = saveData.equipementSlotData;
                equippedSlotData = saveData.equippedSlotData;
                LoadEquipementData(equipementSlotData, Equipement.instance.equipementSlots);
                LoadEquipementData(equippedSlotData, Equipement.instance.equippedSlots);
            }

            if (mines)
                MineManager.instance.mineList = saveData.mines;

            if (eventID)
                twoStateContainer.Load(eventIDSaveFilePath);

            if (soulData)
            {
                GameoverManager.instance.isSoul = saveData.soulData.isSoul;
                GameoverManager.instance.deathPosition = saveData.soulData.position;
                GameoverManager.instance.money = saveData.soulData.money;
                GameoverManager.instance.deathScene = saveData.soulData.deathScene;
                GameoverManager.instance.elapsedTime = saveData.soulData.elapsedTime;

                GameoverManager.instance.RestartSoulMoneyReductionRoutine();
            }

            if (specialObjects)
            {
                SpecialObjectsManager.instance.actualObject = saveData.specialObjectsData.actualObject;
                SpecialObjectsManager.instance.availableObjects = saveData.specialObjectsData.availableObjects;
            }

            if (meteo)
            {
                MeteoManager.instance.timeWorld = saveData.worldTime;
                MeteoManager.instance.isWeather = saveData.isWeather;
                MeteoManager.instance.weatherDuration = saveData.weatherDuration;

                MeteoManager.instance.LoadWeather();
            }

            if (stats)
            {
                StatsManager.instance.monsterKilled = saveData.statsData.monsterKilled;
                StatsManager.instance.pnjSpoken = saveData.statsData.pnjSpoken;
                StatsManager.instance.locationFound = saveData.statsData.locationFound;
            }

            if (quests)
            {
                foreach (var item in saveData.waitingQuests.questContainer)
                {
                    QuestManager.instance.waitingQuests.Add(item);
                }

                foreach (var item in saveData.finishedQuests.questContainer)
                {
                    QuestManager.instance.completedQuests.Add(item);
                }
            }

            if (dungeon)
            {
                DungeonManager.instance.dungeonDB = saveData.dungeons;

                // Si l'utilisateur est dans un donjon, retrouvez le donjon actuel
                if (MeteoManager.instance.actualScene.sceneType == SceneType.DUNGEON)
                {
                    DungeonManager.instance.actualDungeon = DungeonManager.instance.GetDungeon(MeteoManager.instance.actualScene.dungeonName);
                    DungeonManager.instance.isInDungeon = true;
                    DungeonManager.instance.uiDungeon.SetActive(true);
                }
                else
                {
                    DungeonManager.instance.actualDungeon = null;
                    DungeonManager.instance.isInDungeon = false;
                    DungeonManager.instance.uiDungeon.SetActive(false);
                }

                // Mise à jour de l'interface utilisateur
                DungeonManager.instance.UpdateUI();
            }

            // Changer la scène ET la position en même temps
            ScenesManager.instance.ChangeSceneObject(saveData.sceneName, saveData.position, 1);
            Debug.Log("Load successful!");
        }
        else
        {
            Debug.LogWarning("No save file found!");
        }
    }

    public List<SpecialItems> GetSpecialItemsFromSave(List<SaveSpecialItem> saveSpecialItems)
    {
        List<SpecialItems> specialItems = new List<SpecialItems>();

        foreach (var saveItem in saveSpecialItems)
        {
            // Crée une nouvelle instance de SpecialItems pour chaque SaveSpecialItem
            SpecialItems newItem = ScriptableObject.CreateInstance<SpecialItems>();
            newItem.type = saveItem.type;
            newItem.nb = saveItem.nb;
            newItem.name = saveItem.name;
            newItem.itemId = saveItem.id;

            specialItems.Add(newItem);
        }

        return specialItems;
    }

    public List<Item> GetSpecialItemsFromSaveItem(List<SaveSpecialItem> saveSpecialItems)
    {
        List<Item> specialItems = new List<Item>();

        foreach (var saveItem in saveSpecialItems)
        {
            // Crée une nouvelle instance de SpecialItems pour chaque SaveSpecialItem
            SpecialItems newItem = ScriptableObjectUtility.Clone((SpecialItems)PlayerManager.instance.database.GetItemByID(saveItem.id));
            newItem.itemId = saveItem.id;
            newItem.type = saveItem.type;
            newItem.nb = saveItem.nb;
            newItem.name = saveItem.name;

            specialItems.Add(newItem);
        }

        return specialItems;
    }


    public List<SaveSpecialItem> GetAllSpecialItems(List<SpecialItems> specialItem)
    {
        List<SaveSpecialItem> temp = new List<SaveSpecialItem>();

        foreach (var item in specialItem)
        {
            temp.Add(new SaveSpecialItem(item.type, item.nb, item.itemName, item.itemId));
        }

        return temp;
    }

    private void LoadEquipementData(List<EquipementSlotData> slotDataList, List<GameObject> slotList)
    {
        foreach (EquipementSlotData slotData in slotDataList)
        {
            Item originalItem = itemDatabase.GetItemByID(slotData.itemID);
            if (originalItem != null)
            {
                Item itemInstance = ScriptableObjectUtility.Clone(originalItem);

                itemInstance.itemId = slotData.itemID;

                if (itemInstance is Helmet)
                    (itemInstance as Helmet).GenerateStats();
                else if (itemInstance is Chestplate)
                    (itemInstance as Chestplate).GenerateStats();
                else if (itemInstance is Leggings)
                    (itemInstance as Leggings).GenerateStats();
                else if (itemInstance is Boots)
                    (itemInstance as Boots).GenerateStats();
                else if (itemInstance is Weapon)
                    (itemInstance as Weapon).GenerateStats();


                EquipementSlot slot = slotList[slotData.index].GetComponent<EquipementSlot>();
                slot.AddItem(itemInstance);
            }
            else
            {
                Debug.LogWarning("Item with ID " + slotData.itemID + " not found in database!");
            }
        }
    }

    public void SaveEquipementData()
    {
        equipementSlotData.Clear();
        equippedSlotData.Clear();

        List<GameObject> equipementSlots = Equipement.instance.equipementSlots;
        List<GameObject> equippedSlots = Equipement.instance.equippedSlots;

        for (int i = 0; i < equipementSlots.Count; i++)
        {
            EquipementSlot equipementSlot = equipementSlots[i].GetComponent<EquipementSlot>();
            if (equipementSlot.actualItem != null)
            {
                equipementSlotData.Add(new EquipementSlotData(false, i, equipementSlot.actualItem.itemId));
            }
        }

        for (int i = 0; i < equippedSlots.Count; i++)
        {
            EquipementSlot equippedSlot = equippedSlots[i].GetComponent<EquipementSlot>();
            if (equippedSlot.actualItem != null)
            {
                equippedSlotData.Add(new EquipementSlotData(true, i, equippedSlot.actualItem.itemId));
            }
        }
    }
}

[System.Serializable]
public struct QuestContainerData
{
    public List<QuestContainer> questContainer;

    public QuestContainerData(List<QuestContainer> questContainer)
    {
        this.questContainer = questContainer;
    }


}

[System.Serializable]
public struct StatsData
{
    public List<MonsterKilled> monsterKilled;
    public List<string> locationFound;
    public List<string> pnjSpoken;

    public StatsData(List<MonsterKilled> monsterKilled , List<string> locationFound, List<string> pnjSpoken)
    {
        this.monsterKilled = monsterKilled;
        this.locationFound = locationFound;
        this.pnjSpoken = pnjSpoken;     
    }

}

[System.Serializable]
public struct SaveSpecialItem
{
    public string id;
    public string name;
    public SpecialItemType type;
    public int nb;

    public SaveSpecialItem(SpecialItemType type, int nb, string name, string id)
    {
        this.name = name;
        this.type = type; // Corrigé : assigne le paramètre 'type' à 'this.type'
        this.nb = nb;     // Assigne le paramètre 'nb' à 'this.nb'
        this.id = id;
    }

}


[System.Serializable]
public struct PlayerLevelData
{
    public List<string> acquiredSkills;
    public int lvlHP;
    public int lvlSTR;
    public int lvlLUCK;

    public PlayerLevelData(List<string> acquiredSkills, int lvlHP, int lvlSTR, int lvlLUCK)
    {
        this.acquiredSkills = acquiredSkills;
        this.lvlHP = lvlHP;
        this.lvlSTR = lvlSTR;
        this.lvlLUCK = lvlLUCK;
    }
}
[System.Serializable]
public struct SpecialObjectsData
{
    public List<AvailableObjects> availableObjects;
    public AvailableObjects actualObject;

    public SpecialObjectsData(List<AvailableObjects> availableObjects, AvailableObjects actualObject)
    {
        this.availableObjects = availableObjects;
        this.actualObject = actualObject;
    }
}

[System.Serializable]
public struct SaveData
{
    public List<EquipementSlotData> equipementSlotData;
    public List<EquipementSlotData> equippedSlotData;
    public List<Mine> mines;
    public SpecialObjectsData specialObjectsData;
    public string sceneName;
    public int money;
    public int playerLife;
    public Vector2 position;
    public SoulData soulData;
    public List<SaveSpecialItem> specialItems;

    public float worldTime;
    public bool isWeather;
    public float weatherDuration;
    public PlayerLevelData playerLevelData;

    public float explorerReputation;
    public float armorerReputation;
    public float healerReputation;

    public MarketData marketData;

    public StatsData statsData;

    public QuestContainerData waitingQuests;
    public QuestContainerData finishedQuests;

    public List<string> unlockedSpecialAttacks;

    public List<Dungeon> dungeons;

    public int playerLevelPerspective;

    public List<SpawnerState> spawnerStates ;

    public List<TeleportationAvailable> teleportationsAvailable;

    public IntPair? targetCoords;

    public SaveData(
        List<EquipementSlotData> equipementSlotData,
        List<EquipementSlotData> equippedSlotData,
        List<Mine> mines,
        string sceneName,
        Vector2 position,
        int money,
        int playerLife,
        SoulData soulData,
        SpecialObjectsData specialObjectsData,
        List<SaveSpecialItem> specialItems,
        float worldTime,
        bool isWeather,
        float weatherDuration,
        PlayerLevelData playerLevelData,
        float explorerReputation,
        float armorerReputation,
        float healerReputation,
        MarketData marketData,
        StatsData statsData,
        QuestContainerData waitingQuests,
        QuestContainerData finishedQuests,
        List<Dungeon> dungeons,
        List<string> unlockedSpecialAttacks,
        int playerLevelPerspective,
        List<SpawnerState> spawnerState,
        List<TeleportationAvailable> teleportationsAvailable,
        IntPair? targetCoords)
        
    {
        this.equipementSlotData = equipementSlotData;
        this.equippedSlotData = equippedSlotData;
        this.mines = mines;
        this.sceneName = sceneName;
        this.position = position;
        this.soulData = soulData;
        this.money = money;
        this.playerLife = playerLife;
        this.specialObjectsData = specialObjectsData;
        this.specialItems = specialItems;
        this.worldTime = worldTime;
        this.isWeather = isWeather;
        this.weatherDuration = weatherDuration;
        this.playerLevelData = playerLevelData;
        this.explorerReputation = explorerReputation;
        this.healerReputation = healerReputation;
        this.armorerReputation = armorerReputation;
        this.marketData = marketData;
        this.statsData = statsData;
        this.waitingQuests = waitingQuests;
        this.finishedQuests = finishedQuests;
        this.dungeons = dungeons;
        this.unlockedSpecialAttacks = unlockedSpecialAttacks;
        this.playerLevelPerspective = playerLevelPerspective;
        this.spawnerStates = spawnerState;
        this.teleportationsAvailable = teleportationsAvailable;
        this.targetCoords = targetCoords;
    }
}

[System.Serializable]
public struct ItemData
{
    public string id;

    public ItemData(Item item)
    {
        this.id = item.itemId;
    }

    public Item GetItem()
    {
        Item clonedItem = ScriptableObjectUtility.Clone(PlayerManager.instance.database.GetItemByID(this.id));
        clonedItem.itemId = this.id;

        if (clonedItem is Weapon)
            (clonedItem as Weapon).GenerateStats();
        else if (clonedItem is Helmet)
            (clonedItem as Helmet).GenerateStats();
        else if (clonedItem is Chestplate)
            (clonedItem as Chestplate).GenerateStats();
        else if (clonedItem is Leggings)
            (clonedItem as Leggings).GenerateStats();
        else if (clonedItem is Boots)
            (clonedItem as Boots).GenerateStats();

        return clonedItem;
    }

    public static List<Item> ConvertItemDataListToItemList(List<ItemData> itemDataList)
    {
        List<Item> items = new List<Item>();
        foreach (var itemData in itemDataList)
        {
            Item item = itemData.GetItem();
            if (item != null)
            {
                items.Add(item);
            }
        }
        return items;
    }
}



[System.Serializable]
public struct MarketData
{
    public int timerNewMarket;

    public MarketData(int timerNewMarket)
    {
        this.timerNewMarket = timerNewMarket;

    }
}

[System.Serializable]
public struct MonsterSpawnerData
{
    public string id;
    public float spawnChanceEverySeconds;
    public int maxMonsters;
    public List<bool> hasMonsterAtIndex;
    public float timeSinceLastSpawn;

    public MonsterSpawnerData(string id, float spawnChanceEverySeconds, int maxMonsters, List<bool> hasMonsterAtIndex, float timeSinceLastSpawn)
    {
        this.id = id;
        this.spawnChanceEverySeconds = spawnChanceEverySeconds;
        this.maxMonsters = maxMonsters;
        this.hasMonsterAtIndex = hasMonsterAtIndex;
        this.timeSinceLastSpawn = timeSinceLastSpawn;
    }
}



[System.Serializable]
public struct SoulData
{
    public string deathScene;
    public int money;
    public bool isSoul;
    public Vector2 position;
    public float elapsedTime;

    public SoulData(string deathScene, int money, bool isSoul, Vector2 position, float elapsedTime)
    {
        this.isSoul = isSoul;
        this.money = money;
        this.position = position;
        this.deathScene = deathScene;
        this.elapsedTime = elapsedTime;
    }
}

[System.Serializable]
public struct EquipementSlotData
{
    public bool isEquipping;
    public int index;
    public string itemID;

    public EquipementSlotData(bool isEquipping, int index, string itemID)
    {
        this.isEquipping = isEquipping;
        this.index = index;
        this.itemID = itemID;
    }
}

public class TwoStateContainer
{
    private Dictionary<string, bool> permanentStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> temporaryStates = new Dictionary<string, bool>();

    public void AddOrUpdatePermanentState(string id, bool state)
    {
        if (permanentStates.ContainsKey(id))
        {
            permanentStates[id] = state;
        }
        else
        {
            permanentStates.Add(id, state);
        }
    }

    public void AddOrUpdateTemporaryState(string id, bool state)
    {
        if (temporaryStates.ContainsKey(id))
        {
            temporaryStates[id] = state;
        }
        else
        {
            temporaryStates.Add(id, state);
        }
    }

    public void RemoveState(string id)
    {
        permanentStates.Remove(id);
        temporaryStates.Remove(id);
    }

    public bool TryGetState(string id, out bool state)
    {
        if (temporaryStates.TryGetValue(id, out state) ||  permanentStates.TryGetValue(id, out state))
        {
            return true;
        }
        state = false;
        return false;
    }

    public void Save(string filePath)
    {
        // Enregistrer les états permanents
        List<StateData> stateList = new List<StateData>();

        foreach (var kvp in permanentStates)
        {
            stateList.Add(new StateData(kvp.Key, kvp.Value));
        }

        string json = JsonUtility.ToJson(new StateSaveData(stateList));
        File.WriteAllText(filePath, json);

        // Supprimer tous les états temporaires après la sauvegarde
        temporaryStates.Clear();
    }

    public void Load(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            StateSaveData data = JsonUtility.FromJson<StateSaveData>(json);

            permanentStates.Clear();
            foreach (var stateData in data.states)
            {
                permanentStates[stateData.id] = stateData.state;
            }
        }
    }

    public IEnumerable<string> GetTemporaryIDs()
    {
        return temporaryStates.Keys;
    }

    [System.Serializable]
    private class StateSaveData
    {
        public List<StateData> states;

        public StateSaveData(List<StateData> states)
        {
            this.states = states;
        }
    }

    [System.Serializable]
    private class StateData
    {
        public string id;
        public bool state;

        public StateData(string id, bool state)
        {
            this.id = id;
            this.state = state;
        }
    }
}
