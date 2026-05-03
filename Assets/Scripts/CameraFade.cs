using System.Collections.Generic;
using UnityEngine;

public class CameraFade : MonoBehaviour
{
    [SerializeField] private float fadeDistance = 3f;
    [SerializeField] private LayerMask fadeableLayers;

    private readonly Collider[] hits = new Collider[64];
    private readonly HashSet<FadeableObject> currentlyFaded = new();
    private readonly HashSet<FadeableObject> foundThisFrame = new();

    private void Update()
    {
        foundThisFrame.Clear();

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            fadeDistance,
            hits,
            fadeableLayers
        );

        for (int i = 0; i < count; i++)
        {
            FadeableObject fadeable = hits[i].GetComponentInParent<FadeableObject>();

            if (fadeable == null)
                continue;

            foundThisFrame.Add(fadeable);
            fadeable.SetFaded(true);
        }

        foreach (FadeableObject fadeable in currentlyFaded)
        {
            if (!foundThisFrame.Contains(fadeable) && fadeable != null)
                fadeable.SetFaded(false);
        }

        currentlyFaded.Clear();

        foreach (FadeableObject fadeable in foundThisFrame)
            currentlyFaded.Add(fadeable);
    }
}