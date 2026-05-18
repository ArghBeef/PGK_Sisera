using UnityEngine;

[RequireComponent(typeof(Health))]
public class RamDecoy : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifetime = 8f;

    [Header("Suspicious Event")]
    [SerializeField] private float suspiciousRadius = 12f;
    [SerializeField] private float waitTime = 5f;
    [SerializeField] private bool reportOnStart = true;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 40f;
    [SerializeField] private float pushForce = 7f;
    [SerializeField] private float upwardPush = 1.2f;
    [SerializeField] private LayerMask affectedLayers = ~0;
    [SerializeField] private GameObject explosionEffectPrefab;

    private bool exploded;

    private void Start()
    {
        if (reportOnStart)
            ReportSuspiciousEvent();

        Destroy(gameObject, lifetime);
    }

    public void ReportSuspiciousEvent()
    {
        NPCSuspiciousEventSystem.Report(
            transform.position,
            gameObject,
            suspiciousRadius,
            waitTime
        );
    }

    private void OnDestroy()
    {
        Explode();
    }

    private void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius,
            affectedLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.transform.root == transform.root)
                continue;

            Health health = hit.GetComponentInParent<Health>();

            if (health != null && !health.IsDead)
                health.TakeDamage(explosionDamage);

            Rigidbody rb = hit.attachedRigidbody;

            if (rb == null)
                rb = hit.GetComponentInParent<Rigidbody>();

            if (rb != null && !rb.isKinematic)
            {
                Vector3 direction = rb.worldCenterOfMass - transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude < 0.01f)
                    direction = Random.insideUnitSphere;

                direction.y = upwardPush;
                direction.Normalize();

                rb.AddForce(direction * pushForce, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, suspiciousRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}