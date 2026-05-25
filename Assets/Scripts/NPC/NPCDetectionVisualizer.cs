using UnityEngine;

public class NPCDetectionVisualizer : MonoBehaviour
{
    public enum SignState
    {
        None,
        Warning,
        Detected,
        Hostile
    }

    [Header("UI Signs")]
    [SerializeField] private GameObject warningSign;
    [SerializeField] private GameObject detectedSign;
    [SerializeField] private GameObject hostileSign;

    private SignState currentState = SignState.None;

    private void Awake()
    {
        SetState(SignState.None);
    }

    public void SetState(SignState newState)
    {
        currentState = newState;

        if (warningSign != null)
            warningSign.SetActive(newState == SignState.Warning);

        if (detectedSign != null)
            detectedSign.SetActive(newState == SignState.Detected);

        if (hostileSign != null)
            hostileSign.SetActive(newState == SignState.Hostile);
    }

    public SignState GetState()
    {
        return currentState;
    }
}