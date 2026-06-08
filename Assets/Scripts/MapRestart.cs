using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionRestart : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

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
}