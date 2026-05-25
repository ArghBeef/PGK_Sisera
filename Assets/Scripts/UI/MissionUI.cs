using TMPro;
using UnityEngine;

public class MissionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text missionNameText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text objectiveText;

    private void Update()
    {
        MissionManager manager = MissionManager.Instance;

        if (manager == null || manager.CurrentMission == null)
            return;

        MissionDefinition mission = manager.CurrentMission;

        if (missionNameText != null)
            missionNameText.text = mission.missionName;

        if (timerText != null)
        {
            string label = "Mission Time";

            if (manager.CurrentTimerMode == MissionManager.TimerMode.Hold)
                label = "Hold Time";

            if (manager.CurrentTimerMode == MissionManager.TimerMode.Escape)
                label = "Escape Time";

            timerText.text = FormatTime(manager.MissionTimer);
        }

        if (objectiveText != null)
        {
            if (mission.missionType == MissionType.Demolition)
            {
                if (manager.CurrentTimerMode == MissionManager.TimerMode.Hold)
                    objectiveText.text = "Hold the captured area.";
                else
                    objectiveText.text = "Capture the demolition area.";
            }

            if (mission.missionType == MissionType.Assassination)
            {
                if (manager.CurrentTimerMode == MissionManager.TimerMode.Escape)
                    objectiveText.text = "Escape!";
                else
                    objectiveText.text = "Kill the target.";
            }
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}