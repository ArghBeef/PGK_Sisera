using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Ability_CrowGroup", menuName = "Classes/Crow/Skill 2 - Crow Group")]
public class CrowGroupAbility : AbilityDefinition
{
    public GameObject crowGroupPrefab;

    public float maxCastDistance = 30f;
    public float alertRadius = 12f;
    public float investigationTime = 5f;

    public LayerMask groundLayers;

    public override void Activate(GameObject user, PlayerClassController classController)
    {
        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, maxCastDistance, groundLayers))
            return;

        if (crowGroupPrefab != null)
            Instantiate(crowGroupPrefab, hit.point, Quaternion.identity);

        //NPCWorldEventSystem.ReportEvent(
        //    NPCWorldEventType.Suspicious,
        //    hit.point,
        //    user,
        //    alertRadius,
        //    investigationTime
        //);
    }
}