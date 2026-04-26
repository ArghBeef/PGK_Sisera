using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCStatus : MonoBehaviour
{
    private NavMeshAgent agent;
    private Coroutine stunRoutine;

    public bool IsStunned { get; private set; }
    public bool LessVisibleWhenDead { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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

        if (agent != null)
            agent.isStopped = true;

        yield return new WaitForSeconds(duration);

        IsStunned = false;

        if (agent != null)
            agent.isStopped = false;
    }

    public void MarkDeadBodyLessVisible()
    {
        LessVisibleWhenDead = true;
    }
}