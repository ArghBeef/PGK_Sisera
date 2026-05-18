using UnityEngine;

public class SuspiciousSource : MonoBehaviour
{
    [SerializeField] private float radius = 12f;
    [SerializeField] private float waitTime = 3f;
    [SerializeField] private float reportInterval = 1f;
    [SerializeField] private bool reportOnStart = true;

    private float timer;

    private void Start()
    {
        if (reportOnStart)
            Report();

        timer = reportInterval;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Report();
            timer = reportInterval;
        }
    }

    private void Report()
    {
        NPCSuspiciousEventSystem.Report(
            transform.position,
            gameObject,
            radius,
            waitTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}