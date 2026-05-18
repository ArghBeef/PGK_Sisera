using UnityEngine;

public class CrowCloneController : MonoBehaviour
{
    private GameObject activeClone;

    private float placeCooldownTimer;
    private float teleportCooldownTimer;

    public bool HasClone => activeClone != null;
    public bool CanPlaceClone => placeCooldownTimer <= 0f && activeClone == null;
    public bool CanTeleportToClone => teleportCooldownTimer <= 0f && activeClone != null;

    private void Update()
    {
        placeCooldownTimer -= Time.deltaTime;
        teleportCooldownTimer -= Time.deltaTime;

        if (activeClone == null)
            return;

        Health cloneHealth = activeClone.GetComponent<Health>();

        if (cloneHealth != null && cloneHealth.IsDead)
            activeClone = null;
    }

    public void PlaceClone(
        CrowCloneAbility ability,
        Vector3 point,
        Quaternion rotation)
    {
        if (ability == null)
            return;

        if (activeClone != null)
            return;

        if (placeCooldownTimer > 0f)
            return;

        if (ability.clonePrefab == null)
            return;

        activeClone = Instantiate(
            ability.clonePrefab,
            point,
            rotation
        );

        placeCooldownTimer = ability.placeCooldown;
    }

    public void TeleportToClone(CrowCloneAbility ability)
    {
        if (ability == null)
            return;

        if (activeClone == null)
            return;

        if (teleportCooldownTimer > 0f)
            return;

        transform.position = activeClone.transform.position;

        Destroy(activeClone);
        activeClone = null;

        teleportCooldownTimer = ability.teleportCooldown;
    }
}