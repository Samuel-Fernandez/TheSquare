using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum ButtonType
{
    LoadGame,
    ChangeScene,
    CloseOpenUI,
    LeaveGame,
}

public class MyButton : MonoBehaviour
{
    public ButtonType buttonType;
    public GameObject targetObject;
    public AudioClip sound;
    public string sceneName;
    public bool togglePlayerActivation;

    bool wait;

    public void OnButtonClicked()
    {
        if (!wait)
        {
            if (togglePlayerActivation)
                PlayerManager.instance.TogglePlayer(.5f);

            SoundManager.instance.PlaySound(sound, 3);

            StartCoroutine(RoutineWait());

            switch (buttonType)
            {
                case ButtonType.LoadGame:
                    if(SaveManager.instance.CheckIfSave())
                    {
                        SaveManager.instance.Load();
                    }
                    else
                    {
                        ScenesManager.instance.ChangeScene(sceneName);
                    }
                    break;
                case ButtonType.ChangeScene:
                    ScenesManager.instance.ChangeScene(sceneName);
                    break;
                case ButtonType.CloseOpenUI:
                    if (!targetObject.activeSelf)
                        UIAnimator.instance.ActivateObjectWithTransition(targetObject, .2f);
                    else
                        UIAnimator.instance.DeactivateObjectWithTransition(targetObject, .2f);
                    if (targetObject.name == "Inventory")
                        InventoryManager.instance.ToggleInventory(true);
                    break;
                case ButtonType.LeaveGame:
                    Environment.Exit(0);
                    break;
            }
        }
    }

    IEnumerator RoutineWait()
    {
        wait = true;
        yield return new WaitForSecondsRealtime(.5f);
        wait = false;
    }
}
