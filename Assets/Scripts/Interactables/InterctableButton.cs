using UnityEngine;
using UnityEngine.Events;

public class InteractableButton : MonoBehaviour, IInteractable
{
    [SerializeField] private UnityEvent onPressed;
    [SerializeField] private bool pressOnce = false;

    private bool alreadyPressed;

    public void Interact(GameObject interactor)
    {
        if (pressOnce && alreadyPressed)
            return;

        alreadyPressed = true;
        onPressed?.Invoke();

        Debug.Log($"{name} was pressed by {interactor.name}");
    }
}