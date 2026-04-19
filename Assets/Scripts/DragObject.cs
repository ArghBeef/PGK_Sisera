using UnityEngine;
using UnityEngine.InputSystem;

public class DragObject : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject grabbedObject;

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Body"))
                {
                    grabbedObject = hit.collider.gameObject;
                }
            }
        }

        if (Mouse.current.rightButton.isPressed && grabbedObject != null)
        {
            grabbedObject.transform.position = holdPoint.position;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            grabbedObject = null;
        }
    }
}