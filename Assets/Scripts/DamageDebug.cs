using UnityEngine;

public class DamageDebug : MonoBehaviour, IDamageable
{
    public void TakeDamage(float damage)
    {
        Debug.Log(gameObject.name + " was hit for " + damage);
    }
}