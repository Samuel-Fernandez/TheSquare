using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;
    public GameObject cursorPrefab;
    public float cursorSpeed = 1000f;

    private GameObject cursorInstance;
    private Vector2 cursorPosition;
    private bool isGamepadActive = false;
    private PlayerInput playerInput;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Instancier le curseur dans le monde (hors UI)
        cursorInstance = Instantiate(cursorPrefab);
        cursorInstance.SetActive(false);

        cursorPosition = new Vector2(Screen.width / 2, Screen.height / 2); // Position initiale au centre de l'écran

        playerInput = PlayerManager.instance.GetComponent<PlayerInput>();

        // Détection des entrées manette
        PlayerManager.instance.playerInputActions.Menu.Cursor.performed += ctx => MoveCursor(ctx.ReadValue<Vector2>());
        PlayerManager.instance.playerInputActions.Menu.Accept.performed += _ => ClickUI();

        // Détection de la souris via le nouveau Input System
        InputSystem.onAnyButtonPress.Call(CheckMouseInput);
    }

    private void MoveCursor(Vector2 input)
    {
        if (!isGamepadActive)
        {
            cursorInstance.SetActive(true);
            isGamepadActive = true;
        }

        cursorPosition += input * cursorSpeed * Time.deltaTime;
        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);

        cursorInstance.transform.position = cursorPosition;
    }

    private void ClickUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = cursorPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            GameObject clickedObject = results[0].gameObject;
            ExecuteEvents.Execute(clickedObject, pointerData, ExecuteEvents.pointerClickHandler);
        }
    }

    private void CheckMouseInput(InputControl control)
    {
        if (control.device is Mouse && isGamepadActive)
        {
            cursorInstance.SetActive(false);
            isGamepadActive = false;
        }
    }
}
