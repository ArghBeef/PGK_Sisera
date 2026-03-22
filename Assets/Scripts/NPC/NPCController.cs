using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    [System.Serializable]
    public class GameObjectEvent : UnityEvent<GameObject> { }

    private class DetectionProgress
    {
        public Collider target;
        public float timeInside;
        public bool fullyDetected;
        public NPCTagDetection rule;
    }

    [Header("Wandering")]
    [SerializeField] private bool canWander = true;
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float minWaitAtPoint = 1f;
    [SerializeField] private float maxWaitAtPoint = 3f;
    [SerializeField] private float stoppingDistance = 0.2f;

    [Header("Detection")]
    [SerializeField] private NPCDetectionZone detectionZone;
    [SerializeField] private NPCDetectionVisualizer detectionVisualizer;
    [SerializeField] private List<NPCTagDetection> detectionRules = new List<NPCTagDetection>();

    [SerializeField] private bool stopMovingWhenHostileDetected = true;
    [SerializeField] private bool lookAtDetectedTarget = true;
    [SerializeField] private float lookSpeed = 8f;

    [Header("Events")]
    [SerializeField] private UnityEvent onNeutralState;
    [SerializeField] private UnityEvent onWarningState;
    [SerializeField] private UnityEvent onDangerState;

    [SerializeField] private GameObjectEvent onFriendlyDetected;
    [SerializeField] private GameObjectEvent onSuspiciousDetected;
    [SerializeField] private GameObjectEvent onHostileDetected;

    private NavMeshAgent agent;
    private readonly Dictionary<Collider, DetectionProgress> trackedTargets = new();

    private float currentWaitTime;
    private bool waitingAtPoint;
    private Transform currentLookTarget;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;

        if (detectionZone != null)
            detectionZone.Initialize(this);

        SetNeutralState();
    }

    private void Update()
    {
        UpdateDetectionTimers();
        UpdateMovement();
        UpdateLookAt();
        RefreshZoneColor();
    }

    private void UpdateMovement()
    {
        bool hostileDetected = HasDetectedReaction(NPCReactionType.Hostile);

        if (stopMovingWhenHostileDetected && hostileDetected)
        {
            if (!agent.isStopped)
                agent.isStopped = true;

            return;
        }

        agent.isStopped = false;

        if (!canWander)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!waitingAtPoint)
            {
                waitingAtPoint = true;
                currentWaitTime = Random.Range(minWaitAtPoint, maxWaitAtPoint);
            }

            currentWaitTime -= Time.deltaTime;

            if (currentWaitTime <= 0f)
            {
                waitingAtPoint = false;
                MoveToRandomPoint();
            }
        }
    }

    private void UpdateLookAt()
    {
        if (!lookAtDetectedTarget || currentLookTarget == null)
            return;

        Vector3 targetPosition = currentLookTarget.position;
        Vector3 myPosition = transform.position;

        Vector3 direction = targetPosition - myPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }

    private void UpdateDetectionTimers()
    {
        currentLookTarget = null;

        List<Collider> toRemove = null;

        foreach (var kvp in trackedTargets)
        {
            DetectionProgress progress = kvp.Value;

            if (progress.target == null)
            {
                if (toRemove == null)
                    toRemove = new List<Collider>();

                toRemove.Add(kvp.Key);
                continue;
            }

            progress.timeInside += Time.deltaTime;

            if (!progress.fullyDetected && progress.timeInside >= progress.rule.detectionTime)
            {
                progress.fullyDetected = true;
                ReactToTarget(progress);
            }

            if (progress.fullyDetected &&
                (progress.rule.reactionType == NPCReactionType.Hostile ||
                 progress.rule.reactionType == NPCReactionType.Suspicious))
            {
                currentLookTarget = progress.target.transform;
            }
        }

        if (toRemove != null)
        {
            foreach (Collider col in toRemove)
                trackedTargets.Remove(col);
        }
    }

    private void ReactToTarget(DetectionProgress progress)
    {
        if (progress.target == null)
            return;

        GameObject targetObject = progress.target.gameObject;

        switch (progress.rule.reactionType)
        {
            case NPCReactionType.Friendly:
                onFriendlyDetected?.Invoke(targetObject);
                Debug.Log($"{name} detected FRIENDLY target: {targetObject.name}");
                break;

            case NPCReactionType.Suspicious:
                onSuspiciousDetected?.Invoke(targetObject);
                Debug.Log($"{name} detected SUSPICIOUS target: {targetObject.name}");
                break;

            case NPCReactionType.Hostile:
                onHostileDetected?.Invoke(targetObject);
                Debug.Log($"{name} detected HOSTILE target: {targetObject.name}");
                break;

            case NPCReactionType.Neutral:
                Debug.Log($"{name} detected NEUTRAL target: {targetObject.name}");
                break;

            case NPCReactionType.Ignore:
                break;
        }
    }

    private void RefreshZoneColor()
    {
        bool hasAnyTarget = trackedTargets.Count > 0;
        bool hasHostileDetected = HasDetectedReaction(NPCReactionType.Hostile);
        bool hasSuspiciousDetected = HasDetectedReaction(NPCReactionType.Suspicious);

        if (hasHostileDetected || hasSuspiciousDetected)
        {
            SetDangerState();
        }
        else if (hasAnyTarget)
        {
            SetWarningState();
        }
        else
        {
            SetNeutralState();
        }
    }

    private bool HasDetectedReaction(NPCReactionType reactionType)
    {
        foreach (var kvp in trackedTargets)
        {
            DetectionProgress progress = kvp.Value;
            if (progress.fullyDetected && progress.rule.reactionType == reactionType)
                return true;
        }

        return false;
    }

    private NPCTagDetection GetRuleForTag(string tagToCheck)
    {
        for (int i = 0; i < detectionRules.Count; i++)
        {
            if (detectionRules[i] != null && detectionRules[i].targetTag == tagToCheck)
                return detectionRules[i];
        }

        return null;
    }

    private void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void SetNeutralState()
    {
        detectionVisualizer?.SetState(NPCDetectionVisualizer.ZoneState.Neutral);
        onNeutralState?.Invoke();
    }

    private void SetWarningState()
    {
        detectionVisualizer?.SetState(NPCDetectionVisualizer.ZoneState.Warning);
        onWarningState?.Invoke();
    }

    private void SetDangerState()
    {
        detectionVisualizer?.SetState(NPCDetectionVisualizer.ZoneState.Danger);
        onDangerState?.Invoke();
    }

    public void HandleTargetEnter(Collider other)
    {
        if (other == null || other.isTrigger)
            return;

        NPCTagDetection rule = GetRuleForTag(other.tag);
        if (rule == null || rule.reactionType == NPCReactionType.Ignore)
            return;

        if (!trackedTargets.ContainsKey(other))
        {
            trackedTargets.Add(other, new DetectionProgress
            {
                target = other,
                timeInside = 0f,
                fullyDetected = false,
                rule = rule
            });
        }
    }

    public void HandleTargetStay(Collider other)
    {
        if (other == null || other.isTrigger)
            return;

        if (!trackedTargets.ContainsKey(other))
        {
            HandleTargetEnter(other);
        }
    }

    public void HandleTargetExit(Collider other)
    {
        if (other == null)
            return;

        if (trackedTargets.ContainsKey(other))
            trackedTargets.Remove(other);
    }
}