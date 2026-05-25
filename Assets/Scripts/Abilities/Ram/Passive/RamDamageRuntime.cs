using System.Collections;
using UnityEngine;

public class RamDamageHealRuntime : MonoBehaviour
{
    [Header("Healing")]
    [SerializeField] private float healPercent = 0.2f;
    [SerializeField] private float healDelay = 1.5f;

    private Health health;

    public void SetHealPercent(float value)
    {
        healPercent = value;
    }

    public void SetHealDelay(float value)
    {
        healDelay = value;
    }

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (health != null)
            health.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float current, float max, float change)
    {
        if (change >= 0f)
            return;

        float damageTaken = Mathf.Abs(change);
        float healAmount = damageTaken * healPercent;

        if (healAmount > 0f)
            StartCoroutine(DelayedHeal(healAmount));
    }

    private IEnumerator DelayedHeal(float amount)
    {
        yield return new WaitForSeconds(healDelay);

        if (health == null)
            yield break;

        if (health.IsDead)
            yield break;

        health.Heal(amount);
    }
}