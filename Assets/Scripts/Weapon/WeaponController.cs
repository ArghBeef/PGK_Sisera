using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PC_Movement))]
public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponDefinition equippedWeapon;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private AudioSource audioSource;

    [Header("Input")]
    [SerializeField] private InputActionReference aimAction;
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference reloadAction;

    private PC_Movement movement;
    private Camera mainCamera;

    private int currentAmmoInMagazine;
    private int magazinesLeft;
    private float nextFireTime;
    private bool isReloading;

    public WeaponDefinition EquippedWeapon => equippedWeapon;
    public int CurrentAmmoInMagazine => currentAmmoInMagazine;
    public int MagazinesLeft => magazinesLeft;
    public bool IsAiming => aimAction != null && aimAction.action.IsPressed();

    private void Awake()
    {
        movement = GetComponent<PC_Movement>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        EquipWeapon(equippedWeapon);
    }

    private void OnEnable()
    {
        aimAction?.action.Enable();
        shootAction?.action.Enable();
        reloadAction?.action.Enable();
    }

    private void OnDisable()
    {
        aimAction?.action.Disable();
        shootAction?.action.Disable();
        reloadAction?.action.Disable();
    }

    private void Update()
    {
        if (equippedWeapon == null)
        {
            movement.SetMovementLocked(false);
            return;
        }

        HandleAimState();

        if (!IsAiming)
            return;

        RotateTowardsMouse();

        if (reloadAction != null && reloadAction.action.triggered)
        {
            TryStartReload();
        }

        if (shootAction != null && shootAction.action.IsPressed())
        {
            TryShoot();
        }
    }

    public void EquipWeapon(WeaponDefinition weapon)
    {
        equippedWeapon = weapon;

        if (equippedWeapon == null)
        {
            currentAmmoInMagazine = 0;
            magazinesLeft = 0;
            return;
        }

        currentAmmoInMagazine = equippedWeapon.magazineSize;
        magazinesLeft = equippedWeapon.magazines;
        nextFireTime = 0f;
        isReloading = false;
    }

    private void HandleAimState()
    {
        bool aiming = IsAiming;
        movement.SetMovementLocked(aiming);
    }

    private void RotateTowardsMouse()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimLayerMask))
        {
            Vector3 lookPoint = hit.point;
            lookPoint.y = transform.position.y;

            Vector3 direction = lookPoint - transform.position;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = targetRotation;
        }
    }

    private void TryShoot()
    {
        if (isReloading)
            return;

        if (Time.time < nextFireTime)
            return;

        if (currentAmmoInMagazine <= 0)
        {
            TryStartReload();
            return;
        }

        FireShot();
        currentAmmoInMagazine--;
        nextFireTime = Time.time + (1f / equippedWeapon.fireRate);

        if (currentAmmoInMagazine <= 0)
        {
            TryStartReload();
        }
    }

    private void FireShot()
    {
        Vector3 aimDirection = GetAimDirection();

        for (int i = 0; i < equippedWeapon.bulletsPerShot; i++)
        {
            Vector3 shotDirection = ApplySpread(aimDirection, equippedWeapon.spread);

            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position + transform.forward * 0.5f;

            if (Physics.Raycast(origin, shotDirection, out RaycastHit hit, equippedWeapon.range, hitMask))
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(equippedWeapon.damage);
                }

                if (equippedWeapon.hitEffectPrefab != null)
                {
                    Instantiate(
                        equippedWeapon.hitEffectPrefab,
                        hit.point,
                        Quaternion.LookRotation(hit.normal)
                    );
                }
            }
        }

        if (equippedWeapon.muzzleFlashPrefab != null && muzzlePoint != null)
        {
            Instantiate(
                equippedWeapon.muzzleFlashPrefab,
                muzzlePoint.position,
                muzzlePoint.rotation
            );
        }

        if (audioSource != null && equippedWeapon.shootSfx != null)
        {
            audioSource.PlayOneShot(equippedWeapon.shootSfx);
        }
    }

    private Vector3 GetAimDirection()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimLayerMask))
        {
            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Vector3 direction = (hit.point - origin).normalized;

            if (direction.sqrMagnitude > 0.0001f)
                return direction;
        }

        return transform.forward;
    }

    private Vector3 ApplySpread(Vector3 direction, float spreadAngle)
    {
        if (spreadAngle <= 0f)
            return direction.normalized;

        float yaw = Random.Range(-spreadAngle, spreadAngle);
        float pitch = Random.Range(-spreadAngle, spreadAngle);

        Quaternion spreadRotation = Quaternion.Euler(pitch, yaw, 0f);
        return (spreadRotation * direction).normalized;
    }

    private void TryStartReload()
    {
        if (isReloading)
            return;

        if (equippedWeapon == null)
            return;

        if (currentAmmoInMagazine >= equippedWeapon.magazineSize)
            return;

        if (magazinesLeft <= 0)
            return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        if (audioSource != null && equippedWeapon.reloadSfx != null)
        {
            audioSource.PlayOneShot(equippedWeapon.reloadSfx);
        }

        yield return new WaitForSeconds(equippedWeapon.reloadTime);

        currentAmmoInMagazine = equippedWeapon.magazineSize;
        magazinesLeft--;
        isReloading = false;
    }
}