using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCooldownUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerClassController classController;

    [Header("Ability Slot")]
    [SerializeField] private int abilityIndex = 0;

    [Header("UI")]
    [SerializeField] private Slider cooldownSlider;
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TMP_Text cooldownText;

    [Header("Settings")]
    [SerializeField] private bool hideTextWhenReady = true;
    [SerializeField] private bool hideSliderWhenReady = false;

    private AbilityDefinition ability;

    private void Awake()
    {
        if (classController == null)
            classController = GetComponentInParent<PlayerClassController>();

        RefreshAbility();
    }

    private void OnEnable()
    {
        RefreshAbility();
    }

    private void Update()
    {
        if (classController == null)
            return;

        if (ability == null)
            RefreshAbility();

        if (ability == null)
            return;

        float cooldown = classController.GetCooldownTimer(abilityIndex);
        float maxCooldown = Mathf.Max(ability.cooldown, 0.01f);

        float progress = 1f - Mathf.Clamp01(cooldown / maxCooldown);

        if (cooldownSlider != null)
        {
            cooldownSlider.minValue = 0f;
            cooldownSlider.maxValue = 1f;
            cooldownSlider.value = progress;

            if (hideSliderWhenReady)
                cooldownSlider.gameObject.SetActive(cooldown > 0f);
        }

        if (cooldownText != null)
        {
            if (cooldown > 0f)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = Mathf.CeilToInt(cooldown).ToString();
            }
            else
            {
                cooldownText.text = "";

                if (hideTextWhenReady)
                    cooldownText.gameObject.SetActive(false);
            }
        }
    }

    public void RefreshAbility()
    {
        if (classController == null)
            return;

        ability = classController.GetAbility(abilityIndex);

        if (abilityIcon != null)
        {
            if (ability != null && ability.icon != null)
            {
                abilityIcon.sprite = ability.icon;
                abilityIcon.enabled = true;
            }
            else
            {
                abilityIcon.sprite = null;
                abilityIcon.enabled = false;
            }
        }

        if (cooldownSlider != null)
            cooldownSlider.value = 1f;

        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }
}