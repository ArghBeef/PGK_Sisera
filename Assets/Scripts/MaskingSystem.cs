using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class MaskingSystem : MonoBehaviour
{
    public GameObject maskText;

    private bool isNear = false;
    private bool isMasked = false;

    void Update()
    {
        if (isNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            isMasked = !isMasked;
            maskText.SetActive(isMasked);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = false;
            maskText.SetActive(false);
            isMasked = false;
        }
    }
}
