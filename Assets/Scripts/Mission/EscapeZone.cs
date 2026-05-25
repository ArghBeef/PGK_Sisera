using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EscapeZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (MissionManager.Instance == null)
            return;

        MissionManager.Instance.TryEscape();
    }
}