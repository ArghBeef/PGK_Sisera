using UnityEngine;

[CreateAssetMenu(fileName = "Passive_RamDamageHeal", menuName = "Classes/Ram/Passive Damage Heal")]
public class RamDamageHealPassive : PassiveAbilityDefinition
{
    [Range(0f, 1f)]
    public float healPercent = 0.2f;

    public float healDelay = 1.5f;

    public override void Apply(GameObject user, PlayerClassController controller)
    {
        RamDamageHealRuntime runtime = user.GetComponent<RamDamageHealRuntime>();

        if (runtime == null)
            runtime = user.AddComponent<RamDamageHealRuntime>();

        runtime.SetHealPercent(healPercent);
        runtime.SetHealDelay(healDelay);
    }

    public override void Remove(GameObject user, PlayerClassController controller)
    {
        RamDamageHealRuntime runtime = user.GetComponent<RamDamageHealRuntime>();

        if (runtime != null)
            Destroy(runtime);
    }
}