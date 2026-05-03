using UnityEngine;
using UnityEngine.InputSystem;

public class HealthDebug : MonoBehaviour
{
    [SerializeField] private Health health;

    [Header("Test Values")]
    [SerializeField] private float damageAmount = 15f;
    [SerializeField] private float healAmount = 10f;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();
    }

    private void Update()
    {
        if (health == null)
            return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
            health.TakeDamage(damageAmount);

        if (Keyboard.current.kKey.wasPressedThisFrame)
            health.Heal(healAmount);
    }
}