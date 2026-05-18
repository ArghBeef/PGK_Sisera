using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerClassController : MonoBehaviour
{
    [Header("Class")]
    [SerializeField] private ClassDefinition currentClass;

    [Header("Input")]
    [SerializeField] private InputActionReference ability1Action;
    [SerializeField] private InputActionReference ability2Action;
    [SerializeField] private InputActionReference ultimateAction;

    private float ability1Cooldown;
    private float ability2Cooldown;
    private float ultimateCooldown;

    private HoldTargetAbilityDefinition heldAbility;
    private GameObject previewObject;
    private System.Action<float> setHeldCooldown;

    private bool hasValidPlacement;
    private Vector3 placementPoint;
    private Quaternion placementRotation;

    public ClassDefinition CurrentClass => currentClass;

    private void OnEnable()
    {
        EnableAction(ability1Action);
        EnableAction(ability2Action);
        EnableAction(ultimateAction);

        if (ability1Action != null)
        {
            ability1Action.action.started += OnAbility1Started;
            ability1Action.action.canceled += OnAbility1Canceled;
        }

        if (ability2Action != null)
        {
            ability2Action.action.started += OnAbility2Started;
            ability2Action.action.canceled += OnAbility2Canceled;
        }

        if (ultimateAction != null)
        {
            ultimateAction.action.started += OnUltimateStarted;
            ultimateAction.action.canceled += OnUltimateCanceled;
        }
    }

    private void OnDisable()
    {
        if (ability1Action != null)
        {
            ability1Action.action.started -= OnAbility1Started;
            ability1Action.action.canceled -= OnAbility1Canceled;
        }

        if (ability2Action != null)
        {
            ability2Action.action.started -= OnAbility2Started;
            ability2Action.action.canceled -= OnAbility2Canceled;
        }

        if (ultimateAction != null)
        {
            ultimateAction.action.started -= OnUltimateStarted;
            ultimateAction.action.canceled -= OnUltimateCanceled;
        }

        DisableAction(ability1Action);
        DisableAction(ability2Action);
        DisableAction(ultimateAction);

        ClearPreview();
    }

    private void Update()
    {
        ability1Cooldown -= Time.deltaTime;
        ability2Cooldown -= Time.deltaTime;
        ultimateCooldown -= Time.deltaTime;

        UpdatePreview();
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null)
            actionReference.action.Enable();
    }

    private void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null)
            actionReference.action.Disable();
    }

    private void OnAbility1Started(InputAction.CallbackContext context)
    {
        if (currentClass == null)
            return;

        TryStartAbility(
            currentClass.activeAbility1,
            ability1Cooldown,
            value => ability1Cooldown = value
        );
    }

    private void OnAbility1Canceled(InputAction.CallbackContext context)
    {
        TryReleaseHeldAbility();
    }

    private void OnAbility2Started(InputAction.CallbackContext context)
    {
        if (currentClass == null)
            return;

        TryStartAbility(
            currentClass.activeAbility2,
            ability2Cooldown,
            value => ability2Cooldown = value
        );
    }

    private void OnAbility2Canceled(InputAction.CallbackContext context)
    {
        TryReleaseHeldAbility();
    }

    private void OnUltimateStarted(InputAction.CallbackContext context)
    {
        if (currentClass == null)
            return;

        TryStartAbility(
            currentClass.ultimate,
            ultimateCooldown,
            value => ultimateCooldown = value
        );
    }

    private void OnUltimateCanceled(InputAction.CallbackContext context)
    {
        TryReleaseHeldAbility();
    }

    private void TryStartAbility(
        AbilityDefinition ability,
        float cooldownTimer,
        System.Action<float> setCooldown)
    {
        if (ability == null)
            return;

        if (cooldownTimer > 0f)
            return;

        if (ability is CrowCloneAbility crowCloneAbility)
        {
            CrowCloneController cloneController = GetComponent<CrowCloneController>();

            if (cloneController == null)
                cloneController = gameObject.AddComponent<CrowCloneController>();

            if (cloneController.HasClone)
            {
                cloneController.TeleportToClone(crowCloneAbility);
                setCooldown.Invoke(crowCloneAbility.teleportCooldown);
                return;
            }

            StartHoldingAbility(crowCloneAbility, setCooldown);
            return;
        }

        if (ability is HoldTargetAbilityDefinition holdAbility)
        {
            StartHoldingAbility(holdAbility, setCooldown);
            return;
        }

        ability.Activate(gameObject, this);
        setCooldown.Invoke(ability.cooldown);
    }

    private void StartHoldingAbility(
        HoldTargetAbilityDefinition ability,
        System.Action<float> setCooldown)
    {
        ClearPreview();

        heldAbility = ability;
        setHeldCooldown = setCooldown;

        if (heldAbility.previewPrefab != null)
            previewObject = Instantiate(heldAbility.previewPrefab);

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (heldAbility == null)
            return;

        hasValidPlacement = GetMousePlacement(
            heldAbility,
            out placementPoint,
            out placementRotation
        );

        if (previewObject != null)
        {
            previewObject.SetActive(hasValidPlacement);

            if (hasValidPlacement)
            {
                previewObject.transform.position = placementPoint;
                previewObject.transform.rotation = placementRotation;
            }
        }
    }

    private void TryReleaseHeldAbility()
    {
        if (heldAbility == null)
            return;

        if (hasValidPlacement)
        {
            heldAbility.ActivateAtPoint(
                gameObject,
                this,
                placementPoint,
                placementRotation
            );

            if (heldAbility is CrowCloneAbility crowCloneAbility)
                setHeldCooldown?.Invoke(crowCloneAbility.placeCooldown);
            else
                setHeldCooldown?.Invoke(heldAbility.cooldown);
        }

        ClearPreview();
    }

    private bool GetMousePlacement(
        HoldTargetAbilityDefinition ability,
        out Vector3 point,
        out Quaternion rotation)
    {
        point = Vector3.zero;
        rotation = Quaternion.identity;

        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            ability.maxPlacementDistance,
            ability.placementLayers,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        point = hit.point;

        if (ability.alignToGroundNormal)
            rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        else
            rotation = Quaternion.identity;

        return true;
    }

    private void ClearPreview()
    {
        if (previewObject != null)
            Destroy(previewObject);

        previewObject = null;
        heldAbility = null;
        setHeldCooldown = null;
        hasValidPlacement = false;
    }

    public float GetDamageMultiplierAgainst(GameObject target)
    {
        if (currentClass == null)
            return 1f;

        NPCStatus status = target.GetComponent<NPCStatus>();

        if (currentClass.stunnedEnemiesTakeMoreDamage && status != null && status.IsStunned)
            return currentClass.stunnedDamageMultiplier;

        return 1f;
    }

    public void HealFromDamage(float incomingDamage)
    {
        if (currentClass == null || !currentClass.damageHealsPlayer)
            return;

        Health health = GetComponent<Health>();

        if (health != null)
            health.Heal(incomingDamage * currentClass.damageHealPercent);
    }
}