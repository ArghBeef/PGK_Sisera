using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapRestart : MonoBehaviour
{
    [Header("Restart")]
    [SerializeField] private float restartDelay = 3f;

    [SerializeField] private TMP_Text restartText;

    private bool restarting;
    private float countdown;


    private void Update()
    {
        if (!restarting)
            return;

        countdown -= Time.deltaTime;

        if (restartText != null)
            restartText.text =  " " + Mathf.CeilToInt(countdown);

        if (countdown <= 0f)
            RestartMission();
    }

    public void RestartMission()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void RestartMissionDelayed()
    {
        if (restarting)
            return;

        restarting = true;
        countdown = restartDelay;


        if (restartText != null)
            restartText.text = " " + Mathf.CeilToInt(countdown);
    }

    public void RestartMissionByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}