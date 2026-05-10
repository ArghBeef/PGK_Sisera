using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponBullet : MonoBehaviour
{
    private float damage;
    private float speed;
    private float range;
    private Vector3 startPosition;
    private Vector3 direction;
    private GameObject owner;
    private GameObject hitEffectPrefab;

    public void Initialize(
        GameObject bulletOwner,
        Vector3 shootDirection,
        float bulletDamage,
        float bulletSpeed,
        float bulletRange,
        GameObject hitEffect)
    {
        owner = bulletOwner;
        direction = shootDirection.normalized;
        damage = bulletDamage;
        speed = bulletSpeed;
        range = bulletRange;
        hitEffectPrefab = hitEffect;
        startPosition = transform.position;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner)
            return;

        if (other.isTrigger)
            return;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null)
            damageable.TakeDamage(damage);

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}