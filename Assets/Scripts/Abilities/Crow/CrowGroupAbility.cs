using UnityEngine;

[CreateAssetMenu(fileName = "Ability_CrowGroup", menuName = "Classes/Crow/Skill 2 - Crow Group")]
public class CrowGroupAbility : HoldTargetAbilityDefinition
{
    [Header("Crow Group")]
    public GameObject crowGroupPrefab;

    public override void ActivateAtPoint(
        GameObject user,
        PlayerClassController classController,
        Vector3 point,
        Quaternion rotation)
    {
        if (crowGroupPrefab != null)
            Instantiate(crowGroupPrefab, point, rotation);
    }
}