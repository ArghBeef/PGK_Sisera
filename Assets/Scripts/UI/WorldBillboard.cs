using UnityEngine;

public class WorldBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool lockY = false;

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        if (lockY)
        {
            Vector3 direction = transform.position - targetCamera.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            transform.rotation = targetCamera.transform.rotation;
        }
    }
}