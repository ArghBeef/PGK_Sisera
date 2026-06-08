using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerPoints : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] private int currentPoints;

    [Header("Events")]
    public UnityEvent<int> onPointsChanged;
    public UnityEvent<int> onPointsAdded;

    public int CurrentPoints => currentPoints;

    public event Action<int> OnPointsChanged;

    public void AddPoints(int amount)
    {
        if (amount <= 0)
            return;

        currentPoints += amount;

        onPointsAdded?.Invoke(amount);
        onPointsChanged?.Invoke(currentPoints);
        OnPointsChanged?.Invoke(currentPoints);

        Debug.Log("Added points: " + amount + ". Total: " + currentPoints);
    }

    public bool TrySpendPoints(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentPoints < amount)
            return false;

        currentPoints -= amount;

        onPointsChanged?.Invoke(currentPoints);
        OnPointsChanged?.Invoke(currentPoints);

        return true;
    }
}