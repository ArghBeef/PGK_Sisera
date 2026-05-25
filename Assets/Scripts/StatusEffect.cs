using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StatusEffectController : MonoBehaviour
{
    [Header("Stun")]
    [SerializeField] private float defaultStunDuration = 2f;
    [SerializeField] private float stunnedDamageMultiplier = 1.5f;

    [Header("Disable While Stunned")]
    [SerializeField] private bool disableBehavioursWhileStunned = true;
    [SerializeField] private Behaviour[] disableOnStun;

    [Header("Optional NavMesh")]
    [SerializeField] private bool stopNavMeshAgentWhileStunned = true;

    private NavMeshAgent agent;
    private Coroutine stunRoutine;

    public bool IsStunned { get; private set; }

    public float DamageMultiplier
    {
        get
        {
            if (IsStunned)
                return stunnedDamageMultiplier;

            return 1f;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Stun()
    {
        Stun(defaultStunDuration);
    }

    public void Stun(float duration)
    {
        if (stunRoutine != null)
            StopCoroutine(stunRoutine);

        stunRoutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;

        if (disableBehavioursWhileStunned)
        {
            foreach (Behaviour behaviour in disableOnStun)
            {
                if (behaviour != null)
                    behaviour.enabled = false;
            }
        }

        if (stopNavMeshAgentWhileStunned &&
            agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        yield return new WaitForSeconds(duration);

        IsStunned = false;

        if (disableBehavioursWhileStunned)
        {
            foreach (Behaviour behaviour in disableOnStun)
            {
                if (behaviour != null)
                    behaviour.enabled = true;
            }
        }

        if (stopNavMeshAgentWhileStunned &&
            agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        stunRoutine = null;
    }
}