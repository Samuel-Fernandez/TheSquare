using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class TeleportationAvailable
{
    public string sceneName;
    public float positionX;
    public float positionY;

    public TeleportationAvailable(string sceneName, float positionX, float positionY)
    {
        this.sceneName = sceneName;
        this.positionX = positionX;
        this.positionY = positionY;
    }

    public bool Equals(string sceneName, float posX, float posY)
    {
        return this.sceneName == sceneName &&
               Mathf.Approximately(this.positionX, posX) &&
               Mathf.Approximately(this.positionY, posY);
    }
}

public class TeleportationManager : MonoBehaviour
{
    // Ajouter ici dans le SaveManager
    public List<TeleportationAvailable> teleportationsAvailable;

    public TextMeshProUGUI title;
    public TextMeshProUGUI noTeleportationTxt;
    public GameObject teleportationUI;
    public GameObject scrollbarContainer;

    public GameObject buttonPrefab;

    List<GameObject> buttons;
    float defaultFixedDeltaTime;


    public static TeleportationManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            buttons = new List<GameObject>();
            teleportationsAvailable = new List<TeleportationAvailable>();
            defaultFixedDeltaTime = Time.fixedDeltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CheckTeleporter(string sceneName, float positionX, float positionY)
    {
        Debug.Log($"CHECK TELEPORTATION - Paramètres reçus : sceneName='{sceneName}', positionX={positionX}, positionY={positionY}");

        if (teleportationsAvailable == null)
        {
            Debug.Log("CHECK TELEPORTATION - teleportationsAvailable est null !");
            return false;
        }

        foreach (var teleporter in teleportationsAvailable)
        {
            Debug.Log($"CHECK TELEPORTATION - Test du téléporteur : sceneName='{teleporter.sceneName}', positionX={teleporter.positionX}, positionY={teleporter.positionY}");

            if (teleporter.Equals(sceneName, positionX, positionY))
            {
                Debug.Log("CHECK TELEPORTATION - Correspondance trouvée !");
                return true;
            }
        }

        Debug.Log("CHECK TELEPORTATION - Aucun téléporteur correspondant trouvé.");
        return false;
    }



    public void AddTeleporter(string sceneName, float positionX, float positionY)
    {
        if (!CheckTeleporter(sceneName, positionX, positionY))
        {
            teleportationsAvailable.Add(new TeleportationAvailable(sceneName, positionX, positionY));
        }
    }


    public void ToggleTeleportationUI()
    {

        title.text = LocalizationManager.instance.GetText("UI", "TELEPORTATION_MANAGER_TITLE");
        teleportationUI.SetActive(!teleportationUI.activeSelf);

        InventoryManager.instance.canOpenInventory = !teleportationUI.activeSelf;
        QuestManager.instance.canOpenQuests = !teleportationUI.activeSelf;

        if (teleportationUI.activeSelf)
        {
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Assure que fixedDeltaTime est en pause

            UpdateUI();

        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = defaultFixedDeltaTime; // Réinitialise fixedDeltaTime
        }
    }

    public void UpdateUI()
    {
        RemoveAllButtons();
        AddAllButtons();

        if(teleportationsAvailable.Count <= 1)
        {
            noTeleportationTxt.gameObject.SetActive(true);
            noTeleportationTxt.text = LocalizationManager.instance.GetText("UI", "NO_TELEPORTATION");
        }
        else
        {
            noTeleportationTxt.gameObject.SetActive(false);

        }
    }

    void RemoveAllButtons()
    {
        if(buttons.Count > 0)
        {
            foreach (var button in buttons)
            {
                Destroy(button);
            }

            buttons.Clear();
        }
        
    }

    void AddAllButtons()
    {
        GameObject firstButton = null;
        string currentSceneID = MeteoManager.instance.actualScene.sceneID;

        foreach (var teleportation in teleportationsAvailable)
        {
            if (teleportation.sceneName == currentSceneID)
                continue; // Ne pas ajouter de bouton pour la scène actuelle

            GameObject buttonInstance = Instantiate(buttonPrefab, scrollbarContainer.transform);
            buttonInstance.GetComponent<TeleportationSelector>().Init(teleportation);
            buttons.Add(buttonInstance);

            if (firstButton == null)
                firstButton = buttonInstance;
        }

        // Sélection automatique du premier bouton
        if (firstButton != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstButton);
        }
    }



    public void Teleport(TeleportationAvailable teleportationAvailable)
    {
        ToggleTeleportationUI();
        GetComponent<SoundContainer>().PlaySound("Teleporter", 1);
        ScenesManager.instance.ChangeSceneObject(MeteoManager.instance.GetSceneDataByID(teleportationAvailable.sceneName).SceneName, new Vector2(teleportationAvailable.positionX, teleportationAvailable.positionY));
    }
}
