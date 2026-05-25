using UnityEngine;
using UnityEngine.Events;

public class AssassinationTarget : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool killed;

    [Header("Events")]
    public UnityEvent onTargetKilled;

    public bool Killed => killed;

    public void MarkKilled()
    {
        if (killed)
            return;

        killed = true;

        onTargetKilled?.Invoke();

        if (MissionManager.Instance != null)
            MissionManager.Instance.NotifyAssassinationTargetKilled();

        Debug.Log("Assassination target killed: " + gameObject.name);
    }
}