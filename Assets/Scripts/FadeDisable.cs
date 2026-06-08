using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FadeDisableTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Objects To Hide")]
    [SerializeField] private GameObject[] objectsToDisable;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Renderer[] renderers;
    private Material[][] materials;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        CacheRenderersAndMaterials();
    }

    private void CacheRenderersAndMaterials()
    {
        renderers = new Renderer[objectsToDisable.Length];
        materials = new Material[objectsToDisable.Length][];

        for (int i = 0; i < objectsToDisable.Length; i++)
        {
            if (objectsToDisable[i] == null)
                continue;

            renderers[i] = objectsToDisable[i].GetComponentInChildren<Renderer>();

            if (renderers[i] != null)
                materials[i] = renderers[i].materials;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        StartFade(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        StartFade(true);
    }

    private void StartFade(bool fadeIn)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeObjects(fadeIn));
    }

    private IEnumerator FadeObjects(bool fadeIn)
    {
        if (fadeIn)
        {
            SetObjectsActive(true);
            SetAlpha(0f);
        }

        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(targetAlpha);

        if (!fadeIn)
            SetObjectsActive(false);

        fadeRoutine = null;
    }

    private void SetObjectsActive(bool active)
    {
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private void SetAlpha(float alpha)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
                continue;

            foreach (Material mat in materials[i])
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
}