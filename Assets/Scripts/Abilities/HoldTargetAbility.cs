using UnityEngine;

public abstract class HoldTargetAbilityDefinition : AbilityDefinition
{
    [Header("Hold Targeting")]
    public GameObject previewPrefab;
    public LayerMask placementLayers;
    public float maxPlacementDistance = 40f;
    public bool alignToGroundNormal = false;

    public abstract void ActivateAtPoint(
        GameObject user,
        PlayerClassController classController,
        Vector3 point,
        Quaternion rotation
    );

    public override void Activate(GameObject user, PlayerClassController classController)
    {
    }
}