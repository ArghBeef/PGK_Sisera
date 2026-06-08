using UnityEngine;

public class PlayerMissionFail : MonoBehaviour
{
    public void FailMission()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.FailMission();
    }
}