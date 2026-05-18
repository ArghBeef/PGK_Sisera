using System;
using UnityEngine;

public static class NPCSuspiciousEventSystem
{
    public static event Action<Vector3, GameObject, float, float> OnSuspiciousEvent;

    public static void Report(Vector3 position, GameObject source, float radius, float waitTime)
    {
        OnSuspiciousEvent?.Invoke(position, source, radius, waitTime);
    }
}