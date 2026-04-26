using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool startWithFullHealth = true;
    [SerializeField] private float currentHealth;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private float destroyDelay = 0f;
    [SerializeField] private bool changeTagOnDeath = false;
    [SerializeField] private string deathTag = "Body";

    [Header("Optional Components To Disable On Death")]
    [SerializeField] private Behaviour[] disableOnDeath;
    [SerializeField] private Collider[] collidersToDisable;

    [Header("Events")]
    public UnityEvent<float> onDamaged;
    public UnityEvent<float> onHealed;
    public UnityEvent onDeath;

    private bool dead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => dead;
    public bool IsAlive => !dead;

    private void Awake()
    {
        if (startWithFullHealth)
            currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (dead)
            return;

        if (damage <= 0f)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        onDamaged?.Invoke(damage);

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (dead)
            return;

        if (amount <= 0f)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        onHealed?.Invoke(amount);
    }

    public void Kill()
    {
        if (dead)
            return;

        currentHealth = 0f;
        Die();
    }

    private void Die()
    {
        if (dead)
            return;

        dead = true;

        if (changeTagOnDeath)
            gameObject.tag = deathTag;

        foreach (Behaviour behaviour in disableOnDeath)
        {
            if (behaviour != null)
                behaviour.enabled = false;
        }

        foreach (Collider col in collidersToDisable)
        {
            if (col != null)
                col.enabled = false;
        }

        onDeath?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }
}