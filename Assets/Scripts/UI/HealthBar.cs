using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeadlockStyleHealthBar : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Health health;

    [Header("Sliders")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider delayedSlider;

    [Header("UI")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text changeText;

    [Header("Settings")]
    [SerializeField] private float delayedBarSpeed = 60f;
    [SerializeField] private float changeTextDuration = 1.2f;
    [SerializeField] private bool hideWhenDead = false;

    [Header("Colors")]
    [SerializeField] private Color damageColor = new Color(1f, 0.25f, 0.2f);
    [SerializeField] private Color healColor = new Color(0.25f, 1f, 0.35f);

    private Coroutine changeRoutine;
    private float targetHealth;

    private void Awake()
    {
        if (health == null)
            health = GetComponentInParent<Health>();

        if (changeText != null)
            changeText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (health == null)
            return;

        health.OnHealthChanged += HandleHealthChanged;
        RefreshInstant();
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.OnHealthChanged -= HandleHealthChanged;
    }

    private void Update()
    {
        if (health == null || delayedSlider == null)
            return;

        delayedSlider.value = Mathf.MoveTowards(
            delayedSlider.value,
            targetHealth,
            delayedBarSpeed * Time.deltaTime
        );
    }

    private void HandleHealthChanged(float current, float max, float change)
    {
        targetHealth = current;

        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)}";

        ShowChange(change);

        if (hideWhenDead && current <= 0f)
            gameObject.SetActive(false);
    }

    private void RefreshInstant()
    {
        targetHealth = health.CurrentHealth;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = health.MaxHealth;
            healthSlider.value = health.CurrentHealth;
        }

        if (delayedSlider != null)
        {
            delayedSlider.minValue = 0f;
            delayedSlider.maxValue = health.MaxHealth;
            delayedSlider.value = health.CurrentHealth;
        }

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(health.CurrentHealth)}";
    }

    private void ShowChange(float change)
    {
        if (changeText == null || Mathf.Approximately(change, 0f))
            return;

        if (changeRoutine != null)
            StopCoroutine(changeRoutine);

        changeRoutine = StartCoroutine(ChangeTextRoutine(change));
    }

    private IEnumerator ChangeTextRoutine(float change)
    {
        changeText.gameObject.SetActive(true);

        if (change < 0f)
        {
            changeText.color = damageColor;
            changeText.text = $"-{Mathf.CeilToInt(Mathf.Abs(change))}";
        }
        else
        {
            changeText.color = healColor;
            changeText.text = $"+{Mathf.CeilToInt(change)}";
        }

        yield return new WaitForSeconds(changeTextDuration);

        changeText.gameObject.SetActive(false);
    }
}