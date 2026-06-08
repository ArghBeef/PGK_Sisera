using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class DemolitionCaptureZone : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string npcTag = "NPC";

    [Header("Capture")]
    [SerializeField] private bool playerInside;
    [SerializeField] private bool npcInside;
    [SerializeField] private bool captured;
    [SerializeField] private float captureProgress;

    [Header("NPC Suspicion")]
    [SerializeField] private bool reportSuspicionWhileCapturing = true;
    [SerializeField] private float suspicionReportInterval = 1f;
    [SerializeField] private float suspicionRadius = 15f;
    [SerializeField] private float suspicionWaitTime = 3f;

    [Header("Events")]
    public UnityEvent onCaptureStarted;
    public UnityEvent onCaptured;
    public UnityEvent onCapturedBackByNPC;
    [SerializeField] private int capturePoints = 50;
    [SerializeField] private int holdCompletePoints = 100;

    public float CaptureProgress => captureProgress;
    public bool Captured => captured;

    private bool captureStarted;
    private float suspicionReportTimer;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        MissionManager manager = MissionManager.Instance;

        if (manager == null || manager.CurrentMission == null)
            return;

        if (!manager.MissionActive)
            return;

        if (manager.CurrentMission.missionType != MissionType.Demolition)
            return;

        if (npcInside && captured)
        {
            NPCRecaptureZone(manager);
            return;
        }

        if (!playerInside)
            return;

        if (captured)
            return;

        ReportSuspicionIfNeeded();

        UpdatePlayerCapture(manager.CurrentMission.captureTime);
    }

    private void UpdatePlayerCapture(float requiredTime)
    {
        if (!captureStarted)
        {
            captureStarted = true;
            onCaptureStarted?.Invoke();
        }

        captureProgress += Time.deltaTime;

        if (captureProgress >= requiredTime)
        {
            captureProgress = requiredTime;
            captured = true;

            onCaptured?.Invoke();
            GivePointsToPlayer(capturePoints);

            MissionManager.Instance.StartDemolitionHoldTimer();
        }
    }

    private void NPCRecaptureZone(MissionManager manager)
    {
        captured = false;
        captureStarted = false;
        captureProgress = 0f;

        manager.CancelDemolitionHoldTimer();

        onCapturedBackByNPC?.Invoke();

        Debug.Log("NPC recaptured demolition zone.");
    }

    private void ReportSuspicionIfNeeded()
    {
        if (!reportSuspicionWhileCapturing)
            return;

        suspicionReportTimer -= Time.deltaTime;

        if (suspicionReportTimer > 0f)
            return;

        suspicionReportTimer = suspicionReportInterval;

        NPCSuspiciousEventSystem.Report(
            transform.position,
            gameObject,
            suspicionRadius,
            suspicionWaitTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInside = true;

        if (other.CompareTag(npcTag))
            npcInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInside = false;

        if (other.CompareTag(npcTag))
            npcInside = false;
    }

    private void GivePointsToPlayer(int amount)
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
            return;

        PlayerPoints points = player.GetComponent<PlayerPoints>();

        if (points == null)
            points = player.GetComponentInParent<PlayerPoints>();

        if (points != null)
            points.AddPoints(amount);
    }
}