using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerEquipment))]
public class PlayerWeaponContoller : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference aimAction;
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference reloadAction;

    [Header("References")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private Transform modelRoot;
    [SerializeField] private LayerMask mouseAimLayers = ~0;

    [Header("Aiming")]
    [SerializeField] private float mouseRayDistance = 200f;
    [SerializeField] private float rotateSpeed = 18f;

    private PlayerEquipment equipment;
    private WeaponDefinition currentWeapon;

    private int bulletsInMagazine;
    private int magazinesLeft;
    private float nextShootTime;
    private bool isReloading;

    private void Awake()
    {
        equipment = GetComponent<PlayerEquipment>();

        if (modelRoot == null && transform.childCount > 0)
            modelRoot = transform.GetChild(0);

        equipment.OnWeaponChanged += HandleWeaponChanged;

        if (equipment.EquippedWeapon != null)
            HandleWeaponChanged(equipment.EquippedWeapon);
    }

    private void OnEnable()
    {
        if (aimAction != null)
            aimAction.action.Enable();

        if (shootAction != null)
            shootAction.action.Enable();

        if (reloadAction != null)
            reloadAction.action.Enable();
    }

    private void OnDisable()
    {
        if (aimAction != null)
            aimAction.action.Disable();

        if (shootAction != null)
            shootAction.action.Disable();

        if (reloadAction != null)
            reloadAction.action.Disable();
    }

    private void Update()
    {
        if (currentWeapon == null)
            return;

        if (!equipment.CanUseWeapon)
            return;

        bool isAiming = aimAction != null && aimAction.action.IsPressed();

        if (isAiming)
            RotateTowardMouse();

        if (isReloading)
            return;

        if (reloadAction != null && reloadAction.action.WasPressedThisFrame())
        {
            TryReload();
            return;
        }

        if (!isAiming)
            return;

        if (shootAction != null && shootAction.action.WasPressedThisFrame())
            TryShoot();
    }

    private void HandleWeaponChanged(WeaponItemDefinition weaponItem)
    {
        currentWeapon = weaponItem != null ? weaponItem.weaponData : null;

        if (currentWeapon == null)
            return;

        bulletsInMagazine = currentWeapon.magazineSize;
        magazinesLeft = currentWeapon.magazines;
        isReloading = false;
        nextShootTime = 0f;
    }

    private void TryShoot()
    {
        if (currentWeapon == null)
            return;

        if (currentWeapon.bulletPrefab == null)
        {
            Debug.LogWarning("Weapon has no bullet prefab.");
            return;
        }

        if (muzzlePoint == null)
        {
            Debug.LogWarning("No muzzle point assigned.");
            return;
        }

        if (Time.time < nextShootTime)
            return;

        if (bulletsInMagazine <= 0)
        {
            TryReload();
            return;
        }

        Vector3 shootDirection = GetMouseAimDirection();

        for (int i = 0; i < currentWeapon.bulletsPerShot; i++)
        {
            Vector3 spreadDirection = ApplySpread(shootDirection, currentWeapon.spread);

            WeaponBullet bullet = Instantiate(
                currentWeapon.bulletPrefab,
                muzzlePoint.position,
                Quaternion.LookRotation(spreadDirection)
            );

            bullet.Initialize(
                gameObject,
                spreadDirection,
                currentWeapon.damage,
                currentWeapon.bulletSpeed,
                currentWeapon.range,
                currentWeapon.hitEffectPrefab
            );
        }

        bulletsInMagazine--;
        nextShootTime = Time.time + 1f / currentWeapon.fireRate;

        if (currentWeapon.muzzleFlashPrefab != null)
            Instantiate(currentWeapon.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);

        if (currentWeapon.shootSfx != null)
            AudioSource.PlayClipAtPoint(currentWeapon.shootSfx, transform.position);
    }

    private void TryReload()
    {
        if (currentWeapon == null)
            return;

        if (isReloading)
            return;

        if (bulletsInMagazine >= currentWeapon.magazineSize)
            return;

        if (magazinesLeft <= 0)
            return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        if (currentWeapon.reloadSfx != null)
            AudioSource.PlayClipAtPoint(currentWeapon.reloadSfx, transform.position);

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        magazinesLeft--;
        bulletsInMagazine = currentWeapon.magazineSize;

        isReloading = false;
    }

    private Vector3 GetMouseAimDirection()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return transform.forward;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, mouseRayDistance, mouseAimLayers))
        {
            targetPoint = hit.point;
        }
        else
        {
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            if (groundPlane.Raycast(ray, out float distance))
                targetPoint = ray.GetPoint(distance);
            else
                targetPoint = transform.position + transform.forward;
        }

        Vector3 direction = targetPoint - muzzlePoint.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return transform.forward;

        return direction.normalized;
    }

    private Vector3 ApplySpread(Vector3 direction, float spreadAngle)
    {
        if (spreadAngle <= 0f)
            return direction.normalized;

        float randomAngle = Random.Range(-spreadAngle, spreadAngle);
        Quaternion spreadRotation = Quaternion.Euler(0f, randomAngle, 0f);

        return spreadRotation * direction.normalized;
    }

    private void RotateTowardMouse()
    {
        Vector3 direction = GetMouseAimDirection();

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        if (modelRoot != null)
        {
            modelRoot.rotation = Quaternion.Slerp(
                modelRoot.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    public int GetBulletsInMagazine()
    {
        return bulletsInMagazine;
    }

    public int GetMagazinesLeft()
    {
        return magazinesLeft;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}