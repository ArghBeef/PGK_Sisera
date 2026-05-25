using UnityEngine;

public class RamShieldDamageRedirect : MonoBehaviour
{
    private Health shieldTargetHealth;

    public bool HasShieldTarget
    {
        get
        {
            return shieldTargetHealth != null &&
                   !shieldTargetHealth.IsDead;
        }
    }

    public void SetShieldTarget(Health target)
    {
        shieldTargetHealth = target;
    }

    public void ClearShieldTarget(Health target)
    {
        if (shieldTargetHealth == target)
            shieldTargetHealth = null;
    }

    public bool TryRedirectDamage(float damage)
    {
        if (!HasShieldTarget)
            return false;

        shieldTargetHealth.TakeDamage(damage);

        return true;
    }
}