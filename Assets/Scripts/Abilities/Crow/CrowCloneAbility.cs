using UnityEngine;

[CreateAssetMenu(fileName = "Ability_CrowClone", menuName = "Classes/Crow/Skill 1 - Crow Clone")]
public class CrowCloneAbility : AbilityDefinition
{
    public GameObject clonePrefab;
    public float spawnDistance = 1.5f;

    public override void Activate(GameObject user, PlayerClassController classController)
    {
        CrowCloneController controller = user.GetComponent<CrowCloneController>();

        if (controller == null)
            controller = user.AddComponent<CrowCloneController>();

        controller.UseCloneAbility(this);
    }
}