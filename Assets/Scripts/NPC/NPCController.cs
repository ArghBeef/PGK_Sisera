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
        public float timeVisible;
        public bool detected;
        public NPCTagDetection rule;
    }

    private enum NPCDetectionState
    {
        None,
        Warning,
        Detect,
        Hostile
    }

    [Header("Wandering")]
    [SerializeField] private bool canWander = true;
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float minWaitAtPoint = 1f;
    [SerializeField] private float maxWaitAtPoint = 3f;
    [SerializeField] private float stoppingDistance = 0.2f;

    [Header("Detection Box")]
    [SerializeField] private Vector3 detectionBoxSize = new Vector3(4f, 2f, 6f);
    [SerializeField] private Vector3 detectionBoxOffset = new Vector3(0f, 1f, 3f);
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private List<NPCTagDetection> detectionRules = new List<NPCTagDetection>();

    [Header("Line Of Sight")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private float targetHeight = 1f;
    [SerializeField] private LayerMask visionBlockLayers;

    [Header("Suspicious Events")]
    [SerializeField] private bool canReactToSuspiciousEvents = true;
    [SerializeField] private float suspiciousStoppingDistance = 1.2f;
    [SerializeField] private float suspiciousWaitTime = 3f;
    [SerializeField] private float suspiciousInvestigationRadius = 3f;
    [SerializeField] private float suspiciousHostileCheckRadius = 2f;

    [Header("Behaviour")]
    [SerializeField] private bool stopMovingWhenHostileDetected = true;
    [SerializeField] private bool lookAtDetectedTarget = true;
    [SerializeField] private float lookSpeed = 8f;

    [Header("Visual")]
    [SerializeField] private NPCDetectionVisualizer signVisualizer;

    [Header("Events")]
    [SerializeField] private GameObjectEvent onDetect;
    [SerializeField] private GameObjectEvent onWarning;
    [SerializeField] private GameObjectEvent onHostile;

    private NavMeshAgent agent;
    private readonly Dictionary<Collider, DetectionProgress> trackedTargets = new();

    private float currentWaitTime;
    private bool waitingAtPoint;
    private Transform currentLookTarget;
    private NPCDetectionState currentState = NPCDetectionState.None;

    private bool isInvestigatingSuspiciousPlace;
    private Vector3 suspiciousPlace;
    private Vector3 suspiciousSourcePosition;
    private float suspiciousWaitTimer;
    private bool reachedSuspiciousPlace;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;

        SetState(NPCDetectionState.None, null);
    }

    private void OnEnable()
    {
        NPCSuspiciousEventSystem.OnSuspiciousEvent += HandleSuspiciousEvent;
    }

    private void OnDisable()
    {
        NPCSuspiciousEventSystem.OnSuspiciousEvent -= HandleSuspiciousEvent;
    }

    private void Update()
    {
        ScanDetectionBox();
        UpdateDetectionTimers();
        UpdateMovement();
        UpdateLookAt();
        RefreshState();
    }

    private void ScanDetectionBox()
    {
        HashSet<Collider> currentlyInside = new HashSet<Collider>();

        Vector3 center = transform.TransformPoint(detectionBoxOffset);
        Vector3 halfExtents = detectionBoxSize * 0.5f;

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            transform.rotation,
            targetLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit == null || hit.transform.root == transform.root)
                continue;

            NPCTagDetection rule = GetRuleForTag(hit.tag);

            if (rule == null || rule.reactionType == NPCReactionType.Ignore)
                continue;

            currentlyInside.Add(hit);

            if (!trackedTargets.ContainsKey(hit))
            {
                trackedTargets.Add(hit, new DetectionProgress
                {
                    target = hit,
                    timeVisible = 0f,
                    detected = false,
                    rule = rule
                });
            }
        }

        List<Collider> toRemove = new List<Collider>();

        foreach (var kvp in trackedTargets)
        {
            if (!currentlyInside.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }

        foreach (Collider col in toRemove)
            trackedTargets.Remove(col);
    }

    private void UpdateDetectionTimers()
    {
        currentLookTarget = null;

        foreach (var kvp in trackedTargets)
        {
            DetectionProgress progress = kvp.Value;

            if (progress.target == null)
                continue;

            bool canSee = CanSeeTarget(progress.target);

            if (!canSee)
            {
                progress.timeVisible = 0f;
                continue;
            }

            progress.timeVisible += Time.deltaTime;

            if (!progress.detected && progress.timeVisible >= progress.rule.detectionTime)
            {
                progress.detected = true;
                ReactToTarget(progress);
            }

            if (progress.detected &&
                (progress.rule.reactionType == NPCReactionType.Hostile ||
                 progress.rule.reactionType == NPCReactionType.Suspicious))
            {
                currentLookTarget = progress.target.transform;
            }
        }
    }

    private bool CanSeeTarget(Collider target)
    {
        if (target == null)
            return false;

        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * eyeHeight;

        Vector3 targetPoint = target.bounds.center;
        targetPoint.y = target.transform.position.y + targetHeight;

        Vector3 direction = targetPoint - origin;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
            return true;

        if (Physics.Raycast(
            origin,
            direction.normalized,
            out RaycastHit hit,
            distance,
            visionBlockLayers,
            QueryTriggerInteraction.Ignore
        ))
        {
            if (hit.collider != target && !hit.collider.transform.IsChildOf(target.transform))
                return false;
        }

        return true;
    }

    private void ReactToTarget(DetectionProgress progress)
    {
        if (progress.target == null)
            return;

        GameObject targetObject = progress.target.gameObject;

        NPCMutualDialogueTrigger dialogueTrigger = GetComponent<NPCMutualDialogueTrigger>();

        if (dialogueTrigger != null)
        {
            dialogueTrigger.TryDialogueWith(targetObject);

            NPCDialogueController myDialogue = GetComponent<NPCDialogueController>();

            if (myDialogue != null && myDialogue.IsInDialogue)
                return;
        }

        if (progress.rule.reactionType == NPCReactionType.Hostile)
            SetState(NPCDetectionState.Hostile, targetObject);
        else
            SetState(NPCDetectionState.Detect, targetObject);
    }

    private void RefreshState()
    {
        if (isInvestigatingSuspiciousPlace)
            return;

        GameObject visible = GetVisibleTarget();
        GameObject detected = GetDetectedTarget();
        GameObject hostile = GetDetectedTarget(NPCReactionType.Hostile);

        if (hostile != null)
            SetState(NPCDetectionState.Hostile, hostile);
        else if (detected != null)
            SetState(NPCDetectionState.Detect, detected);
        else if (visible != null)
            SetState(NPCDetectionState.Warning, visible);
        else
            SetState(NPCDetectionState.None, null);
    }

    private void SetState(NPCDetectionState newState, GameObject target)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (newState)
        {
            case NPCDetectionState.None:
                signVisualizer?.SetState(NPCDetectionVisualizer.SignState.None);
                break;

            case NPCDetectionState.Warning:
                signVisualizer?.SetState(NPCDetectionVisualizer.SignState.Warning);
                onWarning?.Invoke(target);
                break;

            case NPCDetectionState.Detect:
                signVisualizer?.SetState(NPCDetectionVisualizer.SignState.Detected);
                onDetect?.Invoke(target);
                break;

            case NPCDetectionState.Hostile:
                signVisualizer?.SetState(NPCDetectionVisualizer.SignState.Hostile);
                onHostile?.Invoke(target);
                break;
        }
    }

    private void HandleSuspiciousEvent(Vector3 position, GameObject source, float radius, float waitTime)
    {
        if (!canReactToSuspiciousEvents)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        float distance = Vector3.Distance(transform.position, position);

        if (distance > radius)
            return;

        Vector3 investigationTarget = GetRandomPointAroundSuspiciousPosition(position);

        if (NavMesh.SamplePosition(investigationTarget, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            isInvestigatingSuspiciousPlace = true;
            suspiciousSourcePosition = position;
            suspiciousPlace = hit.position;
            suspiciousWaitTimer = waitTime > 0f ? waitTime : suspiciousWaitTime;
            reachedSuspiciousPlace = false;

            waitingAtPoint = false;

            agent.isStopped = false;
            agent.stoppingDistance = suspiciousStoppingDistance;
            agent.SetDestination(suspiciousPlace);

            SetState(NPCDetectionState.Warning, source);
        }
    }

    private Vector3 GetRandomPointAroundSuspiciousPosition(Vector3 position)
    {
        Vector3 randomOffset = Random.insideUnitSphere * suspiciousInvestigationRadius;
        randomOffset.y = 0f;

        return position + randomOffset;
    }

    private void UpdateSuspiciousMovement()
    {
        GameObject hostile = GetDetectedTarget(NPCReactionType.Hostile);

        if (hostile == null)
            hostile = GetHostileNearSuspiciousPlace();

        if (hostile != null)
        {
            SetState(NPCDetectionState.Hostile, hostile);
            isInvestigatingSuspiciousPlace = false;
            reachedSuspiciousPlace = false;
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        if (!reachedSuspiciousPlace)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                reachedSuspiciousPlace = true;
                agent.isStopped = true;
                SetState(NPCDetectionState.Warning, null);
            }

            return;
        }

        suspiciousWaitTimer -= Time.deltaTime;

        if (suspiciousWaitTimer <= 0f)
            FinishSuspiciousInvestigation();
    }

    private GameObject GetHostileNearSuspiciousPlace()
    {
        Collider[] hits = Physics.OverlapSphere(
            suspiciousSourcePosition,
            suspiciousHostileCheckRadius,
            targetLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.transform.root == transform.root)
                continue;

            NPCTagDetection rule = GetRuleForTag(hit.tag);

            if (rule == null)
                continue;

            if (rule.reactionType != NPCReactionType.Hostile)
                continue;

            if (!CanSeeTarget(hit))
                continue;

            return hit.gameObject;
        }

        return null;
    }

    private void FinishSuspiciousInvestigation()
    {
        isInvestigatingSuspiciousPlace = false;
        reachedSuspiciousPlace = false;

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;

        SetState(NPCDetectionState.None, null);
    }

    private GameObject GetVisibleTarget()
    {
        foreach (var kvp in trackedTargets)
        {
            if (kvp.Value.target != null && CanSeeTarget(kvp.Value.target))
                return kvp.Value.target.gameObject;
        }

        return null;
    }

    private GameObject GetDetectedTarget(NPCReactionType type = NPCReactionType.Neutral)
    {
        foreach (var kvp in trackedTargets)
        {
            DetectionProgress p = kvp.Value;

            if (p.target != null &&
                p.detected &&
                CanSeeTarget(p.target) &&
                (type == NPCReactionType.Neutral || p.rule.reactionType == type))
            {
                return p.target.gameObject;
            }
        }

        return null;
    }

    private NPCTagDetection GetRuleForTag(string tagToCheck)
    {
        foreach (var rule in detectionRules)
        {
            if (rule != null && rule.targetTag == tagToCheck)
                return rule;
        }

        return null;
    }

    private void UpdateMovement()
    {
        if (isInvestigatingSuspiciousPlace)
        {
            UpdateSuspiciousMovement();
            return;
        }

        if (stopMovingWhenHostileDetected && GetDetectedTarget(NPCReactionType.Hostile) != null)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;

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

        Vector3 dir = currentLookTarget.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, lookSpeed * Time.deltaTime);
    }

    private void MoveToRandomPoint()
    {
        Vector3 random = Random.insideUnitSphere * wanderRadius + transform.position;
        random.y = transform.position.y;

        if (NavMesh.SamplePosition(random, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = transform.TransformPoint(detectionBoxOffset);

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, detectionBoxSize);
        Gizmos.matrix = old;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, suspiciousInvestigationRadius);

        Vector3 eye = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(eye, 0.1f);
    }
}