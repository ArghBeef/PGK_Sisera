using System.Collections;
using UnityEngine;

public class InteractableDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform doorPivot;
    [SerializeField] private Vector3 closedRotation;
    [SerializeField] private Vector3 openedRotation = new Vector3(0f, 90f, 0f);
    [SerializeField] private float rotateDuration = 0.3f;
    [SerializeField] private bool startsOpen = false;

    private bool isOpen;
    private Coroutine rotateRoutine;

    private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;

        isOpen = startsOpen;
        doorPivot.localRotation = Quaternion.Euler(isOpen ? openedRotation : closedRotation);
    }

    public void Interact(GameObject interactor)
    {
        SetOpen(!isOpen);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;

        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        rotateRoutine = StartCoroutine(RotateTo(isOpen ? openedRotation : closedRotation));
    }

    private IEnumerator RotateTo(Vector3 targetEuler)
    {
        Quaternion start = doorPivot.localRotation;
        Quaternion end = Quaternion.Euler(targetEuler);

        float time = 0f;

        while (time < rotateDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / rotateDuration);
            doorPivot.localRotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }

        doorPivot.localRotation = end;
        rotateRoutine = null;
    }
}