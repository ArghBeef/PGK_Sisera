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

    [Header("Optional")]
    [SerializeField] private bool billboardToCamera = true;
    [SerializeField] private Transform targetCamera;

    private SignState currentState = SignState.None;

    private void Awake()
    {
        SetState(SignState.None);
    }

    private void LateUpdate()
    {
        if (!billboardToCamera)
            return;

        Camera cam = null;

        if (targetCamera != null)
        {
            cam = targetCamera.GetComponent<Camera>();
        }

        if (cam == null)
            cam = Camera.main;

        if (cam == null)
            return;

        Vector3 direction = transform.position - cam.transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
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