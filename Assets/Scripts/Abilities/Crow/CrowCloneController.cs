using UnityEngine;

public class CrowCloneController : MonoBehaviour
{
    private GameObject activeClone;

    public void UseCloneAbility(CrowCloneAbility ability)
    {
        if (activeClone == null)
        {
            SpawnClone(ability);
        }
        else
        {
            TeleportBack();
        }
    }

    private void SpawnClone(CrowCloneAbility ability)
    {
        Vector3 direction = transform.forward;

        PC_Movement movement = GetComponent<PC_Movement>();
        if (movement != null)
            direction = movement.FacingDirection;

        direction.y = 0f;
        direction.Normalize();

        Vector3 spawnPosition = transform.position + direction * ability.spawnDistance;

        activeClone = Instantiate(
            ability.clonePrefab,
            spawnPosition,
            transform.rotation
        );
    }

    private void TeleportBack()
    {
        transform.position = activeClone.transform.position;

        Destroy(activeClone);
        activeClone = null;
    }
}