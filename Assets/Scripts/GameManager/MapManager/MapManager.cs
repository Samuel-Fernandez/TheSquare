using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public List<MapPiece> map;
    public GameObject mapSlotPrefab;
    public Transform mapSlotParent;
    public TextMeshProUGUI sceneTitle;

    public static MapManager instance;

    private List<GameObject> mapSlots = new List<GameObject>();

    private const int GRID_WIDTH = 24;
    private const int GRID_HEIGHT = 10;

    public IntPair? targetCoords;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void SetTargetCoords(int x, int y)
    {
        IntPair coords = new IntPair(x, y);

        targetCoords = coords;
    }

    public void RemoveTargetCoords()
    {
        targetCoords = null;
    }

    public void OpenMap()
    {
        ResetUI();

        // Dictionnaire (coord -> MapPiece)
        Dictionary<(int, int), MapPiece> mapDict = new Dictionary<(int, int), MapPiece>();
        foreach (var piece in map)
        {
            mapDict[(piece.coords.x, piece.coords.y)] = piece;
        }

        // Boucle sur une grille 16×16
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                GameObject mapSlotInstance = Instantiate(mapSlotPrefab, mapSlotParent);
                MapSlot slot = mapSlotInstance.GetComponent<MapSlot>();
                slot.coords = new IntPair(x, y);

                mapSlotInstance.GetComponent<Button>().onClick.AddListener(() =>
                {
                    MapSlot slot = mapSlotInstance.GetComponent<MapSlot>();
                    MapSlotClick(slot);
                });



                if (mapDict.TryGetValue((x, y), out MapPiece piece))
                {
                    // Vérifier si découvert
                    bool discovered = false;
                    foreach (string scene in piece.scenes)
                    {
                        if (StatsManager.instance.locationFound.Contains(GetSceneID(scene)))
                        {
                            discovered = true;
                            break;
                        }
                    }

                    bool isHere = false;
                    foreach (string scene in piece.scenes)
                    {
                        if (GetSceneID(scene) == MeteoManager.instance.actualScene.sceneID)
                        {
                            isHere = true;
                            break;
                        }
                    }

                    slot.localizatorSprite.SetActive(isHere);

                    if (targetCoords != null && targetCoords.Value.x == piece.coords.x && targetCoords.Value.y == piece.coords.y)
                    {
                        slot.targetSprite.SetActive(true);
                    }
                    else
                    {
                        slot.targetSprite.SetActive(false);
                    }


                    if (discovered)
                    {
                        slot.sceneName = piece.scenes;
                        slot.GetComponent<Image>().sprite = piece.icon;
                        slot.GetComponent<Image>().color = Color.white;
                    }
                    else
                    {
                        slot.sceneName = new List<string> { "?" };
                        slot.GetComponent<Image>().sprite = piece.icon;
                        slot.GetComponent<Image>().color = Color.black;
                    }
                }
                else
                {
                    // Slot vide
                    slot.sceneName = new List<string>();
                    Color c = slot.GetComponent<Image>().color;
                    c.a = 0f; // opacité minimale
                    slot.GetComponent<Image>().color = c;
                }

                mapSlots.Add(mapSlotInstance);

                // Placement manuel : chaque slot en fonction de x/y
                RectTransform rt = mapSlotInstance.GetComponent<RectTransform>();
                float size = rt.sizeDelta.x; // suppose carré
                rt.anchoredPosition = new Vector2(x * size, y * size);
            }
        }
    }

    public void MapSlotClick(MapSlot slot)
    {
        if (slot.sceneName.Count > 0 && slot.sceneName[0] != "?")
        {
            sceneTitle.text = LocalizationManager.instance.GetText(
                "LOCATION",
                GetSceneID(slot.sceneName[0]) + "_SCENE"
            );
        }
        else
        {
            sceneTitle.text = "";
        }
    }

    public string GetSceneID(string sceneName)
    {
        return MeteoManager.instance.GetSceneData(sceneName).sceneID;
    }

    void ResetUI()
    {
        foreach (var slot in mapSlots)
        {
            Destroy(slot);
        }
        mapSlots.Clear();
    }
}
