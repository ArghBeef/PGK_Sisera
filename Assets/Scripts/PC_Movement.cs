using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PC_Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private InputActionReference moveAction;

    [Header("Run")]
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private float runSpeedMultiplier = 1.6f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainPerSecond = 25f;
    [SerializeField] private float staminaRecoveryPerSecond = 18f;
    [SerializeField] private float staminaRecoveryDelay = 0.5f;
    [SerializeField] private float minimumStaminaToRun = 5f;

    [Header("Dash")]
    [SerializeField] private InputActionReference dashAction;
    [SerializeField] private float dashDistance = 4f;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Visual")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Animator animator;

    [Header("Animator Parameters")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string movingParameter = "IsMoving";
    [SerializeField] private string runningParameter = "IsRunning";
    [SerializeField] private string dashTriggerParameter = "Dash";

    public bool IsMovementLocked { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsDashing { get; private set; }

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float Stamina01 => maxStamina > 0f ? currentStamina / maxStamina : 0f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 lastMoveDirection;

    private float moveSpeedMultiplier = 1f;
    private float rotationSpeedMultiplier = 1f;

    private float currentStamina;
    private float lastStaminaUseTime;
    private float lastDashTime = -999f;

    private Coroutine dashRoutine;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (modelRoot == null && transform.childCount > 0)
            modelRoot = transform.GetChild(0);

        if (animator == null && modelRoot != null)
            animator = modelRoot.GetComponentInChildren<Animator>();

        currentStamina = maxStamina;
    }

    private void OnEnable()
    {
        if (moveAction != null)
            moveAction.action.Enable();

        if (runAction != null)
            runAction.action.Enable();

        if (dashAction != null)
            dashAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.action.Disable();

        if (runAction != null)
            runAction.action.Disable();

        if (dashAction != null)
            dashAction.action.Disable();
    }

    private void Update()
    {
        ReadMovementInput();
        HandleRun();
        HandleDashInput();
        RecoverStamina();
        UpdateAnimator();
    }

    private void LateUpdate()
    {
        RotateModel();
    }

    private void FixedUpdate()
    {
        if (IsDashing)
            return;

        if (IsMovementLocked || moveInput.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        float currentSpeed = moveSpeed;

        if (IsRunning)
            currentSpeed *= runSpeedMultiplier;

        Vector3 velocity = moveInput * currentSpeed * moveSpeedMultiplier;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    private void ReadMovementInput()
    {
        if (IsMovementLocked || IsDashing || moveAction == null)
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

        if (Camera.main == null)
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

    private void HandleRun()
    {
        if (IsMovementLocked || IsDashing)
        {
            IsRunning = false;
            return;
        }

        if (runAction == null)
        {
            IsRunning = false;
            return;
        }

        bool wantsToRun = runAction.action.IsPressed();
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool hasEnoughStamina = currentStamina > minimumStaminaToRun;

        IsRunning = wantsToRun && isMoving && hasEnoughStamina;

        if (IsRunning)
        {
            currentStamina -= staminaDrainPerSecond * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            lastStaminaUseTime = Time.time;

            if (currentStamina <= 0f)
                IsRunning = false;
        }
    }

    private void RecoverStamina()
    {
        if (IsRunning || IsDashing)
            return;

        if (Time.time < lastStaminaUseTime + staminaRecoveryDelay)
            return;

        currentStamina += staminaRecoveryPerSecond * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    private void HandleDashInput()
    {
        if (dashAction == null)
            return;

        if (!dashAction.action.WasPressedThisFrame())
            return;

        TryDash();
    }

    public void TryDash()
    {
        if (IsMovementLocked || IsDashing)
            return;

        if (Time.time < lastDashTime + dashCooldown)
            return;

        Vector3 dashDirection = moveInput.sqrMagnitude > 0.01f
            ? moveInput
            : FacingDirection;

        dashDirection.y = 0f;

        if (dashDirection.sqrMagnitude < 0.01f)
            dashDirection = transform.forward;

        dashDirection.Normalize();

        if (dashRoutine != null)
            StopCoroutine(dashRoutine);

        dashRoutine = StartCoroutine(DashRoutine(dashDirection));
    }

    private IEnumerator DashRoutine(Vector3 dashDirection)
    {
        IsDashing = true;
        IsRunning = false;
        lastDashTime = Time.time;
        lastStaminaUseTime = Time.time;

        if (animator != null && !string.IsNullOrWhiteSpace(dashTriggerParameter))
            animator.SetTrigger(dashTriggerParameter);

        float travelled = 0f;

        while (travelled < dashDistance)
        {
            float step = dashSpeed * Time.fixedDeltaTime;

            Vector3 targetPosition = rb.position + dashDirection * step;
            rb.MovePosition(targetPosition);

            travelled += step;

            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        IsDashing = false;
        dashRoutine = null;
    }

    private void RotateModel()
    {
        if (modelRoot == null)
            return;

        if (lastMoveDirection.sqrMagnitude < 0.01f)
            return;

        if (moveInput.sqrMagnitude < 0.01f && !IsDashing)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection);

        modelRoot.rotation = Quaternion.Slerp(
            modelRoot.rotation,
            targetRotation,
            rotationSpeed * rotationSpeedMultiplier * Time.deltaTime
        );
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f && !IsMovementLocked && !IsDashing;

        float speedValue = 0f;

        if (isMoving)
        {
            speedValue = IsRunning ? 1f : 0.5f;
            speedValue *= moveSpeedMultiplier;
        }

        animator.SetFloat(speedParameter, speedValue);
        animator.SetBool(movingParameter, isMoving);
        animator.SetBool(runningParameter, IsRunning);
    }

    public void SetMovementLocked(bool locked)
    {
        IsMovementLocked = locked;

        if (locked)
        {
            moveInput = Vector3.zero;
            IsRunning = false;
        }
    }

    public void SetSpeedMultipliers(float movementMultiplier, float rotationMultiplier)
    {
        moveSpeedMultiplier = movementMultiplier;
        rotationSpeedMultiplier = rotationMultiplier;
    }

    public void ResetSpeedMultipliers()
    {
        moveSpeedMultiplier = 1f;
        rotationSpeedMultiplier = 1f;
    }
}