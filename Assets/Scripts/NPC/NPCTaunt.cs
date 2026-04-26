using UnityEngine;
using UnityEngine.AI;

public class NPCTaunt : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float stopDistance = 2f;

    private Transform forcedTarget;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (forcedTarget == null || agent == null || !agent.enabled)
            return;

        agent.stoppingDistance = stopDistance;
        agent.SetDestination(forcedTarget.position);
    }

    public void SetForcedTarget(Transform target)
    {
        forcedTarget = target;
    }
}