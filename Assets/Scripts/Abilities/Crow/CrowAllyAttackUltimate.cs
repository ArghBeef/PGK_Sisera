using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Ability_CrowAllyAttack", menuName = "Classes/Crow/Ultimate - Ally Attack")]
public class CrowAllyAttackUltimate : AbilityDefinition
{
    public float maxSelectDistance = 40f;
    public float allySearchRadius = 8f;
    public float damage = 40f;

    public LayerMask npcLayers;

    public override void Activate(GameObject user, PlayerClassController classController)
    {
        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, maxSelectDistance, npcLayers))
            return;

        NPCController selectedNpc = hit.collider.GetComponentInParent<NPCController>();

        if (selectedNpc == null)
            return;

        NPCController ally = FindClosestAlly(selectedNpc);

        if (ally == null)
            return;

        IDamageable damageable = ally.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = ally.GetComponentInChildren<IDamageable>();

        if (damageable != null)
            damageable.TakeDamage(damage);
    }

    private NPCController FindClosestAlly(NPCController selectedNpc)
    {
        Collider[] hits = Physics.OverlapSphere(
            selectedNpc.transform.position,
            allySearchRadius,
            npcLayers,
            QueryTriggerInteraction.Ignore
        );

        NPCController closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            NPCController npc = hit.GetComponentInParent<NPCController>();

            if (npc == null)
                continue;

            if (npc == selectedNpc)
                continue;

            Health health = npc.GetComponent<Health>();

            if (health != null && health.IsDead)
                continue;

            float distance = Vector3.Distance(
                selectedNpc.transform.position,
                npc.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = npc;
            }
        }

        return closest;
    }
}