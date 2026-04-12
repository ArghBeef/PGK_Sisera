using UnityEngine;
using UnityEngine.InputSystem;

public class CarHorn : MonoBehaviour
{
    public AudioSource audioSource;

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
                    audioSource.Play();
                }
            }
        }
    }
}