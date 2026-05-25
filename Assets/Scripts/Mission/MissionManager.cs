using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    public enum TimerMode
    {
        Mission,
        Hold,
        Escape
    }

    [Header("Mission")]
    [SerializeField] private MissionDefinition currentMission;
    [SerializeField] private bool startOnAwake = true;

    [Header("State")]
    [SerializeField] private bool missionActive;
    [SerializeField] private bool missionCompleted;
    [SerializeField] private bool missionFailed;
    [SerializeField] private TimerMode currentTimerMode;

    [Header("Timer")]
    [SerializeField] private float missionTimer;
    private float storedMissionTimerBeforeHold;

    [Header("Events")]
    public UnityEvent onMissionStarted;
    public UnityEvent onMissionCompleted;
    public UnityEvent onMissionFailed;
    public UnityEvent onDemolitionCaptured;
    public UnityEvent onAssassinationTargetKilled;

    public MissionDefinition CurrentMission => currentMission;
    public bool MissionActive => missionActive;
    public bool MissionCompleted => missionCompleted;
    public bool MissionFailed => missionFailed;
    public TimerMode CurrentTimerMode => currentTimerMode;
    public float MissionTimer => missionTimer;

    private bool assassinationTargetKilled;

    private void Awake()
    {
        Instance = this;

        if (startOnAwake && currentMission != null)
            StartMission(currentMission);
    }

    private void Update()
    {
        if (!missionActive || missionCompleted || missionFailed)
            return;

        missionTimer -= Time.deltaTime;

        if (missionTimer <= 0f)
        {
            missionTimer = 0f;

            if (currentTimerMode == TimerMode.Hold)
                CompleteMission();
            else
                FailMission();
        }
    }

    public void StartMission(MissionDefinition mission)
    {
        if (mission == null)
            return;

        currentMission = mission;

        missionActive = true;
        missionCompleted = false;
        missionFailed = false;
        assassinationTargetKilled = false;

        currentTimerMode = TimerMode.Mission;
        missionTimer = currentMission.missionTimeLimit;

        onMissionStarted?.Invoke();
    }

    public void StartDemolitionHoldTimer()
    {
        if (!CanUseMissionType(MissionType.Demolition))
            return;

        storedMissionTimerBeforeHold = missionTimer;

        currentTimerMode = TimerMode.Hold;
        missionTimer = currentMission.holdTime;

        onDemolitionCaptured?.Invoke();

        Debug.Log("Zone captured. Timer changed to hold timer.");
    }

    public void CancelDemolitionHoldTimer()
    {
        if (!CanUseMissionType(MissionType.Demolition))
            return;

        if (currentTimerMode != TimerMode.Hold)
            return;

        currentTimerMode = TimerMode.Mission;
        missionTimer = storedMissionTimerBeforeHold;

        Debug.Log("NPC captured zone back. Timer returned to mission timer.");
    }

    public void NotifyAssassinationTargetKilled()
    {
        if (!CanUseMissionType(MissionType.Assassination))
            return;

        if (assassinationTargetKilled)
            return;

        assassinationTargetKilled = true;

        currentTimerMode = TimerMode.Escape;
        missionTimer = currentMission.escapeTimeAfterKill;

        onAssassinationTargetKilled?.Invoke();

        Debug.Log("Mission timer changed to escape timer: " + missionTimer);
    }

    public void TryEscape()
    {
        if (!CanUseMissionType(MissionType.Assassination))
            return;

        if (!assassinationTargetKilled)
            return;

        CompleteMission();
    }

    private bool CanUseMissionType(MissionType type)
    {
        if (!missionActive || missionCompleted || missionFailed)
            return false;

        if (currentMission == null)
            return false;

        return currentMission.missionType == type;
    }

    private void CompleteMission()
    {
        if (missionCompleted || missionFailed)
            return;

        missionCompleted = true;
        missionActive = false;

        onMissionCompleted?.Invoke();

        Debug.Log("Mission completed.");
    }

    private void FailMission()
    {
        if (missionCompleted || missionFailed)
            return;

        missionFailed = true;
        missionActive = false;

        onMissionFailed?.Invoke();

        Debug.Log("Mission failed.");
    }
}