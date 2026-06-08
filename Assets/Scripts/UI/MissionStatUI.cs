using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionResultUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool shown;

    private void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.onMissionCompleted.AddListener(ShowSuccess);
            MissionManager.Instance.onMissionFailed.AddListener(ShowFail);
        }
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.onMissionCompleted.RemoveListener(ShowSuccess);
            MissionManager.Instance.onMissionFailed.RemoveListener(ShowFail);
        }
    }

    public void ShowSuccess()
    {
        ShowResult(true);
    }

    public void ShowFail()
    {
        ShowResult(false);
    }

    private void ShowResult(bool success)
    {
        if (shown)
            return;

        shown = true;

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (titleText != null)
            titleText.text = success ? "MISSION SUCCESS" : "MISSION FAILED";

        MissionStatsTracker stats = MissionStatsTracker.Instance;

        if (stats != null && statsText != null)
        {
            statsText.text =
                "Enemies killed: " + stats.EnemiesKilled + "\n" +
                "Mission time: " + FormatTime(stats.MissionTimePlayed) + "\n" +
                "Points: " + stats.Points + "\n" +
                "Most used ability: " + stats.GetMostUsedAbility() + "\n" +
                "Damage taken: " + Mathf.RoundToInt(stats.DamageTaken);
        }

        Time.timeScale = 0f;
    }

    public void RestartMission()
    {
        Time.timeScale = 1f;

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}