using UnityEngine;
using UnityEngine.InputSystem;

public class LampToggle : MonoBehaviour
{
    private Light lampLight;

    void Start()
    {
        lampLight = GetComponentInChildren<Light>();
    }

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    lampLight.enabled = !lampLight.enabled;
                }
            }
        }
    }
}