using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField] private GameObject brokenPrefab;

    public void Break()
    {
        if (brokenPrefab != null)
            Instantiate(brokenPrefab, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}