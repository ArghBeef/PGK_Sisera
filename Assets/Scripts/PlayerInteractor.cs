using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Interaction")]
    [SerializeField] private string interactableTag = "interactables";
    [SerializeField] private float clickRayDistance = 100f;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.Disable();
    }

    private void Update()
    {
        if (interactAction == null)
            return;

        if (!interactAction.action.WasPressedThisFrame())
            return;

        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, clickRayDistance, interactionLayers))
            return;

        GameObject clickedObject = hit.collider.gameObject;

        if (!clickedObject.CompareTag(interactableTag))
            return;

        IInteractable interactable = hit.collider.GetComponent<IInteractable>();

        if (interactable == null)
            interactable = hit.collider.GetComponentInParent<IInteractable>();

        if (interactable == null)
            interactable = hit.collider.GetComponentInChildren<IInteractable>();

        if (interactable == null)
        {
            Debug.LogWarning(
                $"Clicked object '{clickedObject.name}' has tag '{interactableTag}' but no IInteractable component.");
            return;
        }

        Transform interactableTransform = ((MonoBehaviour)interactable).transform;
        float distanceToInteractable = Vector3.Distance(transform.position, interactableTransform.position);

        if (distanceToInteractable > interactDistance)
        {
            Debug.Log($"Too far away to interact with {clickedObject.name}.");
            return;
        }

        interactable.Interact(gameObject);
    }
}