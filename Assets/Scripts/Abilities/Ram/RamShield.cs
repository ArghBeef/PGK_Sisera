using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Ability_HumanShield", menuName = "Classes/Ram/Active Ability 2 - Human Shield")]
public class RamHumanShieldAbility : AbilityDefinition
{
    [Header("Grab")]
    public float grabRange = 2.5f;
    public float grabRadius = 1f;
    public LayerMask npcLayers;

    [Header("Hold")]
    public float duration = 5f;
    public float holdDistance = 1.2f;
    public float holdHeight = 0f;

    [Header("Player Slow")]
    public float playerMoveSpeedMultiplier = 0.45f;
    public float playerRotationSpeedMultiplier = 0.55f;

    [Header("After")]
    public float endStunDuration = 5f;

    public override void Activate(GameObject user, PlayerClassController classController)
    {
        RamHumanShieldRunner runner = user.GetComponent<RamHumanShieldRunner>();

        if (runner == null)
            runner = user.AddComponent<RamHumanShieldRunner>();

        runner.StartShield(this);
    }
}

public class RamHumanShieldRunner : MonoBehaviour
{
    private bool active;
    private GameObject grabbedEnemy;

    public void StartShield(RamHumanShieldAbility ability)
    {
        if (active)
            return;

        StartCoroutine(ShieldRoutine(ability));
    }

    private IEnumerator ShieldRoutine(RamHumanShieldAbility ability)
    {
        active = true;

        NPCController npc = FindNpcInFront(ability);

        if (npc == null)
        {
            Debug.Log("Human Shield: no NPC found in front.");
            active = false;
            yield break;
        }

        grabbedEnemy = npc.gameObject;

        PC_Movement playerMovement = GetComponent<PC_Movement>();
        NavMeshAgent agent = grabbedEnemy.GetComponent<NavMeshAgent>();
        NPCStatus status = grabbedEnemy.GetComponent<NPCStatus>();
        Rigidbody enemyRb = grabbedEnemy.GetComponent<Rigidbody>();
        NPCController npcController = grabbedEnemy.GetComponent<NPCController>();
        NPCDialogueController dialogueController = grabbedEnemy.GetComponent<NPCDialogueController>();

        bool npcControllerWasEnabled = npcController != null && npcController.enabled;
        bool dialogueWasEnabled = dialogueController != null && dialogueController.enabled;
        bool agentWasEnabled = agent != null && agent.enabled;
        bool enemyRbWasKinematic = enemyRb != null && enemyRb.isKinematic;

        if (playerMovement != null)
        {
            playerMovement.SetSpeedMultipliers(
                ability.playerMoveSpeedMultiplier,
                ability.playerRotationSpeedMultiplier
            );
        }

        if (npcController != null)
            npcController.enabled = false;

        if (dialogueController != null)
            dialogueController.enabled = false;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (enemyRb != null)
        {
            enemyRb.linearVelocity = Vector3.zero;
            enemyRb.angularVelocity = Vector3.zero;
            enemyRb.isKinematic = true;
        }

        if (status != null)
            status.Stun(ability.duration);

        float timer = ability.duration;

        while (timer > 0f && grabbedEnemy != null)
        {
            Health health = grabbedEnemy.GetComponent<Health>();

            if (health != null && health.IsDead)
                break;

            Vector3 facingDirection = GetFacingDirection();

            Vector3 holdPosition =
                transform.position +
                facingDirection * ability.holdDistance +
                Vector3.up * ability.holdHeight;

            grabbedEnemy.transform.position = holdPosition;
            grabbedEnemy.transform.rotation = Quaternion.LookRotation(facingDirection);

            timer -= Time.deltaTime;
            yield return null;
        }

        if (grabbedEnemy != null)
        {
            Health health = grabbedEnemy.GetComponent<Health>();
            bool enemyAlive = health == null || !health.IsDead;

            if (enemyRb != null)
                enemyRb.isKinematic = enemyRbWasKinematic;

            if (agent != null && agentWasEnabled && enemyAlive)
            {
                agent.enabled = true;
                agent.isStopped = false;
            }

            if (npcController != null && npcControllerWasEnabled && enemyAlive)
                npcController.enabled = true;

            if (dialogueController != null && dialogueWasEnabled && enemyAlive)
                dialogueController.enabled = true;

            if (enemyAlive && status != null)
                status.Stun(ability.endStunDuration);
        }

        if (playerMovement != null)
            playerMovement.ResetSpeedMultipliers();

        grabbedEnemy = null;
        active = false;
    }

    private NPCController FindNpcInFront(RamHumanShieldAbility ability)
    {
        Vector3 direction = GetFacingDirection();
        Vector3 origin = transform.position + Vector3.up * 0.8f;

        Collider[] hits = Physics.OverlapSphere(
            origin + direction * ability.grabRange,
            ability.grabRadius,
            ability.npcLayers,
            QueryTriggerInteraction.Ignore
        );

        NPCController closestNpc = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            NPCController npc = hit.GetComponentInParent<NPCController>();

            if (npc == null)
                continue;

            Health health = npc.GetComponent<Health>();

            if (health != null && health.IsDead)
                continue;

            float distance = Vector3.Distance(transform.position, npc.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNpc = npc;
            }
        }

        return closestNpc;
    }

    private Vector3 GetFacingDirection()
    {
        PC_Movement movement = GetComponent<PC_Movement>();

        Vector3 direction = movement != null
            ? movement.FacingDirection
            : transform.forward;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            direction = transform.forward;

        direction.Normalize();
        return direction;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 direction = Application.isPlaying ? GetFacingDirection() : transform.forward;
        Vector3 origin = transform.position + Vector3.up * 0.8f;

        Gizmos.DrawWireSphere(origin + direction * 2.5f, 1f);
    }
}