using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCrosshair : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] private bool hideDefaultCursor = true;
    [SerializeField] private bool lockCursor = false;

    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        Cursor.visible = !hideDefaultCursor;

        Cursor.lockState = lockCursor
            ? CursorLockMode.Locked
            : CursorLockMode.Confined;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        rectTransform.localPosition = localPoint;
    }
}