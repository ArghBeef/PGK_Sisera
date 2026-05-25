using UnityEngine;

[CreateAssetMenu(fileName = "Passive_MoveSpeed", menuName = "Classes/Passives/Passive Move Speed")]
public class MoveSpeedPassive : PassiveAbilityDefinition
{
    public float moveSpeedMultiplier = 1.25f;

    public override void Apply(GameObject user, PlayerClassController controller)
    {
        PC_Movement movement = user.GetComponent<PC_Movement>();

        if (movement != null)
            movement.SetSpeedMultipliers(moveSpeedMultiplier, 1f);
    }

    public override void Remove(GameObject user, PlayerClassController controller)
    {
    }
}