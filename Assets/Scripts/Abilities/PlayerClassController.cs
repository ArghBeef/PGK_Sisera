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

    public ClassDefinition CurrentClass => currentClass;

    private void OnEnable()
    {
        ability1Action?.action.Enable();
        ability2Action?.action.Enable();
        ultimateAction?.action.Enable();
    }

    private void OnDisable()
    {
        ability1Action?.action.Disable();
        ability2Action?.action.Disable();
        ultimateAction?.action.Disable();
    }

    private void Update()
    {
        TickCooldowns();

        if (currentClass == null)
            return;

        if (ability1Action != null && ability1Action.action.WasPressedThisFrame())
            TryUseAbility(currentClass.activeAbility1, ref ability1Cooldown);

        if (ability2Action != null && ability2Action.action.WasPressedThisFrame())
            TryUseAbility(currentClass.activeAbility2, ref ability2Cooldown);

        if (ultimateAction != null && ultimateAction.action.WasPressedThisFrame())
            TryUseAbility(currentClass.ultimate, ref ultimateCooldown);
    }

    private void TickCooldowns()
    {
        ability1Cooldown -= Time.deltaTime;
        ability2Cooldown -= Time.deltaTime;
        ultimateCooldown -= Time.deltaTime;
    }

    private void TryUseAbility(AbilityDefinition ability, ref float cooldownTimer)
    {
        if (ability == null || cooldownTimer > 0f)
            return;

        ability.Activate(gameObject, this);
        cooldownTimer = ability.cooldown;
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