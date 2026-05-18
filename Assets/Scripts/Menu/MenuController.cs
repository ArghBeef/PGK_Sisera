using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera menuCamera;
    [SerializeField] private MainMenuCamera menuCameraMover;
    [SerializeField] private MainMenuUI menuUI;

    [Header("Camera Points")]
    [SerializeField] private Transform selectionCameraPoint;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 200f;
    [SerializeField] private LayerMask selectableLayers = ~0;

    private bool entranceActive = true;

    private MenuSelectable hovered;
    private MenuSelectable selectedSkull;
    private MenuSelectable selectedFolder;

    private void Awake()
    {
        if (menuCamera == null)
            menuCamera = Camera.main;

        if (menuCameraMover == null && menuCamera != null)
            menuCameraMover = menuCamera.GetComponent<MainMenuCamera>();
    }

    private void Update()
    {
        if (entranceActive)
        {
            HandleEntranceInput();
            return;
        }

        HandleHover();
        HandleClick();
    }

    private void HandleEntranceInput()
    {
        bool pressedKeyboard = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool pressedMouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (!pressedKeyboard && !pressedMouse)
            return;

        entranceActive = false;

        if (menuUI != null)
            menuUI.ShowEntrance(false);

        if (menuCameraMover != null)
            menuCameraMover.MoveTo(selectionCameraPoint);
    }

    private void HandleHover()
    {
        MenuSelectable newHover = GetSelectableUnderMouse();

        if (hovered == newHover)
            return;

        if (hovered != null)
            hovered.SetHighlight(false);

        hovered = newHover;

        if (hovered != null)
        {
            hovered.SetHighlight(true);

            if (menuUI != null)
                menuUI.ShowInfo(hovered);
        }
        else
        {
            if (menuUI != null)
                menuUI.HideInfo();
        }
    }

    private void HandleClick()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        MenuSelectable clicked = GetSelectableUnderMouse();

        if (clicked == null)
        {
            Debug.Log("Clicked nothing selectable.");
            return;
        }

        Debug.Log("Clicked: " + clicked.name);

        SelectObject(clicked);
    }

    private MenuSelectable GetSelectableUnderMouse()
    {
        if (menuCamera == null)
        {
            Debug.LogWarning("Menu camera is missing.");
            return null;
        }

        if (Mouse.current == null)
            return null;

        Ray ray = menuCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, selectableLayers))
        {
            Debug.Log("Raycast hit nothing. Check collider/layer/camera.");
            return null;
        }

        Debug.Log("Raycast hit object: " + hit.collider.name);

        MenuSelectable selectable = hit.collider.GetComponent<MenuSelectable>();

        if (selectable == null)
            selectable = hit.collider.GetComponentInParent<MenuSelectable>();

        if (selectable == null)
        {
            Debug.LogWarning("Hit object has no MenuSelectable on itself or parent: " + hit.collider.name);
            return null;
        }

        return selectable;
    }

    private void SelectObject(MenuSelectable selectable)
    {
        if (selectable == null)
            return;

        if (selectable.selectableType == MainMenuSelectableType.Skull)
        {
            if (selectedSkull != null)
                selectedSkull.Deselect();

            selectedSkull = selectable;
            selectedSkull.Select();
        }

        if (selectable.selectableType == MainMenuSelectableType.Folder)
        {
            if (selectedFolder != null)
                selectedFolder.Deselect();

            selectedFolder = selectable;
            selectedFolder.Select();
        }

        if (selectable.focusPoint != null && menuCameraMover != null)
            menuCameraMover.MoveTo(selectable.focusPoint);

        RefreshStartButton();
    }

    private void RefreshStartButton()
    {
        bool ready = selectedSkull != null && selectedFolder != null;

        if (menuUI != null)
            menuUI.SetStartVisible(ready);
    }

    public void StartGame()
    {
        if (selectedSkull == null || selectedFolder == null)
            return;

        PlayerPrefs.SetString("SelectedSkull", selectedSkull.displayName);
        PlayerPrefs.SetString("SelectedFolder", selectedFolder.displayName);

        SceneManager.LoadScene(gameSceneName);
    }
}