using UnityEngine;

[CreateAssetMenu(fileName = "Ability_RamDecoy", menuName = "Classes/Ram/Ultimate - Decoy")]
public class RamDecoyAbility : HoldTargetAbilityDefinition
{
    [Header("Decoy")]
    public GameObject decoyPrefab;

    public override void ActivateAtPoint(
        GameObject user,
        PlayerClassController classController,
        Vector3 point,
        Quaternion rotation)
    {
        if (decoyPrefab == null)
            return;

        Instantiate(decoyPrefab, point, rotation);
    }
}