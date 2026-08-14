using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager instance;
    public GameObject transitionPanel;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ShowSceneTitle();
    }

    public void ChangeSceneObject(string sceneName, Vector2 newPosition, float transitionDuration = .5f)
    {
        canTeleportPlayer = false;
        StartCoroutine(ChangeSceneWithDelay(sceneName, transitionDuration, newPosition));
    }

    public void ChangeScene(string sceneName, float transitionDuration = .5f)
    {
        StartCoroutine(ChangeSceneWithDelay(sceneName, transitionDuration, null));
    }

    public bool isSceneLoaded = false;
    public bool canTeleportPlayer = true;

    // Survit � la destruction de l'instance ScenesManager de l'ancienne sc�ne (contrairement � un
    // champ d'instance) : le joueur de la NOUVELLE sc�ne le lit dans son propre Start() pour savoir
    // s'il doit se prot�ger d'une chute avant m�me que ScenesManager n'ait pu le t�l�porter.
    public static bool pendingTeleport = false;

    private IEnumerator ChangeSceneWithDelay(string sceneName, float transitionDuration, Vector2? newPosition)
    {
        isSceneLoaded = false;
        pendingTeleport = newPosition.HasValue;

        // Bloque le joueur (d�placement, attaque, objets sp�ciaux, inventaire, qu�tes...)
        // pendant toute la transition, jusqu'� la fin du fondu de sortie
        LockPlayer(true);

        UIAnimator.instance.ActivateObjectWithTransition(transitionPanel, transitionDuration);
        yield return new WaitForSecondsRealtime(transitionDuration);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Le joueur de cette nouvelle sc�ne est une INSTANCE DIFFERENTE de celui d'avant (PlayerManager
        // n'est pas DontDestroyOnLoad, chaque sc�ne a sa propre copie) : il d�marre � sa position par
        // d�faut dans l'�diteur. On le reverrouille explicitement (son propre Start() s'est d�j� prot�g�
        // via pendingTeleport, ceci confirme/maintient la protection) puis on le t�l�porte.
        LockPlayer(true);
        pendingTeleport = false;

        if (newPosition.HasValue && PlayerManager.instance != null && PlayerManager.instance.player != null)
            PlayerManager.instance.player.transform.position = newPosition.Value;

        yield return new WaitForSecondsRealtime(transitionDuration);

        SoundManager.instance.PlayMusic(sceneName);
        MeteoManager.instance.UpdateActualScene(SceneManager.GetActiveScene());
        ShowSceneTitle();
        FoundLocation();

        // Signaler que c'est charg� AVANT de fermer la transition
        isSceneLoaded = true;

        // Petit d�lai suppl�mentaire pour �tre s�r que tout est en place
        yield return new WaitForSecondsRealtime(0.5f);

        UIAnimator.instance.DeactivateObjectWithTransition(transitionPanel, transitionDuration);

        // DeactivateObjectWithTransition tourne en fire-and-forget dans UIAnimator : on attend
        // sa dur�e pour �tre s�r que le fondu est termin� avant de red�bloquer le joueur
        yield return new WaitForSecondsRealtime(transitionDuration);

        LockPlayer(false);
        canTeleportPlayer = true;
    }

    private void LockPlayer(bool locked)
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            PlayerManager.instance.player.GetComponent<Stats>().canMove = !locked;

            // Le joueur de la nouvelle sc�ne d�marre � sa position par d�faut avant qu'on le
            // t�l�porte � newPosition : si cette position par d�faut chevauche un trou, la
            // physique peut d�clencher la chute avant m�me que la t�l�portation ne s'ex�cute.
            // On bloque explicitement toute chute pendant toute la dur�e de la transition.
            PlayerController playerController = PlayerManager.instance.player.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.cantFall = locked;
        }

        if (InventoryManager.instance != null)
            InventoryManager.instance.canOpenInventory = !locked;

        if (QuestManager.instance != null)
            QuestManager.instance.canOpenQuests = !locked;
    }

    void ShowSceneTitle()
    {
        foreach (var region in MeteoManager.instance.regions)
        {
            foreach (var scene in region.scenes)
            {
                if (scene.SceneName == SceneManager.GetActiveScene().name)
                {
                    NotificationManager.instance.ShowTitle(LocalizationManager.instance.GetText("LOCATION", region.regionID + "_REGION"), LocalizationManager.instance.GetText("LOCATION", scene.sceneID + "_SCENE"));
                }
            }
        }

    }

    void FoundLocation()
    {
        foreach (var region in MeteoManager.instance.regions)
        {
            foreach (var scene in region.scenes)
            {
                if (scene.SceneName == SceneManager.GetActiveScene().name)
                {
                    StatsManager.instance.LocationFound(scene.sceneID);
                }
            }
        }
    }
}
