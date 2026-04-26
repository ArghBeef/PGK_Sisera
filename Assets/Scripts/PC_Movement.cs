using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PC_Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private InputActionReference moveAction;

    [Header("Visual")]
    [SerializeField] private Transform modelRoot;

    public bool IsMovementLocked { get; private set; }

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 lastMoveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Very important: physics should NOT rotate the player root.
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (modelRoot == null)
            modelRoot = transform.GetChild(0);
    }

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
        ReadMovementInput();
        //UpdateAnimator();
    }

    private void LateUpdate()
    {
        RotateModel();
    }

    private void FixedUpdate()
    {
        if (IsMovementLocked || moveInput.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 velocity = moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    private void ReadMovementInput()
    {
        if (IsMovementLocked || moveAction == null)
        {
            moveInput = Vector3.zero;
            return;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        if (input.sqrMagnitude < 0.01f)
        {
            moveInput = Vector3.zero;
            return;
        }

        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        moveInput = camForward * input.y + camRight * input.x;
        moveInput.Normalize();

        lastMoveDirection = moveInput;
    }

    private void MovePlayer()
    {
        if (IsMovementLocked || moveInput.sqrMagnitude < 0.01f)
            return;

        Vector3 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void RotateModel()
    {
        if (modelRoot == null)
            return;

        if (lastMoveDirection.sqrMagnitude < 0.01f)
            return;

        if (moveInput.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection);
        modelRoot.rotation = Quaternion.Slerp(
            modelRoot.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void SetMovementLocked(bool locked)
    {
        IsMovementLocked = locked;

        if (locked)
            moveInput = Vector3.zero;
    }

    public Vector3 FacingDirection
    {
        get
        {
            if (modelRoot != null)
                return modelRoot.forward;

            if (lastMoveDirection.sqrMagnitude > 0.01f)
                return lastMoveDirection;

            return transform.forward;
        }
    }
}