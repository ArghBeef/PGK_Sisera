using UnityEngine;

public class NPCDetectionVisualizer : MonoBehaviour
{
    public enum ZoneState
    {
        Neutral,
        Warning,
        Danger
    }

    [Header("Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Colors")]
    [SerializeField] private Color neutralColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;

    [Header("Material Color Property")]
    [SerializeField] private string colorProperty = "_Color";

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        propertyBlock = new MaterialPropertyBlock();
        SetState(ZoneState.Neutral);
    }

    public void SetState(ZoneState state)
    {
        if (targetRenderer == null)
            return;

        Color colorToUse = neutralColor;

        switch (state)
        {
            case ZoneState.Warning:
                colorToUse = warningColor;
                break;
            case ZoneState.Danger:
                colorToUse = dangerColor;
                break;
            default:
                colorToUse = neutralColor;
                break;
        }

        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(colorProperty, colorToUse);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }
}