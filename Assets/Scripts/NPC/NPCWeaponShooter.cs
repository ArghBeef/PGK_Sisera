using System.Collections;
using UnityEngine;

public class NPCWeaponShooter : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponDefinition weapon;

    [Header("References")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private Transform modelRoot;

    [Header("Aiming")]
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float targetHeightOffset = 1f;

    [Header("Line Of Sight")]
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask lineOfSightLayers = ~0;

    private GameObject currentTarget;
    private int bulletsInMagazine;
    private int magazinesLeft;
    private float nextShootTime;
    private bool isReloading;

    private void Awake()
    {
        if (modelRoot == null)
            modelRoot = transform;

        ResetAmmo();
    }

    private void Update()
    {
        if (weapon == null)
            return;

        if (currentTarget == null)
            return;

        if (!currentTarget.activeInHierarchy)
        {
            ClearTarget();
            return;
        }

        if (currentTarget.CompareTag("Body"))
        {
            ClearTarget();
            return;
        }

        RotateTowardTarget();

        if (isReloading)
            return;

        if (!CanSeeTarget())
            return;

        TryShoot();
    }

    public void SetTarget(GameObject target)
    {
        if (target == null)
            return;

        currentTarget = target;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public void SetWeapon(WeaponDefinition newWeapon)
    {
        weapon = newWeapon;
        ResetAmmo();
    }

    private void ResetAmmo()
    {
        if (weapon == null)
            return;

        bulletsInMagazine = weapon.magazineSize;
        magazinesLeft = weapon.magazines;
        nextShootTime = 0f;
        isReloading = false;
    }

    private void TryShoot()
    {
        if (weapon == null)
            return;

        if (weapon.bulletPrefab == null)
        {
            Debug.LogWarning($"{name} has no bullet prefab assigned in weapon.");
            return;
        }

        if (muzzlePoint == null)
        {
            Debug.LogWarning($"{name} has no muzzle point assigned.");
            return;
        }

        if (Time.time < nextShootTime)
            return;

        if (bulletsInMagazine <= 0)
        {
            TryReload();
            return;
        }

        Vector3 shootDirection = GetDirectionToTarget();

        for (int i = 0; i < weapon.bulletsPerShot; i++)
        {
            Vector3 spreadDirection = ApplySpread(shootDirection, weapon.spread);

            WeaponBullet bullet = Instantiate(
                weapon.bulletPrefab,
                muzzlePoint.position,
                Quaternion.LookRotation(spreadDirection)
            );

            bullet.Initialize(
                gameObject,
                spreadDirection,
                weapon.damage,
                weapon.bulletSpeed,
                weapon.range,
                weapon.hitEffectPrefab
            );
        }

        bulletsInMagazine--;
        nextShootTime = Time.time + 1f / weapon.fireRate;

        if (weapon.muzzleFlashPrefab != null)
            Instantiate(weapon.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);

        if (weapon.shootSfx != null)
            AudioSource.PlayClipAtPoint(weapon.shootSfx, transform.position);
    }

    private void TryReload()
    {
        if (weapon == null)
            return;

        if (isReloading)
            return;

        if (bulletsInMagazine >= weapon.magazineSize)
            return;

        if (magazinesLeft <= 0)
            return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        if (weapon.reloadSfx != null)
            AudioSource.PlayClipAtPoint(weapon.reloadSfx, transform.position);

        yield return new WaitForSeconds(weapon.reloadTime);

        magazinesLeft--;
        bulletsInMagazine = weapon.magazineSize;

        isReloading = false;
    }

    private Vector3 GetTargetPoint()
    {
        Collider targetCollider = currentTarget.GetComponent<Collider>();

        if (targetCollider == null)
            targetCollider = currentTarget.GetComponentInChildren<Collider>();

        if (targetCollider != null)
            return targetCollider.bounds.center;

        return currentTarget.transform.position + Vector3.up * targetHeightOffset;
    }

    private Vector3 GetDirectionToTarget()
    {
        Vector3 targetPoint = GetTargetPoint();
        Vector3 direction = targetPoint - muzzlePoint.position;

        if (direction.sqrMagnitude < 0.01f)
            return transform.forward;

        return direction.normalized;
    }

    private Vector3 ApplySpread(Vector3 direction, float spreadAngle)
    {
        if (spreadAngle <= 0f)
            return direction.normalized;

        float randomYaw = Random.Range(-spreadAngle, spreadAngle);
        float randomPitch = Random.Range(-spreadAngle, spreadAngle);

        Quaternion spreadRotation = Quaternion.Euler(randomPitch, randomYaw, 0f);
        return spreadRotation * direction.normalized;
    }

    private void RotateTowardTarget()
    {
        Vector3 targetPoint = GetTargetPoint();
        Vector3 direction = targetPoint - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        modelRoot.rotation = Quaternion.Slerp(
            modelRoot.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    private bool CanSeeTarget()
    {
        if (!requireLineOfSight)
            return true;

        if (currentTarget == null || muzzlePoint == null)
            return false;

        Vector3 targetPoint = GetTargetPoint();
        Vector3 direction = targetPoint - muzzlePoint.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(muzzlePoint.position, direction.normalized, out RaycastHit hit, distance, lineOfSightLayers))
        {
            if (hit.collider.gameObject == currentTarget)
                return true;

            if (hit.collider.GetComponentInParent<Transform>() == currentTarget.transform)
                return true;

            if (hit.collider.GetComponentInParent<IDamageable>() != null &&
                currentTarget.GetComponentInParent<IDamageable>() != null)
                return true;

            return false;
        }

        return true;
    }
}