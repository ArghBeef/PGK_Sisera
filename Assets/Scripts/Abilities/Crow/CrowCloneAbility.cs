using UnityEngine;

[CreateAssetMenu(fileName = "Ability_CrowClone", menuName = "Classes/Crow/Skill 1 - Crow Clone")]
public class CrowCloneAbility : HoldTargetAbilityDefinition
{
    [Header("Clone")]
    public GameObject clonePrefab;

    [Header("Cooldowns")]
    public float placeCooldown = 6f;
    public float teleportCooldown = 3f;

    public override void ActivateAtPoint(
        GameObject user,
        PlayerClassController classController,
        Vector3 point,
        Quaternion rotation)
    {
        CrowCloneController controller = user.GetComponent<CrowCloneController>();

        if (controller == null)
            controller = user.AddComponent<CrowCloneController>();

        controller.PlaceClone(this, point, rotation);
    }
}