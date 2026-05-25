using UnityEngine;

public class StatusEffectVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StatusEffectController status;

    [Header("Status Icons / Objects")]
    [SerializeField] private GameObject stunnedSign;


    private void Awake()
    {
        if (status == null)
            status = GetComponentInParent<StatusEffectController>();

        HideAll();
    }

    private void LateUpdate()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (status == null)
        {
            HideAll();
            return;
        }

        if (stunnedSign != null)
            stunnedSign.SetActive(status.IsStunned);
    }

    private void HideAll()
    {
        if (stunnedSign != null)
            stunnedSign.SetActive(false);
    }
}