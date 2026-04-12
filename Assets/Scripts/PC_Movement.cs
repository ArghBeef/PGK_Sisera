using UnityEngine;
using UnityEngine.InputSystem;

public class PC_Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float stopDistance = 0.05f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private InputActionReference moveAction;

    public bool IsMovementLocked { get; private set; }

    private Vector3 targetPosition;
    private bool hasTarget;

    public static event System.Action<Vector3> OnGroundTouch;

    private void OnEnable()
    {
        if (moveAction != null)
            moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.action.Disable();
    }

    private void Update()
    {
        HandleTargetInput();
        HandleMovement();
    }

    private void HandleTargetInput()
    {
        if (IsMovementLocked)
            return;

        if (moveAction == null || !moveAction.action.triggered)
            return;

        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            targetPosition = hit.point;
            targetPosition.y = transform.position.y;
            hasTarget = true;

            OnGroundTouch?.Invoke(targetPosition);
        }
    }

    private void HandleMovement()
    {
        if (IsMovementLocked || !hasTarget)
            return;

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= stopDistance)
        {
            hasTarget = false;
            return;
        }

        Vector3 moveDirection = direction.normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    public void SetMovementLocked(bool locked)
    {
        IsMovementLocked = locked;

        if (locked)
        {
            hasTarget = false;
        }
    }
}