using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TranslucentMeshTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Objects To Fade")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Translucent Shader")]
    [SerializeField] private Shader translucentShader;

    [Header("Fade")]
    [Range(0f, 1f)]
    [SerializeField] private float translucentAlpha = 0.25f;
    [SerializeField] private float fadeDuration = 0.35f;

    private class RendererData
    {
        public Renderer renderer;
        public Material[] originalMaterials;
        public Material[] fadeMaterials;
        public Coroutine fadeRoutine;
    }

    private readonly List<RendererData> renderers = new();

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (translucentShader == null)
            translucentShader = Shader.Find("UI/WorldSpaceVisible");

        SaveOriginalMaterials();
    }

    private void SaveOriginalMaterials()
    {
        renderers.Clear();

        foreach (Renderer r in targetRenderers)
        {
            if (r == null)
                continue;

            Material[] originals = r.materials;
            Material[] fades = new Material[originals.Length];

            for (int i = 0; i < originals.Length; i++)
            {
                fades[i] = new Material(translucentShader);

                if (originals[i].HasProperty("_MainTex") && fades[i].HasProperty("_MainTex"))
                    fades[i].SetTexture("_MainTex", originals[i].GetTexture("_MainTex"));

                Color color = Color.white;

                if (originals[i].HasProperty("_Color"))
                    color = originals[i].GetColor("_Color");

                color.a = 1f;

                if (fades[i].HasProperty("_Color"))
                    fades[i].SetColor("_Color", color);
            }

            renderers.Add(new RendererData
            {
                renderer = r,
                originalMaterials = originals,
                fadeMaterials = fades
            });
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        foreach (RendererData data in renderers)
            StartFade(data, translucentAlpha, false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        foreach (RendererData data in renderers)
            StartFade(data, 1f, true);
    }

    private void StartFade(RendererData data, float targetAlpha, bool restoreOriginalAfterFade)
    {
        if (data.renderer == null)
            return;

        if (data.fadeRoutine != null)
            StopCoroutine(data.fadeRoutine);

        data.fadeRoutine = StartCoroutine(FadeRoutine(data, targetAlpha, restoreOriginalAfterFade));
    }

    private IEnumerator FadeRoutine(RendererData data, float targetAlpha, bool restoreOriginalAfterFade)
    {
        data.renderer.materials = data.fadeMaterials;

        float timer = 0f;
        float startAlpha = GetAlpha(data.fadeMaterials);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            SetAlpha(data.fadeMaterials, alpha);

            yield return null;
        }

        SetAlpha(data.fadeMaterials, targetAlpha);

        if (restoreOriginalAfterFade)
            data.renderer.materials = data.originalMaterials;

        data.fadeRoutine = null;
    }

    private float GetAlpha(Material[] materials)
    {
        foreach (Material mat in materials)
        {
            if (mat != null && mat.HasProperty("_Color"))
                return mat.GetColor("_Color").a;
        }

        return 1f;
    }

    private void SetAlpha(Material[] materials, float alpha)
    {
        foreach (Material mat in materials)
        {
            if (mat == null)
                continue;

            if (!mat.HasProperty("_Color"))
                continue;

            Color color = mat.GetColor("_Color");
            color.a = alpha;
            mat.SetColor("_Color", color);
        }
    }
}