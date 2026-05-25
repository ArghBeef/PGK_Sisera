using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Slider healthSlider;

    [Header("Behaviour")]
    [SerializeField] private float hideAfterSeconds = 2f;
    [SerializeField] private bool hideOnDeath = true;

    private Camera cachedCamera;
    private float hideTimer;
    private bool visible;

    private void Awake()
    {
        if (health == null)
            health = GetComponentInParent<Health>();

        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>(true);

        HideInstant();
        UpdateSlider();
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponentInParent<Health>();

        if (health != null)
        {
            health.OnHealthChanged += HandleHealthChanged;
            health.onDeath.AddListener(HandleDeath);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= HandleHealthChanged;
            health.onDeath.RemoveListener(HandleDeath);
        }
    }

    private void Update()
    {
        if (!visible)
            return;

        hideTimer -= Time.deltaTime;

        if (hideTimer <= 0f)
            HideInstant();
    }

    private void LateUpdate()
    {

        if (cachedCamera == null)
            cachedCamera = Camera.main;

        if (cachedCamera == null)
            return;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth, float changeAmount)
    {
        UpdateSlider();

        if (changeAmount < 0f)
            Show();
    }

    private void HandleDeath()
    {
        UpdateSlider();

        if (hideOnDeath)
            HideInstant();
        else
            Show();
    }

    private void UpdateSlider()
    {
        if (health == null || healthSlider == null)
            return;

        healthSlider.minValue = 0f;
        healthSlider.maxValue = health.MaxHealth;
        healthSlider.value = health.CurrentHealth;
    }

    private void Show()
    {
        visible = true;
        hideTimer = hideAfterSeconds;

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(true);
    }

    private void HideInstant()
    {
        visible = false;

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);
    }
}