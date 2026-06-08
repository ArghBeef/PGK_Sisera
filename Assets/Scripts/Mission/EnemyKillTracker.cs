using UnityEngine;

public class EnemyKillTracker : MonoBehaviour
{
    public void RegisterKill()
    {
        if (MissionStatsTracker.Instance != null)
            MissionStatsTracker.Instance.AddEnemyKill();
    }
}