using UnityEngine;

public class FadeableObject : MonoBehaviour
{
    [SerializeField] private float fadedAlpha = 0.3f;
    [SerializeField] private float fadeSpeed = 8f;

    private Renderer[] renderers;
    private MaterialPropertyBlock block;

    private float currentAlpha = 1f;
    private bool shouldFade;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();
    }

    private void Update()
    {
        float targetAlpha = shouldFade ? fadedAlpha : 1f;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        bool isTransparent = currentAlpha < 0.99f;

        foreach (Renderer rend in renderers)
        {
            rend.GetPropertyBlock(block);

            block.SetFloat("_Alpha", currentAlpha);
            block.SetFloat("_ZWrite", isTransparent ? 0f : 1f);

            rend.SetPropertyBlock(block);
        }
    }

    public void SetFaded(bool faded)
    {
        shouldFade = faded;
    }
}