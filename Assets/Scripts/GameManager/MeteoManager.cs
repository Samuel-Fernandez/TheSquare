using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

enum WeatherType
{
    RAINING,
    BLIZZARD,
    DUST_STORM,
}

public enum DayHour
{
    H0, H1, H2, H3, H4, H5, H6, H7,
    H8, H9, H10, H11, H12, H13, H14, H15,
    H16, H17, H18, H19, H20, H21, H22, H23
}


public class MeteoManager : MonoBehaviour
{
    public static MeteoManager instance;

    public List<RegionData> regions;

    public float timeWorld;
    public float weatherDuration;
    public bool isWeather;


    const int lengthOneDay = 30; // 30 min
    const float maxSunIntensity = .75f;
    const float minSunIntensity = 0.05f;

    const float caveIntensity = .1f;
    const float houseIntensity = .3f;
    float dungeonIntensity = .5f;

    public SceneData actualScene;
    RegionData actualRegion;
    WeatherType actualWeatherType;

    public GameObject dustStorm;
    GameObject dustStormInstance = null;

    public GameObject rain;
    GameObject rainInstance = null;
    const float chanceWeatherPerSecond = .005f;

    public List<GameObject> clouds;
    public GameObject cloudPrefab;

    public bool time = true;

    public void SetTime(bool time)
    {
        this.time = time;
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadWeather();
    }

    public DayHour GetCurrentDayHour()
    {
        float dayCycleDuration = lengthOneDay * 60f;
        float timeInCurrentDay = timeWorld % dayCycleDuration;
        float initialTimeInSeconds = 8 * 60 * 60;
        float totalSecondsInDay = initialTimeInSeconds + timeInCurrentDay * (24 * 60 * 60) / dayCycleDuration;

        int hours = Mathf.FloorToInt(totalSecondsInDay / 3600) % 24;

        return (DayHour)hours;
    }

    public SceneData GetSceneDataByID(string sceneID)
    {
        foreach (RegionData region in regions)
        {
            foreach (SceneData scene in region.scenes)
            {
                if (scene.sceneID == sceneID)
                {
                    return scene;
                }
            }
        }

        Debug.LogWarning($"No scene found with the ID: {sceneID}");
        return null;
    }

    public SceneData GetSceneData(string sceneName)
    {
        foreach (RegionData region in regions)
        {
            // Parcours de toutes les scènes de chaque région
            foreach (SceneData scene in region.scenes)
            {
                // Vérifie si le nom de la scène correspond
                if (scene.SceneName == sceneName)
                {
                    // Si la scène est trouvée, retourne ses données
                    return scene;
                }
            }
        }

        // Si aucune scène avec ce nom n'est trouvée, affiche un message et retourne null
        Debug.LogWarning($"No scene found with the name: {sceneName}");
        return null;
    }


    public RegionData GetRegionByScene(string sceneID)
    {
        foreach (var region in regions)
        {
            foreach (var scene in region.scenes)
            {
                if (scene.SceneName == sceneID)
                {
                    return region;
                }
            }
        }

        Debug.LogWarning($"No region found containing scene: {sceneID}");
        return null;
    }


    public void LoadWeather()
    {
        StopAllCoroutines();

        UpdateActualScene(SceneManager.GetActiveScene());
        StartCoroutine(RoutineTimeWorld());
        StartCoroutine(RoutineIsWeather());

        if (isWeather)
            StartCoroutine(StopWeather(weatherDuration));
    }

    public void SetSceneFilter()
    {
        CameraManager.instance.SetFilter(actualScene.filter);
    }

    public void UpdateActualScene(Scene regionScene)
    {
        foreach (RegionData region in regions)
        {
            foreach (SceneData scene in region.scenes)
            {
                if (scene.SceneName == regionScene.name)
                {
                    actualScene = scene;
                    actualRegion = region;
                    SoundManager.instance.PlayMusic(actualScene.music);
                    SetSceneFilter();
                    if(actualScene.playerCantOpenInventory)
                    {
                        InventoryManager.instance.canOpenInventory = false;
                        QuestManager.instance.canOpenQuests = false;
                    }
                    else
                    {
                        InventoryManager.instance.canOpenInventory = true;
                        QuestManager.instance.canOpenQuests = true;
                    }

                    PnjScheduleManager.instance.UpdateSchedules();
                    break;
                }
            }
        }

        if(actualRegion.type == RegionType.CINEMATIC)
        {
            GameObject.Find("Cinematic").GetComponent<EventPlayer>().PlayAnimation();

        }

        if (actualScene)
            switch (actualScene.sceneType)
            {
                case SceneType.OUTSIDE:
                    break;
                case SceneType.CAVE:
                    if (actualScene.sunIntensity <= 0)
                        LightManager.instance.SetSunIntensity(caveIntensity);
                    else
                        LightManager.instance.SetSunIntensity(actualScene.sunIntensity);
                    break;
                case SceneType.HOUSE:
                    if (actualScene.sunIntensity <= 0)
                        LightManager.instance.SetSunIntensity(houseIntensity);
                    else
                        LightManager.instance.SetSunIntensity(actualScene.sunIntensity);
                    break;
                case SceneType.DUNGEON:
                    if (actualScene.sunIntensity <= 0)
                        LightManager.instance.SetSunIntensity(dungeonIntensity);
                    else
                        LightManager.instance.SetSunIntensity(actualScene.sunIntensity);
                    break;
                default:
                    break;
            }



        switch (actualRegion.type)
        {
            case RegionType.NONE:
                break;
            case RegionType.FOREST:
                actualWeatherType = WeatherType.RAINING;
                break;
            case RegionType.DESERT:
                actualWeatherType = WeatherType.DUST_STORM;
                break;
            default:
                break;
        }



    }

    public string ConvertTimeWorldToDayTime()
    {
        // Durée d'une journée complète en secondes (30 minutes de jeu)
        float dayCycleDuration = lengthOneDay * 60f;

        // Nombre de jours écoulés
        int days = Mathf.FloorToInt(timeWorld / dayCycleDuration);

        // Temps restant dans la journée actuelle en secondes
        float timeInCurrentDay = timeWorld % dayCycleDuration;

        // Calcul du temps de départ en secondes (8h00)
        float initialTimeInSeconds = 8 * 60 * 60;

        // Temps total depuis le début de la journée actuelle (en secondes)
        float totalSecondsInDay = initialTimeInSeconds + timeInCurrentDay * (24 * 60 * 60) / dayCycleDuration;

        // Conversion en heures et minutes
        int hours = Mathf.FloorToInt(totalSecondsInDay / 3600) % 24;
        int minutes = Mathf.FloorToInt((totalSecondsInDay % 3600) / 60);

        // Formater le temps
        return $"Jour {days + 1}, {hours}h{minutes:D2}";
    }

    IEnumerator RoutineIsWeather()
    {
        while (true)
        {
            // Gestion des nuages
            if (!isWeather && Random.Range(0, 100) >= 30 && actualScene.sceneType == SceneType.OUTSIDE)
            {
                GameObject newCloud = Instantiate(
                    cloudPrefab,
                    new Vector3(
                        Random.Range(PlayerManager.instance.player.transform.position.x - 10, PlayerManager.instance.player.transform.position.x + 10),
                        Random.Range(PlayerManager.instance.player.transform.position.y - 10, PlayerManager.instance.player.transform.position.y + 10),
                        0),
                    Quaternion.identity);

                newCloud.GetComponent<CloudBehiavour>().CreateCloud();
                clouds.Add(newCloud);
            }


            // Activation evenement
            if (!isWeather && UnityEngine.Random.value < chanceWeatherPerSecond)
            {
                isWeather = true;

                // Durée aléatoire de la pluie entre 1 et 5 minutes
                weatherDuration = UnityEngine.Random.Range(60f, 300f);
                StartCoroutine(StopWeather(weatherDuration));
            }

            // Vérifier si les effets de pluie doivent être actifs ou non
            if (isWeather)
            {
                // Gère type de weather suivant région
                switch (actualWeatherType)
                {
                    case WeatherType.RAINING:
                        if(CameraManager.instance.GetFilter() == CameraFilter.NONE)
                            CameraManager.instance.SetFilter(CameraFilter.RAIN);

                        if (actualScene.sceneType == SceneType.OUTSIDE && rainInstance == null)
                        {
                            rainInstance = Instantiate(rain, PlayerManager.instance.player.transform.position, Quaternion.identity);

                            foreach (var cloud in clouds)
                            {
                                if(cloud)
                                    cloud.GetComponent<CloudBehiavour>().DestroyCloud();
                            }
                        }
                        else if (actualScene.sceneType != SceneType.OUTSIDE && rainInstance != null)
                        {
                            Destroy(rainInstance);
                            rainInstance = null;
                        }
                        break;
                    case WeatherType.BLIZZARD:
                        break;
                    case WeatherType.DUST_STORM:
                        if (CameraManager.instance.GetFilter() == CameraFilter.NONE)
                            CameraManager.instance.SetFilter(CameraFilter.DUST_STORM);
                        
                        if (actualScene.sceneType == SceneType.OUTSIDE && dustStormInstance == null)
                        {
                            dustStormInstance = Instantiate(dustStorm, PlayerManager.instance.player.transform.position, Quaternion.identity);

                            foreach (var cloud in clouds)
                            {
                                if (cloud)
                                    cloud.GetComponent<CloudBehiavour>().DestroyCloud();
                            }
                        }
                        else if (actualScene.sceneType != SceneType.OUTSIDE && rainInstance != null)
                        {
                            Destroy(dustStormInstance);
                            dustStormInstance = null;
                        }
                        break;
                    default:
                        break;
                }

            }

            // Attendre une seconde avant de vérifier à nouveau
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator StopWeather(float weatherDuration)
    {
        // Attendre la durée de la pluie
        yield return new WaitForSeconds(weatherDuration);

        // Arrêter la pluie et détruire l'objet de pluie
        isWeather = false;

        switch (actualWeatherType)
        {
            case WeatherType.RAINING:
                SetSceneFilter();
                if (rainInstance != null)
                {
                    Destroy(rainInstance);
                    rainInstance = null;
                }
                break;
            case WeatherType.BLIZZARD:
                break;
            case WeatherType.DUST_STORM:
                if (dustStormInstance != null)
                {
                    Destroy(dustStormInstance);
                    dustStormInstance = null;
                }
                break;
            default:
                break;
        }

    }

    private int previousHour = -1;

    IEnumerator RoutineTimeWorld()
    {
        float dayCycleDuration = lengthOneDay * 60f; // Durée d'une journée complète en secondes (1800 secondes)

        while (true)
        {
            timeWorld += Time.deltaTime; // Avance le temps mondial en secondes

            string timeString = ConvertTimeWorldToDayTime();
            int currentHour = int.Parse(timeString.Split(',')[1].Trim().Split('h')[0]);

            if (currentHour != previousHour)
            {
                Debug.Log($"Heure actuelle : {timeString}");
                previousHour = currentHour;
            }

            // Normalise le temps mondial dans une échelle de 0 à 1 (0 étant le début du cycle jour/nuit et 1 la fin)
            float normalizedTime = (timeWorld % dayCycleDuration) / dayCycleDuration;

            // Utilise une sinusoïde pour créer une transition douce entre jour et nuit
            float intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, Mathf.Sin(normalizedTime * Mathf.PI * 2) * 0.5f + 0.5f);

            // Définit l'intensité du soleil
            if (actualScene.sceneType == SceneType.OUTSIDE)
                LightManager.instance.SetSunIntensity(intensity);

            yield return null; // Attendre la prochaine frame
        }
    }




}
