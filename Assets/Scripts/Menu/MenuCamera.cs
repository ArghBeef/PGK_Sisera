using System.Collections;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotateSpeed = 3f;

    private Coroutine moveRoutine;

    public void MoveTo(Transform targetPoint)
    {
        if (targetPoint == null)
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(targetPoint));
    }

    private IEnumerator MoveRoutine(Transform targetPoint)
    {
        while (Vector3.Distance(transform.position, targetPoint.position) > 0.05f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPoint.position,
                moveSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetPoint.rotation,
                rotateSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPoint.position;
        transform.rotation = targetPoint.rotation;
    }
}